using Autofac;
using RSC.CVSP.Compounds.Operations;

namespace RSC.CVSP.Compounds.Autofac
{
	public class ParentChildModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<RSC.CVSP.Compounds.Operations.CalculateParents>().SingleInstance();

			builder.RegisterType<CalculateParents>().Named<IOperation>("operation");
		}
	}
}
