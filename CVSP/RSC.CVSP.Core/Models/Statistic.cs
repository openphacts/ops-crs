using RSC.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RSC.CVSP
{
	[Serializable]
	[DataContract]
	public class Statistic
	{
		/// <summary>
		/// Total number of records in deposition
		/// </summary>
		[DataMember]
		public int RecordsNumber { get; set; }
		/// <summary>
		/// Number of records with error issues
		/// </summary>
		[DataMember]
		public int ErrorsNumber { get; set; }
		/// <summary>
		/// Number of records with warning issues
		/// </summary>
		[DataMember]
		public int WarningsNumber { get; set; }
		/// <summary>
		/// Number of records with informative issues
		/// </summary>
		[DataMember]
		public int InfosNumber { get; set; }

		[DataMember]
		public IEnumerable<IssueStat> Issues { get; set; }
	}

	[Serializable]
	[DataContract]
	public class IssueStat
	{
		[DataMember]
		public string Code { get; set; }

		[DataMember]
		public int Count { get; set; }
	}
}
