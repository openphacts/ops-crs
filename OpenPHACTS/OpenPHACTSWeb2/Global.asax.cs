using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebApiContrib.Formatting.Jsonp;

namespace OpenPHACTSWeb2
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

			GlobalConfiguration.Configuration.AddJsonpFormatter();

			GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
			GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

			//GlobalConfiguration.Configuration.Filters.Add(new ElmahHandleErrorApiAttribute());
		}
    }
}
