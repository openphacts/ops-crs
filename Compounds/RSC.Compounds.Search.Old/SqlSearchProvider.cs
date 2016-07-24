using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChemSpider.Security;
using ChemSpider.Search;
//using RSC.Compounds.Database;

namespace RSC.Compounds.Search.Old
{
	public class CSCSqlSearchProvider : ISqlSearchProvider
	{
		public string GetConnectionString()
		{
			return null;
			//return CompoundsDB.ConnectionString;
		}

		public void GetCommonSqlParts(CommonSearchOptions options, List<string> predicates, List<string> tables, List<string> orderby, List<string> visual)
		{
			// Multicomponents
			if (options.Complexity == CommonSearchOptions.EComplexity.Single)
			{
				//predicates.Add("cp.multicomponent_yn = 0");
				//visual.Add("SingleComponent");
				//bAddTable = true;
			}
			else if (options.Complexity == CommonSearchOptions.EComplexity.Multi)
			{
				//predicates.Add("cp.multicomponent_yn = 1");
				//visual.Add("MultiComponent");
				//bAddTable = true;
			}

			// Labeled
			if (options.Isotopic == CommonSearchOptions.EIsotopic.Labeled)
			{
				//predicates.Add("cp.isotopic_yn = 1");
				//visual.Add("Isotopic");
				//bAddTable = true;
			}
			else if (options.Isotopic == CommonSearchOptions.EIsotopic.NotLabeled)
			{
				//predicates.Add("cp.isotopic_yn = 0");
				//visual.Add("NonIsotopic");
				//bAddTable = true;
			}

			// Spectra
			if (options.HasSpectra)
			{
				//predicates.Add("exists (select 1 from spectra spc where spc.deleted_yn = 0 and spc.cmp_id = c.cmp_id)");
				//visual.Add("HaveSpectra");
			}

			predicates.Add("c.csid = cp.csid");
			tables.Add("compound_properties cp");
		}

		public void GetScopeSqlParts(SearchScopeOptions options, List<string> predicates, List<string> tables, List<string> orderby, List<string> visual)
		{
			if (options.DataSources.Count() > 0)
			{
				string dsns = String.Join("','", options.DataSources.Select(s => s.Replace("'", "''")));
				predicates.Add(String.Format("exists (select 1 from substances s join sid_csid on sid_csid.sid = s.sid where sid_csid.csid = c.csid and s.dsn_guid in ('{0}'))", dsns));
				//predicates.Add(String.Format("exists (select 1 from substances s join datasources ds on s.dsn_id = ds.dsn_id join sid_csid on sid_csid.sid = s.sid where sid_csid.csid = c.csid and ds.dsn_name in ('{0}'))", dsns));
				//predicates.Add(String.Format("exists (select 1 from substances __s join v_data_sources __vds on __s.dsn_id = __vds.dsn_id where __s.deleted_yn = 0 and __vds.name in ('{0}'))", dsns));
				visual.Add("DATA_SOURCE in ('" + dsns + "')");
			}

			if (options.DataSourceTypes.Count() > 0)
			{
				//string dsts = String.Join("','", options.DataSourceTypes.Select(s => s.Replace("'", "''")));
				//predicates.Add(String.Format("exists (select 1 from substances __s join v_ds_ds_types __vdt on __s.dsn_id = __vdt.dsn_id where __s.deleted_yn = 0 and __vdt.name in ('{0}'))", dsts));
				//visual.Add("DATA_SOURCE_TYPE in ('" + dsts + "')");
			}

			// X Sections
			if (options.XSections.Count() > 0)
			{
				//string xsecs = String.Join(",", options.XSections.Select(s => String.Format("'{0}'", s.Replace("'", "''"))));
				//ChemSpiderDB db = new ChemSpiderDB();
				//DataTable dt = db.DBU.m_fillDataTable("select sec_id, sec_type, query from xsections where name in (" + xsecs + ")");
				//foreach (DataRow dr in dt.Rows)
				//{
				//    if (dr["sec_type"].ToString() == "L")
				//        predicates.Add(String.Format("c.cmp_id in (select cmp_id from compounds_lists where sec_id = {0})", dr["sec_id"]));
				//    else
				//        predicates.Add(String.Format("c.cmp_id in ({0})", dr["query"]));
				//}

				//visual.Add("XSECTIONS in (" + xsecs + ")");
			}

			if (options is CSCSearchScopeOptions)
			{
				var scopeOptions = options as CSCSearchScopeOptions;

				if (scopeOptions.RealOnly) {
					predicates.Add("c.isSubstanceDerived = 1");
				}
			}
		}

		public string GetResultSqlCommand(SearchResultOptions options, List<string> predicates, List<string> tables, List<string> columns, List<string> orderby, bool partialSql)
		{
			StringBuilder cmdText = new StringBuilder();
/*
			//Compose the count statement and run that first?
			countSQL = String.Empty;

			StringBuilder sb = new StringBuilder("SELECT count(c.csid) FROM compounds c");
			if (!partialSql)
				if (predicates.Count > 0 || tables.Count > 0)
				{
					foreach (string tbl in tables)
					{
						if (tbl.ToLower().StartsWith("left join") || tbl.ToLower().StartsWith("join"))
							sb.AppendFormat(" {0}", tbl);
						else
							sb.AppendFormat(",{0}", tbl);
					}

					if (predicates.Count > 0)
					{
						sb.Append(" WHERE ");
						sb.Append(String.Join(" AND ", predicates));
					}
					countSQL = sb.ToString();
				}
*/
			// Compose SQL statement
			if (predicates.Count > 0 || tables.Count > 0)
			{
				if (!partialSql)
				{
					cmdText.AppendFormat(@"SELECT {0} c.csid {1},
													--cp.mw_indigo as Molecular_Weight,
													0 AS n_ds_total,
													0 AS n_references,
													0 AS n_pubmed_hits,
													0 AS n_rsc_hits ",
						options == null || options.Limit <= 0 ? "" : String.Format("TOP {0}", options.Limit),
						columns.Count > 0 ? ", " + String.Join(", ", columns.Distinct()) : "");

					cmdText.AppendFormat(@"FROM compounds c 
											--LEFT JOIN compounds_hits pubmed ON c.cmp_id = pubmed.cmp_id AND pubmed.src_id = 1
											--LEFT JOIN compounds_hits rcs_j ON c.cmp_id = rcs_j.cmp_id AND rcs_j.src_id = 2
											");
				}
				if (tables.Count > 0)
				{
					foreach (string tbl in tables)
					{
						if (tbl.ToLower().Trim().StartsWith("left join") || tbl.Trim().ToLower().StartsWith("join"))
							cmdText.AppendFormat(" {0}", tbl);
						else
							cmdText.AppendFormat(",{0}", tbl);
					}
				}

				if (predicates.Count > 0)
				{
					cmdText.Append(" WHERE ");
					cmdText.Append(String.Join(" AND ", predicates));
				}

				if (!partialSql)
				{
					//Add default ordering by # of Data Sources
					if (orderby.Count == 0)
						orderby.Add("n_ds_total DESC");

					//Add the ordering either selected from the front-end or from the base class.
					cmdText.Append(" ORDER BY " + String.Join(", ", (options ?? new SearchResultOptions()).SortOrder.Count > 0 ? options.SortOrder : orderby.Distinct().ToList()));
				}
			}

			return cmdText.ToString();
		}

		public void GetSandboxParts(Sandbox sandbox, List<string> predicates, List<string> tables)
		{
			//if (sandbox != null)
			//{
			//    if (!predicates.Exists(s => s == "s.cmp_id = c.cmp_id"))
			//        predicates.Add("s.cmp_id = c.cmp_id");
			//    if (!tables.Exists(s => s == "substances s"))
			//        tables.Add("substances s");

			//    predicates.Add(String.Format("s.dsn_id = {0}", sandbox.DsnId));
			//    if (sandbox.ColId != null)
			//        predicates.Add(String.Format("s.col_id = {0}", sandbox.ColId));
			//}
		}
	}
}
