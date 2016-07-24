using CVSPWeb.Models;
using RSC.CVSP;
using RSC.CVSP.Search;
using RSC.Process;
using RSC.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CVSPWeb.WebAPI
{
	[Authorize(Roles = "Administrator")]
	[EnableCors("*", "*", "*")]
	public class JobsController : ApiController
	{
		private readonly IJobManager jobManager;

		public JobsController(IJobManager jobManager)
		{
			if (jobManager == null)
				throw new ArgumentNullException("jobManager");

			this.jobManager = jobManager;
		}

		// GET api/jobs/851107d4-ec5d-4cdf-903b-8ed94aa4132d
		/// <summary>
		/// Get job by GUID
		/// </summary>
		/// <param name="guid">Job GUID</param>
		/// <returns>JOb object</returns>
		[Route("api/jobs/{guid}")]
		public IHttpActionResult GetJob(Guid guid)
		{
			var res = jobManager.GetJob(guid);

			return Ok(res);
		}

		// GET api/jobs/list?id={1}&id={2}
		/// <summary>
		/// Returns list of jobs by list of GUIDs
		/// </summary>
		/// <param name="guid">List of job GUIDs</param>
		/// <returns>List of jobs</returns>
		[Route("api/jobs/list")]
		[HttpPost]
		public IHttpActionResult GetJobsList([FromBody] Guid[] id)
		{
			var res = jobManager.GetJobs(id);

			return Ok(res);
		}

		// GET api/jobs/851107d4-ec5d-4cdf-903b-8ed94aa4132d/restart
		/// <summary>
		/// Restart job by GUID
		/// </summary>
		/// <param name="guid">Job GUID</param>
		/// <returns>True if job been successfully restarted</returns>
		[Route("api/jobs/{guid}/restart")]
		[HttpPut]
		public IHttpActionResult RestartJob(Guid guid)
		{
			var job = jobManager.GetJob(guid);

			job.Started = null;
			job.Finished = null;
			job.Error = null;
			job.Status = JobStatus.New;

			var res = jobManager.UpdateJob(guid, job);

			return Ok(res);
		}

		// GET api/depositions/{guid}/jobs
		/// <summary>
		/// Returns list of deposition's jobs GUIDs
		/// </summary>
		/// <param name="guid">Deposition GUID</param>
		/// <param name="start">Index where to start returning GUIDs</param>
		/// <param name="count">Number of returned GUIDs</param>
		/// <returns>List of deposition's jobs GUIDs</returns>
		[Route("api/depositions/{guid}/jobs")]
		public IHttpActionResult GetDepositionJobs(Guid guid, int start = 0, int count = 10)
		{
			var guids = jobManager.SearchJobs(new List<RSC.Process.Job.Parameter>() {
				new RSC.Process.Job.Parameter() {
					Name = "deposition",
					Value = guid.ToString()
				}
			}, JobStatus.All, start, count);

			return Ok(guids);
		}

		// GET api/depositions/{guid}/jobscount
		/// <summary>
		/// Returns jobs number in deposition
		/// </summary>
		/// <param name="guid">Internal deposition GUID</param>
		/// <returns>Jobs number</returns>
		[Route("api/depositions/{guid}/jobscount")]
		public IHttpActionResult GetDepositionJobsCount(Guid guid)
		{
			var guids = jobManager.SearchJobs(new List<RSC.Process.Job.Parameter>() {
				new RSC.Process.Job.Parameter() {
					Name = "deposition",
					Value = guid.ToString()
				}
			});

			return Ok(guids.Count());
		}

		/// <summary>
		/// Run jobs search
		/// </summary>
		/// <param name="search"></param>
		/// <returns>Request ID</returns>
		[Route("api/searches/jobs")]
		[HttpPost]
		public IHttpActionResult JobsSearch([FromBody] CVSPJobsSearchModel search)
		{
			var rid = SearchUtility.RunSearchAsync(typeof(JobsSearch), search.searchOptions, search.commonOptions, search.scopeOptions, search.resultOptions);

			return Ok(rid);
		}

		// GET api/depositions/851107d4-ec5d-4cdf-903b-8ed94aa4132d/jobstats
		/// <summary>
		/// Returns jobs' statictic information by deposition GUID
		/// </summary>
		/// <param name="guid">Internal deposition GUID</param>
		/// <returns>Jobs' statistic object</returns>
		[Route("api/depositions/{guid}/jobstats")]
		public IHttpActionResult GetDepositionJobsStats(Guid guid)
		{
			var stats = jobManager.JobsStatistics(new List<RSC.Process.Job.Parameter>() {
				new RSC.Process.Job.Parameter() {
					Name = "deposition",
					Value = guid.ToString()
				}
			});

			return Ok(stats);
		}
	}
}