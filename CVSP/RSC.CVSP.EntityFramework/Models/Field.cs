using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.CVSP.EntityFramework
{
	[Table("Fields")]
	public class ef_Field
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[Index]
		[Required]
		[MaxLength(200)]
		public string Name { get; set; }

		[ForeignKey("File")]
		[Required]
		public int FileId { get; set; }

		public virtual ef_DepositionFile File { get; set; }

		[ForeignKey("Annotation")]
		public int? AnnotationId { get; set; }
		public virtual ef_Annotation Annotation { get; set; }

		public virtual ICollection<ef_RecordField> Values { get; set; }
	}

	public static class FieldExtensions
	{
		public static Field ToField(this ef_Field ef)
		{
			return new Field()
			{
				Name = ef.Name,
				Annotaition = ef.Annotation == null ? null : new Annotation()
				{
					Name = ef.Annotation.Name,
					Title = ef.Annotation.Title,
					IsRequired = ef.Annotation.IsRequired
				}
			};
		}
	}
}
