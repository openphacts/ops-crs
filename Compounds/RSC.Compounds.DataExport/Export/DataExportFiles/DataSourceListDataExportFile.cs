using System;
using System.Collections.Generic;
using RSC.Datasources;

namespace RSC.Compounds.DataExport
{
	/// <summary>
	/// Used for exporting a data export file based on a particular list of Data Sources and their associated ExportIds.
	/// </summary>
	public class DataSourceListDataExportFile : DataExportFile
	{
		public IDictionary<int, Guid> DataSourceExportIds { get; protected set; }
		public IDataSourcesClient DataSourcesClient { get; protected set; }
		public List<DataSource> DataSources { get; protected set; }

		/// <summary>
		/// Constructor is passed list of data source ids.
		/// </summary>
		/// <param name="dataSourceExportIds">The dictionary of Data source ids and export ids to generate the Data Export from.</param>
		/// <param name="dsc">The DataSources client. Normally this would wrap a database but you can just put a pretend on in.</param>
		public DataSourceListDataExportFile(IDictionary<int, Guid> dataSourceExportIds, IDataSourcesClient dsc = null)
		{
			//Initialise.
			DataSourceExportIds = dataSourceExportIds;
			DataSourcesClient = ( dsc == null ) ? new DataSourcesClient() : dsc;
			DataSources = new List<DataSource>();

			foreach ( var dataSourceId in DataSourceExportIds.Values ) {
				var dataSource = DataSourcesClient.GetDataSource(dataSourceId);
				if ( dataSource != null )
					DataSources.Add(dataSource);
				else
					throw new Exception(string.Format("Invalid DataSource Id : {0}", dataSourceId.ToString()));
			}
		}
	}
}
