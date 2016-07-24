using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP
{
	public class CVSPSection : ConfigurationSection
	{
		public class BoolValueElement : ConfigurationElement
		{
			[ConfigurationProperty("value", IsKey = true, IsRequired = true)]
			public bool Value
			{
				get { return (bool)this["value"]; }
			}
		}

		public class UrlElement : ConfigurationElement
		{
			[ConfigurationProperty("url", IsKey = true, IsRequired = true)]
			public string Url
			{
				get { return this["url"] as string; }
			}
		}

		[ConfigurationProperty("base", IsKey = true, IsRequired = true)]
		public UrlElement Base
		{
			get { return this["base"] as UrlElement; }
		}

		[ConfigurationProperty("debug")]
		public BoolValueElement Debug
		{
			get { return this["debug"] as BoolValueElement; }
		}
	}
}
