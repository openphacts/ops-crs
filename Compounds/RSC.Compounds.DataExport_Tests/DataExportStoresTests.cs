using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RSC.Compounds.DataExport;
using RSC.Compounds.EntityFramework;
using RSC.Properties.EntityFramework;
using DeepEqual.Syntax;

namespace RSC.Compounds.DataExport_Tests
{
	[TestClass]
	public class DataExportStoresTests
	{
		[TestMethod]
		public void GetCompoundIds()
		{
			var propertiesStore = new EFPropertyStore();
			var compoundsStore = new EFCompoundStore(propertiesStore);
			var ids = compoundsStore.GetCompoundIds();
			Assert.AreEqual(141480, ids.Count());

			ids = compoundsStore.GetCompoundIds(1000, 1000);
			Assert.AreEqual(1000, ids.Count());
		}

		[TestMethod]
		public void GetDataSourceCompoundIds()
		{
			var propertiesStore = new EFPropertyStore();
			var compoundsStore = new EFCompoundStore(propertiesStore);
			var ids = compoundsStore.GetDataSourceCompoundIds(Guid.Parse("3384B886-67EA-4793-8240-5AF5D033D5FC"));
			Assert.AreEqual(2000, ids.Count());
		}

		[TestMethod]
		public void GetDataSourceCompoundIds_HMDB()
		{
			var propertiesStore = new EFPropertyStore();
			var compoundsStore = new EFCompoundStore(propertiesStore);
			var ids = compoundsStore.GetDataSourceCompoundIds(Guid.Parse("fc686e2a-11d6-48d5-baa7-3c5232f5bcb0"));
			Assert.AreEqual(2000, ids.Count());
		}

		[TestMethod]
		public void GetSubstanceIds()
		{
			var propertiesStore = new EFPropertyStore();
			var compoundsStore = new EFCompoundStore(propertiesStore);
			var ids = compoundsStore.GetSubstanceIds(Guid.Parse("170B7303-E74F-42E9-A239-0005B4B3FF81"));
			Assert.AreEqual(2, ids.Count());
		}

		[TestMethod]
		public void GetCompoundsPropertiesIds()
		{
			var propertiesStore = new EFPropertyStore();
			var compoundsStore = new EFCompoundStore(propertiesStore);
			var ids = compoundsStore.GetCompoundsPropertiesIds(new[] { Guid.Parse("170B7303-E74F-42E9-A239-0005B4B3FF81"), Guid.Parse("DE5A07CD-B423-4235-97CB-0013D9409DC8") });
			Assert.AreEqual(2, ids.Count());
			Assert.AreEqual(74, ids.First().Value.Count());
			Assert.AreEqual(74, ids.Last().Value.Count());
		}

		[TestMethod]
		public void GetDataSourceRevisionIds()
		{
			var propertiesStore = new EFPropertyStore();
			var compoundsStore = new EFCompoundStore(propertiesStore);
			var ids = compoundsStore.GetDataSourceRevisionIds(Guid.Parse("3384B886-67EA-4793-8240-5AF5D033D5FC"));
			Assert.AreEqual(2000, ids.Count());
		}
	}
}
