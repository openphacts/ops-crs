using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.DataExport
{
    public static class StringExtensions
    {
        /// <summary>
        /// Removes line break characters and escapes backslashes and double quotes.
        /// </summary>
        /// <param name="s">The string for encoding.</param>
        /// <returns>The encoded string.</returns>
        public static string RdfEncode(this string s)
        {
            return !string.IsNullOrEmpty(s) 
                ? s.Replace("\n", "").Replace("\r", "").Replace(@"\", @"\\").Replace("\"", "\\\"")
                : string.Empty;
        }
    }
}
