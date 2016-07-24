using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RSC.Compounds
{
	[Serializable]
	[DataContract]
	public class Synonym
	{
        public Guid Id { get; set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public string LanguageId { get; set; }

        [DataMember]
        public CompoundSynonymState State { get; set; }

        [DataMember]
        public bool IsTitle { get; set; }

		[DataMember]
		public DateTime DateCreated { get; set; }
		
		[DataMember]
		public IEnumerable<SynonymFlag> Flags { get; set; } //Should include both types of flags, those at Synonym level and those at CompoundSynonym level.

		[DataMember]
		public IEnumerable<SynonymHistory> History { get; set; }
	}
}
