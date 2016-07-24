using Autofac;
using Autofac.Integration.WebApi;
using System.Reflection;

namespace Owin
{
	public static class UseChemSpiderPropertiesPluginExtension
	{
        public static IAppBuilder UseChemSpiderPropertiesPlugin(this IAppBuilder app, ContainerBuilder builder)
		{
			builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());

			builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

			return app;
		}
	}
}
