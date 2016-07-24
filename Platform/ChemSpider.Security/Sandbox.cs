using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using System.Web;
using ChemSpider.Database;
using ChemSpider.Data.Database;

namespace ChemSpider.Security
{

    /// <summary>
    /// Summary description for Sandbox
    /// </summary>

    [Serializable]
    public class Sandbox
    {
        private int _dsn_id;
        public int DsnId
        {
            get { return _dsn_id; }
        }

        private int? _col_id;
        public int? ColId
        {
            get { return _col_id; }
        }

        private string _key;
        public string Key
        {
            get { return _key; }
        }

        private Sandbox()
        {

        }

        public static Sandbox Current
        {
            get { return Sandbox.fromHost(HttpContext.Current.Request.Url.Host); }
        }

        private Sandbox(string host, int dsn_id, int? col_id)
        {
            int i = host.IndexOf('.');
            _key = i != -1 ? host.Substring(0, i) : host;
            _dsn_id = dsn_id;
            _col_id = col_id;
        }

        private static Sandbox createSandbox(string host)
        {
            ChemUsersDB cudb = new ChemUsersDB();
            using ( SqlDataReader r = cudb.DBU.m_executeReader(String.Format("select dsn_id, col_id from data_collections where website='{0}'", host)) ) {
                if ( r.Read() )
                    return new Sandbox(host, (int)r[0], (int)r[1]);
            }

            using ( SqlDataReader r = cudb.DBU.m_executeReader(String.Format("select dsn_id from data_sources where website='{0}'", host)) ) {
                if ( r.Read() )
                    return new Sandbox(host, (int)r[0], null);
            }

            return null;
        }

        public static Sandbox fromHost(string host)
        {
            if ( host == null )
                return null;

            System.Web.SessionState.HttpSessionState session = HttpContext.Current.Session;
            if ( session == null )
                return Sandbox.createSandbox(host);
            else {
                Sandbox sb = null;
                SortedDictionary<string, Sandbox> hsb = session["sandboxes"] as SortedDictionary<string, Sandbox>;
                if ( hsb == null ) {
                    hsb = new SortedDictionary<string, Sandbox>();
                    session["sandboxes"] = hsb;
                }

                if ( !hsb.TryGetValue(host, out sb) ) {
                    sb = Sandbox.createSandbox(host);
                    hsb.Add(host, sb);
                }
                return sb;
            }
        }

        static Sandbox GetFromCache(string key)
        {
            if ( HttpContext.Current == null )
                return null;

            System.Web.SessionState.HttpSessionState session = HttpContext.Current.Session;
            if ( session == null )
                return null;
            else {
                Sandbox sb = null;
                SortedDictionary<string, Sandbox> hsb = session["sandboxes"] as SortedDictionary<string, Sandbox>;
                if ( hsb == null )
                    return null;

                if ( hsb.TryGetValue(key, out sb) )
                    return sb;
                else
                    return null;
            }
        }

        static void StoreToCache(string key, Sandbox sb)
        {
            if ( HttpContext.Current != null ) {
                System.Web.SessionState.HttpSessionState session = HttpContext.Current.Session;
                if ( session != null ) {
                    SortedDictionary<string, Sandbox> hsb = session["sandboxes"] as SortedDictionary<string, Sandbox>;
                    if ( hsb == null ) {
                        hsb = new SortedDictionary<string, Sandbox>();
                        session["sandboxes"] = hsb;
                    }

                    if ( !hsb.TryGetValue(key, out sb) )
                        hsb.Add(key, sb);
                    else
                        hsb[key] = sb;
                }
            }
        }

        public static Sandbox fromDSN(string dsn_name)
        {
            if ( dsn_name == null )
                return null;

            Sandbox sb = GetFromCache(dsn_name);
            if ( sb == null ) {

                ChemUsersDB cudb = new ChemUsersDB();
                using ( SqlDataReader r = cudb.DBU.m_executeReader(String.Format("select dsn_id from data_sources where name='{0}'", dsn_name)) ) {
                    if ( r.Read() ) {
                        sb = new Sandbox();
                        sb._key = dsn_name;
                        sb._dsn_id = (int)r[0];
                        StoreToCache(dsn_name, sb);
                    }
                }

            }
            return sb;
        }
    }
}
