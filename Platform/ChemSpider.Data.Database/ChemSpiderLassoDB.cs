using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using ChemSpider.Database;

namespace ChemSpider.Data.Database
{
	public class ChemSpiderLassoDB : ChemSpiderBaseDB
	{
		protected override string ConnString
		{
			get
			{
				return RO_ConnString;
			}
		}

		protected override string RO_ConnString
		{
			get
			{
				return ConfigurationManager.ConnectionStrings["ChemSpiderLassoROConnectionString"] == null ?
					null :
					ConfigurationManager.ConnectionStrings["ChemSpiderLassoROConnectionString"].ConnectionString;
			}
		}

        protected override string DatabaseName
        {
            get { return "ChemSpiderLasso"; }
        }
	}
}
