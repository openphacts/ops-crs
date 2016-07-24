using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.DataExport
{
	public static class CompoundExtensions
	{
		public static string ToChemSpiderLinksetLine(this Compound compound)
		{
			if (compound.ExternalReferences != null)
			{
				//Ensure we have OpsId and CsId in the ExternalIdentifiers.
				var opsIdentifier = compound.ExternalReferences.FirstOrDefault(e => e.Type.UriSpace == Constants.OPSUriSpace);
				var csIdentifier = compound.ExternalReferences.FirstOrDefault(e => e.Type.UriSpace == Constants.CSUriSpace);

				if (opsIdentifier != null
					&& (csIdentifier != null
					&& (csIdentifier.Value != null && opsIdentifier.Value != null)))
				{
					//Use the full uris from the ExternalIdentifiers collection.
					return String.Format("<{0}> {1} <{2}> .", opsIdentifier.ToOpsUri(), "skos:exactMatch",
					csIdentifier.ToOpsUri());
				}
			}
			return null;
		}

		/// <summary>
		/// Our turtle generator puts as much as it can on one line.
		/// </summary>
		/// <param name="compound"></param>
		/// <returns>a predicate/object pair if there is a CSID, otherwise an empty string</returns>
		public static string ToPredicateObjects(this Compound compound)
		{
			string result = "";
			Dictionary<string, string> predObjs = new Dictionary<string, string>()
			{
				{ Turtle.cheminf_stdInchi104, compound.StandardInChI.Inchi },
				{ Turtle.cheminf_stdInchiKey104, compound.StandardInChI.InChIKey },
			};
			if (compound.Smiles != null)
			{
				predObjs.Add(Turtle.cheminf_smiles, compound.Smiles.IndigoSmiles.RdfEncode());
			}
			// Retrieve the CSID from ExternalReferences, if there is one.
			// Treat CSIDs differently.
			var csIdentifier = compound.ExternalReferences.FirstOrDefault(e => e.Type.UriSpace == Constants.CSUriSpace);
			if (csIdentifier != null)
			{
				result = "cheminf:" + Turtle.cheminf_csid + " " + csIdentifier.Value + "; ";
			}
			return result + String.Join("; ", predObjs.Select(p => String.Format("cheminf:{0} \"{1}\"", p.Key, p.Value))) + "; ";
		}
	}
}
