using RSC.Compounds;
using ChemSpider.Mvc.Config;
using ChemSpider.Mvc.Optimization;
using System.Configuration;
using System.Web.Optimization;

namespace CompoundsWeb
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
			 * ChemSpider Toolkit Widgets
			 * ************************************************************************** */

			ScriptsBundleSection scriptsBundleConfig = ConfigurationManager.GetSection("ScriptsBundle") as ScriptsBundleSection;

			Bundle cssStyles = new StyleBundle("~/Widgets/css/css-toolkit").Include(
				"~/Widgets/css/3rd.blockui.css",
				"~/Widgets/css/3rd.lightbox.css",
				"~/Widgets/css/3rd.flexigrid.css",
				"~/Widgets/css/toolkit.generalinfo.css",
				"~/Widgets/css/toolkit.pageview.css",
				"~/Widgets/css/toolkit.gridview.css",
				"~/Widgets/css/toolkit.progress.css",
				"~/Widgets/css/toolkit.scroller.css",
				"~/Widgets/css/toolkit.tile.css",
				"~/Widgets/css/toolkit.tileview.css",
				"~/Widgets/css/toolkit.watermark.css",
				"~/Widgets/css/toolkit.editor.base.css",
				"~/Widgets/css/toolkit.data.lookup.css",
				"~/Widgets/css/toolkit.update.form.css",
				"~/Widgets/css/toolkit.generalinfo.update.css",
				"~/Widgets/css/toolkit.filter.dialog.css"
				);

			bundles.Add(cssStyles);

			if (scriptsBundleConfig != null && scriptsBundleConfig.ElementInformation.IsPresent)
			{
				cssStyles.Transforms.Add(new MinifiedBundleTransform()
				{
					Version = string.Empty,
					RootPath = "~/Widgets/css",
					Minified = CompoundsIntegration.TOOLKIT_WIDGETS_CSS_NAME_MIN,
					Full = CompoundsIntegration.TOOLKIT_WIDGETS_CSS_NAME_DEBUG
				});
			}

			//  main ChemSpider toolkit widgets bundle
			Bundle toolkitWidgets = new ScriptBundle("~/widgets/js/js-toolkit").Include(
				"~/Widgets/3rd/javascript.class.js",
				"~/Widgets/3rd/timespan-1.2.js",
				"~/Widgets/3rd/jquery.cookie.js",
				"~/Widgets/3rd/jquery.storageapi.js",
				"~/Widgets/3rd/jquery.blockui.js",
				"~/Widgets/3rd/jquery.als-1.3.min.js",
				"~/Widgets/3rd/jquery.lightbox.js",
				"~/Widgets/3rd/filesaver/blob.js",
				"~/Widgets/3rd/filesaver/filesaver.js",
				"~/Widgets/3rd/jquery.livequery.js",
				"~/Widgets/3rd/hoverIntent/jquery.hoverIntent.js",

				"~/Widgets/js/Toolkit/prototype.utils.js",

				"~/Widgets/js/Toolkit/jquery.base.js",
				"~/Widgets/js/Toolkit/jquery.resolve.js",
				"~/Widgets/js/Toolkit/jquery.base.store.js",
				"~/Widgets/js/Toolkit/jquery.utils.js",
				"~/Widgets/js/Toolkit/jquery.scripts.js",
				"~/Widgets/js/Toolkit/jquery.watermark.js",
				"~/Widgets/js/Toolkit/jquery.progress.js",
				"~/Widgets/js/Toolkit/jquery.message.js",
				"~/Widgets/js/Toolkit/jquery.zeroclipboard.js",
				"~/Widgets/js/Toolkit/jquery.filter.dialog.js",

				"~/Widgets/js/Toolkit/jquery.chemspider.store.js",

				"~/Widgets/js/Toolkit/Editor/jquery.editor.base.js",

				"~/Widgets/js/Toolkit/Lookup/jquery.data.lookup.js",

				"~/Widgets/js/Toolkit/View/jquery.pageview.js",
				"~/Widgets/js/Toolkit/View/jquery.scroller.js",
				"~/Widgets/js/Toolkit/View/jquery.tile.js",
				"~/Widgets/js/Toolkit/View/jquery.tileview.js",
				"~/Widgets/js/Toolkit/View/jquery.gridview.js",
				"~/Widgets/js/Toolkit/View/jquery.generalinfo.js",
				"~/Widgets/js/Toolkit/View/jquery.xsltransform.js",

				"~/Widgets/js/Toolkit/Update/jquery.form.js",
				"~/Widgets/js/Toolkit/Update/jquery.generalinfo.update.js"
			);

			if (scriptsBundleConfig != null && scriptsBundleConfig.ElementInformation.IsPresent)
			{
				toolkitWidgets.Transforms.Add(new MinifiedBundleTransform()
				{
					Version = string.Empty,
					RootPath = "~/Widgets/js",
					Minified = CompoundsIntegration.TOOLKIT_WIDGETS_SCRIPT_NAME_MIN,
					Full = CompoundsIntegration.TOOLKIT_WIDGETS_SCRIPT_NAME_DEBUG
				});
			}

			bundles.Add(toolkitWidgets);

			/* ****************************************************************************
			 * ChemSpider Compounds Widgets
			 * ************************************************************************** */

			cssStyles = new StyleBundle("~/Widgets/css/css-compounds").Include(
				"~/Widgets/css/compound.molecule.molecule2d.css",
				"~/Widgets/css/compound.structureeditor.css",
				"~/Widgets/css/compound.structureeditor.sketcher.css",
				"~/Widgets/css/compound.structureoptions.css",
				"~/Widgets/css/compound.view.tileview.css",
				"~/Widgets/css/compound.view.similarities.css",
				"~/Widgets/css/compound.compoundslookup.css");

			bundles.Add(cssStyles);

			if (scriptsBundleConfig != null && scriptsBundleConfig.ElementInformation.IsPresent)
			{
				cssStyles.Transforms.Add(new MinifiedBundleTransform()
				{
					Version = string.Empty,
					RootPath = "~/Widgets/css",
					Minified = CompoundsIntegration.COMPOUNDS_WIDGETS_CSS_NAME_MIN,
					Full = CompoundsIntegration.COMPOUNDS_WIDGETS_CSS_NAME_DEBUG
				});
			}

			//  main ChemSpider widgets bundle
			Bundle cscWidgets = new ScriptBundle("~/widgets/js/js-compounds").Include(
				"~/Widgets/js/Compounds/jquery.compounds.store.js",
				"~/Widgets/js/Compounds/jquery.substances.store.js",

				"~/Widgets/js/Compounds/jquery.base.js",

				"~/Widgets/js/Compounds/ChemDoodle/jquery.base.js",
				"~/Widgets/js/Compounds/ChemDoodle/jquery.base.molecule.js",
				"~/Widgets/js/Compounds/ChemDoodle/jquery.chemdoodle.rotator.js",
				"~/Widgets/js/Compounds/ChemDoodle/jquery.chemdoodle.transform.js",
				"~/Widgets/js/Compounds/ChemDoodle/jquery.chemdoodle.viewer.js",
				"~/Widgets/js/Compounds/ChemDoodle/jquery.chemdoodle2d.js",
				"~/Widgets/js/Compounds/ChemDoodle/jquery.chemdoodle3d.js",

				"~/Widgets/js/Compounds/Molecule/jquery.molecule2d.js",
				"~/Widgets/js/Compounds/Molecule/jquery.substance2d.js",
				"~/Widgets/js/Compounds/Molecule/jquery.jsmol.js",

				"~/Widgets/js/Compounds/StructureEditor/jquery.jsdraw.js",
				"~/Widgets/js/Compounds/StructureEditor/jquery.jchempaint.js",
				//"~/Widgets/js/Compounds/StructureEditor/jquery.sketcher.js",
				"~/Widgets/js/Compounds/StructureEditor/jquery.structureeditor.js",
				"~/Widgets/js/Compounds/StructureEditor/jquery.structureoptions.js",

				"~/Widgets/js/Compounds/View/jquery.compounds.tileview.js",
				"~/Widgets/js/Compounds/View/jquery.compounds.gridview.js",
				"~/Widgets/js/Compounds/View/jquery.compound.info.js",
				"~/Widgets/js/Compounds/View/jquery.compound.tile.js",

				"~/Widgets/js/Compounds/View/jquery.substances.tileview.js",
				"~/Widgets/js/Compounds/View/jquery.substance.info.js",
				"~/Widgets/js/Compounds/View/jquery.substance.tile.js",
				"~/Widgets/js/Compounds/View/jquery.similarities.js",
				"~/Widgets/js/Compounds/View/jquery.synonyms.js",

				"~/Widgets/js/Compounds/Search/jquery.compounds.gridresults.js",
				"~/Widgets/js/Compounds/Search/jquery.compounds.tileresults.js",
				"~/Widgets/js/Compounds/Search/jquery.compoundslookup.js",

				"~/Widgets/js/Compounds/NMR/jquery.nmrfeatures.js"
			);

			if (scriptsBundleConfig != null && scriptsBundleConfig.ElementInformation.IsPresent)
			{
				cscWidgets.Transforms.Add(new MinifiedBundleTransform()
				{
					Version = string.Empty,
					RootPath = "~/Widgets/js",
					Minified = CompoundsIntegration.COMPOUNDS_WIDGETS_SCRIPT_NAME_MIN,
					Full = CompoundsIntegration.COMPOUNDS_WIDGETS_SCRIPT_NAME_DEBUG
				});
			}

			bundles.Add(cscWidgets);

			/* ****************************************************************************
			 * ChemSpider UI Widgets
			 * ************************************************************************** */

			cssStyles = new StyleBundle("~/Widgets/css/css-ui").Include(
				"~/Widgets/css/ui.view.collapse.css",
				"~/Widgets/css/ui.view.searchoptions.css"
			);

			bundles.Add(cssStyles);

			if (scriptsBundleConfig != null && scriptsBundleConfig.ElementInformation.IsPresent)
			{
				cssStyles.Transforms.Add(new MinifiedBundleTransform()
				{
					Version = string.Empty,
					RootPath = "~/Widgets/css",
					Minified = CompoundsIntegration.UI_WIDGETS_CSS_NAME_MIN,
					Full = CompoundsIntegration.UI_WIDGETS_CSS_NAME_DEBUG
				});
			}

			//  ChemSpider data sources widgets bundle
			Bundle csuiWidgets = new ScriptBundle("~/widgets/js/js-ui").Include(
				"~/Widgets/js/UI/Models/javascript.searchoptions.js",

				"~/Widgets/js/UI/View/jquery.collapse.js",
				"~/Widgets/js/UI/View/jquery.searchoptions.js"
			);

			if (scriptsBundleConfig != null && scriptsBundleConfig.ElementInformation.IsPresent)
			{
				csuiWidgets.Transforms.Add(new MinifiedBundleTransform()
				{
					Version = string.Empty,
					RootPath = "~/Widgets/js",
					Minified = CompoundsIntegration.UI_WIDGETS_SCRIPT_NAME_MIN,
					Full = CompoundsIntegration.UI_WIDGETS_SCRIPT_NAME_DEBUG
				});
			}

			bundles.Add(csuiWidgets);

			/* ****************************************************************************
			 * ChemSpider Layout Widgets
			 * ************************************************************************** */

			cssStyles = new StyleBundle("~/Widgets/css/css-layout").Include(
				"~/Widgets/css/layout.area.css",
				"~/Widgets/css/layout.infobox.css",
				"~/Widgets/css/layout.infobox.tabs.css",
				"~/Widgets/css/layout.infobox.panel.css",
				"~/Widgets/css/layout.manager.css"
			);

			bundles.Add(cssStyles);

			if (scriptsBundleConfig != null && scriptsBundleConfig.ElementInformation.IsPresent)
			{
				cssStyles.Transforms.Add(new MinifiedBundleTransform()
				{
					Version = string.Empty,
					RootPath = "~/Widgets/css",
					Minified = CompoundsIntegration.LAYOUT_WIDGETS_CSS_NAME_MIN,
					Full = CompoundsIntegration.LAYOUT_WIDGETS_CSS_NAME_DEBUG
				});
			}

			//  ChemSpider data sources widgets bundle
			Bundle csLayoutWidgets = new ScriptBundle("~/widgets/js/js-layout").Include(
				"~/Widgets/js/Layout/jquery.layout.area.js",
				"~/Widgets/js/Layout/jquery.layout.infobox.js",
				"~/Widgets/js/Layout/jquery.layout.manager.js",
				"~/Widgets/js/Layout/jquery.layout.tabs.js",
				"~/Widgets/js/Layout/jquery.layout.panel.js",
				"~/Widgets/js/Layout/jquery.areastats.js",
				"~/Widgets/js/Layout/jquery.customlayout.js"
			);

			if (scriptsBundleConfig != null && scriptsBundleConfig.ElementInformation.IsPresent)
			{
				csLayoutWidgets.Transforms.Add(new MinifiedBundleTransform()
				{
					Version = string.Empty,
					RootPath = "~/Widgets/js",
					Minified = CompoundsIntegration.LAYOUT_WIDGETS_SCRIPT_NAME_MIN,
					Full = CompoundsIntegration.LAYOUT_WIDGETS_SCRIPT_NAME_DEBUG
				});
			}

			bundles.Add(csLayoutWidgets);
		}
	}
}

