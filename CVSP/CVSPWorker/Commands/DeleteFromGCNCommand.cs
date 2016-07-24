using RSC.Compounds;
using RSC.CVSP;
using RSC.CVSP.Compounds;
using RSC.CVSP.EntityFramework;
using RSC.CVSP.Utils;
using RSC.Process;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CVSPWorker
{
	public class DeleteFromGCNCommand : IWorkerCommand
	{
		private readonly ICVSPStore cvsp = null;
		private readonly SubstanceStore substances = null;
		public DeleteFromGCNCommand(ICVSPStore cvsp, SubstanceStore substances)
		{
			if (cvsp == null)
				throw new ArgumentNullException("cvsp");

			if (substances == null)
				throw new ArgumentNullException("substance");

			this.cvsp = cvsp;
			this.substances = substances;
		}

		public bool Execute(CVSPJob parameters)
		{
			cvsp.UpdateDepositionStatus(parameters.Deposition, DepositionStatus.DeletingFromGCN);

			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();

			Trace.WriteLine(string.Format("Deleting records from CGS for deposition: {0}", parameters.Deposition));

			var res = substances.DeleteDeposition(parameters.Deposition);

			stopWatch.Stop();
			Trace.WriteLine(string.Format("Done: {0}", stopWatch.Elapsed.ToString()));

			cvsp.UpdateDepositionStatus(parameters.Deposition, DepositionStatus.Processed);

			return res;
		}
	}
}
