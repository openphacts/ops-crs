using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.EntityFramework
{
    [Table("SynonymsSynonymFlag")]
    public class ef_SynonymSynonymFlag
    {
        [Key, Column(Order = 0)]
        [ForeignKey("Synonym")]
        public Guid SynonymId { get; set; }
        [Required]
        public ef_Synonym Synonym { get; set; }

        [Key, Column(Order = 1)]
        [ForeignKey("SynonymFlag")]
        public int SynonymFlagId { get; set; }
        [Required]
        public ef_SynonymFlag SynonymFlag { get; set; }
    }
}