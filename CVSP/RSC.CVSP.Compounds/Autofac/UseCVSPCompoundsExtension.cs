using Autofac;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owin
{
	public static class UseCVSPCompoundsExtension
	{
		public static IAppBuilder UseCVSPCompounds(this IAppBuilder app, ContainerBuilder builder)
		{
			builder.RegisterModule<RSC.CVSP.Compounds.Autofac.CompoundsBaseModule>();

			return app;
		}
	}
}
