using System;
using System.Runtime.Serialization;

namespace RSC.CVSP
{
	[Serializable]
	[DataContract]
	public class Annotation
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public string Title { get; set; }

		[DataMember]
		public bool IsRequired { get; set; }
	}
}
