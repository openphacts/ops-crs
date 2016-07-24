using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Compounds
{
	public class ReadGzip : IReadRecords
	{
		private string TemporaryFile { get; set; }
		public string FilePath { get; private set; }
		public string SdfFieldFile { get; private set; }
		public FileStream InputFileStream { get; private set; }
		
		private IReadRecords Reader { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path">Gzip file</param>
		public ReadGzip(string path, string sdfFieldFile=null)
		{
			if (String.IsNullOrEmpty(path) || !File.Exists(path) || !Path.GetExtension(path).ToLower().Equals(".gz"))
				return;
			TemporaryFile = Path.GetTempFileName();
			using (FileStream output = new FileStream(TemporaryFile, FileMode.Create))
			using (FileStream input = new FileStream(path, FileMode.Open))
			using (GZipStream gz = new GZipStream(input, CompressionMode.Decompress, false))
			{
				gz.CopyTo(output);
			}

			InputFileStream = new FileStream(TemporaryFile, FileMode.Open);
			SdfFieldFile = sdfFieldFile;
			FilePath = path;

			string uncompressedFileName = FilePath.ToLower().Replace(".gz", "");
			if (Path.GetExtension(uncompressedFileName).Equals(".sdf"))
			{
				Reader = new ReadSdf(InputFileStream, uncompressedFileName);
			}
			else if (Path.GetExtension(uncompressedFileName).Equals(".mol"))
			{
				Reader = new ReadMol(InputFileStream, uncompressedFileName);
			}
			else if (Path.GetExtension(uncompressedFileName).Equals(".cdx"))
			{
				Reader = new ReadCdx(InputFileStream, uncompressedFileName);
			}
		}

		public IEnumerable<Record> ReadChunk(int size = 1000)
		{
			return Reader.ReadChunk(size);
		}

		public void Release()
		{
			InputFileStream.Close();
			if (File.Exists(TemporaryFile))
				File.Delete(TemporaryFile);
		}

		public void Reset()
		{
		}
	}
}
