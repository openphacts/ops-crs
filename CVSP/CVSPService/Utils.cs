using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Diagnostics;
using RSC.CVSP;


namespace CVSPService
{
	/// <summary>
	/// Represents batch information
	/// </summary>

	public class Utils
	{
		public static Dictionary<Guid, List<int>> depositionsProcesses;
		static Utils()
		{
			///key is dep_id, values are process ids
			depositionsProcesses = new Dictionary<Guid, List<int>>();
		}

		public static int LaunchProcess(Guid depositionGuid, string process_param)
		{
			if (depositionGuid == Guid.Empty)
				return 0;

			int pid = 0;

			string cvsp_path = ConfigurationManager.AppSettings["CVSPWorker.exe"];
			if (!File.Exists(cvsp_path))
			{
				Trace.TraceInformation("File not found: " + cvsp_path);
				throw new Exception("File not found: " + cvsp_path);
			}

			pid = executeProcess(cvsp_path, process_param, false);
			Trace.TraceInformation("command line: " + cvsp_path + " " + process_param);
			//add pid to depositon processes
			if (!depositionsProcesses.ContainsKey(depositionGuid))
				depositionsProcesses[depositionGuid] = new List<int>() { pid };
			else if (!depositionsProcesses[depositionGuid].Contains(pid))
				depositionsProcesses[depositionGuid].Add(pid);

			//clean dictionary from depositions that do not have any processes; also remove non-existing processes
			UpdateProcessesPerDepositions();

			Trace.TraceInformation("---\nStarting new process: DepID: " + depositionGuid + "\nProcess ID :" + pid + "\nCommand: " + process_param + "\n---");
			Trace.TraceInformation("Deposition " + depositionGuid + " has " + depositionsProcesses[depositionGuid].Count + " active processes");
			return pid;
		}

		public static int RunWorker(CVSPJob job, int databaseIndex)
		{
			if (job == null || job.Id == Guid.Empty)
				return 0;

			int pid = 0;

			string cvsp_path = ConfigurationManager.AppSettings["CVSPWorker.exe"];
			if (!File.Exists(cvsp_path))
			{
				Trace.TraceInformation("File not found: " + cvsp_path);
				throw new FileNotFoundException(cvsp_path);
			}

			var arguments = "job=" + job.Id;

			//if (job.Command != "prepare")
			//{
			//	arguments += " databaseIndex=" + databaseIndex;
			//}

			pid = executeProcess(cvsp_path, arguments, false);

			Trace.TraceInformation("command line: {0} " + arguments, cvsp_path);

			//add pid to depositon processes
			//if (!depositionsProcesses.ContainsKey(job.Deposition))
			//	depositionsProcesses[job.Deposition] = new List<int>() { pid };
			//else if (!depositionsProcesses[job.Deposition].Contains(pid))
			//	depositionsProcesses[job.Deposition].Add(pid);

			//clean dictionary from depositions that do not have any processes; also remove non-existing processes
			//UpdateProcessesPerDepositions();

			//Trace.TraceInformation("---\nStarting new process: DepositionID: {0}\nProcess ID :{1}\nJob: {2}\n---", job.Deposition, pid, job.Id);
			//Trace.TraceInformation("Deposition {0} has {1} active processes", job.Deposition, depositionsProcesses[job.Deposition].Count);
			return pid;
		}

		public static void UpdateProcessesPerDepositions()
		{
			Dictionary<Guid, List<int>> temp = new Dictionary<Guid, List<int>>();
			foreach (KeyValuePair<Guid, List<int>> depProcesses in depositionsProcesses)
			{
				List<int> updated_processes = new List<int>();
				foreach (int process in depProcesses.Value)
					if (ProcessExists(process) && !updated_processes.Contains(process))
						updated_processes.Add(process);
				if (temp.ContainsKey(depProcesses.Key))
					temp[depProcesses.Key] = updated_processes;
				else if (updated_processes.Count > 0)
					temp.Add(depProcesses.Key, updated_processes);

			}
			depositionsProcesses = temp;
			foreach (KeyValuePair<Guid, List<int>> kv in depositionsProcesses)
			{
				Console.WriteLine("Deposition " + kv.Key + " has " + kv.Value.Count() + " processes");
			}
		}

		public static bool CanStartNewProcesses(Guid dep_id, int maxProcessLimit)
		{
			//bool res = false;
			UpdateProcessesPerDepositions();
			//Process[] processlist = Process.GetProcessesByName(ConfigurationManager.AppSettings["CVSPWorkerName"]);

			if (depositionsProcesses.ContainsKey(dep_id))
			{
				if (depositionsProcesses[dep_id].Count < maxProcessLimit)
				{
					return true;
				}
				else
				{
					Console.WriteLine("Deposition " + dep_id + " has reached its limit.. skipping");
				}
				return false;
			}
			
			return true;

		}

		public static bool ProcessExists(int iProcessID)
		{
			foreach (Process p in Process.GetProcesses())
			{
				if (p.Id == iProcessID)
				{
					return true;
				}
			}
			return false;
		}

		private static int executeProcess(string exePath, string arguments, bool waitForExit)
		{
			try
			{
				Process process = new Process();
				process.StartInfo.FileName = exePath;
				process.StartInfo.Arguments = arguments;

				//process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.ErrorDialog = false;

				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.UseShellExecute = false;

				process.Start();
				//process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
				//process.BeginOutputReadLine();

				Console.WriteLine("Process " + process.Id + " started: " + exePath + " " + arguments);

				process.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);
				process.BeginErrorReadLine();

				process.EnableRaisingEvents = true;
				process.Exited += p_Exited;

				if (waitForExit)
				{
					process.WaitForExit();
					process.CloseMainWindow();
				}
				return process.Id;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Process exception caught: " + ex.Message + "\n" + ex.StackTrace);
				//Console.WriteLine("Relaunching process.. ");
				//return launchProcess(exePath, arguments, waitForExit);
				return 0;
			}
		}

		static void p_Exited(object sender, EventArgs e)
		{
			Process p = sender as Process;
			if (p != null)
			{
				Console.WriteLine("Process " + p.Id + " exited with code:{0} ", p.ExitCode);
			}
			else
				Console.WriteLine("exited");
		}
	}
}
