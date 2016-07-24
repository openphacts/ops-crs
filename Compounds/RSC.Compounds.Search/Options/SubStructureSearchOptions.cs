using RSC.Search;
using System;
using System.Runtime.Serialization;

namespace RSC.Compounds.Search
{
	[Serializable]
	[DataContract]
	public class SubStructureSearchOptions : SearchOptions
	{
		public enum EMolType
		{
			SMILES = 0,
			SMARTS = 1,
		}

		[DataMember]
		public string Molecule { get; set; }

		[DataMember]
		public bool MatchTautomers { get; set; }

		[DataMember]
		public EMolType MolType { get; set; }

		public override bool IsEmpty()
		{
			return string.IsNullOrEmpty(Molecule);
		}
	}
}
