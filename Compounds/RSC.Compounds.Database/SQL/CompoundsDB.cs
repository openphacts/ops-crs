using ChemSpider.Data.Database;
using System.Configuration;

namespace RSC.Compounds.Database
{
	public class CompoundsDB
	{
		public static string ConnectionString
		{
			get
			{
				return ConfigurationManager.ConnectionStrings["CompoundsConnectionString"] == null ?
					null :
					ConfigurationManager.ConnectionStrings["CompoundsConnectionString"].ConnectionString;
			}
		}
	}

	public class CompoundsSSSDB : ChemSpiderSSSDB
	{
		public static string RO_CV_ConnectionString
		{
			get
			{
				return ConfigurationManager.ConnectionStrings["CompoundsConnectionString"] == null ?
					RO_ConnectionString :
					ConfigurationManager.ConnectionStrings["CompoundsConnectionString"].ConnectionString;
			}
		}

		protected override string RO_ConnString
		{
			get
			{
				return RO_CV_ConnectionString;
			}
		}
	}
}
