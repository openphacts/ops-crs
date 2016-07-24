using ChemSpider.Compounds;
using ChemSpider.Compounds.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OpenPHACTSWeb.Controllers
{
	public class HomeController : Controller
	{
		private readonly ICompoundsService compoundsService;

		public HomeController(ICompoundsService compoundsService)
		{
			if (compoundsService == null)
			{
				throw new ArgumentNullException("compoundsService");
			}

			this.compoundsService = compoundsService;
		}

		public ActionResult Index()
		{
			ViewBag.Title = "Home Page";

			return View(compoundsService.GetCompoundsCount());
		}

		public ActionResult Test()
		{
			return View();
		}
	}
}
