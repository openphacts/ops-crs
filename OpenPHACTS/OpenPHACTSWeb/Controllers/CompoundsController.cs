using ChemSpider.Compounds;
using System;
using System.Web.Mvc;

namespace OpenPHACTSWeb.Controllers
{
	public class CompoundsController : Controller
	{
		private readonly ICompoundsService compoundsService;

		public CompoundsController(ICompoundsService compoundsService)
		{
			if (compoundsService == null)
			{
				throw new ArgumentNullException("compoundsService");
			}

			this.compoundsService = compoundsService;
		}

		public ActionResult Index()
		{
			var count = compoundsService.GetCompoundsCount();

			return View(count);
		}

		//
		// GET: /Compouns/{id}
		public ActionResult Get(int id)
		{
			Compound compound = compoundsService.GetCompound(id);

			if (compound == null)
				return HttpNotFound();

			return View(id);
		}
	}
}