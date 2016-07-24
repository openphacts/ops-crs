using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.EntityFramework
{
    [Table("CompoundSynonymHistory")]
    public class ef_CompoundSynonymHistory
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [ForeignKey("CompoundSynonym")]
        [Required]
        public Guid CompoundSynonymId { get; set; }
        public ef_CompoundSynonym CompoundSynonym { get; set; }

        public CompoundSynonymState SynonymState { get; set; }

        public bool IsTitle { get; set; }

        public DateTime DateChanged { get; set; }

        public Guid? CuratorId { get; set; }

        public int? RevisionId { get; set; }

        //We must store the collection of Synonym Flags that were stored against the Compound Synonym at the time of the update.
        public virtual ICollection<ef_CompoundSynonymSynonymFlagHistory> SynonymFlags { get; set; }
    }
}
