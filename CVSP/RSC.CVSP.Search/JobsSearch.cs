using Microsoft.Practices.ServiceLocation;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using RSC.Process;

namespace RSC.CVSP.Search
{
	public class JobsSearch : RSC.Search.Search
	{
		private IJobManager jobManager;

		public JobsSearch()
		{
			jobManager = ServiceLocator.Current.GetService(typeof(IJobManager)) as IJobManager;

			if (jobManager == null)
				throw new NullReferenceException("Jobs search is not configured properly");
		}

		public override IEnumerable<object> DoSearch()
		{
			var parameters = new List<Job.Parameter>();

			var options = Options as CVSPJobsSearchOptions;

			if (options.Deposition != Guid.Empty)
				parameters.Add(new Job.Parameter() { Name = "deposition", Value = options.Deposition.ToString() });

			if(!string.IsNullOrEmpty(options.Command))
				parameters.Add(new Job.Parameter() { Name = "command", Value = options.Command });

			JobStatus status = JobStatus.All;
			if(!string.IsNullOrEmpty(options.Status))
				Enum.TryParse(options.Status, true, out status);

			var res = jobManager.SearchJobs(parameters, status).ToList();

			return res.Cast<object>();
		}
	}
}
