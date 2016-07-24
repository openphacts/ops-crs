using System.Configuration;
using System;
namespace ChemSpider.Sql
{
    public partial class ChemSpiderSqlDataContext : System.Data.Linq.DataContext
    {
        [Flags]
        enum ESynonymMergeOptions
        {
            Replace = 0,
            NoConflict = 1,
            NoApprovedAssociations = 2,
            SkipErrors = 4
        }

        public ChemSpiderSqlDataContext()
            : base(ConfigurationManager.ConnectionStrings["ChemSpiderConnectionString"].ConnectionString)
        {
            OnCreated();
        }
    }
}
