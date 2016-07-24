using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP
{
	[Obsolete("This class will re removed soon!!! DO NOT USE IT!!!")]
	[DataContract]
	public class ProcessingParameters
	{
		public bool IsGCNDeposition { get; set; }
		public bool IsProcessedLocally { get; set; }
		public bool CompoundParentGeneration { get; set; }
		public bool TautomerCanonicalization { get; set; }
		public bool AllowRelativeStereo { get; set; }
		public bool SDFTagsCollected { get; set; }
		public int ChunkSize { get; set; }
		public Guid ValidationContentGuid { get; set; }
		public Guid AcidBaseContentGuid { get; set; }
		public Guid StandardizationContentGuid { get; set; }

		public ProcessingParameters()
		{
		}

		public ProcessingParameters(bool isGCNDeposition, Guid? validationContentGuid, Guid? acidBaseContentGuid, Guid? standardizationContentGuid)
		{
			//IsGCNDeposition = isGCNDeposition;
			if (validationContentGuid.HasValue && validationContentGuid.Value != Guid.Empty)
				ValidationContentGuid = validationContentGuid.Value;
			if (acidBaseContentGuid.HasValue && acidBaseContentGuid.Value != Guid.Empty)
				AcidBaseContentGuid = acidBaseContentGuid.Value;
			if (standardizationContentGuid.HasValue && standardizationContentGuid.Value != Guid.Empty)
				StandardizationContentGuid = standardizationContentGuid.Value;
			//CompoundParentGeneration = false;
			//TautomerCanonicalization = false;
			//AllowRelativeStereo = true;
		}
	}
}
