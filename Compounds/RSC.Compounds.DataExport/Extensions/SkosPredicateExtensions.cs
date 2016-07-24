using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.DataExport
{
    public static class SkosPredicateExtensions
    {
        //Used for the skos: predicates.
        public const string SKOS_PREFIX = "skos";
        public const string SKOS_RELATED_MATCH = "relatedMatch";
        public const string SKOS_EXACT_MATCH = "exactMatch";
        public const string SKOS_CLOSE_MATCH = "closeMatch";

        /// <summary>
        /// Returns a Uri for the Skos Predicate.
        /// </summary>
        /// <param name="predicate">The predicate enum</param>
        /// <returns>A new Uri representing the Skos relation.</returns>
        public static Uri ToUri(this SkosPredicate predicate)
        {
            switch (predicate)
            {
                case SkosPredicate.EXACT_MATCH:
                    return new Uri(Turtle.ns_skos.ToString() + SKOS_EXACT_MATCH);
                case SkosPredicate.CLOSE_MATCH:
                    return new Uri(Turtle.ns_skos.ToString() + SKOS_CLOSE_MATCH);
                case SkosPredicate.RELATED_MATCH:
                    return new Uri(Turtle.ns_skos.ToString() + SKOS_RELATED_MATCH);
                default: //Default to exact match.
                    return new Uri(Turtle.ns_skos.ToString() + SKOS_EXACT_MATCH);
            }
        }

        /// <summary>
        /// Returns a string representing the Skos Predicate.
        /// </summary>
        /// <param name="predicate">The predicate enum</param>
        /// <returns>The predicate string</returns>
        public static string ToName(this SkosPredicate predicate)
        {
            switch (predicate)
            {
                case SkosPredicate.EXACT_MATCH:
                    return SKOS_EXACT_MATCH;
                case SkosPredicate.CLOSE_MATCH:
                    return SKOS_CLOSE_MATCH;
                case SkosPredicate.RELATED_MATCH:
                    return SKOS_RELATED_MATCH;
                default: //Default to exact match.
                    return SKOS_EXACT_MATCH;
            }
        }

        /// <summary>
        /// Returns a description representing the Skos Predicate.
        /// </summary>
        /// <param name="predicate">The predicate enum</param>
        /// <returns>The predicate string</returns>
        public static string ToDescription(this SkosPredicate predicate)
        {
            switch (predicate)
            {
                case SkosPredicate.EXACT_MATCH:
                    return "match exactly with";
                case SkosPredicate.CLOSE_MATCH:
                    return "match closely with";
                case SkosPredicate.RELATED_MATCH:
                    return "are related to";
                default:
                    return "";
            }
        }
    }
}
