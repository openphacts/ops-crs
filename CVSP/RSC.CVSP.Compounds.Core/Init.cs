using RSC.Logging;
using RSC.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Compounds
{
	public static class Init
	{
		public static void Initialize()
		{
			SeedLogCodes();

			SeedSoftwares();
		}

		private static void SeedLogCodes()
		{
			LogManager.Logger.ImportFromXml(CompoundsResource.LogCodes);
		}

		private static void SeedSoftwares()
		{
			PropertyManager.Current.SeedSoftware(new List<Software>() {
				new Software()
				{
					Name = "Indigo",
					Version = "1.1.12.89 win32",
				},
				new Software()
				{
					Name = "InChI",
					Version = "1.04"
				}
			});
		}
	}
}
