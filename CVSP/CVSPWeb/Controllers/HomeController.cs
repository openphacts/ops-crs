using ChemSpider.Profile.Data.Models;
using RSC.CVSP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

namespace CVSPWeb.Controllers
{
	public class HomeController : Controller
	{
		private readonly ICVSPStore repository;

		public HomeController(ICVSPStore repository)
		{
			if (repository == null)
				throw new ArgumentNullException("repository");

			this.repository = repository;
		}

		protected override void OnAuthentication(AuthenticationContext filterContext)
		{
			if(User.Identity.IsAuthenticated)
			{
				//	check if CVSP user profile created...
				var profile = ChemSpiderProfile.GetUserProfile(User.Identity.Name);

				var cvspProfile = repository.GetUserProfile(profile.UserKey);
				if (cvspProfile == null)
				{
					repository.CreateUserProfile(new UserProfile()
					{
						Id = profile.UserKey
					});
				}
			}
		}

		public ActionResult Index()
		{
			return View();
		}

		[ChemSpider.Profile.RSCID.RSCIDAuthorize]
		public ActionResult CVSPProfile()
		{
			var profile = ChemSpiderProfile.GetUserProfile(User.Identity.Name);
			return View(profile.UserKey);
		}
	}
}