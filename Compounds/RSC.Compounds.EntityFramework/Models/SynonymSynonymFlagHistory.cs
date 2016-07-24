using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.EntityFramework
{
    [Table("SynonymsSynonymFlagHistory")]
    public class ef_SynonymSynonymFlagHistory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None), Key()]
        public Guid Id { get; set; }

        [ForeignKey("SynonymHistory")]
        public Guid SynonymHistoryId { get; set; }
        public ef_SynonymHistory SynonymHistory { get; set; }

        [ForeignKey("SynonymFlag")]
        public int SynonymFlagId { get; set; }

        public ef_SynonymFlag SynonymFlag { get; set; }
    }
}