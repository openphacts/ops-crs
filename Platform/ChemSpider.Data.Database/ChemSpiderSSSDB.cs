using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data;
using System.Text;
using ChemSpider.Utilities;
using System.Diagnostics;
using ChemSpider.Database;

namespace ChemSpider.Data.Database
{
	public class ChemSpiderSSSDB : ChemSpiderBaseDB
	{
		public static string ConnectionString
		{
			get
			{
				return ConfigurationManager.ConnectionStrings["ChemSpiderSSSConnectionString"] == null ?
					null :
                    ConfigurationManager.ConnectionStrings["ChemSpiderSSSConnectionString"].ConnectionString;
			}
		}

		public static string RO_ConnectionString
		{
			get
			{
                return ConfigurationManager.ConnectionStrings["ChemSpiderSSSROConnectionString"] == null ?
					ConnectionString :
                    ConfigurationManager.ConnectionStrings["ChemSpiderSSSROConnectionString"].ConnectionString;
			}
		}

		protected override string ConnString
		{
			get
			{
				return ConnectionString;
			}
		}

		protected override string RO_ConnString
		{
			get
			{
				return RO_ConnectionString;
			}
		}

        protected override string DatabaseName
        {
            get { return "ChemSpiderSSS"; }
        }

		public ChemSpiderSSSDB()
		{
			
		}

		public List<int> SubstructureSearch(string smiles, string options)
		{
            using (SqlConnection conn = new SqlConnection(RO_ConnString))
            {
                Trace.TraceInformation("exec SearchByMolSub {0}, {1}", smiles, options);
                return conn.FetchColumn<int>("exec SearchByMolSub @smiles, @options", new { smiles = smiles, options = options ?? "" });
            }
		}

        public DataTable SubstructureSearchWithSimilarity(string smiles, string options)
        {
            using (SqlConnection conn = new SqlConnection(RO_ConnString))
            {
                Trace.TraceInformation("exec SearchByMolSub {0}, {1}", smiles, options);
                return conn.FillDataTable("exec SearchByMolSub @smiles, @options", new { smiles = smiles, options = options });
            }
        }

        public DataTable SubstructureSearchBySMARTSWithSimilarity(string smarts, string options)
        {
            using (SqlConnection conn = new SqlConnection(RO_ConnString))
            {
                Trace.TraceInformation("exec SearchBySMARTSSub {0}, {1}", smarts, options);
                return conn.FillDataTable("exec SearchBySMARTSSub @smarts, @options", new { smarts = smarts, options = options });
            }
        }

        public List<int> SimilaritySearch(string smiles, string metrics, float threshold)
        {
            using (SqlConnection conn = new SqlConnection(RO_ConnString))
            {
                Trace.TraceInformation("exec SearchByMolSim {0}, {1}, {2}", smiles, metrics, threshold);
                return conn.FetchColumn<int>("exec SearchByMolSim @smiles, @metrics, @threshold", new { smiles = smiles, metrics = metrics, threshold = threshold });
            }
        }

        public DataTable SimilaritySearchWithSimilarity(string smiles, string metrics, float threshold)
        {
            using (SqlConnection conn = new SqlConnection(RO_ConnString))
            {
                Trace.TraceInformation("exec SearchByMolSim {0}, {1}, {2}", smiles, metrics, threshold);
                return conn.FillDataTable("exec SearchByMolSim @smiles, @metrics, @threshold", new { smiles = smiles, metrics = metrics, threshold = threshold });
            }
        }
	}

    public class ChemSpiderMLSSSDB : ChemSpiderSSSDB
    {
        public static string RO_ML_ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["ChemSpiderMLSSSROConnectionString"] == null ?
                    RO_ConnectionString :
                    ConfigurationManager.ConnectionStrings["ChemSpiderMLSSSROConnectionString"].ConnectionString;
            }
        }

        protected override string RO_ConnString
        {
            get
            {
                return RO_ML_ConnectionString;
            }
        }
    }

    public class ChemSpiderMISSSDB : ChemSpiderSSSDB
    {
        public static string RO_MI_ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["ChemSpiderMISSSROConnectionString"] == null ?
                    RO_ConnectionString :
                    ConfigurationManager.ConnectionStrings["ChemSpiderMISSSROConnectionString"].ConnectionString;
            }
        }

        protected override string RO_ConnString
        {
            get
            {
                return RO_MI_ConnectionString;
            }
        }
    }

    public class ChemSpiderPSSSSDB : ChemSpiderSSSDB
    {
        public static string RO_MI_ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["ChemSpiderSSSROPSConnectionString"] == null ?
                    RO_ConnectionString :
                    ConfigurationManager.ConnectionStrings["ChemSpiderSSSROPSConnectionString"].ConnectionString;
            }
        }

        protected override string RO_ConnString
        {
            get
            {
                return RO_MI_ConnectionString;
            }
        }
    }
}
