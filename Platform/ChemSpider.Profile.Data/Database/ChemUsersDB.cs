using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data;
using System.Security;
using ChemSpider.Database;

namespace ChemSpider.Data.Database
{
	public class ChemUsersDB : ChemSpiderBaseDB
	{
		public static string ConnectionString
		{
			get
			{
				return ConfigurationManager.ConnectionStrings["ChemUsersConnectionString"] == null ?
					null :
					ConfigurationManager.ConnectionStrings["ChemUsersConnectionString"].ConnectionString;
			}
		}

		public static string RO_ConnectionString
		{
			get
			{
				return ConfigurationManager.ConnectionStrings["ChemUsersROConnectionString"] == null ?
					ConnectionString :
					ConfigurationManager.ConnectionStrings["ChemUsersROConnectionString"].ConnectionString;
			}
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

		protected override string DatabaseName
		{
			get { return "ChemUsers"; }
		}

		public ChemUsersDB()
		{
		}

		public int getUserId(string login)
		{
			object id = DBU.m_querySingleValue(String.Format("select u.usr_id from users u, aspnet_Users au where u.user_guid = au.UserId and au.UserName = '{0}'", login));
			if ( id == null )
				throw new SecurityException("No such user");
			return int.Parse(id.ToString());
		}

		public void getUserProps(int usr_id, out string full_name, out string username, out string email)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString))
			{
				full_name = conn.ExecuteScalar<string>("select dbo.fGetUserName(@usr_id)", new { usr_id = usr_id });
				username = conn.ExecuteScalar<string>("exec GetUserName @usr_id", new { usr_id = usr_id });
				email = conn.ExecuteScalar<string>("exec GetUserEmail @usr_id", new { usr_id = usr_id });
			}
		}

		public string getUsrName(int userID)
		{
			Hashtable ht = new Hashtable();
			ht.Add("@user_id", userID);
			object username = DBU.m_runSqlProc("GetUserName", ht, "UserName");
			if ( username == null )
				throw new SecurityException("No such user");
			return username.ToString();
		}

		public string getUserDisplayName(int usr_id)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString))
			{
				return conn.ExecuteScalar<string>("select dbo.fGetUserDisplayName(@usr_id)", new { usr_id = usr_id });
			}
		}

		public string getUserAvatar(int usr_id)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString))
			{
				return conn.ExecuteScalar<string>("select dbo.fGetUserAvatar(@usr_id)", new { usr_id = usr_id });
			}
		}

		public int getUsrIdByUserGuid(string user_token)
		{
			int usr_id = 0;
			string user_name = String.Empty;
			Hashtable ht = new Hashtable();
			ht.Add("@user_guid", user_token);

			Object obj_usr_id = DBU.m_runSqlProc("GetUserIdByGUID", ht, 0);
			if (obj_usr_id == null)
				throw new SecurityException("No such user");
			usr_id = Convert.ToInt32(obj_usr_id);
			return usr_id;
		}

		public void setUserPrefValue(int usr_id, string pref_name, string value)
		{
			Hashtable ht = new Hashtable();
			ht.Add("@usr_id", usr_id);
			ht.Add("@pref_name", pref_name);
			ht.Add("@value", value);
			DBU.m_runSqlProc("SetUserPrefValue", ht);
		}

		public string getUserPrefValue(int usr_id, string pref_name)
		{
			Hashtable ht = new Hashtable();
			ht.Add("@usr_id", usr_id);
			ht.Add("@pref_name", pref_name);
			return DBU.m_runSqlProc("GetUserPrefValue", ht, 0) as string;
		}

		public string getUsrEmail(int userID)
		{
			Hashtable ht = new Hashtable();
			ht.Add("@user_id", userID);
			object username = DBU.m_runSqlProc("GetUserEmail", ht, "Email");
			return username == null ? null : username.ToString();
		}

		public void MailQueueAdd(string from, string to, string cc, string body, string mtname)
		{
			Hashtable args = new Hashtable();
			args["@from"] = from;
			args["@to"] = to;
			args["@body"] = body;
			args["@mtname"] = mtname;
			args["@interval_day"] = 1;
			SqlCommand cmd = DBU.m_createCommand("x_mq_add_tag", args, CommandType.StoredProcedure);
			cmd.ExecuteNonQuery();
		}

		public StringCollection GetRolesForUser(string username)
		{
			StringCollection roles = new StringCollection();

			Hashtable args = new Hashtable();
			args.Add("@username", username);

			DataSet ds = DBU.m_fillDataset("GetUserRoles", args);
			if ( ds.Tables.Count > 0 ) {
				DataTable rolesTable = ds.Tables[0];

				foreach ( DataRow row in rolesTable.Rows ) {
					roles.Add(row["RoleName"].ToString());
				}
			}

			return roles;
		}

		public StringCollection GetAllRoles()
		{
			StringCollection roles = new StringCollection();

			DataSet ds = DBU.m_fillDataset("GetAllRoles");
			if ( ds.Tables.Count > 0 ) {
				DataTable rolesTable = ds.Tables[0];

				foreach ( DataRow row in rolesTable.Rows ) {
					roles.Add(row["RoleName"].ToString());
				}
			}

			return roles;
		}

		public Hashtable GetAllChemspiderRoles()
		{
			Hashtable roles = new Hashtable();

			using (SqlConnection conn = new SqlConnection(ConnectionString))
			{
				DataTable table = conn.FillDataTable("select rol_id, name from roles");

				foreach (DataRow row in table.Rows)
				{
					roles.Add(Convert.ToInt32(row["rol_id"]), row["name"].ToString());
				}
			}

			return roles;
		}

		public StringCollection GetRequestedUserRoles(string username)
		{
			StringCollection roles = new StringCollection();

			Hashtable args = new Hashtable();
			args.Add("@username", username);

			DataSet ds = DBU.m_fillDataset("GetRequestedUserRoles", args);
			if ( ds.Tables.Count > 0 ) {
				DataTable rolesTable = ds.Tables[0];

				foreach ( DataRow row in rolesTable.Rows ) {
					roles.Add(row["RoleName"].ToString());
				}
			}

			return roles;
		}

		public void RequestRoleForUser(string username, string role, bool isAdminMode)
		{
			Hashtable args = new Hashtable();
			args.Add("@username", username);
			args.Add("@role", role);
			args.Add("@isAdminMode", isAdminMode ? 1 : 0);

			SqlCommand cmd = DBU.m_createCommand("RequestRoleForUser", args);
			cmd.ExecuteNonQuery();
		}

		public void AssignRoleToUser(string username, string role)
		{
			Hashtable args = new Hashtable();
			args.Add("@username", username);
			args.Add("@role", role);

			SqlCommand cmd = DBU.m_createCommand("AssignRoleToUser", args);
			cmd.ExecuteNonQuery();
		}

		public void RemoveRoleForUser(string username, string role)
		{
			Hashtable args = new Hashtable();
			args.Add("@username", username);
			args.Add("@role", role);

			SqlCommand cmd = DBU.m_createCommand("RemoveRoleForUser", args);
			cmd.ExecuteNonQuery();
		}

		public void DeleteUser(string username)
		{
			Hashtable args = new Hashtable();
			args.Add("@username", username);

			SqlCommand cmd = DBU.m_createCommand("DeleteUser", args);
			cmd.ExecuteNonQuery();
		}

		public void BlockUser(int usr_id, bool block)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString))
			{
				conn.ExecuteCommand("update users set blocked_yn = @blocked_yn where usr_id = @usr_id", new { blocked_yn = block ? 'Y' : 'N', usr_id = usr_id });
			}
		}

		/*    public static bool IsUserBlocked(string username)
			{
				Hashtable args = new Hashtable();
				args.Add("@username", username);

				object isBlocked = Utility.runSqlProc("CheckIsUserBlocked", args, 0, DbUtil.ConnectionTimeout);

				return isBlocked.Equals("Y");
			}
		*/
		public void checkAccessToken(string token)
		{
			Guid user_guid = new Guid(token);
			if ( DBU.m_querySingleValue(String.Format("select 1 from users where user_guid = '{0}' and deleted_yn = 'N' and blocked_yn = 'N'", user_guid)) == null )
				throw new SecurityException("Unauthorized web service usage. Please register to get a valid web service token.");
		}

		public void checkAccessToken(string token, Uri referrer)
		{
			Guid user_guid = new Guid(token);
			if ( DBU.m_querySingleValue(String.Format("select 1 from users u join data_source_contacts dsc on u.usr_id = dsc.usr_id join data_sources ds on dsc.dsn_id = ds.dsn_id where u.user_guid = '{0}' and u.deleted_yn = 'N' and u.blocked_yn = 'N' and ds.ds_url like '%{1}%'", user_guid, referrer.Host)) == null )
				throw new SecurityException("Unauthorized web service usage. Please register to get a valid web service token.");
		}

		public void checkAccessToken(string token, string role)
		{
			Guid user_guid = new Guid(token);
			if ( DBU.m_querySingleValue(String.Format("select top 1 1 from users u join user_roles ur on u.usr_id = ur.usr_id join roles r on ur.rol_id = r.rol_id where u.user_guid = '{0}' and u.deleted_yn = 'N' and u.blocked_yn = 'N' and r.name = '{1}'", user_guid, role)) == null )
				throw new SecurityException("Unauthorized web service usage. Please request the role required to access this service.");
		}

		public bool IsUserApproved(string username)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString))
			{
				return conn.ExecuteScalar<int>("select IsApproved from aspnet_Membership m inner join aspnet_Users u on u.UserId = m.UserId where u.UserName = @username", new { username = username }) == 1;
			}
		}

		public bool IsUserBlocked(string username)
		{
			using (SqlConnection conn = new SqlConnection(ConnectionString))
			{
				return conn.ExecuteScalar<int>("select case when blocked_yn = 'Y' then 1 else 0 end from users u inner join aspnet_Users asp_u on u.user_guid = asp_u.UserId where asp_u.UserName = @username", new { username = username }) == 1;
			}
		}

		/* ***************************************************************************************************
		* DataSources functions
		*************************************************************************************************** */
		public int? GetActiveDataSource(int usr_id)
		{
			return (int?)DBU.m_querySingleValue(String.Format("select ds.dsn_id from data_source_contacts dsc, data_sources ds where dsc.usr_id = {0} and dsc.dsn_id = ds.dsn_id", usr_id));
		}

		public DataTable GetDataSourcesList()
		{
			DataSet list = DBU.m_fillDataset("GetDataSourcesList");
			return list.Tables[0];
		}

		public void DeleteDataSource(int dsn_id)
		{
			Hashtable args = new Hashtable();
			args.Add("@id", dsn_id);
			using ( SqlCommand cmd = DBU.m_createCommand("DeleteDataSource", args) ) {
				cmd.ExecuteNonQuery();
			}
		}

		public DataSet GetDataSourcesForDataType(int dst_id)
		{
			Hashtable args = new Hashtable();
			args.Add("@dst_id", dst_id);
			return DBU.m_fillDataset("GetDataSourcesForDataType", args);
		}

		public byte[] GetDataSourceLogo(int dsn_id)
		{
			return DBU.m_querySingleValue(String.Format("select logo from data_sources where dsn_id = {0}", dsn_id)) as byte[];
		}

		public Dictionary<int, string> GetDatasourceTypes()
		{
			Dictionary<int, string> result = new Dictionary<int, string>();
			using ( SqlDataReader r = DBU.m_executeReader("select name, dst_id from ds_types order by ord desc, name") ) {
				while ( r.Read() ) {
					result.Add((int)r.GetValue(1), (string)r.GetValue(0));
				}
			}

			return result;
		}

		public DataSet GetDSType(int dst_id)
		{
			Hashtable args = new Hashtable();
			args.Add("@dst_id", dst_id);
			return DBU.m_fillDataset("GetDSType", args);
		}

		public int CreateDataCollection(int dsn_id, string name, string description, string url, bool focus_lib, string website, byte[] logo)
		{
			Hashtable args = new Hashtable();
			args.Add("@dsn_id", dsn_id);
			args.Add("@name", name);
			args.Add("@description", description);
			args.Add("@url", url);
			args.Add("@focused_library_yn", focus_lib ? 1 : 0);
			args.Add("@website", website);
			args.Add("@logo", logo);

			using ( SqlCommand cmd = DBU.m_createCommand("CreateDataCollection", args) ) {
				SqlParameter idParam = new SqlParameter("@id", SqlDbType.Int);
				idParam.Direction = ParameterDirection.InputOutput;
				idParam.Value = DBNull.Value;

				cmd.Parameters.Add(idParam);

				int res = cmd.ExecuteNonQuery();

				return (int)cmd.Parameters["@id"].Value;
			}
		}

		public DataSet GetDataCollection(int col_id)
		{
			Hashtable args = new Hashtable();
			args.Add("@id", col_id);
			return DBU.m_fillDataset("GetDataCollection", args);
		}

		public Hashtable GetDataCollections(int dsn_id)
		{
			Hashtable collections = new Hashtable();

			Hashtable args = new Hashtable();
			args.Add("@dsn_id", dsn_id);
			DataSet ds = DBU.m_fillDataset("GetDataCollections", args);
			DataTable dcTable = ds.Tables[0];
			if ( dcTable != null ) {
				foreach ( DataRow row in dcTable.Rows ) {
					collections.Add(Convert.ToInt32(row["col_id"]), row["name"].ToString());
				}
			}

			return collections;
		}

		public void DeleteDataCollection(int col_id)
		{
			Hashtable args = new Hashtable();
			args.Add("@id", col_id);
			using ( SqlCommand cmd = DBU.m_createCommand("DeleteDataCollection", args) ) {
				cmd.ExecuteNonQuery();
			}
		}

		public void UpdateDataCollection(int col_id, string name, string description, string url, bool focus_lib, string website, byte[] logo)
		{
			Hashtable args = new Hashtable();
			args.Add("@id", col_id);
			args.Add("@name", name);
			args.Add("@description", description);
			args.Add("@url", url);
			args.Add("@focused_library_yn", focus_lib ? 1 : 0);
			args.Add("@website", website);
			args.Add("@logo", logo);

			using ( SqlCommand cmd = DBU.m_createCommand("UpdateDataCollection", args) ) {
				cmd.ExecuteNonQuery();
			}
		}

		//Jon Steele 2-Nov-2010 #309 - Validate that the entered display_name is not already in use.
		public bool isUniqueDisplayName(string display_name)
		{
			try
			{
				bool isUnique = false;
				using (SqlCommand cmd = DBU.m_createCommand("IsUniqueDisplayName"))
				{
					cmd.Parameters.Add("@display_name", SqlDbType.VarChar).Value = display_name;
					cmd.Parameters.Add("@is_unique", SqlDbType.Int).Direction = ParameterDirection.Output;

					int res = cmd.ExecuteNonQuery();
					isUnique = Convert.ToInt32(cmd.Parameters["@is_unique"].Value) != 0;
				}
				return isUnique;
			}
			catch
			{
				return false;
			}
		}

		public void AddSocialNetwork(int usr_id, string type, string id, string first_name, string last_name, string name, string username, string email, string avatar_url, string profile_url, bool email_verified, string raw)
		{
			using (SqlConnection conn = new SqlConnection(RO_ConnectionString))
			{
				conn.ExecuteCommand("exec AddSocialNetwork @usr_id, @type, @id, @first_name, @last_name, @name, @username, @email, @avatar_url, @profile_url, @email_verified, @raw", new { usr_id = usr_id, type = type, id = id, first_name = first_name, last_name = last_name, name = name, username = username, email = email, avatar_url = avatar_url, profile_url = profile_url, email_verified = email_verified, raw = raw });
			}
		}

		public void UpdateSocialNetwork(string type, string id, string first_name, string last_name, string name, string username, string email, string avatar_url, string profile_url, bool email_verified, string raw)
		{
			using (SqlConnection conn = new SqlConnection(RO_ConnectionString))
			{
				conn.ExecuteCommand("exec UpdateSocialNetwork @type, @id, @first_name, @last_name, @name, @username, @email, @avatar_url, @profile_url, @email_verified, @raw", new { type = type, id = id, first_name = first_name, last_name = last_name, name = name, username = username, email = email, avatar_url = avatar_url, profile_url = profile_url, email_verified = email_verified, raw = raw });
			}
		}

		public void DisconnectSocialNetwork(string id, string type)
		{
			using (SqlConnection conn = new SqlConnection(RO_ConnectionString))
			{
				conn.ExecuteCommand("exec DisconnectSocialNetwork @id, @type", new { id = id, type = type });
			}
		}

		public DataTable GetSocialNetworks(int usr_id)
		{
			using (SqlConnection conn = new SqlConnection(RO_ConnectionString))
			{
				return conn.FillDataTable("exec GetSocialNetworks @usr_id", new { usr_id = usr_id });
			}
		}

		public DataRow GetSocialNetwork(string id, string type)
		{
			using (SqlConnection conn = new SqlConnection(RO_ConnectionString))
			{
				DataTable tbl = conn.FillDataTable("exec GetSocialNetwork @id, @type", new { id = id, type = type });

				return tbl.Rows.Count > 0 ? tbl.Rows[0] : null;
			}
		}

		public string GetSocialNetworkRawData(string id, string provider)
		{
			DataRow r = GetSocialNetwork(id, provider);

			return r != null ? r.Field<string>("raw") : null;
		}

		public string GetUserNameBySocialID(string network_type, string id)
		{
			using (SqlConnection conn = new SqlConnection(RO_ConnectionString))
			{
				return conn.ExecuteScalar<string>("exec GetUserNameBySocialID @type, @id", new { type = network_type, id = id });
			}
		}

		public string GetUserNameByEmail(string email)
		{
			using (SqlConnection conn = new SqlConnection(RO_ConnectionString))
			{
				return conn.ExecuteScalar<string>("exec GetUserNameByEmail @email", new { email = email });
			}
		}

		public bool IsEmailRegistered(string email)
		{
			using (SqlConnection conn = new SqlConnection(RO_ConnectionString))
			{
				return conn.ExecuteScalar<int>("select dbo.fIsEmailRegistered(@email)", new { email = email }) == 1;
			}
		}

		public DataTable SearchUsers(string filter, int? daysAgo = null, string role = "", bool? confirmed = null)
		{
			Hashtable args = new Hashtable();
			args.Add("@filter", filter);
			if (daysAgo != null) args.Add("@daysAgo", daysAgo);
			if (!string.IsNullOrEmpty(role)) args.Add("@role", role);
			if (confirmed != null) args.Add("@confirmed", (bool)confirmed ? 1 : 0);

			DataSet ds = DBU.m_fillDataset("SearchUsers", args);
			return ds.Tables[0];
		}

		public List<string> getBannedClients()
		{
			return DBU.m_fetchColumn<string>("select address from banned");
		}

		public void banClient(string address, string message)
		{
			Hashtable args = new Hashtable();
			args["address"] = address;
			args["reason"] = "S";
			args["message"] = message;
			DBU.m_runSqlProc("BanClient", args, 30);
		}
	}
}