using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RSC.CVSP.EntityFramework
{
	[Table("Records")]
	public class ef_Record
	{
		[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[Required]
		public ExternalId ExternalId { get; set; }

		[Index]
		public int Ordinal { get; set; }

		public DateTime SubmissionDate { get; set; }

		public DateTime? RevisionDate { get; set; }

		[Required]
		public int DataDomain { get; set; }

		[Required]
		public string Original { get; set; }

		public string Standardized { get; set; }

		[ForeignKey("Deposition")]
		[Required]
		public int DepositionId { get; set; }
		public virtual ef_Deposition Deposition { get; set; }

		[ForeignKey("File")]
		[Required]
		public int FileId { get; set; }
		public virtual ef_DepositionFile File { get; set; }

		public virtual ICollection<ef_Issue> Issues { get; set; }

		public virtual ICollection<ef_RecordField> Fields { get; set; }

		public virtual ICollection<ef_Property> Properties { get; set; }

		public string Dynamic { get; set; }
	}

	public static class RecordExtensions
	{
		public static Record ToRecord(this ef_Record ef)
		{
			Record record = new Record()
			{
				DataDomain = (DataDomain)ef.DataDomain,
				Id = ef.ExternalId,
				Ordinal = ef.Ordinal,
				Original = ef.Original,
				Standardized = ef.Standardized,
				SubmissionDate = ef.SubmissionDate,
				RevisionDate = ef.RevisionDate,
				DepositionId = ef.Deposition == null ? Guid.Empty : ef.Deposition.Guid,
				File = ef.File == null ? null : new RSC.CVSP.DepositionFile()
				{
					Name = ef.File.Name,
					Fields = ef.File.Fields.Select(f => new RSC.CVSP.Field()
					{
						Name = f.Name,
						Annotaition = f.Annotation == null ? null : new RSC.CVSP.Annotation()
						{
							Name = f.Annotation.Name,
							Title = f.Annotation.Title,
							IsRequired = f.Annotation.IsRequired
						}
					}).ToList()
				},
				Issues = ef.Issues == null ? null : ef.Issues.Select(i => new RSC.Logging.Issue()
				{
					Code = i.Code,
					Id = i.LogId
				}).ToList(),
				Fields = ef.Fields == null ? null : ef.Fields.Select(f => new RSC.CVSP.RecordField()
				{
					Name = f.Field.Name,
					Value = f.Value
				}).ToList(),
			};

			if (!string.IsNullOrEmpty(ef.Dynamic))
			{
				record.Dynamic = JsonConvert.DeserializeObject<ICollection<DynamicMember>>(ef.Dynamic, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Formatting = Newtonsoft.Json.Formatting.Indented });
			}

			return record;
		}
	}
}
