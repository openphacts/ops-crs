using Autofac;
using RSC.CVSP.Compounds.Operations;
using RSC.Properties;
using System;
using System.Collections.Generic;

namespace RSC.CVSP.Compounds.Autofac
{
	public class CompoundsBaseModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			// Just a tric to get temporary directory created in proper place - otherwise Indigo fails under temporary and service profiles
			InChINet.InChIUtility.GetInstance();

			builder.Register(c => new Acidity(RSC.CVSP.Compounds.Properties.Resources.acidgroups)).As<RSC.CVSP.Compounds.IAcidity>();
            builder.Register(c => new ValidationRuleModule(RSC.CVSP.Compounds.Properties.Resources.validation)).As<RSC.CVSP.Compounds.IValidationRuleModule>();
            builder.RegisterType<ValidationModule>().As<IValidationModule>().SingleInstance();
			builder.RegisterType<ValidationStereoModule>().As<IValidationStereoModule>().SingleInstance();
			builder.RegisterType<StandardizationModule>().As<IStandardizationModule>().SingleInstance();
			builder.RegisterType<StandardizationFragmentsModule>().As<IStandardizationFragmentsModule>().SingleInstance();
			builder.RegisterType<StandardizationChargesModule>().As<IStandardizationChargesModule>().SingleInstance();
			builder.RegisterType<StandardizationStereoModule>().As<IStandardizationStereoModule>().SingleInstance();
			builder.RegisterType<StandardizationMetalsModule>().As<IStandardizationMetalsModule>().SingleInstance();

			builder.RegisterType<Validation>().SingleInstance();
			builder.RegisterType<Standardization>().SingleInstance();

			builder.RegisterType<CalculateMolecularFormula>().Named<IOperation>("operation");
			builder.RegisterType<CalculateMolecularWeight>().Named<IOperation>("operation");
			builder.RegisterType<CalculateMonoisotopicMass>().Named<IOperation>("operation");
			builder.RegisterType<CalculateMostAbundantMass>().Named<IOperation>("operation");
			builder.RegisterType<CalculateStdInChI>().Named<IOperation>("operation");
			builder.RegisterType<CalculateNonStdInChI>().Named<IOperation>("operation");
			builder.RegisterType<CalculateSMILES>().Named<IOperation>("operation");

			builder.RegisterDecorator<IOperation>((c, inner) => inner, fromKey: "operation");
		}
	}
}
