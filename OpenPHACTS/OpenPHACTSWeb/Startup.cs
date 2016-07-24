using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Http;

[assembly: OwinStartup(typeof(OpenPHACTSWeb.Startup))]

namespace OpenPHACTSWeb
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			// For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888

			var builder = new ContainerBuilder();
			builder.RegisterControllers(Assembly.GetExecutingAssembly());
			builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

			builder.RegisterType<ChemSpider.Compounds.Database.SQLCompoundsStore>().As<ChemSpider.Compounds.ICompoundsStore>();
			builder.RegisterType<ChemSpider.Compounds.Database.SQLSubstancesStore>().As<ChemSpider.Compounds.ISubstancesStore>();
			builder.RegisterType<ChemSpider.Compounds.Search.CSCSqlSearchProvider>().As<ChemSpider.Search.ISqlSearchProvider>();

			app.UseCompoundsCore(builder);

			app.UseCompoundsWebApi(builder);

			var container = builder.Build();

			DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
			GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
		}
	}
}
