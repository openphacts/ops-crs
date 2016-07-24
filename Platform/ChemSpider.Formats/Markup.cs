using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using System.Web;

namespace ChemSpider.Formats
{
    public class Markup
    {
        public static string highliteEntities_EE(string htmlInput, List<EntityLocator> entities, bool xml)
        {
            return highliteEntities(htmlInput, "ee", 0.0, entities, xml);
        }

        public static string highliteEntities_Oscar3(string htmlInput, double confidenceThreshold, List<EntityLocator> entities)
        {
            return highliteEntities(htmlInput, "oscar3", confidenceThreshold, entities, true);
        }

        static string DecodeEEEntityType(string entity)
        {
            switch ( entity ) {
                case "person":
                    return "contributor";
                case "date":
                    return "publish-date";
                case "species":
                    return "species";
                case "chemical":
                    return "chemical-name";
                case "chemical-element":
                    return "chemical-element";
                default:
                    return entity;  // use "external" (EE) type by default
            }
        }

        public static int extractEntities(string htmlInput, string program_prefix, double confidenceThreshold, List<EntityLocator> result, bool xml)
        {
            string rnd = Path.GetRandomFileName();
            string htmlInputName = Path.Combine(Path.GetTempPath(), rnd + "-in.html");

            Encoding encoding = Encoding.UTF8;
            string encstring = ConfigurationManager.AppSettings.Get(string.Format("{0}_encoding", program_prefix));
            if ( encstring != null )
                encoding = Encoding.GetEncoding(encstring);

            try {
                // not using writealltext to avoid inserting unicode signature
                File.WriteAllBytes(htmlInputName, encoding.GetBytes(htmlInput));

                if ( result == null )
                    result = new List<EntityLocator>();
                string program = ConfigurationManager.AppSettings.Get(string.Format("{0}_program", program_prefix));
                string directory = ConfigurationManager.AppSettings.Get(string.Format("{0}_directory", program_prefix));
                string arguments = xml ? ConfigurationManager.AppSettings.Get(string.Format("{0}_arguments", program_prefix)) : ConfigurationManager.AppSettings.Get(string.Format("{0}_arguments_text", program_prefix));
                arguments = arguments.Replace("%1", "\"" + htmlInputName + "\"");

                ProcessStartInfo psi = new ProcessStartInfo(program, arguments);
                psi.WorkingDirectory = directory;	//Path.GetDirectoryName(program);
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.StandardOutputEncoding = encoding;
                Process proc = Process.Start(psi);

                Dictionary<string, int> groups = new Dictionary<string, int>();
                string line;
                byte[] raw = File.ReadAllBytes(htmlInputName);
                string utf = encoding.GetString(raw);
                while ( ( line = proc.StandardOutput.ReadLine() ) != null ) {
                    string[] s = line.Split('\t');
                    if ( s.Length >= 5 ) {
                        double conf = s.Length >= 6 ? double.Parse(s[5], System.Globalization.CultureInfo.InvariantCulture.NumberFormat) : 0.0;
                        if ( conf >= confidenceThreshold ) {
                            int start = int.Parse(s[1]);
                            int end = int.Parse(s[2]);
                            int grp = -1;
                            if ( groups.ContainsKey(s[0]) )
                                grp = groups[s[0]];
                            else {
                                grp = groups.Count + 1;
                                groups[s[0]] = grp;
                            }

                            result.Add(new EntityLocator(s[0], start, end,
                                /* TODO: s[3] is in EE encoding, that is it may not 
                                 * be decoded correctly by DecodeEntityType */
                                DecodeEEEntityType(s[3]),
                                conf,
                                utf.Substring(start, end - start),
                                grp)
                                );
                        }
                    }
                }
                StringBuilder error = new StringBuilder();
                while ( ( line = proc.StandardError.ReadLine() ) != null ) {
                    error.AppendLine(line);
                }
                proc.WaitForExit();
                if ( error.Length > 0 )
                    throw new Exception(error.ToString());
                if ( proc.ExitCode != 0 )
                    return proc.ExitCode;

                if ( result.Count > 0 ) {
                    ChemicalEntityValidator validator = new ChemicalEntityValidator();
                    validator.Validate(result);
                }

                return 0;
            }
            finally {
                new FileInfo(htmlInputName).Delete();
            }
        }

        static void x_AutoBalance(string html, out string pre, out string mid, out string post)
        {
            StringBuilder result = new StringBuilder();
            StringBuilder prefix = new StringBuilder();
            StringBuilder postfix = new StringBuilder();
            Stack<int> s = new Stack<int>();
            List<int> remove = new List<int>();
            int i = -1, j = -1;
            do {
                i = html.IndexOf("<", i + 1, StringComparison.InvariantCultureIgnoreCase);
                j = html.IndexOf("</", j + 1, StringComparison.InvariantCultureIgnoreCase);
                if ( i >= 0 && ( i < j || j < 0 ) ) {
                    s.Push(i);
                    j = i;
                }
                else if ( j >= 0 && ( j <= i || i < 0 ) ) {
                    if ( s.Count == 0 ) // extra closing tag
                        remove.Add(j);
                    else
                        s.Pop();
                    i = j;
                }
            }
            while ( i >= 0 || j >= 0 );
            while ( s.Count > 0 )
                remove.Add(s.Pop());
            remove.Sort();

            int start = 0;
            foreach ( int n in remove ) {
                int end = html.IndexOf(">", n);
                if ( end > 0 ) {
                    result.Append(html.Substring(start, n - start));
                    if ( html[n + 1] == '/' ) // closing tag, add to prefix
                        prefix.Append(html.Substring(n, end - n + 1));
                    else // opening tag, add t postfix
                        postfix.Append(html.Substring(n, end - n + 1));
                    start = end + 1;
                }
            }
            if ( start < html.Length )
                result.Append(html.Substring(start, html.Length - start));

            pre = prefix.ToString();
            mid = result.ToString();
            post = postfix.ToString();
        }


        // encoding: defines how input bytes are encoded
        static void HighlightHtml(string article, List<EntityLocator> index, StreamWriter output)
        {
            char[] text = article.ToCharArray();
            if ( index.Count == 0 ) {
                output.Write(text, 0, text.Length);
            }
            else {
                int sp = 0;
                int ep = 0;

                foreach ( EntityLocator el in index ) {
                    sp = el.offset_start;
                    if ( sp < ep )
                        continue;
                    if ( ep == 0 && text[0] == '\xFEFF' )
                        output.Write(text, ep + 1, sp - ep - 1);
                    else
                        output.Write(text, ep, sp - ep);
                    string name = HttpUtility.HtmlEncode(el.entity);
                    string prefix, mid, postfix;
                    x_AutoBalance(el.html, out prefix, out mid, out postfix);
                    if ( prefix.Length > 0 ) {
                        output.Write(prefix);
                    }
                    output.Write("<span class=\"csm-{0}{2}\" id=\"{1}\" grpid=\"{3}\">",
                        el.type.ToString().ToLower(), el.id,
                        el.validated == false ? " csm-not-validated" : el.validation_confidence > 0 && el.validation_confidence < 100 ? " csm-ambiguous" : "",
                        el.group_id);
                    ep = el.offset_end;

                    output.Write(mid);
                    output.Write("</span>");
                    if ( postfix.Length > 0 ) {
                        output.Write(postfix);
                    }
                }
                output.Write(text, ep, text.Length - ep);
            }
        }

        public static string highliteEntities(string htmlInput, List<EntityLocator> index)
        {
            string rnd = Path.GetRandomFileName();
            string htmlOutputName = Path.Combine(Path.GetTempPath(), rnd + "-out.html");
            try {
                using ( StreamWriter output = new StreamWriter(htmlOutputName, false, Encoding.UTF8) ) {
                    HighlightHtml(htmlInput, index, output);
                }

                return File.ReadAllText(htmlOutputName, Encoding.UTF8);
            }
            finally {
                new FileInfo(htmlOutputName).Delete();
            }
        }

        public static string highliteEntities(string htmlInput, string program_prefix, double confidenceThreshold, List<EntityLocator> index, bool xml)
        {
            if ( index == null )
                index = new List<EntityLocator>();
            if ( 0 != extractEntities(htmlInput, program_prefix, confidenceThreshold, index, xml) ) {
                return null;
            }

            return highliteEntities(htmlInput, index);
        }
    }
}
