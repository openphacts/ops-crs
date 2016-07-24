using ChemSpider.Molecules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Compounds
{
	/// <summary>
	/// Reads uncompressed Sdf
	/// </summary>
	public class ReadSdf : IReadRecords
	{
		public int Ordinal { get; private set; }
		private SdfReader Reader { get; set; }
		public string FilePath { get; private set; }
		public IList<string> SdfFields { get; private set; }
		private IEnumerator Enumerator { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path">path to uncompressed Sdf</param>
		public ReadSdf(string path)
		{
			if (File.Exists(path))
				LoadFromFile(path);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileStream">uncompressed Sdf Stream</param>
		public ReadSdf(Stream file, string path)
		{
			LoadFromStream(file, path);
		}

		public ReadSdf()
		{
			Ordinal = 0;
		}

		public void LoadFromStream(Stream stream, string path)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			FilePath = path;
			SdfFields = new List<string>();
			Ordinal = 0;
			Reader = new ChemSpider.Molecules.SdfReader(stream);
			Enumerator = Reader.Records.GetEnumerator();
			Enumerator.MoveNext();
		}

		public void LoadFromFile(string path)
		{
			LoadFromStream(File.OpenRead(path), path);
		}

		public IEnumerable<Record> ReadChunk(int size = 1000)
		{
			ICollection<Record> records = new List<Record>();
			while (true)
			{
				if (Enumerator.Current == null)
					break;
				SdfRecord record = Enumerator.Current as SdfRecord;
				records.Add(new CompoundRecord()
				{
					Ordinal = Ordinal,
					Original = record.ToString(),
					DataDomain = DataDomain.Substances,
					File = new DepositionFile() {
						//Path = Path.GetDirectoryName(FilePath),
						Name = Path.GetFileName(FilePath)
					},
					Fields = record.Properties.Select(p => p.Value.Select(v => new RecordField() { Name = p.Key, Value = v })).SelectMany(f => f).ToList()
				});

				Ordinal++;
				if (!Enumerator.MoveNext() || records.Count >= size)
					break;
			}
			return records;
		}

		public void Release()
		{
			Reader.Dispose();
		}

		public void Reset()
		{
			Ordinal = 0;
		}
	}
}