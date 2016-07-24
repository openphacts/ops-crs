using ChemSpider.Search;
using RSC.Compounds.Search.Old;

namespace CompoundsWeb.Models.Old
{
	public class SearchModel
	{
		public CommonSearchOptions commonOptions { get; set; }
		public CSCSearchScopeOptions scopeOptions { get; set; }
		public SearchResultOptions resultOptions { get; set; }
	}

	public class SimpleSearchModel : SearchModel
	{
		public SimpleSearchOptions searchOptions { get; set; }
	}

	public class ExactStructureSearchModel : SearchModel
	{
		public ExactStructureSearchOptions searchOptions { get; set; }
	}

	public class SubstructureSearchModel : SearchModel
	{
		public SubstructureSearchOptions searchOptions { get; set; }
	}

	public class SimilarityStructureSearchModel : SearchModel
	{
		public SimilaritySearchOptions searchOptions { get; set; }
	}

	public class AdvancedSearchModel : SearchModel
	{
		public CSCAdvancedSearchOptions searchOptions { get; set; }
	}
}