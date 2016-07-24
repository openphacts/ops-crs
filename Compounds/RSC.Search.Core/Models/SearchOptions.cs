using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Search.Core
{
	[Serializable]
	public abstract class SearchOptions
	{
		public abstract bool IsEmpty();
		public static bool IsNullOrEmpty(SearchOptions o)
		{
			return o == null || o.IsEmpty();
		}
	}
}
