using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Compounds
{
	public class ReadFolder : IReadRecords
	{
		public IEnumerable<string> Files { get; private set; }

		private IReadRecords Reader { get; set; }
		private IEnumerator Enumerator { get; set; }

		public string SdfFieldFile { get; private set; }

		public ReadFolder(string directory, string sdfFieldFile=null)
		{
			Files = Directory.GetFiles(directory);
			SdfFieldFile = sdfFieldFile;
			Enumerator = Files.GetEnumerator();
			Enumerator.MoveNext();
		}

		public IEnumerable<Record> ReadChunk(int size = 1000)
		{
			List<Record> records = new List<Record>();

			//	wea already have some reader so, we have to read there first...
			if (Reader != null)
			{
				records.AddRange(Reader.ReadChunk(size));

				//	... if we were able to read the chunk from the reader...
				if (size <= records.Count)
					return records;

				if (!Enumerator.MoveNext())
				{
					//	we are at the end of files' list and don't have anything to read...
					return records;
				}
			}

			//	... we read less than chunk size (or we just started) and have to continue...
			while(true)
			{
				var file = Enumerator.Current as string;

				switch (Path.GetExtension(file).ToLower())
				{
					case ".sdf":
						Reader = new ReadSdf(file);
						break;
					case ".mol":
						Reader = new ReadMol(file);
						break;
					case ".cdx":
						Reader = new ReadCdx(file);
						break;
					default:
						throw new NotSupportedException("Not supported file type!");
				}

				records.AddRange(Reader.ReadChunk(size - records.Count));

				if (size <= records.Count)
					return records;

				if (!Enumerator.MoveNext())
				{
					//	we are at the end of files' list and don't have anything to read...
					return records;
				}
			}
		}

		public void Release()
		{
			Reader.Release();
		}

		public void Reset()
		{
		}
	}
}
