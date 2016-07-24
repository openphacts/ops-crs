using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;
using VDS.RDF;

using RSC.Compounds;
using RSC.Compounds.DataExport;
using RSC.Compounds.EntityFramework;
using RSC.Datasources;
using RSC.Properties;

namespace RSC.Compounds.DataExport_Tests
{
    [TestClass]
    public class PropertyDataExport : DataExportTestBase
    {
        [TestMethod]
        public void RSC_ExternalIdEquality()
        {
            ExternalId e1 = new ExternalId() { DomainId = 1, ObjectId = new Guid("22222222-3333-4444-5555-666666666666") };
            ExternalId e2 = new ExternalId() { DomainId = 1, ObjectId = new Guid("22222222-3333-4444-5555-666666666666") };
            Assert.AreEqual(e1, e2);
            var list = new[] { e2 }.ToList();
            Assert.IsTrue(list.Contains(e1));
        }

        [TestMethod]
        public void PropertyDataExportFile_Constructor()
        {
			var export = new OpsDataSourceExport(dsGuid, DateTime.Now);
            PropertiesDataExportFile pff = new PropertiesDataExportFile(export);
        }

        /*[TestMethod]
        public void PropertyDataExportFile_EmptyGraph()
        {
            PropertiesDataExportFile pff = new PropertiesDataExportFile(new DataSource() { Name = "Test" } );
            var result = pff.ExportLines(dataExport); // defined in DataExportTestBase
            Assert.IsTrue(result.Any(), "there should be lines produced");
            Assert.AreEqual(6, result.Where(l => l.StartsWith("#")).Count(), "wrong number of comments");
            Assert.AreEqual(1, result.Where(l => l.StartsWith("<")).Count(), "wrong number of triples");
        }*/
        
        [TestMethod]
        public void PropertyTurtle_ExportRevisionLines()
        {
			var export = new OpsDataSourceExport(dsGuid, DateTime.Now);
            Guid rguid = new Guid("22222222-3333-4444-5555-666666666666");
            Guid cguid = new Guid("12345678-abcd-abcd-1234-12345678abcd");
            PropertiesDataExportFile pff = new PropertiesDataExportFile(export);
            // fake up externalId in the same way as the inside of ExportRevisionLines.
            var externalId = new ExternalId() { DomainId = 1, ObjectId = rguid };
            var dict = new Dictionary<ExternalId, IEnumerable<Property>>() { { externalId, properties } };
            var compounds = new List<Compound>() { new Compound() { 
                Id = cguid,
                ExternalReferences = new List<ExternalReference>() { 
                    new ExternalReference() { Type = opsReferenceType } }
            } };
            Revision revision = new Revision() { Id = rguid, CompoundId = cguid };
            var result = pff.ExportRevisionLines(revision, dict, compounds, 
                // from DataExportTestBase
                propertyDefs);
            Assert.IsTrue(result.Any(), "there should be lines of text in the export");
        }
        
		/*
        //[TestMethod]
        /// <summary>
        /// 2015-10-26: I am still trying to work out why this is failing.
        /// </summary>
        public void PropertyDataExportFile_ContainsRevisions()
        {
            PropertiesDataExportFile pff = new PropertiesDataExportFile(new DataSource() { Name = "Test" });
            var revisionIds = new List<Guid>() { new Guid("12345678-abcd-abcd-1234-12345678abcd") };
            var result = pff.ExportChunkLines(dataExport, revisionIds, propertyDefs);
            Assert.IsTrue(result.Any());
        }*/

        [TestMethod]
        public void PropertyDataExportFile_PrefixLines()
        {
			var export = new OpsDataSourceExport(dsGuid, DateTime.Now);
            PropertiesDataExportFile pff = new PropertiesDataExportFile(export);
            var result = pff.PrefixLines("ftp://ftp.rsc.org/dummyproperties", TurtlePrefixSets.Properties).ToList();
            Assert.IsTrue(result.Any(), "no prefixes produced");
            foreach (string line in result)
            {
                Assert.IsTrue(line.StartsWith("@prefix"), "malformed line: " + line);
                Assert.IsTrue(line.Contains(": <"), "malformed line: " + line);
                Assert.IsTrue(line.EndsWith("> ."), "malformed line: " + line);
            }
        }
    }
}
