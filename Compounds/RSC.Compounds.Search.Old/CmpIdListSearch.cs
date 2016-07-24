using System;
using System.Collections.Generic;
using ChemSpider.Search;

namespace RSC.Compounds.Search.Old
{
    public class CSCCmpIdListSearch : CSCmpIdListSearch
    {
        protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
        {
            bool bAdded = false;

            if (Options.CmpIdList.Count > 0)
            {
                predicates.Add(String.Format("c.csid IN ({0})", string.Join(",", Options.CmpIdList)));
                bAdded = true;
            }

            return bAdded;
        }
    }
}
