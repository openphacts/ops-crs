using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

//using ChemSpider.CVSP.Compounds;
using BatchWS =  RSC.CVSP.Compounds.com.chemspider.www.BatchNames1;
using InChIWS = RSC.CVSP.Compounds.com.chemspider.www.InChIWS;
using MassSpecWS = RSC.CVSP.Compounds.com.chemspider.www.MassSpecAPI;

using System.Web.Services.Protocols;
using MoleculeObjects;

namespace RSC.CVSP.Compounds
{
    /// <summary>
    /// Provides static methods for generating GenericMolecules, Molecules and CSMolecules from sources specified by
    /// ChemicalFormat.
    /// </summary>
    public static class WebServiceWrapper
    {
        static System.Net.WebProxy s_proxyObject = null;

        static public void setProxy(string proxy)
        {
            if(!String.IsNullOrEmpty(proxy))
                s_proxyObject = new System.Net.WebProxy(proxy);
        }
        
        // some miscellaneous static methods
        // TODO: consider these properly and move them somewhere more appropriate
        /// <summary>
        /// Wrapper for InChI to CSID web service.
        /// </summary>
		static public string InChIToCSID(string inchi)
		{
			InChIWS.InChI i = new InChIWS.InChI();
			if (s_proxyObject != null)
				i.Proxy = s_proxyObject;

			string csid = i.InChIToCSID(inchi);
			return csid;
		}

		public static string CSIDToSMILES(string csid)
		{
			MassSpecWS.MassSpecAPI msapi = new MassSpecWS.MassSpecAPI();
			if (s_proxyObject != null)
				msapi.Proxy = s_proxyObject;
			MassSpecWS.ExtendedCompoundInfo rec_info = msapi.GetExtendedCompoundInfo(Int32.Parse(csid), ConfigurationManager.AppSettings["chemspider_service_key"]);
			return rec_info.SMILES;
		}

		static public IEnumerable<int> SynonymToCSIDs(string s)
		{
			var b = new BatchWS.BatchNames();
			if (s_proxyObject != null)
				b.Proxy = s_proxyObject;
			int[] wsresult = b.synonym2CSIDs(s, ConfigurationManager.AppSettings["chemspider_service_key"]);
			return wsresult;
		}

		public static IEnumerable<string> CSID2Synonym(string csid)
		{
			var b = new BatchWS.BatchNames();
			if (s_proxyObject != null)
				b.Proxy = s_proxyObject;
			return b.GetStructureSynonymsByCSID(Convert.ToInt32(csid), ConfigurationManager.AppSettings["chemspider_service_key"]).OrderBy(l => l);
		}

        /// <summary>
        /// Uses ACD N2S to generate a structure from the name.
        /// </summary>
        public static Molecule FromName(string s)
        {
            Dictionary<string, string> ACDMessageReplacements = new Dictionary<string, string>()
            {
                { "Warning: Structure is generated from SMILES notation$$$$", 
                    "Structure has been generated from a SMILES string." },
                { "Warning: Structure is generated from INChI$$$$", 
                    "Structure has been generated from an InChI string."}
            };
            
            var b = new BatchWS.BatchNames();
            if (s_proxyObject != null)
                b.Proxy = s_proxyObject;
            string ctfile = MoleculeFactory.RestoreWhitespace(b.N2strws(s, ConfigurationManager.AppSettings["chemspider_service_key"]));
            
            if (!String.IsNullOrEmpty(ctfile))
            {
                GenericMolecule gm = MoleculeFactory.FromMolV2000(ctfile);
                var newprops = new Dictionary<string, List<string>>() 
				{ 
					{ 
						Convert.ToString(SDTagOptions.MESSAGE), 
						new List<string> 
						{ 
							ACDMessageReplacements.ContainsKey(gm.FirstProperty("message"))
								? ACDMessageReplacements[gm.FirstProperty("message")] : "Structure has been generated from a chemical name." 
						}
					}
				};
                Dictionary<string, List<string>> finalprops = gm.RemoveProperty("message").Merge(newprops);
                return new Molecule(gm.Headers, gm.IndexedAtoms, gm.IndexedBonds, finalprops);
            }
            else return null;
        }
    }
}
