using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RSC.CVSP.Compounds
{
	public class ReadMol : IReadRecords
	{
		public Stream InputStream { get; private set; }
		public string FilePath { get; private set; }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="path">path to uncompressed Molfile</param>
		public ReadMol(string path)	: this(File.OpenRead(path),path)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileStream"></param>
		/// <param name="path">path to uncompressed Molfile</param>
		public ReadMol(Stream fileStream, string path)
		{
			FilePath = path;
			if (fileStream != null)
				InputStream = fileStream;
		}

		public static string MolToSdf(string mol)
		{
			Regex reg = new Regex(@"\$\$\$\$(\r\n)?$");

			return reg.IsMatch(mol) ? mol : mol.TrimEnd() + Environment.NewLine + "$$$$" + Environment.NewLine;
		}

		public IEnumerable<Record> ReadChunk(int size = 1000)
		{
			ICollection<Record> records = new List<Record>();
			if (!InputStream.CanRead)
				return records;
			using (StreamReader reader = new StreamReader(InputStream))
			{
				records.Add(new CompoundRecord()
				{
					Ordinal = 0,
					Original = MolToSdf(reader.ReadToEnd()),
					DataDomain = DataDomain.Substances,
					File = new DepositionFile() {
						//Path = Path.GetDirectoryName(FilePath),
						Name = Path.GetFileName(FilePath)
					},
					//ExternalId = Path.GetFileNameWithoutExtension(FilePath)
				});
			}
			return records;
		}

		public void Release()
		{
			InputStream.Close();
		}

		public void Reset()
		{
		}
	}
}
