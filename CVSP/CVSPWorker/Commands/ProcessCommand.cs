using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RSC.CVSP;
using RSC.CVSP.Compounds;
using RSC.CVSP.Utils;
using RSC.Process;

namespace CVSPWorker
{
	public class ProcessCommand : IWorkerCommand
	{
		protected readonly IChunkManager chunkManager = null;
		protected readonly IJobManager jobManager = null;
		protected readonly ICVSPStore cvsp = null;

		protected readonly IAcidity acidity = null;
		protected readonly IValidationModule validationModule = null;
        protected readonly IValidationRuleModule validationRuleModule = null;
		protected readonly IValidationStereoModule validationStereoModule = null;
		protected readonly IStandardizationModule standardizationModule = null;
		protected readonly IStandardizationChargesModule standardizationChargesModule = null;
		protected readonly IStandardizationFragmentsModule standardizationFragmentsModule = null;
		protected readonly IStandardizationStereoModule standardizationStereoModule = null;
		protected readonly IStandardizationMetalsModule standardizationMetalsModule = null;

		protected IDictionary<DataDomain, IRecordValidation> validators = new Dictionary<DataDomain, IRecordValidation>();
		protected IDictionary<DataDomain, IRecordStandardization> standardizers = new Dictionary<DataDomain, IRecordStandardization>();
		protected IDictionary<DataDomain, IRecordProcessing> processors = new Dictionary<DataDomain, IRecordProcessing>();

		public ProcessCommand(
			ICVSPStore cvsp, 
			IChunkManager chunkManager, 
			IJobManager jobManager, 
			IAcidity acidity,
			IValidationModule validationModule,
			IValidationStereoModule validationStereoModule,
            IValidationRuleModule validationRuleModule,
			IStandardizationModule standardizationModule,
			IStandardizationChargesModule standardizationChargesModule,
			IStandardizationFragmentsModule standardizationFragmentsModule,
			IStandardizationStereoModule standardizationStereoModule,
			IStandardizationMetalsModule standardizationMetalsModule)
		{
			if (cvsp == null)
				throw new ArgumentNullException("cvsp");

			if (chunkManager == null)
				throw new ArgumentNullException("chunkManager");

			if (jobManager == null)
				throw new ArgumentNullException("jobManager");

			if (acidity == null)
				throw new ArgumentNullException("acidity");

			if (standardizationModule == null)
				throw new ArgumentNullException("standardizationModule");

			if (standardizationChargesModule == null)
				throw new ArgumentNullException("standardizationChargesModule");

			if (standardizationFragmentsModule == null)
				throw new ArgumentNullException("standardizationFragmentsModule");

			if (standardizationStereoModule == null)
				throw new ArgumentNullException("standardizationStereoModule");

			if (standardizationMetalsModule == null)
				throw new ArgumentNullException("standardizationTetalsModule");

			if (validationModule == null)
				throw new ArgumentNullException("validationModule");

			if (validationStereoModule == null)
				throw new ArgumentNullException("validationStereoModule");

            if (validationRuleModule == null)
                throw new ArgumentNullException("validationRuleModule");

			this.cvsp = cvsp;
			this.chunkManager = chunkManager;
			this.jobManager = jobManager;
			this.acidity = acidity;
			this.validationModule = validationModule;
			this.validationStereoModule = validationStereoModule;
            this.validationRuleModule = validationRuleModule;
			this.standardizationModule = standardizationModule;
			this.standardizationChargesModule = standardizationChargesModule;
			this.standardizationFragmentsModule = standardizationFragmentsModule;
			this.standardizationStereoModule = standardizationStereoModule;
			this.standardizationMetalsModule = standardizationMetalsModule;
		}

		protected IRecordValidation GetValidator(DataDomain domain)
		{
			if (validators.ContainsKey(domain))
				return validators[domain];
			IRecordValidation validator = null;
			switch (domain)
			{
				case DataDomain.Substances:
					validator = InitCompoundValidator();
					break;
				case DataDomain.Reactions:
					validator = InitReactionValidator();
					break;
				case DataDomain.Crystals:
					validator = InitCrystalValidator();
					break;
				case DataDomain.Spectra:
					validator = InitSpectraValidator();
					break;
				default:
					throw new NotSupportedException("not supported data domain");
			}
			validators.Add(domain, validator);
			return validator;
		}

		private IRecordValidation InitCompoundValidator()
		{
			//var dictionary = SdfUtils.GetSdfMappedFields(Parameters.Dictionary[Resources.CommandParameter.mappedFieldsFile]);
			//return new CompoundValidation(Parameters.Dictionary[Resources.CommandParameter.validationXMLFilePath],
			//	Parameters.Dictionary[Resources.CommandParameter.acidBaseXMLFilePath],
			//	dictionary);
			return new CompoundValidation(acidity, validationModule, validationStereoModule, validationRuleModule);
		}

		private IRecordValidation InitReactionValidator()
		{
			throw new NotImplementedException();
		}

		private IRecordValidation InitSpectraValidator()
		{
			throw new NotImplementedException();
		}

		private IRecordValidation InitCrystalValidator()
		{
			throw new NotImplementedException();
		}

		private IRecordStandardization InitCompoundStandardizer()
		{
			return new CompoundStandardization(acidity, standardizationModule, standardizationChargesModule, standardizationFragmentsModule, standardizationStereoModule, standardizationMetalsModule);
		}

		private IRecordStandardization InitReactionStandardizer()
		{
			throw new NotImplementedException();
		}

		private IRecordStandardization InitSpectraStandardizer()
		{
			throw new NotImplementedException();
		}

		private IRecordStandardization InitCrystalStandardizer()
		{
			throw new NotImplementedException();
		}

		private IRecordProcessing InitCompoundProcessor(IEnumerable<ProcessingParameter> parameters)
		{
			return new CompoundProcessing(parameters);
		}

		private IRecordProcessing InitReactionProcessor(IEnumerable<ProcessingParameter> parameters)
		{
			throw new NotImplementedException();
		}

		private IRecordProcessing InitSpectraProcessor(IEnumerable<ProcessingParameter> parameters)
		{
			throw new NotImplementedException();
		}

		private IRecordProcessing InitCrystalProcessor(IEnumerable<ProcessingParameter> parameters)
		{
			throw new NotImplementedException();
		}

		protected IRecordStandardization GetStandardizer(DataDomain domain)
		{
			if (standardizers.ContainsKey(domain))
				return standardizers[domain];
			IRecordStandardization standardizer = null;
			switch (domain)
			{
				case DataDomain.Substances:
					standardizer = InitCompoundStandardizer();
					break;
				case DataDomain.Reactions:
					standardizer = InitReactionStandardizer();
					break;
				case DataDomain.Crystals:
					standardizer = InitCrystalStandardizer();
					break;
				case DataDomain.Spectra:
					standardizer = InitSpectraStandardizer();
					break;
				default:
					throw new NotSupportedException("not supported data domain");
			}
			standardizers.Add(domain, standardizer);
			return standardizer;
		}

		protected IRecordProcessing GetProcessor(DataDomain domain, IEnumerable<ProcessingParameter> parameters)
		{
			if (processors.ContainsKey(domain))
				return processors[domain];
			IRecordProcessing processor = null;
			switch (domain)
			{
				case DataDomain.Substances:
					processor = InitCompoundProcessor(parameters);
					break;
				case DataDomain.Reactions:
					processor = InitReactionProcessor(parameters);
					break;
				case DataDomain.Crystals:
					processor = InitCrystalProcessor(parameters);
					break;
				case DataDomain.Spectra:
					processor = InitSpectraProcessor(parameters);
					break;
				default:
					throw new NotSupportedException("not supported data domain");
			}
			processors.Add(domain, processor);
			return processor;
		}

		public virtual bool Execute(CVSPJob parameters)
		{
			var deposition = cvsp.GetDeposition(parameters.Deposition);

			chunkManager.ChangeStatus(parameters.Chunk, ChunkStatus.Processing);

			parameters.StartWatch("ProcessCommand:chunkManager.GetRecords");
			var records = chunkManager.GetRecords(parameters.Chunk) as IEnumerable<Record>;
			parameters.StopWatch("ProcessCommand:chunkManager.GetRecords");

			bool doValidate = deposition.Parameters.AsBool("Validate");
			bool doStandardize = deposition.Parameters.AsBool("Standardize");

			parameters.StartWatch("ProcessCommand:validate and standardize");
			foreach (var record in records)
			{
				if (doValidate)
				{
					IRecordValidation validation = GetValidator(record.DataDomain);
					validation.Validate(record);
				}

				if (doStandardize)
				{
					IRecordStandardization standardization = GetStandardizer(record.DataDomain);
					standardization.Standardize(record);
				}
			}
			parameters.StopWatch("ProcessCommand:validate and standardize");

			parameters.StartWatch("ProcessCommand:process");
			foreach (var domain in records.Select(r => r.DataDomain).Distinct())
			{
				IRecordProcessing processor = GetProcessor(domain, deposition.Parameters);
				processor.Process(records.Where(r => r.DataDomain == domain));
			}
			parameters.StopWatch("ProcessCommand:process");

			parameters.StartWatch("ProcessCommand:chunkManager.CreateChunk");
			var chunkId = chunkManager.CreateChunk(parameters.Deposition, ChunkType.Processed, records);
			parameters.StopWatch("ProcessCommand:chunkManager.CreateChunk");

			chunkManager.ChangeStatus(parameters.Chunk, ChunkStatus.Processed);

			var upload2dbJob = new CVSPJob()
			{
				Command = "upload2db",
				Deposition = parameters.Deposition,
				Chunk = chunkId
			};

			if (deposition.Parameters.AsBool("DelayUpload2db"))
				upload2dbJob.Status = JobStatus.Delayed;

			jobManager.NewJob(upload2dbJob);

			return true;
		}
	}
}
