using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using ChemSpider.Database;
using ChemSpider.Molecules;
using System.Data;
using ChemSpider.ObjectModel;
using RSC.Compounds.Old;

namespace RSC.Compounds.Database
{
	public class SQLSubstancesStore : ISubstancesStore
	{
		/// <summary>
		/// Returns general substance information by Id
		/// </summary>
		/// <param name="id">Substance's ID</param>
		/// <returns>Substance object</returns>
		public RSC.Compounds.Old.Substance GetSubstance(int id)
		{
			var substance = new RSC.Compounds.Old.Substance()
			{
				SUBSTANCE_ID = id
			};

			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
				substance.SUBSTANCE_VERSION = conn.ExecuteScalar<int>("select max(version) from substance_versions where sid = @sid", new { sid = id });

			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				//	check that substance exists
				int subId = conn.ExecuteScalar<int>("select sid from substances where sid = @id", new { id = id });
				if (subId == 0)
					return null;

				conn.ExecuteReader(String.Format(@"
					select  s.ext_regid,
							s.dsn_guid,
							cast(sc.csid as int) csid,
							v.depositor_std_inchi,
							v.depositor_smiles,
							v.depositor_comments,
							v.sdf
					from substances s
						left outer join sid_csid sc on sc.sid = s.sid
						join substance_versions v on v.sid = s.sid
					where s.sid={0} and v.version={1}", substance.SUBSTANCE_ID, substance.SUBSTANCE_VERSION),
					r =>
					{
						substance.COMPOUND_ID = (int)r["csid"];
						substance.RECORD_EXTERNAL_REGID = r["ext_regid"] as string;
						substance.DEPOSITOR_DATA_SOURCE_GUID = Guid.Parse(r["dsn_guid"].ToString());
						//substance.DEPOSITOR_DATA_SOURCE_ID = r["dsn_id"] is DBNull ? 0 : (int)r["dsn_id"];
						//substance.DEPOSITOR_DATA_SOURCE_NAME = r["dsn_name"] as string;
						substance.DEPOSITOR_SUBSTANCE_INCHI = r["depositor_std_inchi"] as string;
						substance.DEPOSITOR_SUBSTANCE_SMILES = r["depositor_smiles"] as string;
						substance.DEPOSITOR_SUBSTANCE_COMMENTS = r["depositor_comments"] as string;

						substance.SDF = r["sdf"] as string;

						SdfRecord _sdf = SdfRecord.FromString(substance.SDF);
						substance.MOL = _sdf.Mol;
					});
			}

			return substance;
		}

		/// <summary>
		/// Returns substances page by page
		/// </summary>
		/// <param name="start">Index where to start returning substances from</param>
		/// <param name="coun">Number of returned substances</param>
		/// <returns>List of substances</returns>
		public IEnumerable<RSC.Compounds.Old.Substance> GetSubstances(int start = 0, int count = 10)
		{
			var substances = new List<RSC.Compounds.Old.Substance>();

			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				foreach (var id in conn.FetchColumn<int>(@"with cte as (
															SELECT [sid], ROW_NUMBER() OVER(ORDER BY [sid]) as rn
															FROM substances
														)
														SELECT [sid] FROM cte
														WHERE rn BETWEEN @start and @start + @count - 1", new { start = start + 1, count = count }))
				{
					var substance = GetSubstance(id);

					substances.Add(substance);
				}
			}

			return substances;
		}

		/// <summary>
		/// Returns list of synonyms
		/// </summary>
		/// <param name="id">Substance ID</param>
		/// <returns>List of synonyms</returns>
		public IEnumerable<string> GetSynonyms(int id)
		{
			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
				return conn.FetchColumn<string>("select synonym_display from substance_derived_synonyms s inner join substance_synonyms d on d.synonym_id=s.synonym_id where d.sid=@sid", new { sid = id });
		}

		public IEnumerable<ChemSpider.ObjectModel.Issue> GetIssues(int id)
		{
			var issues = new List<ChemSpider.ObjectModel.Issue>();

			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				foreach (DataRow dr in conn.FillDataTable("select * from substance_issues where sid=@sid", new { sid = id }).Rows)
				{
					//in new Issue class definition "code" property is integer, however old CRS data in database has some string values not convertable to integer, e.g. "T1", 
					//which will be fixed in next dataase upload
					//try{}catch will catch those unconvertable issue codes so code doesn't fails
					try
					{
						issues.Add(new ChemSpider.ObjectModel.Issue
						{
							Severity = (Severity)Enum.Parse(typeof(Severity), dr["issue_severity"].ToString()),
							IssueType = (IssueType)Enum.Parse(typeof(IssueType), dr["issue_type"].ToString()),
							Code = Convert.ToInt32(dr["issue_code"].ToString()),
							Message = dr["issue_description"].ToString(),
							Description = dr["issue_additional_description"].ToString(),
							StackTrace = dr["stackTrace"].ToString()
						});
					}
					catch (Exception)
					{
					}
				}
			}

			return issues;
		}

		/// <summary>
		/// Returns total number of substances registered in the database
		/// </summary>
		/// <returns>Number of total substances</returns>
		public int GetSubstancesCount()
		{
			int count = 0;

			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				count = conn.ExecuteScalar<int>("select count(*) from substances");
			}

			return count;
		}

		public int GeLastVersionIndex(int id)
		{
			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
				return conn.ExecuteScalar<int>("select top 1 version from substance_versions where sid = @id order by version desc", new { id = id });
		}

		public string GetSDF(int id, int? version = null)
		{
			if (version == null)
				version = GeLastVersionIndex(id);

			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
				return conn.ExecuteScalar<string>("select sdf from substance_versions where sid = @id and version = @version", new { id = id, version = version });
		}
	}
}
