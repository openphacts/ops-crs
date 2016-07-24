using System.Web.Mvc;

namespace CompoundsWeb.Controllers
{
	public class ErrorsController : Controller
	{
		//
		// GET: /Errors/NorFound
		public ActionResult NotFound()
		{
			return View();
		}

	}
}
