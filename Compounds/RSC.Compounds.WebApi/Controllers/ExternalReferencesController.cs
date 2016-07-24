using System;
using System.Web.Http;
using System.Web.Http.Cors;

namespace RSC.Compounds.WebApi
{
	[EnableCors("*", "*", "*")]
	public class ExternalReferencesController : ApiController
	{
		private readonly ICompoundStore compoundStore;

		public ExternalReferencesController(ICompoundStore compoundStore)
		{
			if (compoundStore == null)
			{
				throw new ArgumentNullException("compoundStore");
			}

			this.compoundStore = compoundStore;
		}

		// GET api/externalreferences/types
		/// <summary>
		/// Returns total number of compounds registered in the system
		/// </summary>
		/// <returns>Total number of compounds</returns>
		[Route("api/externalreferences/types")]
		public IHttpActionResult GetExternalReferenceTypes()
		{
			var res = compoundStore.GetExternalReferenceTypes();

			return Ok(res);
		}
	}
}
