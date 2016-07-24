using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChemSpider.Security;

namespace ChemSpider.Search
{
    public class CSAdvancedSearch : CSSqlSearch
    {
        public CSSSSSearch SSSSearch { get; set; }
        public CSCmpIdListSearch CmpIdListSearch { get; set; }
        public CSExactStructureSearch StructureSearch { get; set; }
        public CSKeywordSearch KeywordSearch { get; set; }
        public CSElementsSearch ElementsSearch { get; set; }
        public CSIntrinsicPropertiesSearch IntrinsicPropertiesSearch { get; set; }
        public CSPredictedPropertiesSearch PredictedPropertiesSearch { get; set; }
        public CSDataSourceSearch DataSourceSearch { get; set; }
        public CSLassoSearch LassoSearch { get; set; }
        public CSSuppInfoSearch SuppInfoSearch { get; set; }

        public new AdvancedSearchOptions Options
        {
            get { return base.Options as AdvancedSearchOptions; }
        }

        public override void SetOptions(SearchOptions options, CommonSearchOptions common, SearchScopeOptions scopeOptions, SearchResultOptions resultOptions)
        {
            if (!SearchOptions.IsNullOrEmpty((options as AdvancedSearchOptions).StructureSearchOptions))
            {
                if ((options as AdvancedSearchOptions).StructureSearchOptions is ExactStructureSearchOptions)
                    StructureSearch.SetOptions((options as AdvancedSearchOptions).StructureSearchOptions, common, scopeOptions, resultOptions);
                else if ((options as AdvancedSearchOptions).StructureSearchOptions is SubstructureSearchOptions)
                {
                    SSSSearch = SubstructureSearchInstance();
                    SSSSearch.SetOptions((options as AdvancedSearchOptions).StructureSearchOptions, common, scopeOptions, resultOptions);
                }
                else if ((options as AdvancedSearchOptions).StructureSearchOptions is SimilaritySearchOptions)
                {
                    SSSSearch = SimilarityStructureSearchInstance();
                    SSSSearch.SetOptions((options as AdvancedSearchOptions).StructureSearchOptions, common, scopeOptions, resultOptions);
                }
            }

            if (!SearchOptions.IsNullOrEmpty((options as AdvancedSearchOptions).SubstructureSearchOptions))
            {
                SSSSearch = SubstructureSearchInstance();
                SSSSearch.SetOptions((options as AdvancedSearchOptions).SubstructureSearchOptions, common, scopeOptions, resultOptions);
            }
            else if (!SearchOptions.IsNullOrEmpty((options as AdvancedSearchOptions).SimilaritySearchOptions))
            {
                SSSSearch = SimilarityStructureSearchInstance();
                SSSSearch.SetOptions((options as AdvancedSearchOptions).SimilaritySearchOptions, common, scopeOptions, resultOptions);
            }
            else if (!SearchOptions.IsNullOrEmpty((options as AdvancedSearchOptions).ExactStructureSearchOptions))
            {
                StructureSearch.SetOptions((options as AdvancedSearchOptions).ExactStructureSearchOptions, common, scopeOptions, resultOptions);
            }

            if (!SearchOptions.IsNullOrEmpty((options as AdvancedSearchOptions).KeywordSearchOptions))
                KeywordSearch.SetOptions((options as AdvancedSearchOptions).KeywordSearchOptions, common, scopeOptions, resultOptions);

            if (!SearchOptions.IsNullOrEmpty((options as AdvancedSearchOptions).ElementsSearchOptions))
                ElementsSearch.SetOptions((options as AdvancedSearchOptions).ElementsSearchOptions, common, scopeOptions, resultOptions);

            if (!SearchOptions.IsNullOrEmpty((options as AdvancedSearchOptions).IntrinsicPropertiesSearchOptions))
                IntrinsicPropertiesSearch.SetOptions((options as AdvancedSearchOptions).IntrinsicPropertiesSearchOptions, common, scopeOptions, resultOptions);

            if (!SearchOptions.IsNullOrEmpty((options as AdvancedSearchOptions).PredictedPropertiesSearchOptions))
                PredictedPropertiesSearch.SetOptions((options as AdvancedSearchOptions).PredictedPropertiesSearchOptions, common, scopeOptions, resultOptions);

            if (!SearchOptions.IsNullOrEmpty((options as AdvancedSearchOptions).DataSourceSearchOptions))
                DataSourceSearch.SetOptions((options as AdvancedSearchOptions).DataSourceSearchOptions, common, scopeOptions, resultOptions);

            if (!SearchOptions.IsNullOrEmpty((options as AdvancedSearchOptions).LassoSearchOptions))
                LassoSearch.SetOptions((options as AdvancedSearchOptions).LassoSearchOptions, common, scopeOptions, resultOptions);

            if (!SearchOptions.IsNullOrEmpty((options as AdvancedSearchOptions).SuppInfoSearchOptions))
                SuppInfoSearch.SetOptions((options as AdvancedSearchOptions).SuppInfoSearchOptions, common, scopeOptions, resultOptions);

            if (!SearchOptions.IsNullOrEmpty((options as AdvancedSearchOptions).CmpIdListSearchOptions))
                CmpIdListSearch.SetOptions((options as AdvancedSearchOptions).CmpIdListSearchOptions, common, scopeOptions, resultOptions);
            
            base.SetOptions(options, common, scopeOptions, resultOptions);
        }

        protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
        {
            bool bAdded = false;

            // Exact Structure Search.
            if (!SearchOptions.IsNullOrEmpty(Options.StructureSearchOptions))
            {
                if (Options.StructureSearchOptions is ExactStructureSearchOptions)
                    bAdded = StructureSearch.GetSqlAndSubstitute(predicates, tables, orderby, visual, columns);
                else
                    // Substructure / Similarity Search - just add the description.
                    visual.Add(SSSSearch.Description);
            }

            // Identifier
            if (!SearchOptions.IsNullOrEmpty(Options.KeywordSearchOptions))
                bAdded = KeywordSearch.GetSqlAndSubstitute(predicates, tables, orderby, visual, columns)
                    || bAdded;

            // Elements
            if (!SearchOptions.IsNullOrEmpty(Options.ElementsSearchOptions))
                bAdded = ElementsSearch.GetSqlAndSubstitute(predicates, tables, orderby, visual, columns)
                    || bAdded;

            // Properties
            if (!SearchOptions.IsNullOrEmpty(Options.IntrinsicPropertiesSearchOptions))
                bAdded = IntrinsicPropertiesSearch.GetSqlAndSubstitute(predicates, tables, orderby, visual, columns)
                    || bAdded;

            // Calculated Properties
            if (!SearchOptions.IsNullOrEmpty(Options.PredictedPropertiesSearchOptions))
                bAdded = PredictedPropertiesSearch.GetSqlAndSubstitute(predicates, tables, orderby, visual, columns)
                    || bAdded;

            // Data Source or Data Source Type (Category)
            if (!SearchOptions.IsNullOrEmpty(Options.DataSourceSearchOptions))
                bAdded = DataSourceSearch.GetSqlAndSubstitute(predicates, tables, orderby, visual, columns)
                    || bAdded;

            // LASSO
            if (!SearchOptions.IsNullOrEmpty(Options.LassoSearchOptions))
                bAdded = LassoSearch.GetSqlAndSubstitute(predicates, tables, orderby, visual, columns)
                    || bAdded;

            //Supplementary Information
            if (!SearchOptions.IsNullOrEmpty(Options.SuppInfoSearchOptions))
                bAdded = SuppInfoSearch.GetSqlAndSubstitute(predicates, tables, orderby, visual, columns)
                    || bAdded;

            //Add the CmpIdListOptions - which is specific to AdvancedSearch.
            if (!SearchOptions.IsNullOrEmpty(Options.CmpIdListSearchOptions))
            {
                bAdded = CmpIdListSearch.GetSqlAndSubstitute(predicates, tables, orderby, visual, columns)
                    || bAdded;
            }

            return bAdded;
        }

        /// <summary>
        /// We are overriding Run so we can run 2 search - the normal SQL Search and also a Substructure/Similarity Search.
        /// Then we combine the results.
        /// </summary>
        public override void Run(Sandbox sandbox, CSSearchResult result)
        {
            //Is this a combined search?
            if (SSSSearch != null)
            {
                //Store the old limit.
                int limit = ResultOptions.Limit;

                //Set the Limit to 10000 for the first part (but only if there are other options).
                if (!SearchOptions.IsNullOrEmpty(Options.SuppInfoSearchOptions) ||
                    !SearchOptions.IsNullOrEmpty(Options.DataSourceSearchOptions) ||
                    !SearchOptions.IsNullOrEmpty(Options.ElementsSearchOptions) ||
                    !SearchOptions.IsNullOrEmpty(Options.IntrinsicPropertiesSearchOptions) ||
                    !SearchOptions.IsNullOrEmpty(Options.KeywordSearchOptions) ||
                    !SearchOptions.IsNullOrEmpty(Options.LassoSearchOptions) ||
                    !SearchOptions.IsNullOrEmpty(Options.PredictedPropertiesSearchOptions))
                    {

                        SSSSearch.ResultOptions.Limit = 10000;
                    }

                //Run the SSS Search.
                CSRequestSearchResult sss_result = new CSRequestSearchResult();
                SSSSearch.Run(sandbox, sss_result);

                //If we got some matches then we can run the next search.
                if (sss_result.Status == ERequestStatus.ResultReady && sss_result.FoundCount > 0)
                {
                    //Set the FoundList as one of the options for the SQL search.
                    Options.CmpIdListSearchOptions = new CmpIdListSearchOptions(sss_result.Found.ToList());

                    //Set the limit back to what it should be.
                    ResultOptions.Limit = limit;
                    SetOptions(Options, CommonOptions, ScopeOptions, ResultOptions);

                    //Run the SQL Search with the new parameters set.
                    base.Run(sandbox, result);
                }
                else
                {
                    //No point running the second search if we got no results.
                    result.Progress = 1;
                    result.Status = ERequestStatus.ResultReady;
                    result.Found = new ResultList();
                    result.Message = "Finished";
                    result.Update();
                }
            }
            else
            {
                //If not a combined then just run the SQL Search like normal.
                base.Run(sandbox, result);
            }
        }

        protected virtual CSSubstructureSearch SubstructureSearchInstance()
        {
            return new CSSubstructureSearch();
        }

        protected virtual CSSimilarityStructureSearch SimilarityStructureSearchInstance()
        {
            return new CSSimilarityStructureSearch();
        }
    }
}
