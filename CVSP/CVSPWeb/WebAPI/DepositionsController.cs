using ChemSpider.Profile.Data.Models;
using RSC.CVSP;
using RSC.Process;
using RSC.Web.Http.Filters;
using System;
using System.Linq;
using System.Security.Authentication;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.IO;
using System.Collections.Generic;

namespace CVSPWeb.WebAPI
{
	[EnableCors("*", "*", "*")]
	public class DepositionsController : ApiController
	{
		private readonly ICVSPStore repository;
		private readonly IFileStorage fileStorage;
		private readonly IJobManager jobManager;
		private readonly IStatistics stats;
		private readonly IChunkManager chunkManager;

		public DepositionsController(ICVSPStore repository, IFileStorage fileStorage, IJobManager jobManager, IStatistics stats, IChunkManager chunkManager)
		{
			if (repository == null)
				throw new ArgumentNullException("repository");

			if (fileStorage == null)
				throw new ArgumentNullException("fileStorage");

			if (jobManager == null)
				throw new ArgumentNullException("jobManager");

			if (stats == null)
				throw new ArgumentNullException("stats");

			if (chunkManager == null)
				throw new ArgumentNullException("chunkManager");

			this.repository = repository;
			this.fileStorage = fileStorage;
			this.jobManager = jobManager;
			this.stats = stats;
			this.chunkManager = chunkManager;
		}

		// GET api/depositions/851107d4-ec5d-4cdf-903b-8ed94aa4132d
		/// <summary>
		/// Returns deposition by GUID
		/// </summary>
		/// <param name="guid">Internal deposition GUID</param>
		/// <returns>Deposition object</returns>
		[Route("api/depositions/{guid}")]
		public IHttpActionResult GetDeposition(Guid guid)
		{
			var res = repository.GetDeposition(guid);

			if (!res.IsPublic && !User.Identity.IsAuthenticated)
				throw new AuthenticationException("Not authenticated");

			return Ok(res);
		}

		// GET api/depositions/851107d4-ec5d-4cdf-903b-8ed94aa4132d/fields
		/// <summary>
		/// Returns deposition's fields list
		/// </summary>
		/// <param name="guid">Internal deposition GUID</param>
		/// <returns>List of fields</returns>
		[Route("api/depositions/{guid}/fields")]
		public IHttpActionResult GetDepositionFields(Guid guid)
		{
			var res = repository.GetDepositionFields(guid);

			return Ok(res);
		}

		// GET api/depositions/851107d4-ec5d-4cdf-903b-8ed94aa4132d/stats
		/// <summary>
		/// Returns deposition's statictic information by GUID
		/// </summary>
		/// <param name="guid">Internal deposition GUID</param>
		/// <returns>Deposition's statistics object</returns>
		[Route("api/depositions/{guid}/stats")]
		public IHttpActionResult GetDepositionStats(Guid guid)
		{
			var res = stats.GetDepositionStats(guid);

			return Ok(res);
		}

		[Route("api/depositions")]
		[HttpPost]
		[RSCExceptionFilter]
		[Authorize]
		public IHttpActionResult NewDeposition()
		{
			if (!User.Identity.IsAuthenticated)
				throw new AuthenticationException("Not authenticated");

			if (!HttpContext.Current.Request.Files.AllKeys.Any())
				throw new NoDepositionFileException("There is no any deposition file");

			if (HttpContext.Current.Request.Files.AllKeys.Count() > 1)
				throw new TooManyDepositionFilesException("There are too may deposition files. We currently support one file per deposition");

			var profile = ChemSpiderProfile.GetUserProfile(User.Identity.Name);

			var httpPostedFile = HttpContext.Current.Request.Files["deposition-file[]"];

			if (httpPostedFile == null)
				throw new NoDepositionFileException("Deposition file not found");

			var parameters = new List<ProcessingParameter>();
			var datasource = Guid.Empty;
			bool isPublic = false;

			foreach (var name in HttpContext.Current.Request.Form.AllKeys)
			{
				if (name.Equals("Datasource", StringComparison.CurrentCulture))
				{
					Guid datasourceId = Guid.Empty;
					if (Guid.TryParse(HttpContext.Current.Request.Form[name], out datasourceId))
						datasource = datasourceId;
				}
				else if (name.Equals("Public", StringComparison.CurrentCulture))
				{
					isPublic = Convert.ToBoolean(HttpContext.Current.Request.Form[name]);
				}
				else
				{
					parameters.Add(new ProcessingParameter() {
						Name = name,
						Value = HttpContext.Current.Request.Form[name]
					});
				}
			}

			if (datasource == Guid.Empty)
				throw new NoDatasourceException("Datasource is not assigned");

			var deposition = new Deposition()
			{
				UserId = profile.UserKey,
				DatasourceId = datasource,
				IsPublic = isPublic,
				Status = DepositionStatus.Submitting,
				Parameters = parameters,
				DepositionFiles = new List<DepositionFile>() { new DepositionFile() { Name = httpPostedFile.FileName } }
			};

			deposition.Id = repository.CreateDeposition(deposition);

			var path = fileStorage.UploadFile(deposition.Id, Path.GetFileName(httpPostedFile.FileName), httpPostedFile.InputStream);

			repository.UpdateDepositionStatus(deposition.Id, DepositionStatus.Submitted);

			jobManager.NewJob(new CVSPJob()
			{
				Command = "prepare",
				Deposition = deposition.Id,
				Datasource = datasource,
				DataDomain = DataDomain.Substances
			});

			return Ok(deposition.Id);
		}

		[Route("api/depositions/{guid}")]
		[HttpDelete]
		[RSCExceptionFilter]
		public IHttpActionResult DeleteDeposition(Guid guid)
		{
			if (!User.Identity.IsAuthenticated)
				throw new AuthenticationException("Not authenticated");

			repository.UpdateDepositionStatus(guid, DepositionStatus.Deleting);

			jobManager.NewJob(new CVSPJob()
			{
				Command = "delete",
				Deposition = guid
			});

			return Ok(true);
		}

		[Route("api/depositions/{guid}/gcn")]
		[HttpDelete]
		[RSCExceptionFilter]
		public IHttpActionResult DeleteDepositionFromGCN(Guid guid)
		{
			if (!User.Identity.IsAuthenticated)
				throw new AuthenticationException("Not authenticated");

			repository.UpdateDepositionStatus(guid, DepositionStatus.DeletingFromGCN);

			jobManager.NewJob(new CVSPJob()
			{
				Command = "delete_from_gcn",
				Deposition = guid
			});

			return Ok(true);
		}

		[Route("api/depositions/{guid}/deposit2gcn")]
		[HttpPut]
		[RSCExceptionFilter]
		public IHttpActionResult Deposit2GCN(Guid guid)
		{
			if (!User.Identity.IsAuthenticated)
				throw new AuthenticationException("Not authenticated");

			var requiredAnnotations = repository.GetAllAnnotations().Where(a => a.IsRequired).Select(a => a.Name).ToList();
            var depositionAnnotations = repository.GetDepositionAnnotations(guid).Where(f => f.Annotaition != null && requiredAnnotations.Contains(f.Annotaition.Name));

			if(requiredAnnotations.Count() != depositionAnnotations.Count())
				throw new AnnotationRequiredException("Fields annotation required");

			repository.UpdateDepositionStatus(guid, DepositionStatus.Depositing2GCN);

			jobManager.NewJob(new CVSPJob()
			{
				Command = "upload2gcn",
				Deposition = guid
			});

			return Ok(true);
		}

		[Route("api/depositions/{guid}/status")]
		[HttpGet]
		[RSCExceptionFilter]
		public IHttpActionResult GetDepositionStatus(Guid guid)
		{
			var deposition = repository.GetDeposition(guid);

			if (deposition == null)
				return NotFound();

			return Ok(deposition.Status);
		}

		// GET api/depositions/{guid}/files
		/// <summary>
		/// Returns list of deposition's files
		/// </summary>
		/// <param name="guid">Internal deposition GUID</param>
		/// <returns>List of deposition's files</returns>
		[Route("api/depositions/{guid}/files")]
		public IHttpActionResult GetRecords(Guid guid)
		{
			var files = repository.GetDepositionFiles(guid);

			return Ok(files);
		}

		// GET api/depositions/{guid}/records
		/// <summary>
		/// Returns list of deposition's records' GUIDs
		/// </summary>
		/// <param name="guid">Internal deposition GUID</param>
		/// <param name="start">Index where to start returning GUIDs</param>
		/// <param name="count">Number of returned GUIDs</param>
		/// <returns>List of deposition's records's GUIDs</returns>
		[Route("api/depositions/{guid}/records")]
		public IHttpActionResult GetRecords(Guid guid, int start = 0, int count = 10)
		{
			var ids = repository.GetDepositionRecords(guid, start, count);

			return Ok(ids);
		}

		// GET api/depositions/{guid}/recordscount
		/// <summary>
		/// Returns records number in deposition
		/// </summary>
		/// <param name="guid">Internal deposition GUID</param>
		/// <returns>Records number</returns>
		[Route("api/depositions/{guid}/recordscount")]
		public IHttpActionResult GetRecordsCount(Guid guid)
		{
			var count = repository.GetDepositionRecordsCount(guid);

			return Ok(count);
		}

		// GET api/depositions/guids
		/// <summary>
		/// Returns list of depositions' GUIDs
		/// </summary>
		/// <param name="start">Index where to start returning GUIDs</param>
		/// <param name="count">Number of returned GUIDs</param>
		/// <returns>List of depositions's GUIDs</returns>
		[Route("api/depositions/guids")]
		public IHttpActionResult GetDepositionsGUIDs(int start = 0, int count = 10)
		{
			var ids = repository.GetDepositions(start, count);

			return Ok(ids);
		}

		// GET api/depositions/list?id={1}&id={2}
		/// <summary>
		/// Returns list of depositions by array of GUIDs
		/// </summary>
		/// <param name="guid">List of internal GUIDs</param>
		/// <returns>List of depositions</returns>
		[Route("api/depositions/list")]
		[HttpGet]
		public IHttpActionResult GetDepositionsList([FromUri] Guid[] id)
		{
			var res = repository.GetDepositions(id);

			return Ok(res);
		}

		[Route("api/depositions/{guid}/chunks/stats")]
		public IHttpActionResult GetDepositionChunksStats(Guid guid)
		{
			var OriginalStats = chunkManager.ChunksStatistics(new List<RSC.Process.Chunk.Parameter>()
			{
				new RSC.Process.Chunk.Parameter() { Name = "deposition", Value = guid.ToString() },
				new RSC.Process.Chunk.Parameter() { Name = "chunktype", Value = "Original" }
			});

			var ProcessedStats = chunkManager.ChunksStatistics(new List<RSC.Process.Chunk.Parameter>()
			{
				new RSC.Process.Chunk.Parameter() { Name = "deposition", Value = guid.ToString() },
				new RSC.Process.Chunk.Parameter() { Name = "chunktype", Value = "Processed" }
			});

			return Ok(new
			{
				Original = OriginalStats,
				Processed = ProcessedStats
			});
		}
	}
}
