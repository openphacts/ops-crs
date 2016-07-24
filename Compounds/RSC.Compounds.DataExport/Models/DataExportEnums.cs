using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.DataExport
{
	public enum DataExportStatus
	{
		/// <summary>
		/// Initial status
		/// </summary>
		Started,

		/// <summary>
		/// Something wrong has happened - export can be restarted
		/// </summary>
		ExportFailed,

		/// <summary>
		/// File exported, but not copied yet to the final destination
		/// </summary>
		Exported,

		/// <summary>
		/// File exported, but copying to the final destination failed
		/// </summary>
		UploadFailed,

		/// <summary>
		/// Complete export including copying to the final destination has succeeded
		/// </summary>
		Succeeded
	}
}
