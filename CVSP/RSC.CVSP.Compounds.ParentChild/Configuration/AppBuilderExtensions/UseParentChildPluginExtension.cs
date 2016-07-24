using Autofac;
using Autofac.Integration.WebApi;
using System.Reflection;

namespace Owin
{
	public static class UseParenChildPluginExtension
	{
		public static IAppBuilder UseParentChildPlugin(this IAppBuilder app, ContainerBuilder builder)
		{
			builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());

			builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

			return app;
		}
	}
}
