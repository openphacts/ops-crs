using System;
using System.Runtime.Serialization;

namespace RSC.Compounds
{
    [DataContract]
	public class ParentChild
	{
		[DataMember]
		public ParentChildRelationship Type {get;set;}
		[DataMember]
		public Guid ParentId { get; set; }
		public Compound Parent { get; set; }
		[DataMember]
		public Guid ChildId { get; set; }
        public Compound Child { get; set; }
	}
}
