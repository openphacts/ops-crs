using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ChemSpider.ObjectModel
{
    /// <summary>
    /// Machine readable identifiers
    /// </summary>
    [DataContract]
    public class Property
    {
        /// <summary>
        /// http://chemistry.library.wisc.edu/properties/property-list.html
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string URN
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        public double Value
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        public double? Error
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Machine readable identifiers
    /// </summary>
    [DataContract]
    public class SoftwareVersion
    {
        [DataMember(EmitDefaultValue = false)]
        public string Package
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        public int? MajorVersion
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        public int? MinorVersion
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        public string Company
        {
            get;
            set;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}", Package);
            if ( MajorVersion != null && MinorVersion != null )
                sb.AppendFormat(" v{0}.{1}", MajorVersion, MinorVersion);
            else if ( MajorVersion != null )
                sb.AppendFormat(" v{0}", MajorVersion);
            if ( !String.IsNullOrEmpty(Company) )
                sb.AppendFormat(" ({0})", Company);
            return sb.ToString();
        }
    }

    /// <summary>
    /// Machine readable identifiers
    /// </summary>
    [DataContract(Namespace = "")]
    public class PredictedProperty : Property
    {
        [DataMember(EmitDefaultValue = false)]
        public SoftwareVersion Software
        {
            get;
            set;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}", Value);
            if ( Error != null )
                sb.AppendFormat(" +/- {0}", Error);
            if ( Software != null )
                sb.AppendFormat(" ({0})", Software);
            return sb.ToString();
        }
    }

    /// <summary>
    /// Machine readable identifiers
    /// </summary>
    [DataContract(Namespace = "")]
    public class ExperimentalProperty : Property
    {
        [DataMember(EmitDefaultValue = false)]
        public Uri Reference
        {
            get;
            set;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}", Value);
            if ( Error != null )
                sb.AppendFormat(" +/- {0}", Error);
            if ( Reference != null )
                sb.AppendFormat(" ({0})", Reference);
            return sb.ToString();
        }
    }
}
