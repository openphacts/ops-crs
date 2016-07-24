using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.EntityFramework
{
    [Table("CompoundSynonymSynonymFlagHistory")]
    public class ef_CompoundSynonymSynonymFlagHistory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None), Key()]
        public Guid Id { get; set; }

        [ForeignKey("CompoundSynonymHistory")]
        [Required]
        public Guid CompoundSynonymHistoryId { get; set; }

        public ef_CompoundSynonymHistory CompoundSynonymHistory { get; set; }

        [ForeignKey("SynonymFlag")]
        [Required]
        public int SynonymFlagId { get; set; }
        
        public ef_SynonymFlag SynonymFlag { get; set; }
    }
}
