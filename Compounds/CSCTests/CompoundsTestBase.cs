using Autofac;
using Autofac.Extras.CommonServiceLocator;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.Tests
{
	public class CompoundsTestBase
	{
		protected IContainer container = null;

		public CompoundsTestBase()
		{
			var builder = new ContainerBuilder();

			builder.RegisterType<RSC.Compounds.EntityFramework.EFSubstanceStore>().As<RSC.Compounds.SubstanceStore>();
			builder.RegisterType<RSC.Compounds.EntityFramework.EFSubstanceBulkUpload>().As<RSC.Compounds.ISubstanceBulkUpload>();
			builder.RegisterType<RSC.Compounds.EntityFramework.EFCompoundStore>().As<RSC.Compounds.CompoundStore>();
			builder.RegisterType<RSC.Properties.EntityFramework.EFPropertyStore>().As<RSC.Properties.IPropertyStore>();
			builder.RegisterType<RSC.CVSP.EntityFramework.EFCVSPStore>().As<RSC.CVSP.ICVSPStore>();

			container = builder.Build();

			ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(container));
		}

		public RSC.Compounds.ISubstanceBulkUpload SubstanceBulkUpload
		{
			get { return container.Resolve<RSC.Compounds.ISubstanceBulkUpload>(); }
		}

		public RSC.Compounds.SubstanceStore SubstanceStore
		{
			get { return container.Resolve<RSC.Compounds.SubstanceStore>(); }
		}
	}
}
