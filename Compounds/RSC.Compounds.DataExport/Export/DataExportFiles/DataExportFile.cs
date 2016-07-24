using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RSC.Compounds.DataExport
{
	/// <summary>
	/// Base class used for Data Export Files - this class should be reused for all types of Data Export Files.
	/// </summary>
	public class DataExportFile : IDataExportFile
	{
		/// <summary>
		/// Unique Identifier for the Data Export File.
		/// </summary>
		public int Id { get; set; }

		public string FileName { get; set; }

		public string FileFullPath { get; set; }

		public string CompressedFileName { get; set; }

		public string CompressedFileFullPath { get; set; }

		public DataExportStatus Status { get; set; }

		public string ErrorMessage { get; set; }

		public string TmpDir { get; protected set; }

		/// <summary>
		/// The count of number of triples in the file (required for Void information).
		/// </summary>
		protected int recordCount;
		public int RecordCount
		{
			get { return recordCount; }
			set { recordCount = value; }
		}

		public DataExportFile()
		{

		}

		public virtual void Export(IDataExport exp, Encoding encoding)
		{
			//Set the FileFullPath
			if ( FileName != string.Empty ) {
				if ( String.IsNullOrEmpty(TmpDir) )
					TmpDir = Path.GetTempPath();
				FileFullPath = Path.Combine(TmpDir, FileName);

				if ( File.Exists(FileFullPath) )
					File.Delete(FileFullPath);
			}
		}

		public void Compress()
		{
			if ( !string.IsNullOrEmpty(FileName) ) {
				FileInfo zippedFile = ZipUtils.Compress(new FileInfo(FileName));
				CompressedFileName = zippedFile.Name;
				CompressedFileFullPath = zippedFile.FullName;
			}
		}

		public void Upload(IDataExport exp, IFileUploader uploader)
		{
			string uploadDir = String.Format("{0}/{1}", exp.UploadUrl, exp.ExportDirectory);
			uploader.CreateDirectoryIfNotExists(uploadDir);
			uploader.UploadAsync(CompressedFileFullPath ?? FileFullPath, String.Format("{0}/{1}", uploadDir, CompressedFileName ?? FileName));
		}

		public virtual void PreValidate(IDataExport exp)
		{
			
		}

		public virtual void PostValidate(IDataExport exp)
		{
			
		}
	}
}
