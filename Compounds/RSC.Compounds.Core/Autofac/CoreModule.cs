using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.Core
{
	public class CoreModule : Autofac.Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			var assembly = Assembly.GetExecutingAssembly();

			//builder.RegisterType<RSC.Compounds.CompoundsService>().As<RSC.Compounds.ICompoundConvert>();
		}
	}
}
