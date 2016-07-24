using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using ChemSpider.Database;
using ChemSpider.Data.Database;
using System.Diagnostics;
using System.Data;
using System.Collections;
using ChemSpider.Security;

namespace ChemSpider.Search
{
	public abstract class CSSqlSearch : CSSearch
	{
		protected List<string> m_Predicates;
		protected List<string> m_Columns;
		protected List<string> m_Tables;
		protected List<string> m_Orderby;

		public override void SetOptions(SearchOptions options, CommonSearchOptions common, SearchScopeOptions scopeOptions, SearchResultOptions resultOptions)
		{
			base.SetOptions(options, common, scopeOptions, resultOptions);

			m_Predicates = new List<string>();
			m_Columns = new List<string>();
			m_Tables = new List<string>();
			m_Orderby = new List<string>();

			InterpretOptions();
		}

		static int s_Counter = 0;
		int m_Counter = ++s_Counter;

		protected ISqlSearchProvider m_sqlProvider = null;

		public CSSqlSearch()
		{
			m_sqlProvider = new CSSqlSearchProvider();
		}

		public CSSqlSearch(ISqlSearchProvider provider)
		{
			m_sqlProvider = provider;
		}

		/// <summary>
		/// Max possibly returned records. If more than this amount will be found then the search result status will be set as "Too many records"
		/// </summary>
		protected virtual int MaxRecords
		{
			get { return 100000; }
		}

		protected string Uniq(string s)
		{
			return s.Replace("++", m_Counter.ToString());
		}

		protected abstract bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns);

		public bool GetSqlAndSubstitute(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
		{
			List<string> l_predicates = new List<string>();
			List<string> l_tables = new List<string>();
			List<string> l_orderby = new List<string>();
			List<string> l_columns = new List<string>();

			bool r = GetSqlParts(l_predicates, l_tables, l_orderby, visual, l_columns);

			l_predicates.ForEach(i => predicates.Add(Uniq(i)));
			l_tables.ForEach(i => tables.Add(Uniq(i)));
			l_orderby.ForEach(i => orderby.Add(Uniq(i)));
			l_columns.ForEach(i => columns.Add(Uniq(i)));

			return r;
		}

		private void InterpretOptions()
		{
			List<string> visual_predicates = new List<string>();

			GetSqlAndSubstitute(m_Predicates, m_Tables, m_Orderby, visual_predicates, m_Columns);

			if ( CommonOptions != null )
				m_sqlProvider.GetCommonSqlParts(CommonOptions, m_Predicates, m_Tables, m_Orderby, visual_predicates);

			if ( ScopeOptions != null )
				m_sqlProvider.GetScopeSqlParts(ScopeOptions, m_Predicates, m_Tables, m_Orderby, visual_predicates);

			// Compose visual query string
			Description = String.Join(" AND ", visual_predicates);
		}

		protected string GetSqlCommand(Sandbox sandbox, Request request, bool partialSql)
		{
			// Data Source Sandbox if any
			m_sqlProvider.GetSandboxParts(sandbox, m_Predicates, m_Tables);

			for (int i = 0; i < m_Columns.Count; ++i) {
				for (int j = 0; j < i; ++j) {
					int d1 = m_Columns[i].LastIndexOf('.');
					int d2 = m_Columns[j].LastIndexOf('.');
					if (d1 > 0 && d2 > 0 && m_Columns[i].Substring(d1) == m_Columns[j].Substring(d2)) {
						m_Predicates.Add(m_Columns[i] + " = " + m_Columns[j]);
					}
				}
			}

			m_Tables.Sort((x, y) =>
			{
				bool x1 = x.ToLower().StartsWith("left join") || x.ToLower().StartsWith("join");
				bool y1 = y.ToLower().StartsWith("left join") || y.ToLower().StartsWith("join");
				return x1 && y1 ? x.CompareTo(y) : x1 ? -1 : 1;
			});

			return m_sqlProvider.GetResultSqlCommand(ResultOptions, m_Predicates, m_Tables, m_Columns, m_Orderby, partialSql);
		}

		protected virtual string GetCountSqlCommand(List<string> predicates, List<string> tables, List<string> columns, List<string> orderby)
		{
			StringBuilder sb = new StringBuilder("SELECT count(c.csid) FROM compounds c");

			if (predicates.Count > 0 || tables.Count > 0)
			{
				foreach (string tbl in tables)
				{
					if (tbl.ToLower().Trim().StartsWith("left join ") || tbl.ToLower().Trim().StartsWith("join "))
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

		internal virtual ResultList GetResults (SqlCommand cmd)
		{
			return new ResultList(EResultObjectType.Compound, DbUtil.fetchColumn(cmd));
		}

		public override void Run(Sandbox sandbox, CSSearchResult result)
		{
			string cmdText = GetSqlCommand(sandbox, (result as CSRequestSearchResult).Request, false);

			if ( cmdText == String.Empty ) {
				result.Status = ERequestStatus.Failed;
				result.Message = "No query specified";
				result.Update();
			}
			else {
				try {
					using ( SqlConnection conn = new SqlConnection(m_sqlProvider.GetConnectionString()) ) {

						conn.Open();
						string countSql = GetCountSqlCommand(m_Predicates, m_Tables, m_Columns, m_Orderby);
						if (!string.IsNullOrEmpty(countSql))
						{
							SqlCommand cmdCount = new SqlCommand(countSql, conn);
							cmdCount.CommandType = CommandType.Text;
							cmdCount.CommandTimeout = result.Timeout.Seconds;
							int count = Convert.ToInt32(cmdCount.ExecuteScalar());

							//Only do the full search if the count is less than MaxRecords
							if (count <= MaxRecords)
							{
								//	do not comment out the next line!!!
								//if (count > 0)
								{
									SqlCommand cmd = new SqlCommand(cmdText, conn);
									cmd.CommandType = CommandType.Text;
									cmd.CommandTimeout = result.Timeout.Seconds;
									result.Found = GetResults(cmd);
									result.Count = result.Found.Count;
								}
								result.Progress = 1;
								result.Status = ERequestStatus.ResultReady;
								result.Message = "Finished";
							}
							else
							{
								result.Status = ERequestStatus.TooManyRecords;
								result.Count = count;
								result.Message = "Too many records found";
							}
						}
						else
						{
							SqlCommand cmd = new SqlCommand(cmdText, conn);
							cmd.CommandType = CommandType.Text;
							cmd.CommandTimeout = result.Timeout.Seconds;
							result.Found = GetResults(cmd);
							result.Count = result.Found.Count;

							result.Progress = 1;
							result.Status = ERequestStatus.ResultReady;
							result.Message = "Finished";
						}
					}
				}
				catch ( Exception ex ) {
					result.Status = ERequestStatus.Failed;
					result.Message = ex.Message;
				}
				finally {
					result.Update();
				}
			}
		}
	}
}
