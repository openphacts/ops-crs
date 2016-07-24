using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace RSC.Process
{
	public enum JobStatus
	{
		New = 1,
		Processing = 2,
		Processed = 4,
		Failed = 8,
		Unknown = 16,
		Delayed = 32,
		All = 63
	}

	public class Job
	{
		public class Watch
		{
			public string Name { get; set; }
			public DateTime Begin { get; set; }
			public DateTime End { get; set; }
		}

		public class Parameter
		{
			public string Name { get; set; }
			public string Value { get; set; }
		}

		public Job()
		{
			Status = JobStatus.New;
		}

		public Job(StringDictionary parameters)
		{
			Status = JobStatus.New;

			SetParameters(parameters);
		}

		public Guid Id { get; set; }

		public DateTime Created { get; set; }

		public DateTime? Started { get; set; }

		public DateTime? Finished { get; set; }

		public JobStatus Status { get; set; }

		public string Error { get; set; }

		public List<Parameter> Parameters = new List<Parameter>();

		public List<Watch> Watches = new List<Watch>();

		public void SetParameters(StringDictionary parameters)
		{
			foreach (string key in parameters.Keys)
				AddParameter(key, parameters[key]);
		}

		public void AddParameter(string name, string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				Parameters = Parameters.Where(p => p.Name != name).ToList();
			}
			else
			{
				if (Parameters.Any(p => p.Name.Equals(name)))
				{
					Parameters.Single(p => p.Name.Equals(name)).Value = value;
				}
				else
				{
					Parameters.Add(new Parameter()
					{
						Name = name.ToLower(),
						Value = value
					});
				}
			}
		}

		public bool HasParameter(string name)
		{
			return !string.IsNullOrEmpty(GetParameter(name));
		}

		public string GetParameter(string name)
		{
			var query = Parameters.Where(p => p.Name.Equals(name.ToLower()));
			if (query.Any())
				return query.First().Value;

			return string.Empty;
		}

		public bool GetBool(string name, bool def = false)
		{
			if (HasParameter(name))
				return Convert.ToBoolean(GetParameter(name));

			return def;
		}

		public int GetInt(string name, int def = 0)
		{
			if (HasParameter(name))
				return Convert.ToInt32(GetParameter(name));

			return def;
		}

		public void StartWatch(string name)
		{
			Watches.RemoveAll(w => w.Name.Equals(name));

			Watches.Add(new Watch() { Name = name, Begin = DateTime.Now, End = DateTime.Now });
		}

		public void StopWatch(string name)
		{
			if (!Watches.Any(w => w.Name.Equals(name)))
				throw new InvalidOperationException(name);

			Watches.FirstOrDefault(w => w.Name.Equals(name)).End = DateTime.Now;
		}
	}
}
