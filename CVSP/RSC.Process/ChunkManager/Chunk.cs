using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Linq;

namespace RSC.Process
{
	[DataContract]
	public enum ChunkStatus
	{
		[EnumMember]
		Created = 1,
		[EnumMember]
		Processing = 2,
		[EnumMember]
		Processed = 4,
		[EnumMember]
		Failed = 8,
		[EnumMember]
		Unknown = 16,
		[EnumMember]
		All = 31
	}

	public class Chunk
	{
		public class Parameter
		{
			public string Name { get; set; }
			public string Value { get; set; }
		}

		public Guid Id { get; set; }

		public ChunkStatus Status { get; set; }

		public int NumberOfRecords { get; set; }

		public List<Parameter> Parameters = new List<Parameter>();

		public void AddParameter(string name, string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				Parameters = Parameters.Where(p => p.Name != name).ToList();
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

		public string GetParameter(string name)
		{
			var query = Parameters.Where(p => p.Name.Equals(name.ToLower()));
			if (query.Any())
				return query.First().Value;

			return string.Empty;
		}
	}
}
