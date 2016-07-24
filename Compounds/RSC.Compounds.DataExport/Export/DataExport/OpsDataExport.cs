using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using Microsoft.Practices.ServiceLocation;
using RSC.Compounds.EntityFramework;
using RSC.Logging.EntityFramework;
using RSC.Properties.EntityFramework;
using Newtonsoft.Json;
using System.IO;
using RSC.Properties;
using RSC.Logging;
using System.Diagnostics;
using ChemSpider.Utilities;

namespace RSC.Compounds.DataExport
{
	public class OpsDataExport
	{
		private Dictionary<int, Guid> _exports { get; set; }

		private IDataExportStore DataExportStore;
		private IPropertyStore PropertiesStore;
		private CompoundStore CompoundsStore;
		private ILogStore LogStore;
		private IDatasourceStore DatasourcesStore;

		public OpsDataExport()
		{
			//Register the Property store with the service locator.
			var builder = new ContainerBuilder();
			builder.RegisterType<EFPropertyStore>().As<Properties.IPropertyStore>();
			var container = builder.Build();
			ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(container));

			DataExportStore = new EFDataExportStore();
			PropertiesStore = new EFPropertyStore();
			CompoundsStore = new EFCompoundStore(PropertiesStore);
			LogStore = new EFLogStore(null);
			DatasourcesStore = new EFDatasourceStore();

			_exports = new Dictionary<int, Guid>();
		}

		public void ExportDatasources(OpsDataExportOptions options)
		{
			if ( options.DataSourceIds == null ) {
				options.DataSourceIds = DatasourcesStore.GetDataSourceIds();
				Trace.TraceInformation("No datasources specified - exporting all found: {0}", String.Join(", ", options.DataSourceIds));
			}

			options
				.DataSourceIds
				.AsParallel()
				.ForAll(dsn_id => {
					using ( var export = new OpsDataSourceExport(dsn_id, options.ExportDate) ) {
						export.DataExportStore = DataExportStore;
						export.PropertiesStore = PropertiesStore;
						export.CompoundsStore = CompoundsStore;
						export.LogStore = LogStore;

						export.Compress = options.Compress;
						export.Limited = options.Limited;
						export.UploadUrl = options.UploadUrl;
						export.DownloadUrl = options.DownloadUrl;
						export.Username = options.Username;
						export.Password = options.Password;

						export.Encoding = new UTF8Encoding(false); // Don't use a BOM.
						export.Parent = this;

						export.Export();

						_exports.Add(export.ExportId, dsn_id);
					}
				});
		}

		public void ExportDatasourcesByExportIds(OpsDataExportOptions options)
		{
			options.ExportIds.ForEach(id => {
				using ( var export = new OpsDataSourceExport(id, DataExportStore) )
				{
					if ( options.FileIds.Any() ) {
						export
							.Files
							.Where(f => !options.FileIds.Contains(f.Id))
							.ToList()
							.ForAll(f => export.Files.Remove(f));
					}

					export.PropertiesStore = PropertiesStore;
					export.CompoundsStore = CompoundsStore;
					export.LogStore = LogStore;

					export.Compress = options.Compress;
					export.Limited = options.Limited;
					export.UploadUrl = options.UploadUrl;
					export.DownloadUrl = options.DownloadUrl;
					export.Username = options.Username;
					export.Password = options.Password;

					export.Encoding = new UTF8Encoding(false); // Don't use a BOM.
					export.Parent = this;

					export.Export();
				}
			});
		}

		public void ExportOpsToCsidSdf(OpsDataExportOptions options)
		{
			using ( var export = new OpsSdfMapExport(options.ExportDate, options.ExportIds.FirstOrDefault(), DataExportStore, options.TmpDir) ) {
				export.DataExportStore = DataExportStore;
				export.PropertiesStore = PropertiesStore;
				export.CompoundsStore = CompoundsStore;
				export.LogStore = LogStore;

				export.Compress = options.Compress;
				export.UploadUrl = options.UploadUrl;
				export.DownloadUrl = options.DownloadUrl;
				export.Username = options.Username;
				export.Password = options.Password;

				export.Encoding = new UTF8Encoding(false); // Don't use a BOM.
				export.Parent = this;

				export.Export();
			}
		}

		public void ExportVoID(OpsDataExportOptions options)
		{
			if ( _exports == null || !_exports.Any() ) {
				if ( !options.ExportIds.Any() )
					throw new ApplicationException("VoID export attempted on an empty list of data source exports");

				_exports = options
					.ExportIds
					.Select(id => DataExportStore.GetDataExportLog(id))
					.ToDictionary(l => l.Id, l => l.DataSourceId);

				// Due to the way exports are arranged into a directories they all must fall onto the same date
				options.ExportDate = (DateTime)DataExportStore.GetDataExportLog(options.ExportIds.First()).ExportDate;
			}

			using ( var export = new OpsVoidExport(_exports, options.ExportDate) )
			{
				export.DataExportStore = DataExportStore;
				export.PropertiesStore = PropertiesStore;
				export.CompoundsStore = CompoundsStore;
				export.LogStore = LogStore;

				export.Compress = options.Compress;
				export.Limited = options.Limited;
				export.UploadUrl = options.UploadUrl;
				export.DownloadUrl = options.DownloadUrl;
				export.Username = options.Username;
				export.Password = options.Password;

				export.Encoding = Encoding.GetEncoding(1252);
				export.Parent = this;

				export.Export();
			}
		}

		/// <summary>
		/// Performs the full list of data export tasks for a Data Source.
		/// Includes Rdf, Sdf and Void generation.
		/// </summary>
		public void Export(OpsDataExportOptions options)
		{
			new List<Action<OpsDataExportOptions>>() {
				ExportDatasources,
				ExportOpsToCsidSdf
			}
			.AsParallel()
			.ForAll(a => a(options));

			ExportVoID(options);
		}
	}
}
