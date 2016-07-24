using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ChemSpider.ObjectModel
{
    public enum BlobType
    {
        Spectrum,
        Image,
        CIF
    }

    [DataContract(Namespace = "")]
    public class Blob
    {
        [DataMember]
        public int ID
        {
            get;
            set;
        }

        [DataMember]
        public BlobType BlobType
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        public string SubType
        {
            get;
            set;
        }

        [DataMember]
        public string Filename
        {
            get;
            set;
        }

        [DataMember]
        public bool Open
        {
            get;
            set;
        }

        [DataMember]
        public bool Approved
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        public string HomePageUrl
        {
            get;
            set;
        }

        public override string ToString()
        {
            return String.Format("{0} [{1} {2} {3}]", ID, BlobType.ToString(), SubType, Filename);
        }
    }
}
