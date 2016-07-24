using System;
using System.Runtime.Serialization;

namespace RSC.Compounds
{
    public enum SynonymFlagType
    {
        /// <summary>
        /// The flag will automatically be assigned to all instances of the Synonym on every Compound.
        /// </summary>
        SynonymType,
        
        /// <summary>
        /// The flag will only be assigned to one instance of the Synonym on a Compound.
        /// </summary>
        SynonymAssertion
    }

	[Serializable]
	[DataContract]
	public class SynonymFlag
	{
        [DataMember]
		public string Name { get; set; }

        [DataMember]
		public string Description { get; set; }

        [DataMember]
		public string Url { get; set; }

        [DataMember]
		public int Rank { get; set; }

        [DataMember]
		public bool ExcludeFromTitle { get; set; }

        [DataMember]
        public SynonymFlagType Type { get; set; }
        
        [DataMember]
        public bool IsUniquePerLanguage { get; set; }

        [DataMember]
        public string RegEx { get; set; }

        [DataMember]
        public bool IsRestricted { get; set; }
    }
}
