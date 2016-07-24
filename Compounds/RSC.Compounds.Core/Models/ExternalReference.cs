using System.Runtime.Serialization;

namespace RSC.Compounds
{
    [DataContract]
    public class ExternalReference
    {
        [DataMember]
        public ExternalReferenceType Type { get; set; }
        
        [DataMember]
        public string Value { get; set; }
    }
}
