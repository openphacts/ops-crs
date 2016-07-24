using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Reflection;
using System.Security.Cryptography;
using System.Web;

namespace ChemSpider.Profile.RSCID
{
    public class OAuthHttpRequest
    {
        public const string FormEncoded = "application/x-www-form-urlencoded";
        public const string IgnoreSslErrors = "IgnoreSslErrors";
        public static OAuthHttpResponse HttpRequestToRSCID(string url, params object[] args)
        {
            Trace.WriteLine("=========== OAuthHttpRequest.HttpRequestToRSCID =============");
            Trace.WriteLine("Url: " + url);

            OAuthHttpResponse oauthResponse = new OAuthHttpResponse();
            try
            {
                ASCIIEncoding encoding = new ASCIIEncoding();

                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Timeout = 600000;
                httpRequest.ReadWriteTimeout = 600000;
                httpRequest.KeepAlive = false;
                httpRequest.AllowAutoRedirect = true;
                httpRequest.Method = "POST";

                AddRequestHeader(httpRequest, args);

                //  Temporary hack while Rave guys fixing some issues with Multinode...
                Cookie cookie = GetSessionIdCookie(url);
                if (cookie != null)
                {
                    httpRequest.CookieContainer = new CookieContainer();
                    httpRequest.CookieContainer.Add(cookie);
                }

                ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)System.Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, new RemoteCertificateValidationCallback(OAuthHttpRequest.ValidateRemoteCertificate));
                byte[] bytes = encoding.GetBytes(string.Empty);
                httpRequest.ContentType = "application/x-www-form-urlencoded";
                httpRequest.ContentLength = (long)bytes.Length;
                using (Stream requestStream = httpRequest.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                    using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpRequest.GetResponse())
                    {
                        using (Stream responseStream = httpWebResponse.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(responseStream))
                            {
                                oauthResponse = new OAuthHttpResponse();
                                oauthResponse.response = streamReader.ReadToEnd();
                                oauthResponse.status_code = httpWebResponse.StatusCode;
                            }
                        }
                    }
                }

                Trace.WriteLine("Response: " + oauthResponse.response);
            }
            catch (System.Exception ex)
            {
                oauthResponse.status_code = HttpStatusCode.InternalServerError;

                Trace.TraceError(ex.Message);
            }
            finally
            {
            }

            Trace.WriteLine("==============================================================");

            return oauthResponse;
        }

        private static Cookie GetSessionIdCookie(string url)
        {
            string sessionId = null;

            HttpCookie sessionIdCookie = HttpContext.Current.Request.Cookies[RSCIDAuthentication.RSCID_SESSION_ID_COOKIE_NAME];
            if (sessionIdCookie != null)
                sessionId = sessionIdCookie.Value;

            if (!string.IsNullOrEmpty(sessionId))
            {
                string[] hostParts = new System.Uri(url).Host.Split('.');
                string domain = String.Join(".", hostParts.Skip(Math.Max(0, hostParts.Length - 2)).Take(2));

                Cookie cookie = new Cookie();
                cookie.Name = "ASP.Net_SessionId";
                cookie.Value = sessionId;
                cookie.Domain = "." + domain;

                return cookie;
            }

            return null;
        }

        private static bool ValidateRemoteCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return ConfigurationManager.AppSettings["IgnoreSslErrors"] == null || System.Convert.ToBoolean(ConfigurationManager.AppSettings["IgnoreSslErrors"]) || policyErrors == SslPolicyErrors.None;
        }

        private static void AddRequestHeader(HttpWebRequest httprequest, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    object obj = args[i];
                    if (obj != null)
                    {
                        Type type = obj.GetType();
                        PropertyInfo[] properties = type.GetProperties();
                        for (int j = 0; j < properties.Length; j++)
                        {
                            PropertyInfo propertyInfo = properties[j];
                            object value = propertyInfo.GetValue(obj, null);
                            if (propertyInfo.PropertyType == typeof(byte[]) && value == null)
                            {
                            }
                            else
                            {
                                httprequest.Headers.Add(propertyInfo.Name, (value == null) ? string.Empty : value.ToString());
                                Trace.WriteLine(string.Format("{0}: {1}", propertyInfo.Name, value));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get Encrypted string using using the System.Security.Cryptography.SHA1.
        /// </summary>
        /// <param name="message">String to encrypt</param>
        /// <param name="secretkey">secret key</param>
        /// <returns>encrypted string</returns>
        public static string GetEncryptedString(string message, string secretkey)
        {
            string encryptedString = string.Empty;
            try
            {
                ASCIIEncoding encoding = new ASCIIEncoding();

                byte[] keyByte = encoding.GetBytes(secretkey);

                //Computes a Hash-based Message Authentication Code (HMAC) using the System.Security.Cryptography.SHA1
                using (HMACSHA1 hmacsha1 = new HMACSHA1(keyByte))
                {
                    byte[] messageBytes = encoding.GetBytes(message);

                    // Computes the hash value for the specified byte array.
                    byte[] hashmessage = hmacsha1.ComputeHash(messageBytes);

                    encryptedString = ByteToString(hashmessage);
                }
            }
            finally {
            }

            return encryptedString;
        }

        /// <summary>
        /// Converts byte to encrypted string
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        private static string ByteToString(byte[] buff)
        {
            string sbinary = "";

            for (int i = 0; i < buff.Length; i++)
                sbinary += buff[i].ToString("X2"); // hex format

            return (sbinary);
        }
    }
}
