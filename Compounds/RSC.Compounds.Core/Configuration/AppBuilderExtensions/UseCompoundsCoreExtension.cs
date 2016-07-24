using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Owin
{
	public static class UseCompoundsCoreExtension
	{
		public static IAppBuilder UseCompoundsCore(this IAppBuilder app, ContainerBuilder builder)
		{
			builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());

			return app;
		}
	}
}
