using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System;

using RSC.Collections;
using RSC.Datasources;
using RSC.Logging;
using System.Threading;

namespace RSC.Compounds.DataExport
{
	/// <summary>
	/// Export file for Issues.
	/// Used for generating Rdf files, containing Issues with a particular data sources compounds.
	/// </summary>
	public class IssuesDataExportFile : DataSourceDataExportFile
	{
		private static readonly int chunkSize = 1000;

		/// <summary>
		/// Call the base constructor.
		/// </summary>
		public IssuesDataExportFile(IDataSourceExport exp)
			: base(exp)
		{
			FileName = string.Format("ISSUES_{0}{1}.ttl", ( exp as IDataSourceExport ).DsnLabel, exp.ExportDate.ToString("yyyyMMdd"));
		}

		public IEnumerable<string> AnnotationPropertyLines()
		{
			const string annotationProp = "cheminf:{0} a owl:AnnotationProperty . # {1}";
			yield return string.Format(annotationProp, Turtle.cheminf_issueParsingInfo, "connection table interpretation information data item");
			yield return string.Format(annotationProp, Turtle.cheminf_issueParsingWarning, "connection table interpretation warning");
			yield return string.Format(annotationProp, Turtle.cheminf_issueParsingError, "connection table interpretation error");

			yield return string.Format(annotationProp, Turtle.cheminf_issueValidationInfo, "structural validation information data item");
			yield return string.Format(annotationProp, Turtle.cheminf_issueValidationWarning, "structural validation warning");
			yield return string.Format(annotationProp, Turtle.cheminf_issueValidationError, "structural validation error");

			yield return string.Format(annotationProp, Turtle.cheminf_issueProcessingInfo, "structural processing information data item");
			yield return string.Format(annotationProp, Turtle.cheminf_issueProcessingWarning, "structural processing warning");
			yield return string.Format(annotationProp, Turtle.cheminf_issueProcessingError, "structural processing error");

			yield return string.Format(annotationProp, Turtle.cheminf_issueStandardizationInfo, "structural standardization information data item");
			yield return string.Format(annotationProp, Turtle.cheminf_issueStandardizationWarning, "structural standardization warning");
			yield return string.Format(annotationProp, Turtle.cheminf_issueStandardizationError, "structural standardization error");

		}

		/// <summary>
		/// Exports Rdf Issues for the data source.
		/// </summary>
		public override void Export(IDataExport exp, Encoding encoding)
		{
			base.Export(exp, encoding);

			//Write the properties.
			using ( TextWriter ttl = new StreamWriter(FileFullPath, false, encoding) )
			using ( TextWriter w = TextWriter.Synchronized(ttl) ) {
				//Get the current Data Version.            
				var version = exp.DataExportStore.GetCurrentDataVersion(( exp as IDataSourceExport ).DataSource.Guid);
				var useFullUri = true; //Always use the full uri.
				//Get the DSN prefix for this data source.
				var dbAlias = string.Empty;

				//Get the issues file uri.
				var issuesUri = string.Format("{0}/{1}/{2}", exp.DownloadUrl.TrimEnd('/'), exp.ExportDirectory, FileName);

				foreach ( string line in PrefixLines(issuesUri, TurtlePrefixSets.Issues) )
					w.WriteLine(line);

				//Add the Annotation Property type assignments.
				foreach ( string line in AnnotationPropertyLines() )
					w.WriteLine(line);

				//Get all the entry types so we can look up things like the Severity.
				var logEntryTypes = exp.LogStore.GetLogEntryTypes();
				var logCategories = exp.LogStore.GetLogCategories();

				exp.CompoundsStore
					.GetDataSourceRevisionIds(( exp as IDataSourceExport ).DataSource.Guid)
					.ToChunks(chunkSize)
					.AsParallel()
					.ForAll(revisionIdsChunk => {
						//Get the revisions.
						var revisionChunk = exp.CompoundsStore.GetRevisions(revisionIdsChunk, new[] { "Id", "Issues", "Substance" });

						//Get the log entries based on the Issue Ids.
						var logEntries = exp.LogStore.GetLogEntries(revisionChunk.SelectMany(d => d.Issues).Select(i => i.Id));

						foreach ( var revision in revisionChunk ) {
							string dsnUri = Turtle.GetDsnUri(( exp as IDataSourceExport ).DataSource.Name, revision.Substance.ExternalIdentifier, dbAlias, version.UriSpace, useFullUri);
							//Get the issues for each revision (using Compound Id).
							foreach ( var issue in logEntries.Where(l => l.ObjectId == new RSC.ExternalId() { DomainId = 1, ObjectId = revision.Id }) ) {
								var logEntryType = logEntryTypes.Single(l => l.Id == issue.TypeId);
								var issuePredicate = issue.GetIssuePredicate(logEntryType, logCategories);

								//Add the issue to the export file.
								string s = String.Format("{0} {1} \"{2}{3}\"@en ."
									, dsnUri                                                                //0
									, issuePredicate                                                        //1
									, logEntryType.Title.RdfEncode()                           //2
									, string.IsNullOrEmpty(issue.Message)
										? string.Empty
										: string.Format("; {0}", issue.Message.RdfEncode()));  //3


								w.WriteLine(s);

								Interlocked.Increment(ref recordCount);
							}
						}

						Console.Out.Write(".");
					});
			}
		}
	}
}
