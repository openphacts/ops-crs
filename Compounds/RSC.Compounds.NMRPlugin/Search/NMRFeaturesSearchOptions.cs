using ChemSpider.Search;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.Search
{
	[Serializable]
	[DataContract]
	public class NMRFeature
	{
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public string Count { get; set; }
	}

	[Serializable]
	[DataContract]
	[Description("NMR features search options")]
	public class NMRFeaturesSearchOptions : SearchOptions
	{
		public NMRFeaturesSearchOptions()
		{
			AllowVagueSearch = true;
		}

		[DataMember]
		public NMRFeature[] Features { get; set; }
		[DataMember]
		public bool AllowVagueSearch { get; set; }

		public override bool IsEmpty() { return Features.Count() == 0; }
	}
}
