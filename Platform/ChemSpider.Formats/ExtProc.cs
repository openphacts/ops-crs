using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices; 
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

using ChemSpider.Utilities;

namespace ChemSpider.Formats
{
	public enum EETool
	{
		EE_SureChem,
		EE_OSCAR3
	}

	public static class ExtProc
	{
        public static string WorkingDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

		static public byte[] iecapt(string url, int width)
		{
			if ( String.IsNullOrEmpty(url) )
				return null;

            string rnd = Utility.RandomValue().ToString();
			string img_path = Path.Combine(Path.GetTempPath(), rnd + ".png");
			try {
				try {
					WebRequest req = WebRequest.Create(url);
					req.Method = "HEAD";
					req.Timeout = 5000;
					WebResponse resp = req.GetResponse();
					url = resp.ResponseUri.ToString();
				}
				catch ( Exception ) {
					// Just keep an old URL hoping it'll work
				}

				ProcessStartInfo psi = new ProcessStartInfo();
				psi.WorkingDirectory = ConfigurationManager.AppSettings.Get("iecapt_dir");
				psi.FileName = ConfigurationManager.AppSettings.Get("iecapt_program");
				psi.Arguments = ConfigurationManager.AppSettings.Get("iecapt_arguments");
				psi.Arguments = psi.Arguments.Replace("%3", String.Format("{0}", width));
				psi.Arguments = psi.Arguments.Replace("%2", String.Format("\"{0}\"", img_path));
				psi.Arguments = psi.Arguments.Replace("%1", String.Format("\"{0}\"", url));
				psi.UseShellExecute = false;
				psi.CreateNoWindow = true;
				Process proc = Process.Start(psi);
				if ( !proc.WaitForExit(30000) ) {   // 30 seconds max
					proc.Kill();
					return null;
				}
				else if ( proc.ExitCode != 0 ) {
					return null;
				}

				return File.ReadAllBytes(img_path);
			}
			catch ( Exception ) {
				return null;
			}
			finally {
				new FileInfo(img_path).Delete();
			}
		}

	}
}
