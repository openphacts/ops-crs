using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.EntityFramework
{
    [Table("ExternalReferences")]
    public class ef_ExternalReference
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
        public int Id { get; set; }

        [ForeignKey("Compound")]
        [Index("CompoundId_ExternalReferenceTypeId_Value_idx", IsUnique = true, Order = 0)]
        public Guid CompoundId { get; set; }

        [ForeignKey("Type")]
        [Index("CompoundId_ExternalReferenceTypeId_Value_idx", IsUnique = true, Order = 1)]
        public int ExternalReferenceTypeId { get; set; }

        [Index("CompoundId_ExternalReferenceTypeId_Value_idx", IsUnique = true, Order = 2)]
        [MaxLength(100)]
        public string Value { get; set; }
        
        public virtual ef_ExternalReferenceType Type { get; set; }

        public virtual ef_Compound Compound { get; set; }
    }
}
