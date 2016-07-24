using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.EntityFramework
{
    [Table("SynonymHistory")]
    public class ef_SynonymHistory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None), Key()]
        public Guid Id { get; set; }

        [ForeignKey("Synonym")]
        [Required]
        public Guid SynonymId { get; set; }       
        
        public ef_Synonym Synonym { get; set; }
        
        public Guid? CuratorId { get; set; }
        
        public DateTime DateChanged { get; set; }

        //These fields don't actually need storing as we never change them, so not stored to save space for now.
        //[Required]
        //[MaxLength(450)]
        //public string Synonym { get; set; }
        //[Required]
        //[MaxLength(2)]
        //public string LanguageId { get; set; }
        //public DateTime DateCreated { get; set; }

        //We must store the collection of Synonym Flags that were stored against the Synonym at the time of the update.
        public virtual ICollection<ef_SynonymSynonymFlagHistory> SynonymFlags { get; set; }
    }
}
