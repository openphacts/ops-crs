using System;
using System.Collections.Generic;
using System.Collections;
using System.Security;
using System.Text;
using System.Web;
using ChemSpider.Data.Database;

namespace ChemSpider.Security
{
	public class HttpSecurity
	{
		static private string[] fingerprints = { "0x4445434C41524520" };

		private static Dictionary<string, bool> addresses_hash;

		static HttpSecurity()
		{
			addresses_hash = new Dictionary<string, bool>();
			List<string> addresses = new ChemUsersDB().getBannedClients();
			foreach ( string address in addresses ) {
				addresses_hash.Add(address, true);
			}
		}

		public static void check(HttpContext context)
		{
			checkCache(context);
			assessThreat(context);
		}

		private static void checkCache(HttpContext context)
		{
			bool threat;
			if ( !addresses_hash.TryGetValue(context.Request.UserHostAddress, out threat) )
				addresses_hash.Add(context.Request.UserHostAddress, false);
			else if ( threat )
				respond(context);
		}

		private static void assessThreat(HttpContext context)
		{
			fpCheck(context);
		}

		private static void fpCheck(HttpContext context)
		{
			HttpRequest req = context.Request;

			foreach ( string f in fingerprints ) {
				if ( req.RawUrl.IndexOf(f) != -1 ) {
					new ChemUsersDB().banClient(req.UserHostAddress, req.RawUrl.Substring(0, 1000));
					addresses_hash[req.UserHostAddress] = true;
					respond(context);
				}
			}
		}

		private static void respond(HttpContext context)
		{
			context.Response.Clear();
			context.Response.StatusCode = 403;
			context.Response.StatusDescription = "Client address is banned";
			context.Response.End();
		}

		public static int getInt(HttpRequest req, string name)
		{
			return int.Parse(req[name]);
		}

		public static string getString(HttpRequest req, string name)
		{
			return req[name];
		}
	}
}