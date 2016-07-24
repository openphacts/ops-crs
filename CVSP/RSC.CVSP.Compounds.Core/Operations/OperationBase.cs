using com.ggasoftware.indigo;
using RSC.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Compounds.Operations
{
	public class OperationBase : IOperation
	{
		private static Indigo indigo = null;

		public static Indigo GetIndigo()
		{
			if (indigo == null)
			{
				indigo = new Indigo();
				indigo.setOption("ignore-stereochemistry-errors", "true");
				indigo.setOption("unique-dearomatization", "false");
				indigo.setOption("ignore-noncritical-query-features", "true");
				indigo.setOption("timeout", "600000");
			}

			return indigo;
		}

		public virtual IEnumerable<Record> Process(IEnumerable<Record> records, IDictionary<string, object> options = null)
		{
			foreach (var record in records)
			{
				try
				{
					ProcessRecord(record, options);
				}
				catch (Exception ex)
				{
					AddErrorIssue(record, ex);
				}
			}
			return records;
		}

		public virtual IEnumerable<OperationInfo> GetOperations()
		{
			return null;
		}

		public virtual void ProcessRecord(Record record, IDictionary<string, object> options = null)
		{
		}

		protected void AddErrorIssue(Record record, Exception ex)
		{
			(record.Issues as List<Issue>).Add(
				new Issue()
				{
					Code = "500.1",
					Message = ex.Message,
					AuxInfo = ex.StackTrace,
				}
			);
		}
	}
}
