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

using ChemSpider.Profile.Data.Models;
using ChemSpider.Profile.Data.Providers;

namespace ChemSpider.Profile.RSCID
{
    public class RSCIDLightAuthenticationModule : IHttpModule, IRequiresSessionState
    {
        private RSCIDLightAuthentication rscId = new RSCIDLightAuthentication();

        public String ModuleName
        {
            get { return "RSC-ID OAuth Light Authentication Module"; }
        }

        // In the Init function, register for HttpApplication 
        // events by adding your handlers.
        public void Init(HttpApplication application)
        {
            //  Check if RSC ID integration enabled...
            if (RSCIDLightAuthentication.IsEnabled)
            {
                application.AuthenticateRequest += new EventHandler(this.Application_AuthenticateRequest);
                application.PostAuthenticateRequest += new EventHandler(this.Application_PostAuthenticateRequest);
            }
        }

        private bool SkipRequest(HttpRequest request)
        {
            return !RSCIDLightAuthentication.IsEnabled ||                                                //  remove request if RSCID disabled
                RSCIDLightAuthentication.SkipExtension(Path.GetExtension(request.Url.LocalPath)) ||      //  if the extension in the skip list
                RSCIDLightAuthentication.SkipFileName(request.Url.LocalPath) ||                          //  if the file in the skip list
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
/*
            RSCIDLightAuthentication.ProcessAuthCookie();

            //  this code will work only in case of ASP.NET WabForms when the authorization settings specified in web.config file.
            //  in case of MVC applications RSCIDAuthorization attribute MUST be used.
            if (HttpContext.Current.User != null && !UrlAuthorizationModule.CheckUrlAccessForPrincipal(HttpContext.Current.Request.Path, HttpContext.Current.User, "GET"))
            {
                RSCIDLightAuthentication.ProcessUnauthorized();
            }
*/
            Trace.WriteLine("Application_PostAuthenticateRequest: " + HttpContext.Current.Request.Url.AbsoluteUri);
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

                RSCIDLightAuthentication.SetRSCIDSessionIdCookie(request.QueryString["sid"]);

                if (statusCode == "norec")
                {
                    //  user is not authenticated on RSC-ID side... 

                    //  clean information on ChemSpider side if it was there before...
                    RSCIDLightAuthentication.SignOut();

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
                        RSCIDLightAuthentication.SaveAccessToken(token);

                        if (returnUrl.Contains("ConnectWithRSCID"))
                        {
                            //  save information about authenticated in RSC ID user in order to get it later... 
                            RSCIDLightAuthentication.SaveAuthenticatedRSCIDUser(rscUser);
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

                                RSCIDLightAuthentication.SignIn(username, rscUser);

                                Trace.WriteLine("User was found! Sign In!");
                            }
                            else
                            {
                                //  save information about authenticated in RSC ID user in order to get it later... 
                                RSCIDLightAuthentication.SaveAuthenticatedRSCIDUser(rscUser);

                                //  redirect to manual accounts' connection page...
                                RSCIDLightAuthentication.ManualAccountsConnection();
                            }
                        }
                    }
                    else
                    {
                        //  if no user information returned back, logout autenticated user just in case...
                        RSCIDLightAuthentication.SignOut();

                        if (error != null)
                        {
                            Trace.TraceError("Cannot get User's information from RSC ID: {0}: {1}", error.error, error.error_description);
                        }
                    }
                }

                response.Redirect(returnUrl, true);
            }
            else if (request.Url.LocalPath.Contains("RSCLogoutCallback"))
            {
/*
                //  Sign Out authenticated user...
                RSCIDLightAuthentication.SignOut();

                //  This is VERY IMPORTANT redirect on the same page to clean the cookies from the Request!!! DO NOT REMOVE IT!!!
                //  Redirect on default page
                response.Redirect(RSCIDLightAuthentication.ApplicationHost, true);
*/
            }
            else
            {
/*
                //  check access token here in otder to be sure the user is still logged in...
                AccessToken token = RSCIDLightAuthentication.GetSavedAccessToken();
                if (token != null)
                {
                    if (!RSCIDLightAuthentication.IsLoggedIn(token))
                    {
                        //  if user already not logged in RSC ID we have to logged him out as well...
                        RSCIDLightAuthentication.SignOut();

                        //  This is VERY IMPORTANT redirect on the same page to clean the cookies from the Request!!! DO NOT REMOVE IT!!!
                        //  Redirect on default page
                        response.Redirect(RSCIDLightAuthentication.ApplicationHost, true);
                    }
                }
                else if (RSCIDLightAuthentication.FirstTimeRequest)
                {
                    //  either cookie has expired or we haven't checked RSC-ID yet, so do the check right now...
                    RSCIDLightAuthentication.Authorize();
                }
*/
            }
        }

        public void Dispose()
        {
        }
    }
}
