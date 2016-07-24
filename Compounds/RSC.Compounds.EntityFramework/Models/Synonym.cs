using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.EntityFramework
{
	[Table("Synonyms")]
	public class ef_Synonym
	{
		[DatabaseGenerated(DatabaseGeneratedOption.None), Key()]
		public Guid Id { get; set; }

		[Required]
		[Index("synonym_langid_idx", IsUnique = true, Order = 0)]
		[MaxLength(448)]
		public string Synonym { get; set; }

		[Required]
		[Index("synonym_langid_idx", IsUnique = true, Order = 1)]
		[MaxLength(2)]
		public string LanguageId { get; set; }

		public DateTime DateCreated { get; set; }

        public ICollection<ef_SynonymSynonymFlag> SynonymFlags { get; set; }

        public ICollection<ef_SynonymHistory> History { get; set; }

        public ICollection<ef_Revision> Revisions { get; set; }
	}
}
