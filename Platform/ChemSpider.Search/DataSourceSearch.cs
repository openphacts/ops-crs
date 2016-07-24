using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChemSpider.Database;
using ChemSpider.Data.Database;
using System.Data;

namespace ChemSpider.Search
{
    public class CSDataSourceSearch : CSSqlSearch
    {
        public new DataSourceSearchOptions Options
        {
            get { return base.Options as DataSourceSearchOptions; }
        }

        protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
        {
            bool bAdded = false;

            if (Options.DataSources.Count() > 0 || Options.DataSourceTypes.Count() > 0)
            {
                StringBuilder subQuery = new StringBuilder("JOIN (SELECT DISTINCT ");
                //If we are not looking in focused libraries then no need to join to substances, it is faster to use compounds_datasources.
                subQuery.Append("cds.cmp_id FROM compounds_datasources cds");

                //There are 2 cases for Data Sources and Data Source Types, when we have types or ids or just ids.              
                if (Options.DataSources.Count() > 0 || Options.DataSourceTypes.Count() > 0)
                    subQuery.Append(" JOIN ChemUsers.dbo.data_sources ds ON cds.dsn_id = ds.dsn_id ");

                if (Options.DataSourceTypes.Count() > 0)
                {
                    subQuery.Append(" JOIN ChemUsers.dbo.ds_ds_types ddt ON ds.dsn_id = ddt.dsn_id ");
                }

                subQuery.Append(" WHERE ");

                // Data Sources 
                if (Options.DataSources.Count() > 0)
                {
                    //Map the list of dsns to a list of dsn_ids.
                    ChemSpiderDB db = new ChemSpiderDB();
                    string dsns = String.Join(",", Options.DataSources.Select(s => String.Format("'{0}'", s.Replace("'", "''"))));
                    string dsn_ids = String.Join(",", db.DBU.FetchColumn<int>(String.Format("SELECT dsn_id FROM ChemUsers.dbo.data_sources WHERE name IN ({0}) AND hidden_yn = 0", dsns)));
                    if (dsn_ids != string.Empty)
                    {
                        subQuery.Append(String.Format("ds.dsn_id IN ({0}) AND ", dsn_ids));
                        visual.Add("DATA_SOURCE in (" + dsns + ")");
                    }
                }

                // Data Source Types
                if (Options.DataSourceTypes.Count() > 0)
                {
                    //Map the list of dsts to a list of dst_ids.
                    ChemSpiderDB db = new ChemSpiderDB();
                    string dsts = String.Join(",", Options.DataSourceTypes.Select(s => String.Format("'{0}'", s.Replace("'", "''"))));
                    string dst_ids = String.Join(",", db.DBU.FetchColumn<int>(String.Format("SELECT dst_id FROM ChemUsers.dbo.ds_types WHERE name IN ({0})", dsts)));
                    subQuery.Append(String.Format("ddt.dst_id IN ({0}) AND ", dst_ids));
                    visual.Add("DATA_SOURCE_TYPE in (" + dsts + ")");
                }

                //Remove the extra " AND"
                subQuery = new StringBuilder(subQuery.ToString().Substring(0, subQuery.Length - 4));               
                //Add the join clause.
                subQuery.Append(") s ON c.cmp_id = s.cmp_id");

                tables.Add(subQuery.ToString());
                bAdded = true;
            }

            // Focused Libraries
            if (Options.FocusedLibraries.Count() > 0)
            {
                //Map the list of focused libraries to a list of col_ids.
                ChemSpiderDB db = new ChemSpiderDB();
                string cols = String.Join(",", Options.FocusedLibraries.Select(s => String.Format("'{0}'", s.Replace("'", "''"))));
                string col_ids = String.Join(",", db.DBU.FetchColumn<int>(String.Format("SELECT col_id FROM ChemUsers.dbo.data_collections WHERE name IN ({0})", cols)));
                tables.Add(String.Format("JOIN (SELECT DISTINCT s.cmp_id FROM substances s WHERE s.col_id IN ({0}) AND s.deleted_yn = 0) fl ON c.cmp_id = fl.cmp_id", col_ids));
                visual.Add("DATA_COLLECTION in (" + cols + ")");
            }

            // X Sections
            if ( Options.XSections.Count() > 0 ) {

                //Get the list type sec_ids.
                ChemSpiderDB db = new ChemSpiderDB();
                string secs = String.Join(",", Options.XSections.Select(s => String.Format("'{0}'", s.Replace("'", "''"))));
                List<int> sec_ids_list = db.DBU.FetchColumn<int>(String.Format("SELECT sec_id FROM xsections WHERE name IN ({0}) AND sec_type = 'L'", secs));
                if(sec_ids_list.Count > 0)
                {
                    string sec_ids = String.Join(",", sec_ids_list);
                    tables.Add(String.Format("JOIN (SELECT DISTINCT cl.cmp_id FROM compounds_lists cl WHERE cl.sec_id IN ({0})) xsl ON xsl.cmp_id = c.cmp_id", sec_ids));
                }

                //Deal with the query type XSections.
                List<string> sec_ids_query = db.DBU.FetchColumn<string>(String.Format("SELECT query FROM xsections WHERE name IN ({0}) AND sec_type = 'Q'", secs));
                if (sec_ids_query.Count > 0)
                {
                    foreach (string query in sec_ids_query)
                        predicates.Add(String.Format("c.cmp_id IN ({0})", query));
                }
                
                visual.Add("XSECTIONS in (" + secs + ")");
                bAdded = true;
            }


            return bAdded;
        }
    }
}
