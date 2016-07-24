using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.EntityFramework
{
    [Table("CompoundSynonymsSynonymFlag")]
    public class ef_CompoundSynonymSynonymFlag
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None), Key()]
        public Guid Id { get; set; }

        [ForeignKey("CompoundSynonym")]
        public Guid CompoundSynonymId { get; set; }

        [Required]
        public ef_CompoundSynonym CompoundSynonym { get; set; }

        [ForeignKey("SynonymFlag")]
        public int SynonymFlagId { get; set; }

        [Required]
        public ef_SynonymFlag SynonymFlag { get; set; }
    }
}