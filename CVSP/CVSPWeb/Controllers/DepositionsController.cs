using ChemSpider.Profile.Data.Models;
using CVSPWeb.Models;
using RSC.CVSP;
using System;
using System.Web.Mvc;

namespace CVSPWeb.Controllers
{
    public class DepositionsController : Controller
    {
		private readonly ICVSPStore repository;

		public DepositionsController(ICVSPStore repository)
		{
			if (repository == null)
				throw new ArgumentNullException("repository");

			this.repository = repository;
		}

        // GET: Depositions
        public ActionResult Index()
        {
            return View();
        }

		[ChemSpider.Profile.RSCID.RSCIDAuthorize]
		[Route("depositions/submit")]
		public ActionResult Submit()
		{
			var profile = ChemSpiderProfile.GetUserProfile(User.Identity.Name);

			var cvspProfile = repository.GetUserProfile(profile.UserKey);

			return View(new NewDepositionModel() {
				DataSourceId = cvspProfile.Datasource == null ? Guid.Empty : (Guid)cvspProfile.Datasource
			});
		}

		// GET: /depositions/8903A31E-7AE1-4B50-A49B-72A76D19106B
		[Route("depositions/{guid}")]
		public ActionResult Get(Guid guid)
		{
			var deposition = repository.GetDeposition(guid);

			if (!deposition.IsPublic && !User.Identity.IsAuthenticated)
				return new HttpUnauthorizedResult();

			return View(guid);
		}

		// GET: /depositions/8903A31E-7AE1-4B50-A49B-72A76D19106B/chunks
		[Authorize(Roles = "Administrator")]
		[Route("depositions/{guid}/chunks")]
		public ActionResult GetDepositionChunks(Guid guid)
		{
			return View(guid);
		}

		// GET: /depositions/8903A31E-7AE1-4B50-A49B-72A76D19106B/jobs
		[Authorize(Roles = "Administrator")]
		[Route("depositions/{guid}/jobs")]
		public ActionResult GetDepositionJobs(Guid guid)
		{
			return View(guid);
		}

		// GET: /depositions/8903A31E-7AE1-4B50-A49B-72A76D19106B/annotations
		[Route("depositions/{guid}/annotations")]
		public ActionResult GetDepositionAnnotations(Guid guid)
		{
			return View(guid);
		}
	}
}