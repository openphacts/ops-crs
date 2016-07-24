using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;
using System.Security.Principal;
using System.Web.Configuration;
using System.Web.SessionState;
using System.Web.Security;
using System.Web.Script.Serialization;

//using ChemSpider.Utilities;
using ChemSpider.Profile.Data.Models;
using ChemSpider.Profile.Data.Providers;

namespace ChemSpider.Profile.RSCID
{
    public class AuthenticationModule : IHttpModule, IRequiresSessionState
    {
        private RSCIDAuthentication rscId = new RSCIDAuthentication();

        public String ModuleName
        {
            get { return "RSC-ID OAuth Authentication Module"; }
        }

        // In the Init function, register for HttpApplication 
        // events by adding your handlers.
        public void Init(HttpApplication application)
        {
            //  Check if RSC ID integration enabled...
            if (RSCIDAuthentication.IsEnabled)
            {
                application.AuthenticateRequest += new EventHandler(this.Application_AuthenticateRequest);
                application.PostAuthenticateRequest += new EventHandler(this.Application_PostAuthenticateRequest);
                //application.AuthorizeRequest += new EventHandler(this.Application_AuthorizeRequest);
            }
        }

        private bool SkipRequest(HttpRequest request)
        {
            return !RSCIDAuthentication.IsEnabled ||                                                //  remove request if RSCID disabled
                RSCIDAuthentication.SkipExtension(Path.GetExtension(request.Url.LocalPath)) ||      //  if the extension in the skip list
                RSCIDAuthentication.SkipFileName(request.Url.LocalPath) ||                          //  if the file in the skip list
                request.Browser.Crawler ||                                                          //  if request came from a crawler
                string.IsNullOrEmpty(request.UserAgent) ||                                          //  if the User-Agent is not specified (definitly not a browser)
                request.Browser.Browser.ToLower().Equals("unknown");                                //  if the browser is Unknown
        }

        private void Application_PostAuthenticateRequest(Object source, EventArgs e)
        {
            HttpApplication application = (HttpApplication)source;
            HttpContext context = application.Context;

            //  we don't have to process all requests...
            if (SkipRequest(context.Request))
                return;

            RSCIDAuthentication.ProcessAuthCookie();

            //  this code will work only in case of ASP.NET WabForms when the authorization settings specified in web.config file.
            //  in case of MVC applications RSCIDAuthorization attribute MUST be used.
            if (HttpContext.Current.User != null && !UrlAuthorizationModule.CheckUrlAccessForPrincipal(HttpContext.Current.Request.Path, HttpContext.Current.User, "GET"))
            {
                RSCIDAuthentication.ProcessUnauthorized();
            }

            Trace.WriteLine("Application_PostAuthenticateRequest: " + HttpContext.Current.Request.Url.AbsoluteUri);
        }

        private void Application_AuthorizeRequest(Object source, EventArgs e)
        {

            HttpApplication application = (HttpApplication)source;
            HttpContext context = application.Context;

            //  we don't have to process all requests...
            if (SkipRequest(context.Request))
                return;
            /*
                        //  this code will work only in case of ASP.NET WabForms when the authorization settings specified in web.config file.
                        //  in case of MVC applications RSCIDAuthorization attribute MUST be used.
                        if (!UrlAuthorizationModule.CheckUrlAccessForPrincipal(HttpContext.Current.Request.Path, HttpContext.Current.User, "GET"))
                        {
                            RSCIDAuthentication.ProcessUnauthorized();
                        }
            */
            Trace.WriteLine("Application_AuthorizeRequest: " + HttpContext.Current.Request.Url.AbsoluteUri);
        }

        private void Application_AuthenticateRequest(Object source, EventArgs e)
        {
            HttpApplication application = (HttpApplication)source;
            HttpContext context = application.Context;
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            //  we don't have to process all requests...
            if (SkipRequest(context.Request))
                return;

            Trace.WriteLine("Application_AuthenticateRequest: " + HttpContext.Current.Request.Url.ToString());

            if (request.Url.LocalPath.Contains("RSCLoginCallback"))
            {
                //  we got response from RSC-ID system...
                string code = request.QueryString["code"];
                string statusCode = request.QueryString["statusCode"];
                string returnUrl = request.QueryString["returnurl"];

                RSCIDAuthentication.SetRSCIDSessionIdCookie(request.QueryString["sid"]);

                if (statusCode == "norec")
                {
                    //  user is not authenticated on RSC-ID side... 

                    //  clean information on ChemSpider side if it was there before...
                    RSCIDAuthentication.SignOut();

                    Trace.WriteLine("statusCode: norec");
                }
                else if (!string.IsNullOrEmpty(code))
                {
                    //  user authenticated and access code been gotten successfully... 

                    Trace.WriteLine("code: " + code);

                    //  get access token...
                    AccessToken token = rscId.GetAccessToken(code, returnUrl);

                    RSCError error = null;

                    //  get user's details...
                    RSCUser rscUser = rscId.GetRSCIDUser(token, out error);

                    //  try to connect user with ChemSpider account...
                    if (rscUser != null)
                    {
                        //  save access token for using it later... 
                        RSCIDAuthentication.SaveAccessToken(token);

                        if (returnUrl.Contains("ConnectWithRSCID"))
                        {
                            //  save information about authenticated in RSC ID user in order to get it later... 
                            RSCIDAuthentication.SaveAuthenticatedRSCIDUser(rscUser);
                        }
                        else
                        {
                            UserData data = new UserData();

                            string username = data.GetUsernameByExternalID("rsc", rscUser.UserDetails.UserID);

                            Trace.WriteLine("ChemSpider username: " + username);

                            if (!string.IsNullOrEmpty(username))
                            {
                                //  user was successfully found in ChemSpider by RSC-ID...

                                rscId.RefreshRSCAccountInformation(username, rscUser);

                                RSCIDAuthentication.SignIn(username, rscUser);

                                Trace.WriteLine("User was found! Sign In!");
                            }
                            else
                            {
                                //  save information about authenticated in RSC ID user in order to get it later... 
                                RSCIDAuthentication.SaveAuthenticatedRSCIDUser(rscUser);

                                //  redirect to manual accounts' connection page...
                                RSCIDAuthentication.ManualAccountsConnection();
                            }
                        }
                    }
                    else
                    {
                        //  if no user information returned back, logout autenticated user just in case...
                        RSCIDAuthentication.SignOut();

                        if(error != null) {
                            Trace.TraceError("Cannot get User's information from RSC ID: {0}: {1}", error.error, error.error_description);
                        }
                    }
                }

                response.Redirect(returnUrl, true);
            }
            else if (request.Url.LocalPath.Contains("RSCLogoutCallback"))
            {
                //  Sign Out authenticated user...
                RSCIDAuthentication.SignOut();

                //  This is VERY IMPORTANT redirect on the same page to clean the cookies from the Request!!! DO NOT REMOVE IT!!!
                //  Redirect on default page
                response.Redirect(RSCIDAuthentication.ApplicationHost, true);
            }
            else
            {
                //  check access token here in otder to be sure the user is still logged in...
                AccessToken token = RSCIDAuthentication.GetSavedAccessToken();
                if (token != null)
                {
                    if (!RSCIDAuthentication.IsLoggedIn(token))
                    {
                        //  if user already not logged in RSC ID we have to logged him out as well...
                        RSCIDAuthentication.SignOut();

                        //  This is VERY IMPORTANT redirect on the same page to clean the cookies from the Request!!! DO NOT REMOVE IT!!!
                        //  Redirect on default page
                        response.Redirect(RSCIDAuthentication.ApplicationHost, true);
                    }
                }
                else if (RSCIDAuthentication.FirstTimeRequest)
                {
                    //  either cookie has expired or we haven't checked RSC-ID yet, so do the check right now...
                    RSCIDAuthentication.Authorize();
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
