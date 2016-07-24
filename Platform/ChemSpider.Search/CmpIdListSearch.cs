using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChemSpider.Database;
using ChemSpider.Data.Database;
using System.Data;

namespace ChemSpider.Search
{
    public class CSCmpIdListSearch : CSSqlSearch
    {
        public new CmpIdListSearchOptions Options
        {
            get { return base.Options as CmpIdListSearchOptions; }
        }

        protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
        {
            bool bAdded = false;

            if (Options.CmpIdList.Count > 0)
            {
                string sql_in = string.Empty;
                foreach (int id in Options.CmpIdList) 
                { 
                    sql_in += (id + ","); 
                }

                sql_in = sql_in.Substring(0, sql_in.Length - 1);
                predicates.Add(String.Format("c.cmp_id IN ({0})", sql_in));
                bAdded = true;
            }

            return bAdded;
        }
    }
}
