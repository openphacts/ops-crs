using System;
using System.Collections.Specialized;
using System.Web.Script.Serialization;
using RSC.Process;
using System.Collections.Generic;
using System.Linq;

namespace RSC.CVSP
{
	public static class CVSPJobExtensions
	{
		public static CVSPJob ToCVSPJob(this Job job)
		{
			if (job == null)
				return null;

			return new CVSPJob()
			{
				Id = job.Id,
				Status = job.Status,
				Created = job.Created,
				Started = job.Started,
				Finished = job.Finished,
				Error = job.Error,
				Parameters = job.Parameters
			};
		}
	}

	public static class CVSPJobManagerExtensions
	{
		public static void DeleteDepositionJobs(this IJobManager manager, Guid depositionId)
		{
			var guids = manager.SearchJobs(new List<RSC.Process.Job.Parameter>() { 
				new RSC.Process.Job.Parameter() { Name = "deposition", Value = depositionId.ToString() } 
			});

			foreach (var guid in guids.ToList())
				manager.DeleteJob(guid);
		}
	}

	public class CVSPJob : Job
	{
		public CVSPJob()
		{
		}

		public CVSPJob(StringDictionary parameters) : base(parameters)
		{
		}

		[ScriptIgnore]
		public string Command
		{
			get { return GetParameter("command"); }
			set { AddParameter("command", value); }
		}

		[ScriptIgnore]
		public Guid Deposition
		{
			get { return Guid.Parse(GetParameter("deposition")); }
			set { AddParameter("deposition", value.ToString()); }
		}

		[ScriptIgnore]
		public Guid Chunk
		{
			get
			{ 
				string guid = GetParameter("chunk");
				return string.IsNullOrEmpty(guid) ? Guid.Empty : Guid.Parse(guid);
			}

			set { AddParameter("chunk", value.ToString()); }
		}

		[ScriptIgnore]
		public Guid? Datasource
		{
			get
			{
				var guid = GetParameter("datasource");
				return string.IsNullOrEmpty(guid) ? null : (Guid?)Guid.Parse(guid);
			}

			set { AddParameter("datasource", value.ToString()); }
		}

		[ScriptIgnore]
		public DataDomain DataDomain
		{
			get
			{
				DataDomain dataDomain;
				return Enum.TryParse(GetParameter("datadomain"), out dataDomain) ? dataDomain : DataDomain.Unidentified;
			}
			set { AddParameter("datadomain", value.ToString()); }
		}

		[ScriptIgnore]
		public bool DoValidate
		{
			get
			{
				bool doValidate = true;

				if (HasParameter("doValidate"))
					Boolean.TryParse(GetParameter("doValidate"), out doValidate);

				return doValidate;
			}
			set { AddParameter("dovalidate", value == null ? null : value.ToString()); }
		}

		[ScriptIgnore]
		public bool DoStandardize
		{
			get 
			{
				bool doStandardize = true;

				if (HasParameter("doStandardize"))
					Boolean.TryParse(GetParameter("doStandardize"), out doStandardize);

				return doStandardize;
			}
			set { AddParameter("dostandardize", value.ToString()); }
		}

		[ScriptIgnore]
		public bool DelayUpload2db
		{
			get
			{
				bool delayUpload2db = false;

				if (HasParameter("delayUpload2db"))
					Boolean.TryParse(GetParameter("delayUpload2db"), out delayUpload2db);

				return delayUpload2db;
			}
			set { AddParameter("delayUpload2db", value.ToString()); }
		}

		[ScriptIgnore]
		public bool ParentsGeneration
		{
			get
			{
				bool parentsGeneration = true;

				if (HasParameter("parentsGeneration"))
					Boolean.TryParse(GetParameter("parentsGeneration"), out parentsGeneration);

				return parentsGeneration;
			}
			set { AddParameter("parentsGeneration", value.ToString()); }
		}

		[ScriptIgnore]
		public bool PropertiesCalculation
		{
			get
			{
				bool propertiesCalculation = true;

				if (HasParameter("propertiesCalculation"))
					Boolean.TryParse(GetParameter("propertiesCalculation"), out propertiesCalculation);

				return propertiesCalculation;
			}
			set { AddParameter("propertiesCalculation", value.ToString()); }
		}

		[ScriptIgnore]
		public bool ChemSpiderProperties
		{
			get
			{
				bool chemSpiderProperties = true;

				if (HasParameter("chemSpiderProperties"))
					Boolean.TryParse(GetParameter("chemSpiderProperties"), out chemSpiderProperties);

				return chemSpiderProperties;
			}
			set { AddParameter("chemSpiderProperties", value.ToString()); }
		}

		[ScriptIgnore]
		public bool ChemSpiderSynonyms
		{
			get
			{
				bool chemSpiderSynonyms = true;

				if (HasParameter("chemSpiderSynonyms"))
					Boolean.TryParse(GetParameter("chemSpiderSynonyms"), out chemSpiderSynonyms);

				return chemSpiderSynonyms;
			}
			set { AddParameter("chemSpiderSynonyms", value.ToString()); }
		}

		[ScriptIgnore]
		public Guid User
		{
			get
			{
				Guid guid = Guid.Empty;

				Guid.TryParse(GetParameter("user"), out guid);

				return guid;
			}
			set { AddParameter("user", value.ToString()); }
		}

		[ScriptIgnore]
		public string File
		{
			get { return GetParameter("file"); }
			set { AddParameter("file", value); }
		}

        [ScriptIgnore]
        public string Query
        {
            get { return GetParameter("query"); }
            set { AddParameter("query", value); }
        }


        public override string ToString()
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(this);
		}
	}
}
