using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CVSPWeb.Controllers
{
	[Authorize(Roles = "Administrator")]
	public class JobsController : Controller
    {
		// GET: /jobs/8903A31E-7AE1-4B50-A49B-72A76D19106B
		[Route("jobs/{guid}")]
		public ActionResult Get(Guid guid)
		{
			return View(guid);
		}
	}
}