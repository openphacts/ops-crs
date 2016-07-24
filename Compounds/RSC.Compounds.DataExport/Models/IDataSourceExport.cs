using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSC.Datasources;

namespace RSC.Compounds.DataExport
{
	public interface ILimitedExport
	{
		bool Limited { get; set; }
	}

	public interface IDataSourceExport : IDataExport, ILimitedExport
	{
		string DsnLabel { get; }

		DataSource DataSource { get; set; }

		DataSourcesClient DataSourcesClient { get; set; }
	}
}
