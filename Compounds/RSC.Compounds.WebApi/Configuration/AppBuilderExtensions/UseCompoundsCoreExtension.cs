using Autofac;
using Autofac.Integration.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Owin
{
	public static class UseCompoundsWebApiExtension
	{
		public static IAppBuilder UseCompoundsWebApi(this IAppBuilder app, ContainerBuilder builder)
		{
			builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

			//	Resolve search dependencies...
			builder.RegisterType<RSC.Compounds.EntityFramework.EFSimpleSearch>().As<RSC.Compounds.Search.ISimpleSearch>();
			builder.RegisterType<RSC.Compounds.EntityFramework.EFExactStructureSearch>().As<RSC.Compounds.Search.IExactStructureSearch>();
			builder.RegisterType<RSC.Compounds.EntityFramework.EFSubStructureSearch>().As<RSC.Compounds.Search.ISubStructureSearch>();
			builder.RegisterType<RSC.Compounds.EntityFramework.EFSimilarityStructureSearch>().As<RSC.Compounds.Search.ISimilarityStructureSearch>();
			builder.RegisterType<RSC.Search.MemoryRequestStorage>().As<RSC.Search.IRequestStorage>();

			builder.RegisterType<RSC.Compounds.EntityFramework.EFStatistics>().As<RSC.Compounds.IStatistics>();
			builder.RegisterType<RSC.Compounds.EntityFramework.EFCompoundStore2>().As<RSC.Compounds.ICompoundStore>();
			builder.RegisterType<RSC.Compounds.EntityFramework.EFDatasourceStore>().As<RSC.Compounds.IDatasourceStore>();
			builder.RegisterType<RSC.Compounds.EntityFramework.EFCompoundConvert>().As<RSC.Compounds.ICompoundConvert>();

			builder.RegisterType<RSC.Compounds.EntityFramework.EFCompoundStore>().As<RSC.Compounds.CompoundStore>();
			builder.RegisterType<RSC.Compounds.EntityFramework.EFSubstanceStore>().As<RSC.Compounds.SubstanceStore>();
			builder.RegisterType<RSC.Properties.EntityFramework.EFPropertyStore>().As<RSC.Properties.IPropertyStore>();

			return app;
		}
	}
}
