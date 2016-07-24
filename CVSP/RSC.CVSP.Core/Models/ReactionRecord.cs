using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP
{
	public class ReactionRecord : Record
	{
		public ReactionRecord() : base()
		{
			ReactionExtras = new ReactionExtras();
		}

		[DataMember]
		public ReactionExtras ReactionExtras { get; set; }
	}
}
