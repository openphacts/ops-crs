using System.Runtime.Serialization;

namespace RSC.Compounds
{
    [DataContract]
    public class ExternalReferenceType
    {
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string UriSpace { get; set; }
    }
}
