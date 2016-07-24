using RSC.Compounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OpenPHACTSWeb2.Controllers
{
    public class CompoundsController : Controller
    {
		private readonly CompoundStore compoundStore;

		public CompoundsController(CompoundStore compoundStore)
		{
			if (compoundStore == null)
			{
				throw new ArgumentNullException("compoundStore");
			}

			this.compoundStore = compoundStore;
		}

		public ActionResult Index()
		{
			return View();
		}

		//
		// GET: /Compouns/{id}
		[Route("compounds/{id}")]
		[HttpGet]
		public ActionResult Get(Guid id)
		{
			var compound = compoundStore.GetCompound(id);

			if (compound == null)
				return HttpNotFound();

			return View(id);
		}
	}
}