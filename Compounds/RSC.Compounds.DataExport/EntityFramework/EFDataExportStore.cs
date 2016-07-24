using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Data.Entity;
using RSC.Compounds.DataExport.EntityFramework;
using Newtonsoft.Json;
using System.Diagnostics;

namespace RSC.Compounds.DataExport
{
	public class EFDataExportStore : IDataExportStore
	{
		public T LoadState<T>(int id)
		{
			using ( var db = new DataExportContext() ) {
				string json = db.Logs
					.Where(l => l.Id == id)
					.Select(l => l.State)
					.SingleOrDefault();

				if ( String.IsNullOrEmpty(json) )
					return default(T);

				T state = (T)JsonConvert.DeserializeObject(json, typeof(T));
				return state;
			}
		}

		public void SaveState<T>(int id, T state)
		{
			using ( var db = new DataExportContext() ) {
				var log = db.Logs
					.Where(l => l.Id == id)
					.SingleOrDefault();

				if ( log == null ) {
					log = new ef_DataExportLog();
					log.DateCreated = DateTime.Now;
					log.ExportType = DataExportType.Master;
					db.Logs.Add(log);
				}

				log.State = JsonConvert.SerializeObject(state);
				log.DateUpdated = DateTime.Now;

				db.SaveChanges();
			}
		}

		/// <summary>
		/// Get the current DataVersion for a given data source.
		/// </summary>
		/// <param name="dataSourceId">Data source id</param>
		/// <returns>Current DataVersion</returns>
		public DataVersion GetCurrentDataVersion(Guid dataSourceId)
		{
			//NOTE** : TODO: This is an interim solution for providing Provenance and Versioning information for the VoID generation.
			var dataExportSection = ConfigurationManager.GetSection("DataExport") as DataExportSection;

			if ( dataExportSection == null )
				Trace.TraceError("DataExport section is missing in config file");
			else {
				//Get the Provenance and versioning information from the config.
				var dataExport = dataExportSection.DataExports[dataSourceId];

				if ( dataExport == null )
					Trace.TraceError("DataExport section for given DataSourceId ({0}) is missing in config file", dataSourceId);
				else {
					return new DataVersion {
						CreatedDate = dataExport.CreatedDate,
						DataSourceId = dataExport.DataSourceId,
						DownloadDate = dataExport.DownloadDate,
						DownloadedBy = dataExport.DownloadedBy,
						DownloadUri = dataExport.DownloadUri,
						LicenseUri = dataExport.LicenseUri,
						VersionName = dataExport.VersionName,
						VoidUri = dataExport.VoidUri,
						UriSpace = dataExport.UriSpace
					};
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the previous DataExportLog.
		/// </summary>
		/// <param name="dataExportId">Current data export id</param>
		/// <returns>Previous DataExportLog</returns>
		public DataExportLog GetPreviousDataExportLog(int dataExportId)
		{
			using ( var db = new DataExportContext() ) {
				db.Configuration.AutoDetectChangesEnabled = false;

				return db.Logs
					.Include(i => i.Files)
					.Where(i => i.Id < dataExportId)
					.OrderByDescending(i => i.Id)
					.FirstOrDefault()
					.ToDataExportLog();
			}
		}

		/// <summary>
		/// Returns a Data export log.
		/// </summary>
		/// <param name="dataExportId">Data export id</param>
		/// <returns>Data export log</returns>
		public DataExportLog GetDataExportLog(int dataExportId)
		{
			using ( var db = new DataExportContext() ) {
				db.Configuration.AutoDetectChangesEnabled = false;

				return db.Logs
					.Include(i => i.Files)
					.SingleOrDefault(i => i.Id == dataExportId)
					.ToDataExportLog();
			}
		}

		/// <summary>
		/// Logs the Data Export to the db, sets Files and Data Version.
		/// </summary>
		/// <param name="export">Data export to be logged</param>
		public void CreateExport(IDataExport export)
		{
			if ( export.ExportId > 0 )
				throw new KeyNotFoundException(String.Format("IDataExport({0}) already exists", export.ExportId));

			using ( var db = new DataExportContext() )
			{
				var dbLog = new ef_DataExportLog();

				dbLog.ExportDate = export.ExportDate;
				dbLog.DateCreated = DateTime.Now;
				dbLog.ExportDirectory = export.ExportDirectory;
				dbLog.Status = export.Status;
				dbLog.ErrorMessage = export.ErrorMessage;

				dbLog.ExportType =
					export is OpsDataSourceExport ? DataExportType.DataSource :
					export is OpsSdfMapExport ? DataExportType.Map :
					export is OpsVoidExport ? DataExportType.Void :
					export is OpsDataExport ? DataExportType.Master :
					DataExportType.None;

				if ( export is OpsDataSourceExport ) {
					dbLog.DataSourceId = (export as OpsDataSourceExport).DataSource.Guid;
					var dataVersion = GetCurrentDataVersion((export as OpsDataSourceExport).DataSource.Guid);
					dbLog.DataVersionId = dataVersion.Id;
				}

				dbLog.Files = new List<ef_DataExportLogFile>();

				db.Logs.Add(dbLog);
				db.SaveChanges();
				export.ExportId = dbLog.Id;

				foreach ( var expFile in export.Files ) {
					var dbFile = new ef_DataExportLogFile() {
						Id = expFile.Id,
						FileName = expFile.FileName,
						DateCreated = DateTime.Now
					};
					dbLog.Files.Add(dbFile);
					db.SaveChanges();
					expFile.Id = dbFile.Id;
				}
			}
		}

		public void UpdateExport(IDataExport export)
		{
			if ( export.ExportId == 0 )
				throw new KeyNotFoundException(String.Format("IDataExport({0}) not found", export.ExportId));

			using ( var db = new DataExportContext() ) {
				var dbLog = db.Logs.FirstOrDefault(i => i.Id == export.ExportId);
				if ( dbLog == null )
					throw new KeyNotFoundException(String.Format("IDataExport({0}) not found", export.ExportId));

				dbLog.Status = export.Status;
				dbLog.ErrorMessage = export.ErrorMessage;
				dbLog.DateUpdated = DateTime.Now;

				db.SaveChanges();
			}

			export.Files.ForEach(f => UpdateExportFile(f));
		}

		public void UpdateExportFile(IDataExportFile file)
		{
			if ( file.Id == 0 )
				throw new KeyNotFoundException(String.Format("IDataExportFile({0}) not found", file.Id));

			using ( var db = new DataExportContext() ) {

				var dbFile = db.Files.FirstOrDefault(i => i.Id == file.Id);
				if ( dbFile == null )
					throw new KeyNotFoundException(String.Format("IDataExportFile({0}) not found", file.Id));

				dbFile.FileName = file.FileName;
				dbFile.RecordCount = file.RecordCount;
				dbFile.Status = file.Status;
				dbFile.ErrorMessage = file.ErrorMessage;
				dbFile.DateUpdated = DateTime.Now;
				if ( dbFile.Status == DataExportStatus.Started )
					dbFile.DateCreated = DateTime.Now;

				db.SaveChanges();
			}
		}
	}
}
