using RSC.Search;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

namespace RSC.Compounds.Search
{
	[Serializable]
	[DataContract]
	public class CompoundsSearchScopeOptions : SearchScopeOptions
	{
		[DataMember]
		public IEnumerable<Guid> Datasources { get; set; }

		[DataMember]
		public bool RealOnly { get; set; }

		public CompoundsSearchScopeOptions()
		{
			RealOnly = false;
		}

		public override bool IsEmpty()
		{
			return !(Datasources != null && Datasources.Any() || RealOnly);
		}
	}
}
