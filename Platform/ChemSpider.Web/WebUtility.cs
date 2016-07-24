using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.Caching;
using System.Xml;
using System.Net;
using ChemSpider.Data.Database;
using ChemSpider.Utilities;
using System.Linq;
using ChemSpider.Formats;
using ChemSpider.Molecules;

namespace ChemSpider.Web
{

    /// <summary>
    /// Summary description for WebUtility
    /// </summary>
    public class WebUtility
    {
        private static ChemSpiderDB s_csdb
        {
            get
            {
                return new ChemSpiderDB();
            }
        }

        public static byte[] getStructureImage(HttpContext context, int cmp_id, bool bAddIcon, bool logoNoURL = false)
        {
            byte[] buffer = null;

            try {
                using ( SqlConnection conn = new SqlConnection(ChemSpiderBlobsDB.ConnectionString) ) {
                    conn.Open();
                    using ( SqlCommand cmd = new SqlCommand("select image from images where cmp_id = " + cmp_id.ToString(), conn) ) {
                        using ( SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection) ) {
                            if ( reader.Read() && reader["image"] is byte[] ) {
                                buffer = (byte[])reader["image"];
                                if ( buffer != null && bAddIcon && context != null )
                                    buffer = addImageIcon(context, buffer);
                                else if ( buffer != null && bAddIcon && context != null )
                                    buffer = addImageIcon(context, buffer, logoNoURL);
                            }
                        }
                    }
                }
            }
            catch { }

            return buffer;
        }

        public static byte[] term2image(HttpContext context, string term, int w, int h)
        {
            byte[] buffer = null;
            if ( context != null )
                buffer = context.Cache.Get(term) as byte[];
            if ( buffer == null ) {
                string sdf;
                int confidence = ChemIdUtils.name2str(term, out sdf, true, true);
                if ( confidence > 0 )
                    buffer = MolUtils.GetImage(sdf, w, h, true);
                else {
                    try {
                        buffer = File.ReadAllBytes(context.Server.MapPath("~/images/cannot.gif"));
                    }
                    catch { }
                }

                if ( context != null )
                    context.Cache.Add(term, buffer, null, Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(60), CacheItemPriority.Default, null);
            }
            return buffer;
        }

        public static byte[] addImageIcon(HttpContext context, byte[] buffer, bool logoNoURL = false)
        {
            if ( buffer != null ) {
                MemoryStream ms = new MemoryStream(buffer);
                Image image = Image.FromStream(ms);
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(image);
                Icon icon = new Icon(context.Server.MapPath("favicon.ico"));
                g.DrawIcon(icon, image.Width - icon.Width, image.Height - icon.Height);
                StringFormat format = StringFormat.GenericDefault;
                format.Alignment = StringAlignment.Far;
                format.LineAlignment = StringAlignment.Far;
                if ( !logoNoURL )
                    g.DrawString("www.chemspider.com", new Font("Arial", 7), new SolidBrush(Color.LightGray), image.Width - (int)( icon.Width * 1.2 ), image.Height, format);
                g.Flush();
                MemoryStream ms2 = new MemoryStream();
                image.Save(ms2, ImageFormat.Png);
                buffer = ms2.GetBuffer();
            }
            return buffer;
        }

        public static byte[] addImageTextFields(byte[] buffer, int cmp_id)
        {
            MemoryStream ms = new MemoryStream(buffer);
            Image image = Image.FromStream(ms);
            PropertyItem pi = image.PropertyItems[0];

            // DocumentName
            string docname = String.Format("http://www.chemspider.com/Chemical-Structure.{0}.html", cmp_id);
            pi.Id = 0x010D;
            pi.Type = 2;
            pi.Len = docname.Length + 1;
            pi.Value = new ASCIIEncoding().GetBytes(docname);
            image.SetPropertyItem(pi);
            // Software
            string software = "ChemSpider (http://www.chemspider.com/)";
            pi.Id = 0x0131;
            pi.Type = 2;
            pi.Len = software.Length;
            pi.Value = new ASCIIEncoding().GetBytes(software);
            image.SetPropertyItem(pi);
            // Artist
            string artist = "ChemSpider";
            pi.Id = 0x013B;
            pi.Type = 2;
            pi.Len = artist.Length;
            pi.Value = new ASCIIEncoding().GetBytes(artist);
            image.SetPropertyItem(pi);

            ArrayList properties = s_csdb.DBU.m_fetchRow(String.Format("select i.inchi, i5.inchi_key from inchis i, inchis_md5 i5 where i.cmp_id = i5.cmp_id and i.cmp_id={0}", cmp_id));
            if ( properties.Count > 0 ) {
                // ImageTitle
                if ( properties[0] != null ) {
                    string inchi = properties[0].ToString();
                    pi.Id = 0x0320;
                    pi.Type = 2;
                    pi.Len = inchi.Length + 1;
                    pi.Value = new ASCIIEncoding().GetBytes(inchi);
                    image.SetPropertyItem(pi);
                }
                // ImageDescription
                if ( properties[1] != null ) {
                    string inchi_key = properties[1].ToString();
                    pi.Id = 0x010E;
                    pi.Type = 2;
                    pi.Len = inchi_key.Length + 1;
                    pi.Value = new ASCIIEncoding().GetBytes(inchi_key);
                    image.SetPropertyItem(pi);
                }
                // TODO: Add SMILES here
            }

            MemoryStream ms2 = new MemoryStream();
            image.Save(ms2, ImageFormat.Png);
            buffer = ms2.GetBuffer();
            return buffer;
        }

        public static string RenderXmlAsHtml(object o)
        {
            string s = o != null ? o.ToString() : null;
            if ( string.IsNullOrEmpty(s) )
                return string.Empty;
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(s);

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            XmlTextWriter xtw = null;
            try {
                xtw = new XmlTextWriter(sw);
                xtw.Formatting = Formatting.Indented;
                xd.WriteTo(xtw);
            }
            finally {
                if ( xtw != null )
                    xtw.Close();
            }

            return HttpUtility.HtmlEncode(sb.ToString());
        }

        public static string VirtualPath(string path)
        {
            string virPath = HttpRuntime.AppDomainAppVirtualPath;
            if ( !virPath.EndsWith("/") && !virPath.EndsWith(@"\") )
                virPath += "/";

            if ( !path.StartsWith("/") && !path.StartsWith(@"\") )
                virPath += path;
            else
                virPath += path.Substring(1);

            return virPath;
        }

        public static string FullServerPath(string page)
        {
            string path = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

            if ( !path.EndsWith("/") && !path.EndsWith(@"\") )
                path += "/";

            string virPath = VirtualPath(page);
            if ( !virPath.StartsWith("/") && !virPath.StartsWith(@"\") )
                path += virPath;
            else
                path += virPath.Substring(1);

            return path;
        }

        public static Dictionary<string,string> calcProperties(string mol)
        {
            return MolExtProc.calcProperties(mol, false);
        }

        public static string GetDepositionFilePath(HttpContext context, int file_id, int dep_id)
        {
            Hashtable args = new Hashtable();
            args["@dfid"] = file_id;
            args["@dep_id"] = dep_id;
            ChemSpiderDB csdb = new ChemSpiderDB();
            DataTable dt = csdb.DBU.m_fillDataTable(
                @"select top 1 d.dep_id, usr_id, data_dir, df.file_name, df.typ
                from depositions d, v_depositions_files df
               where d.dep_id = df.dep_id
                 and (@dfid > 0 and dfid = @dfid or @dfid = 0 and df.typ = 'INP' and df.dep_id = @dep_id)", args);
            if ( dt.Rows.Count == 0 ) {
                throw new Exception("File not found");
            }

            string filename = dt.Rows[0][3].ToString();
            filename = Path.Combine(dt.Rows[0][2].ToString(), Path.GetFileNameWithoutExtension(filename) + ".sdf");
            int usr_id = (int)dt.Rows[0][1];
            bool submitter = usr_id == (int)context.Profile.GetPropertyValue("ID");
            bool curator = context.User.IsInRole("Master Curator");
            string typ = dt.Rows[0][4].ToString();

            if ( !submitter && !curator ) {
                throw new Exception("Access denied");
            }

            if ( !curator && typ == "PRC" )
                throw new Exception("Access denied");


            return Path.Combine(ConfigurationManager.AppSettings["data_dir"], filename);
        }

        static XmlDocument GetEPlatformXMLRequest(int csid, int page, int pageSize, string category, string db, out string searchterm)
        {
            //Pass get_rsc flag into GetValidatedSynonyms().
            List<string> synonyms = NameUtility.GetValidatedSynonyms(csid, NameUtility.ValidatedSynonymsSelector.RSC);
            StringBuilder criteria = new StringBuilder();
            searchterm = null;

            if ( synonyms.Count > 0 ) {
                foreach ( string synonym in synonyms ) {
                    if ( criteria.Length == 0 ) {
                        criteria.AppendFormat("\"{0}\"", synonym);
                    }
                    else {
                        criteria.AppendFormat("OR\"{0}\"", synonym);
                    }
                }
                searchterm = HttpUtility.UrlEncode(criteria.ToString());
            }

            XmlDocument xmlRequest = new XmlDocument();

            XmlNode searchCriteria = xmlRequest.CreateElement("SearchCriteria");
            xmlRequest.AppendChild(searchCriteria);

            XmlNode term = xmlRequest.CreateElement("SearchTerm");
            searchCriteria.AppendChild(term);

            /*
                Õ	Journal - searches across RSC Journal Articles
                Õ	Books - searches across RSC Book Chapters
                Õ	Databases - searches across Database Content
                Õ	The above values can be combined. For example: JournalandBooks
                Õ	All Ö searches across RSC Journal Articles, RSC Book Chapters and Database Content
             */
            term.AppendChild(xmlRequest.CreateElement("Category")).InnerText = string.IsNullOrEmpty(category) ? "All" : category;
            term.AppendChild(xmlRequest.CreateElement("ContentType")).InnerText = "All";

            XmlNode criterias = xmlRequest.CreateElement("Criterias");
            term.AppendChild(criterias);

            XmlNode nameValue = xmlRequest.CreateElement("NameValue");
            criterias.AppendChild(nameValue);

            nameValue.AppendChild(xmlRequest.CreateElement("Name")).InnerText = "FreeText";
            nameValue.AppendChild(xmlRequest.CreateElement("Value")).InnerText = criteria.ToString();

            /*
                MSB	    - Mass Spectrometry Bulletin
                HAZ	    - Laboratory Hazards Bulletin
                MOS	    - Methods in Organic Synthesis
                CCR	    - Catalysts & Catalysed Reactions
                NPU	    - Natural Product Updates
                AWB	    - Analytical Abstracts
            */
            if ( !string.IsNullOrEmpty(db) ) {
                string excluded_dbs = string.Empty;
                if ( !db.Equals("MSB") )
                    excluded_dbs = "MSB";
                if ( !db.Equals("HAZ") )
                    excluded_dbs = string.IsNullOrEmpty(excluded_dbs) ? "HAZ" : excluded_dbs + "|HAZ";
                if ( !db.Equals("MOS") )
                    excluded_dbs = string.IsNullOrEmpty(excluded_dbs) ? "MOS" : excluded_dbs + "|MOS";
                if ( !db.Equals("CCR") )
                    excluded_dbs = string.IsNullOrEmpty(excluded_dbs) ? "CCR" : excluded_dbs + "|CCR";
                if ( !db.Equals("NPU") )
                    excluded_dbs = string.IsNullOrEmpty(excluded_dbs) ? "NPU" : excluded_dbs + "|NPU";
                if ( !db.Equals("AWB") )
                    excluded_dbs = string.IsNullOrEmpty(excluded_dbs) ? "AWB" : excluded_dbs + "|AWB";

                XmlNode excludes = xmlRequest.CreateElement("Excludes");
                term.AppendChild(excludes);

                nameValue = xmlRequest.CreateElement("NameValue");
                excludes.AppendChild(nameValue);

                nameValue.AppendChild(xmlRequest.CreateElement("Name")).InnerText = "database";
                nameValue.AppendChild(xmlRequest.CreateElement("Value")).InnerText = excluded_dbs;
            }

            term.AppendChild(xmlRequest.CreateElement("Source")).InnerText = "ChemSpider";

            searchCriteria.AppendChild(xmlRequest.CreateElement("PageNo")).InnerText = page < 1 ? "1" : page.ToString();
            searchCriteria.AppendChild(xmlRequest.CreateElement("PageSize")).InnerText = pageSize.ToString();
            searchCriteria.AppendChild(xmlRequest.CreateElement("SortBy")).InnerText = "Relevance";
            /*
                    <SearchCriteria>
                        <SearchTerm>
                            <Category>All</Category>
                            <ContentType>All</ContentType>
                            <Criterias>
                                <NameValue>
                                    <Name>FreeText</Name>
                                    <Value>bucky</Value>
                                </NameValue>
                            </Criterias>
                            <Excludes>
                                <NameValue>
                                    <Name>database</Name>
                                    <Value>MSB|HAZ|CCR|MOS|NPU</Value>
                                </NameValue>
                            </Excludes>
                            <Source>ChemSpider</Source>
                        </SearchTerm>
                        <PageNo>1</PageNo>
                        <PageSize>10</PageSize>
                        <SortBy>Relevance</SortBy>
                    </SearchCriteria> 
             */
            return xmlRequest;
        }

        public static int GetEPlatform(int csid, int page, string category, string db, out XmlDocument data, out XmlDocument schema, out string term)
        {
            int count = 0;
            return GetEPlatform(csid, page, 10, category, db, out data, out schema, out term, out count);
        }

        public static int GetEPlatform(int csid, int page, int pageSize, string category, string db, out XmlDocument data, out XmlDocument schema, out string term, out int count)
        {
            const string EPLATFORM_URL = "http://pubs.rsc.org/en/federated/search";

            data = new XmlDocument();
            schema = new XmlDocument();
            term = null;
            count = 0;

            //if ( String.IsNullOrEmpty(ConfigurationManager.AppSettings["eplatform_url"]) )
            //    return 0;

            // Try to find in cache
            string cache_key = String.Format("eplatform-{0}-{1}{2}{3}", csid, page, category, db);
            if ( HttpContext.Current != null && HttpContext.Current.Cache[cache_key] != null ) {
                data = HttpContext.Current.Cache[cache_key] as XmlDocument;
                return 0;
            }

            try {
                XmlDocument xmlRequest = GetEPlatformXMLRequest(csid, page, pageSize, category, db, out term);

                string eplatform_url = ConfigurationManager.AppSettings["eplatform_url"] ?? EPLATFORM_URL;
                // Create a request for the URL.
                WebRequest webRequest = WebRequest.Create(string.Format("{0}?federatedsearchname=ChemSpider&inputxml={1}", eplatform_url, HttpUtility.UrlEncode(xmlRequest.OuterXml)));
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                webRequest.Timeout = 100000;
                string data_xml = string.Empty;
                using ( HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse() ) {
                    using ( Stream dataStream = response.GetResponseStream() ) {
                        using ( StreamReader reader = new StreamReader(dataStream) ) {
                            data_xml = reader.ReadToEnd();
                        }
                    }
                }

                if ( !string.IsNullOrEmpty(data_xml) ) {
                    data.LoadXml(data_xml);

                    XmlNamespaceManager xnm = new XmlNamespaceManager(data.NameTable);
                    xnm.AddNamespace("cnt", "http://www.rsc.org/schema/searchresultscount");
                    XmlNode countNode = data.SelectSingleNode("//cnt:counts/cnt:totalResults", xnm);
                    if ( countNode != null )
                        Int32.TryParse(countNode.InnerText, out count);

                    data.DocumentElement.AppendChild(data.ImportNode(xmlRequest.DocumentElement, true));
                }

                if ( HttpContext.Current != null )
                    HttpContext.Current.Cache.Add(cache_key, data, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(1), CacheItemPriority.Default, null);
            }
            catch ( Exception ) {
                //            throw new ChemSpiderException(ex.Message);
                return -1;
            }

            return 0;
        }

        //Jon Steele 10-Nov-2010 - Returns the compound meta description for the page when the sys_name and title are unknown.
        public static string GetCompoundPageDescription(int csid, ref string sys_name, ref string title, string prefix)
        {
            //Generate the description meta tag using the compound properties.
            ChemSpiderDB csdb = new ChemSpiderDB();
            List<string> cmp_props = csdb.DBU.m_fetchRow<string>(String.Format(@"
				            SELECT dbo.fGetSysName(c.cmp_id) AS sys_name, 
                                   dbo.fGetCompoundTitle(c.cmp_id) as title
                                FROM compounds c 
                                WHERE c.cmp_id = " + csid));

            //Populate the output parameters for re-use.
            sys_name = cmp_props[0];
            title = cmp_props[1];

            return GetCompoundPageDescription(csid, sys_name, title, prefix);
        }

        //Jon Steele 10-Nov-2010 - Returns the compound meta description for the page.
        public static string GetCompoundPageDescription(int csid, string sys_name, string title, string prefix)
        {
            //Do not exceed the maximum length of 154 chars (plus a full  stop = 155)
            const int MAX_LENGTH = 154;

            StringBuilder description = new StringBuilder(prefix);
            string compound_title = GetPreferredName(csid, sys_name, title);

            //If the title isn't very long then add it otherwise only add enough of it to get up the limit.                
            description.Append(description.Length + compound_title.Length > MAX_LENGTH ? compound_title.Substring(0, ( MAX_LENGTH - description.Length ) - 1) : compound_title);

            List<string> synonyms = new List<string>();
            synonyms = NameUtility.GetValidatedSynonyms(csid, NameUtility.ValidatedSynonymsSelector.SEO);

            foreach ( string synonym in synonyms.Where((synonym, index) => synonym != compound_title) ) {
                //Ensure we won't go over the maximum description length of 155 chars.
                if ( ( description.Length + synonym.Length + 3 <= MAX_LENGTH ) ) {
                    description.Append(", " + synonym);
                }
                else
                    break;
            }

            //Always add full stop at the end of description.
            description.Append(".");
            return description.ToString();
        }

        public static string GetCompoundPageKeywords(int csid, string sys_name, string title)
        {
            StringBuilder keywords = new StringBuilder();
            string compound_title = title != string.Empty ? title : sys_name;

            keywords.Append(compound_title);

            List<string> synonyms = new List<string>();
            synonyms = NameUtility.GetValidatedSynonyms(csid, NameUtility.ValidatedSynonymsSelector.SEO);

            //Get the first 10 seo compliant validated synonyms and add them to keywords.
            foreach ( string synonym in synonyms.Where((synonym, index) => index < 10 && synonym != compound_title) ) {
                keywords.Append(", " + synonym);
            }
            return keywords.ToString();
        }

        //Set the properties of an image given the mode and csid.
        public static void SetCompoundImageProperties(int csid, System.Web.UI.WebControls.Image img, string mode)
        {
            string sys_name = string.Empty;
            string title = string.Empty;
            string mf = string.Empty;
            SetCompoundImageProperties(csid, img, mode, ref sys_name, ref title, ref mf);
        }

        //Set the properties of an image given the mode and csid and populate reference parameters for further use.
        public static void SetCompoundImageProperties(int csid, System.Web.UI.WebControls.Image img, string mode, ref string sys_name, ref string title, ref string mf)
        {
            img.AlternateText =  String.Format("ChemSpider {0} Image | {1}", mode, GetCompoundPageTitle(csid, ref sys_name, ref title, ref mf));
            img.Attributes.Add("Title", GetCompoundPageDescription(csid, sys_name, title, String.Format("{0} Image for: ", mode)));
        }

        //Jon Steele 10-Nov-2010 - Returns the compound title for the page.
        public static string GetCompoundPageTitle(int csid, ref string sys_name, ref string title, ref string molecular_formula)
        {
            //Generate the description meta tag using the compound properties.
            ChemSpiderDB csdb = new ChemSpiderDB();
            List<string> cmp_props = csdb.DBU.m_fetchRow<string>(String.Format(@"
				            SELECT dbo.fGetSysName(c.cmp_id) AS sys_name, 
                                   dbo.fGetCompoundTitle(c.cmp_id) as title,
                                   c.PUBCHEM_OPENEYE_MF
                                FROM compounds c 
                                WHERE c.cmp_id = " + csid));
            
            //Populate the output parameters for re-use.
            sys_name = cmp_props[0];
            title = cmp_props[1];
            molecular_formula = Utility.decodeACDSymbols(cmp_props[2], Utility.EDecodeStyle.eDecodeRemove);

            //Reurn the formatted page title.
            return GetCompoundPageTitle(csid, sys_name, title, molecular_formula);
        }

        //Jon Steele 10-Nov-2010 - Returns the compound title for the page.
        public static string GetCompoundPageTitle(int csid, string sys_name, string title, string molecular_formula)
        {
            string page_title = GetPreferredName(csid, sys_name, title);

            //Add molecular formula if populated.
            if (molecular_formula != string.Empty)
            {
                page_title = page_title + " | " + molecular_formula;
            }
            return page_title;
        }
        
        //Can either be title, sys_name or csid.
        public static string GetPreferredName(int csid, string sys_name, string title)
        {
            return title != string.Empty ? title : sys_name != string.Empty ? sys_name : string.Format("CSID:{0}",csid);
        }

        //Returns the Citation for a particular CSID.
        //Uses the following format: "CSID:2157, http://www.chemspider.com/Chemical-Structure.2157.html (accessed 13:38, Feb 2, 2012)"
        public static string GetCitation(int csid)
        {
            return String.Format("CSID:{0}, http://www.chemspider.com/Chemical-Structure.{0}.html (accessed {1})", csid, DateTime.Now.ToString("HH:mm, MMM d, yyyy"));
        }
    }
}
