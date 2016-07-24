using System;

namespace RSC.Compounds.DataExport
{
	/// <summary>
	/// Exports an SDF containing chemspider_id, ops_id, chemspider_url, ops_url.
	/// </summary>
	public class OpsSdfMapExport : DataExport
	{

		public OpsSdfMapExport(DateTime exportStart, int? id, IDataExportStore store, string tmpDir)
		{
			if ( id != null ) {
				var exp = store.GetDataExportLog((int)id);
				exportStart = (DateTime)exp.ExportDate;
			}

			ExportDate = exportStart;

			// This export goes into the root directory for the export date
			ExportDirectory = ExportDate.ToString("yyyyMMdd");

			AddFile(new OpsSdfMapDataExportFile(this, tmpDir));
		}
	}
}
