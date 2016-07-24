using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RSC.Properties;

namespace RSC.Compounds.DataExport
{
    public static class PropertyExtensions
    {
        /// <summary>
        /// Returns a list of triples representing a single calculated value and its error for a compound.
        /// </summary>
        /// <param name="compoundId">The compound Id.</param>
        /// <param name="rdfId">A turtle-specific number identifying the compound.</param>
        /// <param name="p">The property.</param>
        /// <param name="unitStyle">The unit style.</param>
        /// <param name="definitions">A list of property definitions.</param>
        /// <returns>A list of triples representing a single calculated value and its error for a compound.</returns>
        public static List<string> ToTurtle(this Property p, string compoundId, int rdfId, UnitStyle unitStyle,
            IEnumerable<PropertyDefinition> definitions)
        {
            //Check for pH Condition.
            var pHCondition = p.Conditions != null ? p.Conditions.SingleOrDefault(c => c.Name == PropertyName.PH) : null;
            var pH = pHCondition != null ? (double?)pHCondition.Value : null;
            //Get the correct mapping (including pH if specified).
            var mapping = pH == null
                ? Turtle.CheminfPropertyMappings.Single(m => m.PropertyName == p.Name)
                : Turtle.CheminfPropertyMappings.Single(m => m.PropertyName == p.Name && m.Ph == pH);
            string propSubject = string.Format(":{0}prop{1}", compoundId, rdfId);
            //Add the pH condition to the description if specified.
            string propDisplayName = definitions.Single(d => d.Name == p.Name).DisplayName;
            if (pH != null)
                propDisplayName += string.Format(" (pH {0})", pH.ToString());

            var result = new List<string>();
            // execution has specified output the particular property
            result.Add(string.Format(":{0}execution {1} {2} .", compoundId, Turtle.obo_hasspecifiedoutput, propSubject));
            result.Add(string.Format("{0} rdfs:label \"Compound {1} property {2} in {3}\"@en .", propSubject, compoundId, propDisplayName, mapping.Unit));
            result.Add(string.Format("{0} {3} {1}:{2} .", propSubject, Turtle.RDF_URI_PREFIX.ToLower(), compoundId, Turtle.obo_isabout));
            result.Add(string.Format("{0} rdf:type cheminf:{1} .", propSubject, mapping.Predicate));
            switch (unitStyle)
            {
                case UnitStyle.QUDT:
                    //Scale the value if required.
                    var scaledValue = mapping.Scaling != null ? Convert.ToDouble(p.Value) * mapping.Scaling ?? 1 : Convert.ToDouble(p.Value);
                    result.Add(string.Format("{0} qudt:numericValue \"{1}\"^^xsd:double .", propSubject, scaledValue));
                    result.Add(string.Format("{0} qudt:unit {1} .", propSubject, mapping.Unit));
                    string error = p.Error.ToString() ?? null;
                    if (!String.IsNullOrEmpty(error))
                    {
                        //Scale the error if required.
                        var scaledError = mapping.Scaling != null ? Convert.ToDouble(error) * mapping.Scaling ?? 1 : Convert.ToDouble(error);
                        result.Add(string.Format("{0} qudt:standardUncertainty \"{1}\"^^xsd:double .", propSubject, scaledError));
                    }
                    return result;
                default: throw new Exception("unsupported unit style");
            }
        }
    }
}
