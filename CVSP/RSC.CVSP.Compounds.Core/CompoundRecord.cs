using RSC.Properties;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RSC.CVSP.Compounds
{
	[DataContract]
	[Serializable]
	public class CompoundRecord : Record
	{
		public CompoundRecord()
			: base()
		{
			//	create new external Id for compound domain
			Id = ExternalId.NewId(1);
		}

		public string OriginalSmiles { get { return this.GetStringProperty(PropertyName.ORIGINAL_SMILES); } }
		public string StandardizedSmiles { get { return this.GetStringProperty(PropertyName.STANDARDIZED_SMILES); } }
        public string ChemSpiderId { get { return this.GetStringProperty(PropertyName.CSID); } }

		public RSC.Compounds.InChI StandardizedStdInChI
		{
			get
			{
				if (this.HasProperty(PropertyName.STD_INCHI) && this.HasProperty(PropertyName.STD_INCHI_KEY))
					return new RSC.Compounds.InChI() { Inchi = this.GetStringProperty(PropertyName.STD_INCHI), InChIKey = this.GetStringProperty(PropertyName.STD_INCHI_KEY) };

				return null;
			}
		}

		public RSC.Compounds.InChI StandardizedNonStdInChI
		{
			get
			{
				if (this.HasProperty(PropertyName.NON_STD_INCHI) && this.HasProperty(PropertyName.NON_STD_INCHI_KEY))
					return new RSC.Compounds.InChI() { Inchi = this.GetStringProperty(PropertyName.NON_STD_INCHI), InChIKey = this.GetStringProperty(PropertyName.NON_STD_INCHI_KEY) };

				return null;
			}
		}

		//[DataMember]
		//public RecordInchis TautomericInchi { get; set; }
	}
}
