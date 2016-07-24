using RSC.Search;
using System;
using System.Runtime.Serialization;

namespace RSC.CVSP.Search
{
	[Serializable]
	[DataContract]
	public class CVSPSearchScopeOptions : SearchScopeOptions
	{
		public Guid[] Datasources { get; set; }

		public CVSPSearchScopeOptions()
		{
		}

		public override bool IsEmpty()
		{
			return false;
		}
	}
}
