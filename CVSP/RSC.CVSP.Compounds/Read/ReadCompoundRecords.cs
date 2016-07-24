using ChemSpider.Molecules;
using ChemSpider.Utilities;
using MoleculeObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RSC.CVSP.Compounds
{
	public class ReadCompoundRecords : IReadRecords
	{
		private string FilePath;
		private IReadRecords Reader { get; set; }
		public int ReadRecords { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filePath">path to *.gz or *.zip compressed file name</param>
		public ReadCompoundRecords(string filePath)
		{
			if (!File.Exists(filePath))
				return;

			FilePath = filePath;
			ReadRecords = 0;
			PrepareReader();
		}

		public void Release()
		{
			if (Reader != null)
				Reader.Release();
		}

		public void Reset()
		{
			ReadRecords = 0;
		}

		public IEnumerable<Record> ReadChunk(int size = 1000)
		{
			return Reader.ReadChunk(size);
		}

		private void PrepareReader()
		{
			//before creating a new reader we check that that resources for the previous reader are released
			if (Reader != null)
				Reader.Release();

			string extension = Path.GetExtension(FilePath.ToLower().Replace(".gz", "").Replace(".zip", ""));

			if (FilePath.ToLower().EndsWith(".gz"))
			{
				Reader = new ReadGzip(FilePath);
			}
			else if (!FilePath.ToLower().EndsWith(".gz") && !FilePath.ToLower().EndsWith(".zip"))
			{
				if (extension.Equals(".sdf"))
					Reader = new ReadSdf(FilePath);
				else if(extension.Equals(".mol"))
					Reader = new ReadMol(FilePath);
				else if(extension.Equals(".cdx"))
					Reader = new ReadCdx(FilePath);
			}
			else if (FilePath.ToLower().EndsWith(".zip"))
			{
				Reader = new ReadZip(FilePath);
			}
		}
	}
}
