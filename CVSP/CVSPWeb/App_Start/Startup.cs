using CVSPWeb.App_Start;
using Microsoft.Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace CVSPWeb.App_Start
{
	using System.Reflection;
	using System.Web.Http;
	using System.Web.Mvc;
	using Autofac;
	using Autofac.Extras.CommonServiceLocator;
	using Autofac.Integration.Mvc;
	using Autofac.Integration.WebApi;
	using Microsoft.Practices.ServiceLocation;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;
	using Owin;
	using RSC.CVSP;
	using RSC.CVSP.Compounds;
	using RSC.CVSP.EntityFramework;
	using RSC.CVSP.Search;
	using RSC.Logging;
	using RSC.Logging.EntityFramework;
	using RSC.Process;
	using RSC.Process.EntityFramework;
	using RSC.Properties;
	using RSC.Properties.EntityFramework;
	using RSC.Search;

	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			// For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888

			var builder = new ContainerBuilder();
			builder.RegisterControllers(Assembly.GetExecutingAssembly());
			builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

			builder.Register(c => new EFLogStore("LoggingConnection")).As<ILogStore>();
			builder.Register(c => new EFPropertyStore("PropertiesConnection")).As<IPropertyStore>();
			builder.Register(c => new EFCVSPStore("CVSPConnection", 60)).As<ICVSPStore>();
			builder.Register(c => new EFStatistics("CVSPConnection", 60)).As<IStatistics>();
			builder.Register(c => new EFCVSPSearch("CVSPConnection", 60)).As<CVSPSearch>();
			builder.RegisterType<OperationsManager>().As<IOperationsManager>();
			builder.RegisterType<FileStorage>().As<IFileStorage>();
			builder.RegisterType<EFJobManager>().As<IJobManager>();
			builder.RegisterType<EFChunkManager>().As<IChunkManager>();
			builder.RegisterType<MemoryRequestStorage>().As<IRequestStorage>();

			app.UseParentChildPlugin(builder);
			app.UseChemSpiderPropertiesPlugin(builder);

			var container = builder.Build();

			ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(container));
			DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
			GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

			Init.Initialize();

			// Remove the XML formatter
			GlobalConfiguration.Configuration.Formatters.Remove(GlobalConfiguration.Configuration.Formatters.XmlFormatter);

			GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
			GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
			GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
		}
	}
}
