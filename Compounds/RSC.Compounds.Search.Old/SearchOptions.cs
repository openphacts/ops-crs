using System;
using System.Runtime.Serialization;

namespace RSC.Compounds.Search.Old
{
	[Serializable]
	[DataContract]
	public class CSCSearchScopeOptions : ChemSpider.Search.SearchScopeOptions
	{
		[DataMember]
		public bool RealOnly { get; set; }
	}
}
