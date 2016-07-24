using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using ChemSpider.Utilities;
using InChINet;
using OpenBabelNet;
using com.ggasoftware.indigo;

namespace ChemSpider.Molecules
{
    public class SdfUtils
    {
        static public Tuple<string, Dictionary<string, List<string>>> retrieveSdfData(string sdf_file)
        {
            return retrieveSdfData(sdf_file, Encoding.UTF8);
        }

        static public Tuple<string, Dictionary<string, List<string>>> retrieveSdfData(string sdf_file, Encoding encoding)
        {
            return retrieveSdfData(sdf_file, new List<string>(), encoding);
        }

        static public Tuple<string, Dictionary<string, List<string>>> retrieveSdfData(string sdf_file, List<string> tags)
        {
            return retrieveSdfData(sdf_file, tags, Encoding.UTF8);
        }

        static public Tuple<string, Dictionary<string, List<string>>> retrieveSdfData(string sdf_file, List<string> tags, Encoding encoding)
        {
            bool all = (tags.Count == 0), bDataBegan = false;
            StringBuilder sb = new StringBuilder();

            var tagValues = new Dictionary<string, List<string>>();
            using ( StreamReader sr = new StreamReader(sdf_file, encoding) ) {
                string line;
                Regex rx = new Regex("^>.*?<(.*?)>", RegexOptions.IgnoreCase);
                while ( (line = sr.ReadLine()) != null ) {
                    Match m = rx.Match(line);
                    if ( m.Success ) {
                        bDataBegan = true;
                        string tag = m.Groups[1].Captures[0].Value;
                        if ( tags.Contains(tag) || all ) {
                            var value = new List<string>();
                            while ( true ) {
                                line = sr.ReadLine();
                                if ( line == String.Empty )
                                    break;
                                value.Add(line);
                            }
                            tagValues.Add(tag, value);
                        }
                    }

                    if ( !bDataBegan ) {
                        sb.Append(line);
                        sb.Append("\n");
                    }
                }
            }
            return new Tuple<string, Dictionary<string, List<string>>>(sb.ToString(), tagValues);
        }

        public static void MergeFiles(string file1, string file2, string key1, string key2, string output, Encoding encoding)
        {
            var fields2 = new Dictionary<string, SdfRecord>();
            using (SdfReader sr1 = new SdfReader(file1, encoding),
                             sr2 = new SdfReader(file2, encoding))
            {
                using (StreamWriter sw = new StreamWriter(output, false, encoding))
                {
                    SdfRecord rec1;
                    while ((rec1 = sr1.ReadSDFRecord()) != null)
                    {
                        SdfRecord rec2 = null;
                        if (rec1.Properties.ContainsKey(key1))
                        {
                            if (!fields2.ContainsKey(rec1[key1].FirstOrDefault()))
                            {
                                while ((rec2 = sr2.ReadSDFRecord()) != null)
                                {
                                    if (rec2.Properties.ContainsKey(key2))
                                    {
                                        fields2[rec2[key2].FirstOrDefault()] = rec2;
                                        if (rec2[key2].FirstOrDefault() == rec1[key1].FirstOrDefault())
                                            break;
                                    }
                                    else
                                    {
                                        sw.Write(rec2.ToString());
                                    }
                                }
                            }
                            if (fields2.ContainsKey(rec1[key1].FirstOrDefault()))
                            {
                                rec2 = (SdfRecord)fields2[rec1[key1].FirstOrDefault()];
                                fields2.Remove(rec1[key1].FirstOrDefault());

                                foreach (KeyValuePair<string, List<string>> e in rec2.Properties)
                                {
                                    if (rec1.Properties.ContainsKey(e.Key))
                                    {
                                        //We don't want to copy over the key values if they already exist as there will be duplicates.
                                        if (e.Key != key2 && e.Key != key1)
                                            rec1.Properties[e.Key].AddRange(e.Value);
                                    }
                                    else
                                    {
                                        List<string> items = new List<string>();
                                        items.AddRange(e.Value);
                                        rec1.Properties[e.Key] = items;
                                    }
                                }
                            }
                        } // else nothing to merge with -- copy to output
                        sw.Write(rec1.ToString());
                    } // End while sr1.GetSDFRecord

                    // copy remaining records over
                    SdfRecord rec = null;
                    foreach (KeyValuePair<string, SdfRecord> p in fields2)
                    {
                        sw.Write(p.Value.ToString());
                    }
                    while ((rec = sr2.ReadSDFRecord()) != null)
                    {
                        sw.Write(rec.ToString());
                    }
                }
            }
        }

        //public static void MergeFiles(string file1, string file2, string key1, string key2, string output, Encoding encoding)
        //{
        //    var fields2 = new Dictionary<string, SdfRecord>();
        //        using ( StreamWriter sw = new StreamWriter(output, false, encoding) ) 
        //        {
        //            using (SdfReader sr1 = new SdfReader(file1, encoding), sr2 = new SdfReader(file2, encoding)) 
        //            {
        //                foreach ( SdfRecord rec1 in sr1.Records ) 
        //                {
        //                    if ( rec1.Properties.ContainsKey(key1) ) 
        //                    {
        //                        if ( !fields2.ContainsKey(rec1[key1].FirstOrDefault()) ) 
        //                        {
        //                            foreach ( SdfRecord rec2 in sr2.Records ) 
        //                            {
        //                                if ( rec2.Properties.ContainsKey(key2) ) 
        //                                {
        //                                    fields2[rec2[key2].FirstOrDefault()] = rec2;
        //                                    if ( rec2[key2] == rec1[key1] )
        //                                        break;
        //                                }
        //                                else 
        //                                {
        //                                    sw.Write(rec2.ToString());
        //                                }
        //                            }
        //                        }
        //                        if ( fields2.ContainsKey(rec1[key1].FirstOrDefault()) ) 
        //                        {
        //                            SdfRecord rec2 = (SdfRecord)fields2[rec1[key1].FirstOrDefault()];
        //                            fields2.Remove(rec1[key1].FirstOrDefault());
        //                            foreach ( var e in rec2.Properties ) 
        //                            {
        //                                rec1.Properties[e.Key] = e.Value;
        //                            }
        //                        }
        //                    } // else nothing to merge with -- copy to output
        //                    sw.Write(rec1.ToString());
        //                }
        //            // copy remaining records over
        //            foreach ( KeyValuePair<string, SdfRecord> p in fields2 ) 
        //            {
        //                sw.Write(p.Value.ToString());
        //            }
        //        }

        //        using (SdfReader sr2 = new SdfReader(file2, encoding))
        //        {
        //            foreach (SdfRecord rec in sr2.Records)
        //            {
        //                sw.Write(rec.ToString());
        //            }
        //        }
        //    }
        //}

        public static void CdxToSdf(string cdx_file, string sdf_file, Dictionary<string, string> dict)
        {
            string tmp = null;
            try {
                tmp = Path.GetTempFileName();
                if ( OpenBabel.GetInstance().convert(cdx_file, "cdx", tmp, "sdf") ) {
                    using ( SdfReader sdfr = new SdfReader(tmp) ) {
                        foreach ( SdfRecord sdfrec in sdfr.Records ) {
                            foreach ( var v in dict )
                                sdfrec.AddField(v.Key, v.Value);

                            File.AppendAllText(sdf_file, sdfrec.ToString());
                        }
                    }
                }
            }
            finally {
                if ( File.Exists(tmp) )
                    File.Delete(tmp);
            }
        }

        /// <summary>
        /// Returns true if the sdf is a v3000 file, otherwise false.
        /// </summary>
        public static bool isV3000File(string sdf)
        {
            return sdf.Contains("M  V30");
        }

        /// <summary>
        /// Converts a V3000 mol file to V2000.
        /// </summary>
        /// <param name="v3000Mol">The V3000 mol file to convert.</param>
        /// <param name="v2000Mol">The converted V2000 mol file.</param>
        /// <param name="errorMessage">Details of any problems that occurred.</param>
        /// <returns>Boolean indicating success of the conversion.</returns>
        public static bool convertV3000ToV2000(string v3000Mol, out string v2000Mol, out string errorMessage)
        {
            try
            {
                //Need to remove the SGROUP and COLLECTION sections as Indigo has a problem with those.
                v3000Mol = Regex.Replace(v3000Mol, @"M  V30 BEGIN SGROUP(.+?)M  V30 END SGROUP\r?\n", string.Empty, RegexOptions.Singleline);
                v3000Mol = Regex.Replace(v3000Mol, @"M  V30 BEGIN COLLECTION(.+?)M  V30 END COLLECTION\r?\n", string.Empty, RegexOptions.Singleline);

                //Use Indigo to convert to V2000, ignroing stereochemisrty errors.
                Indigo i = new Indigo();
                i.setOption("molfile-saving-mode", "2000");
                i.setOption("ignore-stereochemistry-errors", "true");
                IndigoObject im = i.loadMolecule(v3000Mol);
                errorMessage = string.Empty;
                v2000Mol = im.molfile();
                return true;
            }
            catch(Exception ex)
            {
                //If we can't convert then return empty string.
                errorMessage = ex.Message;
                v2000Mol = string.Empty;
                return false;
            }
        }
    }
}
