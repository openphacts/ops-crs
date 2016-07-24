using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace RSC.Compounds.EntityFramework
{
	[Table("InChIMD5s")]
	public class ef_InChIMD5
	{
		[DatabaseGenerated(DatabaseGeneratedOption.None), Key()]
		public Guid Id { get; set; }

		[Required]
		[Index]
		[MaxLength(14)]
		public string InChIKey_A { get; set; }

		[Required]
		[Index]
		[MaxLength(23)]
		public string InChIKey_B { get; set; }

		[Required]
		[Index(IsUnique = true)]
		[MaxLength(16)]
		public byte[] InChI_MD5 { get; set; }

		[Required]
		[Index]
		[MaxLength(16)]
		public byte[] InChI_MF_MD5 { get; set; }

		[Required]
		[Index]
		[MaxLength(16)]
		public byte[] InChI_C_MD5 { get; set; }

		[Required]
		[Index]
		[MaxLength(16)]
		public byte[] InChI_CH_MD5 { get; set; }

		[Required]
		[Index]
		[MaxLength(16)]
		public byte[] InChI_CHSI_MD5 { get; set; }

		[Required]
		public virtual ef_InChI InChI { get; set; }
	}
}
