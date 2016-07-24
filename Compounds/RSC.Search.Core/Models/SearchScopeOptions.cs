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
	public class SearchScopeOptions : SearchOptions
	{
		/// <summary>
		/// List of GUIDs
		/// </summary>
		[DataMember]
		public List<string> DataSources { get; set; }
		//[DataMember]
		//public List<string> DataSourceTypes { get; set; }
		//[DataMember]
		//public List<string> XSections { get; set; }

		public SearchScopeOptions()
		{
			DataSources = new List<string>();
			//DataSourceTypes = new List<string>();
			//XSections = new List<string>();
		}

		public override bool IsEmpty()
		{
			//return DataSources.Count == 0 && DataSourceTypes.Count == 0 && XSections.Count == 0;
			return DataSources.Count == 0;
		}
	}
}
