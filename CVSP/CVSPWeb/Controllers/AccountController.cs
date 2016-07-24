using CVSPWeb.Models;
using System.Web.Mvc;
using System.Web.Security;

namespace CVSPWeb.Controllers
{
    public class AccountController : Controller
    {
		//
		// GET: /Account/Login
		[AllowAnonymous]
		public ActionResult Login(string returnUrl)
		{
			ViewBag.ReturnUrl = returnUrl;
			return View();
		}

		//
		// POST: /Account/Login
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult Login(LoginViewModel model, string returnUrl)
		{
			if (!ModelState.IsValid)
				return View(model);

			var result = Membership.ValidateUser(model.UserName, model.Password);
			if (result)
			{
				FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
				return RedirectToLocal(returnUrl);
			}
			else
			{
				ModelState.AddModelError("", "Invalid login attempt.");
				return View(model);
			}
		}

		public ActionResult LogOff()
		{
			FormsAuthentication.SignOut();
			return RedirectToAction("Index", "Home");
		}

		private ActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}
			return RedirectToAction("Index", "Home");
		}
	}
}