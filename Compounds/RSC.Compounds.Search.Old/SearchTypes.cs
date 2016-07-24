using System;
using System.Collections.Generic;
using System.Linq;
using ChemSpider.Data.Database;
using ChemSpider.Security;
using ChemSpider.Search;
using ChemSpider.Molecules;
using InChINet;
//using RSC.Compounds.Database;

using System.Text;
using ChemSpider;

namespace RSC.Compounds.Search.Old
{
	public class CSCExactStructureSearch : CSExactStructureSearch
	{
		public override string Description
		{
			get { return string.Format("Structure Search - Exact; Molecule: {0}", Options.Molecule); }
		}

		public CSCExactStructureSearch()
		{
			m_sqlProvider = new CSCSqlSearchProvider();
		}

		protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
		{
			bool bAdded = false;
			if (Options.Molecule != String.Empty)
			{
				visual.Add("Structure Search - Exact");

				string mol = Options.Molecule;
				if (!mol.Contains('\n'))
				{
					MoleculeObjects.Molecule molecule = MoleculeObjects.MoleculeFactory.FromSMILES(mol);
					mol = molecule.ct();

					//please leave this comments below
					// using MolUtils.SMILESToMol(mol) will cause some smiles to convert to molfiles with relative stereo, e.g. "OC[C@@H](O­)[C@@H](O)­C=O"
					//mol = MolUtils.SMILESToMol(mol); 
				}

				string[] inchi_info = InChIUtils.mol2inchiinfo(mol, InChIFlags.Standard);
				if ( inchi_info != null && !String.IsNullOrEmpty(inchi_info[0]) )
				{
					string[] inchi_layers = InChIUtils.getInChILayers(inchi_info[0]);
					if (inchi_layers == null)
						throw new ChemSpiderException("Unable to parse InChI: {0}\nOriginal MOL: {1}", inchi_info[0], mol);

					string[] crs_inchi;

					switch (Options.MatchType)
					{
						case ExactStructureSearchOptions.EMatchType.ExactMatch:
							tables.Add("inchi ni");
							predicates.Add("c.inchi_id_fixedh = ni.inc_id");
							crs_inchi = InChIUtils.mol2inchiinfo(mol, InChINet.InChIFlags.CRS);
							predicates.Add(String.Format("ni.inchi_key = '{0}'", crs_inchi[1]));
							break;
						case ExactStructureSearchOptions.EMatchType.AllTautomers:
							crs_inchi = InChIUtils.mol2inchiinfo(mol, InChINet.InChIFlags.CRS_Tautomer);
							visual.Add("All Tautomers");
							tables.Add("inchi ti");
							predicates.Add("c.inchi_id_taut = ti.inc_id");
							predicates.Add(String.Format("ti.inchi_key = '{0}'", crs_inchi[1]));
							break;
						case ExactStructureSearchOptions.EMatchType.SameSkeletonIncludingH:
							visual.Add("Same Skeleton (Including H)");
							tables.Add("inchi_md5 imd5");
							predicates.Add("c.inc_id = imd5.inc_id");
							predicates.Add("imd5.inchi_ch_md5 = HashBytes('md5', cast('" + inchi_layers[(int)InChILayers.CH] + "' as nvarchar(max)))");
							break;
						case ExactStructureSearchOptions.EMatchType.SameSkeletonExcludingH:
							visual.Add("Same Skeleton (Excluding H)");
							tables.Add("inchi_md5 imd5");
							predicates.Add("c.inc_id = imd5.inc_id");
							predicates.Add("imd5.inchi_c_md5 = HashBytes('md5', cast('" + inchi_layers[(int)InChILayers.C] + "' as nvarchar(max)))");
							break;
						case ExactStructureSearchOptions.EMatchType.AllIsomers:
							visual.Add("All Isomers");
							tables.Add("inchi_md5 imd5");
							predicates.Add("c.inc_id = imd5.inc_id");
							predicates.Add("imd5.inchi_mf_md5 = HashBytes('md5', cast('" + inchi_layers[(int)InChILayers.MF] + "' as nvarchar(max)))");
							break;
					}
					bAdded = true;
				}
			}

			return bAdded;
		}
	}

	public class CSCSubstructureSearch : CSSubstructureSearch
	{
		public CSCSubstructureSearch()
		{
			m_sqlProvider = new CSCSqlSearchProvider();
		}

		public override string Description
		{
			get { return string.Format("Structure Search - Substructure; Molecule: {0}", Options.Molecule); }
		}

		protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
		{
			bool bAdded = false;
			if (Options.Molecule != String.Empty)
			{
				visual.Add("Structure Search - Substructure");

				if (Options.MolType == SubstructureSearchOptions.EMolType.SMILES)
				{
					tables.Add(string.Format("join bingo.SearchSub('compounds_sss', '{0}', '{1}') bingo on c.csid = bingo.id", Options.Molecule, ResultOptions != null && ResultOptions.Limit > 0 ? string.Format("TOP {0};", ResultOptions.Limit) : ""));
				}
				else if (Options.MolType == SubstructureSearchOptions.EMolType.SMARTS)
				{
					tables.Add(string.Format("join bingo.SearchSMARTS('compounds_sss', '{0}', '{1}') bingo on c.csid = bingo.id", Options.Molecule, ResultOptions != null && ResultOptions.Limit > 0 ? string.Format("TOP {0};", ResultOptions.Limit) : ""));
				}

				columns.Add(string.Format("Bingo.Sim(smiles, '{0}', 'tanimoto') as similarity", Options.Molecule));
				tables.Add("join compounds_sss sss on sss.csid = bingo.id ");

				orderby.Add("similarity desc"); 

				bAdded = true;
			}

			return bAdded;
		}

		protected override string GetCountSqlCommand(List<string> predicates, List<string> tables, List<string> columns, List<string> orderby)
		{
			if (Options.Molecule != String.Empty)
			{
				StringBuilder sb = new StringBuilder("select count(c.csid) from compounds c ");

				foreach (string tbl in tables)
				{
					if (tbl.Contains("bingo.Search"))
					{
						if (Options.MolType == SubstructureSearchOptions.EMolType.SMILES)
							sb.AppendFormat("join bingo.SearchSub('compounds_sss', '{0}', '{1}') bingo on c.csid = bingo.id", Options.Molecule, ResultOptions != null && ResultOptions.Limit > 0 ? string.Format("TOP {0};", ResultOptions.Limit) : "");
						else if (Options.MolType == SubstructureSearchOptions.EMolType.SMARTS)
							sb.AppendFormat("join bingo.SearchSMARTS('compounds_sss', '{0}', '{1}') bingo on c.csid = bingo.id", Options.Molecule, ResultOptions != null && ResultOptions.Limit > 0 ? string.Format("TOP {0};", ResultOptions.Limit) : "");
					}
					else if (tbl.ToLower().Trim().StartsWith("left join") || tbl.ToLower().Trim().StartsWith("join"))
						sb.AppendFormat(" {0}", tbl);
					else
						sb.AppendFormat(",{0}", tbl);
				}

				if (predicates.Count > 0)
				{
					sb.Append(" WHERE ");
					sb.Append(String.Join(" AND ", predicates));
				}

				return sb.ToString();
			}

			return string.Empty;
		}
	}

	public class CSCSimilarityStructureSearch : CSSimilarityStructureSearch
	{
		public CSCSimilarityStructureSearch()
		{
			m_sqlProvider = new CSCSqlSearchProvider();
		}

		public override string Description
		{
			get
			{
				if(Options.SimilarityType == SimilaritySearchOptions.ESimilarityType.Tversky)
					return string.Format("Structure Search - Similarity; Molecule: {0}; Type: {1}; Threshold: {2}; Alpha: {3}; Beta: {4}", Options.Molecule, Options.SimilarityType.ToString(), Options.Threshold, Options.Alpha, Options.Beta);
				else
					return string.Format("Structure Search - Similarity; Molecule: {0}; Type: {1}; Threshold: {2}", Options.Molecule, Options.SimilarityType.ToString(), Options.Threshold);
			}
		}

		protected override string GetCountSqlCommand(List<string> predicates, List<string> tables, List<string> columns, List<string> orderby)
		{
			return string.Empty;
		}

		protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
		{
			bool bAdded = false;
			if (Options.Molecule != String.Empty)
			{
				visual.Add("Structure Search - Similarity");

				string s;
				switch (Options.SimilarityType)
				{
					case SimilaritySearchOptions.ESimilarityType.Tanimoto:
						s = "Tanimoto";
						break;
					case SimilaritySearchOptions.ESimilarityType.Tversky:
						s = "Tversky";
						if (Options.Alpha > 0 || Options.Beta > 0)
							s += string.Format(" {0} {1}", Options.Alpha, Options.Beta);

						break;
					case SimilaritySearchOptions.ESimilarityType.Euclidian:
						s = "Euclid-sub";
						break;
					default:
						throw new ArgumentException("Unsupported similarity search type");
				}

				tables.Add(string.Format("join bingo.SearchSim('compounds_sss', '{0}', '{1}', {2}, null) bingo on c.csid = bingo.id", Options.Molecule, s + (ResultOptions != null && ResultOptions.Limit > 0 ? String.Format("; TOP {0}", ResultOptions.Limit) : ""), Options.Threshold));

				columns.Add("bingo.similarity");
				//tables.Add("join compounds_sss sss on sss.csid = bingo.id ");

				orderby.Add("similarity desc");

				bAdded = true;
			}

			return bAdded;
		}
	}
}
