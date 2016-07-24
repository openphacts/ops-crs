using RSC.Search;
using System;
using System.Runtime.Serialization;

namespace RSC.Compounds.Search
{
	[Serializable]
	[DataContract]
	public class CompoundsSearchResultOptions : SearchResultOptions
	{
		public CompoundsSearchResultOptions() : base()
		{
		}

		public override bool IsEmpty()
		{
			return false;
		}
	}
}
