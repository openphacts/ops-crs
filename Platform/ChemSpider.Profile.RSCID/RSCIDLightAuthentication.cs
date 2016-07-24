using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.Script.Serialization;

using ChemSpider.Profile.RSCID.Helpers;
using ChemSpider.Profile.Data.Models;
using ChemSpider.Profile.Data.Providers;
//using ChemSpider.Utilities;

namespace ChemSpider.Profile.RSCID
{
    public class RSCIDLightAuthentication
    {
        private static IEnumerable<string> _extensionsToSkip = null;
        private static IEnumerable<string> _filesToSkip = null;

        private class LoggedInModel
        {
            public bool LoggedIn { get; set; }
        }

        public const string RSCID_PROVIDER_NAME = "rsc";

        public const string RSCID_ACCESS_TOKEN_COOKIE_NAME = ".RSCID-ACCESS-TOKEN";
        public const string RSCID_COOKIE_NAME = ".RSCID-AUTH";
        public const string RSCID_TEMP_COOKIE_NAME = ".RSCID-USER";
        public const string RSCID_SESSION_ID_COOKIE_NAME = ".RSCID-SESSION-ID";
        public const string RSCID_LOGGEDIN_COOKIE_NAME = ".RSCID-LOGGED-IN";

        private const string NOREC = "norec";

        private static RSCIDSection Config { get { return ConfigurationManager.GetSection("RSCID") as RSCIDSection; } }

        public static bool IsEnabled { get { return Config != null && !string.IsNullOrEmpty(PlatformId); } }

        public static string AuthorizationURL { get { return Config != null ? Config.Authorization.Url : null; } }
        public static string AccessTokenURL { get { return Config != null ? Config.AccessToken.Url : null; } }
        public static string UserDetailsURL { get { return Config != null ? Config.UserDetails.Url : null; } }
        public static string IsLoggedInURL { get { return Config != null ? Config.IsLoggedIn.Url : null; } }

        public static string PlatformId { get { return Config != null ? Config.PlatformId.Value : null; } }
        public static string SecretKey { get { return Config != null ? Config.SecretKey.Value : null; } }
        public static string CookieDomain { get { return Config != null && !string.IsNullOrEmpty(Config.CookieDomain.Value) ? Config.CookieDomain.Value : null; } }
        public static IEnumerable<string> ExtensionsToSkip
        {
            get
            {
                if (_extensionsToSkip == null)
                {
                    if (Config == null)
                        return new List<string>();

                    _extensionsToSkip = Config.ExtensionsToSkip.Value.Split('|').Select(m => m.ToLower()).ToList();
                }

                return _extensionsToSkip;
            }
        }
        public static IEnumerable<string> FilesToSkip
        {
            get
            {
                if (_filesToSkip == null)
                {
                    if (Config == null)
                        return new List<string>();

                    _filesToSkip = from file in Config.FilesToSkip.Cast<FileElement>() select file.Path.ToLower();
                }

                return _filesToSkip;
            }
        }
        public static int LoggedInCookieExpiration { get { return Config.LoggedInCookieExpiration.Value > 0 ? Config.LoggedInCookieExpiration.Value : 60; } }

        public static string HeaderURL { get { return Config != null ? Config.Header.Url : null; } }
        public static string FooterURL { get { return Config != null ? Config.Footer.Url : null; } }

        public static string AccessDeniedPage { get { return !string.IsNullOrEmpty(Config.AccessDeniedPage.Url) ? Config.AccessDeniedPage.Url : "/Account/AccessDenied"; } }
        public static string ManualConnectWithRSCIDPage { get { return !string.IsNullOrEmpty(Config.ManualConnectWithRSCIDPage.Url) ? Config.ManualConnectWithRSCIDPage.Url : "/Account/MultipleAccounts"; } }

        public static string ApplicationHost
        {
            get
            {
                HttpRequest request = HttpContext.Current.Request;
                return !string.IsNullOrEmpty(Config.ApplicationHost.Url) ? Config.ApplicationHost.Url : request.Url.Scheme + System.Uri.SchemeDelimiter + request.Url.Host + (request.Url.IsDefaultPort ? "" : ":" + request.Url.Port);
            }
        }

        public static bool SkipExtension(string extension)
        {
            return ExtensionsToSkip.Contains(extension.ToLower());
        }

        public static bool SkipFileName(string fileName)
        {
            return FilesToSkip.Contains(fileName.ToLower());
        }

        public static void SetRSCIDSessionIdCookie(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                HttpContext.Current.Response.SetCookie(new HttpCookie(RSCID_SESSION_ID_COOKIE_NAME, sessionId) { Domain = CookieDomain, Expires = DateTime.Now.AddDays(-1) });
            else
                HttpContext.Current.Response.SetCookie(new HttpCookie(RSCID_SESSION_ID_COOKIE_NAME, sessionId) { Domain = CookieDomain });
        }

        public static bool FirstTimeRequest
        {
            get { return HttpContext.Current.Request.Cookies.Get(RSCID_COOKIE_NAME) == null && HttpContext.Current.Request.Cookies.Get(RSCID_TEMP_COOKIE_NAME) == null; }
        }

        public static string GetAuthorizeURL(bool loginPageRequired = false, string returnUrl = null)
        {
            HttpContext context = HttpContext.Current;

            string state = Guid.NewGuid().ToString();

            string authorizeUrl = string.Format("{0}?client_id={1}&response_type=code&state={2}&scope=developer&isLoginPageRequired={3}&redirect_uri={4}", AuthorizationURL, PlatformId, state, loginPageRequired, returnUrl ?? context.Server.UrlEncode(ApplicationHost + context.Request.RawUrl));

            Trace.WriteLine("GetAuthorizeURL: " + authorizeUrl);

            return authorizeUrl;
        }

        #region General OAuth oprations
        public static void Authorize(bool loginPageRequired = false, string returnUrl = null)
        {
            string authorizationUrl = GetAuthorizeURL(loginPageRequired, returnUrl);

            Trace.WriteLine("Authorize: " + authorizationUrl);

            HttpContext.Current.Response.Redirect(authorizationUrl, true);
        }

        public AccessToken GetAccessToken(string accessCode, string redirectUrl)
        {
            OAuthHttpResponse response = OAuthHttpRequest.HttpRequestToRSCID(AccessTokenURL, new
            {
                grant_type = "authorization_code",
                code = accessCode,
                client_id = PlatformId,
                redirect_uri = redirectUrl
            });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            return javaScriptSerializer.Deserialize<AccessToken>(response.response);
        }

        public AccessToken RefreshAccessToken(string refreshToken, string redirectUrl)
        {
            string nonceString = Guid.NewGuid().ToString();

            //Concat access token, platformId and nonce using. 
            string concateString = String.Concat(refreshToken, PlatformId, nonceString);
            //Generate Encrypted key
            string encryptedString = OAuthHttpRequest.GetEncryptedString(concateString, SecretKey);

            OAuthHttpResponse response = OAuthHttpRequest.HttpRequestToRSCID(AccessTokenURL, new
            {
                grant_type = "refresh_token",
                refresh_token = refreshToken,
                client_id = PlatformId,
                nonce = nonceString,
                encrypted_string = encryptedString
            });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            return javaScriptSerializer.Deserialize<AccessToken>(response.response);
        }

        public RSCUser GetRSCIDUser(AccessToken token, out RSCError error)
        {
            error = null;

            if (token == null || string.IsNullOrEmpty(token.access_token))
            {
                error = new RSCError()
                {
                    error = "access_token_invalid",
                    error_title = "Invalid access token",
                    error_description = "Access token is null or empty. Check the access tocket that you pass to the method"
                };
                return null;
            }

            string nonceString = Guid.NewGuid().ToString();

            //Concat access token, platformId and nonce using. 
            string concateString = String.Concat(token.access_token, PlatformId, nonceString);
            //Generate Encrypted key
            string encryptedString = OAuthHttpRequest.GetEncryptedString(concateString, SecretKey);

            OAuthHttpResponse response = OAuthHttpRequest.HttpRequestToRSCID(
                UserDetailsURL,
                new
                {
                    access_token = token.access_token,
                    client_id = PlatformId,
                    nonce = nonceString,
                    encrypted_string = encryptedString,
                    scope = "reader"
                });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            //  try to check if the response is Error...
            error = javaScriptSerializer.Deserialize<RSCError>(response.response);
            if (error != null && error.IsValid)
                return null;

            RSCUser user = javaScriptSerializer.Deserialize<RSCUser>(response.response);

            if (user != null && user.UserDetails != null)
            {
                user.Raw = response.response;
                return user;
            }

            return null;
        }

        public static bool IsLoggedIn(AccessToken token)
        {
            //  KINDA HACK THAT SHOULD BE REMOVED LATER!!! 
            //  in case if URL is not specified, the method should always returns true like the user still authenticated... 
            if (string.IsNullOrEmpty(IsLoggedInURL))
                return true;

            //  Check if the logged in status was checked recently... 
            HttpCookie cookie = HttpContext.Current.Request.Cookies[RSCID_LOGGEDIN_COOKIE_NAME];
            if (cookie != null)
                return true;

            OAuthHttpResponse response = OAuthHttpRequest.HttpRequestToRSCID(
                IsLoggedInURL,
                new
                {
                    refresh_token = token.refresh_token
                });

            //  we didn't get appropriate response from RSC ID...
            if (string.IsNullOrEmpty(response.response))
                return false;

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            LoggedInModel result = javaScriptSerializer.Deserialize<LoggedInModel>(response.response);

            //  if the user still logged id cache this information for a while...
            if (result.LoggedIn && LoggedInCookieExpiration > 0)
                HttpContext.Current.Response.SetCookie(new HttpCookie(RSCID_LOGGEDIN_COOKIE_NAME) { Expires = DateTime.Now.AddSeconds(LoggedInCookieExpiration), Domain = CookieDomain, Value = "1" });

            return result.LoggedIn;
        }

        #endregion

        public void ConnectChemSpiderAndRSCAccounts(int chemSpiderUserId, RSCUser rscUser)
        {
            if (chemSpiderUserId > 0)
            {
                UserData userProvider = new UserData();

                userProvider.AddOAuthMembership(chemSpiderUserId, new OAuthMembership
                {
                    Provider = RSCID_PROVIDER_NAME,
                    ProviderUserId = rscUser.UserDetails.UserID,
                    FirstName = rscUser.UserDetails.GivenName,
                    LastName = rscUser.UserDetails.FamilyName,
                    Email = rscUser.UserDetails.Email,
                    Username = rscUser.UserDetails.Username,
                    Name = rscUser.UserDetails.PreferredDisplayName,
                    AvatarUrl = rscUser.ProfileImageUrl,
                    ProfileUrl = rscUser.ProfileUrl,
                    IsEmailVerified = rscUser.IsEmailVerified,
                    Raw = rscUser.Raw
                });
            }
        }

        public void RefreshRSCAccountInformation(string username, RSCUser rscUser)
        {
            UserData userProvider = new UserData();

            userProvider.UpdateOAuthMembership(new OAuthMembership
            {
                Provider = RSCID_PROVIDER_NAME,
                ProviderUserId = rscUser.UserDetails.UserID,
                FirstName = rscUser.UserDetails.GivenName,
                LastName = rscUser.UserDetails.FamilyName,
                Email = rscUser.UserDetails.Email,
                Username = rscUser.UserDetails.Username,
                Name = rscUser.UserDetails.PreferredDisplayName,
                AvatarUrl = rscUser.ProfileImageUrl,
                ProfileUrl = rscUser.ProfileUrl,
                IsEmailVerified = rscUser.IsEmailVerified,
                Raw = rscUser.Raw
            });
        }

        #region Sign In/Sign Out
        //  Use this Sign In method only for simulation when you want to authenticate user by ChemSpider username. If user is not connected with RSCID yet then no one will be logged in...
        public static void SignIn(string username)
        {
            UserData userProvider = new UserData();

            RSCUser user = userProvider.GetRSCUser(username);

            if (user != null)
                SignIn(username, user);
        }

        public static void SignIn(string username, RSCUser user)
        {
            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                     1,
                     username,
                     DateTime.Now,
                     DateTime.Now.AddSeconds(1200),
                     false,
                     user.UserDetails.UserID);

            string ticket = FormsAuthentication.Encrypt(authTicket);
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(RSCID_COOKIE_NAME, ticket) { Domain = CookieDomain });

            HttpContext.Current.User = new RSCPrincipal(new GenericIdentity(username, "rsc-id"), user);

            //  Just in case update ChemSpider profile as well...
            ChemSpiderProfile profile = ChemSpiderProfile.GetUserProfile(username);
            if (profile != null)
            {
                profile.SetPropertyValue("Email", user.UserDetails.Email);
                profile.SetPropertyValue("FirstName", user.UserDetails.GivenName);
                profile.SetPropertyValue("LastName", user.UserDetails.FamilyName);
                profile.SetPropertyValue("DisplayName", user.UserDetails.PreferredDisplayName);

                profile.Save();
            }
        }

        public static void SignOut()
        {
            HttpResponse response = HttpContext.Current.Response;

            response.SetCookie(new HttpCookie(RSCID_COOKIE_NAME, NOREC) { Domain = CookieDomain });
            response.SetCookie(new HttpCookie(RSCID_ACCESS_TOKEN_COOKIE_NAME) { Expires = DateTime.Now.AddDays(-1), Domain = CookieDomain });
            response.SetCookie(new HttpCookie(RSCID_TEMP_COOKIE_NAME) { Expires = DateTime.Now.AddDays(-1), Domain = CookieDomain });
            response.SetCookie(new HttpCookie(RSCID_LOGGEDIN_COOKIE_NAME) { Expires = DateTime.Now.AddDays(-1), Domain = CookieDomain });
        }
        #endregion

        #region Session access token
        public static void SaveAccessToken(AccessToken token)
        {
            if (token == null)
                return;

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            FormsAuthenticationTicket formTicket = new FormsAuthenticationTicket(
                     1,
                     token.access_token,
                     DateTime.Now,
                     DateTime.Now.AddSeconds(1200),
                     false,
                     serializer.Serialize(token));

            string ticket = FormsAuthentication.Encrypt(formTicket);
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(RSCID_ACCESS_TOKEN_COOKIE_NAME, ticket) { Domain = CookieDomain });
        }

        public static AccessToken GetSavedAccessToken()
        {
            try
            {
                HttpCookie cookie = HttpContext.Current.Request.Cookies[RSCID_ACCESS_TOKEN_COOKIE_NAME];
                if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
                {
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);

                    JavaScriptSerializer serializer = new JavaScriptSerializer();

                    return serializer.Deserialize<AccessToken>(ticket.UserData);
                }
            }
            catch (Exception)
            {
            }

            return null;
        }
        #endregion

        #region Temporary RSC user authenticated in RSC ID system
        public static void SaveAuthenticatedRSCIDUser(RSCUser user)
        {
            if (user == null)
                return;

            //  only necessary parameters needed for the association RSC IS account with the ChemSpider one...
            RSCUser usr = new RSCUser()
            {
                UserDetails = new RSCUser.Details()
                {
                    UserID = user.UserDetails.UserID,
                    Email = user.UserDetails.Email,
                    GivenName = user.UserDetails.GivenName,
                    FamilyName = user.UserDetails.FamilyName,
                    PreferredDisplayName = user.UserDetails.PreferredDisplayName
                }
            };

            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                     1,
                     user.UserDetails.UserID,
                     DateTime.Now,
                     DateTime.Now.AddSeconds(1200),
                     false,
                     usr.ToString());

            string ticket = FormsAuthentication.Encrypt(authTicket);
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(RSCID_TEMP_COOKIE_NAME, ticket) { Domain = CookieDomain });
        }

        public static RSCUser GetAuthenticatedRSCIDUser()
        {
            try
            {
                HttpCookie cookie = HttpContext.Current.Request.Cookies[RSCID_TEMP_COOKIE_NAME];
                if (cookie != null && !string.IsNullOrEmpty(cookie.Value) && !cookie.Value.Equals(NOREC))
                {
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);

                    RSCUser user = new RSCUser(ticket.UserData);

                    return user;
                }
            }
            catch (Exception)
            {
            }

            return null;
        }
        #endregion

        #region Post processing
        public static void ManualAccountsConnection()
        {
            Trace.WriteLine("Manual accounts' connection");

            NameValueCollection parameters = new NameValueCollection();

            parameters.Add("returnUrl", HttpContext.Current.Request.QueryString["returnUrl"]);

            string redirectUrl = ManualConnectWithRSCIDPage + "?" + string.Join("&", Array.ConvertAll(parameters.AllKeys, key => string.Format("{0}={1}", key, HttpUtility.UrlEncode(parameters[key]))));

            Trace.WriteLine("Redirect: " + redirectUrl);

            HttpContext.Current.Response.Redirect(redirectUrl, true);
        }

        public static void ProcessAuthCookie()
        {
            try
            {
                HttpCookie authCookie = HttpContext.Current.Request.Cookies[RSCID_COOKIE_NAME];
                if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value) && !authCookie.Value.Equals(NOREC))
                {
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);

                    string id = ticket.UserData;

                    UserData userProvider = new UserData();

                    string raw = userProvider.GetSocialNetworkRawData(id, "rsc");

                    if (!string.IsNullOrEmpty(raw))
                    {
                        HttpContext.Current.User = new RSCPrincipal(new GenericIdentity(ticket.Name, "rsc-id"), new RSCUser(raw));
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static void ProcessUnauthorized()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                //  User doesn't have permissions to have access to the page...
                HttpContext.Current.Response.Redirect(AccessDeniedPage, true);
            }
            else
            {
                //  user not authenticated... 
                RSCIDAuthentication.Authorize(true);
            }
        }
        #endregion

        #region Header/Footer
        public string RSCHeader()
        {
            if (IsEnabled && !string.IsNullOrEmpty(HeaderURL))
            {
                AccessToken token = GetSavedAccessToken();

                RSCUser user = HttpContext.Current.User.Identity.IsAuthenticated ? HttpContext.Current.User.RSCUser() : null;

                if (user == null)
                    user = GetAuthenticatedRSCIDUser();

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(HeaderURL);

                request.Headers.Add(new NameValueCollection() {
                    {"username", user == null ? string.Empty : user.UserDetails.PreferredDisplayName},
                    {"accesstoken", token != null ? token.access_token : string.Empty},
                    {"platformId", PlatformId},
                    {"secret", SecretKey},
                    {"returnUrl", HttpContext.Current.User.Identity.IsAuthenticated ? ApplicationHost  + "/RSCLogoutCallback" : ApplicationHost + HttpContext.Current.Request.RawUrl},
                    {"role", "Reader"}
                });

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }

            return string.Empty;
        }

        public string RSCFooter()
        {
            if (IsEnabled && !string.IsNullOrEmpty(FooterURL))
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(FooterURL);

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (StreamReader sReader = new StreamReader(responseStream))
                        {
                            return sReader.ReadToEnd();
                        }
                    }
                }
            }

            return string.Empty;
        }
        #endregion
    }
}
