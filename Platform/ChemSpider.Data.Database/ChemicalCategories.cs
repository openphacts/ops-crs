using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.Linq;
using ChemSpider.Database;

namespace ChemSpider.Data.Database
{
    public class ChemicalCategories
    {
        public int cmp_id = 0;

        public class Category
        {
            public int cat_id = 0;
            public string name = string.Empty;
            public string description = string.Empty;
            public string url = string.Empty;
        }

        public ChemicalCategories(int cmp_id)
        {
            this.cmp_id = cmp_id;
        }

        public static List<Category> AllCategories
        {
            get
            {
                using (SqlConnection conn = new SqlConnection(ChemSpiderDB.ConnectionString))
                {
                    DataTable result = conn.FillDataTable("select cat_id, name, description, ref_url from categories", new { });

                    IEnumerable<DataRow> rows = result.Rows.Cast<DataRow>();

                    return (from c in rows
                            select new Category()
                            {
                                cat_id = Convert.ToInt32(c["cat_id"]),
                                description = (string)c["description"],
                                name = (string)c["name"],
                                url = (string)c["ref_url"]
                            }).ToList();
                }
            }
        }

        public List<Category> Categories
        {
            get
            {
                using (SqlConnection conn = new SqlConnection(ChemSpiderDB.ConnectionString))
                {
                    DataTable result = conn.FillDataTable(@"select c.cat_id, c.name, c.description, c.ref_url 
                                                        from compound_categories cc inner join categories c on cc.cat_id = c.cat_id
                                                        where cmp_id = @cmp_id", new { cmp_id = cmp_id });

                    IEnumerable<DataRow> rows = result.Rows.Cast<DataRow>();

                    return (from c in rows
                            select new Category()
                            {
                                cat_id = Convert.ToInt32(c["cat_id"]),
                                description = (string)c["description"],
                                name = (string)c["name"],
                                url = (string)c["ref_url"]
                            }).ToList();
                }
            }
        }

        public static List<ChemicalCategories> CategoriesArray(int[] CSIDs)
        {
            return (from id in CSIDs
                    select new ChemicalCategories(id) ).ToList();
        }
    }
}
