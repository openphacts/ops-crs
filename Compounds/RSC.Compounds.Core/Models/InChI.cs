using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds
{
	[Serializable]
	[DataContract]
	public class InChI
	{
		public InChI()
		{
		}

		public InChI(string inchi, string key)
		{
			Inchi = inchi;
			InChIKey = key;
		}

		[DataMember]
		public string Inchi { get; set; }

		[DataMember]
		public string InChIKey { get; set; }
		
	}

	public class InChIComparer : IEqualityComparer<InChI>
	{
		bool IEqualityComparer<InChI>.Equals(InChI x, InChI y)
		{
			if (Object.ReferenceEquals(x, y)) return true;

			if (Object.ReferenceEquals(x, null) ||
				Object.ReferenceEquals(y, null))
				return false;

			return x.InChIKey == y.InChIKey;
		}

		int IEqualityComparer<InChI>.GetHashCode(InChI inchi)
		{
			if (Object.ReferenceEquals(inchi, null)) return 0;

			int hashTextual = inchi.InChIKey == null
				? 0 : inchi.InChIKey.GetHashCode();

			return hashTextual;
		}
	}
}
