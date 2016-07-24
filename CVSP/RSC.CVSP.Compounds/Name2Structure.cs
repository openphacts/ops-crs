using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ChemSpider.Utilities;
using MoleculeObjects;


namespace RSC.CVSP.Compounds
{
    public class N2S
    {
        public static bool IsLinux()
        {
            int p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }

        public static string N2str(List<string> names)
        {
            string sdf = "";
            // build input file
            string program,arguments;
            if(IsLinux())
            {
                program = ConfigurationManager.AppSettings.Get("n2s_program_hadoop");
                arguments = ConfigurationManager.AppSettings.Get("n2s_arguments_hadoop");
                
            }
            else{
                program = ConfigurationManager.AppSettings.Get("n2s_program");
                arguments = ConfigurationManager.AppSettings.Get("n2s_arguments");
            }
            Trace.TraceInformation("n2s_program path: " + program);
            Trace.TraceInformation("n2s_arguments: " + arguments);
            string core = Utility.RandomValue().ToString() + Thread.CurrentThread.ManagedThreadId.ToString();
            string txt_path = Path.Combine(Path.GetTempPath(), core + ".txt");
            string sdf_path = Path.Combine(Path.GetTempPath(), core + ".sdf");
            arguments = arguments.Replace("%1", "\"" + txt_path + "\"");
            arguments = arguments.Replace("%2", "\"" + sdf_path + "\"");
            //arguments += " -DICT+ -SMILES+";
            using (StreamWriter sw = new StreamWriter(txt_path))
            {
                foreach (string name in names)
                    sw.WriteLine(name);
            }
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(program, arguments);
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.ErrorDialog = false;
                Process proc = Process.Start(psi);
                if (!proc.WaitForExit(60000))
                {   // 1 minute max
                    proc.Kill();
                    return "Name to structure code has timed out.";
                }
                else if (proc.ExitCode != 0)
                {
                    return "Name to structure code has failed with exit code " + proc.ExitCode;
                }
                using (StreamReader sr = new StreamReader(sdf_path))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                        sdf += line;
                }
            }
            catch (Exception e) { sdf += ". didn't work; see error: " + e.ToString(); }
            finally
            {
                new FileInfo(txt_path).Delete();
                new FileInfo(sdf_path).Delete();
            }
            
            return MoleculeFactory.RestoreWhitespace(sdf); 
        }
    }
}
