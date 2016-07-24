using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.DataExport
{
    /// <summary>
    /// Class for mapping Properties to their cheminf versions, which may be in different units and require scaling.
    /// </summary>
    public class ChemInfPropertyMapping
    {
        public ChemInfPropertyMapping(string propertyName, string predicate, double? ph = null, string unit = "qudt:DimensionlessUnit", double? scaling = null)
        {
            PropertyName = propertyName;
            Predicate = predicate;
            Unit = unit;
            Scaling = scaling;
            Ph = ph;
        }

        public string PropertyName { get; set; }
        public string Predicate { get; set; }
        public string Unit { get; set; }
        public double? Scaling { get; set; }
        public double? Ph { get; set; }
    }
}
