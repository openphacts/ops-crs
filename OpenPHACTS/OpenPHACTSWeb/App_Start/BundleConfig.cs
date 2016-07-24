using System.Web;
using System.Web.Optimization;

namespace OpenPHACTSWeb
{
	public class BundleConfig
	{
		// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
						"~/Scripts/jquery-{version}.js"));

			bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
						"~/Scripts/jquery-ui-{version}.js"));

			bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
						"~/Scripts/jquery.validate*"));

			// Use the development version of Modernizr to develop with and learn from. Then, when you're
			// ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
			bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
						"~/Scripts/modernizr-*"));

			bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
					  "~/Scripts/bootstrap.js",
					  "~/Scripts/respond.js"));

			bundles.Add(new StyleBundle("~/Content/css").Include(
					  "~/Content/bootstrap.css",
					  "~/Content/site.css"));

			bundles.Add(new StyleBundle("~/Content/css-bootstrap").Include(
					"~/Content/bootstrap.css"));

			bundles.Add(new StyleBundle("~/Content/css/css-website").Include(
					"~/Content/css/chemspider-layout.css",
					"~/Content/css/home-search-tabs.css",
					"~/Content/css/jquery-ui-tune.css",
					"~/Content/css/site.css"));

			//bundles.Add(new StyleBundle("~/Content/themes/base/css-jqueryui").Include(
			//	"~/Content/themes/base/jquery.ui.accordion.css",
			//	"~/Content/themes/base/jquery.ui.autocomplete.css",
			//	"~/Content/themes/base/jquery.ui.button.css",
			//	"~/Content/themes/base/jquery.ui.core.css",
			//	"~/Content/themes/base/jquery.ui.datepicker.css",
			//	"~/Content/themes/base/jquery.ui.dialog.css",
			//	"~/Content/themes/base/jquery.ui.menu.css",
			//	"~/Content/themes/base/jquery.ui.progressbar.css",
			//	"~/Content/themes/base/jquery.ui.resizable.css",
			//	"~/Content/themes/base/jquery.ui.selectable.css",
			//	"~/Content/themes/base/jquery.ui.slider.css",
			//	"~/Content/themes/base/jquery.ui.spinner.css",
			//	"~/Content/themes/base/jquery.ui.tabs.css",
			//	"~/Content/themes/base/jquery.ui.theme.css",
			//	"~/Content/themes/base/jquery.ui.tooltip.css"
			//));

			bundles.Add(new StyleBundle("~/Content/themes/base/css-jqueryui").Include(
				"~/Content/themes/base/accordion.css",
				"~/Content/themes/base/autocomplete.css",
				"~/Content/themes/base/button.css",
				"~/Content/themes/base/core.css",
				"~/Content/themes/base/datepicker.css",
				"~/Content/themes/base/dialog.css",
				"~/Content/themes/base/menu.css",
				"~/Content/themes/base/progressbar.css",
				"~/Content/themes/base/resizable.css",
				"~/Content/themes/base/selectable.css",
				"~/Content/themes/base/slider.css",
				"~/Content/themes/base/spinner.css",
				"~/Content/themes/base/tabs.css",
				"~/Content/themes/base/theme.css",
				"~/Content/themes/base/tooltip.css"
			));

			// Set EnableOptimizations to false for debugging. For more information,
			// visit http://go.microsoft.com/fwlink/?LinkId=301862
			//BundleTable.EnableOptimizations = true;
		}
	}
}
