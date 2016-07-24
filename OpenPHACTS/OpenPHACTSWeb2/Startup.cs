using Autofac;
using Autofac.Extras.CommonServiceLocator;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Microsoft.Owin;
using Microsoft.Practices.ServiceLocation;
using Owin;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;

[assembly: OwinStartupAttribute(typeof(OpenPHACTSWeb2.Startup))]
namespace OpenPHACTSWeb2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
			var builder = new ContainerBuilder();
			builder.RegisterControllers(Assembly.GetExecutingAssembly());
			builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

			//app.UseCompoundsCore(builder);

			app.UseCVSPCompounds(builder);

			app.UseCompoundsWebApi(builder);

			var container = builder.Build();

			ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(container));
			DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
			GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
		}
    }
}
