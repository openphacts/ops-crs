using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.Properties.EntityFramework
{
    public class Unit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
		[ForeignKey("BaseUnit")]
        public int? BaseUnitId { get; set; }
		public Unit BaseUnit { get; set; }
        public string BaseUnitConversion { get; set; }
		public virtual ICollection<PropertyValue> Values { get; set; }
		public virtual ICollection<Unit> Units { get; set; }
    }
}
