using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ChemSpider.Database;
using ChemSpider.Data.Database;
using System.IO.Compression;
using System.Xml.Serialization;
using System.Xml.Xsl;

namespace ChemSpider.Formats
{
    public class FilexInfo
    {
        public int content_length;
        public string content_type;
        public string file_name;
        public string disk_file;
        public string client_address;
        public string description;

        public override string ToString()
        {
            return String.Format(
                "File:        {0} ({1} bytes)\n" +
                "Mime-Type:   {2}\n" +
                "Client:      {3}\n" +
                "Description: {4}\n", file_name, content_length, content_type, client_address, description);
        }

        public void save(string file)
        {
            using ( FileStream fs = File.OpenWrite(file) ) {
                XmlSerializer ser = new XmlSerializer(typeof(FilexInfo));
                ser.Serialize(fs, this);
                fs.Close();
            }
        }

        public static FilexInfo load(string file)
        {
            using ( FileStream fs = File.OpenRead(file) ) {
                XmlSerializer ser = new XmlSerializer(typeof(FilexInfo));
                FilexInfo fi = ser.Deserialize(fs) as FilexInfo;
                fs.Close();
                return fi;
            }
        }
    }

    public static class Exports
    {
        public static void GenerateXMLfile(string filex, string id_file, string Request_UserHostName, int? whichzipfile)
        {
            filex = filex ?? "";
            string statusFileName = Path.Combine(filex, id_file + ".status");
            try {
                ChemSpiderDB m_csdb = new ChemSpiderDB();
                Dictionary<int, string> molsandcsids = null;
                Dictionary<int, string> supppropsandcsids = null;
                StreamWriter ZipStreamWriter = null;
                string filenameminuspath = id_file + (whichzipfile == null ? "" : "_" + whichzipfile) + ".xml";
                string filename = Path.Combine(filex, filenameminuspath);
                if ( !File.Exists(filename) ) {
                    ZipStreamWriter = File.CreateText(filename);
                    ZipStreamWriter.Flush();
                    ZipStreamWriter.Close();
                }
                int maxcsid = ChemSpiderDB.GetMaxCSIDInDatabse();
                List<int> intcsids = ChemSpiderDB.GetCSIDsInRange(0, maxcsid, false, true);
                if ( intcsids != null ) {
                    string thissuppinfo;
                    object supp_info = null;
                    object common_info = null;
                    object smiles_info = null;
                    foreach ( int thisid in intcsids.ToArray() ) {
                        if ( thisid % 1000 == 0 ) {
                            //only writes out to status file every 1000 records (using mod)
                            WriteToStatusFile(statusFileName, "Processing compounds " + thisid + " of " + maxcsid, false);
                        }
                        if ( CheckIfExportIsCancelled(filex, id_file) ) {
                            CancelExport(molsandcsids, supppropsandcsids);
                            return;
                        }
                        thissuppinfo = null;
                        supp_info = m_csdb.DBU.m_querySingleValue("WITH XMLNAMESPACES ('chemspider:xmlns:user-data' as userdata) select cast([supp_info].query('(/cs-record/userdata:user-data-tree/userdata:categories/userdata:experimental-physchem-properties)') as nvarchar(max)) from compounds_supp_info where cmp_id = " + thisid + " and cast([supp_info].query('(/cs-record/userdata:user-data-tree/userdata:categories/userdata:experimental-physchem-properties)') as nvarchar(max)) != '';");
                        common_info = m_csdb.DBU.m_querySingleValue("WITH XMLNAMESPACES ('chemspider:xmlns:user-data' as userdata) select cast([supp_info].query('(/cs-record/userdata:user-data-tree/userdata:common-meta)') as nvarchar(max)) from compounds_supp_info where cmp_id = " + thisid + " and cast([supp_info].query('(/cs-record/userdata:user-data-tree/userdata:common-meta)') as nvarchar(max)) != '';");
                        smiles_info = m_csdb.DBU.m_querySingleValue("Select SMILES from compounds where cmp_id = " + thisid + ";");
                        //object supp_info = m_csdb.DBU.m_querySingleValue("select supp_info from compounds_supp_info where cmp_id = " + thisid);
                        if ( supp_info != null ) {
                            thissuppinfo = supp_info.ToString();
                            if ( smiles_info != null ) {
                                thissuppinfo = "<categories>" + "<identifiers>" + "<smiles>" + smiles_info.ToString() + "</smiles>" + "</identifiers>" + thissuppinfo + "</categories>";
                            }
                            else {
                                thissuppinfo = "<categories>" + thissuppinfo + "</categories>";
                            }
                            if ( common_info != null ) {
                                thissuppinfo = common_info.ToString() + thissuppinfo;
                            }
                            thissuppinfo = thissuppinfo.Replace(" xmlns:userdata=\"chemspider:xmlns:user-data\"", "");
                            thissuppinfo = thissuppinfo.Replace("userdata:", "");
                            thissuppinfo = "<cs-record id=\"" + thisid + "\"><user-data-tree xmlns=\"chemspider:xmlns:user-data\">" + thissuppinfo + "</user-data-tree></cs-record>\n";
                            //StreamWriter outputstreamwriter = File.AppendText(outputFileName);
                            //outputstreamwriter.WriteLine(thissuppinfo);
                            //outputstreamwriter.Flush();
                            //outputstreamwriter.Close();
                            ZipStreamWriter = File.AppendText(filename);
                            ZipStreamWriter.Write(thissuppinfo);
                            ZipStreamWriter.Flush();
                            ZipStreamWriter.Close();
                        }
                    }
                }
                string ZipFileName = Path.Combine(filex, id_file + ( whichzipfile == null ? "" : "_" + whichzipfile ) + ".gz");
                ZipFileUp(filename, ZipFileName);
                File.Delete(filename);
                WriteToInfoFile(id_file, filex, whichzipfile, Request_UserHostName, "XML of PhysChem properties");
                WriteToStatusFile(statusFileName, "Success - " + maxcsid + " compounds written", false);
            }
            catch ( Exception ex ) {
                WriteToStatusFile(statusFileName, String.Format("Error - export aborted: {0}", ex.ToString()), true);
            }
        }

        public static void GenerateSDFfile(string filex, string id_file, string Request_UserHostName, int startcsid, int whichzipfile, XslCompiledTransform xslt)
        {
            string statusFileName = Path.Combine(filex, id_file + ".status");
            try {
                ExportSDF SDFFunctions = new ExportSDF();
                ChemSpiderBlobsDB m_csbdb = new ChemSpiderBlobsDB();
                string ZipFileName = Path.Combine(filex, id_file + "_" + whichzipfile + ".gz");
                int totalcsid = ChemSpiderDB.GetMaxCSIDInDatabse();

                StreamWriter ZipStreamWriter = null;
                Dictionary<int, string> molsandcsids = null;
                Dictionary<int, string> supppropsandcsids = null;

                string thissdf = null;
                string allsdfs = null;
                string thisuserdatasdf = null;
                string sqlcommandtext = null;

                //int filemincsid = startcsid;
                //int filechunksize = 100000;
                //int filemaxcsid = filemincsid + filechunksize - 1;

                int mincsid = startcsid;
                int chunksize = 1000;
                int maxcsid = mincsid + chunksize - 1;

                List<int> intcsids = null;

                string filenameminuspath = id_file + "_" + whichzipfile + ".sdf";
                string filename = Path.Combine(filex, filenameminuspath);
                if ( !File.Exists(filename) ) {
                    ZipStreamWriter = File.CreateText(filename);
                    ZipStreamWriter.Flush();
                    ZipStreamWriter.Close();
                }
                Exports.WriteToStatusFile(statusFileName, "Processing compounds " + mincsid + " of " + totalcsid, false);

                string internalfieldsxml = ChemSpiderDB.GetInternalFieldsAsXML();
                allsdfs = null;
                while ( maxcsid <= totalcsid ) { //start loop for each file

                    maxcsid = mincsid + chunksize - 1;

                    //while (maxcsid <= filemaxcsid)
                    //{ //start loop to query database in chunks of 100 compounds
                    intcsids = ChemSpiderDB.GetCSIDsInRange(mincsid, maxcsid, false, true);
                    if ( CheckIfExportIsCancelled(filex, id_file) == true ) {
                        CancelExport(molsandcsids, supppropsandcsids);
                        return;
                    }
                    if ( intcsids.Count > 0 ) {
                        sqlcommandtext = " where";
                        foreach ( int thisid in intcsids ) {
                            sqlcommandtext = string.Format("{0} cmp_id = {1} or", sqlcommandtext, thisid);
                        }
                        sqlcommandtext = sqlcommandtext.Substring(0, sqlcommandtext.Length - 3);
                        //sqlcommandtext = " where cmp_id >= " + mincsid + " and cmp_id <= " + maxcsid;
                        molsandcsids = m_csbdb.getMultipleSdfDictionary(sqlcommandtext);
                        supppropsandcsids = ChemSpiderDB.GetMultipleSuppInfoDictionary(sqlcommandtext, xslt);
                    }
                    foreach ( int thisid in intcsids ) {
                        thissdf = null;
                        if ( supppropsandcsids != null ) {
                            if ( supppropsandcsids.ContainsKey(thisid) ) {

                                if ( molsandcsids != null ) {
                                    if ( molsandcsids.ContainsKey(thisid) ) {
                                        thissdf = molsandcsids[thisid];
                                    }
                                }
                                if ( thissdf == null ) {
                                    thissdf = SDFFunctions.GetNoStructureMol(thisid);
                                }
                                else {
                                    thissdf = SDFFunctions.TrimBeginningAndEndOfMol(thisid, thissdf);
                                }
                                thisuserdatasdf = supppropsandcsids[thisid];
                                thissdf = SDFFunctions.AddChemSpiderIDAsProperty(thisid, thissdf);
                                if ( thisuserdatasdf != null ) {
                                    thissdf = thissdf + thisuserdatasdf;
                                }
                                thissdf = thissdf + "$$$$\n";
                                allsdfs = allsdfs + thissdf;
                            }
                        }
                    }
                    if ( molsandcsids != null ) { molsandcsids.Clear(); }
                    if ( supppropsandcsids != null ) { supppropsandcsids.Clear(); }
                    //}// end of loop for this database lookup
                    if ( CheckIfExportIsCancelled(filex, id_file) == true ) {
                        CancelExport(molsandcsids, supppropsandcsids);
                        return;
                    }
                    ZipStreamWriter = File.AppendText(filename);
                    ZipStreamWriter.Write(allsdfs);
                    ZipStreamWriter.Flush();
                    ZipStreamWriter.Close();
                    allsdfs = null;
                    WriteToStatusFile(statusFileName, "Processing compounds " + maxcsid + " of " + totalcsid, false);
                    FileInfo f = new FileInfo(Path.Combine(filex, filename));
                    int thisfilelength = Convert.ToInt32(f.Length);
                    //zips it up when it gets to a GB (assume 10% compression)
                    if ( thisfilelength > 1000000000 ) {
                        ZipFileUp(filename, ZipFileName);
                        File.Delete(filename);
                        WriteToInfoFile(id_file, filex, whichzipfile, Request_UserHostName, "SDF of PhysChem properties");
                        whichzipfile++;
                        ZipFileName = Path.Combine(filex, id_file + "_" + whichzipfile + ".gz");
                        filenameminuspath = id_file + "_" + whichzipfile + ".sdf";
                        filename = Path.Combine(filex, filenameminuspath);
                        if ( !File.Exists(filename) ) {
                            ZipStreamWriter = File.CreateText(filename);
                            ZipStreamWriter.Flush();
                            ZipStreamWriter.Close();
                        }
                    }
                    mincsid = maxcsid + 1;
                    maxcsid = mincsid + chunksize - 1;
                }
                ZipFileUp(filename, ZipFileName);
                File.Delete(filename);
                WriteToInfoFile(id_file, filex, whichzipfile, Request_UserHostName, "SDF of PhysChem properties");
                WriteToStatusFile(statusFileName, "Success - " + totalcsid + " compounds written", false);
            }
            catch {
                WriteToStatusFile(statusFileName, "Error - export aborted", true);
            }
        }

        private static void ZipFileUp(string filename, string ZipFileName)
        {
            //Open the file as a FileStream object.    
            FileStream infile = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] buffer = new byte[infile.Length];
            // Read the file to ensure it is readable.   
            int count = infile.Read(buffer, 0, buffer.Length);
            if ( count != buffer.Length ) {
                infile.Close();
                Console.WriteLine("Test Failed: Unable to read data from file");
                return;
            }
            infile.Close();
            Stream fs = File.Create(ZipFileName);
            // Use the newly created stream for the compressed data.   
            GZipStream gZip = new GZipStream(fs, CompressionMode.Compress, true);
            gZip.Write(buffer, 0, buffer.Length);
            // Close the stream.   
            gZip.Close();
        }

        private static Boolean CheckIfExportIsCancelled(string filex, string id_file)
        {
            string statusFileName = Path.Combine(filex ?? "", id_file + ".status");
            if ( !File.Exists(statusFileName) )
                return false;
            else
                return File.ReadAllText(statusFileName).Contains("Cancelled");
        }

        public static void WriteToStatusFile(string statusFileName, string texttowrite, Boolean appendtofile)
        {
            if ( File.Exists(statusFileName) && appendtofile ) {
                StreamWriter statusstreamwriter = File.AppendText(statusFileName);
                statusstreamwriter.WriteLine("Cancelled export");
                statusstreamwriter.Flush();
                statusstreamwriter.Close();
            }
            else {
                StreamWriter statusstreamwriter = new StreamWriter(statusFileName);
                statusstreamwriter.WriteLine(texttowrite);
                statusstreamwriter.Flush();
                statusstreamwriter.Close();
                statusstreamwriter.Dispose();
            }
        }

        private static void WriteToInfoFile(string id_file, string filex, int? whichzipfile, string Request_UserHostName, string description)
        {
            string info_file = Path.Combine(filex, id_file + ( whichzipfile == null ? "" : "_" + whichzipfile ) + ".info");
            string content_file = Path.Combine(filex, id_file + ( whichzipfile == null ? "" : "_" + whichzipfile ) + ".gz");
            FilexInfo fi = new FilexInfo();

            FileInfo f = new FileInfo(Path.Combine(filex, content_file));
            fi.content_length = Convert.ToInt32(f.Length);
            fi.content_type = f.GetType().ToString();
            fi.file_name = content_file;
            DateTime today = System.DateTime.Now;
            fi.description = description + " downloaded on " + today.Year.ToString() + "/" + today.Month.ToString() + "/" + today.Day.ToString();
            fi.client_address = Request_UserHostName;
            fi.disk_file = Path.Combine(filex, content_file);

            fi.save(Path.Combine(filex, info_file));
        }

        private static void CancelExport(Dictionary<int, string> molsandcsids, Dictionary<int, string> supppropsandcsids)
        {
            if ( molsandcsids != null ) {
                molsandcsids.Clear();
            }
            if ( supppropsandcsids != null ) {
                supppropsandcsids.Clear();
            }
            return;
        }
    }
}
