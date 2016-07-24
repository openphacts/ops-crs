using RSC.Process;
using RSC.Search;
using System;
using System.Runtime.Serialization;

namespace RSC.CVSP.Search
{
	[Serializable]
	[DataContract]
	public class CVSPJobsSearchOptions : SearchOptions
	{
		[DataMember]
		public Guid Deposition { get; set; }

		[DataMember]
		public string Status { get; set; }

		[DataMember]
		public string Command { get; set; }

		public override bool IsEmpty()
		{
			return Deposition == Guid.Empty && string.IsNullOrEmpty(Status) && string.IsNullOrEmpty(Command);
		}
		public static bool IsNullOrEmpty(CVSPRecordsSearchOptions o)
		{
			return o == null || o.IsEmpty();
		}
	}
}
