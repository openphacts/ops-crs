using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSC.Compounds;

namespace RSC.Compounds.EntityFramework
{
	[Table("SynonymFlags")]
	public class ef_SynonymFlag
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int Id { get; set; }

		[MaxLength(20)]
		[Required]
		public string Name { get; set; }

		[MaxLength(200)]
		public string Description { get; set; }

		[MaxLength(200)]
		public string Url { get; set; }

		public int Rank { get; set; }

		public bool ExcludeFromTitle { get; set; }

        public SynonymFlagType Type { get; set; }

        public bool IsUniquePerLanguage { get; set; }

        public string RegEx { get; set; }

        public bool IsRestricted { get; set; }
	}
}
