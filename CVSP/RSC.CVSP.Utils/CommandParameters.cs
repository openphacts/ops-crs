using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSC.CVSP;
using System.IO;

namespace RSC.CVSP.Utils
{
	public class CommandParameters
	{
		public IDictionary<Resources.CommandParameter, string> Dictionary = new Dictionary<Resources.CommandParameter, string>();

		public CommandParameters()
		{
		}

		public CommandParameters(string path)
		{
			if(!String.IsNullOrEmpty(path) && File.Exists(path))
			{
				string[] lines = File.ReadAllLines(path);
				Dictionary = ParseCommandParameters(lines);
			}
		}
		
		public Resources.ProcessingType ProcessingStep
		{
			get
			{
				Resources.ProcessingType processingType;
				return Enum.TryParse(Dictionary[Resources.CommandParameter.processingType], out processingType) ? processingType : Resources.ProcessingType.Unknown;
			}
		}

		public Guid DepositionGuid
		{
			get
			{
				if (!Dictionary.ContainsKey(Resources.CommandParameter.depositionId))
					return Guid.Empty;
				return Guid.Parse(Dictionary[Resources.CommandParameter.depositionId]);
			}
		}

		public string Input
		{
			get
			{
				if (!Dictionary.ContainsKey(Resources.CommandParameter.inputFilePath))
					return null;
				return Dictionary[Resources.CommandParameter.inputFilePath];
			}
		}

		public string OutputFilePath
		{
			get
			{
				if (!Dictionary.ContainsKey(Resources.CommandParameter.outputFilePath))
					return null;
				return Dictionary[Resources.CommandParameter.outputFilePath];

			}
		}

		public string OutputDirectory
		{
			get
			{
				if (!Dictionary.ContainsKey(Resources.CommandParameter.outputDir))
					return null;
				return Dictionary[Resources.CommandParameter.outputDir];
			}
		}
		
		public DataDomain DataDomain
		{
			get
			{
				DataDomain dataDomain;
				return Enum.TryParse(Dictionary[Resources.CommandParameter.dataDomain], out dataDomain) ? dataDomain : DataDomain.Unidentified;
			}
		}

		public bool IsGcnDeposition
		{
			get
			{
				bool isGcnDeposition = false;
				if (Dictionary.ContainsKey(Resources.CommandParameter.isGcnDeposition))
				{
					Boolean.TryParse(Dictionary[Resources.CommandParameter.isGcnDeposition], out isGcnDeposition);
				}
				return isGcnDeposition;
			}
		}

		public bool DoValidate
		{
			get
			{
				bool doValidate = false;
				if (Dictionary.ContainsKey(Resources.CommandParameter.doValidate))
					Boolean.TryParse(Dictionary[Resources.CommandParameter.doValidate], out doValidate);
				return doValidate;
			}
		}

		public bool DoStandardize
		{
			get
			{
				bool doStandardize = false;
				if (Dictionary.ContainsKey(Resources.CommandParameter.doStandardize))
					Boolean.TryParse(Dictionary[Resources.CommandParameter.doStandardize], out doStandardize);
				return doStandardize;
			}
		}

		protected static IDictionary<Resources.CommandParameter, string> ParseCommandParameters(string[] args)
		{
			var result = new Dictionary<Resources.CommandParameter, string>();
			foreach(string s in args)
			{
				var values = s.Split('=');
				if(values.Length == 2)
				{
					Resources.CommandParameter pp;
					if (Enum.TryParse(values[0], out pp))
						result.Add(pp, values[1]);
				}
			}
			return result;
		}

		public void AddParameter(Resources.CommandParameter parameter, string value)
		{
			if (!Dictionary.ContainsKey(parameter))
				Dictionary.Add(parameter, value);
		}

		public bool Store(string path)
		{
			if (!Directory.Exists(new FileInfo(path).Directory.FullName))
				return false;

			using(StreamWriter sw = new StreamWriter(path))
			{
				foreach(KeyValuePair<Resources.CommandParameter,string> kv in Dictionary)
					sw.WriteLine(kv.Key.ToString() + "=" + kv.Value);
			}

			if (!File.Exists(path))
				return false;

			return true;
		}
	}
}
