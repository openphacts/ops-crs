using System;
using System.Linq;
using System.Collections.Generic;
using RSC.Datasources;
using ChemSpider.Utilities;

namespace RSC.Compounds.DataExport
{
	/// <summary>
	/// Class for generating Rdf Data Exports for CVSP.
	/// </summary>
	public class OpsDataSourceExport : DataExport, IDataSourceExport, ILimitedExport
	{
		public string DsnLabel
		{
			get { return DataSource.GetLabel(true); }
		}

		public DataSource DataSource { get; set; }

		public DataSourcesClient DataSourcesClient { get; set; }

		private void setupDataSource(Guid dataSourceId)
		{
			DataSourcesClient = new DataSourcesClient();

			DataSource = DataSourcesClient.GetDataSource(dataSourceId);
			if ( DataSource == null )
				throw new Exception(string.Format("Invalid DataSource Id: {0}", dataSourceId.ToString()));
		}

		private void addFiles()
		{
			AddFile(new ExactLinksetDataExportFile(this));

			if ( Turtle.UseSkosRelatedMatchForDsn(DataSource.Name) )
				AddFile(new RelatedLinksetDataExportFile(this));

			// These two only will be partially exported
			AddFile(new SynonymsDataExportFile(this));

			if ( !Limited )
				AddFile(new PropertiesDataExportFile(this));

			AddFile(new ParentChildLinksetDataExportFile(this, ParentChildRelationship.ChargeInsensitive));
			AddFile(new ParentChildLinksetDataExportFile(this, ParentChildRelationship.Fragment));
			AddFile(new ParentChildLinksetDataExportFile(this, ParentChildRelationship.IsotopInsensitive));
			AddFile(new ParentChildLinksetDataExportFile(this, ParentChildRelationship.StereoInsensitive));
			AddFile(new ParentChildLinksetDataExportFile(this, ParentChildRelationship.SuperInsensitive));
			AddFile(new ParentChildLinksetDataExportFile(this, ParentChildRelationship.TautomerInsensitive));

			//Export the OPS-ChemSpider linkset.
			AddFile(new ChemSpiderLinksetDataExportFile(this));

			//Export the Issues.
			AddFile(new IssuesDataExportFile(this));
		}

		/// <summary>
		/// Constructor is passed the Data Source Id to generate the export for.
		/// </summary>
		public OpsDataSourceExport(Guid dataSourceId, DateTime exportStart)
		{
			setupDataSource(dataSourceId);

			ExportDate = exportStart;
			ExportDirectory = String.Format("{0}/{1}", ExportDate.ToString("yyyyMMdd"), DsnLabel);

			addFiles();
		}

		/// <summary>
		/// This constructor is to rerun previous data export 
		/// </summary>
		public OpsDataSourceExport(int exportId, IDataExportStore store)
		{
			ExportId = exportId;

			DataExportStore = store;

			var export = store.GetDataExportLog(exportId);
			ExportDate = (DateTime)export.ExportDate;
			ExportDirectory = export.ExportDirectory;
			setupDataSource(export.DataSourceId);

			// Add files objects...
			addFiles();

			// ...and initialize them from database
			foreach ( IDataExportFile file in Files ) {
				DataExportLogFile logFile = export
					.Files
					.Where(f => f.FileName == file.FileName)
					.Single();
				file.Id = logFile.Id;
				file.FileName = logFile.FileName;
				file.RecordCount = logFile.RecordCount;
				file.Status = logFile.Status;
				file.ErrorMessage = logFile.ErrorMessage;
			}
		}

		public override void PreValidate()
		{
			var cmpIds = CompoundsStore.GetDataSourceCompoundIds(DataSource.Guid, 0, 1);
			if ( cmpIds.Count() != 1 )
				throw new CompoundsExportException("Datasource ({0}) is empty", DsnLabel);

			var compounds = CompoundsStore
				.GetCompounds(cmpIds, new[] { "ExternalReferences", "Id" });
			if ( compounds.Count() != 1 )
				throw new CompoundsExportException("Cannot get compound by id ({0}) - something is wrong with database content", cmpIds.First());

			var extRefs = compounds 
				.First()
				.ExternalReferences;
			if ( extRefs == null || extRefs.Count() == 0 )
				throw new CompoundsExportException("No external references for compound ({0})", cmpIds.First());

			var opsRefs = extRefs
				.Where(e => e.Type.UriSpace == Constants.OPSUriSpace);
			if ( opsRefs.Count() != 1 )
				throw new CompoundsExportException("It looks like OPS IDs were not assigned - run OpsIdImport in OpenPHACTS solution");
		}

		public override void PostValidate()
		{

		}

		public bool Limited { get; set; }
	}
}
