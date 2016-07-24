using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using ChemSpider.Utilities;

namespace RSC.Compounds
{
	[Serializable]
	[DataContract]
	public class Smiles
	{
		private string smiles = string.Empty;

		public Smiles()
		{
		}

		public Smiles(string smiles)
		{
			IndigoSmiles = smiles;
		}

		[DataMember]
		[Display(Name = "Indigo Smiles")]
		public string IndigoSmiles
		{
			get
			{
				return smiles;
			}
			set
			{
				smiles = value;

				IndigoSmilesHash = smiles == null ? null : smiles.GetMD5String();
			}
		}

		[DataMember]
		[Display(Name = "Indigo Smiles Hash")]
		public string IndigoSmilesHash { get; private set; }
	}

	public class SmilesComparer : IEqualityComparer<Smiles>
	{
		bool IEqualityComparer<Smiles>.Equals(Smiles x, Smiles y)
		{
			if (Object.ReferenceEquals(x, y)) return true;

			if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
				return false;

			return x.IndigoSmiles == y.IndigoSmiles;
		}

		int IEqualityComparer<Smiles>.GetHashCode(Smiles s)
		{
			if (Object.ReferenceEquals(s, null)) return 0;

			int hashTextual = s.IndigoSmiles == null ? 0 : s.IndigoSmiles.GetHashCode();

			return hashTextual;
		}
	}
}
