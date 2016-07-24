using RSC.Search;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace RSC.Compounds.Search
{
	[Serializable]
	[DataContract]
	public class SimilarityStructureSearchOptions : SearchOptions
	{
		public enum ESimilarityType
		{
			Tanimoto = 0,
			Tversky = 1,
			Euclidian = 2,
		}

		[DataMember]
		public string Molecule { get; set; }

		[DataMember]
		[Description("In case of Tversky metric, there are optional 'alpha' and 'beta' parameters: Tversky 0.9 0.1 denotes alpha = 0.9, beta = 0.1. The default is alpha = beta = 0.5")]
		public float Alpha { get; set; }

		[DataMember]
		[Description("In case of Tversky metric, there are optional 'alpha' and 'beta' parameters: Tversky 0.9 0.1 denotes alpha = 0.9, beta = 0.1. The default is alpha = beta = 0.5")]
		public float Beta { get; set; }

		[DataMember]
		[Description("Specifying the metric to use: Tanimoto, Tversky or Euclidian")]
		public ESimilarityType SimilarityType { get; set; }

		[DataMember]
		[Description("The lower limit of the desired similarity")]
		public float Threshold { get; set; }

		public override bool IsEmpty()
		{
			return string.IsNullOrEmpty(Molecule);
		}
	}
}
