using RSC.Compounds;
using System;
using System.Web.Mvc;

namespace OpenPHACTSWeb2.Controllers
{
	public class HomeController : Controller
	{
		private readonly IStatistics statistics;

		public HomeController(IStatistics statistics)
		{
			if (statistics == null)
				throw new ArgumentNullException("statistics");

			this.statistics = statistics;
		}

		public ActionResult Index()
		{
			ViewBag.Title = "Home Page";

			var stats = statistics.GetGeneralStatistics();

			return View(stats);
		}
	}
}