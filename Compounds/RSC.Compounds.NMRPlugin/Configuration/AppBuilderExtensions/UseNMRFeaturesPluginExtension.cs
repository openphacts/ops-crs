using Autofac;
using RSC.Compounds.NMRFeatures;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Owin
{
	public static class UseNMRFeaturesPluginExtension
	{
		public static IAppBuilder UseNMRFeaturesPlugin(this IAppBuilder app, ContainerBuilder builder)
		{
			//	initialize DB model necessary for plugin's functionality...
			Database.SetInitializer(new MigrateDatabaseToLatestVersion<NMRFeaturesDbContext, NMRFeaturesConfiguration>());
			NMRFeaturesDbContext db = new NMRFeaturesDbContext();
			db.Database.Initialize(true);

			builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());

			return app;
		}
	}
}
