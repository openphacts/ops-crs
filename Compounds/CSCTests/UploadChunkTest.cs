using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;
using RSC.Compounds;
using RSC.Compounds.EntityFramework;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace RSC.Compounds.Tests
{
	//[TestClass]
	public class UploadChunkTest : CompoundsTestBase
	{
		[TestMethod]
		public void BulkUpload()
		{
			IEnumerable<RecordData> data = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<RecordData>>(TestResources.upload2gcnJSON, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

			//var ds = new DataContractSerializer(typeof(IEnumerable<RecordData>));

			//using (XmlReader reader = XmlReader.Create(new StringReader(TestResources.upload2gcn)))
			//{
			//	data = (IEnumerable<RecordData>)ds.ReadObject(reader);
			//}

			//using (CompoundsContext context = new CompoundsContext())
			//{
			//	var count = context.Compounds.Count();
			//}

			var watch = Stopwatch.StartNew();

			SubstanceBulkUpload.BulkUpload(Guid.Parse("AF824C22-CE35-4BE1-A68D-A24D59A47E69"), data);

			watch.Stop();
			var elapsedMs = watch.ElapsedMilliseconds;
		}
	}
}
