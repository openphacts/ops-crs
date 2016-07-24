using ChemSpider.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Compounds
{
	public class ReadZip : IReadRecords
	{
		private IReadRecords Reader { get; set; }

		private string TemporaryDirectory { get; set;}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path">Zip file</param>
		public ReadZip(string path)
			: this(File.OpenRead(path))
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="file">Stream to Zip file</param>
		public ReadZip(Stream fileStream)
		{
			TemporaryDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(TemporaryDirectory);
			ZipUtils.UnzipFiles(fileStream, TemporaryDirectory,null);
			Reader = new ReadFolder(TemporaryDirectory);
		}

		public IEnumerable<Record> ReadChunk(int size = 1000)
		{
			return Reader.ReadChunk(size);
		}

		public void Release()
		{
			Reader.Release();
			if (Directory.Exists(TemporaryDirectory))
				Directory.Delete(TemporaryDirectory, true);
		}

		public void Reset()
		{
			//SdfReader = null;
			//MolfileReader = null;
		}

		~ReadZip()
		{
			//Release();
		}
	}
}
