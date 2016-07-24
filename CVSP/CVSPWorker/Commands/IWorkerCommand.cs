using RSC.CVSP;

namespace CVSPWorker
{
	public interface IWorkerCommand
	{
		bool Execute(CVSPJob parameters);
	}
}
