using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace RSC.Compounds
{
	[Serializable]
	[DataContract]
	public class Substance
	{
		[Display(Name = "ID")]
		[DataMember]
		public Guid Id { get; set; }

		[Display(Name = "External ID")]
		[DataMember]
		public string ExternalIdentifier { get; set; }

		[Display(Name = "Data Source ID")]
		[DataMember]
		public Guid DataSourceId { get; set; }

		[Display(Name = "Revisions")]
		[DataMember]
		public IEnumerable<int> Revisions { get; set; }
	}
}
