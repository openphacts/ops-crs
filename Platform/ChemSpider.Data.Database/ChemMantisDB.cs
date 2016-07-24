using System;
using System.Collections.Generic;
using System.Collections;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data;
using ChemSpider.Utilities;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using ChemSpider.Database;

namespace ChemSpider.Data.Database
{
	public class ChemMantisDB : ChemSpiderBaseDB
	{
		protected override string ConnString
		{
			get
			{
				return ConfigurationManager.ConnectionStrings["ChemMantisConnectionString"] == null ?
					null :
					ConfigurationManager.ConnectionStrings["ChemMantisConnectionString"].ConnectionString;
			}
		}

		protected override string RO_ConnString
		{
			get
			{
				return ConfigurationManager.ConnectionStrings["ChemMantisROConnectionString"] == null ?
					ConnString :
					ConfigurationManager.ConnectionStrings["ChemMantisROConnectionString"].ConnectionString;
			}
		}

        protected override string DatabaseName
        {
            get { return "ChemMantis"; }
        }

		public ChemMantisDB()
		{
			
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//
		//	Articles functions...
		//
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void AddEntity(int art_id, string ent_id, string ent_type, string ent_text, string ent_html, short extractor, out int group_id)
		{
			Hashtable args = new Hashtable();
			args["@art_id"] = art_id;
			args["@ent_id"] = ent_id;
			args["@ent_type"] = ent_type;
			args["@ent_text"] = ent_text;
			args["@ent_html"] = ent_html;
			if ( extractor > 0 )
				args["@extractor"] = extractor;
			using ( SqlCommand cmd = DBU.m_createCommand("AddEntity", args, CommandType.StoredProcedure) ) {
				try {
					cmd.Parameters.Add("@group_id", SqlDbType.Int).Direction = ParameterDirection.Output;
					cmd.ExecuteNonQuery();
					Int32.TryParse(cmd.Parameters["@group_id"].Value.ToString(), out group_id);
				}
				finally {
					DBU.CloseConn(cmd);
				}
			}
		}

		public void GetEntity(int art_id, int? rev_id, string ent_id, out string ent_type, out string ent_text, out string ent_html)
		{
			Hashtable args = new Hashtable();
			args["@art_id"] = art_id;
			args["@rev_id"] = rev_id;
			args["@ent_id"] = ent_id;
			using ( SqlCommand cmd = DBU.m_createCommand("GetEntity", args, CommandType.StoredProcedure) ) {
				try {
					SqlDataReader reader = cmd.ExecuteReader();
					if ( !reader.Read() )
						throw new Error("Cannot retrieve entity data");

					ent_type = reader["ent_type"] as string;
					ent_text = reader["entity_text"] as string;
					ent_html = reader["entity_html"] as string;
				}
				finally {
					DBU.CloseConn(cmd);
				}
			}
		}

		public void SetEntityAttributes(int article_id, string entity_id, string attrs_xml)
		{
			Hashtable args = new Hashtable();
			args["@art_id"] = article_id;
			args["@ent_id"] = entity_id;
			args["@attrs_xml"] = @attrs_xml;
			using ( SqlCommand cmd = DBU.m_createCommand("SetEntityAttributes", args, CommandType.StoredProcedure) ) {
				cmd.ExecuteNonQuery();
			}
		}

		public void GetEntityAttributes(int article_id, int? rev_id, string entity_id, out string attrs_xml)
		{
			SqlCommand cmd = null;
			try {
				using ( cmd = DBU.m_createCommand("GetEntityAttributes") ) {
					cmd.Parameters.Add("@art_id", SqlDbType.Int).Value = article_id;
					cmd.Parameters.Add("@rev_id", SqlDbType.Int).Value = rev_id == null ? SqlInt32.Null : (int)rev_id;
					cmd.Parameters.Add("@ent_id", SqlDbType.VarChar).Value = entity_id;
					cmd.Parameters.Add("@attrs_xml", SqlDbType.Xml, int.MaxValue).Direction = ParameterDirection.Output;

					int res = cmd.ExecuteNonQuery();

    			    attrs_xml = cmd.Parameters["@attrs_xml"].Value != null ? cmd.Parameters["@attrs_xml"].Value.ToString() : null;
				}
			}
			finally {
				DBU.CloseConn(cmd);
			}
		}

		public void ClearEntity(int article_id, int? rev_id, string entity_id)
		{
			Hashtable args = new Hashtable();
			args["@art_id"] = article_id;
			args["@rev_id"] = rev_id;
			args["@ent_id"] = entity_id;
			using ( SqlCommand cmd = DBU.m_createCommand("ClearEntity", args, CommandType.StoredProcedure) ) {
				cmd.ExecuteNonQuery();
			}
		}

		public void AddEntityToList(string term, string short_list_name, string category, int usr_id)
		{
			Hashtable args = new Hashtable();
			args["@term"] = term;
			args["@short_list_name"] = short_list_name;
			args["@category"] = category;
			args["@usr_id"] = usr_id;
			using ( SqlCommand cmd = DBU.m_createCommand("AddEntityToList", args, CommandType.StoredProcedure) ) {
				cmd.ExecuteNonQuery();
			}
		}

		public void ImportEntityList(List<string> list, string category, string short_list_name, int usr_id)
		{
			Hashtable args = new Hashtable();
			args["@list"] = list;
			args["@short_list_name"] = short_list_name;
			args["@category"] = category;
			args["@usr_id"] = usr_id;
			using ( SqlCommand cmd = DBU.m_createCommand("ImportEntityList", args, CommandType.StoredProcedure) ) {
				cmd.ExecuteNonQuery();
			}
		}

		public string GetArticleTitle(int art_id)
		{
			string id, title, authors, address, history, article_urn, datasource, corr_author, corr_author_address, organizations, art_abstract, tags;
			int iss_no;
			DateTime embargo_date;

			GetArticleProperties(art_id, out id, out title, out authors, out address, out history, out article_urn, out embargo_date,
									out datasource, out corr_author, out corr_author_address, out organizations, out art_abstract, out iss_no, out tags);
			return title;
		}

        public void GetArticleProperties(int art_id, out string id, out string title, out string authors, out string address,
                                            out string history, out string article_urn, out DateTime embargo_date, out string datasource,
                                            out string corr_author, out string corr_author_address, out string organizations, out string art_abstract,
                                            out int iss_no, out string tags)
        {
            id = string.Empty;
            title = string.Empty;
            authors = string.Empty;
            address = string.Empty;
            history = string.Empty;
            article_urn = string.Empty;
            embargo_date = DateTime.MinValue;
            datasource = string.Empty;
            corr_author = string.Empty;
            corr_author_address = string.Empty;
            organizations = string.Empty;
            art_abstract = string.Empty;
            tags = string.Empty;
            iss_no = 0;

            Hashtable args = new Hashtable();
            args.Add("@art_id", art_id);

            using (SqlDataReader reader = DBU.m_executeReader("GetArticleProperties", args)) {	// CAUTION: was CommandBehavior.CloseConnection
                if (reader.Read()) {
                    id = reader["id"].ToString();
                    title = reader["title"].ToString();
                    authors = reader["authors"].ToString();
                    address = reader["address"].ToString();
                    history = reader["history"].ToString();
                    article_urn = reader["article_urn"].ToString();
                    //                    DateTime embargo_date = null;
                    //                    datasource = reader[""].ToString();
                    corr_author = reader["corr_author"].ToString();
                    corr_author_address = reader["corr_author_address"].ToString();
                    organizations = reader["organizations"].ToString();
                    art_abstract = reader["abstract"].ToString();
                    iss_no = (int)reader["iss_no"];
                    tags = reader["tags"].ToString();
                }
            }
        }

		public void GetArticleExport(int art_id, int? rev_id, out string xml, out string schema, int usr_id)
		{
			SqlCommand cmd = null;
			try {
				using ( cmd = DBU.m_createCommand("GetArticleExport") ) {
					cmd.Parameters.Add("@art_id", SqlDbType.Int).Value = art_id;
					cmd.Parameters.Add("@rev_id", SqlDbType.Int).Value = rev_id == null ? SqlInt32.Null : (int)rev_id;
					cmd.Parameters.Add("@xml_data", SqlDbType.Xml, int.MaxValue).Direction = ParameterDirection.Output;
					cmd.Parameters.Add("@xml_schema", SqlDbType.Xml, int.MaxValue).Direction = ParameterDirection.Output;
					cmd.Parameters.Add("@usr_id", SqlDbType.Int).Value = usr_id;

					int res = cmd.ExecuteNonQuery();

					xml = cmd.Parameters["@xml_data"].Value.ToString();
					schema = cmd.Parameters["@xml_schema"].Value.ToString();
				}
			}
			finally {
				DBU.CloseConn(cmd);
			}
		}

		public void GetArticleEntitiesXML(int art_id, out string xml)
		{
			SqlCommand cmd = null;
			try {
				using ( cmd = DBU.m_createCommand("GetArticleEntitiesXML") ) {
					cmd.Parameters.Add("@art_id", SqlDbType.Int).Value = art_id;
					cmd.Parameters.Add("@xml_data", SqlDbType.Xml, int.MaxValue).Direction = ParameterDirection.Output;

					int res = cmd.ExecuteNonQuery();

					xml = cmd.Parameters["@xml_data"].Value.ToString();
				}
			}
			finally {
				DBU.CloseConn(cmd);
			}
		}

		public void SetArticleStatus(int art_id, string status, int usr_id)
		{
			Hashtable args = new Hashtable();
			args["@art_id"] = art_id;
			args["@status"] = status;
			args["@usr_id"] = usr_id;
            DataTable dt = DBU.m_fillDataTableProc("SetArticleStatus", args);
            if (dt.Rows.Count > 0) {
                System.Text.StringBuilder sb = new System.Text.StringBuilder("Article validation error:\n");
                foreach (DataRow dr in dt.Rows) {
                    sb.AppendFormat("{0}: {1}\n", dr["display_name"], dr["msg"]);
                }
                throw new Exception(sb.ToString());
            }
		}

		public void AddArticleComment(int art_id, int usr_id, string subject, string comment, string severity, int parent_id /* -1 = NULL */, string author_name, string author_email, bool notify)
		{
			Hashtable args = new Hashtable();
			args["@art_id"] = art_id;
			args["@usr_id"] = usr_id;
			args["@subject"] = subject;
			args["@comment"] = comment;
			args["@severity"] = severity;
			if ( !string.IsNullOrEmpty(author_email) )
				args["@author_email"] = author_email;
			if ( !string.IsNullOrEmpty(author_name) )
				args["@author_name"] = author_name;
			if ( parent_id > 0 )
				args["@parent_id"] = parent_id;
			else
				args["@parent_id"] = null;
			args["@notify"] = notify;
			using ( SqlCommand cmd = DBU.m_createCommand("AddArticleComment", args, CommandType.StoredProcedure) ) {
				cmd.ExecuteNonQuery();
			}
		}

		public string GetArticleCommentsXml(int art_id, int filter_usr_id, string filter_status)
		{
			using ( SqlCommand cmd = DBU.m_createCommand("GetArticleCommentsXml") ) {
				try {
					cmd.Parameters.Add("@art_id", SqlDbType.Int).Value = art_id;
					if ( !String.IsNullOrEmpty(filter_status) )
						cmd.Parameters.Add("@filter_status", SqlDbType.VarChar, 10).Value = filter_status;
					if ( filter_usr_id > 0 )
						cmd.Parameters.Add("@filter_usr_id", SqlDbType.Int).Value = filter_usr_id;
					cmd.Parameters.Add("@xml", SqlDbType.Xml, int.MaxValue).Direction = ParameterDirection.Output;

					int res = cmd.ExecuteNonQuery();

					return cmd.Parameters["@xml"].Value.ToString();
				}
				finally {
					DBU.CloseConn(cmd);
				}
			}
		}

		public void SetArticleCommentStatus(int com_id, string status, int usr_id, string message)
		{
			Hashtable args = new Hashtable();
			args["@com_id"] = com_id;
			args["@status"] = status;
			args["@usr_id"] = usr_id;
            args["@message"] = message;
			using ( SqlCommand cmd = DBU.m_createCommand("SetArticleCommentStatus", args, CommandType.StoredProcedure) ) {
				cmd.ExecuteNonQuery();
			}
		}

		public List<string> GetArticleActions(int usr_id, int art_id)
		{
			List<string> result = new List<string>();
			Hashtable args = new Hashtable();
			args["@art_id"] = art_id;
			args["@usr_id"] = usr_id;
            DataTable t = DBU.m_fillDataTableProc("GetArticleActions", args);
            foreach (DataRow r in t.Rows) {
                result.Add((string)r[0]);
            }

			return result;
		}

        public int CreateArticle(string article, bool bPrivate, int? dsn_id, int usr_id)
        {
            Hashtable args = new Hashtable();
            if (article != null)
                args.Add("article", ZipUtils.gzip(article, System.Text.Encoding.UTF8));
            args.Add("private_yn", bPrivate);
            args.Add("dsn_id", dsn_id);
            args.Add("usr_id", usr_id);
            return Convert.ToInt32(DBU.m_runSqlProc("CreateArticle", args, 0));
        }

		public int CreateArticleImage(int art_id, string image_name, byte[] image_content)
		{
			Hashtable args = new Hashtable();
			args.Add("art_id", art_id);
			args.Add("image_name", image_name);
			args.Add("image_content", image_content);
			string ext;
			string image_type = MimeUtils.decodeMimeTypeByFileExt(image_name, out ext);
			args.Add("image_type", image_type);
			return Convert.ToInt32(DBU.m_runSqlProc("CreateArticleImage", args, 0));
		}

		public void UpdateArticle(int art_id, string article, string action_type, int usr_id)
		{
			Hashtable args = new Hashtable();
			args.Add("art_id", art_id);
			args.Add("article", ZipUtils.gzip(article, System.Text.Encoding.UTF8));
			args.Add("action_type", action_type);
			args.Add("usr_id", usr_id);
			DBU.m_runSqlProc("UpdateArticle", args);
		}

        public int SaveArticleSnapshot(int art_id, int usr_id, string alias, string comments)
        {
            Hashtable args = new Hashtable();
            args.Add("art_id", art_id);
            args.Add("usr_id", usr_id);
            args.Add("alias", alias);
            args.Add("comments", comments);
            return Convert.ToInt32(DBU.m_runSqlProc("SaveArticleSnapshot", args, 0));
        }

		public void RestoreArticleSnapshot(int art_id, int revision, int usr_id)
		{
			Hashtable args = new Hashtable();
			args.Add("art_id", art_id);
			args.Add("revision", revision);
			args.Add("usr_id", usr_id);
			DBU.m_runSqlProc("RestoreArticleSnapshot", args);
		}

		public void DeleteArticleSnapshot(int art_id, int revision)
		{
			Hashtable args = new Hashtable();
			args.Add("art_id", art_id);
			args.Add("revision", revision);
			DBU.m_runSqlProc("DeleteArticleSnapshot", args);
		}

		public void AddPublicationAlert(string email)
		{
			Hashtable args = new Hashtable();
			args.Add("email", email);
			DBU.m_runSqlProc("AddPublicationALert", args);
		}
	}

    [Serializable]
    [XmlType(TypeName = "author")]
    public class ArticleAuthor
    {
        [XmlAttribute]
        public string fname = string.Empty;
        [XmlAttribute]
        public string lname = string.Empty;
        [XmlAttribute]
        public string organization = string.Empty;
        [XmlAttribute]
        public string addr = string.Empty;
        [XmlAttribute]
        public bool first_yn = false;
        [XmlAttribute]
        public bool corresponding_yn = false;
        [XmlAttribute]
        public int ordinal = 0;
        [XmlAttribute]
        public bool is_primary_submitter = false;
    }

    [Serializable]
    [System.Xml.Serialization.XmlRootAttribute("authors")]
    public class ArticleAuthors
    {
        [System.Xml.Serialization.XmlElementAttribute("author")]
        public ArticleAuthor[] authors;
    }

    public class ArticleProperties
    {
        public int art_id;
        public int usr_id = -1;
        public string title = string.Empty;
        public string id = string.Empty;
        public string url = string.Empty;
        public DateTime embargo = DateTime.MinValue;
        public int dsn_id = -1;
        public string tags = string.Empty;
        public string status = string.Empty;
        public List<ArticleAuthor> authors = new List<ArticleAuthor>();
        public bool abstracted = false;
        public string doi = string.Empty;
        public DateTime? published_date;

        public ArticleProperties()
        {
        }

        public ArticleProperties(int a_art_id)
        {
            art_id = a_art_id;
        }
    }

    class XmlFragmentWriter : XmlTextWriter
    {
        public XmlFragmentWriter(TextWriter w) : base(w) { }
        public XmlFragmentWriter(Stream w, System.Text.Encoding encoding) : base(w, encoding) { }
        public XmlFragmentWriter(string filename, System.Text.Encoding encoding) :
            base(new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None), encoding) { }

        bool _skip = false;

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            // STEP 1 - Omits XSD and XSI declarations.
            // From Kzu - http://weblogs.asp.net/cazzu/archive/2004/01/23/62141.aspx
            if (prefix == "xmlns" && (localName == "xsd" || localName == "xsi")) {
                _skip = true;
                return;
            }
            base.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteString(string text)
        {
            if (_skip) return;
            base.WriteString(text);
        }

        public override void WriteEndAttribute()
        {
            if (_skip) {
                // Reset the flag, so we keep writing.
                _skip = false;
                return;
            }
            base.WriteEndAttribute();
        }

        public override void WriteStartDocument()
        {
        }
    }
    public class ChemSpiderSynthesisDB: ChemMantisDB 
    {
        byte[] m_EmptyBlob = new byte[0];
        protected override string ConnString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["ChemSpiderSynthesisConnectionString"] == null ?
                    null :
                    ConfigurationManager.ConnectionStrings["ChemSpiderSynthesisConnectionString"].ConnectionString;
            }
        }

        protected override string RO_ConnString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["ChemSpiderSynthesisConnectionString"] == null ?
                    ConnString :    
                    ConfigurationManager.ConnectionStrings["ChemSpiderSynthesisROConnectionString"].ConnectionString;
            }
        }

        public static string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["ChemSpiderSynthesisConnectionString"] == null ?
                    null :
                    ConfigurationManager.ConnectionStrings["ChemSpiderSynthesisConnectionString"].ConnectionString;
            }
        }

        public ChemSpiderSynthesisDB()
        {

        }

        string SerialiazeAuthors(ArticleProperties p)
        {
            StringWriter sw = new StringWriter();
            XmlFragmentWriter xw = new XmlFragmentWriter(sw);
            XmlSerializer xs = new XmlSerializer(typeof(ArticleAuthor));
            foreach (ArticleAuthor aa in p.authors) {
                xs.Serialize(xw, aa);
            }
            return sw.ToString();
        }

        public int CreateArticle(string template, ArticleProperties p)
        {
            Hashtable args = new Hashtable();
            args.Add("template", template);
            args.Add("dsn_id", p.dsn_id);
            args.Add("usr_id", p.usr_id);
            args.Add("id", p.id);
            args.Add("urn", p.url);
            if (p.embargo != DateTime.MinValue)
                args.Add("embargo_date", p.embargo);
            args.Add("tags", p.tags);
            args.Add("abstracted_yn", p.abstracted);
            args.Add("authors", SerialiazeAuthors(p));
            return Convert.ToInt32(DBU.m_runSqlProc("CreateArticle", args, 0));
        }

        public void UpdateArticleProperties(ArticleProperties p)
        {
            Hashtable args = new Hashtable();
            args.Add("@art_id", p.art_id);
            args.Add("@id", p.id);
            args.Add("@article_urn", p.url);
            if (p.embargo >= DateTime.Now)
                args.Add("@embargo_date", p.embargo);
            else
                args.Add("@embargo_date", null);
            if (p.dsn_id > 0)
                args.Add("@dsn_id", p.dsn_id);
            else
                args.Add("@dsn_id", null);

            args.Add("@tags", p.tags);
            args.Add("@usr_id", p.usr_id);
            args.Add("@abstracted_yn", p.abstracted);
            args.Add("@authors", SerialiazeAuthors(p));

            DBU.m_runSqlProc("UpdateArticleProperties", args);
        }

        public void GetArticleXml(int art_id, int rev_no, out string xml, out string xsl)
        {
            using (SqlCommand cmd = DBU.m_createCommand("GetArticleXml")) {
                try {
                    cmd.Parameters.Add("@art_id", SqlDbType.Int).Value = art_id;
                    if (rev_no >= 0)
                        cmd.Parameters.Add("@rev_no", SqlDbType.Int).Value = rev_no;
                    cmd.Parameters.Add("@xml", SqlDbType.NVarChar, int.MaxValue).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@xsl", SqlDbType.NVarChar, int.MaxValue).Direction = ParameterDirection.Output;

                    int res = cmd.ExecuteNonQuery();

                    xml = cmd.Parameters["@xml"].Value.ToString();
                    xsl = cmd.Parameters["@xsl"].Value.ToString();
                }
                finally {
                    DBU.CloseConn(cmd);
                }
            }
        }

        public string GetCompoundArticlesXml(int cmp_id)
        {
            using (SqlCommand cmd = DBU.m_createCommand("GetCompoundArticlesXml")) {
                try {
                    cmd.Parameters.Add("@cmp_id", SqlDbType.Int).Value = cmp_id;
                    cmd.Parameters.Add("@xml_data", SqlDbType.NVarChar, int.MaxValue).Direction = ParameterDirection.Output;

                    int res = cmd.ExecuteNonQuery();

                    return cmd.Parameters["@xml_data"].Value.ToString();
                }
                finally {
                    DBU.CloseConn(cmd);
                }
            }
        }


        public void GetArticleTemplateXml(string template_name, out string xml, out string xsl)
        {
            using (SqlCommand cmd = DBU.m_createCommand("GetArticleTemplateXml")) {
                try {
                    cmd.Parameters.Add("@tmpl_name", SqlDbType.NVarChar, 100).Value = template_name;
                    cmd.Parameters.Add("@xml", SqlDbType.NVarChar, int.MaxValue).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@xsl", SqlDbType.NVarChar, int.MaxValue).Direction = ParameterDirection.Output;

                    int res = cmd.ExecuteNonQuery();

                    xml = cmd.Parameters["@xml"].Value.ToString();
                    xsl = cmd.Parameters["@xsl"].Value.ToString();
                }
                finally {
                    DBU.CloseConn(cmd);
                }
            }
        }

        public void SetArticlePartText(int art_id, int revision, string name, int ordinal, string contents)
        {
            Hashtable args = new Hashtable();
            args.Add("@art_id", art_id);
            args.Add("@revision", revision);
            args.Add("@name", name);
            args.Add("@ordinal", ordinal);
            args.Add("@contents", contents);

            DBU.m_runSqlProc("SetArticlePartText", args);
        }

        public int SetArticlePartBlob(int art_id, int revision, string name, int ordinal, byte [] contents, 
                                       string content_type, string filename, string label)

        {
            Hashtable args = new Hashtable();
            args.Add("@art_id", art_id);
            args.Add("@revision", revision);
            args.Add("@name", name);
            args.Add("@ordinal", ordinal);
            if (contents == null)
                args.Add("@contents", m_EmptyBlob);
            else
                args.Add("@contents", contents);
            args.Add("@content_type", content_type);
            args.Add("@filename", filename);
            args.Add("@label", label);

            object o = DBU.m_runSqlProc("SetArticlePartBlob", args, 0);
            return o is DBNull ? -1 : Convert.ToInt32(o);
        }

        public void UpdateArticleBlob(int ab_id, byte[] contents, int offset, int length)
        {
            Hashtable args = new Hashtable();
            args.Add("@ab_id", ab_id);
            args.Add("@contents", contents);
            args.Add("@offset", offset);
            args.Add("@length", length);

            DBU.m_runSqlProc("UpdateArticleBlob", args);
        }

        public void DeleteArticlePart(int art_id, int revision, string name, int ordinal) 
        {
            Hashtable args = new Hashtable();
            args.Add("@art_id", art_id);
            args.Add("@revision", revision);
            args.Add("@name", name);
            args.Add("@ordinal", ordinal);

            DBU.m_runSqlProc("DeleteArticlePart", args);
        }

        public ArticleProperties GetArticleProperties(int art_id)
        {
            ArticleProperties result = new ArticleProperties(art_id);

            Hashtable args = new Hashtable();
            args.Add("@art_id", art_id);

            using (SqlDataReader reader = DBU.m_executeReader("GetArticleProperties", args)) {	// CAUTION: was CommandBehavior.CloseConnection
                if (reader.Read()) {
                    result.id = reader["id"].ToString();
                    result.url = reader["urn"].ToString();
                    result.tags = reader["tags"].ToString();
                    result.title = reader["title"].ToString();
                    result.doi = reader["doi"].ToString();
                    if (!(reader["published_date"] is DBNull))
                        result.published_date = Convert.ToDateTime(reader["published_date"]);
                    result.abstracted = Convert.ToBoolean(reader["abstracted_yn"]);
                    if (!(reader["dsn_id"] is DBNull))
                        result.dsn_id = Convert.ToInt32(reader["dsn_id"]);
                    if (!(reader["embargo_date"] is DBNull))
                        result.embargo = Convert.ToDateTime(reader["embargo_date"]);
                    result.status = reader["status"].ToString();
                    StringReader sr = new StringReader("<authors>" + reader["authors"].ToString() + "</authors>");
                    XmlSerializer xs = new XmlSerializer(typeof(ArticleAuthors));
                    ArticleAuthors aa;
                    aa = (ArticleAuthors)xs.Deserialize(sr);
                    if (aa != null && aa.authors != null)
                        foreach (ArticleAuthor a in aa.authors)
                            result.authors.Add(a);
                }
                else
                    return null;
            }

            return result;
        }
        
        new public int SaveArticleSnapshot(int art_id, int usr_id, string label, string comments)
        {
            Hashtable args = new Hashtable();
            args.Add("art_id", art_id);
            args.Add("usr_id", usr_id);
            args.Add("label", label);
            args.Add("comments", comments);
            return Convert.ToInt32(DBU.m_runSqlProc("SaveArticleSnapshot", args, 0));
        }

        public void SetArticleKeywords(int art_id, List<string> keywords)
        {
            Hashtable args = new Hashtable();
            args.Add("art_id", art_id);
            args.Add("keywords", keywords);
            DBU.m_runSqlProc("SetArticleKeywords", args, 0);
        }

        public void SaveSearchResults(string search_id, List<int> results, string search_type)
        {
            Hashtable args = new Hashtable();
            args.Add("search_id", search_id);
            args.Add("search_type", search_type);
            args.Add("results", results);
            DBU.m_runSqlProc("SaveSearchResults", args, 0);
        }

        public List<string> GetArticleKeywords(int art_id)
        {
            List<string> result = new List<string>();
            Hashtable args = new Hashtable();
            args.Add("art_id", art_id);
            DataTable dt = DBU.m_fillDataTableProc("GetArticleKeywords", args);
            foreach (DataRow dr in dt.Rows) {
                result.Add(Convert.ToString(dr[0]));
            }
            return result;
        }

        public class ArticlePart
        {
            public string name = string.Empty;
            public string contents_txt = string.Empty;
            public int contents_bin = -1;
            public bool auto_markup = true;
        }

        public List<ArticlePart> GetArticleParts(int art_id)
        {
            Hashtable args = new Hashtable();
            args.Add("art_id", art_id);
            List<ArticlePart> result = new List<ArticlePart>();
            DataTable t = DBU.m_fillDataTable(@"select name, contents_txt, contents_bin, auto_markup_yn
                                    from v_articles_parts
                                   where rev_no = 0
                                     and art_id = @art_id", args);
            foreach (DataRow dr in t.Rows) {
                ArticlePart p = new ArticlePart();
                p.name = dr["name"].ToString();
                if (dr["contents_txt"] is DBNull)
                    p.contents_bin = Convert.ToInt32(dr["contents_bin"]);
                else
                    p.contents_txt = dr["contents_txt"].ToString();
                p.auto_markup = Convert.ToBoolean(dr["auto_markup_yn"]);
                result.Add(p);
            }
            return result;
        }

        public enum EArticlePartDataType
        {
            OtherFile, ReactionFile, ImageFile, MultilineText, SingleLineText
        }

        public class ArticleTemplatePart
        {
            public string name;
            public bool title_yn;
            public EArticlePartDataType data_type;
            public bool binary_yn;
            public bool auto_markup_yn;
        }

        public ArticleTemplatePart GetArticleTemplatePart(int art_id, string part_name)
        {
            ArticleTemplatePart result = new ArticleTemplatePart();

            Hashtable args = new Hashtable();
            args.Add("@art_id", art_id);
            args.Add("@part_name", part_name);

            using (SqlDataReader reader = DBU.m_executeReader("GetArticleTemplatePart", args)) {	
                if (reader.Read()) {
                    result.name = reader["name"].ToString();
                    result.title_yn = Convert.ToBoolean(reader["title_yn"]);
                    string dt = reader["data_type"].ToString();
                    switch (dt) {
                        case "OF":
                            result.data_type = EArticlePartDataType.OtherFile;
                            break;
                        case "RF":
                            result.data_type = EArticlePartDataType.ReactionFile;
                            break;
                        case "IF":
                            result.data_type = EArticlePartDataType.ImageFile;
                            break;
                        case "MT":
                            result.data_type = EArticlePartDataType.MultilineText;
                            break;
                        case "ST":
                            result.data_type = EArticlePartDataType.SingleLineText;
                            break;
                    }
                    result.binary_yn = Convert.ToBoolean(reader["binary_yn"]);
                    result.auto_markup_yn = Convert.ToBoolean(reader["auto_markup_yn"]);
                }
                else
                    return null;
            }

            return result;
        }

        new public string GetArticleTitle(int art_id)
        {
            ArticleProperties p = GetArticleProperties(art_id);
            return p == null ? string.Empty : p.title;
        }
        public void RequestGroupMembership(int requester, int usr_id, int gid, string comments)
        {
            Hashtable args = new Hashtable();
            args.Add("requester", requester);
            args.Add("usr_id", usr_id);
            args.Add("gid", gid);
            args.Add("comments", comments);
            DBU.m_runSqlProc("RequestGroupMembership", args, 0);
        }
        public void CancelGroupMembership(int usr_id, int gid)
        {
            Hashtable args = new Hashtable();
            args.Add("usr_id", usr_id);
            args.Add("gid", gid);
            DBU.m_runSqlProc("CancelGroupMembership", args, 0);
        }
        public void ApproveGroupMembership(int grid, int approved_by)
        {
            Hashtable args = new Hashtable();
            args.Add("approved_by", approved_by);
            args.Add("grid", grid);
            DBU.m_runSqlProc("ApproveGroupMembership", args, 0);
        }
        
        public void RejectGroupMembership(int grid, string comments)
        {
            Hashtable args = new Hashtable();
            args.Add("comments", comments);
            args.Add("grid", grid);
            DBU.m_runSqlProc("RejectGroupMembership", args, 0);
        }

        public void ChangeGroupMembership(int usr_id, int gid, bool manager_yn)
        {
            Hashtable args = new Hashtable();
            args.Add("usr_id", usr_id);
            args.Add("gid", gid);
            args.Add("manager_yn", manager_yn);
            DBU.m_runSqlProc("ChangeGroupMembership", args, 0);
        }

        public string GetCrossrefXml(int art_id)
        {
            Hashtable args = new Hashtable();
            args.Add("art_id", art_id);
            string result = DBU.m_runSqlProc("GetCrossrefXml", args, 0).ToString();
            return result;
        }

        public int CreateCrossrefDeposition(int art_id, string data)
        {
            Hashtable args = new Hashtable();
            args.Add("art_id", art_id);
            args.Add("data", data);
            return Convert.ToInt32(DBU.m_runSqlProc("CreateCrossrefDeposition", args, 0));
        }

        public void UpdateCrossrefDeposition(int dep_id, string status)
        {
            Hashtable args = new Hashtable();
            args.Add("dep_id", dep_id);
            args.Add("status", status);
            DBU.m_runSqlProc("UpdateCrossrefDeposition", args);
        }
    
        public DataTable SearchArticles(string term, string search_id)
        {
            Hashtable args = new Hashtable();
            args["@term"] = term;
            args["@search_id"] = search_id;
            DataTable dt = DBU.m_fillDataTableProc("SearchArticles", args);
            return dt;
        }

        public void InsertGroupInvitation(int gid, string email, int sent_by)
        {
            DBU.ExecuteCommand("exec InsertGroupInvitation", new { gid = gid, email = email, sent_by = sent_by} );
        }

        public string GetSetting(string setting_title)
        {
            return DBU.m_querySingleValue(String.Format("SELECT value FROM settings WHERE title = '{0}'", setting_title)) as string;
        }
    }
}