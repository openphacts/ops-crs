using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Autofac;
using Autofac.Integration.WebApi;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using Autofac.Integration.Mvc;
using Microsoft.Practices.ServiceLocation;
using Autofac.Extras.CommonServiceLocator;

[assembly: OwinStartup(typeof(CompoundsWeb.Startup))]

namespace CompoundsWeb
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			// For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888

			var builder = new ContainerBuilder();
			builder.RegisterControllers(Assembly.GetExecutingAssembly());
			builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

			//builder.RegisterType<ChemSpider.Compounds.Search.CSCNMRFeaturesSearch>().As<ChemSpider.Compounds.Search.INMRFeaturesSearch>();
			//builder.RegisterType<ChemSpider.Compounds.Search.CSCSqlSearchProvider>().As<ChemSpider.Search.ISqlSearchProvider>();

			app.UseCompoundsCore(builder);

			app.UseCompoundsWebApi(builder);

			//app.UseNMRFeaturesPlugin(builder);

			var container = builder.Build();
			ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(container));
			DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
			GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
		}
	}
}
