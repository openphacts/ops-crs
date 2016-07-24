using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP
{
	public class RecordProcessing : IRecordProcessing
	{
		protected List<IOperation> Operations = new List<IOperation>();
		public IEnumerable<Record> Process(IEnumerable<Record> records)
		{
			foreach (var operation in Operations)
			{
				try
				{
					records = operation.Process(records);
				}
				catch (Exception ex)
				{
					//record.Issues.Add(
					//	new Issue()
					//	{
					//		Severity = Severity.Error,
					//		Message = "Processing operation failed",
					//		Exception = ex.StackTrace,
					//		Description = ex.Message,
					//		Type = IssueType.Processing,
					//		Code = (int)IssueCodes.ProcessingCode.OperationError
					//	}
					//);
				}
			}

			return records;
		}
	}
}
