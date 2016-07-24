using RSC.Compounds.Search;
using RSC.Search;

namespace CompoundsWeb.Models
{
	public class SearchModel
	{
		public CompoundsCommonSearchOptions commonOptions { get; set; }
		public CompoundsSearchScopeOptions scopeOptions { get; set; }
		public CompoundsSearchResultOptions resultOptions { get; set; }
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
		public SubStructureSearchOptions searchOptions { get; set; }
	}

	public class SimilarityStructureSearchModel : SearchModel
	{
		public SimilarityStructureSearchOptions searchOptions { get; set; }
	}

	//public class AdvancedSearchModel : SearchModel
	//{
	//	public CSCAdvancedSearchOptions searchOptions { get; set; }
	//}
}