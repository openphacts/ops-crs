using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ChemSpider.Database;
using ChemSpider.Data.Database;
using System.Data.SqlClient;
using System.ComponentModel;
using ChemSpider.Molecules;
using System.Xml.Linq;

namespace ChemSpider.ObjectModel
{
    [DataContract]
    public class Compound : CompoundBase
    {
        protected override Hashtable PullProperties()
        {
            Hashtable properties = new Hashtable();

            using (SqlConnection conn = new SqlConnection(ChemSpiderDB.ConnectionString))
            {
                conn.ExecuteReader(String.Format(@"select	dbo.fGetCompoundTitle(c.cmp_id) as title, 
		                                                    dbo.fGetSysName(c.cmp_id) as sys_name, 
		                                                    PUBCHEM_OPENEYE_MF, 
		                                                    Average_Mass, 
		                                                    Molecular_Weight, 
		                                                    Monoisotopic_Mass, 
		                                                    Nominal_Mass, 
		                                                    ds.ds_count,
		                                                    ref.ref_count, 
		                                                    pubmed.n_hits as pubmed_hits,
		                                                    rsc.n_hits as rsc_hits
                                                      from compounds c 
	                                                    left join v_acdlabs_props ap on c.cmp_id = ap.cmp_id
	                                                    left join v_compound_ds_count ds ON ds.cmp_id = c.cmp_id
	                                                    left join v_compound_ref_count ref ON ref.cmp_id = c.cmp_id
	                                                    left join compounds_hits pubmed ON pubmed.cmp_id = c.cmp_id AND pubmed.src_id = 1
	                                                    left join compounds_hits rsc ON rsc.cmp_id = c.cmp_id AND rsc.src_id = 2
                                                    where c.cmp_id = {0}", CSID),
                    r =>
                    {
                        properties["name"] = r["title"] as string ?? r["sys_name"] as string;
                        properties["sys_name"] = r["sys_name"] as string;
                        properties["MF"] = MolecularFormula.prepareMF(r["PUBCHEM_OPENEYE_MF"] as string);
                        properties["MW"] = r["Molecular_Weight"] is DBNull ? null : r["Molecular_Weight"] as double?;
                        properties["AM"] = r["Average_Mass"] is DBNull ? null : r["Average_Mass"] as Single?;
                        properties["MM"] = r["Monoisotopic_Mass"] is DBNull ? null : r["Monoisotopic_Mass"] as double?;
                        properties["NM"] = r["Nominal_Mass"] is DBNull ? null : r["Nominal_Mass"] as Single?;
                        properties["ds_count"] = Convert.ToInt32(r["ds_count"]);
                        properties["ref_count"] = Convert.ToInt32(r["ref_count"]);
                        properties["pubmed_hits"] = r["pubmed_hits"] is DBNull ? 0 : r["pubmed_hits"] as int?;
                        properties["rsc_hits"] = r["rsc_hits"] is DBNull ? 0 : r["rsc_hits"] as int?;
                    });
            }

            return properties;
        }

        public Compound(int csid) : base(csid)
        {
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("Common compound's name")]
        public string Name
        {
            get { return Properties["name"] as string; }
            set { }
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("Systematic compound's name")]
        public string SysName
        {
            get { return Properties["sys_name"] as string; }
            set { }
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("Molecule formula")]
        public string MF
        {
            get { return Properties["MF"] as string; }
            set { }
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("Molecule weight")]
        public double? MW
        {
            get { return Properties["MW"] as double?; }
            set { }
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("Average Mass")]
        public double? AM
        {
            get { return Properties["AM"] as Single?; }
            set { }
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("Monoisotopic Mass")]
        public double? MM
        {
            get { return Properties["MM"] as double?; }
            set { }
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("Nominal Mass")]
        public double? NM
        {
            get { return Properties["NM"] as Single?; }
            set { }
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("Data Sources Count")]
        public int? DataSourcesCount
        {
            get { return Properties["ds_count"] as int?; }
            set { }
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("References Count")]
        public int? ReferencesCount
        {
            get { return Properties["ref_count"] as int?; }
            set { }
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("Pubmed Hits")]
        public int? PubmedHits
        {
            get { return Properties["pubmed_hits"] as int?; }
            set { }
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("RSC Hits")]
        public int? RSCHits
        {
            get { return Properties["rsc_hits"] as int?; }
            set { }
        }

        private static Reliability decodeReliability(string opinion, bool approved_yn)
        {
            if (opinion == null)
                return Reliability.Uncertaint;
            else if (opinion == "Y")
                return approved_yn ? Reliability.Approved : Reliability.Confirmed;
            else
                return approved_yn ? Reliability.Deleted : Reliability.Rejected;
        }

        protected override IEnumerable<Similarity> PullSimilarities()
        {
            List<Similarity> list = new List<Similarity>();
            using (SqlConnection conn = new SqlConnection(ChemSpiderDB.ConnectionString))
            {
                conn.ExecuteReader(
                    string.Format(@"select cmp_id, cmp_id_to, tanimoto_score 
                                    from compounds_similarity 
                                    where cmp_id = {0} 
                                    order by tanimoto_score desc", CSID),
                    r =>
                    {
                        list.Add(new Similarity
                        {
                            CSID = (int)r["cmp_id_to"],
                            Score = (double)r["tanimoto_score"],
                            SimilarityType = SimilarityType.Tanimoto
                        });
                    });
            }
            return list;
        }

        protected override IEnumerable<Synonym> PullSynonyms()
        {
            List<Synonym> list = new List<Synonym>();
            using (SqlConnection conn = new SqlConnection(ChemSpiderDB.ConnectionString))
            {
                conn.ExecuteReader(
                    String.Format(@"select cs.opinion, cs.approved_yn, y.synonym, y.lang_id1,
                                            (select (select * from v_synonyms_synonyms_flags flags where syn_id = y.syn_id for xml auto, type) for xml path('synonyms-flags'), type) flags
                                        from compounds_synonyms cs join synonyms y on cs.syn_id = y.syn_id 
                                        where cs.deleted_yn = 0 
                                        and y.deleted_yn = 0 
                                        and cs.cmp_id = {0}
                                        and not exists (select *
                                                            from v_synonyms_synonyms_flags ssf
                                                        where ssf.syn_id = y.syn_id
                                                            and name = 'DBID')", CSID),
                    r =>
                    {
                        list.Add(new Synonym
                        {
                            Name = r["synonym"] as string,
                            Reliability = decodeReliability(r["opinion"] is DBNull ? null : r["opinion"] as string, (bool)r["approved_yn"]),
                            LangID = r["lang_id1"] as string,
                            Flags = r["flags"] as string
                        });
                    });
            }
            return list;
        }

        protected override IEnumerable<Identifier> PullIdentifiers()
        {
            List<Identifier> list = new List<Identifier>();
            using (SqlConnection conn = new SqlConnection(ChemSpiderDB.ConnectionString))
            {
                conn.ExecuteReader(String.Format(@"select
                    (select i.inchi from inchis i where i.cmp_id = {0}) as inchi, 
                    (select i.inchi from inchis_std i where i.cmp_id = {0}) as inchi_std, 
                    (select i5.inchi_key from inchis_md5 i5 where i5.cmp_id = {0}) as inchi_key, 
                    (select i.inchi_key from inchis_std i where i.cmp_id = {0}) as inchi_key_std,
                    (select c.SMILES from compounds c where c.cmp_id = {0}) as SMILES", CSID),
                    r =>
                    {
                        list.Add(new Identifier { IdentifierType = IdentifierType.InChI, Version = "v1.02b", Value = r["inchi"] as string });
                        list.Add(new Identifier { IdentifierType = IdentifierType.InChI, Version = "v1.02s", Value = r["inchi_std"] as string });
                        list.Add(new Identifier { IdentifierType = IdentifierType.InChIKey, Version = "v1.02b", Value = r["inchi_key"] as string });
                        list.Add(new Identifier { IdentifierType = IdentifierType.InChIKey, Version = "v1.02s", Value = r["inchi_key_std"] as string });
                        list.Add(new Identifier { IdentifierType = IdentifierType.SMILES, Version = "OEChem", Value = r["SMILES"] as string });
                    });
            }

            return list;
        }

        protected override IEnumerable<Reference> PullReferences(int? dsn_id)
        {
            List<Reference> list = new List<Reference>();
            using (SqlConnection conn = new SqlConnection(ChemSpiderDB.ConnectionString))
            {
                string sql = String.Format(@"select distinct ds.name, s.ext_id, s.ext_url from substances s join ChemUsers..data_sources ds on s.dsn_id = ds.dsn_id where cmp_id = {0}", CSID);

                if (dsn_id != null)
                    sql += " and ds.dsn_id = " + dsn_id;

                conn.ExecuteReader(sql,
                    r =>
                    {
                        Uri uri;
                        Uri.TryCreate(r["ext_url"] is DBNull ? null : r["ext_url"] as string, UriKind.Absolute, out uri);

                        list.Add(new Reference
                        {
                            Source = r["name"] as string,
                            Text = r["ext_id"] as string,
                            Link = uri
                        });
                    });
            }

            return list;
        }

        protected override IEnumerable<DatasourceType> PullDatasources(int? dsn_id, int? col_id, int? dst_id)
        {
            string data_xml = string.Empty;
            string schema_xml = string.Empty;
            string add_info_xml = string.Empty;
            string add_schema_xml = string.Empty;

            (new ChemSpiderDB()).GetDataSourcesXMLForRecord2(CSID, dsn_id, col_id, dst_id, out data_xml, out schema_xml, out add_info_xml, out add_schema_xml);

            XNamespace ns = "http://www.chemspider.com";
            XDocument doc = XDocument.Parse(data_xml);

            return (from dt in doc.Root.Elements(ns + "dt")
                    select new DatasourceType()
                    {
                        ID = dt.Attribute("dst_id") != null ? Convert.ToInt32(dt.Attribute("dst_id").Value) : (int?)null,
                        Name = dt.Attribute("name") != null ? dt.Attribute("name").Value : null,
                        ShortName = dt.Attribute("short_name") != null ? dt.Attribute("short_name").Value : null,
                        Datasources = (from ds in dt.Elements(ns + "ds")
                                       select new Datasource()
                                       {
                                           ID = ds.Attribute("dsn_id") != null ? Convert.ToInt32(ds.Attribute("dsn_id").Value) : 0,
                                           Name = ds.Attribute("name") != null ? ds.Attribute("name").Value : null,
                                           Url = ds.Attribute("ds_url") != null ? ds.Attribute("ds_url").Value : null,
                                           References = (from s in ds.Elements(ns + "ext_id")
                                                         select new Reference()
                                                         {
                                                            Text = s.Attribute("ext_id") != null ? s.Attribute("ext_id").Value : null,
                                                            //Link = s.Attribute("ext_url") != null ? s.Attribute("ext_url").Value : null
                                                         }).ToList()
                                       }).ToList()
                    }).ToList();
        }

        protected override string PullSDF()
        {
            return new CSMolecule(CSID).ToString(true);
        }

        protected override IEnumerable<Blob> PullBlobs()
        {
            List<Blob> list = new List<Blob>();
            using (SqlConnection conn = new SqlConnection(ChemSpiderDB.ConnectionString))
            {
                conn.ExecuteReader(
                    String.Format(@"select spc_id, spc_type, open_yn, blob_type, approved_yn, filename, home_page_url
                                        from spectra
                                        where deleted_yn = 0
                                        and cmp_id = {0}", CSID),
                    r =>
                    {
                        list.Add(new Blob
                        {
                            ID = Convert.ToInt32(r["spc_id"]),
                            BlobType = decodeBlobType(r["blob_type"] as string),
                            SubType = r["spc_type"] as string,
                            Filename = r["filename"] as string,
                            Open = (bool)r["open_yn"],
                            Approved = (bool)r["approved_yn"],
                            HomePageUrl = r["home_page_url"] as string
                        });
                    });
            }

            return list;
        }

        private static BlobType decodeBlobType(string type)
        {
            if (type == "I")
                return BlobType.Image;
            else if (type == "C")
                return BlobType.CIF;
            else
                return BlobType.Spectrum;
        }
    }
}
