using System;
using System.Data;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Configuration;
using System.Collections;
using System.Data.SqlClient;
using ChemSpider.Database;

namespace ChemSpider.Data.Database
{
    /// <summary>
    /// Summary description for DatasourceInfo
    /// </summary>
    public class DatasourceInfo
    {
        private ChemUsersDB cu_db = new ChemUsersDB();

        private int? m_PrimaryUserId;
        private string m_PrimaryUserName = string.Empty;

        private int? m_Id;
        private string m_Name = string.Empty;
        private string m_ContactName = string.Empty;
        private string m_Description = string.Empty;
        private string m_SiteUrl = string.Empty;
        private string m_CompanyName = string.Empty;
        private string m_StreetAddress = string.Empty;
        private string m_City = string.Empty;
        private string m_State = string.Empty;
        private string m_ZipCode = string.Empty;
        private string m_Country = string.Empty;
        private string m_Website = string.Empty;
        private string m_Email = string.Empty;
        private string m_Phone = string.Empty;
        private string m_Fax = string.Empty;
        private byte[] m_Logo;
        private string m_License = string.Empty;
        private string m_Void_Uri = string.Empty;
        private string m_Uri_Space = string.Empty;
        private bool m_Active = false;
        private bool m_Hidden = false;
        private bool m_FeaturedDataSource = false;
        private bool m_updated = false;
        private bool m_created = false;

        private DataTable m_DsTypesTable = new DataTable();
        private DataTable m_secondaryUsersTable = new DataTable();
        private DataTable m_dataCollectionsTable = new DataTable();

        public int ID
        {
            get { return m_Id == null ? -1 : (int)m_Id; }
        }

        public bool HasDS
        {
            get { return m_Name != null && m_Name != ""; }
        }

        public bool IsActive
        {
            get { return m_Active; }
            set
            {
                m_updated = m_updated || (m_Active != value);
                m_Active = value;
            }
        }

        public bool IsHidden
        {
            get { return m_Hidden; }
            set
            {
                m_updated = m_updated || (m_Hidden != value);
                m_Hidden = value;
            }
        }

        public bool IsFeaturedDataSource
        {
            get { return m_FeaturedDataSource; }
            set
            {
                m_updated = m_updated || (m_FeaturedDataSource != value);
                m_FeaturedDataSource = value;
            }
        }

        public bool IsUpdated
        {
            get { return m_updated; }
        }

        public bool IsCreated
        {
            get { return m_created; }
        }

        public string Name
        {
            get { return m_Name; }
            set
            {
                m_updated = m_updated || (m_Name != value);
                m_Name = value;
            }
        }

        public string ContactName
        {
            get { return m_ContactName; }
            set
            {
                m_updated = m_updated || (m_ContactName != value);
                m_ContactName = value;
            }
        }

        public string Description
        {
            get { return m_Description; }
            set
            {
                m_updated = m_updated || (m_Description != value);
                m_Description = value;
            }
        }
        public string SiteUrl
        {
            get { return m_SiteUrl; }
            set
            {
                m_updated = m_updated || (m_SiteUrl != value);
                m_SiteUrl = value;
            }
        }

        public string CompanyName
        {
            get { return m_CompanyName; }
            set
            {
                m_updated = m_updated || (m_CompanyName != value);
                m_CompanyName = value;
            }
        }

        public string StreetAddress
        {
            get { return m_StreetAddress; }
            set
            {
                m_updated = m_updated || (m_StreetAddress != value);
                m_StreetAddress = value;
            }
        }

        public string City
        {
            get { return m_City; }
            set
            {
                m_updated = m_updated || (m_City != value);
                m_City = value;
            }
        }

        public string State
        {
            get { return m_State; }
            set
            {
                m_updated = m_updated || (m_State != value);
                m_State = value;
            }
        }

        public string ZipCode
        {
            get { return m_ZipCode; }
            set
            {
                m_updated = m_updated || (m_ZipCode != value);
                m_ZipCode = value;
            }
        }

        public string Country
        {
            get { return m_Country; }
            set
            {
                m_updated = m_updated || (m_Country != value);
                m_Country = value;
            }
        }

        public string Website
        {
            get { return m_Website; }
            set
            {
                m_updated = m_updated || (m_Website != value);
                m_Website = value;
            }
        }

        public string Email
        {
            get { return m_Email; }
            set
            {
                m_updated = m_updated || (m_Email != value);
                m_Email = value;
            }
        }

        public string Phone
        {
            get { return m_Phone; }
            set
            {
                m_updated = m_updated || (m_Phone != value);
                m_Phone = value;
            }
        }

        public string Fax
        {
            get { return m_Fax; }
            set
            {
                m_updated = m_updated || (m_Fax != value);
                m_Fax = value;
            }
        }

        public string License
        {
            get { return m_License; }
            set
            {
                m_updated = m_updated || (m_License != value);
                m_License = value;
            }
        }

        public string VoidUri
        {
            get { return m_Void_Uri; }
            set
            {
                m_updated = m_updated || (m_Void_Uri != value);
                m_Void_Uri = value;
            }
        }

        public string UriSpace
        {
            get { return m_Uri_Space; }
            set
            {
                m_updated = m_updated || (m_Uri_Space != value);
                m_Uri_Space = value;
            }
        }

        public byte[] Logo
        {
            get { return m_Logo; }
            set
            {
                m_updated = m_updated || (m_Logo != value);
                m_Logo = value;
            }
        }

        public DataTable DsTypes { get { return m_DsTypesTable; } }

        public int? PrimaryUserID
        {
            get { return m_PrimaryUserId; }
            set
            {
                m_updated = m_updated || (m_PrimaryUserId != value);
                m_PrimaryUserId = value;

                if (m_PrimaryUserId != null)
                {
                    string username = cu_db.getUsrName((int)m_PrimaryUserId);
                    if (!string.IsNullOrEmpty(username))
                    {
                        m_PrimaryUserName = username;
                    }
                    else
                    {
                        m_PrimaryUserName = string.Empty;
                    }
                }
                else
                {
                    m_PrimaryUserName = string.Empty;
                }
            }
        }

        public string PrimaryUserName
        {
            get { return m_PrimaryUserName; }
        }

        public DataTable SecondaryUsers
        {
            get { return m_secondaryUsersTable; }
            set { m_secondaryUsersTable = value; }
        }

        public DataTable DataCollections
        {
            get { return m_dataCollectionsTable; }
        }

        public DatasourceInfo()
        {
        }

        public DatasourceInfo(int dsnID)
        {
            m_Id = dsnID;
            Retrieve();
        }

        public void LoadForUser(int userID)
        {
            m_PrimaryUserId = userID;
            Retrieve();
        }

        public void Retrieve()
        {
            if (m_Id == null)
            {
                Hashtable args = new Hashtable();
                args.Add("@usr_id", m_PrimaryUserId);
                SqlCommand c = cu_db.DBU.m_createCommand(
                        "select dsn_id " +
                          "from data_source_contacts " +
                         "where usr_id = @usr_id",
                         args,
                         CommandType.Text);

                m_Id = (int?)c.ExecuteScalar();
            }

            if (m_Id != null)
            {
                Hashtable args = new Hashtable();
                args.Add("@id", m_Id);

                DataSet ds = cu_db.DBU.m_fillDataset("GetDataSource", args);

                //	Get main information about Data Source...
                DataTable dataSourceTable = ds.Tables[0];

                if (dataSourceTable.Rows.Count != 1)
                    throw new Exception("Couldn't retrieve datasource details");

                m_Name = dataSourceTable.Rows[0]["name"].ToString();
                m_ContactName = dataSourceTable.Rows[0]["contact_name"].ToString();
                m_Description = dataSourceTable.Rows[0]["description"].ToString();
                m_SiteUrl = dataSourceTable.Rows[0]["ds_url"].ToString();
                m_Website = dataSourceTable.Rows[0]["website"].ToString();
                m_Email = dataSourceTable.Rows[0]["email"].ToString();
                m_Phone = dataSourceTable.Rows[0]["phone"].ToString();
                m_Fax = dataSourceTable.Rows[0]["fax"].ToString();
                m_CompanyName = dataSourceTable.Rows[0]["company_name"].ToString();
                m_StreetAddress = dataSourceTable.Rows[0]["street_address"].ToString();
                m_City = dataSourceTable.Rows[0]["city"].ToString();
                m_State = dataSourceTable.Rows[0]["state"].ToString();
                m_ZipCode = dataSourceTable.Rows[0]["zipcode"].ToString();
                m_Country = dataSourceTable.Rows[0]["country"].ToString();
                if (!(dataSourceTable.Rows[0]["logo"] is DBNull))
                    m_Logo = (byte[])dataSourceTable.Rows[0]["logo"];
                m_Active = dataSourceTable.Rows[0]["active_yn"].ToString().Equals("Y");
                m_Hidden = (bool)dataSourceTable.Rows[0]["hidden_yn"];
                m_FeaturedDataSource = (bool)dataSourceTable.Rows[0]["featured_yn"];
                if (!(dataSourceTable.Rows[0]["usr_id"] is DBNull))
                    m_PrimaryUserId = Convert.ToInt32(dataSourceTable.Rows[0]["usr_id"]);
                if (!(dataSourceTable.Rows[0]["username"] is DBNull))
                    m_PrimaryUserName = dataSourceTable.Rows[0]["username"].ToString();
                m_License = dataSourceTable.Rows[0]["dataset_license"].ToString();
                m_Void_Uri = dataSourceTable.Rows[0]["dataset_void_uri"].ToString();
                m_Uri_Space = dataSourceTable.Rows[0]["dataset_uri_space"].ToString();

                //	Get information about Data Source types...
                m_DsTypesTable = ds.Tables[1];

                //	Get information about Secondary Users...
                m_secondaryUsersTable = ds.Tables[2];

                //	Get list of Data Collections...
                m_dataCollectionsTable = ds.Tables[3];

                m_updated = false;
            }
        }

        public void Update()
        {
            SqlCommand cmd = null;
            try
            {
                Hashtable args = new Hashtable();

                args.Add("@usr_id", m_PrimaryUserId);
                args.Add("@name", Name);
                args.Add("@contact_name", ContactName);
                args.Add("@description", Description);
                args.Add("@url", SiteUrl);
                args.Add("@company_name", CompanyName);
                args.Add("@street_address", StreetAddress);
                args.Add("@city", City);
                args.Add("@state", State);
                args.Add("@zipcode", ZipCode);
                args.Add("@country", Country);
                args.Add("@website", Website);
                args.Add("@email", Email);
                args.Add("@phone", Phone);
                args.Add("@fax", Fax);
                args.Add("@logo", m_Logo);
                args.Add("@active", m_Active ? 'Y' : 'N');
                args.Add("@hidden", m_Hidden);
                args.Add("@featured", m_FeaturedDataSource);
                args.Add("@dataset_license", m_License);
                args.Add("@dataset_uri_space", m_Uri_Space);
                args.Add("@dataset_void_uri", m_Void_Uri);

                List<int> secondaryUsers = new List<int>();
                foreach (DataRow row in m_secondaryUsersTable.Rows)
                {
                    secondaryUsers.Add(Convert.ToInt32(row["usr_id"]));
                }
                args.Add("@sec_users", secondaryUsers);

                List<int> dsTypes = new List<int>();
                foreach (DataRow row in m_DsTypesTable.Rows)
                {
                    dsTypes.Add(Convert.ToInt32(row["dst_id"]));
                }
                args.Add("@dst", dsTypes);

                cmd = cu_db.DBU.m_createCommand("UpdateDataSource", args);

                SqlParameter idParam = new SqlParameter("@id", SqlDbType.Int);
                idParam.Direction = ParameterDirection.InputOutput;
                if (m_Id != null)
                    idParam.Value = m_Id;
                else
                    idParam.Value = DBNull.Value;

                cmd.Parameters.Add(idParam);

                int res = cmd.ExecuteNonQuery();

                if (m_Id == null)
                {
                    m_created = true;
                }

                m_Id = (int)cmd.Parameters["@id"].Value;

                m_updated = true;
            }
            finally
            {
                if (cmd != null)
                    cmd.Connection.Close();
            }
        }

        public static int? GetDSIdForUser(string username, ref bool isPrimary)
        {
            int? dsId = null;

            Hashtable args = new Hashtable();
            args.Add("@username", username);

            DataSet ds = (new ChemUsersDB()).DBU.m_fillDataset("GetDataSourceForUser", args);

            //	Get main information about Data Source...
            DataTable tbl = ds.Tables[0];

            if (tbl != null && tbl.Rows.Count > 0)
            {
                dsId = (int?)tbl.Rows[0]["dsn_id"];
                isPrimary = tbl.Rows[0]["contact_type"].ToString().Equals("P");
            }

            return dsId;
        }

        public static int? GetDSIdForDataCollection(int id)
        {
            int? dsId = null;

            DataSet ds = (new ChemUsersDB()).GetDataCollection(id);

            DataTable dcTable = ds.Tables[0];

            if (ds.Tables.Count != 0 && dcTable.Rows.Count == 1)
            {
                dsId = (int?)dcTable.Rows[0]["dsn_id"];
            }

            return dsId;
        }

        public static byte[] getDataSourceLogo(int dsn_id)
        {
            return (new ChemUsersDB()).GetDataSourceLogo(dsn_id);
        }

        public static List<int> GetDataSourcesForDataType(int dst_id)
        {
            List<int> list = new List<int>();

            DataSet ds = (new ChemUsersDB()).GetDataSourcesForDataType(dst_id);

            //	Get main information about Data Source...
            DataTable dsTable = ds.Tables[0];

            for (int i = 0; i < dsTable.Rows.Count; i++)
            {
                list.Add(Convert.ToInt32(dsTable.Rows[i]["dsn_id"]));
            }

            return list;
        }
    }
}
