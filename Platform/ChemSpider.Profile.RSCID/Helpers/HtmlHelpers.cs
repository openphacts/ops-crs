using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Net;

using ChemSpider.Profile.Data.Providers;

namespace ChemSpider.Profile.RSCID.Helpers
{
    public static class HtmlHelpers
    {
        private static RSCIDAuthentication rscid = new RSCIDAuthentication();

        public static MvcHtmlString RSCHeader(this HtmlHelper htmlHelper)
        {
            return MvcHtmlString.Create(rscid.RSCHeader());
        }

        public static MvcHtmlString RSCFooter(this HtmlHelper htmlHelper)
        {
            return MvcHtmlString.Create(rscid.RSCFooter());
        }

        public static MvcHtmlString Avatar(this HtmlHelper htmlHelper)
        {
            UserData db = new UserData();

            int usr_id = db.GetUserId(htmlHelper.ViewContext.HttpContext.User.Identity.Name);

            UrlHelper urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            string url = urlHelper.Action("Activity", "Account");

            var a = new TagBuilder("a");
            a.AddCssClass("avatar");
            a.Attributes.Add("href", url);

            var img = new TagBuilder("img");
            img.Attributes.Add("src", db.GetUserAvatar(usr_id) ?? "/Images/default_avatar.jpg");
            a.InnerHtml = img.ToString();

            var span = new TagBuilder("span");
            span.AddCssClass("caret");
            a.InnerHtml += span.ToString();

            return MvcHtmlString.Create(a.ToString());
        }
    }
}
