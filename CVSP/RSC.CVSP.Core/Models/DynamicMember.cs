using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace RSC.CVSP
{
    [Serializable]
    [DataContract]
    public class DynamicMember
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public object Member { get; set; }
    }
}
