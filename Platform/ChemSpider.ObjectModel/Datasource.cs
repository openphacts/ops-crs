using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace ChemSpider.ObjectModel
{
    [DataContract(Namespace = "")]
    public class DatasourceType
    {
        [DataMember(EmitDefaultValue = false)]
        [Description("Internal ChemSpider data source type ID")]
        public int? ID
		{
			get;
			set;
		}

        [DataMember(EmitDefaultValue = false)]
        [Description("General data source type name")]
        public string Name
        {
			get;
			set;
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("Short data source type's name")]
        public string ShortName
        {
			get;
			set;
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("List of datasources")]
        public IEnumerable<Datasource> Datasources
        {
            get;
            set;
        }

        public override string ToString()
        {
            return String.Format("{0} [{1}]", Name, ShortName);
        }
    }

    [DataContract(Namespace = "")]
    public class Datasource
    {
        [DataMember]
        [Description("Internal ChemSpider data source ID")]
        public int ID
		{
			get;
			set;
		}

        [DataMember(EmitDefaultValue = false)]
        [Description("General data source name")]
        public string Name
        {
			get;
			set;
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("Data source's URL")]
        public string Url
        {
			get;
			set;
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("List of references from the data source")]
        public IEnumerable<Reference> References
        {
            get;
            set;
        }

    }
/*
    [DataContract(Namespace = "")]
    public class Substance
    {
        [DataMember]
        [Description("Internal ChemSpider substance ID")]
        public int ID
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("External substance ID. Used in the provider's system.")]
        public string ExtID
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("Substance's URL")]
        public string Url
        {
            get;
            set;
        }
    }
*/
}
