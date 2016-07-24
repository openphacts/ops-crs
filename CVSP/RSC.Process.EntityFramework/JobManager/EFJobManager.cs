using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Process.EntityFramework
{
	public class EFJobManager : IJobManager
	{
		private JobManagerContext db = new JobManagerContext();

		private JobManagerContext GetReadContext()
		{
			var db = new JobManagerContext();
			return db;
		}

		private JobManagerContext GetUpdateContext()
		{
			var db = new JobManagerContext();
			return db;
		}

		/// <summary>
		/// Check if job with Id exists
		/// </summary>
		/// <param name="id">Job id</param>
		/// <returns>True is job exists</returns>
		public bool HasJob(Guid id)
		{
			return db.Jobs.Any(j => j.ExternalId == id);
		}

		/// <summary>
		/// Register new job in the queue
		/// </summary>
		/// <param name="parameters">Job's parameters</param>
		/// <returns>New job's guid</returns>
		public Guid NewJob(RSC.Process.Job job)
		{
			job.Id = Guid.NewGuid();
			//job.Status = JobStatus.New;
			job.Created = DateTime.Now;

			db.Jobs.Add(new RSC.Process.JobManager.EntityFramework.Job()
			{
				ExternalId = job.Id,
				Created = job.Created,
				Status = job.Status,
				Error = job.Error,
				Parameters = job.Parameters.Select(p => new RSC.Process.JobManager.EntityFramework.Parameter()
				{
					Name = p.Name,
					Value = p.Value,
					//JobId = job.Id
				}).ToList()
			});

			db.SaveChanges();

			return job.Id;
		}

		/// <summary>
		/// Get jobs by status
		/// </summary>
		/// <param name="status">Jobs status</param>
		/// <returns>Jobs GUIDs</returns>
		public IEnumerable<Guid> GetJobsByStatus(JobStatus status, int start = 0, int count = 100)
		{
			if (count == 0)
				return new List<Guid>();

			var query = db.Jobs.Where(j => j.Status == status).Select(j => j.ExternalId);

			if (start > 0)
				query = query.Skip(start);

			if (count > 0)
				query = query.Take(count);

			return query;
		}

		/// <summary>
		/// Fetch next new job to run. Change job's status from New to Processing.
		/// </summary>
		/// <returns>Job Id</returns>
		public Guid? FetchJob()
		{
			using (var db = GetUpdateContext())
			{
				var sql = @"DECLARE @UpdatedId table (Id uniqueidentifier)

							UPDATE TOP (1) Jobs
							SET [status] = 2
							OUTPUT inserted.ExternalId
							INTO @UpdatedId
							WHERE [status] = 1

							select Id from @UpdatedId";

				var jobId = db.Database.SqlQuery<Guid?>(sql).FirstOrDefault();

				return jobId;
			}
		}

		/// <summary>
		/// Get job
		/// </summary>
		/// <param name="id">Job guid</param>
		/// <returns>Job parameters</returns>
		public RSC.Process.Job GetJob(Guid id)
		{
			return GetJobs(new List<Guid>() { id }).FirstOrDefault();
		}

		/// <summary>
		/// Get jobs by list of IDs
		/// </summary>
		/// <param name="ids">List of jobs IDs</param>
		/// <returns>List of jobs</returns>
		public IEnumerable<RSC.Process.Job> GetJobs(IEnumerable<Guid> ids)
		{
			return db.Jobs.Where(j => ids.Contains(j.ExternalId)).OrderBy(j => j.Created).Select(j => new RSC.Process.Job()
			{
				Id = j.ExternalId,
				Created = j.Created,
				Started = j.Started,
				Finished = j.Finished,
				Status = j.Status,
				Error = j.Error,
				Parameters = j.Parameters.Select(p => new RSC.Process.Job.Parameter()
				{
					Name = p.Name,
					Value = p.Value
				}).ToList(),
				Watches = j.Watches.Select(w => new RSC.Process.Job.Watch()
				{
					Name = w.Name,
					Begin = w.Begin,
					End = w.End
				}).ToList()
			});
		}

		/// <summary>
		/// Get job status
		/// </summary>
		/// <param name="id">Job id</param>
		/// <returns>Job status</returns>
		public JobStatus GetJobStatus(Guid id)
		{
			var job = GetJob(id);

			if (job == null)
				return JobStatus.Unknown;

			return job.Status;
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
				{
					job.Started = DateTime.Now;
					job.Finished = null;
					job.Error = null;
				}

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
		public bool UpdateJob(Guid id, RSC.Process.Job job)
		{
			var efJob = db.Jobs.FirstOrDefault(j => j.ExternalId == id);
			if (efJob != null)
			{
				efJob.Status = job.Status;
				efJob.Started = job.Started;
				efJob.Finished = job.Finished;
				efJob.Error = job.Error;

				db.Parameters.RemoveRange(efJob.Parameters);

				db.Parameters.AddRange(job.Parameters.Select(p => new RSC.Process.JobManager.EntityFramework.Parameter()
				{
					Name = p.Name,
					Value = p.Value,
					Job = efJob
				}));

				db.Watches.RemoveRange(efJob.Watches);

				db.Watches.AddRange(job.Watches.Select(w => new RSC.Process.JobManager.EntityFramework.Watch()
				{
					Name = w.Name,
					Begin = w.Begin,
					End = w.End,
					Job = efJob
				}));

				return db.SaveChanges() > 0;
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
			var job = db.Jobs.Where(j => j.ExternalId == id).FirstOrDefault();

			if (job == null)
				return false;

			db.Parameters.RemoveRange(job.Parameters);
			db.Watches.RemoveRange(job.Watches);
			db.Jobs.Remove(job);

			return db.SaveChanges() > 0;
		}

		/// <summary>
		/// Search jobs
		/// </summary>
		/// <param name="parameters">List of parameters to query</param>
		/// <returns>List of jobs GUIDs</returns>
		public IEnumerable<Guid> SearchJobs(IEnumerable<RSC.Process.Job.Parameter> parameters, JobStatus status = JobStatus.All, int start = 0, int count = -1)
		{
			IQueryable<int> query = null;

			foreach (var param in parameters)
			{
				if (query == null)
					query = db.Parameters.AsNoTracking().Where(p => p.Name == param.Name && p.Value == param.Value).Select(p => p.JobId);
				else
					query = db.Parameters.AsNoTracking().Where(p => p.Name == param.Name && p.Value == param.Value).Select(p => p.JobId).Intersect(query);
			}

			if (status != JobStatus.All)
			{
				var listOfIds = query.ToList();
				query = db.Jobs.Where(j => j.Status == status && listOfIds.Contains(j.Id)).Select(j => j.Id);
			}

			var result = db.Jobs.Where(j => query.ToList().Contains(j.Id)).OrderBy(j => j.Created).Select(j => j.ExternalId);

			if (start > 0)
				result = result.Skip(start);

			if (count > 0)
				result = result.Take(count);

			return result;
		}

		/// <summary>
		/// Jobs statistics
		/// </summary>
		/// <param name="parameters">List of parameters to query</param>
		/// <returns>Jobs statistics object</returns>
		public JobsStatistics JobsStatistics(IEnumerable<RSC.Process.Job.Parameter> parameters)
		{
			using (var db = new JobManagerContext())
			{
				db.Configuration.AutoDetectChangesEnabled = false;

				IEnumerable<int> ids = null;

				foreach (var param in parameters)
				{
					if (ids == null)
						ids = db.Parameters.AsNoTracking().Where(p => p.Name == param.Name && p.Value == param.Value).Select(p => p.JobId).ToList();
					else
						ids = db.Parameters.AsNoTracking().Where(p => p.Name == param.Name && p.Value == param.Value).Select(p => p.JobId).ToList().Intersect(ids).ToList();
				}

				var stats = new JobsStatistics();

				stats.Total = db.Jobs.AsNoTracking().Where(j => ids.Contains(j.Id)).Count();

				var statuses = db.Jobs.AsNoTracking().Where(j => ids.Contains(j.Id)).GroupBy(c => c.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToList();

				stats.Commands = db.Jobs.AsNoTracking().Where(j => ids.Contains(j.Id)).SelectMany(j => j.Parameters).Where(p => p.Name == "command").Select(p => p.Value).Distinct().ToList();

				var processingStarted = db.Jobs.AsNoTracking().Where(j => ids.Contains(j.Id)).Min(j => j.Started);
				var processingFinished = db.Jobs.AsNoTracking().Where(j => ids.Contains(j.Id)).Max(j => j.Finished);

				stats.ProcessingTime = processingFinished - processingStarted;

				foreach (var s in statuses)
				{
					if (s.Status == JobStatus.New)
						stats.New = s.Count;
					else if (s.Status == JobStatus.Processing)
						stats.Processing = s.Count;
					else if (s.Status == JobStatus.Processed)
						stats.Processed = s.Count;
					else if (s.Status == JobStatus.Failed)
						stats.Failed = s.Count;
					else if (s.Status == JobStatus.Delayed)
						stats.Delayed = s.Count;
				}

				stats.Stopwatches = db.Watches.AsNoTracking()
					.Where(w => ids.Contains(w.JobId))
					.GroupBy(w => w.Name)
					.ToDictionary(g => g.Key, g => Convert.ToInt64(g.Sum(w => (w.End - w.Begin).TotalMilliseconds)));

				return stats;
			}
		}
	}
}
