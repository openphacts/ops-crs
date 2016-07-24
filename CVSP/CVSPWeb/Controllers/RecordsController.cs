using RSC.CVSP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CVSPWeb.Controllers
{
    public class RecordsController : Controller
    {
		private readonly ICVSPStore repository;

		public RecordsController(ICVSPStore repository)
		{
			if (repository == null)
				throw new ArgumentNullException("repository");

			this.repository = repository;
		}

		// GET: /records/8903A31E-7AE1-4B50-A49B-72A76D19106B
		[Route("records/{guid}")]
		public ActionResult Get(Guid guid)
		{
			var record = repository.GetRecord(guid);

			var deposition = repository.GetDeposition(record.DepositionId);

			if(!deposition.IsPublic && !User.Identity.IsAuthenticated)
				return new HttpUnauthorizedResult();

			return View(guid);
		}
	}
}