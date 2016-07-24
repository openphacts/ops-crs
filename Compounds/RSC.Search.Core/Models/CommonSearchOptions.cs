using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Search.Core
{
	[Serializable]
	[DataContract]
	public class CommonSearchOptions : SearchOptions
	{
		public enum EComplexity
		{
			Any,
			Single,
			Multi
		}

		//public enum EIsotopic
		//{
		//	Any,
		//	Labeled,
		//	NotLabeled
		//}

		[DataMember]
		public EComplexity Complexity { get; set; }
		//[DataMember]
		//public EIsotopic Isotopic { get; set; }
		//[DataMember]
		//public bool HasSpectra { get; set; }
		//[DataMember]
		//public bool HasPatents { get; set; }

		public override bool IsEmpty() { return false; }
	}
}
