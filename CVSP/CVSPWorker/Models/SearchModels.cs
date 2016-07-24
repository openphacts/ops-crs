using RSC.CVSP.Search;

namespace CVSPWorker.Models
{
    public class SearchModel
    {
        public CVSPCommonSearchOptions commonOptions { get; set; }
        public CVSPSearchScopeOptions scopeOptions { get; set; }
        public CVSPSearchResultOptions resultOptions { get; set; }
    }

    public class CVSPRecordsSearchModel : SearchModel
    {
        public CVSPRecordsSearchOptions searchOptions { get; set; }
    }
}
