using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChemSpider.Security;

namespace ChemSpider.Search
{
    /*public class CSAppFabricSearch : CSSearch
    {
        CSSearch m_Search;

        public CSAppFabricSearch(CSSearch search)
        {
            m_Search = search;
        }

        public override void Run(Sandbox sandbox, CSSearchResult result)
        {
            Action<Sandbox, CSSearchResult> run = new Action<Sandbox, CSSearchResult>(RunSearchOnAppFabric);
            run.BeginInvoke(sandbox, result, null, null);
        }

        protected void RunSearchOnAppFabric(Sandbox sandbox, CSSearchResult result)
        {
            try
            {
                BPF.Search.SearchOptions options = null;
                BPF.Search.CommonSearchOptions commonOptions = null;
                BPF.Search.SearchResultOptions resultOptions = null;
                BPF.Search.SearchScopeOptions scopeOptions = null;

                if (Options is ChemSpider.Search.SubstructureSearchOptions)
                    options = new BPF.Search.SubstructureSearchOptions
                    {
                        Molecule = (Options as SubstructureSearchOptions).Molecule,
                        MatchTautomers = (Options as SubstructureSearchOptions).MatchTautomers
                    };
                else if(Options is ChemSpider.Search.SimilaritySearchOptions)
                    options = new BPF.Search.SimilaritySearchOptions
                    {
                        Molecule = (Options as SimilaritySearchOptions).Molecule,
                        SimilarityType = (BPF.Search.SimilaritySearchOptions.ESimilarityType)((Options as SimilaritySearchOptions).SimilarityType),
                        Threshold = (Options as SimilaritySearchOptions).Threshold
                    };

                if (ScopeOptions != null)
                {
                    scopeOptions = new BPF.Search.SearchScopeOptions
                    {
                        DataSources = ScopeOptions.DataSources,
                        DataSourceTypes = ScopeOptions.DataSourceTypes,
                        XSections = ScopeOptions.XSections
                    };
                }

                if (CommonOptions != null)
                {
                    commonOptions = new BPF.Search.CommonSearchOptions
                    {
                        Complexity = (BPF.Search.CommonSearchOptions.EComplexity)CommonOptions.Complexity,
                        HasPatents = CommonOptions.HasPatents,
                        HasSpectra = CommonOptions.HasSpectra,
                        Isotopic = (BPF.Search.CommonSearchOptions.EIsotopic)CommonOptions.Isotopic
                    };
                }

                if (ResultOptions != null)
                {
                    resultOptions = new BPF.Search.SearchResultOptions
                    {
                        Length = ResultOptions.Length,
                        Limit = ResultOptions.Limit,
                        SortOrder = ResultOptions.SortOrder,
                        Start = ResultOptions.Start
                    };
                }
 
                BPF.Search.SearchParameters parameters = new BPF.Search.SearchParameters()
                {
                    SearchOptions = options,
                    ScopeOptions = scopeOptions,
                    ResultOptions = resultOptions,
                    CommonOptions = commonOptions
                };

                BPFSearchService.SearchServiceClient client = new BPFSearchService.SearchServiceClient();

                result.Found = new ResultList(EResultObjectType.Compound, client.StartSearch(parameters));
                result.Description = m_Search.Description;
                result.Status = ERequestStatus.ResultReady;
                result.Progress = 1;
                result.Message = "Finished";
                result.Count = result.Found.Count;
            }
            catch (Exception ex)
            {
                result.Status = ERequestStatus.Failed;
                result.Message = ex.Message;
            }
            finally
            {
                result.Update();
            }
        }
    }*/
}
