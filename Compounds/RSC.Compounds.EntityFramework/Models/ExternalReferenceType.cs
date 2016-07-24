using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.EntityFramework
{
    [Table("ExternalReferenceTypes")]
    public class ef_ExternalReferenceType
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None), Key()]
        public int Id { get; set; }

        public string Description { get; set; }

        public string UriSpace { get; set; }
    }
}
