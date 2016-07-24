using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Search.Core
{
	[Serializable]
	[DataContract]
	public class SearchResultOptions : SearchOptions
	{
		[DataMember]
		public int Limit { get; set; }
		//[DataMember]
		//public int Start { get; set; }
		//[DataMember]
		//public int Length { get; set; }
		[DataMember]
		public List<string> SortOrder { get; set; }

		public SearchResultOptions()
		{
			Limit = -1;
			//Start = 0;
			//Length = -1;
			SortOrder = new List<string>();
		}

		public override bool IsEmpty()
		{
			return false;
		}
	}
}
