using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ChemSpider.Constants
{
	//needed to serialize/deserialize
	[DataContract]
	public enum ParentType
	{
		[EnumMember]
		Fragment = 0,
		[EnumMember]
		FragmentUnsensitive,
		[EnumMember]
		ChargeUnsensitive,
		[EnumMember]
		StereoUnsensitive,
		[EnumMember]
		IsotopeUnsensitive,
		[EnumMember]
		TautomerUnsensitive,
		[EnumMember]
		SuperUnsensitive,
		[EnumMember]
		Neutral_pH_Tautomer
	}
}
