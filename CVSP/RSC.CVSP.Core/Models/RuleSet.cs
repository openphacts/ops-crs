using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP
{

	[DataContract]
	public enum RuleType
	{
		[Display(Name = "Acid-Base rule set")]
		[EnumMember]
		AcidBase = 1,

		[Display(Name = "Validation rule set")]
		[EnumMember]
		Validation,

		[Display(Name = "Standardization rule set")]
		[EnumMember]
		Standardization,

		[Display(Name = "Unidentified rule set")]
		[EnumMember]
		None
	}

	[Serializable]
	[DataContract]
	public class RuleSet
	{
		public RuleSet()
		{
		}

		public RuleSet(Guid userGuid, RuleType type, string title, string description, string body)
		{
			Collaboraters = new List<Guid>();
			Type = type;
			Title = title;
			UserGuid = userGuid;
			Description = description;
			RuleSetBody = body;
			IsPublic = false;
			IsApproved = false;
			IsPlatformDefault = false;
			IsValidated = false;
			CountOfCloned = 0;
		}

		public Guid Id { get; set; }

		public Guid UserGuid { get; set; }

		public RuleType Type { get; set; }
		public DateTime DateCreated { get; set; }

		public DateTime? DateRevised { get; set; }

		public string Title { get; set; }

		public string Description { get; set; }

		public string RuleSetBody { get; set; }

		public bool IsPlatformDefault { get; set; }

		public bool IsValidated { get; set; }

		public string XmlValidationMessages { get; set; }

		public bool IsApproved { get; set; }

		public bool IsPublic { get; set; }

		public int CountOfCloned { get; set; }

		public IEnumerable<Guid> Collaboraters { get; set; }
	}
}
