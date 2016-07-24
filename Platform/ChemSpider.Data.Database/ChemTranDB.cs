using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChemSpider.Data.Database;
using System.Configuration;
using System.Data.SqlClient;
using ChemSpider.Database;

namespace ChemSpider.Data.Database
{
	public class ChemTranDB : ChemSpiderBaseDB
	{
		public static string ConnectionString
		{
			get
			{
				return ConfigurationManager.ConnectionStrings["ChemTranConnectionString"] == null ?
					null :
					ConfigurationManager.ConnectionStrings["ChemTranConnectionString"].ConnectionString;
			}
		}

		public static string RO_ConnectionString
		{
			get
			{
				return ConfigurationManager.ConnectionStrings["ChemTranROConnectionString"] == null ?
					ConnectionString :
					ConfigurationManager.ConnectionStrings["ChemTranROConnectionString"].ConnectionString;
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
            get { return "ChemTran"; }
        }

        public void SetSessionValue(string ses_id, string key, string value)
        {
            DBU.ExecuteCommand("exec SetSessionValue", new { ses_id = ses_id, key = key, strVal = value });
        }

        public string GetSessionValue(string ses_id, string key)
        {
            return DBU.ExecuteScalar<string>(
                @"declare @strVal varchar(300)
                  exec GetSessionValue @ses_id, @key, @strVal out
                  select @strVal", 
                  new { ses_id = ses_id, key = key });
        }

        public void SetSessionBlob(string ses_id, string key, byte[] blob)
        {
            DBU.ExecuteCommand("exec SetSessionValue", new { ses_id = ses_id, key = key, blobVal = blob });
        }

        public byte[] GetSessionBlob(string ses_id, string key)
        {
            return DBU.ExecuteScalar<byte[]>(
                "select v_blob from session_vars where ses_id = @ses_id and v_key = @key",
                new { ses_id = ses_id, key = key }
                );
        }

        public void ClearSessionValues(string ses_id)
        {
            DBU.ExecuteCommand("exec ClearSessionValues", new { ses_id = ses_id });
        }
    }
}
