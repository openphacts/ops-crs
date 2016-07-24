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

namespace RSC.Compounds.Tests
{
	//[TestClass]
	public class DeleteDepositionTest : CompoundsTestBase
	{
		[TestMethod]
		public void DeleteDeposition()
		{
			SubstanceStore.DeleteDeposition(Guid.Parse("2563f3ce-1453-4def-acce-672770e5d0fd"));
		}
	}
}
