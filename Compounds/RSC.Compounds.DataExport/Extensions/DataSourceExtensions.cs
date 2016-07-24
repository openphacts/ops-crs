using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSC.Datasources;

namespace RSC.Compounds.DataExport
{
    public static class DataSourceExtensions
    {
        /// <summary>
        /// Returns a label for a Data Source which is Turtle-safe.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <returns>The Turtle-safe label.</returns>
        public static string GetLabel(this DataSource dataSource, bool upperCase = false)
        {
            string ret = dataSource.Name.Replace(" ", "_");
            return upperCase ? ret.ToUpper() : ret.ToLower();
        }
    }
}
