using Autofac;
using Autofac.Extras.CommonServiceLocator;
using Microsoft.Practices.ServiceLocation;
using RSC.CVSP.Compounds;

namespace CVSPTests
{
	public class CVSPTestBase
	{
		protected IContainer container = null;

		public CVSPTestBase()
		{
			var builder = new ContainerBuilder();

			builder.Register(c => new RSC.CVSP.EntityFramework.EFCVSPStore("CVSPConnection", 60)).As<RSC.CVSP.ICVSPStore>();
			builder.Register(c => new RSC.Properties.EntityFramework.EFPropertyStore("PropertiesConnection")).As<RSC.Properties.IPropertyStore>();

			builder.RegisterModule<RSC.CVSP.Compounds.Autofac.CompoundsBaseModule>();
            builder.RegisterModule<RSC.CVSP.Compounds.Autofac.ParentChildModule>();
            builder.RegisterModule<RSC.CVSP.Compounds.Autofac.ChemSpiderPropertiesModule>();

			container = builder.Build();

			ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(container));

			RSC.CVSP.Compounds.Init.Initialize();
		}

		public RSC.CVSP.ICVSPStore CVSPStore
		{
			get { return container.Resolve<RSC.CVSP.ICVSPStore>(); }
		}

		public IStandardizationModule StandardizationModule
		{
			get { return container.Resolve<IStandardizationModule>(); }
		}

		public IStandardizationFragmentsModule StandardizationFragmentsModule
		{
			get { return container.Resolve<IStandardizationFragmentsModule>(); }
		}

		public IStandardizationChargesModule StandardizationChargesModule
		{
			get { return container.Resolve<IStandardizationChargesModule>(); }
		}

		public IStandardizationStereoModule StandardizationStereoModule
		{
			get { return container.Resolve<IStandardizationStereoModule>(); }
		}

		public IStandardizationMetalsModule StandardizationMetalsModule
		{
			get { return container.Resolve<IStandardizationMetalsModule>(); }
		}

		public IValidationModule ValidationModule
		{
			get { return container.Resolve<IValidationModule>(); }
		}

        public IValidationRuleModule ValidationRuleModule
        {
            get { return container.Resolve<IValidationRuleModule>(); }
        }

		public IValidationStereoModule ValidationStereoModule
		{
			get { return container.Resolve<IValidationStereoModule>(); }
		}

		public IAcidity Acidity
		{
			get { return container.Resolve<IAcidity>(); }
		}

		public Validation Validation
		{
			get { return container.Resolve<Validation>(); }
		}

		public Standardization Standardization
		{
			get { return container.Resolve<Standardization>(); }
		}
	}
}
