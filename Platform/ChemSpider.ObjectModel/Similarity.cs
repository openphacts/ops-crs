using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ChemSpider.ObjectModel
{
    public enum SimilarityType
    {
        Tanimoto
    }

    [DataContract(Namespace = "")]
    public class Similarity
    {
        [DataMember]
        public int CSID { get; set; }

        [DataMember]
        public double Score { get; set; }

        [DataMember]
        public SimilarityType SimilarityType { get; set; }
    }
}
