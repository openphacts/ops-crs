using System;
using System.Collections.Generic;

namespace RSC.Process
{
	public interface IJobManager
	{
		/// <summary>
		/// Check if job with Id exists
		/// </summary>
		/// <param name="id">Job id</param>
		/// <returns>True is job exists</returns>
		bool HasJob(Guid id);

		/// <summary>
		/// Register new job in the queue
		/// </summary>
		/// <param name="parameters">Job's parameters</param>
		/// <returns>New job's guid</returns>
		Guid NewJob(Job job);

		/// <summary>
		/// Get jobs by status
		/// </summary>
		/// <param name="status">Jobs status</param>
		/// <returns>Jobs GUIDs</returns>
		IEnumerable<Guid> GetJobsByStatus(JobStatus status, int start = 0, int count = 100);

		/// <summary>
		/// Fetch next new job to run. Change job's status from New to Processing.
		/// </summary>
		/// <returns>Job Id</returns>
		Guid? FetchJob();

		/// <summary>
		/// Get job
		/// </summary>
		/// <param name="id">Job guid</param>
		/// <returns>Job parameters</returns>
		Job GetJob(Guid id);

		/// <summary>
		/// Get jobs by list of IDs
		/// </summary>
		/// <param name="ids">List of jobs IDs</param>
		/// <returns>List of jobs</returns>
		IEnumerable<Job> GetJobs(IEnumerable<Guid> ids);

		/// <summary>
		/// Get job status
		/// </summary>
		/// <param name="id">Job id</param>
		/// <returns>Job status</returns>
		JobStatus GetJobStatus(Guid id);

		/// <summary>
		/// Change job status
		/// </summary>
		/// <param name="id">Job id</param>
		/// <param name="status">New job status</param>
		bool ChangeJobStatus(Guid id, JobStatus status);

		/// <summary>
		/// Change job
		/// </summary>
		/// <param name="id">Job guid</param>
		/// <param name="status">Updated job instance</param>
		bool UpdateJob(Guid id, Job job);

		/// <summary>
		/// Delete job by Id
		/// </summary>
		/// <param name="guid">Job Id</param>
		/// <returns>True if operation was successful</returns>
		bool DeleteJob(Guid id);

		/// <summary>
		/// Search jobs
		/// </summary>
		/// <param name="parameters">List of parameters to query</param>
		/// <returns>List of jobs GUIDs</returns>
		IEnumerable<Guid> SearchJobs(IEnumerable<RSC.Process.Job.Parameter> parameters, JobStatus status = JobStatus.All, int start = 0, int count = -1);

		/// <summary>
		/// Jobs statistics
		/// </summary>
		/// <param name="parameters">List of parameters to query</param>
		/// <returns>Jobs statistics object</returns>
		JobsStatistics JobsStatistics(IEnumerable<RSC.Process.Job.Parameter> parameters);
	}
}
