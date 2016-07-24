using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Xml;
using ChemSpider.Database;
using ChemSpider.Data.Database;
using System.Data;
using System.IO;
using System.Diagnostics;
using ChemSpider.Molecules;
using MoleculeObjects;
using OpenEyeNet;

namespace ChemSpider.Formats
{
	public class ExportUtils
	{
		private static ChemSpiderDB s_db = new ChemSpiderDB();
		private static ChemSpiderBlobsDB s_bdb = new ChemSpiderBlobsDB();
		
		public static void ExportXml(int min_id, int max_id, string file, bool bWithSynonymsOnly)
		{
			Dictionary<string, string> langs = new Dictionary<string, string>();
			langs.Add("F", "French");
			langs.Add("G", "German");
			langs.Add("S", "Spanish");
			langs.Add("L", "Latin");
			langs.Add("C", "Czech");
			langs.Add("P", "Polish");
			langs.Add("R", "Russian");
			langs.Add("D", "Dutch");
			langs.Add("N", "Netherlands");
			langs.Add("I", "Italian");

			if ( String.IsNullOrEmpty(file) )
				file = String.Format("ChemSpider-{0}-{1}.xml", min_id, max_id);

			using ( XmlWriter writer = XmlWriter.Create(file) ) {
				writer.WriteStartElement("compounds");
				writer.WriteAttributeString("min_csid", min_id.ToString());
				writer.WriteAttributeString("max_csid", max_id.ToString());

				using ( SqlConnection conn = new SqlConnection(ChemSpiderDB.ConnectionString) ) {
					conn.Open();

					SqlCommand cmd1 = new SqlCommand("select inchi, SMILES, (select count(*) from compounds_synonyms where cmp_id = @cmp_id and approved_yn = 1 and opinion = 'Y' and deleted_yn = 0) as cnt from inchis i join compounds c on c.cmp_id = i.cmp_id where c.cmp_id = @cmp_id", conn);
					cmd1.Parameters.Add("@cmp_id", SqlDbType.Int);

					SqlCommand cmd3 = new SqlCommand("select inchi from inchis_std where cmp_id = @cmp_id", conn);
					cmd3.Parameters.Add("@cmp_id", SqlDbType.Int);

					SqlCommand cmd2 = new SqlCommand("select y.synonym, cs.common_name_yn, y.cas_rn_yn, y.lang_id from compounds_synonyms cs join synonyms y on cs.syn_id = y.syn_id where cs.cmp_id = @cmp_id and approved_yn = 1 and opinion = 'Y' and cs.deleted_yn = 0 and y.deleted_yn = 0", conn);
					cmd2.Parameters.Add("@cmp_id", SqlDbType.Int);

					for ( int cmp_id = min_id; cmp_id <= max_id; cmp_id++ ) {
						cmd1.Parameters["@cmp_id"].Value = cmp_id;
						SqlDataReader r1 = cmd1.ExecuteReader();
						if ( !r1.Read() || bWithSynonymsOnly && ( (int)r1[2] ) == 0 )
							r1.Close();
						else {
							string inchi = r1[0] as string;
							string smiles = r1[1] as string, smiles_version = "ACDLabs";
							r1.Close();

							writer.WriteStartElement("compound");
							writer.WriteAttributeString("CSID", cmp_id.ToString());

							writer.WriteStartElement("InChI");	// <InChI v1.02b
							writer.WriteAttributeString("version", "1.02b");
							writer.WriteString(inchi);
							writer.WriteEndElement();

							cmd3.Parameters["@cmp_id"].Value = cmp_id;
							SqlDataReader r3 = cmd3.ExecuteReader();
							try {
								if ( r3.Read() ) {
									writer.WriteStartElement("InChI");	// <InChI v1.02b
									writer.WriteAttributeString("version", "1.02s");
									writer.WriteString(r3[0] as string);
									writer.WriteEndElement();
								}
							}
							finally {
								r3.Close();
							}

							string sdf = new CSMolecule(cmp_id).ct;
							if ( !String.IsNullOrEmpty(sdf) ) {
								// File.WriteAllText(cmp_id.ToString() + ".sdf", sdf);
								smiles = MolUtils.MolToSMILES(sdf);
								smiles_version = "OE-1.6.1-iso";
							}

							writer.WriteStartElement("SMILES");	// <SMILES
							writer.WriteAttributeString("version", smiles_version);
							writer.WriteString(smiles);
							writer.WriteEndElement();

							cmd2.Parameters["@cmp_id"].Value = cmp_id;
							using ( SqlDataReader r2 = cmd2.ExecuteReader() ) {
								while ( r2.Read() ) {
									writer.WriteStartElement("synonym");	// <synonym
									if ( (bool)r2[1] )
										writer.WriteAttributeString("common-name", "yes");
									if ( (bool)r2[2] )
										writer.WriteAttributeString("cas-no", "yes");
									if ( r2[3] as string != "E" )
										writer.WriteAttributeString("lang", langs[r2[3] as string]);
									writer.WriteString(r2[0] as string);
									writer.WriteEndElement();
								}
							}

							writer.WriteEndElement();	// compound
							writer.Flush();
						}
					}
				}
				writer.WriteEndElement();	// compounds
			}
		}

		public static bool DumpSdfFile(string file, IEnumerable<int> range, bool bStripH)
		{
			bool empty = true;
			OpenEyeUtility oeutil = OpenEyeUtility.GetInstance();
			using ( StreamWriter ofs = new StreamWriter(file) ) {
				foreach ( int cmp_id in range ) {
					string sdf = s_bdb.getSdf(cmp_id);
					if ( !String.IsNullOrWhiteSpace(sdf) ) {
						if ( bStripH )
							sdf = oeutil.StripHydrogens(sdf);
						SdfRecord rec = SdfRecord.FromString(sdf);
						if ( rec.Properties != null )
							rec.Properties.Clear();
						rec.AddField("CSID", cmp_id.ToString());
						ofs.Write(rec.ToString());
						empty = false;
					}
				}
			}
			
			return !empty;
		}

		public static bool DumpSdfFile(string file, int min_id, int max_id, bool bStripH)
		{
			return DumpSdfFile(file, Enumerable.Range(min_id, max_id - min_id + 1), bStripH);
		}

		public static void DumpSdfFiles(string file_base, int min_id, int max_id, int chunk_size, bool bStripH)
		{
			if (chunk_size == 0) {
				DumpSdfFile(String.Format("{0}-{1}-{2}.sdf", file_base, min_id, max_id), min_id, max_id, bStripH);
			}
			else {
				for (int i = min_id, j = 0; i < max_id; i += chunk_size, ++j) {
					string file_name = String.Format("{0}-{1}-{2}-{3}.sdf", file_base, min_id, max_id, j);
					DumpSdfFile(file_name, i, i + chunk_size, bStripH);
				}
			}
		}

		/* REFACTORING
		 * Commented out to get rid of Sdf class
		public static void dump_tautomerCapableSDF(int count, string inchi_pattern, string outputFile)
		{
			string ChemSpiderBlobsConnectionString;
			if (ConfigurationManager.ConnectionStrings["ChemSpiderBlobsConnectionString"] == null)
			{
				throw new ApplicationException("Missing Connection String variable from Web.Config.");
			}
			else ChemSpiderBlobsConnectionString = ConfigurationManager.ConnectionStrings["ChemSpiderBlobsConnectionString"].ConnectionString;
			ChemSpiderBlobsDB db = new ChemSpiderBlobsDB();
			string query = "select top " + count + " i.cmp_id, i.inchi from [ChemSpider].[dbo].[inchis_std] i inner join [ChemSpiderBlobs].[dbo].[sdfs] b on b.cmp_id=i.cmp_id where i.inchi like '%" + inchi_pattern + "%'";
			DataTable dt1 = executeScalarSQLWithDataTableReturned(query,ChemSpiderBlobsConnectionString);
			
			using (StreamWriter sw = new StreamWriter(outputFile))
			{
				foreach (DataRow dr in dt1.Rows)
				{
					int cmp_id = Convert.ToInt32(dr["cmp_id"]);
					string s_sdf = db.getSdf(cmp_id);
					Sdf obj_sdf = new Sdf(s_sdf);
					List<ChemSpider.Molecules.Molecule> list_gm = obj_sdf.molecules;
					ChemSpider.Molecules.Molecule mol = null;
					if (list_gm.Count > 0)
						mol = list_gm[0];//single record per sdf
					
					sw.Write(mol.ToString());
				}
			}        
		}
		 * */

		public static DataTable executeScalarSQLWithDataTableReturned(string query,string connectionString)
		{
			DataTable dt = new DataTable();
			if (query != null && query.Length > 0)
			{
				SqlCommand cmd = new SqlCommand(query);
				SqlConnection con = new SqlConnection(connectionString);
				cmd.Connection = con;
				SqlDataAdapter adapter = new SqlDataAdapter(cmd);

				try
				{
					con.Open();
					adapter.Fill(dt);
				}

				finally
				{
					con.Close();
				}
			}
			return dt;
		}


		public static void DumpSmiles(string file_base, int min_id, int max_id, int chunk_size, bool bStripH)
		{
			int chunk = 0, count = 0;
			string file_name = String.Format("{0}-{1}-{2}{3}.smi", file_base, min_id, max_id, chunk_size == 0 ? "" : "-" + chunk.ToString());
			StreamWriter ofs = new StreamWriter(file_name);

			using ( SqlConnection conn = new SqlConnection(ChemSpiderDB.ConnectionString) ) {
				conn.Open();

				string command = String.Format("select cmp_id from ChemSpider..compounds where cmp_id not in (select cmp_id from ChemSpiderBlobs..epi where cmp_id >= {0} and cmp_id < {1}) and cmp_id >= {0} and cmp_id < {1}", min_id, max_id);
				SqlCommand sel_cmd = new SqlCommand(command, conn);
				sel_cmd.CommandTimeout = 0;

				using ( SqlDataReader reader = sel_cmd.ExecuteReader() ) {
					while ( reader.Read() ) {
						int cmp_id = (int)reader["cmp_id"];
						string sdf = new CSMolecule(cmp_id).ToString(bStripH);
						if ( !String.IsNullOrEmpty(sdf) ) {
							ofs.WriteLine(MolUtils.MolToSMILES(sdf));
						}
						if ( ++count == chunk_size && chunk_size != 0 ) {
							ofs.Close();
							if ( cmp_id != max_id - 1 ) {
								++chunk;
								count = 0;
								file_name = String.Format("{0}-{1}-{2}-{3}.smi", file_base, min_id, max_id, chunk.ToString());
								ofs = new StreamWriter(file_name);
							}
						}
					}
				}
			}

			ofs.Close();
		}

		public static void ExportMeSH(string file)
		{
			XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
			using ( XmlWriter writer = XmlWriter.Create(file, settings) )
			using ( SqlConnection conn = new SqlConnection(ChemSpiderDB.ConnectionString) ) {
				writer.WriteStartElement("ChemSpider-MeSH");

				conn.ExecuteReader("select cmp_id, ConceptUI, isnull(ChemSpider.dbo.fGetCompoundTitle(cmp_id), ChemSpider.dbo.fGetSysName(cmp_id)) as name, Concept from ChemSpiderBlobs..mesh_concepts c join ChemSpiderBlobs..mesh_concept_compounds cc on c.cpt_id = cc.cpt_id order by cmp_id",
					r => {
						writer.WriteStartElement("record");

						writer.WriteAttributeString("CSID", r["cmp_id"].ToString());
						writer.WriteAttributeString("ConceptUI", r["ConceptUI"].ToString());
						writer.WriteElementString("CSName", r["name"].ToString());
						writer.WriteRaw(r["Concept"].ToString());

						writer.WriteEndElement();
					});

				writer.WriteEndElement();
			}
		}

		public static void CSSPExportBlobs(string mime)
		{
			using ( SqlConnection conn = new SqlConnection(ChemSpiderSynthesisDB.ConnectionString) ) {
				conn.ExecuteReader(String.Format("select * from articles_blobs where content_type = '{0}'", mime), r => {
					File.WriteAllBytes(String.Format("{0}{1}", r["ab_id"], Path.GetExtension(r["filename"] as string)), r["data"] as byte[]);
				});
			}
		}
	}
}
