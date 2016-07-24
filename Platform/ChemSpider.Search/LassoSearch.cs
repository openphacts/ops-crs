using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChemSpider.Database;
using ChemSpider.Data.Database;
using System.Collections;

namespace ChemSpider.Search
{
    public class CSLassoSearch : CSSqlSearch
    {
        public new LassoSearchOptions Options
        {
            get { return base.Options as LassoSearchOptions; }
        }

        protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
        {
            ChemSpiderDB db = new ChemSpiderDB();
            ArrayList row = db.DBU.m_fetchRow(String.Format("select lsf_id, name from ChemSpiderLasso.dbo.lasso_families where shortname = '{0}'", Options.FamilyMin));
            object lsf_id = row[0];
            string lsf_text = row[1].ToString();

            tables.Add("ChemSpiderLasso.dbo.lasso_similarities sim");

            predicates.Add(String.Format("sim.lsf_id = {0}", lsf_id));
            predicates.Add("c.cmp_id = sim.cmp_id");
            predicates.Add(String.Format("sim.simvalue >= {0}", Options.ThresholdMin));

            orderby.Add("sim.simvalue desc");

            visual.Add(String.Format("{0} >= {1}", lsf_text, Options.ThresholdMin));

            foreach ( string shortname in Options.FamilyMax ) {
                row = db.DBU.m_fetchRow(String.Format("select lsf_id, name from ChemSpiderLasso.dbo.lasso_families where shortname = '{0}'", shortname));
                lsf_id = row[0];
                lsf_text = row[1].ToString();
                string id = shortname.Replace("-", "_").Replace(" ", "");

                tables.Add(String.Format("ChemSpiderLasso.dbo.lasso_similarities {0}", id));

                predicates.Add(String.Format("{0}.lsf_id = {1}", id, lsf_id));
                predicates.Add(String.Format("c.cmp_id = {0}.cmp_id", id));
                predicates.Add(String.Format("{0}.simvalue <= {1}", id, Options.ThresholdMax));
                
                visual.Add(String.Format("{0} <= {1}", lsf_text, Options.ThresholdMax));
            }

            return true;
        }
    }
}
