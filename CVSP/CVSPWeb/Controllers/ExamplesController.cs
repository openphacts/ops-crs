using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CVSPWeb.Controllers
{
    public class ExamplesController : Controller
    {
        // GET: Examples
        public ActionResult Index()
        {
            return View();
        }

		public ActionResult DepositionList()
		{
			return View();
		}

		public ActionResult DepositionInfo()
		{
			return View();
		}

		public ActionResult RecordInfo()
		{
			return View();
		}

		public ActionResult RecordIssues()
		{
			return View();
		}

		public ActionResult RecordProperties()
		{
			return View();
		}

		public ActionResult RecordFields()
		{
			return View();
		}

		public ActionResult RecordList()
		{
			return View();
		}
	}
}