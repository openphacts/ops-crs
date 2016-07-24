using System.Collections.Generic;

namespace RSC.Compounds
{
	public class RecordData
	{
		public string ExternalId { get; set; }
		public Revision Revision { get; set; }
		public Compound Compound { get; set; }
		public IEnumerable<Parent> Parents { get; set; }
	}
}
