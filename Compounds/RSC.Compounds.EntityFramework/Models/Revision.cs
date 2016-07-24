using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using MoleculeObjects;
using RSC.Logging;

namespace RSC.Compounds.EntityFramework
{
    [Table("Revisions")]
    public class ef_Revision
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None), Key()]
        public Guid Id { get; set; }

        [Required]
        [Index]
        public Guid DepositionId { get; set; }

        [Required]
        public int Version { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        public DateTime? DateModified { get; set; }

        //depositor may choose to make substance public on certain  date
        public DateTime? EmbargoDate { get; set; }

        public bool Revoked { get; set; }

        public string Sdf { get; set; }

        public virtual ICollection<ef_Annotation> Annotations { get; set; }

        public virtual ICollection<ef_Issue> Issues { get; set; }

        [Required]
        [ForeignKey("Substance")]
        public Guid SubstanceId { get; set; }

        [Required]
        public virtual ef_Substance Substance { get; set; }

        [ForeignKey("Compound")]
        public Guid CompoundId { get; set; }

        public virtual ef_Compound Compound { get; set; }

        public virtual ICollection<ef_Synonym> Synonyms { get; set; }
    }
}