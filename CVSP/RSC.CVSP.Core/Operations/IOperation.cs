using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP
{
	public class OperationInfo
	{
		// Id and other information
		public string Id { get; set; }
		public string Name { get; set; }
		public string Category { get; set; }
		public string Description { get; set; }

		// Full versioning and way to get to the module
		public Module Module { get; set; }
		public int Version { get; set; }

		// Optional parameters which may be specific to implementation - just need to be serializable 
		public string Parameters { get; set; }
	}

	public interface IOperation
	{
		/// <summary>
		/// Returns an array of structures holding metainformation about operations which current implementation provides
		/// </summary>
		/// <returns></returns>
		IEnumerable<OperationInfo> GetOperations();

		/// <summary>
		/// Runs all registered operations sequentially
		/// </summary>
		/// <param name="data">Record that should be processed</param>
		/// <param name="options">Key/Value pairs with OperationId/Options (from OperationInfo)</param>
		/// <returns></returns>
		//Record Process(Record record, IDictionary<string, object> options = null);

		/// <summary>
		/// Runs all registered operations sequentially
		/// </summary>
		/// <param name="data">Record that should be processed</param>
		/// <param name="options">Key/Value pairs with OperationId/Options (from OperationInfo)</param>
		/// <returns></returns>
		IEnumerable<Record> Process(IEnumerable<Record> records, IDictionary<string, object> options = null);
	}
}
