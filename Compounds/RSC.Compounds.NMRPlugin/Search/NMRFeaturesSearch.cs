using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChemSpider.Search;
using ChemSpider.Security;
using RSC.Compounds.Search;
using RSC.Compounds.NMRFeatures;

namespace RSC.Compounds.Search
{
	public interface INMRFeaturesSearch
	{
		CSSearch Instance { get; }
	}

	public class CSCNMRFeaturesSearch : CSSqlSearch, INMRFeaturesSearch
	{
		private readonly ISqlSearchProvider searchProvider;

		public CSCNMRFeaturesSearch(ISqlSearchProvider searchProvider)
		{
			if (searchProvider == null)
			{
				throw new ArgumentNullException("searchProvider");
			}

			m_sqlProvider = searchProvider;
		}

		public CSSearch Instance
		{
			get { return this; }
		}

		public new NMRFeaturesSearchOptions Options
		{
			get { return base.Options as NMRFeaturesSearchOptions; }
		}

		protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
		{
			int index = 1;

			foreach (var feature in Options.Features)
			{
				tables.Add(string.Format("JOIN CompoundNMRFeatures cnmr{0} ON cnmr{0}.CompoundId = c.csid join NMRFeatures nmr{0} on nmr{0}.Id = cnmr{0}.NMRFeatureId ", index));
				predicates.Add(string.Format("nmr{0}.Name = '{1}' and cnmr{0}.Count = {2}", index, feature.Name, feature.Count));
				visual.Add(string.Format("NMR: {0} = {1}", feature.Name, feature.Count));

				index++;
			}

			return Options.Features.Count() > 0;
		}
	}
}
