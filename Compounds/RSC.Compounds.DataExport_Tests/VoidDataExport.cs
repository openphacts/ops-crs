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
    public class VoidDataExport : DataExportTestBase
    {
        /// <summary>
        /// Checks that we have a config file and everything.
        /// </summary>
        [TestMethod]
        public void VoidDataExportFile_Constructor()
        {
			VoidDataExportFile f = new VoidDataExportFile(dataExport, new Dictionary<int, Guid>());
        }

        /// <summary>
        /// checks that guids and everything are wired up properly
        /// </summary>
        [TestMethod]
        public void DataExport_SetupDataExportStore()
        {
            Guid guid = new Guid("1441d974-bd2c-44ae-8b27-110089dc8e44");
            var m_dataexportstore = new Mock<RSC.Compounds.DataExport.IDataExportStore>();
            m_dataexportstore.Setup(c => c.GetCurrentDataVersion(guid)).Returns(new DataVersion()
            {
                VoidUri = "http://www.rsc.org/void.ttl"
            });
            var dataexportstore = m_dataexportstore.Object;
            DataExport.DataExport dataExport = new DataExport.DataExport()
            {
				DownloadUrl = "ftp://ftp.rsc.org/ops/",
                DataExportStore = dataexportstore
            };
            var dv = dataExport.DataExportStore.GetCurrentDataVersion(guid);
            Assert.IsNotNull(dv);
            
        }

        [TestMethod]
        public void VoidDataExportFile_EmptyGraph()
        {
			VoidDataExportFile f = new VoidDataExportFile(dataExport, dataSources, dataSourcesClient);
            Assert.AreEqual(1, f.DataSources.Count);
            Assert.AreEqual(dsGuid, f.DataSources.First().Guid);
            var result = f.VoidGraph(dataExport);
            Assert.IsTrue(result.Triples.Any(), "should be some triples in the graph");
            var triplecounts = TriplesByPredicate(result, "http://rdfs.org/ns/void#triples");
            Assert.AreEqual(8, triplecounts.Count());
            Assert.AreEqual("0^^http://www.w3.org/2001/XMLSchema#integer", triplecounts.First().Object.ToString()); // this is an *empty* graph
            Assert.AreEqual("0^^http://www.w3.org/2001/XMLSchema#integer", triplecounts.ElementAt(1).Object.ToString()); // this is an *empty* graph
            var topleveltriples = TriplesBySubject(result, "ftp://ftp.rsc.org/ops///");

            var testsubset = TriplesBySubject(result, "ftp://ftp.rsc.org/ops///#openphacts-test");
            var testdataset = TriplesBySubject(result, "ftp://ftp.rsc.org/ops///#openphactsDataset");
            TestDatasetVoidPredicates(testdataset);
            var exactMatch = TriplesBySubject(result, "ftp://ftp.rsc.org/ops///#test_exactMatch");
            var CSexactMatch = TriplesBySubject(result, "ftp://ftp.rsc.org/ops///#test_ops_chemspider_exactMatch");
            Assert.AreEqual(7, topleveltriples.Count());
            Assert.AreEqual(15, testsubset.Count(), "wrong number of triples in test subset");
            Assert.AreEqual(3, TriplesByPredicate(testsubset, "http://rdfs.org/ns/void#dataDump").Count(), "should be three data dumps");
            Assert.AreEqual(15, exactMatch.Count());
            TestSubsetVoidPredicates(exactMatch);
            Assert.AreEqual(15, CSexactMatch.Count());
            TestSubsetVoidPredicates(CSexactMatch);
            Assert.AreEqual("A VoID Description of the OpenPhacts Dataset@en",
                TriplesByPredicate(topleveltriples, "http://purl.org/dc/terms/title").First().Object.ToString());
            Assert.IsTrue(exactMatch.Any(t => t.Predicate.ToString().Contains("expresses")), "exact set lacks justification");
            Assert.IsTrue(CSexactMatch.Any(t => t.Predicate.ToString().Contains("expresses")), "exact CS set lacks justification");

            //
            foreach (var t in result.Triples) Console.WriteLine(t);
        }

        public void TestPredicate(IEnumerable<Triple> triples, string predicate, string description)
        {
            Assert.IsTrue(triples.Any(t => t.Predicate.ToString() == predicate), description + " not specified");
        }

        public void TestTopLevelVoidPredicates(IEnumerable<Triple> triples)
        {
            TestPredicate(triples, "http://purl.org/dc/terms/description", "top level description");
            TestPredicate(triples, "http://purl.org/dc/terms/title", "top level title");
            TestPredicate(triples, "http://purl.org/pav/createdBy", "top level created by");
            TestPredicate(triples, "http://purl.org/pav/createdOn", "top level created on date");
            TestPredicate(triples, "http://purl.org/pav/lastUpdateOn", "top level last update on date");
            TestPredicate(triples, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "top level type");
        }

        public void TestDatasetVoidPredicates(IEnumerable<Triple> triples)
        {
            TestPredicate(triples, "http://purl.org/dc/terms/description", "description");
            TestPredicate(triples, "http://purl.org/dc/terms/created", "dataset DC created");
            TestPredicate(triples, "http://purl.org/dc/terms/modified", "dataset DC modified");
            TestPredicate(triples, "http://purl.org/dc/terms/publisher", "dataset DC publisher");
            TestPredicate(triples, "http://purl.org/dc/terms/title", "title");
            TestPredicate(triples, "http://purl.org/dc/terms/license", "licence");
            TestPredicate(triples, "http://rdfs.org/ns/void#exampleResource", "VoID example resource");
            TestPredicate(triples, "http://rdfs.org/ns/void#feature", "VoID feature");
            TestPredicate(triples, "http://rdfs.org/ns/void#subset", "VoID subset");
            TestPredicate(triples, "http://rdfs.org/ns/void#uriSpace", "VoID URI space");
            TestPredicate(triples, "http://rdfs.org/ns/void#vocabulary", "VoID vocabulary");
            TestPredicate(triples, "http://voag.linkedmodel.org/schema/voag#frequencyOfChange", "frequency of change");
            TestPredicate(triples, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "dataset type");
            TestPredicate(triples, "http://xmlns.com/foaf/0.1/homepage", "dataset FOAF homepage");
        }

        public void TestSubsetVoidPredicates(IEnumerable<Triple> triples)
        {
            TestPredicate(triples, "http://purl.org/dc/terms/description", "description");
            TestPredicate(triples, "http://purl.org/dc/terms/title", "title");
            TestPredicate(triples, "http://purl.org/dc/terms/license", "licence");
            TestPredicate(triples, "http://purl.org/pav/authoredBy", "author");
            TestPredicate(triples, "http://purl.org/pav/authoredOn", "authoring date");
            TestPredicate(triples, "http://purl.org/pav/createdBy", "creator");
            TestPredicate(triples, "http://purl.org/pav/createdWith", "creation software");
            TestPredicate(triples, "http://purl.org/pav/createdOn", "creation date");
            TestPredicate(triples, "http://rdfs.org/ns/void#subjectsTarget", "VoID subjects");
            TestPredicate(triples, "http://rdfs.org/ns/void#objectsTarget", "VoID objects");
            TestPredicate(triples, "http://rdfs.org/ns/void#linkPredicate", "VoID predicates");
            TestPredicate(triples, "http://rdfs.org/ns/void#dataDump", "data dump location");
            TestPredicate(triples, "http://rdfs.org/ns/void#triples", "triple count");
            TestPredicate(triples, "http://www.ontologydesignpatterns.org/ont/dul/DUL.owl#expresses", "justification");
            TestPredicate(triples, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "subset type");
        }

        public IEnumerable<Triple> TriplesByPredicate(IEnumerable<Triple> triples, string predicate)
        {
            return triples.Where(t => t.Predicate.ToString() == predicate);
        }

        public IEnumerable<Triple> TriplesByPredicate(Graph g, string predicate)
        {
            return g.Triples.Where(t => t.Predicate.ToString() == predicate);
        }

        public IEnumerable<Triple> TriplesBySubject(Graph g, string subject)
        {
            return g.Triples.Where(t => t.Subject.ToString() == subject);
        }
    }
}
