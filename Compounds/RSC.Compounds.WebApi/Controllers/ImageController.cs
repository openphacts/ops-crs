using RSC.Compounds;
using com.ggasoftware.indigo;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace RSC.Compounds.WebApi
{
	public class ImageController : ApiController
	{
		static private Indigo s_indigo;
		static private IndigoRenderer s_renderer;

		private readonly CompoundStore compoundStore;
		private readonly SubstanceStore substanceStore;

		public ImageController(CompoundStore compoundStore, SubstanceStore substanceStore)
		{
			if (compoundStore == null)
			{
				throw new ArgumentNullException("compoundStore");
			}

			if (substanceStore == null)
			{
				throw new ArgumentNullException("substanceStore");
			}

			this.compoundStore = compoundStore;
			this.substanceStore = substanceStore;
		}

		static ImageController()
		{
			s_indigo = new Indigo();
			s_indigo.setOption("ignore-stereochemistry-errors", true);
			s_indigo.setOption("ignore-noncritical-query-features", true);

			s_renderer = new IndigoRenderer(s_indigo);
			s_indigo.setOption("render-output-format", "png");
			s_indigo.setOption("render-stereo-style", "ext");
			s_indigo.setOption("render-margins", 5, 5);
			s_indigo.setOption("render-coloring", true);
			s_indigo.setOption("render-relative-thickness","1.5");
		}

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
		public HttpResponseMessage SubstanceImage(Guid id, int? version = null, int width = 300, int height = 300)
		{
			var mol = substanceStore.GetSDF(id, version);

			byte[] image = Mol2Image(mol, width, height);

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
		public HttpResponseMessage CompoundImage(Guid id, int width = 300, int height = 300)
		{
			var mol = compoundStore.GetMol(id);

			byte[] image = Mol2Image(mol, width, height);

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
		/// <param name="stripH">Strip hydrogens from the result image</param>
		/// <returns></returns>
		[AcceptVerbs("GET")]
		[Route("api/image/compound/smiles")]
		public HttpResponseMessage CompoundImageSMILES(string smiles, int width = 300, int height = 300)
		{
			byte[] image = Smiles2Image(smiles, width, height);

			HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new ByteArrayContent(image),
			};

			result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/gif");

			return result;
		}

		// GET api/compounds/image/5
		/// <summary>
		/// Returns image based on URL that returns MOL file
		/// </summary>
		/// <param name="url">URL to MOL file</param>
		/// <param name="width">Image width</param>
		/// <param name="height">Image height</param>
		/// <returns></returns>
		[AcceptVerbs("GET")]
		[Route("api/image/compound/url")]
		public HttpResponseMessage CompoundImageURL(string url, int width = 300, int height = 300)
		{
			using (var webClient = new System.Net.WebClient())
			{
				string mol = webClient.DownloadString(url);

				byte[] image = Mol2Image(mol, width, height);

				HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new ByteArrayContent(image),
				};

				result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/gif");

				return result;
			}
		}

		[AcceptVerbs("POST")]
		[Route("api/image/compound/mol")]
		[EnableCors("*", "*", "*")]
		public IHttpActionResult CompoundImageMOL([FromBody] Mol2Image options)
		{
			byte[] image = Mol2Image(options.Mol, options.Width, options.Height);
			return Ok(Convert.ToBase64String(image));
		}

		private byte[] Mol2Image(string mol, int width, int height)
		{
			lock (s_indigo)
			{
				try
				{
					s_indigo.setOption("render-image-size", width, height);

					//IndigoObject molObj = s_indigo.loadQueryMolecule(mol);
					IndigoObject molObj = s_indigo.loadMolecule(mol);

					return s_renderer.renderToBuffer(molObj);
				}
				catch (Exception)
				{
					return File.ReadAllBytes(HttpContext.Current.Server.MapPath("~/images/error-logo.png"));
				}
			}
		}

		private byte[] Smiles2Image(string smiles, int width, int height)
		{
			lock (s_indigo)
			{
				try
				{
					s_indigo.setOption("render-image-size", width, height);

					IndigoObject molObj = s_indigo.loadMolecule(smiles);

					return s_renderer.renderToBuffer(molObj);
				}
				catch (Exception)
				{
					return File.ReadAllBytes(HttpContext.Current.Server.MapPath("~/images/error-logo.png"));
				}
			}
		}
	}

	public class Mol2Image
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public string Mol { get; set; }
	}
}
