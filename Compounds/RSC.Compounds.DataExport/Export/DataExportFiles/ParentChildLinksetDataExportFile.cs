using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using RSC.Collections;
using RSC.Datasources;
using System.Threading;
using System.Diagnostics;

namespace RSC.Compounds.DataExport
{
	public class ParentChildLinksetDataExportFile : DataSourceDataExportFile
	{
		private static readonly int chunkSize = 1000;

		/// <summary>
		/// The id of the type of Parent we are referencing in this linkset file.
		/// </summary>
		public ParentChildRelationship Relationship { get; set; }

		/// <summary>
		/// A label used in filenames - replace spaces and dots and lower-case.
		/// </summary>
		public string Label { get { return Relationship.GetName().Replace(" ", "_").Replace(".", "_").ToLower(); } }

		/// <summary>
		/// Call the base constructor.
		/// </summary>
		public ParentChildLinksetDataExportFile(IDataSourceExport exp, ParentChildRelationship relationship)
			: base(exp)
		{
			Relationship = relationship;

			var filePart = Relationship.GetPredicate().ToName().Replace("Match", "").ToUpper();
			FileName = String.Format("LINKSET_{0}_PARENT_CHILD_{1}_{2}{3}.ttl", filePart, Label.ToUpper(), ( exp as IDataSourceExport ).DsnLabel, exp.ExportDate.ToString("yyyyMMdd"));
		}

		/// <summary>
		/// Exports an Rdf Parent Child Data Source Export File.
		/// </summary>
		public override void Export(IDataExport exp, Encoding encoding)
		{
			// Perform the full export of related matches.
			base.Export(exp, encoding);

			//Get the linkset and parent_child prefixes.
			var prefixes = new Dictionary<string, Uri>(TurtlePrefixSets.Linkset);
			foreach ( var p in TurtlePrefixSets.ParentChild )
				prefixes.Add(p.Key, p.Value);

			//Add the ops url to the prefixes.
			prefixes.Add(Turtle.RDF_URI_PREFIX.ToString().ToLower(), new Uri(Turtle.RDF_URI.ToString() + "/"));  //Re-use the prefix as the alias.

			//Get the linkset uri.
			var linksetUri = string.Format("{0}/{1}/{2}", exp.DownloadUrl.TrimEnd('/'), exp.ExportDirectory, FileName);

			//Add the prefix for this void file with the empty prefix.
			prefixes.Add("", new Uri(String.Format("{0}#", linksetUri)));

			//Write the Parent child turtle export.
			using ( TextWriter ttl = new StreamWriter(FileFullPath, false, encoding) )
			using ( TextWriter w = TextWriter.Synchronized(ttl) ) {
				//Output the prefixes.
				foreach ( var p in prefixes )
					w.WriteLine(string.Format("@prefix {0}: <{1}> .", p.Key, p.Value));

				//Add the predicate to describe which subset this dataset is in.
				var subsetObject = string.Format("{0}_parent_child_{1}_{2}", ( exp as IDataSourceExport ).DsnLabel.ToLower(), Label, Relationship.GetPredicate().ToName()); //E.g. :drugbank_parent_child_isotope_unsensitive_parent_exactMatch
				w.WriteLine(Turtle.VoidInDatasetLine(( exp as IDataSourceExport ).DataSource.Name, subsetObject, linksetUri, exp.DownloadUrl, exp.ExportDate));

				exp.CompoundsStore
					.GetDataSourceCompoundIds(( exp as IDataSourceExport ).DataSource.Guid)
					.ToChunks(chunkSize)
					.AsParallel()
					.ForAll(compoundIdsChunk => {
						var compoundsChunkParentAndChildren = exp.CompoundsStore.GetCompoundsParentChildren(compoundIdsChunk, Relationship);

						//Get the Compounds Ids of all Compounds, Parents and Children.
						var allCompoundIds = compoundsChunkParentAndChildren
							.Select(c => c.Parent.Id)
							.ToList()
							.Concat(compoundsChunkParentAndChildren.Select(c => c.Child.Id).ToList())
							.Concat(compoundIdsChunk);

						//Get the compounds as we need External References.
						var allCompoundsChunk = exp.CompoundsStore.GetCompounds(allCompoundIds, new[] { "ExternalReferences", "Id" });

						//Get the next chunk of parent children.
						foreach ( var parentChildren in compoundsChunkParentAndChildren ) {
							var parentOpsIdentifier = allCompoundsChunk
								.FirstOrDefault(c => c.Id == parentChildren.Parent.Id)
								.ExternalReferences
								.FirstOrDefault(e => e.Type.UriSpace == Constants.OPSUriSpace);

							var childOpsIdentifier = allCompoundsChunk
								.FirstOrDefault(c => c.Id == parentChildren.Child.Id)
								.ExternalReferences
								.FirstOrDefault(e => e.Type.UriSpace == Constants.OPSUriSpace);

							if ( parentOpsIdentifier != null && childOpsIdentifier != null ) {
								w.WriteLine("<{0}> {1} <{2}> ."
									, parentOpsIdentifier.ToOpsUri()                                   //0
									, string.Format("{0}:{1}", SkosPredicateExtensions.SKOS_PREFIX, Relationship.GetPredicate().ToName())  //1
									, childOpsIdentifier.ToOpsUri());                                  //2

								Interlocked.Increment(ref recordCount);
							}
						}

						Console.Out.Write(".");
					});
			}
		}
	}
}
