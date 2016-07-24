using RSC.Search;
using System;
using System.Runtime.Serialization;

namespace RSC.CVSP.Search
{
	[Serializable]
	[DataContract]
	public class CVSPSearchResultOptions : SearchResultOptions
	{
		public CVSPSearchResultOptions() : base()
		{
		}

		public override bool IsEmpty()
		{
			return false;
		}
	}
}
