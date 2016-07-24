using RSC.Config;
using RSC.Web.Optimization;
using RSC.CVSP;
using System.Configuration;
using System.Web;
using System.Web.Optimization;

namespace CVSPWeb
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

			bundles.Add(new StyleBundle("~/Content/css-bootstrap").Include(
					"~/Content/bootstrap.css"));

			bundles.Add(new StyleBundle("~/Content/css/css-website").Include(
					"~/Content/css/chemspider-layout.css",
					"~/Content/css/home-search-tabs.css",
					"~/Content/css/jquery-ui-tune.css",
					"~/Content/css/site.css"));

			bundles.Add(new StyleBundle("~/Content/css/css-flex-website").Include(
					"~/Content/css/chemspider-flex-layout.css",
					"~/Content/css/home-search-tabs.css",
					"~/Content/css/jquery-ui-tune.css",
					"~/Content/css/site.css"));

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

			/* ****************************************************************************
			* ChemSpider CVSP Widgets
			* ************************************************************************** */

			ScriptsBundleSection scriptsBundleConfig = ConfigurationManager.GetSection("ScriptsBundle") as ScriptsBundleSection;

			var cssStyles = new StyleBundle("~/Widgets/css/css-cvsp").Include(
				"~/Widgets/css/depositions.deposition.info.css",
				"~/Widgets/css/depositions.deposition.new.css",
				"~/Widgets/css/depositions.depositions.gridview.css",
				"~/Widgets/css/depositions.deposition.status.css",
				"~/Widgets/css/depositions.deposition.progress.css",
				"~/Widgets/css/depositions.deposition.stats.css",
				"~/Widgets/css/records.record.info.css",
				"~/Widgets/css/records.records.gridview.css",
				"~/Widgets/css/ui.records.filter.css",
				"~/Widgets/css/ui.jobs.filter.css",
				"~/Widgets/css/jobs.job.info.css",
				"~/Widgets/css/chunks.chunk.info.css"
			);

			bundles.Add(cssStyles);

			if (scriptsBundleConfig != null && scriptsBundleConfig.ElementInformation.IsPresent)
			{
				cssStyles.Transforms.Add(new MinifiedBundleTransform()
				{
					Version = string.Empty,
					RootPath = "~/Widgets/css",
					Minified = CVSPIntegration.CVSP_WIDGETS_CSS_NAME_MIN,
					Full = CVSPIntegration.CVSP_WIDGETS_CSS_NAME_DEBUG
				});
			}

			Bundle cvspWidgets = new ScriptBundle("~/widgets/js/js-cvsp").Include(
				"~/Widgets/js/jquery.search.store.js",
				"~/Widgets/js/Depositions/jquery.depositions.store.js",
				"~/Widgets/js/Records/jquery.records.store.js",
				"~/Widgets/js/Chunks/jquery.chunks.store.js",
				"~/Widgets/js/Jobs/jquery.jobs.store.js",
				"~/Widgets/js/Profiles/jquery.profiles.store.js",
				"~/Widgets/js/Annotations/jquery.annotations.store.js",
				"~/Widgets/js/Logger/jquery.logger.store.js",
				"~/Widgets/js/Properties/jquery.properties.store.js",

				"~/Widgets/js/Annotations/New/jquery.annotations.add.js",
				"~/Widgets/js/Annotations/View/jquery.annotations.gridview.js",

				"~/Widgets/js/Depositions/New/jquery.deposition.new.js",
				"~/Widgets/js/Depositions/View/jquery.deposition.status.js",
				"~/Widgets/js/Depositions/View/jquery.deposition.progress.js",
				"~/Widgets/js/Depositions/View/jquery.deposition.stats.js",
				"~/Widgets/js/Depositions/View/jquery.deposition.info.js",
				"~/Widgets/js/Depositions/View/jquery.depositions.gridview.js",

				"~/Widgets/js/Logger/View/jquery.issues.gridview.js",
				"~/Widgets/js/Logger/View/jquery.issues.shortview.js",

				"~/Widgets/js/Properties/View/jquery.properties.gridview.js",

				"~/Widgets/js/Records/View/jquery.record.info.js",
				"~/Widgets/js/Records/View/jquery.fields.gridview.js",
				"~/Widgets/js/Records/View/jquery.properties.gridview.js",
				"~/Widgets/js/Records/View/jquery.chemspider.properties.gridview.js",
				"~/Widgets/js/Records/View/jquery.chemspider.synonyms.gridview.js",
				"~/Widgets/js/Records/View/jquery.records.gridview.js",

				"~/Widgets/js/Chunks/View/jquery.chunks.gridview.js",
				"~/Widgets/js/Chunks/View/jquery.chunk.info.js",

				"~/Widgets/js/Jobs/View/jquery.jobs.gridview.js",
				"~/Widgets/js/Jobs/View/jquery.job.info.js",
				"~/Widgets/js/Jobs/View/jquery.deposition.jobs.info.js",

				"~/Widgets/js/Profiles/jquery.profile.view.js",
				"~/Widgets/js/Profiles/jquery.profile.update.js",

				"~/Widgets/js/UI/jquery.records.filter.js",
				"~/Widgets/js/UI/jquery.jobs.filter.js",
				"~/Widgets/js/UI/jquery.deposition.toolbar.js"
			);

			if (scriptsBundleConfig != null && scriptsBundleConfig.ElementInformation.IsPresent)
			{
				cvspWidgets.Transforms.Add(new MinifiedBundleTransform()
				{
					Version = string.Empty,
					RootPath = "~/Widgets/js",
					Minified = CVSPIntegration.CVSP_WIDGETS_SCRIPT_NAME_MIN,
					Full = CVSPIntegration.CVSP_WIDGETS_SCRIPT_NAME_DEBUG
				});
			}

			bundles.Add(cvspWidgets);
		}
	}
}
