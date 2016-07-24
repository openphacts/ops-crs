using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System;
using RSC.Datasources;
using RSC.Properties;
using RSC.Collections;
using System.Threading;

namespace RSC.Compounds.DataExport
{
	/// <summary>
	/// Used for generating export files for the Synonyms RDF file.
	/// </summary>
	public class SynonymsDataExportFile : DataSourceDataExportFile
	{
		private static readonly int chunkSize = 500;

		/// <summary>
		/// Call the base constructor.
		/// </summary>
		public SynonymsDataExportFile(IDataSourceExport exp)
			: base(exp)
		{
			FileName = string.Format("SYNONYMS_{0}{1}.ttl", ( exp as IDataSourceExport ).DsnLabel, exp.ExportDate.ToString("yyyyMMdd"));
		}

		public IEnumerable<string> AnnotationPropertyLines()
		{
			//Add the Annotation Property type assignments.
			const string annotationProp = "cheminf:{0} a owl:AnnotationProperty . # {1}";
			yield return String.Format(annotationProp, Turtle.cheminf_validatedSynonym, "Validated Synonym");
			yield return String.Format(annotationProp, Turtle.cheminf_unvalidatedSynonym, "Unvalidated Synonym");
			yield return String.Format(annotationProp, Turtle.cheminf_validatedDbid, "Validated Database Identifier");
			yield return String.Format(annotationProp, Turtle.cheminf_unvalidatedDbid, "Unvalidated Database Identifier");
			yield return String.Format(annotationProp, Turtle.cheminf_stdInchi104, "Standard InChI 1.04");
			yield return String.Format(annotationProp, Turtle.cheminf_stdInchiKey104, "Standard InChIKey 1.04");
			yield return String.Format(annotationProp, Turtle.cheminf_csid, "ChemSpider ID");
			yield return String.Format(annotationProp, Turtle.cheminf_title, "ChemSpider title");
			yield return String.Format(annotationProp, Turtle.cheminf_smiles, "SMILES");
			yield return String.Format(annotationProp, Turtle.cheminf_mf, "Molecular Formula");
		}

		/// <summary>
		/// Exports Rdf Synonyms: the full export.
		/// </summary>
		public override void Export(IDataExport exp, Encoding encoding)
		{
			base.Export(exp, encoding);

			//Write the Identifiers and Synonyms.
			using ( TextWriter ttl = new StreamWriter(FileFullPath, false, encoding) )
			using ( TextWriter w = TextWriter.Synchronized(ttl) )
			{
				//Get the synonyms file uri.
				var synonymsUri = string.Format("{0}/{1}/{2}", exp.DownloadUrl.TrimEnd('/'), exp.ExportDirectory, FileName);
				foreach ( string line in PrefixLines(synonymsUri, TurtlePrefixSets.Synonyms) )
					w.WriteLine(line);

				//Add the predicate to describe which dataset this is in.
				var datasetObject = string.Format("{0}-{1}", Turtle.DATASET_LABEL.ToLower(), ( exp as IDataSourceExport ).DsnLabel.ToLower()); //E.g. :chemspider-chebi
				w.WriteLine(Turtle.VoidInDatasetLine(( exp as IDataSourceExport ).DataSource.Name, datasetObject, synonymsUri, exp.DownloadUrl, exp.ExportDate));

				foreach ( string line in AnnotationPropertyLines() )
					w.WriteLine(line);

				exp.CompoundsStore
					.GetDataSourceRevisionIds(( exp as IDataSourceExport ).DataSource.Guid)
					.ToChunks(chunkSize)
					.AsParallel()
					.ForAll(revisionIdsChunk => {
						//Get the revisions as Properties are stored against revisions.
						var revisionsChunk = exp.CompoundsStore.GetRevisions(revisionIdsChunk, new[] { "Id", "CompoundId" });

						//Get the CompoundIds from the Revisions as we need OpsId from the 
						var compoundIdsChunk = revisionsChunk.Select(r => r.CompoundId).ToList();

						var compoundsChunk = exp.CompoundsStore.GetCompounds(compoundIdsChunk, new[] { "Id", "ExternalReferences", "CompoundSynonyms", "StandardInChI", "Smiles" });

						//Get the dictionary of Compound Ids and Properties for the chunk.
						var compoundRecordsProperties =
							exp.PropertiesStore.GetRecordsProperties(
								revisionsChunk.Select(r => new ExternalId() { DomainId = 1, ObjectId = r.Id }), new[] { PropertyName.MOLECULAR_FORMULA });

						// Write the synonym triples.
						foreach ( var compound in compoundsChunk ) {
							string mf = exp.CompoundsStore
								.GetRecordPropertyValue(compoundRecordsProperties,
									new ExternalId() { DomainId = 1, ObjectId = revisionsChunk.FirstOrDefault(r => r.CompoundId == compound.Id).Id },
									PropertyName.MOLECULAR_FORMULA);

							var opsIdentifier = compound.ExternalReferences.FirstOrDefault(e => e.Type.UriSpace == Constants.OPSUriSpace);
							if ( opsIdentifier != null ) {
								StringBuilder buf = new StringBuilder();
								buf.AppendFormat("<{0}> ", opsIdentifier.ToOpsUri());
								buf.Append(compound.ToPredicateObjects());
								buf.AppendFormat("{0}:{1} \"{2}\"; ", "cheminf", Turtle.cheminf_mf, mf);
								Interlocked.Add(ref recordCount, 6);

								if ( compound.Synonyms != null && !DataSourceExport.Limited ) {
									buf.Append(String.Join("; ", compound.Synonyms.Select(s => s.ToPredicateObject())));
									Interlocked.Add(ref recordCount, compound.Synonyms.Count());
								}

								w.WriteLine("{0} .", buf.ToString());
							}
						}

						Console.Out.Write(".");
					});
			}
		}
	}
}
