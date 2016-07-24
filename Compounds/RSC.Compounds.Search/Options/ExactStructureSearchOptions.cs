using RSC.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.Search
{
	[Serializable]
	[DataContract]
	public class ExactStructureSearchOptions : SearchOptions
	{
		public enum EMatchType
		{
			ExactMatch = 0,
			AllTautomers,
			SameSkeletonIncludingH,
			SameSkeletonExcludingH,
			AllIsomers
		}

		[DataMember]
		public EMatchType MatchType { get; set; }

		[DataMember]
		public string Molecule { get; set; }

		public override bool IsEmpty()
		{
			return string.IsNullOrEmpty(Molecule);
		}
	}
}
