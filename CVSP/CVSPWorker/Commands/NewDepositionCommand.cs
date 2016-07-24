using RSC.CVSP;
using RSC.Process;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVSPWorker
{
	public class NewDepositionCommand : IWorkerCommand
	{
		private readonly ICVSPStore cvsp = null;
		private readonly IFileStorage fileStorage = null;
		private readonly IJobManager jobManager = null;

		public NewDepositionCommand(ICVSPStore cvsp, IFileStorage fileStorage, IJobManager jobManager)
		{
			if (cvsp == null)
				throw new ArgumentNullException("cvsp");

			if (fileStorage == null)
				throw new ArgumentNullException("fileStorage");

			if (jobManager == null)
				throw new ArgumentNullException("jobManager");

			this.cvsp = cvsp;
			this.fileStorage = fileStorage;
			this.jobManager = jobManager;
		}

		public bool Execute(CVSPJob parameters)
		{
			if(!File.Exists(parameters.File))
				throw new FileNotFoundException("Cannot find file: " + parameters.File);

			if (parameters.Datasource == null)
				throw new NoDatasourceException("Datasource is not assigned");

			FileStream file = new FileStream(parameters.File, FileMode.Open, FileAccess.Read);
			FileInfo fo = new FileInfo(parameters.File);

			var processingParameters = new List<ProcessingParameter>();

			processingParameters.AddRange(new ProcessingParameter[] { 
				new ProcessingParameter()
				{
					Name = "Validate",
					Value = parameters.DoValidate.ToString()
				},
				new ProcessingParameter()
				{
					Name = "Standardize",
					Value = parameters.DoStandardize.ToString()
				},
				new ProcessingParameter()
				{
					Name = "PropertiesCalculation",
					Value = parameters.PropertiesCalculation.ToString()
				},
				new ProcessingParameter()
				{
					Name = "ParentsGeneration",
					Value = parameters.PropertiesCalculation.ToString()
				},
				new ProcessingParameter()
				{
					Name = "DelayUpload2db",
					Value = parameters.DelayUpload2db.ToString()
				},
				new ProcessingParameter()
				{
					Name = "ChemSpiderProperties",
					Value = parameters.ChemSpiderProperties.ToString()
				},
				new ProcessingParameter()
				{
					Name = "ChemSpiderSynonyms",
					Value = parameters.ChemSpiderSynonyms.ToString()
				}
			});

			var guid = cvsp.CreateDeposition(new Deposition()
			{
				UserId = parameters.User,
				DatasourceId = (Guid)parameters.Datasource,
				Status = DepositionStatus.Submitting,
				Parameters = processingParameters
			});

			fileStorage.UploadFile(guid, fo.Name, file);

			cvsp.UpdateDepositionStatus(guid, DepositionStatus.Submitted);

			jobManager.NewJob(new CVSPJob()
			{
				Command = "prepare",
				Deposition = guid,
				Datasource = parameters.Datasource,
				DataDomain = DataDomain.Substances
			});

			return true;
		}
	}
}
