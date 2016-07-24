using ChemSpider.Search;

namespace RSC.Compounds.Search.Old
{
    public class CSCSearchFactory
    {
        public static CSSearch GetExactStructureSearch()
        {
            return CSSearchFactory.CreateWrappedInstance(typeof(CSCExactStructureSearch));
        }

        public static CSSearch GetSubstructureSearch()
        {
            return CSSearchFactory.CreateWrappedInstance(typeof(CSCSubstructureSearch));
        }

        public static CSSearch GetSimilarityStructureSearch()
        {
            return CSSearchFactory.CreateWrappedInstance(typeof(CSCSimilarityStructureSearch));
        }

        public static CSSearch GetSimpleSearch()
        {
            return CSSearchFactory.CreateWrappedInstance(typeof(CSCSimpleSearch));
        }

        public static CSSearch GetIntrinsicPropertiesSearch()
        {
            return CSSearchFactory.CreateWrappedInstance(typeof(CSCIntrinsicPropertiesSearch));
        }

		//public static CSSearch GetAdvancedSearch()
		//{
		//	CSCAdvancedSearch result = new CSCAdvancedSearch();
		//	result.DataSourceSearch = null;
		//	result.ElementsSearch = null;
		//	result.IntrinsicPropertiesSearch = new CSCIntrinsicPropertiesSearch();
		//	result.KeywordSearch = null;
		//	result.LassoSearch = null;
		//	result.PredictedPropertiesSearch = null;
		//	result.StructureSearch = new CSCExactStructureSearch();
		//	result.SuppInfoSearch = null;
		//	result.CmpIdListSearch = new CSCCmpIdListSearch();
		//	result.NMRFeaturesSearch = new CSCNMRFeaturesSearch();
		//	return CSSearchFactory.WrapInstance(result);
		//}
    }
}
