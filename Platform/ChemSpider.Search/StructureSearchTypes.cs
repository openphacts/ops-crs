using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using ChemSpider.Security;
using ChemSpider.Database;
using ChemSpider.Data.Database;
using System.Diagnostics;
using System.Configuration;
using ChemSpider.Molecules;
using InChINet;
using System.Data.SqlClient;

namespace ChemSpider.Search
{
	public class CSExactStructureSearch : CSSqlSearch
	{
		public new ExactStructureSearchOptions Options
		{
			get { return base.Options as ExactStructureSearchOptions; }
		}

		protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
		{
			bool bAdded = false;
			if (Options.Molecule != String.Empty)
			{
				visual.Add("Structure Search - Exact");

				string mol = Options.Molecule;
				if (!mol.Contains('\n'))
					mol = MolUtils.SMILESToMol(mol);

				string inchi = InChIUtils.mol2InChI(mol, InChIFlags.Default);
				if (!String.IsNullOrEmpty(inchi))
				{
					string[] inchi_layers = InChIUtils.getInChILayers(inchi);
					if (inchi_layers == null)
						throw new ChemSpiderException("Unable to parse InChI: {0}\nOriginal MOL: {1}", inchi, mol);

					switch (Options.MatchType)
					{
						case ExactStructureSearchOptions.EMatchType.ExactMatch:
							tables.Add("inchis_md5 imd5");
							predicates.Add("c.cmp_id = imd5.cmp_id");
							predicates.Add("imd5.inchi_md5 = HashBytes('md5', '" + DbUtil.prepare4sql(inchi_layers[0]) + "')");
							break;
						case ExactStructureSearchOptions.EMatchType.AllTautomers:
							visual.Add("All Tautomers");
							tables.Add("inchis_md5 imd5");
							predicates.Add("c.cmp_id = imd5.cmp_id");
							predicates.Add("imd5.inchi_chsi_md5 = HashBytes('md5', '" + DbUtil.prepare4sql(inchi_layers[4]) + "')");
							break;
						case ExactStructureSearchOptions.EMatchType.SameSkeletonIncludingH:
							visual.Add("Same Skeleton (Including H)");
							tables.Add("inchis_md5 imd5");
							predicates.Add("c.cmp_id = imd5.cmp_id");
							predicates.Add("imd5.inchi_ch_md5 = HashBytes('md5', '" + DbUtil.prepare4sql(inchi_layers[1]) + "')");
							break;
						case ExactStructureSearchOptions.EMatchType.SameSkeletonExcludingH:
							visual.Add("Same Skeleton (Excluding H)");
							tables.Add("inchis_md5 imd5");
							predicates.Add("c.cmp_id = imd5.cmp_id");
							predicates.Add("imd5.inchi_c_md5 = HashBytes('md5', '" + DbUtil.prepare4sql(inchi_layers[2]) + "')");
							break;
						case ExactStructureSearchOptions.EMatchType.AllIsomers:
							visual.Add("All Isomers");
							tables.Add("inchis i");
							predicates.Add("c.cmp_id = i.cmp_id");
							predicates.Add("i.inchi_mf = '" + DbUtil.prepare4sql(inchi_layers[3]) + "'");
							break;
					}
					bAdded = true;
				}
			}

			return bAdded;
		}
	}
	public abstract class CSSSSSearch : CSSqlSearch
	{
		protected new StructureSearchOptions Options
		{
			get { return base.Options as StructureSearchOptions; }
		}

		protected override int MaxRecords
		{
			get { return 10000; }
		}

		public override void SetOptions(SearchOptions options, CommonSearchOptions common, SearchScopeOptions scopeOptions, SearchResultOptions resultOptions)
		{
			if ((options as StructureSearchOptions) == null)
				throw new ArgumentException("Options is null or of wrong type");

			base.SetOptions(options, common, scopeOptions, resultOptions);
		}
/*
		public override void Run(Sandbox sandbox, CSSearchResult result)
		{
			string countSql;
			string cmdText = GetSqlCommand(sandbox, out countSql, (result as CSRequestSearchResult).Request, false);

			if (cmdText == String.Empty)
			{
				result.Status = ERequestStatus.Failed;
				result.Message = "No query specified";
				result.Update();
			}
			else
			{
				try
				{
					using (SqlConnection conn = new SqlConnection(m_sqlProvider.GetConnectionString()))
					{
						conn.Open();

						SqlCommand cmd = new SqlCommand(cmdText, conn);
						cmd.CommandType = CommandType.Text;
						cmd.CommandTimeout = result.Timeout.Seconds;

						result.Found = new ResultList(EResultObjectType.Compound);
						result.Found.SetResultRecords(GetListOfResults(cmd));
						result.Count = result.Found.Count;

						result.Progress = 1;
						result.Status = ERequestStatus.ResultReady;
						result.Message = "Finished";
					}
				}
				catch (Exception ex)
				{
					result.Status = ERequestStatus.Failed;
					result.Message = ex.Message;
				}
				finally
				{
					result.Update();
				}
			}
		}
*/
		protected List<ResultRecord> GetListOfResults(SqlCommand cmd)
		{
			SqlDataReader reader = cmd.ExecuteReader();
			List<ResultRecord> list = new List<ResultRecord>();

			while (reader.Read())
			{
				list.Add(new ResultRecord()
				{
					Id = int.Parse(reader[0].ToString()),
					Relevance = double.Parse(reader["similarity"].ToString())
				});
			}

			reader.Close();

			return list;
		}

		/// <summary>
		/// Returns a boolean determining whether the scope is set, such that this should use the MarinLit Structure Search Database.
		/// </summary>
		/// <returns>bool</returns>
		protected bool isMLSearch()
		{
			if (ScopeOptions != null)
			{
				if (ScopeOptions.DataSources.Count > 0)
				{
					if (ConfigurationManager.AppSettings["ml_dsn"] != null)
					{
						return ScopeOptions.DataSources.Count == 1 && ScopeOptions.DataSources[0].Equals(ConfigurationManager.AppSettings["ml_dsn"]);
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Returns a boolean determining whether the scope is set, such that this should use the Merck Index Structure Search Database.
		/// </summary>
		/// <returns>bool</returns>
		protected bool isMISearch()
		{
			if (ScopeOptions != null)
			{
				if (ScopeOptions.DataSources.Count > 0)
				{
					if (ConfigurationManager.AppSettings["mi_dsn"] != null)
					{
						return ScopeOptions.DataSources.Count == 1 && ScopeOptions.DataSources[0].Equals(ConfigurationManager.AppSettings["mi_dsn"]);
					}
				}
			}
			return false;
		}


		/// <summary>
		/// Returns a boolean determining whether the scope is set, such that this should use the Pharmasea Structure Search Database.
		/// </summary>
		/// <returns>bool</returns>
		protected bool isPSSearch(Sandbox sandbox)
		{
			if (ScopeOptions != null)
			{
				if (ScopeOptions.DataSources.Count > 0)
				{
					if (ConfigurationManager.AppSettings["ps_dsn"] != null)
					{
						return ScopeOptions.DataSources.Count == 1 && ScopeOptions.DataSources[0].Equals(ConfigurationManager.AppSettings["ps_dsn"]);
					}
				}
			}
			else
			{
				//Check for PharmaSea as the Sandbox.
				if (sandbox != null)
				{
					if (ConfigurationManager.AppSettings["ps_dsn"] != null)
					{
						return sandbox.Key.Equals(ConfigurationManager.AppSettings["ps_dsn"], StringComparison.OrdinalIgnoreCase);
					}
				}
			}
			return false;
		}

		protected virtual ChemSpiderSSSDB GetSearchDB(Sandbox sandbox)
		{
			//Check to see if the list of dsn ids in the ScopeOptions matches or is a sub-set of the list of OPS dsn_ids.            
			if (isMLSearch())
				return new ChemSpiderMLSSSDB();
			else if (isMISearch())
				return new ChemSpiderMISSSDB();
			else if (isPSSearch(sandbox))
				return new ChemSpiderPSSSSDB();
			else
				return new ChemSpiderSSSDB();
		}
	}

	public class CSSubstructureSearch : CSSSSSearch
	{
		protected new SubstructureSearchOptions Options
		{
			get { return base.Options as SubstructureSearchOptions; }
		}

		public override string Description
		{
			get { return "Structure Search - Substructure"; }
		}

		protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
		{
			bool bAdded = false;
			if (Options.Molecule != String.Empty)
			{
				visual.Add("Structure Search - Substructure");

				tables.Add(string.Format(" join bingo.SearchSub('compounds_sss', '{0}', '{1}') bingo on c.cmp_id = bingo.id", Options.Molecule, ResultOptions != null && ResultOptions.Limit > 0 ? string.Format("TOP {0};", ResultOptions.Limit) : ""));
				tables.Add(" join compounds_sss sss on sss.csid = bingo.id ");
			}

			return bAdded;
		}
	}
	public class CSSimilarityStructureSearch : CSSSSSearch
	{
		protected new SimilaritySearchOptions Options
		{
			get { return base.Options as SimilaritySearchOptions; }
		}

		public override string Description
		{
			get { return "Structure Search - Similarity"; }
		}

		protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
		{
			bool bAdded = false;
			if (Options.Molecule != String.Empty)
			{
				visual.Add("Structure Search - Similarity");

				//tables.Add(string.Format(" join bingo.SearchSub('compounds_sss', '{0}', '{1}') bingo on c.cmp_id = bingo.id", Options.Molecule, ResultOptions != null && ResultOptions.Limit > 0 ? string.Format("TOP {0};", ResultOptions.Limit) : ""));
				//tables.Add(" join compounds_sss sss on sss.csid = bingo.id ");
			}

			return bAdded;
		}
	}
}
