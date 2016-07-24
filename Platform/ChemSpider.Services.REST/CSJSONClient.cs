using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Net;
using System.Web;
using System.IO;
using System.Runtime.Serialization.Json;

namespace ChemSpider.Web
{
    public class CSJSONClient : WebClient
    {
        private string jsonUrl = string.Empty;

        public CSJSONClient(string url)
        {
            jsonUrl = url;

            RedirectCookies = true;
        }

        public bool RedirectCookies { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest r = base.GetWebRequest(address);
            var request = r as HttpWebRequest;
            if (request != null && RedirectCookies)
            {
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    request.UserAgent = HttpContext.Current.Request.UserAgent;

                    request.CookieContainer = new CookieContainer();

                    HttpCookieCollection cookies = HttpContext.Current.Request.Cookies;
                    for (int j = 0; j < cookies.Count; j++)
                    {
                        HttpCookie cookie = cookies.Get(j);
                        Cookie newCookie = new Cookie();

                        // Convert between the System.Net.Cookie to a System.Web.HttpCookie...
                        newCookie.Domain = request.RequestUri.Host;
                        newCookie.Expires = cookie.Expires;
                        newCookie.Name = cookie.Name;
                        newCookie.Path = cookie.Path;
                        newCookie.Secure = cookie.Secure;
                        newCookie.Value = cookie.Value;

                        request.CookieContainer.Add(newCookie);
                    }
                }
            }
            return r;
        }

        #region GET operations
        protected Dictionary<string, string> ConvertToQueryParameters(object request, string prefix = null)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();

            PropertyInfo[] props = request.GetType().GetProperties();

            foreach (PropertyInfo p in props)
            {
                object obj = p.GetValue(request, null);

                string propName = string.IsNullOrEmpty(prefix) ? p.Name : prefix + "." + p.Name;

                if (obj.GetType().IsGenericType)
                    foreach (var param in ConvertToQueryParameters(obj, propName))
                        queryParams.Add(param.Key, param.Value);
                else
                    queryParams.Add(propName, obj.ToString());
            }

            return queryParams;
        }

        public string GET(string operation, object parameters)
        {
            Dictionary<string, string> queryParams = ConvertToQueryParameters(parameters);

            return GET(operation, queryParams);
        }

        protected string GET(string operation, IDictionary<string, string> parameters)
        {
            parameters.Add("op", operation);

            string query = string.Join("&", parameters.Select(p => string.Format("{0}={1}", p.Key, HttpUtility.UrlEncode(p.Value))));

            string url = jsonUrl + "?" + query;

            byte[] responseBytes = DownloadData(jsonUrl + "?" + query);

            return Encoding.Default.GetString(responseBytes);
        }
        #endregion

        #region POST operations
        protected string ConvertToJSON(object request)
        {
            if (request.GetType().ToString().Contains("AnonymousType"))
            {
                string json = string.Empty;

                PropertyInfo[] props = request.GetType().GetProperties();

                foreach (PropertyInfo p in props)
                {
                    object obj = p.GetValue(request, null);

                    if (!string.IsNullOrEmpty(json))
                        json += ",";

                    json += string.Format("\"{0}\":{1}", p.Name, ConvertToJSON(obj));
                }

                return string.Format("{{{0}}}", json);
            }
            else
            {
                MemoryStream stream = new MemoryStream();
                DataContractJsonSerializer ser = new DataContractJsonSerializer(request.GetType());

                ser.WriteObject(stream, request);

                stream.Position = 0;
                StreamReader sr = new StreamReader(stream);
                return sr.ReadToEnd();
            }
        }

        public string POST(string operation, object data, string serfilter = null)
        {
            string jsonPOST = ConvertToJSON(data);

            var bytes = Encoding.Default.GetBytes(jsonPOST);

            byte[] responseBytes = UploadData(jsonUrl + "?op=" + operation  + (string.IsNullOrEmpty(serfilter) ? "" : "&serfilter=" + serfilter), bytes);

            return Encoding.Default.GetString(responseBytes);
        }
        #endregion
    }
}
