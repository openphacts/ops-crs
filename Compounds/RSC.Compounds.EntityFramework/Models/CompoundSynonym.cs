using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.EntityFramework
{
	[Table("CompoundSynonyms")]
	public class ef_CompoundSynonym
	{
        [DatabaseGenerated(DatabaseGeneratedOption.None), Key()]
        public Guid Id { get; set; }

		[ForeignKey("Compound")]
        [Required]
        [Index("IX_CompoundIdAndSynonymState", 1)] 
		public Guid CompoundId { get; set; }

		public ef_Compound Compound { get; set; }

		[ForeignKey("Synonym")]
        [Required]
		public Guid SynonymId { get; set; }

        public ef_Synonym Synonym { get; set; }

        [Index("IX_CompoundIdAndSynonymState", 2)] 
		public CompoundSynonymState SynonymState { get; set; }

        public bool IsTitle { get; set; }

        public virtual ICollection<ef_CompoundSynonymSynonymFlag> CompoundSynonymSynonymFlags { get; set; }

        public virtual ICollection<ef_CompoundSynonymHistory> History { get; set; }
	}
}
