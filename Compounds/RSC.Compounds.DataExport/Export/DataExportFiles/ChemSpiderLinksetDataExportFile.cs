using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using RSC.Collections;
using RSC.Datasources;
using System.Threading;

namespace RSC.Compounds.DataExport
{
	/// <summary>
	/// Used for generating export files for linking OPS compounds to ChemSpider compounds for a particular Data Source.
	/// </summary>
	public class ChemSpiderLinksetDataExportFile : DataSourceDataExportFile
	{
		private static readonly int chunkSize = 1000;

		/// <summary>
		/// Call the base constructor.
		/// </summary>
		public ChemSpiderLinksetDataExportFile(IDataSourceExport exp)
			: base(exp)
		{
			FileName = String.Format("LINKSET_EXACT_OPS_CHEMSPIDER_{0}{1}.ttl", ( exp as IDataSourceExport ).DsnLabel, exp.ExportDate.ToString("yyyyMMdd"));
		}

		/// <summary>
		/// Exports an Rdf Linkset containing Exact Matches.
		/// </summary>
		public override void Export(IDataExport exp, Encoding encoding)
		{
			base.Export(exp, encoding);

			//Get a copy of the linkset prefixes.
			var prefixes = new Dictionary<string, Uri>(TurtlePrefixSets.Linkset);

			//Perform the full export of exact matches.

			//Write the linkset containing skos:exactMatch References.
			using ( TextWriter ttl = new StreamWriter(FileFullPath, false, encoding) )
			using ( TextWriter w = TextWriter.Synchronized(ttl) ) {
				//Get the linkset uri.
				var linksetUri = new Uri(String.Format("{0}/{1}/{2}", exp.DownloadUrl.TrimEnd('/'), exp.ExportDirectory, FileName));

				//Add the prefix for this void file with the empty prefix.
				prefixes.Add("", new Uri(String.Format("{0}#", linksetUri.ToString())));

				//Output the prefixes for the linkset.
				foreach ( var p in prefixes )
					w.WriteLine("@prefix {0}: <{1}> .", p.Key, p.Value);

				//Add the predicate to describe which subset this dataset is in.
				var subsetObject = string.Format("{0}_{1}_{2}", ( exp as IDataSourceExport ).DsnLabel.ToLower(), "ops_chemspider", SkosPredicateExtensions.SKOS_EXACT_MATCH); //E.g. :drugbank_ops_chemspider_exactMatch
				w.WriteLine(Turtle.VoidInDatasetLine(( exp as IDataSourceExport ).DataSource.Name, subsetObject, linksetUri.ToString(), exp.DownloadUrl, exp.ExportDate));

				exp.CompoundsStore
					.GetDataSourceCompoundIds(( exp as IDataSourceExport ).DataSource.Guid)
					.ToChunks(chunkSize)
					.AsParallel()
					.ForAll(compoundIdsChunk => {
						foreach ( var compound in exp.CompoundsStore.GetCompounds(compoundIdsChunk, new[] { "Id", "ExternalReferences" }) ) {
							if ( compound.ExternalReferences != null ) {
								var opsIdentifier = compound.ExternalReferences.FirstOrDefault(e => e.Type.UriSpace == Constants.OPSUriSpace);
								var csIdentifier = compound.ExternalReferences.FirstOrDefault(e => e.Type.UriSpace == Constants.CSUriSpace);
								if ( opsIdentifier != null && ( csIdentifier != null && ( csIdentifier.Value != null && opsIdentifier.Value != null ) ) ) {
									Interlocked.Increment(ref recordCount);
									w.WriteLine("<{0}> skos:exactMatch <{1}> .", opsIdentifier.ToOpsUri(), csIdentifier.ToOpsUri());
								}
							}
						}
					});
			}
		}
	}
}
