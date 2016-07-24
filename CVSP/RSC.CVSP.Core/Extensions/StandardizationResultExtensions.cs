using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP
{
    public static class StandardizationResultExtensions
    {
        public static StandardizationResult AddTransformation(this StandardizationResult sr, string standardized, string SMIRKS)
        {
            var newTransformations = new List<Transformation>(sr.Transformations);
            newTransformations.Add(new Transformation() { Type = TransformationType.SMIRKS, Value = SMIRKS });
            return new StandardizationResult()
            {
                Standardized = standardized,
                Issues = sr.Issues,
                Transformations = newTransformations
            };
        }
    }
}
