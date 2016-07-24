using RSC.CVSP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CVSPWeb.WebAPI
{
    public class ProfilesController : ApiController
    {
		private readonly ICVSPStore repository;

		public ProfilesController(ICVSPStore repository)
		{
			if (repository == null)
				throw new ArgumentNullException("repository");

			this.repository = repository;
		}

		// GET api/profiles/851107d4-ec5d-4cdf-903b-8ed94aa4132d
		/// <summary>
		/// Returns CVSP user profile by GUID
		/// </summary>
		/// <param name="guid">Internal user profile GUID</param>
		/// <returns>CVSP profile</returns>
		[Route("api/profiles/{guid}")]
		public IHttpActionResult GetProfile(Guid guid)
		{
			var profile = repository.GetUserProfile(guid);

			return Ok(profile);
		}

		/// <summary>
		/// Update user profile
		/// </summary>
		/// <param name="guid">User profile GUID</param>
		/// <param name="datasource">Updated data source's object</param>
		/// <returns>True if user profile has been successfully updated</returns>
		[Route("api/profiles/{guid}")]
		[HttpPut]
		public IHttpActionResult UpdateProfile(Guid guid, UserProfile profile)
		{
			if (guid != profile.Id)
				return BadRequest("GUIDs are not equal");

			var result = repository.UpdateUserProfile(profile);

			return Ok(result);
		}

		// GET api/profiles/authenticated
		/// <summary>
		/// Check if user authenticated
		/// </summary>
		[Route("api/profiles/authenticated")]
		[HttpGet]
		public IHttpActionResult IsAuthenticated()
		{
			return Ok(User.Identity.IsAuthenticated);
		}

		// GET api/profiles/isadmin
		/// <summary>
		/// Check if user administrator
		/// </summary>
		[Route("api/profiles/isadmin")]
		[HttpGet]
		public IHttpActionResult IsAdmin()
		{
			return Ok(User.IsInRole("Administrator"));
		}
	}
}
