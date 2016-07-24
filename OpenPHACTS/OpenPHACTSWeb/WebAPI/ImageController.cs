using ChemImageNet;
using ChemSpider.Compounds.Database;
using MoleculeObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace OpenPHACTSWeb.WebAPI.Controllers
{
/*
	public class ImageController : ApiController
	{
		protected CompoundProvider compoundProvider = new CompoundProvider();
		protected SubstanceProvider substanceProvider = new SubstanceProvider();

		// GET api/substance/image/5
		/// <summary>
		/// Returns substance's image
		/// </summary>
		/// <param name="id">Internal substance ID</param>
		/// <param name="version">Internal substance version</param>
		/// <param name="width">Image width</param>
		/// <param name="height">Image height</param>
		/// <returns></returns>
		[AcceptVerbs("GET")]
		[Route("api/image/substance/{id}")]
		public HttpResponseMessage SubstanceImage(int id, int? version = null, int width = 300, int height = 300)
		{
			byte[] sdf = substanceProvider.GetSDF(id, version);

			byte[] image = Renderers.ImageFromMolecule(sdf, width, height);

			HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new ByteArrayContent(image),
			};

			result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/gif");

			return result;
		}

		// GET api/compounds/image/5
		/// <summary>
		/// Returns compound's image
		/// </summary>
		/// <param name="id">Internal compound ID</param>
		/// <param name="width">Image width</param>
		/// <param name="height">Image height</param>
		/// <returns></returns>
		[AcceptVerbs("GET")]
		[Route("api/image/compound/{id}")]
		public HttpResponseMessage CompoundImage(int id, int width = 300, int height = 300)
		{
			byte[] sdf = compoundProvider.GetSDF(id);

			byte[] image = Renderers.ImageFromMolecule(sdf, width, height);

			HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new ByteArrayContent(image),
			};

			result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/gif");

			return result;
		}

		// GET api/compounds/image/5
		/// <summary>
		/// Returns image based on SMILES
		/// </summary>
		/// <param name="smiles">SMILES</param>
		/// <param name="width">Image width</param>
		/// <param name="height">Image height</param>
		/// <returns></returns>
		[AcceptVerbs("GET")]
		[Route("api/image/compound/smiles")]
		public HttpResponseMessage CompoundImageSMILES(string smiles, int width = 300, int height = 300)
		{
			string mol = OpenEyeNet.OpenEyeUtility.GetInstance().SMILESToMol(smiles);
			if (string.IsNullOrEmpty(mol))
				mol = OpenEyeNet.OpenEyeUtility.GetInstance().Clean(mol, false);

			byte[] image = ChemImage.GetInstance().sdf2image(mol, width, height, true);

			HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new ByteArrayContent(image),
			};

			result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/gif");

			return result;
		}
	}
*/
}
