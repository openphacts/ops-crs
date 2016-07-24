using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.CVSP.EntityFramework
{
	[Table("RecordFields")]
	public class ef_RecordField
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[ForeignKey("Field")]
		[Required]
		public int FieldId { get; set; }
		public virtual ef_Field Field { get; set; }

		public string Value { get; set; }

		[ForeignKey("Record")]
		[Required]
		public int RecordId { get; set; }
		public virtual ef_Record Record { get; set; }
	}

	public static class RecordFieldExtensions
	{
		public static RecordField ToField(this ef_RecordField ef)
		{
			return new RecordField()
			{
				Name = ef.Field.Name,
				Value = ef.Value
			};
		}
	}
}
