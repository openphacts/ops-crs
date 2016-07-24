using RSC.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Search
{
	[Serializable]
	[DataContract]
	public class CVSPDepositionsSearchOptions : SearchOptions
	{
		[DataMember]
		public Guid User { get; set; }

		[DataMember]
		public DepositionStatus[] Status { get; set; }

		public override bool IsEmpty()
		{
			return User == Guid.Empty && Status.Length == 0;
		}
		public static bool IsNullOrEmpty(CVSPRecordsSearchOptions o)
		{
			return o == null || o.IsEmpty();
		}
	}
}
