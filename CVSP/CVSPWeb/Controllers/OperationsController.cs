using RSC.CVSP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CVSPWeb.Controllers
{
    public class OperationsController : Controller
    {
		private readonly IOperationsManager manager;

		public OperationsController(IOperationsManager manager)
		{
			if (manager == null)
				throw new ArgumentNullException("manager");

			this.manager = manager;
		}

        // GET: Operations
        public ActionResult Index()
        {
			var operationsInfo = manager.GetOperationsInfo();

			return View(operationsInfo);
        }
    }
}