using CompoundsWeb.Models;
using RSC.Compounds;
using System;
using System.Web.Mvc;

namespace CompoundsWeb.Controllers
{
    public class DatasourcesController : Controller
    {
		private readonly IStatistics statistics;
		private readonly IDatasourceStore datasourceStore;

		public DatasourcesController(IDatasourceStore datasourceStore, IStatistics statistics)
		{
			if (datasourceStore == null)
			{
				throw new ArgumentNullException("datasourceStore");
			}

			if (statistics == null)
			{
				throw new ArgumentNullException("statistics");
			}

			this.datasourceStore = datasourceStore;
			this.statistics = statistics;
		}

        // GET: Datasources
        public ActionResult Index()
        {
			var stats = statistics.GetGeneralStatistics();

            return View(stats.DatasourcesNumber);
        }

		[Route("datasources/{guid}/compounds")]
		public ActionResult Compounds(Guid guid)
		{
			return View(new DatasourceCompoundsView() {
				Guid = guid,
				Count = datasourceStore.GetCompoundsCount(guid)
			});
		}
	}
}