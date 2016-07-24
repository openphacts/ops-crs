using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.EntityFramework
{
    [Table("Compounds")]
    public class ef_Compound
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None), Key()]
        public Guid Id { get; set; }

        [Required]
		[Index]
        public DateTime DateCreated { get; set; }

        //not required because for tautomers and super parents there is no Mol
        public string Mol { get; set; }

        //indexes will be created automatically by EF on foreign keys

        //exists only for for real compounds with existsing Mol
        [ForeignKey("NonStandardInChI")]
        public Guid? NonStandardInChIId { get; set; }
        public virtual ef_InChI NonStandardInChI { get; set; }

        //exists only for tautomers and super parents
        [ForeignKey("TautomericNonStdInChI")]
        public Guid? TautomericNonStdInChIId { get; set; }
        public virtual ef_InChI TautomericNonStdInChI { get; set; }

        //exists only for for real compounds with existsing Molfile
        [ForeignKey("StandardInChI")]
        public Guid? StandardInChIId { get; set; }
        public virtual ef_InChI StandardInChI { get; set; }

        //not required - because Smiles exists only when Mol is given
        [ForeignKey("Smiles")]
        public Guid? SmilesId { get; set; }
        public virtual ef_Smiles Smiles { get; set; }

        public virtual ICollection<ef_CompoundSynonym> CompoundSynonyms { get; set; }

        public virtual ICollection<ef_ParentChild> Parents { get; set; }

        public virtual ICollection<ef_ParentChild> Children { get; set; }

        public virtual ICollection<ef_ExternalReference> ExternalReferences { get; set; }

		public virtual ICollection<ef_Revision> Revisions { get; set; }
	}
}
