using System;
using System.Configuration;

namespace RSC.Compounds
{
	public class CompoundsIntegration
	{
		public const string TOOLKIT_WIDGETS_SCRIPT_NAME_MIN = "jquery.toolkit.min.js";
		public const string TOOLKIT_WIDGETS_SCRIPT_NAME_DEBUG = "jquery.toolkit.js";
		public const string TOOLKIT_WIDGETS_CSS_NAME_MIN = "chemspider.toolkit.min.css";
		public const string TOOLKIT_WIDGETS_CSS_NAME_DEBUG = "chemspider.toolkit.css";

		public const string COMPOUNDS_WIDGETS_SCRIPT_NAME_MIN = "jquery.compounds.min.js";
		public const string COMPOUNDS_WIDGETS_SCRIPT_NAME_DEBUG = "jquery.compounds.js";
		public const string COMPOUNDS_WIDGETS_CSS_NAME_MIN = "chemspider.compounds.min.css";
		public const string COMPOUNDS_WIDGETS_CSS_NAME_DEBUG = "chemspider.compounds.css";

		//public const string DATASOURCES_WIDGETS_SCRIPT_NAME_MIN = "jquery.datasources.min.js";
		//public const string DATASOURCES_WIDGETS_SCRIPT_NAME_DEBUG = "jquery.datasources.js";
		//public const string DATASOURCES_WIDGETS_CSS_NAME_MIN = "chemspider.datasources.min.css";
		//public const string DATASOURCES_WIDGETS_CSS_NAME_DEBUG = "chemspider.datasources.css";

		public const string UI_WIDGETS_SCRIPT_NAME_MIN = "jquery.ui.min.js";
		public const string UI_WIDGETS_SCRIPT_NAME_DEBUG = "jquery.ui.js";
		public const string UI_WIDGETS_CSS_NAME_MIN = "chemspider.ui.min.css";
		public const string UI_WIDGETS_CSS_NAME_DEBUG = "chemspider.ui.css";

		public const string LAYOUT_WIDGETS_SCRIPT_NAME_MIN = "jquery.layout.min.js";
		public const string LAYOUT_WIDGETS_SCRIPT_NAME_DEBUG = "jquery.layout.js";
		public const string LAYOUT_WIDGETS_CSS_NAME_MIN = "chemspider.layout.min.css";
		public const string LAYOUT_WIDGETS_CSS_NAME_DEBUG = "chemspider.layout.css";

		private static CompoundsSection Config { get { return ConfigurationManager.GetSection("RSC.Compounds") as CompoundsSection; } }

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

		public static string UIScript
		{
			get { return Config != null ? new Uri(BaseUri, string.Format("Widgets/js/{0}", Config.Debug.Value ? UI_WIDGETS_SCRIPT_NAME_DEBUG : UI_WIDGETS_SCRIPT_NAME_MIN)).ToString() : null; }
		}

		public static string UICSS
		{
			get { return Config != null ? new Uri(BaseUri, string.Format("Widgets/css/{0}", Config.Debug.Value ? UI_WIDGETS_CSS_NAME_DEBUG : UI_WIDGETS_CSS_NAME_MIN)).ToString() : null; }
		}

		public static string LayoutScript
		{
			get { return Config != null ? new Uri(BaseUri, string.Format("Widgets/js/{0}", Config.Debug.Value ? LAYOUT_WIDGETS_SCRIPT_NAME_DEBUG : LAYOUT_WIDGETS_SCRIPT_NAME_MIN)).ToString() : null; }
		}

		public static string LayoutCSS
		{
			get { return Config != null ? new Uri(BaseUri, string.Format("Widgets/css/{0}", Config.Debug.Value ? LAYOUT_WIDGETS_CSS_NAME_DEBUG : LAYOUT_WIDGETS_CSS_NAME_MIN)).ToString() : null; }
		}

		public static string ToolkitScript
		{
			get { return Config != null ? new Uri(BaseUri, string.Format("Widgets/js/{0}", Config.Debug.Value ? TOOLKIT_WIDGETS_SCRIPT_NAME_DEBUG : TOOLKIT_WIDGETS_SCRIPT_NAME_MIN)).ToString() : null; }
		}

		public static string ToolkitCSS
		{
			get { return Config != null ? new Uri(BaseUri, string.Format("Widgets/css/{0}", Config.Debug.Value ? TOOLKIT_WIDGETS_CSS_NAME_DEBUG : TOOLKIT_WIDGETS_CSS_NAME_MIN)).ToString() : null; }
		}

		public static string Script
		{
			get { return Config != null ? new Uri(BaseUri, string.Format("Widgets/js/{0}", Config.Debug.Value ? COMPOUNDS_WIDGETS_SCRIPT_NAME_DEBUG : COMPOUNDS_WIDGETS_SCRIPT_NAME_MIN)).ToString() : null; }
		}

		public static string CSS
		{
			get { return Config != null ? new Uri(BaseUri, string.Format("Widgets/css/{0}", Config.Debug.Value ? COMPOUNDS_WIDGETS_CSS_NAME_DEBUG : COMPOUNDS_WIDGETS_CSS_NAME_MIN)).ToString() : null; }
		}
	}
}
