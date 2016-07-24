using MoleculeObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Compounds
{
	public class ReadCdx : IReadRecords
	{
		private IEnumerable<string> molFiles = new List<string>();
		public int Ordinal {get; private set;}
		public string FilePath { get; private set; }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="path">path to uncompressed Cdx</param>
		public ReadCdx(string path) : this(File.OpenRead(path),path)
		{
				
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileStream">uncompressed Stream</param>
		public ReadCdx(Stream fileStream, string path)
		{
			Ordinal = 0;
			FilePath = path;
			if (fileStream != null)
			{
				using (var streamReader = new MemoryStream())
				{
					byte[] result;
					fileStream.CopyTo(streamReader);
					result = streamReader.ToArray();
					molFiles = (new Cdx(result)).ToMolFiles(CdxEnumerateOptions.EnumerateMarkush);
				}
			}
		}

		public IEnumerable<Record> ReadChunk(int size = 1000)
		{
			var records = new List<Record>();
			foreach (string molfile in molFiles.Skip(Ordinal).Take(size))
			{
				records.Add(new CompoundRecord()
				{
					Ordinal = Ordinal,
					Original = ReadMol.MolToSdf(molfile),
					DataDomain = DataDomain.Substances,
					File = new DepositionFile() {
						//Path = Path.GetDirectoryName(FilePath),
						Name = Path.GetFileName(FilePath)
					}
				});
				Ordinal++;
			}
			return records;
		}

		public void Release()
		{
		}

		public void Reset()
		{
		}
	}
}
