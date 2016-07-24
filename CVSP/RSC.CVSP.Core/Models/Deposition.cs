using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Linq;

namespace RSC.CVSP
{
	[DataContract]
	public enum DepositionStatus
	{
		[Display(Name = "Submitting")]
		[EnumMember]
		Submitting = 1,

		[Display(Name = "Submitted")]
		[EnumMember]
		Submitted,

		[Display(Name = "Uploaded")]
		[EnumMember]
		Uploaded,

		[Display(Name = "Processing")]
		[EnumMember]
		Processing,

		[Display(Name = "Processed")]
		[EnumMember]
		Processed,

		[Display(Name = "Failed")]
		[EnumMember]
		Failed,

		[Display(Name = "Locked")]
		[EnumMember]
		Locked,

		[Display(Name = "Revision Scheduled for Reprocessing")]
		[EnumMember]
		ToProcessRevisions,

		[Display(Name = "Scheduled for Reprocessing")]
		[EnumMember]
		ToReprocess,

		[Display(Name = "Scheduled for Deletion")]
		[EnumMember]
		ToDelete,

		[Display(Name = "User Action Required")]
		[EnumMember]
		UserActionRequired,

		[Display(Name = "Deleting")]
		[EnumMember]
		Deleting,

		[Display(Name = "Scheduled for Deposition to GCN")]
		[EnumMember]
		SubmittedForGCN,

		[Display(Name = "Depositing to GCN")]
		[EnumMember]
		ProcessingForGCN,

		[Display(Name = "Depositing to GCN")]
		[EnumMember]
		Depositing2GCN,

		[Display(Name = "Deposited to GCN")]
		[EnumMember]
		Deposited2GCN,

		[Display(Name = "Preparing for Reprocessing")]
		[EnumMember]
		Prepare4Reprocessing,

		[Display(Name = "Deleting from GCN")]
		[EnumMember]
		DeletingFromGCN,

		[Display(Name = "Unknown")]
		[EnumMember]
		Unknown
	}

	[Serializable]
	[DataContract]
	public class Deposition
	{
		public Deposition()
		{
			DepositionFiles = new List<DepositionFile>();
			Parameters = new List<ProcessingParameter>();
		}

		/// <summary>
		/// Constructor by default sets deposition status to "locked". 
		/// After creating an instance set it to "submitted" via update to start processing
		/// </summary>
		/// <param name="userGuid"></param>
		/// <param name="location">folder or file path</param>
		public Deposition(Guid userGuid, DataDomain dd)
		{
			UserId = userGuid;
			DataDomain = dd;
			Status = DepositionStatus.Locked;
			DepositionFiles = new List<DepositionFile>();
			Parameters = new List<ProcessingParameter>();
		}

		[DataMember]
		public Guid Id { get; set; }

		[DataMember]
		public Guid UserId { get; set; }

		[DataMember]
		public DataDomain DataDomain { get; set; }

		[DataMember]
		public IEnumerable<DepositionFile> DepositionFiles { get; set; }

		[Display(Name = "Submitted")]
		[DataMember]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		public DateTime DateSubmitted { get; set; }

		[DataMember]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		public DateTime? DateReprocessed { get; set; }

		[DataMember]
		public DepositionStatus Status { get; set; }

		[DataMember]
		public Guid DatasourceId { get; set; }

		public string StatusName
		{
			get
			{
				var type = typeof(DepositionStatus);
				var memberInfo = type.GetMember(this.Status.ToString());
				var attributes = memberInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);
				var name = ((DisplayAttribute)attributes[0]).Name;
				return name;
			}
		}

		[DataMember]
		public bool IsPublic { get; set; }

		[DataMember]
		public IEnumerable<ProcessingParameter> Parameters { get; set; }
	}

	[DataContract]
	[Serializable]
	public enum SDTagOptions
	{
		UNKNOWN,
		CHEMSPIDER_NONE,//used
		CHEMSPIDER_ID,
		SUBSTANCE_ID,
		COMPOUND_ID,
		MOLFORMULA,
		MW,
		SMILES,
		INDIGO_CANONICAL_SMILES,
		OPENEYE_ABS_SMILES,
		STANDARD_INCHI,
		STANDARD_INCHIKEY,
		MESSAGE,
		INDIGO_EXCEPTION,
		EXISTING_SYNONYMS_BY_CHEMSPIDER_ID,
		CDX_DIAGNOSTICS,
		AUX_INFO,

		DEPOSITOR_SOURCE_ID,

		[Display(Name = "Record unique identifier")]
		[EnumMember]
		DEPOSITOR_SUBSTANCE_REGID,

		[Display(Name = "InChI")]
		[EnumMember]

		DEPOSITOR_SUBSTANCE_INCHI,

		[Display(Name = "Smiles")]
		[EnumMember]
		DEPOSITOR_SUBSTANCE_SMILES,

		DEPOSITOR_SUBSTANCE_COMMENTS,

		[Display(Name = "Synonyms")]
		[EnumMember]
		DEPOSITOR_SUBSTANCE_SYNONYM,

		DEPOSITOR_SUBSTANCE_CAS,
		DEPOSITOR_SUBSTANCE_URL,
		DEPOSITOR_SUBSTANCE_XREFS,
		DEPOSITOR_PRIMARY_STRUCTURE_TAG,
		DEPOSITOR_SUBSTANCE_REVOKE,
		DEPOSITOR_DATASOURCE_URL,

		FRAGMENT_UNSENSITIVE_PARENT_INDIGO_CANONICAL_SMILES,
		CHARGE_UNSENSITIVE_PARENT_INDIGO_CANONICAL_SMILES,
		ISOTOPE_UNSENSITIVE_PARENT_INDIGO_CANONICAL_SMILES,
		STEREO_UNSENSITIVE_PARENT_INDIGO_CANONICAL_SMILES,
		TAUTOMER_UNSENSITIVE_PARENT_INDIGO_CANONICAL_SMILES,
		SUPER_UNSENSITIVE_PARENT_INDIGO_CANONICAL_SMILES
	}
}
