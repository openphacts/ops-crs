using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.ComponentModel;
using OpenEyeNet;
using ChemSpider.Database;
using ChemSpider.Data.Database;
using ChemSpider.Utilities;
using InChINet;
using com.ggasoftware.indigo;

namespace ChemSpider.Molecules
{
	public static class ChemIdUtils
	{
        [Serializable]
        [DataContract]
        public class ConvertOptions
        {
            /**
             *              Name/Term       SMILES          InChI           InChIKey        MOL             CSID
             * Name/Term    x               Name2Smiles     Name2InChI      Name2InChIKey   Term2Mol        Name2CSID
             *                              Term2SMILES                                     Name2Mol        
             * SMILES       SMILES2Names    x               Smiles2InChi    SMILES2InChIKey Smiles2Mol      SMILES2CSID
             * InChI        InChI2Name      InChI2SMILES    x               InChI2InChIKey  InChI2Mol       InChi2CSID
             * InChIKey     InChIKey2Name   InChIKey2SMILES InChIKey2InChI  x               InChIKey2Mol    InChiKey2CSID
             * MOL          Mol2Name        Mol2SMILES      Mol2InChI       Mol2InChIKey    x               Mol2CSID
             * CSID         CSID2Name       CSID2SMILES     CSID2InChI      CSID2InChIKey   CSID2Mol        x
             * 
             * */
            public enum EDirection
            {
                Name2Mol,
                Smiles2Mol,
                Smiles2InChi,
                Name2Smiles,
                Term2Mol,
                Term2SMILES,
                InChiKey2CSID,
                InChi2CSID,

                // New ones (not implemented)
                Name2InChI,
                Name2InChIKey,
                Name2CSID,
                SMILES2Names,
                SMILES2InChIKey,
                SMILES2CSID,
                InChI2Name,
                InChI2SMILES,
                InChI2InChIKey,
                InChI2Mol,
                InChIKey2Name,
                InChIKey2SMILES,
                InChIKey2InChI,
                InChIKey2Mol,
                Mol2Name,
                Mol2SMILES,
                Mol2InChI,
                Mol2InChIKey,
                Mol2CSID,
                CSID2Name,
                CSID2SMILES,
                CSID2InChI,
                CSID2InChIKey,
                CSID2Mol
            }

            [DataMember]
            public EDirection Direction { get; set; }

            [DataMember]
            public string Text { get; set; }
        }

        private static ChemSpiderDB s_csdb
        {
            get { return new ChemSpiderDB(); }
        }

        private static ChemSpiderBlobsDB s_csbdb
        {
            get { return new ChemSpiderBlobsDB(); }
        }

		static public string anyId2InChI(string any_id, out string message, InChIFlags flags)
		{
			return anyId2InChI(any_id, out message, flags, true, true);
		}

		static public string anyId2InChI(string any_id, out string message, InChIFlags flags, bool bUseACDN2S, bool bUseUnreliableSynonyms)
		{
			string mol;
			if ( name2str(any_id, out mol, bUseACDN2S, bUseUnreliableSynonyms) > 0 ) {
				message = "Converted";
				return InChINet.InChIUtils.mol2InChI(mol, flags);
			}

			message = "Cannot convert";
			return String.Empty;
		}


		static public int name2str(string any_id, out string mol, bool bUseACDN2S, bool bUseUnreliableSynonyms)
		{
			Dictionary<string, N2SResult> result = new Dictionary<string, N2SResult>();
			result[any_id] = null;
			name2strbat(result, bUseACDN2S, bUseUnreliableSynonyms);
			if ( result[any_id] == null ) {
				mol = null;
				return 0;
			}

			mol = result[any_id].mol;
			return result[any_id].confidence;
		}

        [DataContract]
		public class N2SResult
		{
            [DataMember]
            [Description("Specify how accurate convertion was. Possible results: 100 - unambiguous conversion; 0 - can't convert; between 0 and 100 - converted sucessfully but with some limitations, in this case 'message' property will return some additional information")]
			public int confidence { get; set; }
            [DataMember]
            [Description("Converted molecule")]
            public string mol { get; set; }
            [DataMember]
            [Description("Returns some information, warning or error message that got during the conversion")]
            public string message { get; set; }
			public N2SResult()
			{
			}
			public N2SResult(int aConfidence, string aMol)
			{
				confidence = aConfidence;
				mol = aMol;
			}
		}

        public static int ? ResolveValidatedSynonym(string any_id, bool nullIfMultiple)
        {
            Hashtable args = new Hashtable();
            args["@id"] = any_id;
            System.Data.DataTable t = s_csdb.DBU.m_fillDataTable(
                @"select top 2 cs.cmp_id 
                    from compounds_synonyms cs join synonyms y on cs.syn_id = y.syn_id 
                   where y.synonym = @id 
                     and cs.opinion = 'Y' 
                     and cs.approved_yn = 1 
                     and cs.deleted_yn = 0 
                     and y.deleted_yn = 0 
                  order by dbo.fSynonymMatchScore(cs.cmp_id, cs.syn_id) desc", args);
            if (t.Rows.Count == 0 || nullIfMultiple && t.Rows.Count > 1)
                return null;

            return t.Rows[0]["cmp_id"] as int ?;
        }

		public static List<int> ResolveValidatedSynonym(string any_id)
		{
			Hashtable args = new Hashtable();
			args["@id"] = any_id;
			return s_csdb.DBU.m_fetchColumn<int>(
				@"select cs.cmp_id 
                    from compounds_synonyms cs join synonyms y on cs.syn_id = y.syn_id 
                   where y.synonym = @id 
                     and cs.opinion = 'Y' 
                     and cs.approved_yn = 1 
                     and cs.deleted_yn = 0 
                     and y.deleted_yn = 0 
                  order by dbo.fSynonymMatchScore(cs.cmp_id, cs.syn_id) desc", args, 0);
		}

		static public void name2strbat(Dictionary<string, N2SResult> any_ids, bool bUseACDN2S, bool bUseUnreliableSynonyms)
		{
			string[] keys = new string[any_ids.Count];
			//
			any_ids.Keys.CopyTo(keys, 0);

			bool bACDN2SUsed = false;

			foreach ( string key in keys ) {
				if ( any_ids[key] != null && !String.IsNullOrEmpty(any_ids[key].mol) )
					continue;

				string any_id = key.Trim();
				string sdf;
				if ( any_id.StartsWith("InChI=", StringComparison.InvariantCultureIgnoreCase) )
				{
					// Let's use ACDLabs if we're alllowed to
					if ( bUseACDN2S && !bACDN2SUsed ) {
						acd_n2s_batch(any_ids);
						bACDN2SUsed = true;
						if ( any_ids[key] != null && !String.IsNullOrEmpty(any_ids[key].mol) )
							continue;
					}

					sdf = InChI.InChIToMol(any_id);
					if ( !string.IsNullOrEmpty(sdf) ) {
                        any_ids[key] = new N2SResult(50, OpenEyeUtility.GetInstance().Clean(sdf, false));
						continue;
					}
				}
				else if ( InChIUtility.GetInstance().isValidInChIKey(any_id) ) {
					int cmp_id = Convert.ToInt32(s_csdb.DBU.m_querySingleValue(String.Format("select cmp_id from inchis_md5 where inchi_key = '{0}'", any_id)));
					if ( cmp_id != 0 ) {
                        string m = new CSMolecule(cmp_id).ToString(true);
						any_ids[key] = new N2SResult(m != String.Empty ? 100 : 0, m);
						continue;
					}
				}
				else {
					int? cmp_id = ResolveValidatedSynonym(any_id, false);
                    if (cmp_id != null) {
                        string m = new CSMolecule((int)cmp_id).ToString(true);
						if ( !String.IsNullOrEmpty(m) ) {
							any_ids[key] = new N2SResult(100, m);
							continue;
						}
					}

					/* TODO: Dictionary must be cleaned up - it's a mess now
					cmp_id = DbUtil.querySingleValue("select top 1 dc.cmp_id from dictionary d join dictionary_compounds dc on d.dic_id = dc.dic_id where name = '" + Utility.prepare4sql(any_id) + "' order by dc.confidence desc") as int?;
					if (cmp_id != null) {
						string m = m_blobs_db.getSdf((int)cmp_id, true, false);
						if (!String.IsNullOrEmpty(m)) {
							any_ids[key] = new N2SResult(90, m);
							continue;
						}
					}*/

					ArrayList res = s_csdb.DBU.m_fetchRow("select top 1 ds.confidence, s.sdf from dictionary d join dictionary_sdfs ds on d.dic_id = ds.dic_id join sdfs s on ds.sdf_id = s.sdf_id where d.name = '" + DbUtil.prepare4sql(any_id) + "' order by ds.confidence desc");
					if ( res.Count > 0 && ( res[1] as byte[] ) != null ) {
                        any_ids[key] = new N2SResult((int)res[0], ZipUtils.ungzip(res[1] as byte[], Encoding.UTF8));
						continue;
					}

					if ( bUseACDN2S && !bACDN2SUsed &&
						 s_csdb.DBU.m_querySingleValue("select top 1 1 from dictionary where name = '" + DbUtil.prepare4sql(any_id) + "' and invalid_yn = 1") == null ) {
						acd_n2s_batch(any_ids);
						bACDN2SUsed = true;
					}

					if ( any_ids[key] != null && !String.IsNullOrEmpty(any_ids[key].mol) )
						continue;

                    sdf = OpenEyeUtility.GetInstance().SMILESToMol(any_id);
					if ( !string.IsNullOrEmpty(sdf) ) {
						any_ids[key] = new N2SResult(100, sdf);
						continue;
					}

					if ( any_ids[key] == null || String.IsNullOrEmpty(any_ids[key].mol) ) { // if ACD N2S has not done good (default mols values)
                        sdf = OpenEyeUtility.GetInstance().NameToMol(any_id);
						if ( !string.IsNullOrEmpty(sdf) ) {
							addDictionaryEntry(any_id, sdf, 90);
							any_ids[key] = new N2SResult(90, sdf);
							continue;
						}

						if ( bUseUnreliableSynonyms ) {
							cmp_id = s_csdb.DBU.m_querySingleValue(
								"select top 1 cs.cmp_id " +
								"from compounds_synonyms cs join synonyms y on cs.syn_id = y.syn_id " +
								"where (cs.opinion is null or cs.opinion <> 'N') and y.synonym = '" + DbUtil.prepare4sql(any_id) + "' and cs.deleted_yn = 0 and y.deleted_yn = 0 " +
								"order by dbo.fSynonymMatchScore(cs.cmp_id, cs.syn_id) desc") as int?;
							if ( cmp_id != null ) {
                                string m = new CSMolecule((int)cmp_id).ToString(true);
								if ( !String.IsNullOrEmpty(m) ) {
									any_ids[key] = new N2SResult(50, m);
									continue;
								}
							}
						}
					}

                    try
                    {
                        Hashtable args = new Hashtable();
                        args.Add("name", any_id);
                        s_csdb.DBU.m_runSqlProc("AddUnresolvableDictionaryEntry", args);
                    }
                    catch
                    {
                        // Don't let it out - it's just an optimization trick
                    }
				}
			}
		}

		private static void acd_n2s_batch(Dictionary<string, N2SResult> any_ids)
		{
			string[] keys = new string[any_ids.Count];
			any_ids.Keys.CopyTo(keys, 0);

			List<KeyValuePair<string, int>> conv_res = null;
			try {
				conv_res = new List<KeyValuePair<string, int>>(MolExtProc.n2sbat(keys));
			}
			catch {
				// TODO: report
			}
			if ( conv_res != null ) {
				for ( int i = 0; i < conv_res.Count; ++i ) {
					if ( String.IsNullOrEmpty(conv_res[i].Key) )
						any_ids[keys[i]] = new N2SResult(0, String.Empty);
					else {
						addDictionaryEntry(keys[i], conv_res[i].Key, conv_res[i].Value);
						any_ids[keys[i]] = new N2SResult(conv_res[i].Value, conv_res[i].Key);
					}
				}
			}
		}

		private static void addDictionaryEntry(string any_id, string mol, int confidence)
		{
			try {
                string[] inchis = InChINet.InChIUtils.mol2inchiinfo(mol, InChIFlags.Default);	// old style InChI
				Hashtable args = new Hashtable();
				args.Add("inchi_key", inchis[1]);
				args.Add("name", any_id);
				args.Add("confidence", confidence);
                args.Add("mol", ZipUtils.gzip(mol, Encoding.UTF8));
				s_csdb.DBU.m_runSqlProc("AddDictionaryEntry", args);
			}
			catch {
				// Suppress errors while adding to database - we can live with that
			}
		}

        static Indigo _indigo = new Indigo();

		public static string smiles2mol(string smiles)
		{
            string mol = OpenEyeUtility.GetInstance().SMILESToMol(smiles);
            if ( String.IsNullOrEmpty(mol) ) {
                lock ( _indigo ) {
                    IndigoObject io = _indigo.loadMolecule(smiles);
                    if ( io != null )
                        mol = io.molfile();
                }
            }
            return mol;
		}

		public static string id2mol(string id, string id_type = null)
		{
            if ( String.IsNullOrEmpty(id_type) ) {
                if ( id_type.StartsWith("InChI=", StringComparison.InvariantCultureIgnoreCase) )
                    id_type = "inchi";
            }
			switch ( id_type ) {
				case "chemical-name":
					return name2mol(id);
				case "inchi":
					return InChI.InChIToMol(id);
				case "smiles":
					return smiles2mol(id);
				default:
					throw new ArgumentException(string.Format("ID type '{0}' not supported", id_type));
			}
		}

		public static string name2mol(string name)
		{
            string mol;
            name2str(name, out mol, true, false);
            return String.IsNullOrWhiteSpace(mol) ? null : mol;
		}

		public static string mol2name(string mol)
		{
            using (var file = new TempFile(null, "sdf")) {
                File.WriteAllText(file.FullPath, mol);
                MolExtProc.namebat(file.FullPath);
                var sdfdata = SdfUtils.retrieveSdfData(file.FullPath, new List<string>() { "IUPACName"});
                return sdfdata.Item2["IUPACName"].First();
            }
		}
	}
}
