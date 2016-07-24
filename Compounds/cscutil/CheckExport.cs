using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace cscutil
{
    /// <summary>
    /// May need a better name.
    /// 
    /// This is roughly based on the Assert.* methods in the default .NET testing framework except that
    /// instead of being void methods they return a string if something is wrong or null otherwise.
    /// </summary>
    public static class Report
    {
        public static string AllTrue<T>(Func<T, bool> fn, IEnumerable<T> list, string message)
        {
            var fails = list.Where(t => !fn(t)).ToList();
            if (fails.Any()) return message + ". Exceptions: " + string.Join("; ", fails);
            return null;
        }

        public static string AreEqual<T>(T t1, T t2, string message)
        {
            if (!t1.Equals(t2)) return message + " should be = " + t1 + " actual = " + t2;
            return null;
        }

        public static string AreNotEqual<T>(T t1, T t2, string message)
        {
            if (t1.Equals(t2)) return message + " equal. Should differ. Both are " + t1;
            return null;
        }

        public static string ExactlyOneTrue<T>(Func<T, bool> fn, IEnumerable<T> list, string message)
        {
            var matches = list.Where(fn);
            var count = matches.Count();
            if (count == 0) return message + ": none found. Should be exactly one.";
            if (count > 1) return message + ": too many found: " + count + ". Should be exactly one.";
            return null;
        }
    }

    public static class GraphReport
    {
        public static string Any(IGraph g, INode subject, INode predicate, string message)
        {
            if (!g.GetTriplesWithSubjectPredicate(subject, predicate).Any())
                return "no triples: " + message;
            return null;
        }

        public static string ExactlyOne(IGraph g, INode subject, INode predicate, string message)
        {
            var count = g.GetTriplesWithSubjectPredicate(subject, predicate).Count();
            if (count != 1) return message + ": should be exactly 1, is " + count;
            return null;
        }
    }

    public static class CheckExport
    {
        public static List<string> Turtleprefixes = new List<string>
        {
            "ISSUES_", "PROPERTIES_", "SYNONYMS_",
            "LINKSET_CLOSE_PARENT_CHILD_CHARGE_INSENSITIVE_PARENT_",
            "LINKSET_CLOSE_PARENT_CHILD_ISOTOPE_INSENSITIVE_PARENT_",
            "LINKSET_CLOSE_PARENT_CHILD_STEREO_INSENSITIVE_PARENT_",
            "LINKSET_CLOSE_PARENT_CHILD_SUPER_INSENSITIVE_PARENT_",
            "LINKSET_CLOSE_PARENT_CHILD_TAUTOMER_INSENSITIVE_PARENT_",
            "LINKSET_EXACT_OPS_CHEMSPIDER_","LINKSET_RELATED_PARENT_CHILD_FRAGMENT_"
        };

        /// <summary>
        /// there should only be one of each of these in each linkset
        /// </summary>
        public static List<string> LinksetSinglePredicates = new List<string>
        {
            "dcterms:description", "dcterms:license", "dcterms:title", "dul:expresses",
            "pav:authoredBy", "pav:authoredOn", "pav:createdBy", "pav:createdOn", "pav:createdWith",
            "void:linkPredicate", "void:objectsTarget", "void:subjectsTarget"
        };

        public static List<string> SubsetSinglePredicates = new List<string>
        {
            "dcterms:description", "dcterms:title", "foaf:page", "prov:wasDerivedFrom"
        };

        /// <summary>
        /// assume that we know already that these triples belong to a dataset
        /// </summary>
        public static List<string> ValidateLinkset(IGraph g, IUriNode subject, string foldername, string baseFolder)
        {
            // special checks
            var triplesPredicate = g.CreateUriNode("void:triples");
            var dumpPredicate = g.CreateUriNode("void:dataDump");
            var firsttriple = g.GetTriplesWithSubjectPredicate(subject, dumpPredicate).First();
            var linkset = firsttriple.Subject;
            var dumplocation = firsttriple.Object.ToString();
            var result = new List<string> { dumplocation };
            result.AddRange(CheckExport.LinksetSinglePredicates.Select(p => CheckExport.CheckForExactlyOneTriple(g, subject, p)));

            var expectedcount = Convert.ToInt32(((ILiteralNode)g.GetTriplesWithSubjectPredicate(linkset, triplesPredicate).First().Object).Value);
            result.Add("*INFORMATION* " + expectedcount + " triples expected in file " + dumplocation);
            var linksetlocation = dumplocation.Replace(baseFolder, foldername);
            if (File.Exists(linksetlocation))
            {
                var actualcount = CheckExport.CountTriples(dumplocation.Replace(baseFolder, foldername));
                if (actualcount == -1) result.Add("*CRITICAL ERROR* file " + dumplocation + " did not validate");
                else if (actualcount != expectedcount) result.Add("*ERROR*" + actualcount + " triples actually found in file");
            }
            else
                result.Add("*CRITICAL ERROR* linkset not found at: " + linksetlocation);
            return result;
        }

        public static string CheckForExactlyOneTriple(IGraph g, IUriNode subject, string predicate)
        {
            return GraphReport.ExactlyOne(g, subject, g.CreateUriNode(predicate), predicate);
        }

        /// <summary>
        /// assume that we know already that these triples belong to a dataset
        /// </summary>
        public static List<string> ValidateSubset(IGraph g, IUriNode subject, string foldername, string basefolder)
        {
            var dumpPredicate = g.CreateUriNode("void:dataDump");
            var subsetPredicate = g.CreateUriNode("void:subset");
            var result = new List<string> { subject.ToString() };
            result.AddRange(CheckExport.SubsetSinglePredicates.Select(p => CheckExport.CheckForExactlyOneTriple(g, subject, p)));
            result.Add(Report.AreEqual(3, g.GetTriplesWithSubjectPredicate(subject, dumpPredicate).Count(),
                "number of dataDumps per dataset"));
            result.Add(Report.AreEqual(9, g.GetTriplesWithSubjectPredicate(subject, subsetPredicate).Count(),
                "*CRITICAL ERROR* number of subsets per dataset wrong "));
            return result;
        }

        public static int CountTriples(string filename)
        {
            UriFactory.Clear();
            try
            {
                var handler = new CountHandler();

                if (filename.EndsWith(".gz", StringComparison.Ordinal))
                {
                    var ttlparser = new GZippedTurtleParser();
                    ttlparser.Load(handler, filename);
                }
                else
                {
                    var ttlparser = new TurtleParser();
                    ttlparser.Load(handler, filename);
                }
                return handler.Count;

            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return -1;
            }
        }

        /// <summary>
        /// Does streaming.
        /// </summary>
        public static string ValidateRdf(string filename)
        {
            // we don't load the whole thing into memory or everything dies.
            try
            {
                CheckExport.CountTriples(filename);
            }
            catch (Exception e)
            {
                return filename + ": validation error" + e.Message;
            }
            return filename + ": rdf validated ok";
        }

        public static List<string> DatasetSinglePredicates = new List<string>
        {
            "dcterms:created", "dcterms:description", "dcterms:license", "dcterms:modified", "dcterms:publisher",
            "dcterms:subject", "dcterms:title", "foaf:homepage", "voag:frequencyOfChange", "void:exampleResource",
            "void:feature", "void:uriSpace"
        };

        public static List<string> DatasetDescriptionSinglePredicates = new List<string>
        {
            "dcterms:description", "dcterms:title", "foaf:primaryTopic",
            "pav:createdBy", "pav:createdOn", "pav:lastUpdateOn", "pav:previousVersion"
        };

        public static IEnumerable<string> ValidateDatasetDescription(IGraph g, IUriNode subject)
        {
            var result = new List<string>();
            result.AddRange(CheckExport.DatasetDescriptionSinglePredicates.Select(p => CheckExport.CheckForExactlyOneTriple(g, subject, p)));
            var previousVersion = g.CreateUriNode("pav:previousVersion");
            var previous = g.GetTriplesWithSubjectPredicate(subject, previousVersion).First().Object;

            result.Add(Report.AreNotEqual(subject.ToString(), previous.ToString(), "*CRITICAL ERROR*. Subject and Object for pav:previousVersion "));
            return result;
        }

        public static IEnumerable<string> ValidateDatasetAsGraph(IGraph g, IUriNode subject)
        {
            var result = new List<string>();
            result.AddRange(CheckExport.DatasetSinglePredicates.Select(p => CheckExport.CheckForExactlyOneTriple(g, subject, p)));
            result.Add(GraphReport.Any(g, subject, g.CreateUriNode("void:subset"), "void subset"));
            var vocabularyCount = g.GetTriplesWithSubjectPredicate(subject, g.CreateUriNode("void:vocabulary")).Count();
            result.Add(Report.AreEqual(15, vocabularyCount, "void vocabulary count"));
            return result;
        }

        /// <summary>
        /// In principle foldername and location ought to be the same.
        /// </summary>
        /// <param name="foldername">Where to look on the local disc for everything.</param>
        /// <param name="location"></param>
        public static void ValidateVoidAgainstFolder(string foldername, string location)
        {
            var filename = Directory.GetFiles(foldername).First(f =>
            {
                var fileName = Path.GetFileName(f);
                return (fileName != null) && fileName.ToUpper().StartsWith("VOID", StringComparison.Ordinal);
            });
            IGraph voidGraph = new Graph();
            if (filename.EndsWith(".gz", StringComparison.Ordinal))
            {
                var voidParser = new GZippedTurtleParser();
                voidParser.Load(voidGraph, filename);
            }
            else
            {
                var voidParser = new TurtleParser();
                voidParser.Load(voidGraph, filename); // basically it's OK to die here if the VoID is bad
            }
            var rdfType = voidGraph.CreateUriNode("rdf:type");
            var dataset = voidGraph.CreateUriNode("void:Dataset");
            var datasetdescclass = voidGraph.CreateUriNode("void:DatasetDescription");
            var linkset = voidGraph.CreateUriNode("void:Linkset");
            var datasets = voidGraph.GetTriplesWithPredicateObject(rdfType, dataset).Select(t => t.Subject).ToList();
            var linksets = voidGraph.GetTriplesWithPredicateObject(rdfType, linkset).Select(t => t.Subject).ToList();
            var datasetdesc = voidGraph.GetTriplesWithPredicateObject(rdfType, datasetdescclass).First().Subject;
            var result = new List<string>();
            result.AddRange(CheckExport.ValidateDatasetDescription(voidGraph, (IUriNode)datasetdesc));
            result.Add("");
            result.Add("datasets");
            result.Add("");
            // separate rule here for the top-level dataset
            foreach (var d in datasets.Where(d => !d.ToString().Contains("openphactsDataset")).Cast<IUriNode>())
            {
                result.AddRange(CheckExport.ValidateSubset(voidGraph, d, foldername, location));
            }
            result.AddRange(CheckExport.ValidateDatasetAsGraph(voidGraph, (IUriNode)datasets.First(d => d.ToString().Contains("openphactsDataset"))));
            result.Add("");
            result.Add("linksets");
            result.Add("");
            foreach (var l in linksets.Cast<IUriNode>())
            {
                result.AddRange(CheckExport.ValidateLinkset(voidGraph, l, foldername, location));
            }
            foreach (var l in result.Where(l => l != null))
                Console.WriteLine(l);
        }

        private static string CheckStart(string start, IEnumerable<string> list)
        {
            return Report.ExactlyOneTrue(f => f.StartsWith(start, StringComparison.Ordinal), list, "Starting with " + start);
        }

        private static IEnumerable<string> CheckSubFolder(string foldername)
        {
            var result = new List<string>();
            var leaf = Path.GetFileName(foldername);
            var filenames = Directory.GetFiles(foldername).Select(Path.GetFileName).ToList();
            result.Add("Data source " + leaf);

            result.Add(Report.AllTrue(f => (leaf != null) && f.Contains(leaf), filenames, "Filename should match data source. "));
            result.AddRange(CheckExport.Turtleprefixes.Select(p => CheckExport.CheckStart(p,filenames)));
            if (result.Count(r => r != null) == 1) result.Add("no problems found");
            result.Add("");
            return result;
        }

        /// <summary>
        /// This is called directly from Program.cs.
        /// </summary>
        public static void AllFilesValidate(string foldername)
        {
            Options.InternUris = false;
            Console.WriteLine(@"folder name = {0}", foldername);
            var filenames = Directory.GetFiles(foldername, "*.ttl", SearchOption.AllDirectories).ToList();
            foreach (var lines in filenames.Select(CheckExport.CheckPrefixes))
                Console.WriteLine(string.Join(Environment.NewLine,lines));
            foreach (var l2 in filenames.Select(CheckExport.ValidateRdf))
                Console.WriteLine(l2);
        }

        public static IEnumerable<string> CheckPrefixes(string filename)
        {
            return from line in File.ReadLines(filename).Where(l => Regex.IsMatch(l, @"^@prefix")) where line.Contains("//>")
                   select string.Format("FATAL ERROR IN FILE {1}: line '{0}' defines prefix ending in double solidus.", line, filename);
        }

        public static void AllFilesPresent(string foldername)
        {
            var result = new List<string> { "folder name = " + foldername };
            var filenames = Directory.GetFiles(foldername, "*", SearchOption.AllDirectories).Select(Path.GetFileName).ToList();
            var subfolders = Directory.GetDirectories(foldername);
            var datedturtles = filenames.Where(f => Regex.IsMatch(f, @"\D\d{4}-?\d{2}-?\d{2}\.ttl$"))
                // we have two conventions for dates, one for the void and one for everything else
                // Not checking date on sdf for now as it's bonus data.
                .Select(f => f.Replace("-", "")).ToList();
            var firstdate = datedturtles.Any()
                ? datedturtles.First().Substring(datedturtles.First().Length - 12, 8)
                : "";
            result.Add(firstdate == "" ? "No dates found" : "First date = " + firstdate);
            result.Add(Report.AllTrue(f => f.Contains(firstdate), datedturtles, "All should have the same date"));
            result.Add(Report.ExactlyOneTrue(f => Regex.IsMatch(f, @"[Vv][Oo][Ii][Dd].*\.ttl"), filenames, "VoID check"));
            result.Add(Report.ExactlyOneTrue(f => f.StartsWith("OPS_CHEMSPIDER", StringComparison.Ordinal) && f.EndsWith(".sdf", StringComparison.Ordinal), filenames, "sd file check"));
            foreach (var subfolder in subfolders)
            {
                var datasourcename = Path.GetFileName(subfolder);
                result.AddRange(CheckExport.CheckSubFolder(subfolder));
                result.Add(Report.ExactlyOneTrue(f => f.Contains("LINKSET_EXACT_" + datasourcename), filenames, "Starting with LINKSET_EXACT_" + datasourcename));
            }
            foreach (var line in result.Where(r => r != null))
                Console.WriteLine(line);
        }

        /// <summary>
        /// The aim here is to provide an at-a-glance report on what has changed between two reports.
        /// </summary>
        public static void CompareExports(string folder1, string folder2)
        {
            var subfolders1 = Directory.GetDirectories(folder1).Select(Path.GetFileName).ToList();
            var subfolders2 = Directory.GetDirectories(folder2).Select(Path.GetFileName).ToList();
            var result = subfolders1.Except(subfolders2).Select(f => "data source folder " + f + " in export 1 but not in export 2").ToList();
            result.AddRange(subfolders2.Except(subfolders1).Select(f => "data source folder " + f + " in export 2 but not in export 1"));

            // now check intersection
            var shareddsns = subfolders1.Intersect(subfolders2).ToList();
            foreach (var dsn in shareddsns)
            {
                result.Add(dsn);
                var dsn1 = Path.Combine(folder1, dsn);
                var dsn2 = Path.Combine(folder2, dsn);
                var dsn1Files = Directory.GetFiles(dsn1);
                var dsn2Files = Directory.GetFiles(dsn2);
                foreach (var prefix in CheckExport.Turtleprefixes)
                {
                    var indsn1 = dsn1Files.Any(f => f.Contains(prefix));
                    var indsn2 = dsn2Files.Any(f => f.Contains(prefix));
                    if (!indsn1)
                        result.Add(prefix + " missing from export 1");
                    if (!indsn2)
                        result.Add(prefix + " missing from export 2");
                    if (!indsn1 || !indsn2)
                    {
                        continue;
                    }
                    result.Add(prefix + " in both");
                    result.AddRange(
                        CheckExport.CompareGraphs(
                            dsn1Files.First(f => f.Contains(prefix)),
                            dsn2Files.First(f => f.Contains(prefix))));
                }
            }
            foreach (var line in result.Where(r => r != null))
                Console.WriteLine(line);
        }

        /// <summary>
        /// Only take the first 100 lines of a comparison.
        /// </summary>
        /// <param name="filename1"></param>
        /// <param name="filename2"></param>
        /// <returns></returns>
        public static IEnumerable<string> CompareGraphs(string filename1, string filename2)
        {
            // this looks a bit nuts. I think there must be a better way of doing this.
            return File.ReadLines(filename1).Except(File.ReadLines(filename2))
                .Concat(File.ReadLines(filename2).Except(File.ReadLines(filename1))).Take(100);
        }
    }
}


