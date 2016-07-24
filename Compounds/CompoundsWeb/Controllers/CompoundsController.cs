using System.Web.Mvc;
using RSC.Compounds;
using System;

namespace CompoundsWeb.Controllers
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
			//var count = compoundStore.GetCompoundsCount();

			return View();
		}

		//
		// GET: /Compouns/{id}
		[Route("compounds/{id}")]
		public ActionResult Get(Guid id)
		{
			var compound = compoundStore.GetCompound(id);

			if (compound == null)
				return HttpNotFound();

			return View(id);
		}

		public ActionResult Flex(int id)
		{
			return View(id);
		}

		public ActionResult FlexConfig(int id)
		{
			return View(id);
		}
	}
}