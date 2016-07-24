using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.DataExport
{
	public class AutofacModule : Autofac.Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			var assembly = Assembly.GetExecutingAssembly();

            //TODO: Register the types.

            //builder.RegisterType<RSC.Compounds.NMRFeatures.EntityFramework.NMRFeaturesStore>().As<RSC.Compounds.NMRFeatures.INMRFeaturesStore>();
            //builder.RegisterType<RSC.Compounds.NMRFeatures.NMRFeaturesService>().As<RSC.Compounds.NMRFeatures.INMRFeaturesService>();

			//builder.RegisterApiControllers(assembly);
		}
	}
}
