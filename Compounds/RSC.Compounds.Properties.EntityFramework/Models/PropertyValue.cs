using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.Properties.EntityFramework
{
    public class PropertyValue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

		[Required]
		public int RecordId { get; set; }

		[Required]
		[ForeignKey("Property")]
        public int PropertyId { get; set; }
		public Property Property { get; set; }

		public int? IntValue { get; set; }
        public float? FloatValue { get; set; }
        public string TextValue { get; set; }
		public float? ErrorValue { get; set; }

		[ForeignKey("Unit")]
		public int? UnitId { get; set; }
		public Unit Unit { get; set; }

		[ForeignKey("Company")]
        public int? CompanyId { get; set; }
		public Company Company { get; set; }

		[ForeignKey("SoftwareInstrument")]
        public int? SoftwareInstrumentId { get; set; }
		public SoftwareInstrument SoftwareInstrument { get; set; }
    }
}
