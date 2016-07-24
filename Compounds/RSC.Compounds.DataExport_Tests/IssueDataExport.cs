using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSC.Compounds.DataExport;
using RSC.Datasources;
using RSC.Logging;
using RSC.Properties;

namespace RSC.Compounds.DataExport_Tests
{
    [TestClass]
    public class IssueDataExport : DataExportTestBase
    {
        [TestMethod]
        public void IssueDataExport_IssueLines()
        {
			var export = new OpsDataSourceExport(dsGuid, DateTime.Now);
            DataSource ds = dataSourcesClient.GetDataSource(dsGuid);
            IssuesDataExportFile idef = new IssuesDataExportFile(export);
            DataVersion version = dataExport.DataExportStore.GetCurrentDataVersion(ds.Guid);
            Assert.IsNotNull(version, "current data version should not be null");
            Assert.IsNotNull(version.UriSpace, "uri space should not be null");
            Revision revision = revisions.First();
            List<LogCategory> logCategories = new List<LogCategory>()
            {
                new LogCategory() { ShortName = ("vld.fmt"), Id = new Guid("33333333-3333-3333-3333-333333333333") }
            };
            List<LogEntry> logEntries = new List<LogEntry>() { 
                new LogEntry() { 
                    Message = "this is a test which is a test",
                    TypeId = new Guid("44444444-4444-4444-4444-444444444444"),
                    Severity = Severity.Error, 
                    ObjectId = new RSC.ExternalId() { DomainId = 1, ObjectId = revision.Id }
                }};
            List<LogEntryType> logEntryTypes = new List<LogEntryType>()
            {
                new LogEntryType() {
                    Title = "Reverse polarity",
                    CategoryId = new Guid("33333333-3333-3333-3333-333333333333"),
                    Id = new Guid("44444444-4444-4444-4444-444444444444")
                }
            };
            Func<string,string> toUri = s => Turtle.GetDsnUri("test", s, "", version.UriSpace, true);
            // var result = idef.IssueLines(revision, logEntryTypes, logEntries, logCategories, toUri);
			IEnumerable<string> result = new List<string>();
            Assert.IsTrue(result.Any(), "there should be some issue lines");
            foreach (string line in result) Console.WriteLine(line);
            string first = result.First();
            Assert.IsTrue(first.Contains("\"Reverse polarity; this is a test which is a test"));
            Assert.IsTrue(first.StartsWith("<http://www.rsc.org/CHEBI_12345>"));
        }

        public LogEntry MockLogEntry(Severity severity, string shortname)
        {
            return new LogEntry() { Severity = severity, CategoryId = categoryGuids[shortname] };
        }

        public Dictionary<string, Guid> categoryGuids = new Dictionary<string,Guid>();
        public List<LogCategory> logCategories;

        public string TestIssuePredicate(Severity severity, string shortname)
        {
            return MockLogEntry(severity, shortname).GetIssuePredicate(new LogEntryType()
            {
                CategoryId = categoryGuids[shortname]
            }, logCategories);
        }

        [TestMethod]
        public void IssueDataExport_AnnotationPredicates()
        {
            categoryGuids.Add("vld.fmt", Guid.NewGuid());
            categoryGuids.Add("prc.chm", Guid.NewGuid());
            categoryGuids.Add("prc.pred", Guid.NewGuid());
            categoryGuids.Add("indigo", Guid.NewGuid());
            categoryGuids.Add("vld.chm", Guid.NewGuid());
            categoryGuids.Add("std.charge", Guid.NewGuid());
            categoryGuids.Add("std.chm", Guid.NewGuid());
            categoryGuids.Add("std.layout", Guid.NewGuid());
            logCategories = categoryGuids.Select(p => new LogCategory() { ShortName = p.Key, Id = p.Value }).ToList();
            // parsing
            Assert.AreEqual("cheminf:CHEMINF_000556", TestIssuePredicate(Severity.Error, "vld.fmt"));
            
            Assert.AreEqual("cheminf:CHEMINF_000556", TestIssuePredicate(Severity.Fatal, "vld.fmt"));
            Assert.AreEqual("cheminf:CHEMINF_000555", TestIssuePredicate(Severity.Warning, "vld.fmt"));
            Assert.AreEqual("cheminf:CHEMINF_000558", TestIssuePredicate(Severity.Information,"vld.fmt"));
            // processing
            Assert.AreEqual("cheminf:CHEMINF_000507", TestIssuePredicate(Severity.Error, "prc.chm"));
            Assert.AreEqual("cheminf:CHEMINF_000507", TestIssuePredicate(Severity.Fatal, "prc.chm"));
            Assert.AreEqual("cheminf:CHEMINF_000506", TestIssuePredicate(Severity.Warning, "prc.chm"));
            Assert.AreEqual("cheminf:CHEMINF_000557", TestIssuePredicate(Severity.Information,"prc.chm"));
            Assert.AreEqual("cheminf:CHEMINF_000507", TestIssuePredicate(Severity.Error, "prc.pred"));
            Assert.AreEqual("cheminf:CHEMINF_000507", TestIssuePredicate(Severity.Fatal, "prc.pred"));
            Assert.AreEqual("cheminf:CHEMINF_000506", TestIssuePredicate(Severity.Warning, "prc.pred"));
            Assert.AreEqual("cheminf:CHEMINF_000557", TestIssuePredicate(Severity.Information, "prc.pred"));
            // validation
            Assert.AreEqual("cheminf:CHEMINF_000426", TestIssuePredicate(Severity.Error, "indigo"));
            Assert.AreEqual("cheminf:CHEMINF_000426", TestIssuePredicate(Severity.Fatal,"indigo"));
            Assert.AreEqual("cheminf:CHEMINF_000425", TestIssuePredicate(Severity.Warning, "indigo"));
            Assert.AreEqual("cheminf:CHEMINF_000560", TestIssuePredicate(Severity.Information, "indigo"));
            Assert.AreEqual("cheminf:CHEMINF_000426", TestIssuePredicate(Severity.Error, "vld.chm"));
            Assert.AreEqual("cheminf:CHEMINF_000426", TestIssuePredicate(Severity.Fatal, "vld.chm"));
            Assert.AreEqual("cheminf:CHEMINF_000425", TestIssuePredicate(Severity.Warning,"vld.chm"));
            Assert.AreEqual("cheminf:CHEMINF_000560", TestIssuePredicate(Severity.Information, "vld.chm"));
            // standardization
            Assert.AreEqual("cheminf:CHEMINF_000553", TestIssuePredicate(Severity.Error, "std.charge"));
            Assert.AreEqual("cheminf:CHEMINF_000553", TestIssuePredicate(Severity.Fatal, "std.charge"));
            Assert.AreEqual("cheminf:CHEMINF_000554", TestIssuePredicate(Severity.Warning, "std.charge"));
            Assert.AreEqual("cheminf:CHEMINF_000559", TestIssuePredicate(Severity.Information, "std.charge"));
            Assert.AreEqual("cheminf:CHEMINF_000553", TestIssuePredicate(Severity.Error, "std.chm"));
            Assert.AreEqual("cheminf:CHEMINF_000553", TestIssuePredicate(Severity.Fatal, "std.chm"));
            Assert.AreEqual("cheminf:CHEMINF_000554", TestIssuePredicate(Severity.Warning, "std.chm"));
            Assert.AreEqual("cheminf:CHEMINF_000559", TestIssuePredicate(Severity.Information, "std.chm"));
            Assert.AreEqual("cheminf:CHEMINF_000553", TestIssuePredicate(Severity.Error, "std.layout"));
            Assert.AreEqual("cheminf:CHEMINF_000553", TestIssuePredicate(Severity.Fatal,"std.layout"));
            Assert.AreEqual("cheminf:CHEMINF_000554", TestIssuePredicate(Severity.Warning, "std.layout"));
            Assert.AreEqual("cheminf:CHEMINF_000559", TestIssuePredicate(Severity.Information, "std.layout"));
        }

        [TestMethod]
        public void IssueDataExport_PrefixLines()
        {
            DataSource ds = new DataSource();
			var export = new OpsDataSourceExport(dsGuid, DateTime.Now);
            IssuesDataExportFile sdef = new IssuesDataExportFile(export);
            var result = sdef.PrefixLines("ftp://ftp.rsc.org/dummyproperties", TurtlePrefixSets.Properties).ToList();
            Assert.IsTrue(result.Any(), "no prefixes produced");
            foreach (string line in result)
            {
                Assert.IsTrue(line.StartsWith("@prefix"), "malformed line: " + line);
                Assert.IsTrue(line.Contains(": <"), "malformed line: " + line);
                Assert.IsTrue(line.EndsWith("> ."), "malformed line: " + line);
            }
        }

        [TestMethod]
        public void IssueDataExport_AnnotationProperties()
        {
            DataSource ds = new DataSource();
			var export = new OpsDataSourceExport(dsGuid, DateTime.Now);
            IssuesDataExportFile sdef = new IssuesDataExportFile(export);
            var result = sdef.AnnotationPropertyLines().ToList();
            foreach (string line in result)
            {
                Console.WriteLine(line);
                Assert.IsTrue(line.Contains(" . # "), "no explanation of what is going on in: " + line);
                Assert.IsTrue(line.Contains(" a "), "no predicate in: " + line);
                Assert.IsTrue(line.Contains("owl:AnnotationProperty"), "no assertion that this is an annotation property: " + line);
            }
        }

        [TestMethod]
        public void IssueDataExport_IssuesDataExportFile()
        {
            DataSource ds = new DataSource();
			var export = new OpsDataSourceExport(dsGuid, DateTime.Now);
            IssuesDataExportFile sdef = new IssuesDataExportFile(export);
            // var result = sdef.ExportLines(dataExport);
			IEnumerable<string> result = new List<string>();
            Assert.IsTrue(result.Any());
            foreach (string line in result)
                Console.WriteLine(line);
        }
    }
}
