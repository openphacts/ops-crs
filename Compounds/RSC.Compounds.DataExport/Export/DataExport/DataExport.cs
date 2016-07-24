using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using RSC.Datasources;
using RSC.Logging;
using RSC.Properties;
using System.Text.RegularExpressions;
using ChemSpider.Utilities;
using System.IO;

namespace RSC.Compounds.DataExport
{
	public class DataExport : IDataExport
	{
		//TODO: Implement dependency injection using AutoFac (IDataExportStore, CompoundStore, IPropertyStore, ILogStore)

		public IDataExportStore DataExportStore { get; set; }

		public CompoundStore CompoundsStore { get; set; }

		public IPropertyStore PropertiesStore { get; set; }

		public ILogStore LogStore { get; set; }

		public int VersionId { get; set; }

		public int ExportId { get; set; }

		public DateTime ExportDate { get; set; }

		public string ExportDirectory { get; set; }

		private readonly List<IDataExportFile> _files = new List<IDataExportFile>();

		public List<IDataExportFile> Files
		{
			get { return _files; }
		}

		public Encoding Encoding { get; set; }

		public bool Compress { get; set; }

		// public bool Limited { get; set; }

		private string _uploadUri;
		public string UploadUrl
		{
			get { return _uploadUri; }
			set {
				_uploadUri = value;
				if ( !Regex.IsMatch(_uploadUri, "^(file|ftp):") )
					throw new FormatException(String.Format("Only file:// or ftp:// uploads are supported", _uploadUri));
			}
		}

		public string DownloadUrl { get; set; }

		public string Username { get; set; }

		public string Password { get; set; }

		public DataExportStatus Status { get; set; }

		public string ErrorMessage { get; set; }

		public OpsDataExport Parent { get; set; }


		public DataExport()
		{

		}

		public void AddFile(IDataExportFile fileToAdd)
		{
			_files.Add(fileToAdd);
		}

		private void updateStatus(IDataExportFile file, DataExportStatus status, string message = null)
		{
			file.Status = status;
			file.ErrorMessage = message;
			DataExportStore.UpdateExportFile(file);
		}

		public virtual void Export()
		{
			PreValidate();

			Status = DataExportStatus.Started;
			if ( ExportId == 0 )
				DataExportStore.CreateExport(this);
			else
				DataExportStore.UpdateExport(this);

			IFileUploader uploader = null;

			try {
				if ( !String.IsNullOrEmpty(UploadUrl) ) {
					var uri = new Uri(UploadUrl);
					if ( uri.Scheme == Uri.UriSchemeFtp )
						uploader = new FtpFileUploader(UploadUrl, Username, Password);
					else if ( uri.Scheme == Uri.UriSchemeFile )
						uploader = new CopyFileUploader();
					else
						throw new UriFormatException(String.Format("Unsupported Uri schema: {0}", uri));
				}

				Files
					.AsParallel()
					.ForAll(file => {
						if ( file.Status == DataExportStatus.Started || file.Status == DataExportStatus.ExportFailed ) {
							try {
								updateStatus(file, DataExportStatus.Started);

								file.Export(this, Encoding);

								updateStatus(file, DataExportStatus.Exported);
							}
							catch ( Exception ex ) {
								updateStatus(file, DataExportStatus.ExportFailed, ex.ToString());
							}
						}

						if ( file.Status == DataExportStatus.Exported || file.Status == DataExportStatus.UploadFailed ) {
							try {
								if ( Compress )
									file.Compress();

								if ( uploader != null ) {
									file.Upload(this, uploader);

									// Delete temporary file after successful upload to clean space, otherwise leave it to examine
									if ( File.Exists(file.FileFullPath) )
										File.Delete(file.FileFullPath);
								}

								updateStatus(file, DataExportStatus.Succeeded);
							}
							catch ( Exception ex ) {
								updateStatus(file, DataExportStatus.UploadFailed, ex.ToString());
							}
						}
					});

				while ( uploader != null && uploader.JobsRemaining > 0 )
					Thread.Sleep(1000);

				Status =
					Files.Any(f => f.Status == DataExportStatus.ExportFailed) ? DataExportStatus.ExportFailed :
					Files.Any(f => f.Status == DataExportStatus.UploadFailed) ? DataExportStatus.UploadFailed :
					Files.All(f => f.Status == DataExportStatus.Exported) ? DataExportStatus.Exported :
					Files.All(f => f.Status == DataExportStatus.Succeeded) ? DataExportStatus.Succeeded :
					Status;

				PostValidate();
			}
			catch ( Exception ex ) {
				Status = DataExportStatus.ExportFailed;
				ErrorMessage = ex.ToString();
			}
			finally {
				if ( Status != DataExportStatus.Succeeded )
					Trace.TraceError("Export Failed: Export Id: {0}, Message: {1}", ExportId, ErrorMessage);
				else
					Trace.TraceInformation("Export Succeeded: Id: {0}, Location: {1}", ExportId, DownloadUrl);

				// Finally update the export status success / failure, finished, error message etc (if implemented).
				DataExportStore.UpdateExport(this);

				if ( uploader != null )
					uploader.Dispose();
			}
		}

		public virtual void Dispose()
		{

		}

		public virtual void PreValidate()
		{

		}

		public virtual void PostValidate()
		{

		}
	}
}
