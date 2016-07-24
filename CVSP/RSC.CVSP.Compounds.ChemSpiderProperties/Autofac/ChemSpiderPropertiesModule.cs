using Autofac;
using RSC.CVSP.Compounds.Operations;

namespace RSC.CVSP.Compounds.Autofac
{
	public class ChemSpiderPropertiesModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
            builder.RegisterType<RSC.CVSP.Compounds.Operations.ImportChemSpiderProperties>().SingleInstance();

			builder.RegisterType<ImportChemSpiderSynonyms>().Named<IOperation>("operation");
			builder.RegisterType<ImportChemSpiderProperties>().Named<IOperation>("operation");
		}
	}
}
