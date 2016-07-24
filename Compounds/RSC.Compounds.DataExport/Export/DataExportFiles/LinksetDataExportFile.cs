using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using RSC.Collections;
using RSC.Datasources;
using System;
using System.Threading;

namespace RSC.Compounds.DataExport
{
	/// <summary>
	/// Used for generating export files for the Exact Match Linkset data export file.
	/// </summary>
	public class LinksetDataExportFile : DataSourceDataExportFile
	{
		private static readonly int chunkSize = 1000;

		protected virtual SkosPredicate SkosPredicate { get; set; }

		/// <summary>
		/// Call the base constructor.
		/// </summary>
		public LinksetDataExportFile(IDataSourceExport exp)
			: base(exp)
		{

		}

		/// <summary>
		/// Exports an Rdf Linkset containing Exact Matches.
		/// </summary>
		public override void Export(IDataExport exp, Encoding encoding)
		{
			base.Export(exp, encoding);

			//Get the current Data Version.
			var version = exp.DataExportStore.GetCurrentDataVersion((exp as IDataSourceExport).DataSource.Guid);

			//Get a copy of the linkset prefixes.
			var prefixes = new Dictionary<string, Uri>(TurtlePrefixSets.Linkset);
			var dbAlias = string.Empty;
			var useFullUri = true; //Always use the full uri.

			var linksetUri = string.Format("{0}/{1}/{2}", exp.DownloadUrl.TrimEnd('/'), exp.ExportDirectory, FileName);

			//Add the base uri prefix.
			prefixes.Add(Turtle.RDF_URI_PREFIX.ToLower(), new Uri(Turtle.RDF_URI.ToString() + "/"));  //Re-use the prefix as the alias.

			//Add the prefix for this void file with the empty prefix.
			prefixes.Add("", new Uri(String.Format("{0}#", linksetUri)));

			//Write the linkset containing skos:exactMatch References.
			using ( TextWriter ttl = new StreamWriter(FileFullPath, false, encoding) )
			using ( TextWriter w = TextWriter.Synchronized(ttl) ) {
				foreach ( var p in prefixes )
					w.WriteLine(string.Format("@prefix {0}: <{1}> .", p.Key, p.Value));

				//Add the predicate to describe which subset this dataset is in.
				var subsetObject = string.Format("{0}_{1}", ( exp as IDataSourceExport ).DsnLabel.ToLower(), SkosPredicate.ToName()); //E.g. :chebi_exactMatch
				w.WriteLine(Turtle.VoidInDatasetLine(( exp as IDataSourceExport ).DataSource.Name, subsetObject, linksetUri, exp.DownloadUrl, exp.ExportDate));

				exp.CompoundsStore
					.GetDataSourceRevisionIds(( exp as IDataSourceExport ).DataSource.Guid)
					.ToChunks(chunkSize)
					.AsParallel()
					.ForAll(revisionIdsChunk => {
						var revisions = exp.CompoundsStore.GetRevisions(revisionIdsChunk, new[] { "Substance", "CompoundId" });
						var compounds = exp.CompoundsStore.GetCompounds(revisions.Select(r => r.CompoundId).ToList(), new[] { "ExternalReferences", "Id" });

						foreach ( var revision in revisions ) {
							if ( Turtle.UseSkosExactMatchForId(( exp as IDataSourceExport ).DataSource.Name, revision.Substance.ExternalIdentifier) ) {
								var opsIdentifier = compounds
									.FirstOrDefault(c => c.Id == revision.CompoundId)
									.ExternalReferences
									.FirstOrDefault(e => e.Type.UriSpace == Constants.OPSUriSpace);

								if ( opsIdentifier != null ) {
										w.WriteLine("<{0}> {1}:{2} {3} ."
											, opsIdentifier.ToOpsUri()                         //0
											, SkosPredicateExtensions.SKOS_PREFIX                            //1
											, SkosPredicateExtensions.ToName(SkosPredicate)   //2
											, Turtle.GetDsnUri(( exp as IDataSourceExport ).DataSource.Name, revision.Substance.ExternalIdentifier, dbAlias, version.UriSpace, useFullUri));                                 //3
										Interlocked.Increment(ref recordCount);
								}
							}
						}

						Console.Out.Write(".");
					});
			}
		}
	}
}
