using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace RSC.Compounds.DataExport
{
	public interface IDataExportFile
	{
		int Id { get; set; }

		string FileName { get; set; }

		string FileFullPath { get; set; }

		string CompressedFileName { get; set; }

		string CompressedFileFullPath { get; set; }

		DataExportStatus Status { get; set; }

		string ErrorMessage { get; set; }

		/// <summary>
		/// The count of number of triples in the file (required for Void information).
		/// </summary>
		int RecordCount { get; set; }

		void PreValidate(IDataExport exp);

		void Export(IDataExport exp, Encoding encoding);

		void PostValidate(IDataExport exp);

		void Compress();

		void Upload(IDataExport exp, IFileUploader uploader);
	}
}
