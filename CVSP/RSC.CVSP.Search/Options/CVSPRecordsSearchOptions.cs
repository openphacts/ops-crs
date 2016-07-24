using RSC.Logging;
using RSC.Search;
using System;
using System.Runtime.Serialization;

namespace RSC.CVSP.Search
{
	[Serializable]
	[DataContract]
	public class CVSPRecordsSearchOptions : SearchOptions
	{
		[DataMember]
		public Guid Deposition { get; set; }

		[DataMember]
		public string[] Codes { get; set; }

		[DataMember]
		public int[] Ordinals { get; set; }

		[DataMember]
		public string[] RegIDs { get; set; }

		public override bool IsEmpty()
		{
			return Deposition == Guid.Empty && Codes.Length == 0;
		}
		public static bool IsNullOrEmpty(CVSPRecordsSearchOptions o)
		{
			return o == null || o.IsEmpty();
		}
	}
}
