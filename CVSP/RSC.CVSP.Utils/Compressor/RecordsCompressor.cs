using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSC.CVSP;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Xml;
using RSC.CVSP.Compounds;

namespace RSC.CVSP
{
	public class RecordsCompressor : ICompressRecords
	{
		public string OutputFile { get; set; }
		public RecordsCompressor(string file)
		{
			OutputFile = file;
		}

		public RecordsCompressor()
		{
		}

		public bool Compress(IEnumerable<Record> records)
		{
			if (String.IsNullOrEmpty(OutputFile))
				throw new ArgumentNullException("OutputFile");

			XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
			using (FileStream RecordSerializationStream = new FileStream(OutputFile, FileMode.Create))
			using (GZipStream compress = new GZipStream(RecordSerializationStream, CompressionMode.Compress))
			{
				var ds = new DataContractSerializer(typeof(IEnumerable<Record>), new[] { typeof(CompoundRecord), typeof(ReactionRecord) });
				using (XmlWriter w = XmlWriter.Create(compress, settings))
					ds.WriteObject(w, records);
			}
			return true;
		}

		public IEnumerable<Record> Uncompress(string path)
		{
			if (!File.Exists(path))
				return new List<Record>();

			using (FileStream RecordSerializationStream = new FileStream(path, FileMode.Open))
			using (GZipStream decompress = new GZipStream(RecordSerializationStream, CompressionMode.Decompress))
			{
				var ds = new DataContractSerializer(typeof(IEnumerable<Record>), new[] { typeof(CompoundRecord), typeof(ReactionRecord) });
				using (XmlReader xml_r = XmlReader.Create(decompress))
					return (IEnumerable<Record>)ds.ReadObject(xml_r);
			}
		}
	}
}
