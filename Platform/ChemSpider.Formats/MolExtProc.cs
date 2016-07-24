using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices; 
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

using ChemSpider.Utilities;
using InChINet;

namespace ChemSpider.Molecules
{
	public static class MolExtProc
	{
        public static string WorkingDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

		static public bool namebat(string sdf_file)
		{
            return namebat(sdf_file, string.Empty);
		}

        static public bool namebat(string sdf_file, string argsKeySuffix)
        {
            string program = ConfigurationManager.AppSettings.Get("name_program");
            string argsKey = "name_arguments";
            if (!string.IsNullOrEmpty(argsKeySuffix))
                argsKey += "_" + argsKeySuffix;
            string arguments = ConfigurationManager.AppSettings.Get(argsKey);
            arguments = arguments.Replace("%1", "\"" + sdf_file + "\"");
            ProcessStartInfo psi = new ProcessStartInfo(program, arguments);
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.WorkingDirectory = WorkingDirectory;
            using (ChildProcess p = new ChildProcess(psi)) {
                p.Process.WaitForExit();
                return p.Process.ExitCode == 0;
            }
        }

        /// <summary>
        /// Returns boolean as it modifies the sdf file in situ.
        /// </summary>
        public static bool namebat_all(string path)
        {
            bool r = namebat(path);
            if (r) {
                r = namebat(path, "french");
            }
            if (r) {
                r = namebat(path, "german");
            }
            return r;
        }

        static public bool pchbat(string sdf_file)
		{
			string program = ConfigurationManager.AppSettings.Get("pcb_program");
			string arguments = ConfigurationManager.AppSettings.Get("pcb_arguments");
			arguments = arguments.Replace("%1", "\"" + sdf_file + "\"");
			ProcessStartInfo psi = new ProcessStartInfo(program, arguments);
            psi.WorkingDirectory = WorkingDirectory;
			psi.CreateNoWindow = true;
			psi.UseShellExecute = false;
            using (ChildProcess p = new ChildProcess(psi)) {
                p.Process.WaitForExit();
                return p.Process.ExitCode == 0;
            }
        }

		

		static public string n2s(string any_id)
		{
			if ( ConfigurationManager.AppSettings["csws_url"] == null )
				return local_n2s(any_id);
			else {
                throw new NotImplementedException();
				// com.chemspider.ws.CSWS csws = new com.chemspider.ws.CSWS();
				// csws.Url = ConfigurationManager.AppSettings["csws_url"];
				// return csws.n2s(any_id);
			}
		}

		private static string local_n2s(string any_id)
		{
			string program = ConfigurationManager.AppSettings.Get("n2s_program");
			string arguments = ConfigurationManager.AppSettings.Get("n2s_arguments");
            string core = Utility.RandomValue().ToString();
			string txt_path = Path.Combine(Path.GetTempPath(), core + ".txt");
			File.WriteAllText(txt_path, any_id.Trim());
			string sdf_path = Path.Combine(Path.GetTempPath(), core + ".sdf");

			arguments = arguments.Replace("%1", "\"" + txt_path + "\"");
			arguments = arguments.Replace("%2", "\"" + sdf_path + "\"");
			arguments += " -DICT+ -SMILES+";

			try {
				ProcessStartInfo psi = new ProcessStartInfo(program, arguments);
				psi.CreateNoWindow = true;
				psi.UseShellExecute = false;
				Process proc = Process.Start(psi);
				if ( !proc.WaitForExit(10000) ) {   // 10 seconds max
					proc.Kill();
					return String.Empty;
				}
				else if ( proc.ExitCode != 0 ) {
					return String.Empty;
				}

                var sdfData = SdfUtils.retrieveSdfData(sdf_path, new List<string>() { "message" });
                return sdfData.Item2.First().Value.First().StartsWith("Error:") ? String.Empty : sdfData.Item1;
			}
			finally {
				new FileInfo(txt_path).Delete();
				new FileInfo(sdf_path).Delete();
			}
		}

		static public IEnumerable<KeyValuePair<string, int>> n2sbat(string[] any_id)
		{
			if ( ConfigurationManager.AppSettings["use_n2s_dll"] != null )
				return n2sbat_dll(any_id);
            if ( ConfigurationManager.AppSettings["csws_url"] == null )
                return local_n2sbat(any_id);
            else {
                throw new NotImplementedException();
                // com.chemspider.ws.CSWS csws = new com.chemspider.ws.CSWS();
                // csws.Url = ConfigurationManager.AppSettings["csws_url"];
                // List<KeyValuePair<string, int>> result = new List<KeyValuePair<string, int>>();
                // return csws.n2sbat(any_id).Select(s => new KeyValuePair<string, int>(s, 100));
            }
		}

        [DllImport("strglib.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int GenerateStructure(string name, string sdf_file);

        private static List<KeyValuePair<string, int>> n2sbat_dll(string[] ids)
        {
            List<KeyValuePair<string, int>> result = new List<KeyValuePair<string, int>>(ids.Length);
            try {
                using ( TempFile tf = new TempFile() ) {
                    foreach ( string id in ids ) {
                        int n = GenerateStructure(id, tf.FullPath);
                        switch ( n ) {
                            case 0:
                                result.Add(new KeyValuePair<string, int>(String.Empty, 0));
                                break;
                            case 1:
                                result.Add(new KeyValuePair<string, int>(File.ReadAllText(tf.FullPath).Replace("ACD/Labs", ""), 100));
                                break;
                            default:
                                result.Add(new KeyValuePair<string, int>(File.ReadAllText(tf.FullPath).Replace("ACD/Labs", ""), 50));
                                break;
                        }
                    }
                }
            }
            catch ( Exception ex ) {
                Trace.TraceError(ex.ToString());
            }
            return result;
        }

        private static List<KeyValuePair<string, int>> local_n2sbat(string[] any_id)
		{
			string program = ConfigurationManager.AppSettings.Get("n2s_program");
			string arguments = ConfigurationManager.AppSettings.Get("n2s_arguments");
            string core = Utility.RandomValue().ToString() + Thread.CurrentThread.ManagedThreadId.ToString();
			string txt_path = Path.Combine(Path.GetTempPath(), core + ".txt");
			using ( StreamWriter sw = new StreamWriter(txt_path) ) {
				foreach ( string id in any_id ) {
					sw.WriteLine(id);
				}
				sw.Close();
			}
			string sdf_path = Path.Combine(Path.GetTempPath(), core + ".sdf");

			arguments = arguments.Replace("%1", "\"" + txt_path + "\"");
			arguments = arguments.Replace("%2", "\"" + sdf_path + "\"");
			arguments += " -DICT+ -SMILES+";

			try {
				ProcessStartInfo psi = new ProcessStartInfo(program, arguments);
				psi.CreateNoWindow = true;
				psi.UseShellExecute = false;
				Process proc = Process.Start(psi);
				if ( !proc.WaitForExit(10000) ) {   // 10 seconds max
					proc.Kill();
					return null;
				}
				else if ( proc.ExitCode != 0 ) {
					return null;
				}

				List<KeyValuePair<string, int>> result = new List<KeyValuePair<string, int>>(any_id.Length);
				using ( SdfReader sr = new SdfReader(sdf_path) ) {
					foreach ( SdfRecord rec in sr.Records )
                        addN2SResult(result, rec);
				}
				return result;
			}
			finally {
				if ( File.Exists(txt_path) )
					File.Delete(txt_path);
				if ( File.Exists(sdf_path) )
					File.Delete(sdf_path);
			}
		}

        private static void addN2SResult(List<KeyValuePair<string, int>> result, SdfRecord rec)
        {
            if ( rec.Properties != null && rec.Properties.ContainsKey("message") ) {
                string message = rec.Properties["message"][0].ToString();
                if ( message.StartsWith("Error:") )
                    result.Add(new KeyValuePair<string, int>(String.Empty, 0));
                else if ( message.StartsWith("Warning:") )
                    result.Add(new KeyValuePair<string, int>(rec.Mol.Replace("ACD/Labs", ""), 50));
            }
            else {
                result.Add(new KeyValuePair<string, int>(rec.Mol.Replace("ACD/Labs", ""), 100));
            }
        }

        public static Dictionary<string, string> calcProperties(string mol, bool all)
        {
            string sdf_path = Path.Combine(Path.GetTempPath(), Utility.RandomValue() + ".sdf");
            try {
                File.WriteAllText(sdf_path, mol);

                // ExtProc.namebat(sdf_path);
                MolExtProc.pchbat(sdf_path);

                var tags = all ? new List<string>()
                               : new List<string>() { "ACD_Prop_Molecular_Formula_All",
                    "ACD_Prop_Monoisotopic_Mass", "ACD_Prop_Nominal_Mass", "ACD_Prop_Average_Mass",
                    "ACD_RuleOf5_MW",
                    "ACD_Prop_Molar_Refractivity", "ACD_Prop_Molar_Refractivity_Error", "ACD_Prop_Molar_Refractivity_Units",
                    "ACD_Prop_Molar_Volume", "ACD_Prop_Molar_Volume_Error", "ACD_Prop_Molar_Volume_Units", 
                    "ACD_Prop_Density", "ACD_Prop_Density_Error", "ACD_Prop_Density_Units", 
                    "ACD_Prop_Polarizability", "ACD_Prop_Polarizability_Error", "ACD_Prop_Polarizability_Units", 
				    "ACD_Prop_Dielectric_Constant", "ACD_Prop_Dielectric_Constant_Error", "ACD_Prop_Dielectric_Constant_Units",
					"ACD_Prop_Index_Of_Refraction", "ACD_Prop_Index_Of_Refraction_Error", "ACD_Prop_Index_Of_Refraction_Units",
					"ACD_Prop_Surface_Tension", "ACD_Prop_Surface_Tension_Error", "ACD_Prop_Surface_Tension_Units",
					"ACD_Prop_Parachor", "ACD_Prop_Parachor_Error", "ACD_Prop_Parachor_Units",
					"ACD_BP", "ACD_BP_Error", "ACD_BP_Units",
					"ACD_VP", "ACD_VP_Error", "ACD_VP_Units", 
					"ACD_FP", "ACD_FP_Error", "ACD_FP_Units",
					"ACD_Enthalpy", "ACD_Enthalpy_Error", "ACD_Enthalpy_Units",
					"ACD_LogP", "ACD_LogP_Error", 
					"ACD_RuleOf5_PSA", "ACD_RuleOf5_FRB", "ACD_RuleOf5_HDonors", "ACD_RuleOf5_HAcceptors", "ACD_RuleOf5"
				};
                var result = (from r in SdfUtils.retrieveSdfData(sdf_path, tags).Item2 select new KeyValuePair<string, string>(r.Key, r.Value.First())).ToDictionary(p => p.Key, p => p.Value);
                result.Add("SMILES", MolUtils.MolToSMILES(mol));

                string[] inchis = InChINet.InChIUtils.mol2inchiinfo(mol, InChIFlags.Default);
                if ( inchis != null ) {
                    result.Add("InChI", inchis[0]);
                    result.Add("InChIKey", inchis[1]);
                }

                string[] stdinchis = InChINet.InChIUtils.mol2inchiinfo(mol, InChIFlags.Standard);
                if ( stdinchis != null ) {
                    result.Add("StdInChI", stdinchis[0]);
                    result.Add("StdInChIKey", stdinchis[1]);
                }

                return result;
            }
            finally {
                File.Delete(sdf_path);
            }
        }
	}
}
