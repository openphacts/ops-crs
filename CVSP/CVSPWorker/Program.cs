using System;
using System.Configuration.Install;
using Autofac;
using RSC.CVSP;
using RSC.Process;
using Microsoft.Practices.ServiceLocation;
using Autofac.Extras.CommonServiceLocator;
using System.Collections.Generic;
using RSC.Properties.EntityFramework;
using RSC.Properties;
using RSC.Logging;
using System.Linq;
using System.Diagnostics;

namespace CVSPWorker
{
    using RSC.CVSP.EntityFramework;
    using RSC.CVSP.Search;
    using RSC.Logging.EntityFramework;
    using RSC.Search;
    using System.Configuration;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.SqlClient;

    class Program
	{
		static string _usage = @"
Usage: CVSPWorker 
	[/job=<guid>]
	[/command={new|prepare|process|upload2db|delete|upload2gcn|delete_from_gcn|export}]
	[/user=<guid>]
	[/file=<path>]
	[/datasource=<guid>]
	[/deposition=<guid>]
	[/chunk=<guid>]
	[/validate={true|false}]
	[/standardize={true|false}]
	[/delayupload2db={true|false}]
	[/chemspiderproperties={true|false}]
	[/chemspidersynonyms={true|false}]
	[/databaseIndex=<integer>]
	[/newOnly={true|false}]
    [/query={search query in JSON format}]
";

		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine(_usage);
				Trace.TraceInformation("No arguments provided - exiting");
				return;
			}

			InstallContext context = new InstallContext(null, args);

			var loggingConnectionString = "LoggingConnection";
			var propertiesConectionString = "PropertiesConnection";
			var cvspConnectionString = "CVSPConnection";

			if (context.Parameters.ContainsKey("databaseIndex"))
			{
				var databaseId = context.Parameters["databaseIndex"];

				loggingConnectionString += databaseId;
				propertiesConectionString += databaseId;
				//cvspConnectionString += databaseId;
			}

			LogManager.Logger.AddTarget(new RSC.Logging.EntityFramework.EFLogStore(loggingConnectionString));

			var builder = new ContainerBuilder();

			builder.Register(c => new RSC.Properties.EntityFramework.EFPropertyStore(propertiesConectionString)).As<RSC.Properties.IPropertyStore>();
			builder.Register(c => new RSC.CVSP.EntityFramework.EFStatistics(cvspConnectionString, 300)).As<RSC.CVSP.IStatistics>();

			builder.RegisterType<RSC.Process.EntityFramework.EFChunkManager>().As<RSC.Process.IChunkManager>();
			builder.RegisterType<RSC.Process.EntityFramework.EFJobManager>().As<RSC.Process.IJobManager>();
			builder.RegisterType<RSC.CVSP.FileStorage>().As<RSC.CVSP.IFileStorage>();
			builder.Register(c => new RSC.CVSP.EntityFramework.EFCVSPStore(cvspConnectionString, 300)).As<RSC.CVSP.ICVSPStore>();
			builder.RegisterType<RSC.Compounds.EntityFramework.EFSubstanceStore>().As<RSC.Compounds.SubstanceStore>();
			builder.RegisterType<RSC.Compounds.EntityFramework.EFSubstanceBulkUpload>().As<RSC.Compounds.ISubstanceBulkUpload>();
			builder.RegisterType<RSC.Compounds.EntityFramework.EFCompoundStore2>().As<RSC.Compounds.ICompoundStore>();
            builder.Register(c => new EFLogStore("LoggingConnection")).As<ILogStore>();
            builder.Register(c => new EFCVSPSearch("CVSPConnection", 60)).As<CVSPSearch>();
            builder.RegisterType<MemoryRequestStorage>().As<IRequestStorage>();

            builder.RegisterModule<RSC.CVSP.Compounds.Autofac.ChemSpiderPropertiesModule>();
			builder.RegisterModule<RSC.CVSP.Compounds.Autofac.CompoundsBaseModule>();
			builder.RegisterModule<RSC.CVSP.Compounds.Autofac.ParentChildModule>();

			var container = builder.Build();

			ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(container));

			//	run specific initialisation process
			RSC.CVSP.Compounds.Init.Initialize();

			IJobManager jobManager = container.Resolve<IJobManager>();
			IChunkManager chunkManager = container.Resolve<IChunkManager>();

			CVSPJob job = null;

			if (context.Parameters.ContainsKey("job"))
			{
				//	automatic mode and job came from job manager...
				job = jobManager.GetJob(Guid.Parse(context.Parameters["job"])).ToCVSPJob();
				if (job == null)
				{
					Console.Error.WriteLine("Cannot find job {0}", context.Parameters["job"]);
					return;
				}

				//	rewrite some parameters in manual mode...
				job.SetParameters(context.Parameters);
			}
			else
			{
				//	in manual mode...
				job = new CVSPJob(context.Parameters);
			}

			var updater = new ContainerBuilder();

			switch (job.Command.ToLower())
			{
				case "new":
					updater.RegisterType<NewDepositionCommand>().As<IWorkerCommand>();
					break;
				case "prepare":
					updater.RegisterType<PrepareCommand>().As<IWorkerCommand>();
					break;
				case "process":
					updater.RegisterType<ProcessCommand>().As<IWorkerCommand>();
					break;
				case "upload2db":
					updater.RegisterType<UploadCommand>().As<IWorkerCommand>();
					break;
				case "upload2gcn":
					updater.RegisterType<Upload2GCNCommand>().As<IWorkerCommand>();
					break;
				case "delete_from_gcn":
					updater.RegisterType<DeleteFromGCNCommand>().As<IWorkerCommand>();
					break;
				case "delete":
					updater.RegisterType<DeleteDepositionCommand>().As<IWorkerCommand>();
					break;
                case "export":
                    updater.RegisterType<ExportCommand>().As<IWorkerCommand>();
                    break;
                default:
					Console.Error.WriteLine("Cannot recognize command: {0}", job.Command);
					Console.WriteLine(_usage);
					return;
			}

			updater.Update(container);

			LogManager.Logger.Info("Running CVSPWorker");

			using (var scope = container.BeginLifetimeScope())
			{
				try
				{
					if (jobManager.HasJob(job.Id))
					{
						job.Status = JobStatus.Processing;
						job.Started = DateTime.Now;

						jobManager.ChangeJobStatus(job.Id, JobStatus.Processing);
					}

					var worker = scope.Resolve<IWorkerCommand>();
					worker.Execute(job);

					if (jobManager.HasJob(job.Id))
					{
						job.Status = JobStatus.Processed;
						job.Finished = DateTime.Now;

						jobManager.UpdateJob(job.Id, job);
					}
				}
				catch (Exception ex)
				{
					if (jobManager.HasJob(job.Id))
					{
						job.Status = JobStatus.Failed;
						job.Error = ex.ToString();
						jobManager.UpdateJob(job.Id, job);
					}

					if (job.Chunk != Guid.Empty)
					{
						chunkManager.ChangeStatus(job.Chunk, ChunkStatus.Failed);
					}

					Console.Error.WriteLine(ex.ToString());
				}
			}
		}
	}
}
