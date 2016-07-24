using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChemSpider.Search
{
    public class CSElementsSearch : CSSqlSearch
    {
        public new ElementsSearchOptions Options
        {
            get { return base.Options as ElementsSearchOptions; }
        }

        protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
        {
            bool bAdded = false;

            if ( !Options.IncludeAll ) {
                StringBuilder pred = new StringBuilder();
                StringBuilder vis = new StringBuilder();
                Options.IncludeElements.ForEach(el => {
                    pred.AppendFormat(" or ce++.el_{0} > 0", el);
                    vis.AppendFormat(" or {0}", el);
                    bAdded = true;
                });

                if ( bAdded ) {
                    predicates.Add("(" + pred.ToString().Substring(3) + ")");
                    visual.Add("(" + vis.ToString().Substring(4) + ")");
                }
            }
            else {
                Options.IncludeElements.ForEach(el => {
                    predicates.Add(String.Format("ce++.el_{0} > 0", el));
                    visual.Add(String.Format("+{0}", el));
                    bAdded = true;
                });
            }

            Options.ExcludeElements.ForEach(el => {
                predicates.Add(String.Format("(ce++.el_{0} is null OR ce++.el_{0} = 0)", el));
                visual.Add(String.Format("-{0}", el));
                bAdded = true;
            });

            if ( bAdded ) {
                predicates.Add("c.cmp_id = ce++.cmp_id");
                tables.Add("compounds_elements ce++");
            }

            return bAdded;
        }
    }
}
