using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSC.Compounds;

namespace RSC.Compounds.DataExport
{
    public static class ParentChildRelationshipExtensions
    {
        //Descriptions.
        const string ChargeInsensitiveDescription = "Charge Insensitive Parent: this parent is generated when it is possible to unambiguously neutralize charge, e.g. metal cation to metal or carboxylate to carboxylic acid.";
        const string FragmentDescription = "Fragment: fragment relationships are for dealing with mixtures or ion/counter ion couples. It will connect each distinct chemical in the mixture or couple to the parent.";
        const string IsotopeInsensitiveDescription = "Isotope Insensitive Parent: this parent replaces all isotopes with average mass.";
        const string StereoInsensitiveDescription = "Stereo Insensitive Parent: this parent completely strips out stereo (both SP3 and double bond)";
        const string SuperInsensitiveDescription = "Super Insensitive Parent: this parent is produced by sequential application of molecular modifications to produce stereo insensitive, charge insensitive, isotope insensitive parent.";
        const string TautomerInsensitiveDescription = "Tautomer Insensitive Parent: this parent represents the canonicalized tautomer.";

        //Name.
        const string FragmentName = "Fragment";
        const string ChargeInsensitiveName = "Charge Insensitive Parent";
        const string IsotopeInsensitiveName = "Isotope Insensitive Parent";
        const string StereoInsensitiveName = "Stereo Insensitive Parent";
        const string SuperInsensitiveName = "Super Insensitive Parent";
        const string TautomerInsensitiveName = "Tautomer Insensitive Parent";

        /// <summary>
        /// Returns the description of a Parent Child relationship.
        /// </summary>
        /// <param name="relationship">The ParentChildRelationship</param>
        /// <returns>The Parent Child relationship description.</returns>
        public static string GetDescription(this ParentChildRelationship relationship)
        {
            switch (relationship)
            {
                case ParentChildRelationship.ChargeInsensitive:
                    return ChargeInsensitiveDescription;
                case ParentChildRelationship.Fragment:
                    return FragmentDescription;
                case ParentChildRelationship.IsotopInsensitive:
                    return IsotopeInsensitiveDescription;
                case ParentChildRelationship.StereoInsensitive:
                    return StereoInsensitiveDescription;
                case ParentChildRelationship.SuperInsensitive:
                    return SuperInsensitiveDescription;
                case ParentChildRelationship.TautomerInsensitive:
                    return TautomerInsensitiveDescription;
                default:
                    return string.Empty;
            }
        }
        
        /// <summary>
        /// Returns the name of a Parent Child relationship.
        /// </summary>
        /// <param name="relationship">The ParentChildRelationship</param>
        /// <returns>The Parent Child relationship name.</returns>
        public static string GetName(this ParentChildRelationship relationship)
        {
            switch (relationship)
            {
                case ParentChildRelationship.ChargeInsensitive:
                    return ChargeInsensitiveName;
                case ParentChildRelationship.Fragment:
                    return FragmentName;
                case ParentChildRelationship.IsotopInsensitive:
                    return IsotopeInsensitiveName;
                case ParentChildRelationship.StereoInsensitive:
                    return StereoInsensitiveName;
                case ParentChildRelationship.SuperInsensitive:
                    return SuperInsensitiveName;
                case ParentChildRelationship.TautomerInsensitive:
                    return TautomerInsensitiveName;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Returns the required SkosPredicate for a particular Parent Child relationship.
        /// </summary>
        /// <param name="relationship">The ParentChildRelationship</param>
        /// <returns>The required SkosPredicate</returns>
        public static SkosPredicate GetPredicate(this ParentChildRelationship relationship)
        {
            switch (relationship)
            {
                case ParentChildRelationship.Fragment:
                    return SkosPredicate.RELATED_MATCH;
                default:
                    return SkosPredicate.CLOSE_MATCH;
            }
        }

        /// <summary>
        /// Returns the required Rdf predicate - currently this uses CHEMINF.
        /// </summary>
        /// <param name="relationship">The ParentChildRelationship</param>
        /// <returns>The required predicate.</returns>
        public static Uri GetRdfPredicate(this ParentChildRelationship relationship)
        {
            switch (relationship)
            {
                case ParentChildRelationship.ChargeInsensitive:
                    return Turtle.ChargeInsensitiveRdfPredicate;
                case ParentChildRelationship.Fragment:
                    return Turtle.FragmentRdfPredicate;
                case ParentChildRelationship.IsotopInsensitive:
                    return Turtle.IsotopeInsensitiveRdfPredicate;
                case ParentChildRelationship.StereoInsensitive:
                    return Turtle.StereoInsensitiveRdfPredicate;
                case ParentChildRelationship.SuperInsensitive:
                    return Turtle.SuperInsensitiveRdfPredicate;
                case ParentChildRelationship.TautomerInsensitive:
                    return Turtle.TautomerInsensitiveRdfPredicate;
                default:
                    return null;
            }
        }
    }
}
