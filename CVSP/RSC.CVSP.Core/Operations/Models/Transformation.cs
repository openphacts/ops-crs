using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP
{
    public enum TransformationType
    {
        SMIRKS = 0
    }

    /// <summary>
    /// If Type is SMIRKS, then Value should be set to the SMIRKS string used.
    /// If we come up with any other transforms then we shall see.
    /// </summary>
    public class Transformation
    {
        public TransformationType Type { get; set; }
        public string Value { get; set; }
    }
}
