using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;
using ChemSpider.Database;
using ChemSpider.Data.Database;
using ChemSpider.Utilities;
using System.Web;
using ChemSpider.Molecules;

namespace ChemSpider.Formats
{
    /// <summary>
    /// Summary description for CSData
    /// </summary>
    public class ExportSDF
    {
        private ChemSpiderDB m_db = new ChemSpiderDB();

        public ExportSDF()
        {
        }

        public string GetNoStructureMol(int id)
        {
            string mol = null;
            mol = string.Format("  -MissingMOL_{0}\n{1}\n  0  0  0  0  0  0  0  0  0  0999 V2000\nM  END\n", id, id);
            return mol;
        }

        public string TrimBeginningAndEndOfMol(int id, string mol)
        {
            int i;
            if ( mol == null ) {
                return null;
            }
            while ( mol.StartsWith("  ") != true ) {
                i = mol.IndexOf("\n");
                mol = mol.Substring(i + 1);
            }
            if ( mol.Contains("\n$$$$") ) {
                i = mol.IndexOf("$$$$");
                mol = mol.Substring(0, i);
            }
            return mol;
        }

        public string AddChemSpiderIDAsProperty(int id, string mol)
        {
            if ( mol == null ) {
                return null;
            }
            //adds chemspider id as property
            mol = string.Format("{0}\n> <ChemSpider ID>\n{1}\n\n", mol, id);
            return mol;
        }

        public string GetSuppInfoXmlForSingleCompound(int id)
        {
            Hashtable args = new Hashtable();
            args["cmp_id"] = id;
            string xml = m_db.DBU.m_runSqlProc("cmp_get_supp_info_xml", args, 0).ToString();
            return xml;
        }

        public string TransformUserDataXmltoSDF(string suppprops)
        {
            if ( suppprops == null ) {
                return "";
            }
            try {
                if ( suppprops != "" ) {
                    XmlDocument data = new XmlDocument();
                    data.LoadXml(suppprops);
                    if ( data != null ) {
                        XslCompiledTransform xslt = new XslCompiledTransform();
                        xslt.Load(HttpContext.Current.Server.MapPath("controls/View/supp_props.xsl"));
                        StringWriter sw = new StringWriter();
                        xslt.Transform(data, null, sw);
                        suppprops = sw.ToString();
                    }
                }
                return suppprops;
            }
            catch {
                return "";
            }
        }

        public string ExportCompounds(IEnumerable<int> ids)
        {
            List<SdfRecord> records = ConvertToSDFRecords(ids);

            StringBuilder sb = new StringBuilder();

            foreach ( SdfRecord rec in records )
                sb.Append(rec.ToString());

            return sb.ToString();
        }

        public List<SdfRecord> ConvertToSDFRecords(IEnumerable<int> ids)
        {
            const int BUNCH_SIZE = 500;

            List<SdfRecord> records = new List<SdfRecord>();

            for ( int i = 0; i < ids.Count() / BUNCH_SIZE + 1; i++ ) {
                int offset = i * BUNCH_SIZE;
                string csids = String.Join(",", ids.Skip(offset).Take(Math.Min(ids.Count() - offset, BUNCH_SIZE)));

                if ( csids.Length > 0 ) {
                    DataTable dt = m_db.DBU.FillDataTable(
                        string.Format(@"SELECT  c.cmp_id AS CSID, 
                                            c.PUBCHEM_OPENEYE_MF AS MF, 
                                            c.Molecular_Weight AS MW, 
                                            c.SMILES AS SMILES, 
                                            si.inchi AS InChI,
                                            si.inchi_key AS InChIKey,
                                            s.sdf AS SDF,
                                            ds.ds_count AS 'Data Sources', 
                                            ref.ref_count AS 'References', 
                                            pubmed.n_hits AS PubMed,
                                            rsc.n_hits AS RSC
                                    FROM compounds c 
                                        JOIN v_compound_ds_count ds ON ds.cmp_id = c.cmp_id
                                        JOIN v_compound_ref_count ref ON ref.cmp_id = c.cmp_id
                                        JOIN inchis_std si ON c.cmp_id = si.cmp_id
                                        LEFT JOIN ChemSpiderBlobs.dbo.sdfs_3d s3d ON s3d.cmp_id = c.cmp_id
                                        LEFT JOIN ChemSpiderBlobs.dbo.sdfs s ON s.cmp_id = c.cmp_id
                                        LEFT JOIN compounds_hits pubmed ON pubmed.cmp_id = c.cmp_id AND pubmed.src_id = 1
                                        LEFT JOIN compounds_hits rsc ON rsc.cmp_id = c.cmp_id AND rsc.src_id = 2
                                    WHERE c.cmp_id IN ({0})", csids.ToString()), new { });

                    foreach ( DataRow row in dt.Rows ) {
                        string mol = ZipUtils.ungzip(row["SDF"] as byte[], Encoding.UTF8);

                        if ( !string.IsNullOrEmpty(mol) ) {
                            mol = MolUtils.StripHydrogens(mol);

                            using ( SdfReader reader = new SdfReader(new StringReader(mol)) ) {
                                SdfRecord old_rec = reader.ReadSDFRecord();
                                SdfRecord rec = new SdfRecord();
                                rec.Molecule = old_rec.Molecule;

                                foreach ( DataColumn column in dt.Columns ) {
                                    string name = column.ColumnName;
                                    if ( name != "SDF" )
                                        rec.AddField(name, row[name].ToString());
                                }

                                //Include the canonical url (CSURL) in the compound data.
                                rec.AddField("CSURL", string.Format("http://{0}/Chemical-Structure.{1}.html", HttpContext.Current.Request.ServerVariables["HTTP_HOST"], row["CSID"].ToString()));

                                records.Add(rec);
                            }
                        }
                    }
                }
            }

            return records;
        }
    }
}
