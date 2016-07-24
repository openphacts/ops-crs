using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChemSpider.Search
{
    public class SearchResultStatement
    {
        /// <summary>
        /// Accepts a search results and returns the message for display in the result statement control.
        /// </summary>
        /// <returns>string describing how the results were matched.</returns>
        public static string SearchResultToResultStatement(CSSearchResult res)
        {
            //First get the description
            string match_type_description = SearchResultToDescription(res.ResultMatchType);

            //Add the brackets for display.
            if (match_type_description != string.Empty)
                match_type_description = "(" + match_type_description + ")";

            //Then build the result statement.
            string result_statement = String.Empty;

            //Next format the results based on what information we have.
            switch (res.Status)
            {
                case ERequestStatus.TooManyRecords:
                    result_statement = String.Format("Too many records found, please refine your search. <br/>Search terms: <b>{2}</b><br/>{3}", res.Count, res.Elapsed.TotalSeconds, res.Description, match_type_description);
                    break;
                case ERequestStatus.ResultReady:
                    //Single result.
                    if (res.Count == 1)
                        result_statement = String.Format("Search term: <b>{0}</b> {1}", res.Description, match_type_description);
                    //Multiple results or no results.
                    else if (res.Count > 1 || res.Count == 0)
                        //More results than the selected limit.
                        if (res.Count > res.FoundCount)
                            result_statement = String.Format("{0} hits returned from a total of {1}. <br/>Search terms: <b>{3}</b><br/>{4}", res.FoundCount, res.Count, res.Elapsed.TotalSeconds, res.Description, match_type_description);
                        else
                            result_statement = String.Format("{0} hits found. <br/>Search terms: <b>{2}</b><br/>{3}", res.FoundCount, res.Elapsed.TotalSeconds, res.Description, match_type_description);
                    break;
                case ERequestStatus.Failed:
                    result_statement = String.Format("<img src='images/ico/exclamation.png'/> {0}", res.Message);
                    break;  
                default:
                    //Use the message in other cases.
                    result_statement = res.Message;
                    break;
            }
            return result_statement;
        }

        /// <summary>
        /// Accepts a search result match type and returns the message.
        /// </summary>
        /// <returns>string describing how the results were matched.</returns>
        public static string SearchResultToDescription(ESimpleSearchMatchType matchType)
        {
            string description = string.Empty;
            switch (matchType)
            {
                case ESimpleSearchMatchType.CSID:
                    description = "Found by CSID";
                    break;
                case ESimpleSearchMatchType.Synonym:
                    description = "Found by approved synonym";
                    break;
                case ESimpleSearchMatchType.NonApprovedSynonym:
                    description = "Found by synonym";
                    break;
                case ESimpleSearchMatchType.InChIKey:
                    description = "Found by InChIKey (full match)";
                    break;
                case ESimpleSearchMatchType.InChIKeySkeleton:
                    description = "Found by InChIKey (skeleton match)";
                    break;
                case ESimpleSearchMatchType.ConvertedToInChI:
                    description = "Found by conversion of search term to chemical structure (full match)";
                    break;
                case ESimpleSearchMatchType.ConvertedToInChITautomerStereoMismatch:
                    description = "Found by conversion of search term to chemical structure (tautomer/stereo mismatch)";
                    break;
                case ESimpleSearchMatchType.ConvertedToInChIConnectivityMatch:
                    description = "Found by conversion of search term to chemical structure (connectivity match)";
                    break;
                case ESimpleSearchMatchType.ConvertedToInChISkeletonMatch:
                    description = "Found by conversion of search term to chemical structure (skeleton match)";
                    break;
                case ESimpleSearchMatchType.MolecularFormula:
                    description = "Found by molecular formula";
                    break;
                case ESimpleSearchMatchType.FullTextSynonym:
                    description = "Found by matching substring to any synonym - approximate match!";
                    break;
                case ESimpleSearchMatchType.TokenizedSynonym:
                    description = "Found by splitting search term into separate tokens";
                    break;
                case ESimpleSearchMatchType.TokenizedFullTextNearSynonym:
                    description = "Found by matching nearby substrings to any synonym - approximate match!";
                    break;
                default:
                    break;
            }
            return description;
        }

        /// <summary>
        /// Accepts a search result match type and returns the inline help page.
        /// </summary>
        /// <returns>string describing the inline help identifier for the matchType.</returns>
        public static string SearchResultToInlineHelp(ESimpleSearchMatchType matchType)
        {
            string inlineHelp = string.Empty;
            switch (matchType)
            {
                case ESimpleSearchMatchType.CSID:
                    inlineHelp = "csid";
                    break;
                case ESimpleSearchMatchType.Synonym:
                    inlineHelp = "synonym";
                    break;
                case ESimpleSearchMatchType.NonApprovedSynonym:
                    inlineHelp = "nonapproved_synonym";
                    break;
                case ESimpleSearchMatchType.InChIKey:
                    inlineHelp = "inchikey";
                    break;
                case ESimpleSearchMatchType.InChIKeySkeleton:
                    inlineHelp = "inchikey_skeleton";
                    break;
                case ESimpleSearchMatchType.ConvertedToInChI:
                    inlineHelp = "converted_inchi";
                    break;
                case ESimpleSearchMatchType.ConvertedToInChITautomerStereoMismatch:
                    inlineHelp = "converted_inchi_tautomer";
                    break;
                case ESimpleSearchMatchType.ConvertedToInChIConnectivityMatch:
                    inlineHelp = "converted_inchi_connectivity";
                    break;
                case ESimpleSearchMatchType.ConvertedToInChISkeletonMatch:
                    inlineHelp = "converted_inchi_skeleton";
                    break;
                case ESimpleSearchMatchType.MolecularFormula:
                    inlineHelp = "mf";
                    break;
                case ESimpleSearchMatchType.FullTextSynonym:
                    inlineHelp = "ft_synonym";
                    break;
                case ESimpleSearchMatchType.TokenizedSynonym:
                    inlineHelp = "tokenized_synonym";
                    break;
                case ESimpleSearchMatchType.TokenizedFullTextNearSynonym:
                    inlineHelp = "tokenized_near_synonym";
                    break;
                default:
                    break;
            }
            return inlineHelp;
        }
    }
}
