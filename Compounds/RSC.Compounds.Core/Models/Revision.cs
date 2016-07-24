using RSC.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RSC.Compounds
{
	[Serializable]
	[DataContract]
	public class Revision
	{
        [DataMember]
        public Guid Id { get; set; }

		[DataMember]
		public int Version { get; set; }

		[DataMember]
		public DateTime DateCreated { get; set; }

		/// <summary>
		/// date when user choses to make the version public
		/// </summary>
		[DataMember]
		public DateTime? EmbargoDate { get; set; }

		//1. depositor may chose to revoke substance and in this case the substance should stop being searchable 
		//2. the link from substance to compound should be supressed
		//3. direct link to substance may exist and may show that substance has been revoked by depositor
		[DataMember]
		public bool Revoked { get; set; }

		[DataMember]
		public string Sdf { get; set; }

		[DataMember]
		public IEnumerable<Annotation> Annotations { get; set; }

		[DataMember]
		public IEnumerable<Issue> Issues { get; set; }

        [DataMember]
        public Guid CompoundId { get; set; }

        [DataMember]
        public Substance Substance { get; set; }

        [DataMember]
        public IEnumerable<Synonym> Synonyms { get; set; }

		[DataMember]
		public IEnumerable<Guid> Properties { get; set; }
	}
}
