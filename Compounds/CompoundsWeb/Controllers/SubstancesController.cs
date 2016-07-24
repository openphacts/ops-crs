using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ChemSpider.ObjectModel;
using RSC.Compounds;

namespace CompoundsWeb.Controllers
{
	public class SubstancesController : Controller
	{
		private readonly SubstanceStore substanceStore;

		public SubstancesController(SubstanceStore substanceStore)
		{
			if (substanceStore == null)
			{
				throw new ArgumentNullException("substanceStore");
			}

			this.substanceStore = substanceStore;
		}

		//
		// GET: /Substances/
		public ActionResult Index()
		{
			var count = substanceStore.GetSubstancesCount();
			
			return View(count);
		}

		//
		// GET: /Substances/5
		[Route("substances/{id}")]
		public ActionResult Get(Guid id)
		{
			var revision = substanceStore.GetRevision(id);

			if (revision == null)
				return HttpNotFound();

			//IEnumerable<Issue> issues = substanceStore.GetIssues(id);
			//substance.ISSUES = issues.GroupBy(i => new { i.Code }).Select(g=>g.First()).ToList();//just single occurentce of code needs to be shown
			return View(revision);
		}
	}
}