using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP
{
	public interface IOperationsManager
	{
		IEnumerable<IOperation> GetOperations();

		IEnumerable<IEnumerable<OperationInfo>> GetOperationsInfo();
	}
}
