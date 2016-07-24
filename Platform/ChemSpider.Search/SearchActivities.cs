using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using ChemSpider.Database;
using ChemSpider.Data.Database;
using System.Data.SqlClient;
using ChemSpider.Security;
using ChemSpider.Utilities;

namespace ChemSpider.Search
{
    public class CSSearchActivity : CodeActivity
    {
        public InArgument<string> Options { get; set; }
        public InArgument<string> CommonOptions { get; set; }
        public InArgument<string> DataSourceOptions { get; set; }
        public InArgument<string> ResultOptions { get; set; }
        public InArgument<string> Sandbox { get; set; }
        public InArgument<string> Rid { get; set; }
        public InArgument<string> SearchHandler { get; set; }

        protected override void Execute(System.Activities.CodeActivityContext context)
        {
            SearchOptions options = XamlUtils.Xaml2Object(Options.Get(context)) as SearchOptions;
            CommonSearchOptions commonOptions = XamlUtils.Xaml2Object(CommonOptions.Get(context)) as CommonSearchOptions;
            SearchScopeOptions scopeOptions = XamlUtils.Xaml2Object(DataSourceOptions.Get(context)) as SearchScopeOptions;
            SearchResultOptions resultOptions = XamlUtils.Xaml2Object(ResultOptions.Get(context)) as SearchResultOptions;

            Sandbox sandbox = XamlUtils.Xaml2Object(Sandbox.Get(context)) as Sandbox;
            Request result = Request.loadFromTransaction(Rid.Get(context));
            CSSearch search = Activator.CreateInstance(Type.GetType(SearchHandler.Get(context))) as CSSearch;

            search.SetOptions(options, commonOptions, scopeOptions, resultOptions);
            search.Run(sandbox, new CSRequestSearchResult(result));
        }
    }

    public class CSSSSSearchConnectionKeeper : CodeActivity
    {
        protected override void Execute(System.Activities.CodeActivityContext context)
        {
            // do some substructure search and then keep connection alive

            ChemSpiderSSSDB sssdb = new ChemSpiderSSSDB();
            SqlConnection conn = sssdb.DBU.getConnection(ChemSpiderSSSDB.ConnectionString);
            while ( true ) {
                int i = 0;
                sssdb.SubstructureSearch(
                    "CC1=C2[C@@]([C@]([C@H]([C@@H]3[C@]4([C@H](OC4)C[C@@H]([C@]3(C(=O)[C@@H]2OC(=O)C)C)O)" + /* taxol */
                    "OC(=O)C)OC(=O)c5ccccc5)(C[C@@H]1OC(=O)[C@H](O)[C@@H](NC(=O)c6ccccc6)c7ccccc7)O)(C)C",
                        string.Empty);
                while ( i++ < 20 ) {    // Send SSS query every 5 minutes
                    conn.ExecuteScalar<DateTime>("select getdate() now");
                    System.Threading.Thread.Sleep(15000);   // Ping every 15 seconds... just in case - I'm not sure how connections pools is being managed
                }
            }
        }
    }
}
