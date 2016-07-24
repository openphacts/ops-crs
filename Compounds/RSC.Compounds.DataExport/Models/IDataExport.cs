using System;
using System.Collections.Generic;
using System.Text;
using RSC.Datasources;
using RSC.Logging;
using RSC.Properties;

namespace RSC.Compounds.DataExport
{
	public interface IDataExport : IDisposable
	{
		IDataExportStore DataExportStore { get; set; }

		CompoundStore CompoundsStore { get; set; }

		IPropertyStore PropertiesStore { get; set; }

		ILogStore LogStore { get; set; }

		int VersionId { get; set; }

		List<IDataExportFile> Files { get; }

		Encoding Encoding { get; set; }

		bool Compress { get; set; }

		string UploadUrl { get; set; }

		string DownloadUrl { get; set; }

		string Username { get; set; }

		string Password { get; set; }

		DataExportStatus Status { get; set; }
		string ErrorMessage { get; set; }

		int ExportId { get; set; }

		/// <summary>
		/// Relative directory where export files will be placed
		/// </summary>
		string ExportDirectory { get; set; }

		DateTime ExportDate { get; set; }

		void AddFile(IDataExportFile fileToAdd);

		void PreValidate();

		void Export();

		void PostValidate();
	}
}
