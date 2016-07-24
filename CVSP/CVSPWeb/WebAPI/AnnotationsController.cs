using RSC.CVSP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CVSPWeb.WebAPI
{
	[EnableCors("*", "*", "*")]
	public class AnnotationsController : ApiController
    {
		private readonly ICVSPStore repository;

		public AnnotationsController(ICVSPStore repository)
		{
			if (repository == null)
				throw new ArgumentNullException("repository");

			this.repository = repository;
		}

		// GET api/annotations/all
		/// <summary>
		/// Returns all annotations registered in the system
		/// </summary>
		/// <returns>List of annotations</returns>
		[Route("api/annotations/all")]
		[HttpGet]
		public IHttpActionResult GeAllAnnotations()
		{
			var annotations = repository.GetAllAnnotations();

			return Ok(annotations);
		}

		// GET api/depositions/851107d4-ec5d-4cdf-903b-8ed94aa4132d/annotations
		/// <summary>
		/// Returns deposition's annotations list
		/// </summary>
		/// <returns>List of annotations</returns>
		[Route("api/depositions/{guid}/annotations")]
		[HttpGet]
		public IHttpActionResult GeDepositionAnnotations(Guid guid)
		{
			var annotations = repository.GetDepositionAnnotations(guid);

			return Ok(annotations);
		}

		// PUT api/depositions/851107d4-ec5d-4cdf-903b-8ed94aa4132d/annotate
		/// <summary>
		/// Annotate deposition's field
		/// </summary>
		/// <param name="guid">Deposition's GUID</param>
		/// <param name="field">Field's name</param>
		/// <param name="annotation">Annotation's name</param>
		/// <returns>True if annotation was successfull</returns>
		[Route("api/depositions/{guid}/annotate")]
		[HttpPut]
		public IHttpActionResult AnnotateDepositionField(Guid guid, CVSPWeb.Models.FieldAnnotation annotation)
		{
			var res = repository.AnnotateDepositionField(guid, annotation.Field, annotation.Annotation);

			return Ok(res);
		}

		// DELETE api/depositions/851107d4-ec5d-4cdf-903b-8ed94aa4132d/annotation/extid
		/// <summary>
		/// Delete deposition's annotation
		/// </summary>
		/// <param name="guid">Deposition's GUID</param>
		/// <param name="annotation">Annotation's name</param>
		/// <returns>True if annotation have beed deleted successfully</returns>
		[Route("api/depositions/{guid}/annotation/{annotation}")]
		[HttpDelete]
		public IHttpActionResult DeleteDepositionAnnotation(Guid guid, string annotation)
		{
			var res = repository.DeleteDepositionAnnotation(guid, annotation);

			return Ok(res);
		}
	}
}
