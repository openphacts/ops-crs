using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace RSC.CVSP
{
	public class UserProfile
	{
		public UserProfile()
		{
			UserVariableCollection = new List<UserVariable>();
			RuleSetCollection = new List<RuleSet>();
		}

		[Display(Name = "User Guid")]
		public Guid Id { get; set; }

		public bool SendEmail { get; set; }

		public string FtpDirectory { get; set; }

		public Guid? Datasource { get; set; }

		public IEnumerable<UserVariable> UserVariableCollection { get; set; }

		public IEnumerable<RuleSet> RuleSetCollection { get; set; }
	}
}
