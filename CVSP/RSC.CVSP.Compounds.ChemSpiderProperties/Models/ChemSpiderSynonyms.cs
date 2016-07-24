using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using RSC.Compounds;

namespace RSC.CVSP.Compounds
{
	[DataContract]
	[Serializable]
	public class ChemSpiderSynonyms
	{
        public ChemSpiderSynonyms()
		{
            Synonyms = new List<Synonym>();
		}

		[DataMember]
        public ICollection<Synonym> Synonyms { get; set; }
	}
}
