using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using ChemSpider.Database;
using ChemSpider.Data.Database;
using ChemSpider.Utilities;
using System.Security;
using System.Threading;
using System.Data;

namespace ChemSpider.Web
{
    /// <summary>
    /// Summary description for CSServiceBase
    /// </summary>
    public static class CSServiceUtils
    {
        public static void LogServiceTransaction(string token, string service)
        {
            //  if the connection string is not specified then the log system is disabled...
            if (string.IsNullOrEmpty(ChemTranDB.ConnectionString))
                return;

            //  if ChemUsersDB or ChemTranDB is not configured properly that we have to skip this step...
            //if (string.IsNullOrEmpty(ChemTranDB.ConnectionString) || string.IsNullOrEmpty(ChemUsersDB.ConnectionString))
            //    return;

            using (SqlConnection conn = new SqlConnection(ChemTranDB.ConnectionString))
            {
                string caller = service ?? ReflectUtils.GetCallerName(1);
                conn.ExecuteCommand("exec LogServiceTransaction @token, @service", new { token = token, service = caller });
            }
        }

        public static void LogServiceError(string token, string message, string service)
        {
            //  if the connection string is not specified then the log system is disabled...
            if (string.IsNullOrEmpty(ChemTranDB.ConnectionString))
                return;

            using (SqlConnection conn = new SqlConnection(ChemTranDB.ConnectionString))
            {
                string caller = service ?? ReflectUtils.GetCallerName(1);
                conn.ExecuteCommand("exec LogServiceError @token, @service, @error", new { token = token, service = caller, error = message.Shrink(4000) });
            }
        }

        /// <summary>
        /// Checks whether a user identified by a token is allowed to call the caller method (which is supposed to be a webservice operation method).
        /// </summary>
        /// <param name="token">User security token</param>
        public static void GetWebServiceAccess(string token, string service)
        {
            //  if the connection string is not specified then the security is disabled...
            if (string.IsNullOrEmpty(ChemUsersDB.ConnectionString))
                return;

            int? delay;
            if ( !CheckWebServiceAccess(token, service, out delay) )
                throw new SecurityException("Unauthorized web service usage. Please request access to this service.");

            if ( delay != null ) {
                DateTime? ts = GetWebServiceTimestamp(token, service);
                if ( ts != null ) {
                    TimeSpan passed_delay = (TimeSpan)( DateTime.Now - ts );
                    if ( delay > passed_delay.TotalMilliseconds )
                        Thread.Sleep(TimeSpan.FromMilliseconds((double)delay) - passed_delay); // Wait till delay is reached
                }
            }
        }

        public static bool IsTokenRequired(string service)
        {
            //  if the connection string is not specified then the security is disabled...
            if (string.IsNullOrEmpty(ChemUsersDB.ConnectionString))
                return false;

            return !String.Equals(GetAccessType(service), "A");
        }

        public static string GetAccessType(string service)
        {
            //  if the connection string is not specified then the security is disabled...
            if (string.IsNullOrEmpty(ChemUsersDB.ConnectionString))
                return "A";

            using ( SqlConnection conn = new SqlConnection(ChemUsersDB.ConnectionString) ) {
                return conn.ExecuteScalar<string>("select access from web_services where name = @name", new { name = service });
            }
        }

        public static string GetAccessTypeString(string service)
        {
            switch ( GetAccessType(service) ) {
                case "T":
                    return "Authenticated";
                case "R":
                    return "Role";
                case "F":
                    return "Fine-grained";
                case "A":
                    return "Anonymous";
                default:
                    return "Unknown";
            }
        }

        #region Private methods

        public static bool CheckWebServiceAccess(string token, string service, out int? delay)
        {
            delay = null;

            //  if the connection string is not specified then the security is disabled...
            if (ChemUsersDB.ConnectionString == null)
                return true;

            using ( SqlConnection conn = new SqlConnection(ChemUsersDB.ConnectionString) ) {
                DataTable dt = conn.FillDataTable("exec CheckWebServiceAccess @token, @service", new { token = token, service = service });
                if ( dt.Rows.Count == 0 || !bool.Equals(dt.Rows[0]["granted"], true) )
                    return false;
                delay = dt.Rows[0]["delay"] as int?;
                return true;
            }
        }

        private static DateTime GetWebServiceTimestamp(string token, string service)
        {
            using ( SqlConnection conn = new SqlConnection(ChemTranDB.ConnectionString) ) {
                return conn.ExecuteScalar<DateTime>("exec GetWebServiceTimestamp @token, @service", new { token = token, service = service });
            }
        }

        #endregion
    }

}
