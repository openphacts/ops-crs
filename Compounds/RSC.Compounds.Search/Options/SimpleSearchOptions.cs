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
	public class SimpleSearchOptions : SearchOptions
	{
		[DataMember]
		public string QueryText { get; set; }

		public override bool IsEmpty()
		{
			return string.IsNullOrEmpty(QueryText);
		}
	}
}
