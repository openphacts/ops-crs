using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OpenPHACTSWeb2.Controllers
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