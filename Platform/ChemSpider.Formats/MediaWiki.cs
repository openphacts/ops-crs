using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data.SqlClient;
using ChemSpider.Database;
using ChemSpider.Data.Database;
using ChemSpider.Utilities;
using System.Xml;
using System.Text.RegularExpressions;

namespace ChemSpider.Formats
{
    public class MediaWiki
    {
        private static ChemSpiderDB s_csdb
        {
            get { return new ChemSpiderDB(); }
        }

        private static ChemSpiderBlobsDB s_csbdb
        {
            get { return new ChemSpiderBlobsDB(); }
        }

        private static string EduWiki =
@"*'''[http://www.chemspider.com/Chemical-Structure.{ChemSpiderID}.html Main ChemSpider page]'''
*Molecular formula: {chemical_formula}
*Molar mass: {molecular_weight}
*CAS Registry Number: {CAS_number}
*Appearance: {appearance}
*Melting point: {melting_point}
*Boiling point: {boiling_point}
*Solubility: {solubility}
*Safety sheet: {safety}
*Spectra: {spectra}
";

        public static string getEduWikiInfo(int? csid)
        {
            string output = null;
            try {
                output = getWikiboxText(EduWiki, csid);
                return output;
            }
            catch {
                return null;
            }
        }

        public static string getWikiboxText(string template, int? CSID)
        {
            StringBuilder sb = new StringBuilder(template);
            Hashtable args = new Hashtable();
            args.Add("cmp_id", CSID);

            using ( SqlDataReader r = s_csdb.DBU.m_executeReader("GetWikiboxInfo", args) ) {
                if ( r.Read() ) {
                    setTmplParamValue(sb, r, "IUPAC_name", template);
                    setTmplParamValue(sb, r, "inchi", template);
                    setTmplParamValue(sb, r, "inchikey", template);
                    setTmplParamValue(sb, r, "stdinchi", template);
                    setTmplParamValue(sb, r, "stdinchikey", template);
                    setTmplParamValue(sb, r, "CAS_number", template);
                    setTmplParamValue(sb, r, "einecs", template);
                    setTmplParamValue(sb, r, "PubChem", template);
                    setTmplParamValue(sb, r, "DrugBank", template);
                    setTmplParamValue(sb, r, "mesh", template);
                    setTmplParamValue(sb, r, "chebi", template);
                    setTmplParamValue(sb, r, "kegg", template);
                    setTmplParamValue(sb, r, "molecular_weight", template);
                    setTmplParamValue(sb, r, "chemical_formula", template);
                    setTmplParamValue(sb, r, "smiles", template);
                    setTmplParamValue(sb, r, "DailyMedID", template);
                    sb.Replace("{ChemSpiderID}", CSID.ToString());

                    //if (!(r["chemical_formula"] is DBNull) && !String.IsNullOrEmpty(r["chemical_formula"].ToString().Trim()))
                    //    sb.Replace("{chemical_formula}", r["chemical_formula"].ToString().Replace("_{", "").Replace("}", ""));
                    //else
                    //    sb.Replace("{chemical_formula}", "");
                }
            }
            //first tries to get the solubility from the epi predicted properties (if this doesn't have a value then it tries to find a value in the supplementary information xml below
            addSolubilityValue(sb, Convert.ToInt32(CSID));
            //Now gets properties that are in supplementary information xml
            using ( SqlDataReader r = s_csdb.DBU.m_executeReader("cmp_get_supp_info_xml", args) ) {
                if ( r.Read() ) {
                    setTmplParamXMLValue(sb, r, "appearance", template);
                    setTmplParamXMLValue(sb, r, "melting_point", template);
                    setTmplParamXMLValue(sb, r, "boiling_point", template);
                    setTmplParamXMLValue(sb, r, "solubility", template);
                    setTmplParamXMLValue(sb, r, "safety", template);
                }
            }
            addSpectra(sb, Convert.ToInt32(CSID));
            return sb.ToString();
        }

        private static void setTmplParamValue(StringBuilder sb, SqlDataReader r, string name, string template)
        {
            if ( !( r[name] is DBNull ) && !String.IsNullOrEmpty(r[name].ToString()) ) {
                if ( name == "molecular_weight" )
                    sb.Replace("{" + name + "}", String.Format("{0:f3}", r[name]));
                if ( name == "chemical_formula" )
                    sb.Replace("{" + name + "}", Utility.decodeACDSymbols(r[name].ToString(), Utility.EDecodeStyle.eDecodeHtml));
                if ( name == "IUPAC_name" )
                    sb.Replace("{" + name + "}", Utility.decodeACDSymbols(r[name].ToString(), Utility.EDecodeStyle.eDecodeHtml).Replace("\n", "").Replace("\r", "").Replace("'", "’"));
                if ( ( name == "inchi" ) || ( name == "stdinchi" ) )
                    sb.Replace("{" + name + "}", r[name].ToString().Replace("InChI=", ""));
                else
                    sb.Replace("{" + name + "}", r[name].ToString());
            }
            else {
                if ( template == EduWiki )
                    sb.Replace("{" + name + "}", "Not available");
                else
                    sb.Replace("{" + name + "}", "");
            }
        }

        private static void setTmplParamXMLValue(StringBuilder sb, SqlDataReader r, string name, string template)
        {
            if ( !( r["result"] is DBNull ) && !String.IsNullOrEmpty(r["result"].ToString()) ) {
                XmlDocument xml_userdatatree = new XmlDocument();
                xml_userdatatree.LoadXml(r["result"].ToString());
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml_userdatatree.NameTable);
                nsmgr.AddNamespace("userdata", "chemspider:xmlns:user-data");
                if ( name == "appearance" )
                    sb.Replace("{" + name + "}", getAppearanceFromUserDataTree(xml_userdatatree, nsmgr).Replace("\n", "").Replace("\r", ""));
                else if ( name == "melting_point" )
                    sb.Replace("{" + name + "}", getMeltingOrBoilingPointFromUserDataTree(xml_userdatatree, nsmgr, "experimental-melting-point"));
                else if ( name == "boiling_point" )
                    sb.Replace("{" + name + "}", getMeltingOrBoilingPointFromUserDataTree(xml_userdatatree, nsmgr, "experimental-boiling-point"));
                else if ( name == "solubility" )
                    sb.Replace("{" + name + "}", getSolubilityFromUserDataTree(xml_userdatatree, nsmgr, "experimental-solubility"));
                else if ( name == "safety" )
                    sb.Replace("{" + name + "}", getSafetyFromUserDataTree(xml_userdatatree, nsmgr));
            }
            else
                sb.Replace("{" + name + "}", "Not available");

        }

        private static string getAppearanceFromUserDataTree(XmlDocument xml_userdatatree, XmlNamespaceManager nsmgr)
        {
            if ( xml_userdatatree == null ) {
                return "Not available";
            }
            try {
                string output = "";
                XmlNodeList xnList = null;
                xnList = xml_userdatatree.SelectNodes("/cs-record/userdata:user-data-tree/userdata:categories/userdata:miscellaneous/userdata:appearance", nsmgr);
                foreach ( XmlNode xn in xnList ) {
                    output = output + "; " + xn.Attributes["value"].Value.Trim();
                }
                output = output.TrimStart("; ".ToCharArray());
                if ( output == "" )
                    output = "Not available";
                return output;
            }
            catch {
                return "Not available";
            }

        }

        private static string getMeltingOrBoilingPointFromUserDataTree(XmlDocument xml_userdatatree, XmlNamespaceManager nsmgr, string propertyname)
        {
            if ( xml_userdatatree == null ) {
                return "Not available";
            }
            try {
                string output = "";
                double overallmin = 1000000;
                double overallmax = -1000000;
                string overallunit = "";
                double nounitoverallmin = 1000000;
                double nounitoverallmax = -1000000;
                double thismin;
                double thismax;
                string thisunit;
                string thisunitfromcommonmeta = "";
                XmlNodeList xnList = null;
                xnList = xml_userdatatree.SelectNodes("/cs-record/userdata:user-data-tree/userdata:categories/userdata:experimental-physchem-properties/userdata:" + propertyname, nsmgr);
                foreach ( XmlNode xn in xnList ) {
                    splitIntoMinMaxUnit(xn.Attributes["value"].Value.Trim(), out thismin, out thismax, out thisunit);
                    XmlNode node = xn.Attributes["units_Id"];
                    if ( node != null )
                        thisunitfromcommonmeta = getCommonData(xn.Attributes["units_Id"].Value.Trim(), xml_userdatatree, nsmgr, "melting_point");
                    if ( ( thisunitfromcommonmeta != "" ) && ( !string.IsNullOrEmpty(thisunitfromcommonmeta) ) )
                        thisunit = thisunitfromcommonmeta;
                    if ( ( thisunit != "" ) && ( !string.IsNullOrEmpty(thisunit) ) ) {
                        if ( ( thisunit == "°C" ) ) {
                            if ( thismin < overallmin )
                                overallmin = thismin;
                            if ( thismax > overallmax )
                                overallmax = thismax;
                            overallunit = thisunit;
                        }
                    }
                    else {
                        if ( thismin < nounitoverallmin )
                            nounitoverallmin = thismin;
                        if ( thismax > nounitoverallmax )
                            nounitoverallmax = thismax;
                    }
                }
                if ( ( overallunit != "" ) && ( !string.IsNullOrEmpty(overallunit) ) ) {
                    if ( overallmin != 1000000 ) {
                        if ( overallmin == overallmax )
                            output = overallmin.ToString() + " " + overallunit.ToString();
                        else
                            output = overallmin.ToString() + " to " + overallmax.ToString() + " " + overallunit;
                    }
                    else {
                        output = "Not available";
                    }
                }
                else {
                    if ( nounitoverallmin != 1000000 ) {
                        if ( nounitoverallmin == nounitoverallmax )
                            output = nounitoverallmin.ToString();
                        else
                            output = nounitoverallmin.ToString() + " to " + nounitoverallmax.ToString();
                    }
                    else {
                        output = "Not available";
                    }
                }
                return output;
            }
            catch {
                return "Not available";
            }

        }

        private static void splitIntoMinMaxUnit(string thisvalue, out double thismin, out double thismax, out string thisunit)
        {
            thismin = 1000000;
            thismax = -1000000;
            thisunit = "";
            if ( String.IsNullOrEmpty(thisvalue) ) {
                return;
            }
            try {
                //ignores open ended ranges
                if ( new Regex("^[\\s]*[\\>\\<]+").IsMatch(thisvalue) ) {
                    //do nothing
                }
                //first for case where it's just a number e.g. 88 or -88 or -1.680000000000000e+002 followed by a unit
                else if ( new Regex("^[\\s]*[^0-9\\-\\<]*[\\s]*[^0-9\\-\\<]*[\\s]*[\\-]*[0-9eE\\+\\.]+[\\s]*[^0-9]*[\\s]*$").IsMatch(thisvalue) ) {
                    thismin = returnregexmatch_dbl(thisvalue, new Regex("^[\\s]*[^0-9\\-\\<]*[\\s]*(?<item>[\\-]*[0-9eE\\+\\.]+)[\\s]*[^0-9]*[\\s]*$"));
                    if ( thismin == 1000000 )
                        thismax = -1000000;
                    else
                        thismax = thismin;
                    thisunit = tidyTemperatureUnits(returnregexmatch_str(thisvalue, new Regex("^[\\s]*[^0-9\\-\\<]*[\\s]*[\\-]*[0-9eE\\+\\.]+[\\s]*(?<item>[^0-9]*)[\\s]*$"), "single"));
                    return;
                }
                // now e.g. two numbers separated by a - and followed by a unit
                else if ( new Regex("^[\\s]*[^0-9\\-\\<]*[\\s]*[\\-]*[0-9eE\\+\\.]+[\\s]*\\-[\\s]*[0-9eE\\+\\.]+[\\s]*[^0-9]*[\\s]*$").IsMatch(thisvalue) ) {
                    thismin = returnregexmatch_dbl(thisvalue, new Regex("^[\\s]*[^0-9\\-\\<]*[\\s]*(?<item>[\\-]*[0-9eE\\+\\.]+)+[\\s]*\\-[\\s]*[0-9eE\\+\\.]+[\\s]*[^0-9]*[\\s]*$"));
                    if ( thismin == 1000000 )
                        thismax = -1000000;
                    else
                        thismax = thismin;
                    thismax = returnregexmatch_dbl(thisvalue, new Regex("^[\\s]*[^0-9\\-\\<]*[\\s]*[\\-]*[0-9eE\\+\\.]+[\\s]*\\-[\\s]*(?<item>[0-9eE\\+\\.]+)[\\s]*[^0-9]*[\\s]*$"));
                    thisunit = tidyTemperatureUnits(returnregexmatch_str(thisvalue, new Regex("^[\\s]*[^0-9\\-\\<]*[\\s]*[\\-]*[0-9eE\\+\\.]+[\\s]*\\-[\\s]*[0-9eE\\+\\.]+[\\s]*(?<item>[^0-9]*)[\\s]*$"), "single"));
                    return;
                }
                //ignores open ended ranges e.g. >360 dec. (it's not worth the extra work getting these to work
                else {
                    return;
                }
            }
            catch {
                return;
            }

        }

        public static string returnregexmatch_str(string inputstring, Regex regex, string singleormultiple)
        {
            try {
                string str_output = "";
                MatchCollection theMatches = regex.Matches(inputstring);
                foreach ( Match m in theMatches ) {
                    if ( singleormultiple == "multiple" )
                        str_output = str_output + ", " + m.Groups[1].Value;
                    else
                        str_output = m.Groups[1].Value;
                }
                if ( singleormultiple == "multiple" )
                    str_output = str_output.TrimStart(", ".ToCharArray());
                return str_output;
            }
            catch {
                return "";
            }
        }


        public static double returnregexmatch_dbl(string inputstring, Regex regex)
        {
            try {
                double dbl_output = 1000000;
                MatchCollection theMatches = regex.Matches(inputstring);
                foreach ( Match m in theMatches ) {
                    if ( Double.TryParse(m.Groups[1].Value, out dbl_output) )
                        return dbl_output;
                    else
                        return 1000000;
                }
                return dbl_output;
            }
            catch {
                return 1000000;
            }
        }

        public static string tidyTemperatureUnits(string inputstring)
        {
            if ( string.IsNullOrEmpty(inputstring) ) {
                return "";
            }
            else {
                string outputstring;
                if ( ( inputstring == "°C" ) || ( inputstring == "C" ) || ( inputstring == "C." ) || ( inputstring == "dec" ) || ( inputstring == "dec." ) )
                    outputstring = "°C";
                else if ( ( inputstring == "°C" ) || ( inputstring == "F" ) || ( inputstring == "F." ) )
                    outputstring = "°F";
                else
                    outputstring = "";
                return outputstring;
            }
        }

        public static string getCommonData(string inputUnitId, XmlDocument xml_userdatatree, XmlNamespaceManager nsmgr, string propertyname)
        {
            if ( String.IsNullOrEmpty(inputUnitId) )
                return "";
            else {
                try {
                    string outputstring = "";
                    XmlNodeList xnUnitsList = null;
                    xnUnitsList = xml_userdatatree.SelectNodes("/cs-record/userdata:user-data-tree/userdata:common-meta/userdata:units[@units_Id=" + inputUnitId + "]", nsmgr);
                    foreach ( XmlNode xnUnit in xnUnitsList ) {
                        XmlNode node = xnUnit.Attributes["value"];
                        if ( node != null ) {
                            if ( propertyname == "experimental-melting-point" )
                                outputstring = tidyTemperatureUnits(xnUnit.Attributes["value"].Value.Trim());
                            if ( propertyname == "experimental-boiling-point" )
                                outputstring = tidyTemperatureUnits(xnUnit.Attributes["value"].Value.Trim());
                            else
                                outputstring = xnUnit.Attributes["value"].Value.Trim();
                        }
                    }
                    return outputstring;
                }
                catch {
                    return "";
                }
            }
        }

        private static void addSolubilityValue(StringBuilder sb, int CSID)
        {
            try {
                string epiText = s_csbdb.getEpiText(CSID);
                if ( !String.IsNullOrEmpty(epiText) ) {
                    epiText = returnregexmatch_str(epiText, new Regex("[\\n\\r]\\s*Water Sol \\(Exper\\. database match\\) =[\\s]*(?<item>[^\\n\\r]+)[\\s]*[\\n\\r]"), "multiple");
                    if ( !String.IsNullOrEmpty(epiText) )
                        sb.Replace("{solubility}", "Water, " + epiText);
                }
                else {
                    // does nothing
                }
            }
            catch {
                // does nothing
            }
        }

        private static string getSolubilityFromUserDataTree(XmlDocument xml_userdatatree, XmlNamespaceManager nsmgr, string propertyname)
        {
            if ( xml_userdatatree == null ) {
                return "Not available";
            }
            try {
                string output = "";
                XmlNodeList xnList = null;
                xnList = xml_userdatatree.SelectNodes("/cs-record/userdata:user-data-tree/userdata:categories/userdata:experimental-physchem-properties/userdata:" + propertyname, nsmgr);
                foreach ( XmlNode xn in xnList ) {
                    output = xn.Attributes["value"].Value.Trim();
                    if ( !String.IsNullOrEmpty(output) ) {
                        XmlNode node = xn.Attributes["units_Id"];
                        if ( node != null )
                            output = output + " " + getCommonData(xn.Attributes["units_Id"].Value.Trim(), xml_userdatatree, nsmgr, "solubility");
                        node = xn.Attributes["conditions_Id"];
                        if ( node != null )
                            output = output + " (" + getCommonData(xn.Attributes["conditions_Id"].Value.Trim() + ")", xml_userdatatree, nsmgr, "solubility");
                    }
                }
                if ( output == "" )
                    return "Not available";
                else
                    return output;
            }
            catch {
                return "Not available";
            }
        }

        private static string getSafetyFromUserDataTree(XmlDocument xml_userdatatree, XmlNamespaceManager nsmgr)
        {
            if ( xml_userdatatree == null ) {
                return "Not available";
            }
            try {
                string output = "";
                XmlNodeList xnList = null;
                XmlNodeList xnSubList = null;
                xnList = xml_userdatatree.SelectNodes("/cs-record/userdata:user-data-tree/userdata:categories/userdata:miscellaneous/userdata:safety", nsmgr);
                foreach ( XmlNode xn in xnList ) {
                    if ( !String.IsNullOrEmpty(xn.Attributes["value"].Value.Trim()) ) {
                        XmlNode node = xn.Attributes["sub_id"];
                        if ( node != null )
                            xnSubList = null;
                        xnSubList = xml_userdatatree.SelectNodes("/cs-record/substances/substance[@sub_id=" + xn.Attributes["sub_id"].Value.Trim() + "]", nsmgr);
                        foreach ( XmlNode xnSub in xnSubList ) {
                            if ( xnSub.Attributes["ext_url"].Value.Trim().StartsWith("http://msds.chem.ox.ac.uk/") )
                                output = "[" + xnSub.Attributes["ext_url"].Value.Trim() + " From Oxford University]";
                        }
                    }
                }
                if ( output == "" )
                    output = "Not available";
                return output;
            }
            catch {
                return "Not available";
            }

        }

        private static void addSpectra(StringBuilder sb, int CSID)
        {
            try {
                string output = "";

                List<string> IRspectraCS = GetChemSpiderSpectra(CSID, "IR");
                List<string> HNMRspectraCS = GetChemSpiderSpectra(CSID, "HNMR");
                List<string> CspectraCS = GetChemSpiderSpectra(CSID, "CNMR");
                List<string> MSspectraCS = GetChemSpiderSpectra(CSID, "APCI+");
                List<string> UVspectraCS = GetChemSpiderSpectra(CSID, "UV-Vis");

                if ( IRspectraCS != null || HNMRspectraCS != null || CspectraCS != null || MSspectraCS != null || UVspectraCS != null ) {
                    if ( IRspectraCS.Count + HNMRspectraCS.Count + CspectraCS.Count + MSspectraCS.Count + UVspectraCS.Count > 0 ) {
                        output = output + "ChemSpider (";
                        output = output + AddToSpectraOutput(IRspectraCS, "", "IR");
                        output = output + AddToSpectraOutput(HNMRspectraCS, "", "<sup>1</sup>H NMR");
                        output = output + AddToSpectraOutput(CspectraCS, "", "<sup>13</sup>C NMR");
                        output = output + AddToSpectraOutput(MSspectraCS, "", "MS");
                        output = output + AddToSpectraOutput(UVspectraCS, "", "UV");
                        output = output.TrimEnd(", ".ToCharArray());
                        output = output + "), ";
                    }
                }

                //add NMRShift spectra to CNMR values
                List<string> NMRShiftSpectra = GetExternalSpectraURL(CSID, "NMRShiftDB");
                //add NMRShift spectra to MS values
                List<string> MassBankSpectra = GetExternalSpectraURL(CSID, "MassBank");
                //add NMRShift spectra to MS values
                List<string> NISTWebBookSpectra = GetExternalSpectraURL(CSID, "NIST Chemistry WebBook Spectra");

                output = output + AddToSpectraOutput(NMRShiftSpectra, "NMRShiftDB", "<sup>13</sup>C NMR");
                output = output + AddToSpectraOutput(MassBankSpectra, "Massbank", "MS");
                output = output + AddToSpectraOutput(NISTWebBookSpectra, "", "NIST Chemistry WebBook");

                output = output.TrimEnd(", ".ToCharArray());

                if ( output != "" )
                    output = output + ", also check on [http://riodb01.ibase.aist.go.jp/sdbs/cgi-bin/cre_index.cgi?lang=eng SDBS]. ";
                else
                    output = "Check on [http://riodb01.ibase.aist.go.jp/sdbs/cgi-bin/cre_index.cgi?lang=eng SDBS]. ";

                output = output + "[http://www.chemspider.com/AddBlob.aspx?id=" + CSID + "&type=spectrum Add Spectra] ([[Help:How to add new spectra|Help]]). ";
                sb.Replace("{spectra}", output);

            }
            catch {
                sb.Replace("{spectra}", "Not available");
            }
        }

        public static List<string> GetChemSpiderSpectra(int CSID, string spc_type)
        {
            List<string> spectra = new List<string>();
            //return int.Parse(s_csdb.DBU.m_querySingleValue(String.Format("select COUNT(cmp_id) from spectra where cmp_id = {0} and spc_type = '{1}'", CSID, spc_type)));
            Hashtable args = new Hashtable();
            args["@cmp_id"] = CSID;
            args["@spc_type"] = spc_type;
            List<string> newspectra = s_csdb.DBU.m_fetchColumn<string>(
                @"select 'http://www.chemspider.com/Spectrum.aspx?spc_id=' + cast(spc_id as nvarchar(max)) 
                    from spectra
                    where cmp_id = @cmp_id 
                     and spc_type = @spc_type
                    and deleted_yn = 0", args, 0);
            foreach ( string thisstring in newspectra ) {
                spectra.Add(thisstring);
            }
            return spectra;
        }

        public static string AddToSpectraOutput(List<string> spectra, string source, string spc_type)
        {
            string output = "";
            if ( spectra != null ) {
                if ( spectra.Count == 0 ) {
                    // do nothing
                }
                else if ( spectra.Count == 1 ) {
                    if ( spectra[0] != "" ) {
                        if ( source != "" )
                            output = output + source + " ";
                        output = output + "[" + spectra[0] + " ";
                        output = output + spc_type;
                        output = output + "]" + ", ";
                    }
                }
                else {
                    if ( source != "" )
                        output = output + source + " ";
                    output = output + spc_type + " (";
                    int counter = 1;
                    foreach ( string thisurl in spectra ) {
                        if ( thisurl != "" )
                            output = output + "[" + thisurl + " " + counter + "], ";
                        counter++;
                    }
                    output = output.TrimEnd(", ".ToCharArray());
                    output = output + "), ";
                }
                return output;
            }

            return output;
        }


        public static List<string> GetExternalSpectraURL(int CSID, string dsn_name)
        {
            List<string> spectra = new List<string>();
            Hashtable args = new Hashtable();
            args["@dsn_name"] = dsn_name;
            args["@cmp_id"] = CSID;
            List<string> newspectra = s_csdb.DBU.m_fetchColumn<string>(
                @"select substances.ext_url
                    from substances
                    join [data_sources]
                    on [substances].[dsn_id] = [data_sources].[dsn_id]
                    where [data_sources].[name] ='" + dsn_name + "' and substances.cmp_id = '" + CSID + "' and substances.deleted_yn = 0 and substances.ext_url Like 'http%' ", args, 0);
            foreach ( string thisstring in newspectra ) {
                spectra.Add(thisstring);
            }
            return spectra;

        }

        public static string getWikipediaForWikiInfo(int? csid)
        {
            string output = "";
            try {
                List<string> names = new List<string>();
                Hashtable args = new Hashtable();
                args["@cmp_id"] = csid;
                names = s_csdb.DBU.m_fetchColumn<string>(@"
				select substances.ext_id
                from substances
                join [data_sources]
                on [substances].[dsn_id] = [data_sources].[dsn_id]
                where [data_sources].[name] ='Wikipedia' and substances.cmp_id = @cmp_id and substances.deleted_yn=0", args, 0);
                if ( names.Count > 0 ) {
                    output = "===From Wikipedia===";
                }
                foreach ( string thisName in names ) {
                    WikiUtility wiki = new WikiUtility(thisName);
                    string wikitext = wiki.getWikiSummary();
                    output = output + "\n" + wiki.wiki2otherwiki(wikitext);
                }
                output = output.TrimStart("\n".ToCharArray());
                return output;
            }
            catch {
                return "";
            }
        }
    }
}
