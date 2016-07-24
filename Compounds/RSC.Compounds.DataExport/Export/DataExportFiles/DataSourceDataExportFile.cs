using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using RSC.Datasources;

namespace RSC.Compounds.DataExport
{
	/// <summary>
	/// Used for exporting a data export file based on a particular Data Source.
	/// </summary>
	public class DataSourceDataExportFile : DataExportFile
	{
		protected IDataSourceExport DataSourceExport { get; private set; }

		/// <summary>
		/// Constructor is passed the DataSource to generate the export for.
		/// </summary>
		/// <param name="dataSource">The DataSource to generate the export for.</param>
		public DataSourceDataExportFile(IDataSourceExport exp)
		{
			DataSourceExport = exp;
		}

		/// <summary>
		/// Writes out the prefixes at the start of a Turtle file.
		/// </summary>
		/// <param name="baseUri"></param>
		/// <returns></returns>
		public IEnumerable<string> PrefixLines(string baseUri, Dictionary<string,Uri> prefixSet)
		{
			//Get a copy of the issues prefixes.
			var dict = new Dictionary<string, Uri>(prefixSet);
			//Add the base uri prefix.
			dict.Add(Turtle.RDF_URI_PREFIX.ToLower(), new Uri(Turtle.RDF_URI.ToString() + "/"));  //Re-use the prefix as the alias.

			//Add the prefix for this void file with the empty prefix.
			dict.Add("", new Uri(String.Format("{0}#", baseUri).ToString()));

			//Output the prefixes.
			foreach (var p in dict)
			{
				yield return String.Format("@prefix {0}: <{1}> .", p.Key, p.Value);
			}
		}
	}
}
