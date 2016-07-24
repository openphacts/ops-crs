using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP
{
	public class OperationsManager : IOperationsManager
	{
		private List<IOperation> operations = new List<IOperation>();
		private IEnumerable<string> assemblies = new string[] { "RSC.CVSP.Compounds" };

		public OperationsManager()
		{
			GetAllRegisteredOperations();
		}

		private void GetAllRegisteredOperations()
		{
			var interfaceType = typeof(IOperation);

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => assemblies.Contains(a.GetName().Name)))
			{
				foreach (var type in assembly.GetTypes().Where(t => interfaceType.IsAssignableFrom(t)))
				{
					operations.Add(Activator.CreateInstance(type) as IOperation);
				}
			}
		}

		public IEnumerable<IOperation> GetOperations()
		{
			return operations;
		}

		public IEnumerable<IEnumerable<OperationInfo>> GetOperationsInfo()
		{
			return operations.Select(o => o.GetOperations());
		}
	}
}
