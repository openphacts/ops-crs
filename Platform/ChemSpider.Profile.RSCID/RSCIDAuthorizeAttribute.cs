using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ChemSpider.Profile.RSCID
{
    public class RSCIDAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
			if (RSCIDAuthentication.IsEnabled)
				RSCIDAuthentication.ProcessUnauthorized();

			base.HandleUnauthorizedRequest(filterContext);
        }
    }
}
