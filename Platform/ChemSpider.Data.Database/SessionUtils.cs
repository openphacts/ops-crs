using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace ChemSpider.Data.Database
{
	public class SessionUtils
	{
		private static ChemTranDB s_udb
		{
			get
			{
				return new ChemTranDB();
			}
		}

		public static void setSessionValue(string ses_id, string key, string value)
		{
			s_udb.SetSessionValue(ses_id, key, value);
		}

		public static string getSessionValue(string ses_id, string key)
		{
            return s_udb.GetSessionValue(ses_id, key);
		}

		public static void setSessionBlob(string ses_id, string key, byte[] blob)
		{
            s_udb.SetSessionBlob(ses_id, key, blob);
		}

		public static byte[] getSessionBlob(string ses_id, string key)
		{
            return s_udb.GetSessionBlob(ses_id, key);
		}
         
		public static void setSessionValueAsBlob(string ses_id, string key, string value)
		{
            setSessionBlob(ses_id, key, string.IsNullOrEmpty(value) ? null : Encoding.UTF8.GetBytes(value));
		}

		public static string getSessionValueAsBlob(string ses_id, string key)
		{
			ASCIIEncoding enc = new ASCIIEncoding();
			byte[] blob = getSessionBlob(ses_id, key);
			if ( blob != null )
				return enc.GetString(blob);

			return string.Empty;
		}

		public static void clearSessionValues(string ses_id)
		{
            s_udb.ClearSessionValues(ses_id);
		}
	}
}
