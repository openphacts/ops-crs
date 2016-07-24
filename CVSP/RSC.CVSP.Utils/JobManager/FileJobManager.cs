using RSC.Process;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace RSC.CVSP
{
	public class FileJobManager : IJobManager
	{
		private Object thisLock = new Object();

		public string Root { get; private set; }

		public FileJobManager()
		{
			Root = ConfigurationManager.AppSettings["job_manager_root"];

			if (string.IsNullOrEmpty(Root))
				throw new ArgumentNullException("Job manager root is not specified");

			if (!Directory.Exists(Root))
				Directory.CreateDirectory(Root);
		}

		/// <summary>
		/// Check if job with Id exists
		/// </summary>
		/// <param name="id">Job id</param>
		/// <returns>True is job exists</returns>
		public bool HasJob(Guid id)
		{
			var files = Directory.GetFiles(Root, string.Format("{0}.json", id), SearchOption.AllDirectories);

			return files.Length == 1;
		}

		/// <summary>
		/// Register new job in the queue
		/// </summary>
		/// <param name="job">Job's description</param>
		/// <returns>New job's guid</returns>
		public Guid NewJob(Job job)
		{
			job.Id = Guid.NewGuid();
			job.Status = JobStatus.New;
			job.Created = DateTime.Now;

			var json = new JavaScriptSerializer().Serialize(job);
			File.WriteAllText(Path.Combine(Root, string.Format("{0}.json", job.Id)), json);

			return job.Id;
		}

		public IEnumerable<Guid> GetJobsByStatus(JobStatus status, int start = 0, int count = 100)
		{
			var files = Directory.GetFiles(Root, "*.json");

			var serializer = new JavaScriptSerializer();

			var jobs = files.Select(f => serializer.Deserialize<CVSPJob>(File.ReadAllText(f))).ToList();

			return jobs.Where(j => j.Status == status).Select(j => j.Id).ToList();
		}

		public Guid? FetchJob()
		{
			throw new NotImplementedException();
		}

		public Job GetJob(Guid id)
		{
			var files = Directory.GetFiles(Root, string.Format("{0}.json", id));

			if (files.Length == 0)
				return null;

			var json = File.ReadAllText(files[0]);

			return new JavaScriptSerializer().Deserialize<Job>(json);
		}

		public IEnumerable<Job> GetJobs(IEnumerable<Guid> ids)
		{
			var jobs = new List<Job>();

			foreach (var id in ids)
				jobs.Add(GetJob(id));

			return jobs;
		}

		/// <summary>
		/// Get job status
		/// </summary>
		/// <param name="id">Job id</param>
		/// <returns>Job status</returns>
		public JobStatus GetJobStatus(Guid id)
		{
			var job = GetJob(id);
			if (job != null)
				return job.Status;

			return JobStatus.Unknown;
		}

		/// <summary>
		/// Change job status
		/// </summary>
		/// <param name="id">Job id</param>
		/// <param name="status">New job status</param>
		public bool ChangeJobStatus(Guid id, JobStatus status)
		{
			var job = GetJob(id);
			if (job != null)
			{
				job.Status = status;

				if (status == JobStatus.Processing)
					job.Started = DateTime.Now;

				if (status == JobStatus.Processed)
					job.Finished = DateTime.Now;

				return UpdateJob(id, job);
			}

			return false;
		}

		/// <summary>
		/// Change job
		/// </summary>
		/// <param name="id">Job guid</param>
		/// <param name="status">Updated job instance</param>
		public bool UpdateJob(Guid id, Job job)
		{
			lock(thisLock)
			{
				var files = Directory.GetFiles(Root, string.Format("{0}.json", id));

				if (files.Length > 0)
				{
					var json = File.ReadAllText(files[0]);
					var jb = new JavaScriptSerializer().Deserialize<CVSPJob>(json);

					jb.Status = job.Status;
					jb.Error = job.Error;

					json = new JavaScriptSerializer().Serialize(jb);
					File.WriteAllText(files[0], json);

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Delete job by Id
		/// </summary>
		/// <param name="guid">Job Id</param>
		/// <returns>True if operation was successful</returns>
		public bool DeleteJob(Guid id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Search jobs
		/// </summary>
		/// <param name="parameters">List of parameters to query</param>
		/// <returns>List of jobs GUIDs</returns>
		public IEnumerable<Guid> SearchJobs(IEnumerable<RSC.Process.Job.Parameter> parameters, JobStatus status = JobStatus.All, int start = 0, int count = -1)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Jobs statistics
		/// </summary>
		/// <param name="parameters">List of parameters to query</param>
		/// <returns>Jobs statistics object</returns>
		public JobsStatistics JobsStatistics(IEnumerable<RSC.Process.Job.Parameter> parameters)
		{
			throw new NotImplementedException();
		}
	}
}
