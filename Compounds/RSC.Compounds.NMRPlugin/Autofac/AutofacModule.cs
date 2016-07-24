using Autofac;
using Autofac.Integration.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.NMRFeatures
{
	public class AutofacModule : Autofac.Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			var assembly = Assembly.GetExecutingAssembly();

			builder.RegisterType<RSC.Compounds.NMRFeatures.EntityFramework.NMRFeaturesStore>().As<RSC.Compounds.NMRFeatures.INMRFeaturesStore>();
			builder.RegisterType<RSC.Compounds.NMRFeatures.NMRFeaturesService>().As<RSC.Compounds.NMRFeatures.INMRFeaturesService>();

			builder.RegisterApiControllers(assembly);
		}
	}
}
