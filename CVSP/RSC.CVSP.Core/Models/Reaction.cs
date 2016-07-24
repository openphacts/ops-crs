using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSC.CVSP
{
	public class ReactionExtras
	{
		public Guid Id { get; set; }

		public int OriginalValidationScore { get; set; }

		public int OriginalMappedToUnmappedRation { get; set; }

		public int StandardizedValidationScore { get; set; }

		public int StandardizedMappedToUnmappedRation { get; set; }
	}
}
