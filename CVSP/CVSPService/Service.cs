using System;
using System.Configuration;
using System.Configuration.Install;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Linq;

using CVSPService;
using RSC.CVSP;
using System.Collections.Generic;
using RSC.Process;
using RSC.Process.EntityFramework;

/// <summary>
/// Incapsulates all ChemValidator functionality as a Windows Service
/// </summary>
public class ChemValidatorService : ServiceBase
{
	/// <summary>
	/// Serializes access to service operations and stop event
	/// </summary>
	private Mutex mutex = new Mutex();

	/// <summary>
	/// Used to communicate command line parameters to the process/gather routines
	/// </summary>
	private InstallContext m_context;

	/// <summary>
	/// Short service name
	/// </summary>
	public static string Service_Name = "ChemValidator";

	/// <summary>
	/// Long service name
	/// </summary>
	public static string Service_DisplayName = "Chemistry Validation Platform";

	/// <summary>
	/// Service description
	/// </summary>
	public static string Service_Description = "Chemistry Validation Platform Service performs all processing activity on incoming chemical data including splitting CDX files, generating properties, publishing data to PubChem and ChemSpider and retrieving results back.";

	//	List of running jobs: <JobID, PID>
	public IDictionary<Guid, int> runningJobs = new Dictionary<Guid, int>();

	/// <summary>
	/// Variable of the class which helps to distinguish systematic name from ID and extract ID parts
	/// </summary>
	// private StructureIDValidator m_id_validator = new StructureIDValidator();
	private readonly int maxJobsAmount;

	private readonly Queue<int> databaseIndexes;
	private readonly Dictionary<Guid, int> databaseIndexesInUse;

	public ChemValidatorService(InstallContext context)
	{
		ServiceName = Service_Name;
		m_context = context;

		this.maxJobsAmount = Environment.ProcessorCount;

		if (ConfigurationManager.AppSettings["maxJobsAmount"] != null)
		{
			this.maxJobsAmount = Convert.ToInt32(ConfigurationManager.AppSettings["maxJobsAmount"]);
		}

		this.databaseIndexes = new Queue<int>(this.maxJobsAmount);
		this.databaseIndexesInUse = new Dictionary<Guid, int>();

		for (var i = 0; i < this.maxJobsAmount; i++)
		{
			this.databaseIndexes.Enqueue(i);
		}
	}

	/// <summary>
	/// Called on service start. Create and start timer.
	/// </summary>
	/// <param name="args">Service arguments</param>
	protected override void OnStart(string[] args)
	{
		start();
	}

	private Thread _worker_thread;

	/// <summary>
	/// Create a timer that'll call process/gather routines.
	/// </summary>
	public void start()
	{
		_worker_thread = new Thread(service_method);
		_worker_thread.Start();
	}

	/// <summary>
	/// Called on service stop. Wait for current run to finish, then disposes timer.
	/// </summary>
	protected override void OnStop()
	{
		mutex.WaitOne();
		try
		{
			if (_worker_thread != null)
				_worker_thread.Abort();
			_worker_thread = null;
		}
		finally
		{
			mutex.ReleaseMutex();
		}
	}

	/// <summary>
	/// This is "workhorse" method. Waits for its order (to not interfere with service stop event) and then processes/gathers data.
	/// </summary>
	/// <param name="o"></param>
	private void service_method(object o)
	{
		while (true)
		{
			mutex.WaitOne();
			try
			{
				process();
			}
			catch (ThreadAbortException)
			{
				Trace.TraceError("Aborting");
				break;
			}
			catch (Exception ex)
			{
				Trace.TraceError(String.Format("{0}\n{1}", ex.Message, ex.StackTrace));

			}
			finally
			{
				mutex.ReleaseMutex();
			}

			Thread.Sleep(int.Parse(ConfigurationManager.AppSettings["period"] ?? "60") * 500);
		}
	}



	/// <summary>
	/// Processes one or all outstanding files (which are in Submitted state)
	/// </summary>
	public void process()
	{
		Console.WriteLine(DateTime.Now);

		IJobManager jobManager = new EFJobManager();

		//	check if we still have jobs running...
		foreach (var id in runningJobs.Keys.ToList())
		{
			var pid = runningJobs[id];

			//	Job's status changed from Processing to something else... so we need to remove it from the list of running jobs...
			if (jobManager.GetJobStatus(id) != JobStatus.Processing)
			{
				runningJobs.Remove(id);
				this.databaseIndexes.Enqueue(this.databaseIndexesInUse[id]);
				this.databaseIndexesInUse.Remove(id);
				continue;
			}

			//	check that the process with PID still exist... otherwise we need to remove job from the running list and set status as Failed... 
			if (!Process.GetProcesses().Any(p => p.Id == pid))
			{
				jobManager.ChangeJobStatus(id, JobStatus.Failed);
				runningJobs.Remove(id);
				this.databaseIndexes.Enqueue(this.databaseIndexesInUse[id]);
				this.databaseIndexesInUse.Remove(id);
			}
		}

		Console.WriteLine("Running jobs: {0}", runningJobs.Count);

		//	check if we are out of allowed amount of jobs running simultaneously...
		if (runningJobs.Count >= this.maxJobsAmount)
			return;

		for (int i = 0; i < this.maxJobsAmount - runningJobs.Count; i++)
		{
			var jobId = jobManager.FetchJob();

			if (jobId == null)
				break;

			var job = jobManager.GetJob((Guid)jobId).ToCVSPJob();

			var databaseIndex = this.databaseIndexes.Dequeue();

			this.databaseIndexesInUse[job.Id] = databaseIndex;

			var pid = Utils.RunWorker(job, databaseIndex);

			runningJobs[job.Id] = pid;
		}

		/*var newJobs = jobManager.GetJobs(jobManager.GetJobsByStatus(JobStatus.New)).Select(j => j.ToCVSPJob()).ToList();

		Console.WriteLine("Jobs in queue: {0}; Running jobs: {1}", newJobs.Count, runningJobs.Count);

		foreach (var job in newJobs)
		{
			//	check if we are out of allowed amount of jobs running simultaneously...
			if (runningJobs.Count >= maxJobsAmount)
				break;

			//	mark job as "Processing" in order to hide it from the queue...
			jobManager.ChangeJobStatus(job.Id, JobStatus.Processing);

			var pid = Utils.RunWorker(job);

			runningJobs[job.Id] = pid;
		}*/
	}
}
