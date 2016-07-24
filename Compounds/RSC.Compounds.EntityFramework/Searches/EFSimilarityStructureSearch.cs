using RSC.Compounds.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.EntityFramework
{
	public class EFSimilarityStructureSearch : ISimilarityStructureSearch
	{
		/// <summary>
		/// Run similarity structure search
		/// </summary>
		/// <param name="options">Similarity structure search options for search</param>
		/// <param name="commonOptions">Common options</param>
		/// <param name="scopeOptions">Scope options</param>
		/// <param name="resultsOptions">Results options</param>
		/// <returns>List of compounds' IDs that match the search request</returns>
		public IEnumerable<SearchResult> DoSearch(SimilarityStructureSearchOptions options, CompoundsCommonSearchOptions commonOptions, CompoundsSearchScopeOptions scopeOptions, CompoundsSearchResultOptions resultsOptions)
		{
			using (CompoundsContext db = new CompoundsContext())
			{
				string parameters = "";

				switch (options.SimilarityType)
				{
					case SimilarityStructureSearchOptions.ESimilarityType.Tanimoto:
						parameters = "Tanimoto";
						break;
					case SimilarityStructureSearchOptions.ESimilarityType.Tversky:
						parameters = "Tversky";
						if (options.Alpha > 0 || options.Beta > 0)
							parameters += string.Format(" {0} {1}", options.Alpha, options.Beta);

						break;
					case SimilarityStructureSearchOptions.ESimilarityType.Euclidian:
						parameters = "Euclid-sub";
						break;
					default:
						throw new StructureSearchException("Unsupported similarity search type");
				}

				parameters += resultsOptions != null && resultsOptions.Limit > 0 ? string.Format("; TOP {0}", resultsOptions.Limit) : "";

				var res = db.Database.SqlQuery<StructureSearchResult>("exec SearchByMolSim {0}, {1}, {2}", options.Molecule, parameters, options.Threshold).ToList();

				return res;
			}
		}
	}
}
