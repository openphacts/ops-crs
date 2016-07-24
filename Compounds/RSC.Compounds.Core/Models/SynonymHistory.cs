using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RSC.Compounds
{
	[Serializable]
	[DataContract]
	public class SynonymHistory
	{
        [DataMember]
        public CompoundSynonymState SynonymState { get; set; }

        [DataMember]
        public IEnumerable<SynonymFlag> SynonymFlags { get; set; }

		[DataMember]
		public Guid? CuratorId { get; set; }

        [DataMember]
        public bool IsTitle { get; set; }

        [DataMember]
        public DateTime DateChanged { get; set; }
	}
}
