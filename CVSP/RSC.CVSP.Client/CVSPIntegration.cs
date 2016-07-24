using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP
{
	public class CVSPIntegration
	{
		public const string CVSP_WIDGETS_SCRIPT_NAME_MIN = "jquery.cvsp.min.js";
		public const string CVSP_WIDGETS_SCRIPT_NAME_DEBUG = "jquery.cvsp.js";
		public const string CVSP_WIDGETS_CSS_NAME_MIN = "rsc.cvsp.min.css";
		public const string CVSP_WIDGETS_CSS_NAME_DEBUG = "rsc.cvsp.css";

		private static CVSPSection Config { get { return ConfigurationManager.GetSection("RSC.CVSP") as CVSPSection; } }

		public static bool IsEnabled
		{
			get { return Config != null && Config.ElementInformation.IsPresent; }
		}

		public static string Url
		{
			get { return Config != null ? Config.Base.Url : null; }
		}

		public static Uri BaseUri
		{
			get { return Config != null ? new Uri(Config.Base.Url) : null; }
		}

		public static string Script
		{
			get { return Config != null ? new Uri(BaseUri, string.Format("Widgets/js/{0}", Config.Debug.Value ? CVSP_WIDGETS_SCRIPT_NAME_DEBUG : CVSP_WIDGETS_SCRIPT_NAME_MIN)).ToString() : null; }
		}

		public static string CSS
		{
			get { return Config != null ? new Uri(BaseUri, string.Format("Widgets/css/{0}", Config.Debug.Value ? CVSP_WIDGETS_CSS_NAME_DEBUG : CVSP_WIDGETS_CSS_NAME_MIN)).ToString() : null; }
		}
	}
}
