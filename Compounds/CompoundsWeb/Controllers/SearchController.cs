using System.Web.Mvc;

namespace CompoundsWeb.Controllers
{
	public class SearchController : Controller
	{
		//
		// GET: /Search/Results/
		public ActionResult Results(string id)
		{
			return View(id as object);
		}

		//
		// GET: /Search/Settings/
		public ActionResult Settings()
		{
			return View();
		}
	}
}