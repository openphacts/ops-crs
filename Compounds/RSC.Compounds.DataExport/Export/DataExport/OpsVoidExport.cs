using System;
using System.Linq;
using System.Collections.Generic;

namespace RSC.Compounds.DataExport
{
	/// <summary>
	/// Class for generating the Void.ttl metadata information for a list of data sources, for the OPS project.
	/// </summary>
	public class OpsVoidExport : DataExport, ILimitedExport
	{
		private readonly IDictionary<int, Guid> _dataSourceExportIds;

		/// <summary>
		/// Constructor is passed the dsns to generate the export for.
		/// </summary>
		/// <param name="dataSourceExportIds">Dictionary of Data Source Ids and Export Ids to generate the export for.</param>
		public OpsVoidExport(IDictionary<int, Guid> exports, DateTime exportStart)
		{
			_dataSourceExportIds = exports;

			ExportDate = exportStart;

			// VoID file goes into the root directory for the export date
			ExportDirectory = ExportDate.ToString("yyyyMMdd");

			AddFile(new VoidDataExportFile(this, _dataSourceExportIds));
		}

		public bool Limited { get; set; }
	}
}
