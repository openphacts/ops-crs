using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ChemSpider.ObjectModel;
using ChemSpider.Compounds;
using ChemSpider.Compounds.Database;
using System;


namespace OpenPHACTSWeb.Controllers
{
	public class SubstancesController : Controller
	{
		private readonly ISubstancesService substancesService;

		public SubstancesController(ISubstancesService substancesService)
		{
			if (substancesService == null)
			{
				throw new ArgumentNullException("substancesService");
			}

			this.substancesService = substancesService;
		}

		//
		// GET: /Substances/
		public ActionResult Index()
		{
			var count = substancesService.GetSubstancesCount();
			
			return View(count);
		}

		//
		// GET: /Substances/
		public ActionResult Get(int id)
		{
			Substance substance = substancesService.GetSubstance(id);

			if (substance == null)
				return HttpNotFound();

			IEnumerable<Issue> issues = substancesService.GetIssues(id);
			substance.ISSUES = issues.GroupBy(i => new { i.Code }).Select(g=>g.First()).ToList();//just single occurentce of code needs to be shown
			return View(substance);
		}
	}
}