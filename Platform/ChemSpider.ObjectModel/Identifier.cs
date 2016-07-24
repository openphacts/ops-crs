using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ChemSpider.ObjectModel
{
	public enum IdentifierType
	{
		InChI,
		SMILES,
		InChIKey,
        Title,
        SysName
	}

	/// <summary>
	/// Machine readable identifiers
	/// </summary>
    [DataContract(Namespace = "")]
    public class Identifier
	{
        [DataMember(EmitDefaultValue = false)]
		public string Version
		{
			get;
			set;
		}

        [DataMember]
		public string Value
		{
			get;
			set;
		}

        [DataMember]
		public IdentifierType IdentifierType
		{
			get;
			set;
		}

        public override string ToString()
        {
            return String.Format("{0} [{1} {2}]", Value, IdentifierType.ToString(), Version);
        }
	}
}

