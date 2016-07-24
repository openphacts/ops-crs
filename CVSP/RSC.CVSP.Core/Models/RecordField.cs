using System;
using System.Runtime.Serialization;

namespace RSC.CVSP
{
	[Serializable]
	[DataContract]
	public class RecordField
	{
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public string Value { get; set; }
	}
}
