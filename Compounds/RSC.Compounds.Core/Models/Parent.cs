using System;
using System.Runtime.Serialization;

namespace RSC.Compounds
{
	public enum ParentChildRelationship
	{
		Fragment = 0,
		ChargeInsensitive,
		StereoInsensitive,
		IsotopInsensitive,
		TautomerInsensitive,
		SuperInsensitive
	}

    [Serializable]
    [DataContract]
	public class Parent : Compound
	{
		public ParentChildRelationship Relationship { get; set; }
	}
}

/* ForSureChEMBL

0	510399
1	354748
2	2544648
3	10364
4	7272705
5	203015
 
*/