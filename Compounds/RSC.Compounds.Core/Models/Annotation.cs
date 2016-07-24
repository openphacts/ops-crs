using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace RSC.Compounds
{
	[Serializable]
	[DataContract]
	public class Annotation
	{
		[DataMember]
		[Display(Name = "Type")]
		public int Type { get; set; }

		[DataMember]
		[Display(Name = "Value")]
		public string Value { get; set; }

	}
}
