using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using ChemSpider.Utilities;
using ChemSpider.Database;

namespace ChemSpider.Data.Database
{
	/// <summary>
	/// Summary description for ChemSpiderDB
	/// </summary>
	public class ChemSpiderDB : ChemSpiderBaseDB
	{
		private static ChemSpiderDB s_csdb
		{
			get
			{
				return new ChemSpiderDB();
			}
		}

		public static string ConnectionString
		{
			get
			{
				return ConfigurationManager.ConnectionStrings["ChemSpiderConnectionString"] == null ?
					null :
					ConfigurationManager.ConnectionStrings["ChemSpiderConnectionString"].ConnectionString;
			}
		}

		public static string RO_ConnectionString
		{
			get
			{
				return ConfigurationManager.ConnectionStrings["ChemSpiderROConnectionString"] == null ?
					ConnectionString :
					ConfigurationManager.ConnectionStrings["ChemSpiderROConnectionString"].ConnectionString;
			}
		}

		protected override string DatabaseName
		{
			get { return "ChemSpider"; }
		}
		
		protected override string ConnString
		{
			get
			{
				return ConnectionString;
			}
		}

		protected override string RO_ConnString
		{
			get
			{
				return RO_ConnectionString;
			}
		}

		public ChemSpiderDB()
			: this(null)
		{
		}

		public ChemSpiderDB(DbUtil u): base(u)
		{
		}

		public struct DataSourceType
		{
			public int id;
			public string name;
			public DataSourceType(int i, string s)
			{
				id = i;
				name = s;
			}
		};

		//public Dictionary<int, string> getDatasourceTypes()
		//{
		//    Dictionary<int, string> result = new Dictionary<int, string>();
		//    using ( SqlDataReader r = DBU.executeReader("select name, dst_id from ds_types order by ord desc, name") ) {
		//        while ( r.Read() ) {
		//            result.Add((int)r.GetValue(1), (string)r.GetValue(0));
		//        }
		//    }
		//    return result;
		//}

		// returns deposition id
		public int create_deposition(int usr_id, bool open_yn, string description, int num_records,
								 string orig_file_name, string typ, int col_id, string encoding,
								 bool updateDSN, int update_flags, out string data_dir)
		{
			return create_deposition(usr_id, open_yn, null, description, num_records, orig_file_name, typ, col_id, encoding, updateDSN, update_flags, out data_dir);
		}

		public int create_deposition(int usr_id, bool open_yn, int? dsn_id,
				string description, int num_records, string orig_file_name, string typ, int? col_id,
				string encoding, bool updateDSN, int update_flags, out string data_dir)
		{
			Hashtable args = new Hashtable();
			args.Add("@usr_id", usr_id);
			args.Add("@dsn_id", dsn_id);
			args.Add("@open_yn", open_yn ? 1 : 0);
			args.Add("@description", description);
			args.Add("@num_records", num_records);
			args.Add("@orig_file_name", orig_file_name);
			args.Add("@typ", typ);
			args.Add("@col_id", col_id);
			args.Add("@update_dsn_yn", updateDSN);
			args.Add("@update_flags", update_flags);
			if ( !string.IsNullOrEmpty(encoding) )
				args.Add("@encoding", encoding);
			DataTable dt = DBU.m_fillDataTableProc("create_deposition", args);
			int? dep_id = (int?)dt.Rows[0]["dep_id"];
			data_dir = (string)dt.Rows[0]["data_dir"];
			if ( dep_id == null )
				throw new Error("Cannot save data");

			return dep_id == null ? -1 : (int)dep_id;
		}

		// returns "track id"
		public int dep_change_status_tag(int dep_id, int usr_id, string status_tag, bool? open_yn)
		{
			Hashtable args = new Hashtable();
			args.Add("@dep_id", dep_id);
			args.Add("@status_tag", status_tag);
			args.Add("@usr_id", usr_id);
			if ( open_yn != null )
				args.Add("@open_yn", (bool)open_yn ? 1 : 0);
			object o = DBU.m_runSqlProc("dep_change_status_tag", args, "track_id");
			if ( o is DBNull )
				return -1;
			return (int)o;
		}

		public void dep_track(int dep_id, char status, int usr_id)
		{
			dep_track(dep_id, status, usr_id, -1);
		}

		public int dep_track(int dep_id, char status, int usr_id, int dtid)
		{
			Hashtable args = new Hashtable();
			args.Add("@dep_id", dep_id);
			args.Add("@status", status);
			args.Add("@usr_id", usr_id);
			args.Add("@dtid", dtid);
			object o = DBU.m_runSqlProc("dep_track", args, "track_id");
			if ( o is DBNull )
				return -1;
			return (int)o;
		}

		public void dep_write_log(char severity, string msg, string raw_msg, int dtid)
		{
			Hashtable args = new Hashtable();
			args.Add("@severity", severity);
			args.Add("@msg", msg);
			args.Add("@raw_msg", raw_msg);
			args.Add("@dtid", dtid);
			DBU.m_runSqlProc("dep_write_log", args);
		}

		public int dep_add_file(int dep_id, string file_name, string typ, int nrecords)
		{
			Hashtable args = new Hashtable();
			args.Add("@dep_id", dep_id);
			args.Add("@file_name", file_name);
			args.Add("@typ", typ);
			args.Add("@nrecords", nrecords);
			return (int?)DBU.m_runSqlProc("dep_add_file", args, "dfid") ?? -1;
		}

		public bool dep_change_file_status(int dfid, string status)
		{
			Hashtable args = new Hashtable();
			args.Add("@dfid", dfid);
			args.Add("@status", status);
			return 1 == (int)DBU.m_runSqlProc("dep_change_file_status", args, "rc");
		}

		public DataTable dep_get_files(int dep_id, string file_name, string typ, string status)
		{
			Hashtable args = new Hashtable();
			args.Add("@dep_id", dep_id);
			args.Add("@file_name", file_name);
			args.Add("@status", status);
			args.Add("@typ", typ);
			return DBU.m_fillDataTableProc("dep_get_files", args);
		}

		public void dep_update_substances_cache(int dfid, int cmp_id, string ext_id, string ext_url, string pubdate, string supp_info)
		{
			Hashtable args = new Hashtable();
			args.Add("@dfid", dfid);
			args.Add("@cmp_id", cmp_id);
			args.Add("@ext_id", ext_id);
			args.Add("@ext_url", ext_url);
			args.Add("@pubdate", pubdate);
			args.Add("@supp_info", supp_info);
			DBU.m_runSqlProc("dep_update_substances_cache", args);
		}

		public void dep_update_substances_from_cache(int dfid)
		{
			Hashtable args = new Hashtable();
			args.Add("@dfid", dfid);
			DBU.m_runSqlProc("dep_update_substances_from_cache", args);
		}

		public bool dep_lock(int dep_id)
		{
			return 0 != DBU.ExecuteScalar<int>(
								@"declare @r int
								  exec @r = dep_lock @dep_id
								  select @r", new { dep_id = dep_id });
		}

		public void dep_unlock(int dep_id)
		{
			DBU.ExecuteCommand("exec dep_unlock", new { dep_id = dep_id });
		}

		public void dep_delete(int dep_id)
		{
			DBU.ExecuteCommand("exec dep_delete", new { dep_id = dep_id });
		}

		public void tmpdep_assign_dep_id(int usr_id, int dep_id)
		{
			Hashtable args = new Hashtable();
			args.Add("@usr_id", usr_id);
			args.Add("@dep_id", usr_id);
			DBU.m_executeUpdate("update tmp_depositions set perm_dep_id = @dep_id where usr_id = @usr_id and perm_dep_id is null", args);
		}

		public int tmpdep_update(int usr_id, int rec_id, string molfile, string molfile_v3000, byte[] preview,
					int? perm_dep_id, string supp_info)
		{
			Hashtable args = new Hashtable();
			args.Add("@usr_id", usr_id);
			args.Add("@rec_id", rec_id);
			args.Add("@molfile", molfile);
			args.Add("@molfile_v3000", molfile_v3000);
			args.Add("@preview", preview);
			args.Add("@perm_dep_id", perm_dep_id);
			args.Add("@supp_info", supp_info);
			return (int)DBU.m_runSqlProc("tmpdep_update", args, "rec_id");
		}

		public void tmpdep_delete(int rec_id)
		{
			Hashtable args = new Hashtable();
			args.Add("@rec_id", rec_id);
			DBU.m_runSqlProc("tmpdep_delete", args);
		}

		public void tmpdep_delete_all(int usr_id)
		{
			Hashtable args = new Hashtable();
			args.Add("@usr_id", usr_id);
			DBU.m_runSqlProc("tmpdep_delete_all", args);
		}

		public void dep_assign_fields(int dep_id, Hashtable fields, bool replace)
		{
			Hashtable args = new Hashtable();
			args["@dep_id"] = dep_id;
			args["@fields"] = fields;
			args["@replace"] = replace;
			DBU.m_runSqlProc("dep_assign_fields", args);
		}

		public void dep_map_fields(int dep_id, DataTable fields)
		{
			Hashtable args = new Hashtable();
			args["@dep_id"] = dep_id;
			args["@map"] = fields;
			fields.TableName = "field";
			DBU.m_runSqlProc("dep_map_fields", args);
		}

		public void dep_edit_field(int dep_id, string field_name, string display_name, int internal_id, string meta, bool read_only, bool confirmed, string calc_meta)
		{
			Hashtable args = new Hashtable();
			args["@dep_id"] = dep_id;
			args["@field_name"] = field_name;
			args["@display_name"] = display_name;
			if ( internal_id >= 0 )
				args["@internal_id"] = internal_id;
			if ( !string.IsNullOrEmpty(meta) )
				args["@field_meta"] = meta;
			args["@readonly"] = read_only;
			args["@confirmed"] = confirmed;
			if ( !string.IsNullOrEmpty(calc_meta) )
				args["@calc_meta"] = calc_meta;
			DBU.m_runSqlProc("dep_edit_field", args);
		}

		public int rev_create(string rtag, int cmp_id, int usr_id, string comment, int update_mask, string type_tag)
		{
			return rev_create(rtag, cmp_id, null, usr_id, comment, update_mask, type_tag);
		}

		public int rev_create(string rtag, int cmp_id, int? elem_id, int usr_id, string comment, int update_mask, string type_tag)
		{
			Hashtable args = new Hashtable();
			args["@rtag"] = rtag;
			args["@cmp_id"] = cmp_id;
			if (elem_id != null) args["@elem_id"] = (int)elem_id;
			args["@usr_id"] = usr_id;
			args["@comment"] = comment;
			args["@update_mask"] = update_mask;
			args["@type_tag"] = type_tag;
			int? rid = (int?)DBU.m_runSqlProc("rev_create", args, 0);

			return rid ?? -1;
		}

		public void cmp_supp_info_delete_dsn(int cmp_id, int except_dep_id)
		{
			Hashtable args = new Hashtable();
			args["@cmp_id"] = cmp_id;
			args["@except_dep_id"] = except_dep_id;
			DBU.m_runSqlProc("cmp_supp_info_delete_dsn", args);
		}

		public void cmp_supp_info_cleanup(int cmp_id)
		{
			Hashtable args = new Hashtable();
			args["@cmp_id"] = cmp_id;
			DBU.m_runSqlProc("cmp_supp_info_cleanup", args);
		}

		public int InChIKeyToCSID(string inchi_key)
		{
			int? cmp_id = null;

			if ( inchi_key.Length == 25 )	// v1.02b
				cmp_id = (int?)DBU.m_querySingleValue(String.Format("select top 1 cmp_id from inchis_md5 where inchi_key = '{0}'", inchi_key));
			else if ( inchi_key.Length == 27 )	// v1.02s
				cmp_id = (int?)DBU.m_querySingleValue(String.Format("select top 1 cmp_id from inchis_std where inchi_key = '{0}'", inchi_key));

			return cmp_id ?? -1;
		}

		public int InChIToCSID(string inchi)
		{
			int? cmp_id = null;

			cmp_id = (int?)DBU.m_querySingleValue(String.Format("select top 1 cmp_id from inchis_md5 where inchi_md5 = HashBytes('md5', '{0}')", inchi));
			if (cmp_id == null)
				cmp_id = (int?)DBU.m_querySingleValue(String.Format("select top 1 cmp_id from inchis_std_md5 where inchi_md5 = HashBytes('md5', '{0}')", inchi));

			return cmp_id ?? -1;
		}

		/// <summary>
		/// Returns 1.02b InChIKey
		/// </summary>
		public string CSIDToInChIKey(int csid)
		{
			return DBU.m_querySingleValue(String.Format("select inchi_key from inchis_md5 where cmp_id = {0}", csid)) as string;
		}

		/// <summary>
		/// Returns Standard InChIKey
		/// </summary>
		public string CSID2InChIKey(int csid)
		{
			return DBU.m_querySingleValue(String.Format("select inchi_key from inchis_std where cmp_id = {0}", csid)) as string;
		}

		/// <summary>
		/// Returns Standard InChI
		/// </summary>
		public string CSID2InChI(int csid)
		{
			return DBU.m_querySingleValue(String.Format("select inchi from inchis_std where cmp_id = {0}", csid)) as string;
		}

		public string CSID2SMILES(int csid)
		{
			return DBU.m_querySingleValue(String.Format("select smiles from compounds where cmp_id = {0}", csid)) as string;
		}

		public void dep_undo_changes(int dep_id, int usr_id, string comment)
		{
			Hashtable args = new Hashtable();
			args["@dep_id"] = dep_id;
			args["@usr_id"] = usr_id;
			args["@comment"] = comment;
			DBU.m_runSqlProc("dep_undo_changes", args, 0);
		}

		private void GetInfoBoxXMLForRecord(int cmp_id, string procname, Hashtable args, out string xml, out string schema, out string add_info, out string add_schema, int max_results)
		{
			SqlCommand cmd = null;
			try {
				using ( cmd = DBU.m_createCommand(procname, args) ) {
					cmd.Parameters.Add("@cmp_id", SqlDbType.Int).Value = cmp_id;
					cmd.Parameters.Add("@xml_data", SqlDbType.Xml, int.MaxValue).Direction = ParameterDirection.Output;
					cmd.Parameters.Add("@xml_schema", SqlDbType.Xml, int.MaxValue).Direction = ParameterDirection.Output;
					cmd.Parameters.Add("@xml_add_info", SqlDbType.Xml, int.MaxValue).Direction = ParameterDirection.Output;
					cmd.Parameters.Add("@xml_add_schema", SqlDbType.Xml, int.MaxValue).Direction = ParameterDirection.Output;
					if (max_results >= 0)
						cmd.Parameters.AddWithValue("@max_results", max_results);

					int res = cmd.ExecuteNonQuery();

					xml = cmd.Parameters["@xml_data"].Value.ToString();
					schema = cmd.Parameters["@xml_schema"].Value.ToString();
					add_info = cmd.Parameters["@xml_add_info"].Value.ToString();
					add_schema = cmd.Parameters["@xml_add_schema"].Value.ToString();
				}
			}
			finally {
				DBU.CloseConn(cmd);
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//
		//	MeSH functions...
		//
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void GetMeSHXMLForRecord(int cmp_id, out string xml, out string schema, out string add_info, out string add_schema)
		{
			Hashtable args = new Hashtable();

			GetInfoBoxXMLForRecord(cmp_id, "GetMeSHXMLForRecord", args, out xml, out schema, out add_info, out add_schema, -1);
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//
		//	Dailymed functions...
		//
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void GetDailymedXMLForRecord(int cmp_id, out string xml, out string schema, out string add_info, out string add_schema)
		{
			Hashtable args = new Hashtable();

			GetInfoBoxXMLForRecord(cmp_id, "GetDailymedXMLForRecord", args, out xml, out schema, out add_info, out add_schema, -1);
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//
		//	SuppInfo functions...
		//
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void GetSuppInfoReferencesXMLForRecord(int cmp_id, out string xml, out string schema, out string add_info, out string add_schema)
		{
			Hashtable args = new Hashtable();

			GetInfoBoxXMLForRecord(cmp_id, "GetSuppInfoReferencesXMLForRecord", args, out xml, out schema, out add_info, out add_schema, -1);
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//
		//	Articles references functions...
		//
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public int AddArticleReference(int cmp_id, string title, string journal, string authors, string description, string doi, int? pmid, string url, bool approved_yn, int? dep_id)
		{
			return DBU.ExecuteScalar<int>(
				"declare @res int exec @res = AddArticleReference @cmp_id, @title, @authors, @journal, @description, @doi, @pmid, @url, @approved_yn, @dep_id select @res",
					new
					{
						cmp_id = cmp_id,
						title = title,
						authors = authors,
						journal = journal,
						description = description,
							doi = doi,
							pmid = pmid,
						url = url,
						approved_yn = approved_yn,
						dep_id = dep_id
					});
		}

		public void GetArticlesReferencesXMLForRecord(int cmp_id, out string xml, out string schema, out string add_info, out string add_schema, int max_results)
		{
			Hashtable args = new Hashtable();

			GetInfoBoxXMLForRecord(cmp_id, "GetArticlesXMLForRecord", args, out xml, out schema, out add_info, out add_schema, max_results);
		}

		public bool HasArticleReferences(int cmp_id)
		{
			using (SqlConnection conn = new SqlConnection(ChemSpiderDB.ConnectionString))
			{
				return conn.ExecuteScalar<int>("SELECT TOP 1 * FROM articles a JOIN compounds_articles ca ON ca.art_id = a.art_id WHERE cmp_id = @cmp_id AND a.deleted_yn = 0", new { cmp_id = cmp_id }) > 0;
			}
		}

		public DataRow GetArticleDetailes(int art_id, int? cmp_id)
		{
			DataTable table = DBU.FillDataTable("exec GetArticleDetailes @art_id, @cmp_id", new { art_id = art_id, cmp_id = cmp_id });
			if (table.Rows.Count != 1)
				return null;

			return table.Rows[0];
		}

		public List<int> GetArticleCompounds(int art_id)
		{
			return DBU.FetchColumn<int>("exec GetArticleCompounds @id", new { id = art_id });
		}

		public void DeleteArticle(int art_id, bool deleted)
		{
			DBU.ExecuteCommand("exec DeleteArticle @art_id, @deleted_yn", new { art_id = art_id, deleted_yn = deleted ? 1 : 0 });
		}

		public void DeleteArticleFromCompound(int art_id, int cmp_id)
		{
			DBU.ExecuteCommand("exec DeleteArticleFromCompound @art_id, @cmp_id", new { art_id = art_id, cmp_id = cmp_id });
		}

		public void DeleteArticlesFromDeposition(int dep_id)
		{
			DBU.ExecuteCommand("exec DeleteArticlesFromDeposition @dep_id", new { dep_id = dep_id });
		}

		public void ApproveArticleReference(int art_id, int cmp_id, bool approved_yn)
		{
			DBU.ExecuteCommand("exec ApproveArticleReference @art_id, @cmp_id, @approved_yn", new { art_id = art_id, cmp_id = cmp_id, approved_yn = approved_yn });
		}

		public void UpdateArticle(int art_id, string title, string journal, string authors, string description, string doi, int? pmid, string url)
		{
			DBU.ExecuteCommand("exec UpdateArticleDetailes @art_id, @title, @journal, @authors, @description, @doi, @pmid, @url", new { art_id = art_id, title = title, journal = journal, authors = authors, description = description, doi = doi, pmid = pmid, url = url });
		}

		public bool IsArticleReferenceApproved(int art_id, int cmp_id)
		{
			return DBU.ExecuteScalar<bool>("select dbo.fIsArticleReferenceApproved(@art_id, @cmp_id)", new { art_id = art_id, cmp_id = cmp_id});
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//
		//	Synonyms functions...
		//
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void GetSynonymsXMLForRecord(int cmp_id, bool? db_id_yn, out string xml, out string schema, out string add_info, out string add_schema)
		{
			Hashtable args = new Hashtable();
			//		args.Add("@cmp_id", cmp_id);
			if ( db_id_yn != null )
				args.Add("@db_id_yn", (bool)db_id_yn ? 1 : 0);

			GetInfoBoxXMLForRecord(cmp_id, "GetSynonymsXMLForRecord", args, out xml, out schema, out add_info, out add_schema, -1);
		}

		/// <summary>
		/// Retrieve the synonyms list as a string but without the schema.
		/// </summary>
		public string GetSynonymsXMLForRecordReadonly(int cmp_id, bool? db_id_yn)
		{           
			return DBU.ExecuteScalar<string>("EXEC GetSynonymsXMLForRecordReadOnly @cmp_id, @db_id_yn", new { cmp_id = cmp_id, db_id_yn = db_id_yn });
		}

		public static Dictionary<int, string> GetMultipleSuppInfoDictionary(string sqlcriteria, XslCompiledTransform xslt)
		{
			string thisxml;
			string internalfieldsxml = GetInternalFieldsAsXML();
			int thiscsid;
			Dictionary<int, string> supppropsandcsids = new Dictionary<int, string>();
			string sqlcommandtext = "WITH XMLNAMESPACES ('chemspider:xmlns:user-data' as userdata) select cast(supp_info as nvarchar(max)), cmp_id from compounds_supp_info" + sqlcriteria + " and cast([supp_info].query('(/cs-record/userdata:user-data-tree)') as nvarchar(max)) != '';";
			SqlDataReader rdr = null;
			try
			{
				using (SqlConnection conn = new SqlConnection(RO_ConnectionString))
				{
					conn.Open();
					SqlCommand cmd = new SqlCommand(sqlcommandtext, conn);
					cmd.CommandText = sqlcommandtext;
					rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
					if (rdr == null)
					{
						return null;
					}
					while (rdr.Read())
					{
						thisxml = rdr.GetString(0);
						thiscsid = rdr.GetInt32(1);
						if (thisxml != null)
						{
							if (thisxml != "")
							{
								thisxml = thisxml.Replace("</cs-record>", "") + "<internal-fields>" + internalfieldsxml + "</internal-fields></cs-record>";
								XmlDocument data = new XmlDocument();
								data.LoadXml(thisxml);
								StringWriter sw = new StringWriter();
								xslt.Transform(data, null, sw);
								thisxml = sw.ToString();
								sw.Flush();
								sw.Close();
								supppropsandcsids.Add(thiscsid, thisxml);
							}
						}
						else
						{

						}
					}
					rdr.Close();
					conn.Close();
					return supppropsandcsids;
				}
			}
			catch
			{
				return supppropsandcsids;
			}


		}

		public static int GetMaxCSIDInDatabse()
		{
			try {
				using (SqlConnection conn = new SqlConnection(RO_ConnectionString)) {
					return conn.ExecuteScalar<int>("select max(cmp_id) from compounds");
				}
			}
			catch {
				return 0;
			}
		}

		public static List<int> GetCSIDsInRange(int mincsid, int maxcsid, Boolean includeDepreciated, Boolean justCompsWithSuppInfo)
		{
			List<int> intcsids = new List<int>();

			try
			{
				string sqlcommandtext = "select cmp_id from compounds where cmp_id >= " + mincsid + " and cmp_id <= " + maxcsid;
				if (justCompsWithSuppInfo == true) {
					sqlcommandtext = sqlcommandtext.Replace("cmp_id", "compounds.cmp_id");
					sqlcommandtext = sqlcommandtext.Replace("from compounds", "from compounds INNER JOIN compounds_supp_info on compounds.cmp_id = compounds_supp_info.cmp_id"); 
					sqlcommandtext = "WITH XMLNAMESPACES ('chemspider:xmlns:user-data' as userdata) " + sqlcommandtext + " and cast([supp_info].query('(/cs-record/userdata:user-data-tree)') as nvarchar(max)) != ''"; 
				}
				if (includeDepreciated == false) { sqlcommandtext = sqlcommandtext + " and deleted_yn =0"; }
				sqlcommandtext = sqlcommandtext + ";";
				SqlDataReader rdr = null;
				using (SqlConnection conn = new SqlConnection(RO_ConnectionString))
				{
					conn.Open();
					SqlCommand cmd = new SqlCommand(sqlcommandtext, conn);
					cmd.CommandText = sqlcommandtext;
					rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
					if (rdr == null)
					{
						return null;
					}
					
					while (rdr.Read())
					{
						intcsids.Add(rdr.GetInt32(0));
					}
					rdr.Close();
					conn.Close();

					return intcsids;
				}
			}
			catch
			{
				return null;
			}


		}

		public static string GetInternalFieldsAsXML()
		{
			string thisxml;
			try
			{
				using (SqlConnection conn = new SqlConnection(RO_ConnectionString))
				{
					conn.Open();
					SqlCommand cmd = new SqlCommand("select * from internal_fields field for xml auto", conn);
					cmd.CommandText = "select * from internal_fields field for xml auto";
					//System.Xml.XmlReader rdr = cmd.ExecuteXMLReader();
					SqlDataReader rdr = cmd.ExecuteReader();
					thisxml = "";
					if (rdr == null)
					{
						return null;
					}
					while (rdr.Read())
					{
						thisxml = thisxml + rdr.GetString(0);
						//thisxml = thisxml + rdr.ReadOuterXml();
					}
					rdr.Close();
					conn.Close();
					return thisxml;
				}
			}
	
			catch
			{
				return null;
			}


		}

		/// <summary>
		/// Dictionary of ISO 639-1 ID as keys and language name as values.
		/// </summary>
		public Dictionary<string, string> LanguagesFromDB()
		{
			var mapping = new Dictionary<string, string>();
			using (SqlConnection conn = new SqlConnection(ChemSpiderDB.ConnectionString))
			{
				conn.Open();
				string queryString = "select lang_id, name from languages order by name";
				var command = new SqlCommand(queryString, conn);
				var reader = command.ExecuteReader();
				while (reader.Read())
				{
					mapping.Add(reader[0].ToString(), reader[1].ToString());
				}
				conn.Close();
			}
			return mapping;
		}

		public DataSet GetSynonymsDataset(int cmp_id, bool? db_id_yn)
		{
			DataSet ds = new DataSet();

			string xml = string.Empty;
			string schema = string.Empty;
			string add_info = string.Empty;
			string add_info_schema = string.Empty;

			GetSynonymsXMLForRecord(cmp_id, db_id_yn, out xml, out schema, out add_info, out add_info_schema);
			DbUtil.DatasetReadSchema(ds, schema);

			if ( !string.IsNullOrEmpty(xml) ) {
				ds.ReadXml(new StringReader(xml));
			}

			using ( DataSet add_info_ds = new DataSet() )
			{
				DbUtil.DatasetReadSchema(add_info_ds, add_info_schema);

				if (!string.IsNullOrEmpty(add_info))
					add_info_ds.ReadXml(new StringReader(add_info));

				ds.Tables.Add(add_info_ds.Tables["info"].Copy());

				ds.Relations.Add(new DataRelation("syn_info", ds.Tables["synonym"].Columns["syn_id"], ds.Tables["info"].Columns["syn_id"]));
			}

			ds.AcceptChanges();

			return ds;
		}

		public void UpdateSynonymsFromXML(string synonyms_xml, bool deposition)
		{
			using ( SqlCommand cmd = DBU.m_createCommand("UpdateSynonymsFromXML") ) {
				cmd.Parameters.Add("@synonyms_xml", SqlDbType.Xml).Value = synonyms_xml;
				cmd.Parameters.Add("@deposition", SqlDbType.Bit).Value = deposition ? 1 : 0;
				cmd.ExecuteNonQuery();
			}
		}

		public void UpdateSynonymsFromXML(string synonyms_xml)
		{
			UpdateSynonymsFromXML(synonyms_xml, false);
		}
		
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//
		//	Blobs functions...
		//
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void GetBlobsXMLForRecord(int cmp_id, char? blob_type, out string xml, out string schema, out string add_info, out string add_schema)
		{
			Hashtable args = new Hashtable();
			if ( blob_type != null )
				args.Add("blob_type", (char)blob_type);

			GetInfoBoxXMLForRecord(cmp_id, "GetBlobsXMLForRecord", args, out xml, out schema, out add_info, out add_schema, -1);            
		}

		public DataSet GetBlobsDataset(int cmp_id, char? blob_type)
		{
			DataSet ds = new DataSet();

			string xml = string.Empty;
			string schema = string.Empty;
			string add_info = string.Empty;
			string add_schema = string.Empty;

			GetBlobsXMLForRecord(cmp_id, blob_type, out xml, out schema, out add_info, out add_schema);

			DbUtil.DatasetReadSchema(ds, schema);

			if ( !string.IsNullOrEmpty(xml) ) {
				ds.ReadXml(new StringReader(xml));
			}

			ds.AcceptChanges();

			return ds;
		}

		public void UpdateBlobsFromXML(string xml)
		{
			using ( SqlCommand cmd = DBU.m_createCommand("UpdateBlobsFromXML") ) {
				cmd.Parameters.Add("@xml", SqlDbType.Xml).Value = xml;
				cmd.ExecuteNonQuery();
			}
		}

		public bool HasBlobs(int cmp_id, char? blob_type)
		{
			using (SqlConnection conn = new SqlConnection(ChemSpiderDB.ConnectionString))
			{
				return conn.ExecuteScalar<int>("select count(spc_id) from spectra where cmp_id = @cmp_id and (@blob_type is null or blob_type = @blob_type) and deleted_yn = 0", new { cmp_id = cmp_id, blob_type = blob_type }) > 0;
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//
		//	DataSources functions...
		//
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void GetDataSourcesXMLForRecord(int cmp_id, int? dsn_id, int? col_id, out string xml, out string schema, out string add_info, out string add_schema)
		{
			Hashtable args = new Hashtable();
			if ( dsn_id != null )
				args.Add("@dsn_id", (int)dsn_id);
			if ( col_id != null )
				args.Add("@col_id", (int)col_id);

			GetInfoBoxXMLForRecord(cmp_id, "GetDataSourcesXMLForRecord", args, out xml, out schema, out add_info, out add_schema, -1);
		}

		public void GetDataSourcesXMLForRecord2(int cmp_id, int? dsn_id, int? col_id, int? dst_id, out string xml, out string schema, out string add_info, out string add_schema)
		{
			Hashtable args = new Hashtable();
			if (dsn_id != null)
				args.Add("@dsn_id", (int)dsn_id);
			if (col_id != null)
				args.Add("@col_id", (int)col_id);
			if (dst_id != null)
				args.Add("@dst_id", (int)dst_id);

			GetInfoBoxXMLForRecord(cmp_id, "GetDataSourcesXMLForRecord2", args, out xml, out schema, out add_info, out add_schema, -1);
		}

		public DataSet GetDataSourcesDataset(int cmp_id, int? dsn_id, int? col_id)
		{
			DataSet ds = new DataSet();

			string xml = string.Empty;
			string schema = string.Empty;
			string add_info = string.Empty;
			string add_schema = string.Empty;

			GetDataSourcesXMLForRecord(cmp_id, dsn_id, col_id, out xml, out schema, out add_info, out add_schema);
			DbUtil.DatasetReadSchema(ds, schema);

			if ( !string.IsNullOrEmpty(xml) ) {
				ds.ReadXml(new StringReader(xml));
			}

			ds.AcceptChanges();

			return ds;
		}

		public void UpdateDataSourcesFromXML(string xml)
		{
			using ( SqlCommand cmd = DBU.m_createCommand("UpdateDataSourcesFromXML") ) {
				cmd.Parameters.Add("@xml", SqlDbType.Xml).Value = xml;
				cmd.ExecuteNonQuery();
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//
		//	Decsription functions...
		//
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void GetDescriptionXMLForRecord(int cmp_id, int? dscr_id, out string xml, out string schema)
		{
			Hashtable args = new Hashtable();
			if (dscr_id != null)
				args.Add("@elem_id", (int)dscr_id);

			string add_info = string.Empty;
			string add_schema = string.Empty;

			GetInfoBoxXMLForRecord(cmp_id, "GetDescriptionXMLForRecord", args, out xml, out schema, out add_info, out add_schema, -1);
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//
		//	Feedback functions...
		//
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void GetFeedbackXMLForRecord(int cmp_id, out string xml, out string schema)
		{
			Hashtable args = new Hashtable();

			string add_info = string.Empty;
			string add_schema = string.Empty;

			GetInfoBoxXMLForRecord(cmp_id, "GetFeedbackXMLForRecord", args, out xml, out schema, out add_info, out add_schema, -1);
		}

		private string GetDescrFromXML(string xml)
		{
			string html = string.Empty;

			if ( !string.IsNullOrEmpty(xml) ) {
				XmlDocument data = new XmlDocument();
				data.LoadXml(xml);

				XPathNavigator navigator = data.CreateNavigator();

				//XmlNamespaceManager manager = new XmlNamespaceManager(navigator.NameTable);
				//manager.AddNamespace("cs", "http://www.chemspider.com");

				XPathNavigator descr = navigator.SelectSingleNode("/descriptions/description/text");

				if (descr != null)
					html = descr.InnerXml;
			}

			return html;
		}

		public XmlDocument GetDescriptionForRecord(int cmp_id, int dscr_id)
		{
			string html = string.Empty;
			string xml = string.Empty;

			string schema = string.Empty;

			GetDescriptionXMLForRecord(cmp_id, dscr_id, out xml, out schema);

			XmlDocument data = new XmlDocument();
			data.LoadXml(xml);

			return data;
		}

		public void UpdateDescriptionFromXML(string xml)
		{
			Hashtable args = new Hashtable();
			args["@xml"] = xml;

			DBU.m_runSqlProc("UpdateDescriptionFromXML", args, 0);
		}

		public void RemoveDescriptionFromRecord(string xml)
		{
			Hashtable args = new Hashtable();
			args["@xml"] = xml;

			DBU.m_runSqlProc("RemoveDescriptionFromRecord", args, 0);
		}

		public string GetDescriptionRevision(int rev_id)
		{
			string html = string.Empty;

			XmlDocument revXml = GetHistoryBlob(rev_id);
			XmlNode descr = revXml.SelectSingleNode("/descriptions/description/text");

			if (descr != null)
				html = descr.InnerXml;

			return html;
		}

		public XmlDocument GetHistoryBlob(int revId)
		{
			XmlDocument xml = new XmlDocument();

			Hashtable args = new Hashtable();
			args["@rev_id"] = revId;

			using (SqlCommand cmd = DBU.m_createCommand("GetHystoryBlob", args))
			{
				cmd.Parameters.Add("@xml_data", SqlDbType.Xml, int.MaxValue).Direction = ParameterDirection.Output;
				cmd.Parameters.Add("@xml_schema", SqlDbType.Xml, int.MaxValue).Direction = ParameterDirection.Output;

				int res = cmd.ExecuteNonQuery();

				string xmlData = cmd.Parameters["@xml_data"].Value.ToString();
				string xmlSchema = cmd.Parameters["@xml_schema"].Value.ToString();

				if (!string.IsNullOrEmpty(xmlData))
					xml.LoadXml(xmlData);
			}

			return xml;
		}

		public void GetHistoryBlob(int revId, out string xmlData, out string xmlSchema)
		{
			Hashtable args = new Hashtable();
			args["@rev_id"] = revId;

			using ( SqlCommand cmd = DBU.m_createCommand("GetHystoryBlob", args) ) {
				cmd.Parameters.Add("@xml_data", SqlDbType.Xml, int.MaxValue).Direction = ParameterDirection.Output;
				cmd.Parameters.Add("@xml_schema", SqlDbType.Xml, int.MaxValue).Direction = ParameterDirection.Output;

				int res = cmd.ExecuteNonQuery();

				xmlData = cmd.Parameters["@xml_data"].Value.ToString();
				xmlSchema = cmd.Parameters["@xml_schema"].Value.ToString();
			}
		}

		public void CheckInfoBoxes(int cmp_id, out bool focused_libraries, out bool synonyms, out bool db_ids, out bool suppinfo
									, out bool rev_history, out bool feedback, out bool images, out bool description, out bool epi
									, out bool dailymed, out bool wiki, out bool mesh, out bool mesh_supp, out bool lasso
									, out bool chem_vendors)
		{
			focused_libraries = false;
			synonyms = false;
			db_ids = false;
			suppinfo = false;
			rev_history = false;
			feedback = false;
			images = false;
			description = false;
			epi = false;
			dailymed = false;
			wiki = false;
			mesh = false;
			mesh_supp = false;
			lasso = false;
			chem_vendors = false;

			using ( SqlCommand cmd = DBU.m_createCommand("CheckInfoBoxes") ) {
				cmd.Parameters.Add("@cmp_id", SqlDbType.Int).Value = cmp_id;

				using ( SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection) ) {
					if ( reader.Read() ) {
						focused_libraries = (bool)reader["focused_libraries"];
						synonyms = (bool)reader["synonyms"];
						db_ids = (bool)reader["db_ids"];
						suppinfo = (bool)reader["suppinfo"];
						rev_history = (bool)reader["rev_history"];
						feedback = (bool)reader["feedback"];
						images = (bool)reader["images"];
						description = (bool)reader["description"];
						epi = (bool)reader["epi"];
						dailymed = (bool)reader["dailymed"];
						wiki = (bool)reader["wiki"];
						mesh = (bool)reader["mesh"];
						mesh_supp = (bool)reader["mesh_supp"];
						lasso = (bool)reader["lasso"];
						chem_vendors = (bool)reader["chem_vendors"];
					}
				}
			}
		}

		public void GetFeatureIconsInfo(int cmp_id, out bool spectra, out bool wiki)
		{
			spectra = false;
			wiki = false;

			using ( SqlCommand cmd = DBU.m_createCommand("GetFeatureIconsInfo") ) {
				cmd.Parameters.Add("@cmp_id", SqlDbType.Int).Value = cmp_id;

				using ( SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection) ) {
					if ( reader.Read() ) {
						spectra = (bool)reader["has_spectra"];
						wiki = (bool)reader["has_wiki"];
					}
				}
			}
		}

		public void addSpectrum(string spc_type, string file_name, string home_page_url, byte[] file_body, int cmp_id, int usr_id, string comments, bool bOpen)
		{
			Hashtable args = new Hashtable();

			args.Add("@cmp_id", cmp_id);
			args.Add("@usr_id", usr_id);
			args.Add("@blob_type", "S");
			args.Add("@spc_type", spc_type);
			args.Add("@home_page_url", home_page_url);
			args.Add("@filename", file_name);
			args.Add("@comments", comments);
			args.Add("@open_yn", bOpen ? 1 : 0);
			args.Add("@blob", file_body);

			SqlCommand cmd = DBU.m_createCommand("AddBlobToRecord", args);
			cmd.ExecuteNonQuery();
		}

		public void addImage(string file_name, string home_page_url, byte[] file_body, int cmp_id, int usr_id, string comments, bool bOpen)
		{
			Hashtable args = new Hashtable();

			args.Add("@cmp_id", cmp_id);
			args.Add("@usr_id", usr_id);
			args.Add("@blob_type", "I");
			args.Add("@spc_type", "");
			args.Add("@home_page_url", home_page_url);
			args.Add("@filename", file_name);
			args.Add("@comments", comments);
			args.Add("@open_yn", bOpen ? 1 : 0);
			args.Add("@blob", file_body);

			SqlCommand cmd = DBU.m_createCommand("AddBlobToRecord", args);
			cmd.ExecuteNonQuery();
		}

		public void addCIF(string file_name, string home_page_url, byte[] file_body, int cmp_id, int usr_id, string comments, bool bOpen)
		{
			Hashtable args = new Hashtable();

			args.Add("@cmp_id", cmp_id);
			args.Add("@usr_id", usr_id);
			args.Add("@blob_type", "C");
			args.Add("@spc_type", "");
			args.Add("@home_page_url", home_page_url);
			args.Add("@filename", file_name);
			args.Add("@comments", comments);
			args.Add("@open_yn", bOpen ? 1 : 0);
			args.Add("@blob", file_body);

			SqlCommand cmd = DBU.m_createCommand("AddBlobToRecord", args);
			cmd.ExecuteNonQuery();
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//
		//	Compound Feedback functions...
		//
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void leaveCompoundFeedback(int? usr_id, string feedback, string severity, int cmp_id, string email)
		{
			Hashtable ht = new Hashtable();
			ht.Add("@usr_id", usr_id);
			ht.Add("@email", email);
			ht.Add("@feedback", feedback);
			ht.Add("@severity", severity);
			ht.Add("@cmp_id", cmp_id);
			DBU.m_runSqlProc("LeaveCompoundFeedback", ht);
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//
		//	Watch/UnWatch functions...
		//
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool IsWatchedMolecule(int cmp_id, int usr_id)
		{
			bool isWatched = false;

			using ( SqlCommand cmd = DBU.m_createCommand("IsWatchedMolecule") ) {
				cmd.Parameters.Add("@cmp_id", SqlDbType.Int).Value = cmp_id;
				cmd.Parameters.Add("@usr_id", SqlDbType.Int).Value = usr_id;
				cmd.Parameters.Add("@is_watched", SqlDbType.Int).Direction = ParameterDirection.Output;

				int res = cmd.ExecuteNonQuery();

				isWatched = Convert.ToInt32(cmd.Parameters["@is_watched"].Value) != 0;
			}

			return isWatched;
		}

		public void WatchMolecule(int cmp_id, int usr_id)
		{
			Hashtable args = new Hashtable();
			args["@cmp_id"] = cmp_id;
			args["@usr_id"] = usr_id;
			using ( SqlCommand cmd = DBU.m_createCommand("WatchMolecule", args, CommandType.StoredProcedure) ) {
				cmd.ExecuteNonQuery();
			}
		}

		public void UnWatchMolecule(int cmp_id, int usr_id)
		{
			Hashtable args = new Hashtable();
			args["@cmp_id"] = cmp_id;
			args["@usr_id"] = usr_id;
			using ( SqlCommand cmd = DBU.m_createCommand("UnWatchMolecule", args, CommandType.StoredProcedure) ) {
				cmd.ExecuteNonQuery();
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//
		//	Depricate/Undepricate Compound functions...
		//
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void DepricateCompound(int cmp_id, int user_id, bool depricate)
		{
			using ( SqlCommand cmd = DBU.m_createCommand("DepricateCompound") ) {
				cmd.Parameters.Add("@cmp_id", SqlDbType.Int).Value = cmp_id;
				cmd.Parameters.Add("@usr_id", SqlDbType.Int).Value = user_id;
				cmd.Parameters.Add("@depricated_yn", SqlDbType.Bit).Value = depricate;

				int res = cmd.ExecuteNonQuery();
			}
		}

		public bool is_service_enabled(string subsystem, string service)
		{
			Hashtable args = new Hashtable();
			args.Add("@subsystem", subsystem);
			args.Add("@service", service);
			object o = DBU.m_runSqlProc("is_service_enabled", args, "result");
			if ( o is DBNull )
				return false;
			return (int)o == 1;
		}

		public struct PrePublishRecord
		{
			public int rec_id;
			public int cmp_id;
			public bool has_sdf;
			public bool has_sdf_3d;
			public bool has_image;
			public int sub_id;
		}

		public PrePublishRecord dep_prepublish(string inchi_key, int dfid, string ext_id, string ext_url, string pubdate, string record_xml, bool recSuppInfo, 
											   bool recSynonyms, bool has_articles)
		{
			PrePublishRecord result = new PrePublishRecord();
			Hashtable args = new Hashtable();
			args["@inchi_key"] = inchi_key;
			args["@dfid"] = dfid;
			args["@record_xml"] = record_xml;
			args["@ext_id"] = ext_id;
			args["@ext_url"] = ext_url;
			args["@pubdate"] = pubdate;
			args["@has_supp_info"] = recSuppInfo;
			args["@has_synonyms"] = recSynonyms;
			args["@has_articles"] = has_articles;
			using (SqlDataReader reader = DBU.m_executeReader("ChemSpiderStaging..dep_prepublish", args)) {
				if ( reader.Read() ) {
					result.rec_id = (int)reader["rec_id"];
					result.cmp_id = (int)reader["cmp_id"];
					result.has_sdf = (bool)reader["has_sdf"];
					result.has_sdf_3d = (bool)reader["has_sdf_3d"];
					result.has_image = (bool)reader["has_image"];
					result.sub_id = (int)reader["sub_id"];
				}
			}

			return result;
		}

		public void dep_set_image(int rec_id, byte[] preview, bool create_only)
		{
			Hashtable args = new Hashtable();
			args["@rec_id"] = rec_id;
			args["@image"] = preview;
			args["@createonly"] = create_only;
			DBU.m_runSqlProc("ChemSpiderStaging..dep_set_image", args);
		}

		public void dep_set_sdf(int rec_id, string sdf, string v3000sdf, bool create_only)
		{
			byte[] mol = ZipUtils.zip_sdf(sdf);
			byte[] molv3000 = null;

			Hashtable args = new Hashtable();
			args["@rec_id"] = rec_id;
			args["@sdf"] = mol;
			if (!String.IsNullOrEmpty(v3000sdf))
			{
				molv3000 = ZipUtils.zip_sdf(v3000sdf);
				args["@sdf_v3000"] = molv3000;
			}
			args["@createonly"] = create_only;
			DBU.m_runSqlProc("ChemSpiderStaging..dep_set_sdf", args);
		}

		public void dep_set_sdf_3d(int rec_id, string sdf, bool create_only)
		{
			byte[] mol = ZipUtils.zip_sdf(sdf);

			Hashtable args = new Hashtable();
			args["@rec_id"] = rec_id;
			args["@sdf"] = mol;
			args["@createonly"] = create_only;
			DBU.m_runSqlProc("ChemSpiderStaging..dep_set_sdf_3d", args);
		}

		public void dep_set_inchis(int rec_id, string inchi, string inchi_mf, string inchi_key_1, byte[] inchi_md5, byte[] inchi_c_md5,
								   byte[] inchi_ch_md5, byte[] inchi_chsi_md5, bool multicomponent_yn, bool isotopic_yn)
		{
			Hashtable args = new Hashtable();
			args["@rec_id"] = rec_id;
			args["@inchi"] = inchi;
			args["@inchi_mf"] = inchi_mf;
			args["@inchi_key_1"] = inchi_key_1;
			args["@inchi_md5"] = inchi_md5;
			args["@inchi_c_md5"] = inchi_c_md5;
			args["@inchi_ch_md5"] = inchi_ch_md5;
			args["@inchi_chsi_md5"] = inchi_chsi_md5;
			args["@multicomponent_yn"] = multicomponent_yn;
			args["@isotopic_yn"] = isotopic_yn;
			DBU.m_runSqlProc("ChemSpiderStaging..dep_set_inchis", args);
		}

		public void dep_set_inchis_std(int rec_id, string inchi, string inchi_key, string inchi_key_a, string inchi_key_b, byte[] inchi_md5, byte[] inchi_mf_md5,
									   byte[] inchi_c_md5, byte[] inchi_ch_md5, byte[] inchi_chsi_md5)
		{
			Hashtable args = new Hashtable();
			args["@rec_id"] = rec_id;
			args["@inchi"] = inchi;
			args["@inchi_key"] = inchi_key;
			args["@inchi_key_a"] = inchi_key_a;
			args["@inchi_key_b"] = inchi_key_b;
			args["@inchi_md5"] = inchi_md5;
			args["@inchi_mf_md5"] = inchi_mf_md5;
			args["@inchi_c_md5"] = inchi_c_md5;
			args["@inchi_ch_md5"] = inchi_ch_md5;
			args["@inchi_chsi_md5"] = inchi_chsi_md5;
			DBU.m_runSqlProc("ChemSpiderStaging..dep_set_inchis_std", args);
		}

		public void dep_set_oechem_name(int rec_id, string name, bool blah)
		{
			Hashtable args = new Hashtable();
			args["@rec_id"] = rec_id;
			args["@name"] = name;
			args["@blah"] = blah;
			DBU.m_runSqlProc("ChemSpiderStaging..dep_set_oechem_name", args);
		}

		public void dep_publish_file(int dfid)
		{
			Hashtable args = new Hashtable();
			args["@dfid"] = dfid;
			DBU.m_runSqlProc("ChemSpiderStaging..dep_publish_file", args);
		}

		public void dep_clear_file(int dfid)
		{
			Hashtable args = new Hashtable();
			args["@dfid"] = dfid;
			DBU.m_runSqlProc("ChemSpiderStaging..dep_clear_file", args);
		}

		public void dep_clear(int dep_id)
		{
			Hashtable args = new Hashtable();
			args["@dep_id"] = dep_id;
			DBU.m_runSqlProc("ChemSpiderStaging..dep_clear", args);
		}

		public void dep_publish_substances_info(int dfid, bool update_supp_info)
		{
			Hashtable args = new Hashtable();
			args.Add("@dfid", dfid);
			args.Add("@update_supp_info", update_supp_info);
			DBU.m_runSqlProc("ChemSpiderStaging..dep_publish_substances_info", args);
		}

		public void dep_set_file_records_count(int file_id, int nrecords)
		{
			Hashtable args = new Hashtable();
			args["@dfid"] = file_id;
			args["@n"] = nrecords;
			DBU.m_executeUpdate("update depositions_files set records = @n where dfid = @dfid", args);
		}

		public static byte[] getBlob(int blob_id, ref string fileName, ref string blobType)
		{
			using ( SqlConnection conn = new SqlConnection(ConnectionString) ) {
				try {
					conn.Open();
					SqlCommand cmd = new SqlCommand("select spectrum, filename, blob_type from spectra where spc_id = " + blob_id.ToString(), conn);
					SqlDataReader reader = cmd.ExecuteReader();
					if ( reader.Read() ) {
						fileName = reader["filename"].ToString();
						blobType = reader["blob_type"].ToString();
						return reader["spectrum"] as byte[];
					}

					return new byte[0];
				}
				finally {
					conn.Close();
				}
			}
		}

		public static string getCompoundsSuppInfo(int id)
		{
			object supp_info = s_csdb.DBU.m_querySingleValue("select supp_info from compounds_supp_info where cmp_id = " + id);
			if ( supp_info != null ) {
				return supp_info.ToString();
			}

			return string.Empty;
		}

		static public void createSubscription(string full_name, string email)
		{
			Hashtable ht = new Hashtable();
			ht.Add("@full_name", full_name);
			ht.Add("@email", email);
			using ( SqlCommand cmd = s_csdb.DBU.m_createCommand("CreateSubscription", ht) ) {
				try {
					cmd.ExecuteNonQuery();
				}
				finally {
					cmd.Connection.Close();
				}
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//
		//	Data Slices functions...
		//
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public int CreateDataSlice(int usr_id, string name, string description, string sec_type, string access_type, string query, string listDef, int molOfInterest)
		{
			using (SqlCommand cmd = DBU.m_createCommand("UpdateDataSlice"))
			{
				SqlParameter idParam = new SqlParameter("@id", SqlDbType.Int);
				idParam.Direction = ParameterDirection.InputOutput;
				idParam.Value = DBNull.Value;

				cmd.Parameters.Add(idParam);
				cmd.Parameters.Add("@usr_id", SqlDbType.Int).Value = usr_id;
				cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = name;
				cmd.Parameters.Add("@description", SqlDbType.VarChar).Value = description;
				cmd.Parameters.Add("@sec_type", SqlDbType.Char).Value = sec_type;
				cmd.Parameters.Add("@acc_type", SqlDbType.Char).Value = access_type;
				cmd.Parameters.Add("@query", SqlDbType.NVarChar).Value = query;
				cmd.Parameters.Add("@listDef", SqlDbType.NVarChar).Value = listDef;
				cmd.Parameters.Add("@mol_of_interest_yn", SqlDbType.Bit).Value = molOfInterest;

				int res = cmd.ExecuteNonQuery();

				return (int)cmd.Parameters["@id"].Value;
			}
		}

		public DataRow GetDataSlice(int sec_id)
		{
			Hashtable args = new Hashtable();
			args.Add("@id", sec_id);

			DataSet ds = DBU.m_fillDataset("GetDataSlice", args);
			DataTable table = ds.Tables[0];

			if (table.Rows.Count != 1)
				return null;

			return table.Rows[0];
		}

		public int UpdateDataSlice(int sec_id, string name, string description, string sec_type, string access_type, string query, string listDef, int molOfInterest)
		{
			using (SqlCommand cmd = DBU.m_createCommand("UpdateDataSlice"))
			{
				cmd.Parameters.Add("@id", SqlDbType.Int).Value = sec_id;
				cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = name;
				cmd.Parameters.Add("@description", SqlDbType.VarChar).Value = description;
				cmd.Parameters.Add("@sec_type", SqlDbType.Char).Value = sec_type;
				cmd.Parameters.Add("@acc_type", SqlDbType.Char).Value = access_type;
				cmd.Parameters.Add("@query", SqlDbType.NVarChar).Value = query;
				cmd.Parameters.Add("@listDef", SqlDbType.NVarChar).Value = listDef;
				cmd.Parameters.Add("@mol_of_interest_yn", SqlDbType.Bit).Value = molOfInterest;

				int res = cmd.ExecuteNonQuery();

				return (int)cmd.Parameters["@id"].Value;
			}
		}

		public void DeleteDataSlice(int sec_id)
		{
			using (SqlCommand cmd = DBU.m_createCommand("DeleteDataSlice"))
			{
				cmd.Parameters.Add("@id", SqlDbType.Int).Value = sec_id;

				int res = cmd.ExecuteNonQuery();
			}
		}

		public DataTable SearchDataSlices(int? usr_id, string filter, string type, string access_type, bool? mol_of_interest)
		{
			Hashtable args = new Hashtable();
			if(usr_id != null) args.Add("@usr_id", usr_id);
			if(!string.IsNullOrEmpty(filter)) args.Add("@filter", filter);
			if(!string.IsNullOrEmpty(type)) args.Add("@sec_type", type);
			if(!string.IsNullOrEmpty(access_type)) args.Add("@acc_type", access_type);
			if (mol_of_interest != null) args.Add("@mol_of_interest_yn", (bool)mol_of_interest ? 1 : 0);

			DataSet ds = DBU.m_fillDataset("SearchDataSlices", args);
			return ds.Tables[0];
		}

		public void AddRecordToDataSlice(int cmp_id, int sec_id)
		{
			using (SqlCommand cmd = DBU.m_createCommand("AddRecordToDataSlice"))
			{
				cmd.Parameters.Add("@cmp_id", SqlDbType.Int).Value = cmp_id;
				cmd.Parameters.Add("@sec_id", SqlDbType.Int).Value = sec_id;

				int res = cmd.ExecuteNonQuery();
			}
		}

		public string GetCompoundTitle(int cmp_id)
		{
			object o = DBU.m_querySingleValue(String.Format("select dbo.fGetCompoundTitle({0})", cmp_id));
			if (o is DBNull)
			{
				o = DBU.m_querySingleValue(String.Format("select dbo.fGetSysName({0})", cmp_id));
			}
			if (o is DBNull)
			{
				o = Convert.ToString(cmp_id);
			}
			return o.ToString();
		}

		//Jon Steele 27-Oct-2010 - Validate that the entered cs_id is valid when proposing to merge records.
		public bool isValidCmpId(int cmp_id)
		{
			bool isValid = false;
			using (SqlCommand cmd = DBU.m_createCommand("IsValidCmpId"))
			{
				cmd.Parameters.Add("@cmp_id", SqlDbType.Int).Value = cmp_id;
				cmd.Parameters.Add("@is_valid", SqlDbType.Int).Direction = ParameterDirection.Output;
				
				int res = cmd.ExecuteNonQuery();

				isValid = Convert.ToInt32(cmd.Parameters["@is_valid"].Value) != 0;
			}

			return isValid;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//
		//	Data Export functions...
		//
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public int CreateDataExport(string name, string description, string format, int batch_size, bool new_folder_yn, string path, XmlDocument columns, string where)
		{
			return DBU.ExecuteScalar<int>("exec UpdateDataExport @id, @name, @description, @format, @batch_size, @new_folder_yn, @path, @columns, @where", 
						new {   id = DBNull.Value, name = name, description = description, format = 
								format, batch_size = batch_size, new_folder_yn = new_folder_yn, path = path, columns = columns.InnerXml, where = where });
		}

		public DataRow GetDataExport(int id)
		{
			DataTable table = DBU.FillDataTable("exec GetDataExport @id", new { id = id });

			if (table.Rows.Count != 1)
				return null;

			return table.Rows[0];
		}

		public int UpdateDataExport(int exp_id, string name, string description, string format, int batch_size,  bool new_folder_yn, string path, XmlDocument columns, string where)
		{
			return DBU.ExecuteScalar<int>("exec UpdateDataExport @id, @name, @description, @format, @batch_size, @new_folder_yn, @path, @columns, @where", 
											new {   id = exp_id, name = name, description = description, format = format, 
													batch_size = batch_size, new_folder_yn = new_folder_yn, path = path, columns = columns.InnerXml, where = where });
		}

		public void AssignDataExportJob(int exp_id, int job_id)
		{
			DBU.ExecuteCommand("exec AssignDataExportJob @exp_id, @job_id", new { exp_id = exp_id, job_id = job_id });
		}

		public void DeleteDataExport(int id)
		{
			DBU.ExecuteCommand("exec DeleteDataExport @id", new { id = id });
		}

		public int CheckDataExport(string where)
		{
			return DBU.ExecuteScalar<int>("exec CheckDataExport @where", new { where = where });
		}

		public List<int> ExecuteDataExport(int exp_id)
		{
			return DBU.FetchColumn<int>("exec ExecuteDataExport @exp_id", new { exp_id = exp_id });
		}

		public DataTable GetExportData(int exp_id, string ids)
		{
			return DBU.FillDataTable("exec GetExportData @exp_id, @ids", new { exp_id = exp_id, ids = ids });
		}

		/// <summary>
		/// function which will quite often set the input parameter newname as the name at the top of the ChemSpider page for the compound with CSID cmp_id
		/// If the name doesn't exist for the compound already it will be added
		/// if the name does exist its flags (and if necessar yteh capitilization of the name) will be fixed:
		/// in the synonyms table these flags are set: chemical_name_yn = 1, checked_yn = 1, deleted_yn = 0, lang_id = 'E', date_updated, and wiki_yn depending on the input parameter of this program
		/// in the compounds_synonyms table these flags are set: approved_yn = 1, opinion = 'Y', common_name_yn = 1, date_created, usr_id, cur_id
		/// </summary>
		/// <param name="cmp_id">CSID of compound to get new name</param>
		/// <param name="newname">new name</param>
		/// <param name="userid">the id of the user who will be marked as adding the new name</param>
		/// <param name="setwikiflag">if set to true the new name will marked with a wiki flag which makes it more likely to be the top name. Is not appropriate to flag this as true if it doesn't correspond to e.g. a name in Wikipedia though</param>
		public void SetCompoundTitle(int cmp_id, string newname, int userid, Boolean setwikiflag)
		{
			DBU.ExecuteCommand("exec cmp_set_title @cmp_id, @newname, @userid, @setwikiflag", new { cmp_id = cmp_id, newname = newname, userid = userid, setwikiflag = setwikiflag });
		}

		/// <summary>
		/// Updates external link of compound with CSID cmp_id and datasource ID dsn_id from a value of old_ext_url so that its ext_id and ext_url are set to new_ext_id and new_ext_url respectively
		/// </summary>
		/// <param name="cmp_id">CSID of compound whose external link will be fixed</param>
		/// <param name="old_ext_url">url of old link to fix (if blank then a new link is made)</param>
		/// <param name="new_ext_id">id of new new link (old link is just deleted if this is blank)</param>
		/// <param name="new_ext_url">url of new new link (old link is just deleted if this is blank)</param>
		/// <param name="dsn_id">data source if of compound whose external link will be fixed</param>
		public void UpdateExternalLink(int cmp_id, string old_ext_url, string new_ext_id, string new_ext_url, int dsn_id)
		{
			DBU.ExecuteCommand("exec sub_update_external_link @cmp_id, @old_ext_url, @new_ext_id, @new_ext_url, @dsn_id",
									new
									{
										cmp_id = cmp_id,
										old_ext_url = old_ext_url,
										new_ext_id = new_ext_id,
										new_ext_url = new_ext_url,
										dsn_id = dsn_id
									});
		}

		public void UpdateCompoundHits(int src_id, int cmp_id, int hits)
		{
			DBU.ExecuteCommand("exec UpdateCompoundHits @src_id, @cmp_id, @hits", new { src_id = src_id, cmp_id = cmp_id, hits = hits });
		}

		public void DeleteSubstance(int sub_id)
		{
			DBU.ExecuteCommand("exec DeleteSubstance", new { sub_id = sub_id });
		}

		public int RegisterIncomingLink(string token, int cmp_id, string url, string eid)
		{ 
			return DBU.ExecuteScalar<int?>("exec RegisterIncomingLink", new { token = token, cmp_id = cmp_id, url = url, eid = eid }) ?? -1; 
		}
	}
}
