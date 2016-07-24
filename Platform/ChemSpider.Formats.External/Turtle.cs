using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Configuration;
using ChemSpider.Database;
using VDS.RDF;

namespace ChemSpider.Formats
{
    public enum UnitStyle { QUDT, OBI }

    public class Turtle
    {
        //These need to go in a configuration file E.g. in UtilityProcessor.exe

        //RDF Base prefix.
        public static string RDF_URI = ConfigurationManager.AppSettings["rdf_uri"].ToString();  //E.g. http://ops.rsc-us.org/

        //ChemSpider RDF Base prefix.
        public static string CHEMSPIDER_RDF_URI = ConfigurationManager.AppSettings["chemspider_rdf_uri"].ToString();  //E.g. http://rdf.chemspider.com/

        //Identifier prefix.
        public static string RDF_URI_PREFIX = ConfigurationManager.AppSettings["rdf_uri_prefix"].ToString();  //E.g. /OPS

        //Ftp location.
        public static string FTP_PREFIX = ConfigurationManager.AppSettings["ftp_url"].ToString();  //E.g. ftp://ftp.rsc-us.org/OPS/

        //Dataset Label used for VOID and Linksets.
        public static string DATASET_LABEL = ConfigurationManager.AppSettings["void_dataset_label"].ToString();   //E.g. "ChemSpider" or "OpenPhacts"

        //Used for the skos: predicates.
        public const string SKOS_PREFIX = "skos";
        public const string SKOS_RELATED_MATCH = "relatedMatch";
        public const string SKOS_EXACT_MATCH = "exactMatch";
        public const string SKOS_CLOSE_MATCH = "closeMatch";
        public enum SkosPredicate { RELATED_MATCH, EXACT_MATCH, CLOSE_MATCH }

        //xsd:dateTime format.
        public const string DATE_TIME_FORMAT = "yyyy-MM-dd\\THH:mm:ss\\Z";

        //****************************
        // Namespaces.
        //****************************
        public static Uri ns_dcterms = new Uri("http://purl.org/dc/terms/");
        public static Uri ns_dctype = new Uri("http://purl.org/dc/dcmitype/");
        public static Uri ns_foaf = new Uri("http://xmlns.com/foaf/0.1/");
        public static Uri ns_rdf = new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#");
        public static Uri ns_rdfs = new Uri("http://www.w3.org/2000/01/rdf-schema#");
        public static Uri ns_freq = new Uri("http://purl.org/cld/freq/");
        public static Uri ns_pav = new Uri("http://purl.org/pav/");
        public static Uri ns_void = new Uri("http://rdfs.org/ns/void#");
        public static Uri ns_voag = new Uri("http://voag.linkedmodel.org/schema/voag#");
        public static Uri ns_xsd = new Uri("http://www.w3.org/2001/XMLSchema#");
        public static Uri ns_skos = new Uri("http://www.w3.org/2004/02/skos/core#");
        public static Uri ns_dul = new Uri("http://www.ontologydesignpatterns.org/ont/dul/DUL.owl#");
        public static Uri ns_turtle = new Uri("http://www.w3.org/ns/formats/data/Turtle");
        public static Uri ns_obo = new Uri("http://purl.obolibrary.org/obo/");
        public static Uri ns_obo2 = new Uri("http://purl.obolibrary.org/obo#");
        public static Uri ns_obo_ro = new Uri("http://www.obofoundry.org/ro/ro.owl#");
        public static Uri ns_cheminf = new Uri("http://semanticscience.org/resource/");
        public static Uri ns_qudt = new Uri("http://qudt.org/1.1/schema/qudt#");
        public static Uri ns_prov = new Uri("http://www.w3.org/ns/prov#");
        public static Uri ns_owl = new Uri("http://www.w3.org/2002/07/owl#");

        //****************************
        // Predicates.
        //****************************
        //rdf
        public static Uri has_type = new Uri(ns_rdf.ToString() + "type");

        //dcterms - Dublin Core.
        public static Uri dcterms_title = new Uri(ns_dcterms.ToString() + "title");
        public static Uri dcterms_description = new Uri(ns_dcterms.ToString() + "description");
        public static Uri dcterms_license = new Uri(ns_dcterms.ToString() + "license");
        public static Uri dcterms_publisher = new Uri(ns_dcterms.ToString() + "publisher");
        public static Uri dcterms_created = new Uri(ns_dcterms.ToString() + "created");
        public static Uri dcterms_modified = new Uri(ns_dcterms.ToString() + "modified");
        public static Uri dcterms_subject = new Uri(ns_dcterms.ToString() + "subject");

        //dctype - Dublin Core Types.
        public static Uri dctype_Dataset = new Uri(ns_dctype.ToString() + "Dataset");

        //pav - Provenance and Versioning.
        public static Uri pav_createdBy = new Uri(ns_pav.ToString() + "createdBy");
        public static Uri pav_createdOn = new Uri(ns_pav.ToString() + "createdOn");
        public static Uri pav_createdWith = new Uri(ns_pav.ToString() + "createdWith");
        public static Uri pav_lastUpdateOn = new Uri(ns_pav.ToString() + "lastUpdateOn");
        public static Uri pav_importedOn = new Uri(ns_pav.ToString() + "importedOn");
        public static Uri pav_importedFrom = new Uri(ns_pav.ToString() + "importedFrom");
        public static Uri pav_importedBy = new Uri(ns_pav.ToString() + "importedBy");
        public static Uri pav_version = new Uri(ns_pav.ToString() + "version");
        public static Uri pav_authoredBy = new Uri(ns_pav.ToString() + "authoredBy");
        public static Uri pav_authoredOn = new Uri(ns_pav.ToString() + "authoredOn");
        public static Uri pav_previousVersion = new Uri(ns_pav.ToString() + "previousVersion");
        public static Uri pav_retrievedBy = new Uri(ns_pav.ToString() + "retrievedBy");
        public static Uri pav_retrievedFrom = new Uri(ns_pav.ToString() + "retrievedFrom");
        public static Uri pav_retrievedOn = new Uri(ns_pav.ToString() + "retrievedOn");

        //prov - Provenance.
        public static Uri prov_wasDerivedFrom = new Uri(ns_prov.ToString() + "wasDerivedFrom");

        //foaf - Friend of a Friend.
        public static Uri foaf_primaryTopic = new Uri(ns_foaf.ToString() + "primaryTopic");
        public static Uri foaf_homepage = new Uri(ns_foaf.ToString() + "homepage");
        public static Uri foaf_page = new Uri(ns_foaf.ToString() + "page");

        //void - Vocabulary of Interlinked Datasets.
        public static Uri void_DataSet = new Uri(ns_void.ToString() + "Dataset");
        public static Uri void_uriSpace = new Uri(ns_void.ToString() + "uriSpace");
        public static Uri void_subset = new Uri(ns_void.ToString() + "subset");
        public static Uri void_vocabulary = new Uri(ns_void.ToString() + "vocabulary");
        public static Uri void_exampleResource = new Uri(ns_void.ToString() + "example_resource");
        public static Uri void_feature = new Uri(ns_void.ToString() + "feature");
        public static Uri void_dataDump = new Uri(ns_void.ToString() + "dataDump");
        public static Uri void_Linkset = new Uri(ns_void.ToString() + "Linkset");
        public static Uri void_subjectsTarget = new Uri(ns_void.ToString() + "subjectsTarget");
        public static Uri void_objectsTarget = new Uri(ns_void.ToString() + "objectsTarget");
        public static Uri void_linkPredicate = new Uri(ns_void.ToString() + "linkPredicate");
        public static Uri void_triples = new Uri(ns_void.ToString() + "triples");

        //xsd - Data types.
        public static Uri xsd_string = new Uri(ns_xsd.ToString() + "string");
        public static Uri xsd_dateTime = new Uri(ns_xsd.ToString() + "dateTime");
        public static Uri xsd_integer = new Uri(ns_xsd.ToString() + "integer");

        //voag - Vocabulary Of Attribution and Governance.
        public static Uri voag_frequencyOfChange = new Uri(ns_voag.ToString() + "frequencyOfChange");

        //freq - Frequency
        public static Uri freq_monthly = new Uri(ns_freq.ToString() + "monthly");

        //skos - Simple Knowledge Organisation System.
        public static Uri skos_relatedMatch = new Uri(ns_skos.ToString() + "relatedMatch");
        public static Uri skos_exactMatch = new Uri(ns_skos.ToString() + "exactMatch");

        //dul - DOLCE+DnS Ultralite.
        public static Uri dul_expresses = new Uri(ns_dul.ToString() + "expresses");

        //Some ChemInf Uris used for expressing relationships.
        public static Uri dul_expresses_inchi = new Uri(ns_cheminf.ToString() + "CHEMINF_000059");
        public static Uri dul_expresses_ligates = new Uri(ns_cheminf.ToString() + "CHEMINF_000454");

        //Some ChemInf strings used for expressing predicates used in the Synonyms export file.
        public static string cheminf_validated_synonym = "CHEMINF_000465";      //Validated Synonym
        public static string cheminf_unvalidated_synonym = "CHEMINF_000466";    //Unvalidated Synonym
        public static string cheminf_validated_dbid = "CHEMINF_000467";         //Validated Database Identifier
        public static string cheminf_unvalidated_dbid = "CHEMINF_000464";       //Unvalidated Database Identifier
        public static string cheminf_std_inchi_104 = "CHEMINF_000396";          //Std InChI 1.04
        public static string cheminf_std_inchikey_104 = "CHEMINF_000399";       //Std InChIKey 1.04
        public static string cheminf_csid = "CHEMINF_000405";                   //ChemSpider ID
        public static string cheminf_title = "CHEMINF_000476";                  //ChemSpider title
        public static string cheminf_smiles = "CHEMINF_000018";                 //SMILES
        public static string cheminf_mf = "CHEMINF_000490";                     //Molecular Formula

        //Some ChemInf strings used for expressing the predicates used in the Issues export file.
        public static string cheminf_issue_parsing_info = "CHEMINF_000558";             //Parsing - Information
        public static string cheminf_issue_parsing_warning = "CHEMINF_000555";          //Parsing - Warning
        public static string cheminf_issue_parsing_error = "CHEMINF_000556";            //Parsing - Error
        public static string cheminf_issue_validation_info = "CHEMINF_000560";          //Validation - Information
        public static string cheminf_issue_validation_warning = "CHEMINF_000425";       //Validation - Warning
        public static string cheminf_issue_validation_error = "CHEMINF_000426";         //Validation - Error
        public static string cheminf_issue_standardization_info = "CHEMINF_000559";     //Standardization - Information
        public static string cheminf_issue_standardization_warning = "CHEMINF_000554";  //Standardization - Warning
        public static string cheminf_issue_standardization_error = "CHEMINF_000553";    //Standardization - Error

        /// <summary>
        /// Returns a Uri for the Skos Predicate.
        /// </summary>
        /// <param name="predicate">The predicate enum</param>
        /// <returns>A new Uri representing the Skos relation.</returns>
        public static Uri SkosPredicateToUri(SkosPredicate predicate)
        {
            switch (predicate)
            {
                case SkosPredicate.EXACT_MATCH:
                    return new Uri(ns_skos.ToString() + SKOS_EXACT_MATCH);
                case SkosPredicate.CLOSE_MATCH:
                    return new Uri(ns_skos.ToString() + SKOS_CLOSE_MATCH);
                case SkosPredicate.RELATED_MATCH:
                    return new Uri(ns_skos.ToString() + SKOS_RELATED_MATCH);
                default: //Default to exact match.
                    return new Uri(ns_skos.ToString() + SKOS_EXACT_MATCH);
            }
        }

        /// <summary>
        /// Returns a string representing the Skos Predicate.
        /// </summary>
        /// <param name="predicate">The predicate enum</param>
        /// <returns>The predicate string</returns>
        public static string SkosPredicateToString(SkosPredicate predicate)
        {
            switch (predicate)
            {
                case SkosPredicate.EXACT_MATCH:
                    return SKOS_EXACT_MATCH;
                case SkosPredicate.CLOSE_MATCH:
                    return SKOS_CLOSE_MATCH;
                case SkosPredicate.RELATED_MATCH:
                    return SKOS_RELATED_MATCH;
                default: //Default to exact match.
                    return SKOS_EXACT_MATCH;
            }
        }

        /// <summary>
        /// Returns a description representing the Skos Predicate.
        /// </summary>
        /// <param name="predicate">The predicate enum</param>
        /// <returns>The predicate string</returns>
        public static string SkosPredicateToDescription(SkosPredicate predicate)
        {
            switch (predicate)
            {
                case SkosPredicate.EXACT_MATCH:
                    return "match exactly with";
                case SkosPredicate.CLOSE_MATCH:
                    return "match closely with";
                case SkosPredicate.RELATED_MATCH:
                    return "are related to";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Returns a string representing the Skos Predicate.
        /// </summary>
        /// <param name="predicate">The predicate enum</param>
        /// <returns>The predicate string</returns>
        public static SkosPredicate SkosStringToPredicate(string predicate)
        {
            switch (predicate)
            {
                case SKOS_EXACT_MATCH:
                    return SkosPredicate.EXACT_MATCH;
                case SKOS_CLOSE_MATCH:
                    return SkosPredicate.CLOSE_MATCH;
                case SKOS_RELATED_MATCH:
                    return SkosPredicate.RELATED_MATCH;
                default: //Default to exact match.
                    return SkosPredicate.EXACT_MATCH;
            }
        }

        /// <summary>
        /// Asserts a Uri Triple object for a given Uri - if that Uri does not exist in the Graph then add it.
        /// </summary>
        /// <param name="g">The graph.</param>
        /// <param name="subject">The subject Uri.</param>
        /// <param name="predicate">The predicate Uri.</param>
        /// <param name="literalObject">The uri object.</param>
        public static void AssertTriple(Graph g, Uri subject, Uri pred, Uri obj)
        {
            IUriNode subject_node = g.CreateUriNode(subject);
            IUriNode predicate_node = g.CreateUriNode(pred);
            IUriNode object_node = g.CreateUriNode(obj);
            g.Assert(new Triple(subject_node, predicate_node, object_node));
        }

        /// <summary>
        /// Asserts a Literal Triple object for a given Uri - if that Uri does not exist in the Graph then add it.
        /// </summary>
        /// <param name="g">The graph.</param>
        /// <param name="subject">The subject Uri.</param>
        /// <param name="predicate">The predicate Uri.</param>
        /// <param name="literalObject">The uri object.</param>
        public static void AssertTriple(Graph g, Uri subject, Uri pred, string obj, Uri type = null)
        {
            IUriNode subject_node = g.CreateUriNode(subject);
            IUriNode predicate_node = g.CreateUriNode(pred);
            ILiteralNode object_node;

            //Either set the data type of the string or set the language.
            if (type == null)
                object_node = g.CreateLiteralNode(obj, "en");
            else
                object_node = g.CreateLiteralNode(obj, type);

            g.Assert(new Triple(subject_node, predicate_node, object_node));
        }

        /// <summary>
        /// Removes line break characters and escapes backslashes and double quotes.
        /// </summary>
        public static string RdfEncodedString(string s)
        {
            return !String.IsNullOrEmpty(s) ? s.Replace("\n", "").Replace("\r", "").Replace(@"\", @"\\").Replace("\"", "\\\"")
                : String.Empty;
        }

        /// <summary>
        /// Removes underscores and parentheses from the molecular formula.
        /// </summary>
        public static string FormatMolecularFormula(string s)
        {
            return !String.IsNullOrEmpty(s) ? s.Replace("_", "").Replace("{", "").Replace("}", "")
                : String.Empty;
        }

        //The list of prefixes required for parent child.
        public static Dictionary<string, string> parent_child_prefixes = new Dictionary<string, string>()
                    {
                        { "cheminf", ns_cheminf.ToString() },
                        { "obo2", ns_obo2.ToString() }
                    };

        //The list of prefixes required for the linkset.
        public static Dictionary<string, string> linkset_prefixes = new Dictionary<string, string>()
                    {
                        { "rdf", ns_rdf.ToString() },
                        { "rdfs", ns_rdfs.ToString() }, 
                        { "xsd", ns_xsd.ToString() },
                        { "void", ns_void.ToString() },
                        { SKOS_PREFIX, ns_skos.ToString() },
                    };

        //The prefixes required for the properties.
        public static Dictionary<string, string> props_prefixes = new Dictionary<string, string>()
                    {
                        { "rdf", ns_rdf.ToString() }, 
                        { "rdfs", ns_rdfs.ToString() }, 
                        { "xsd", ns_xsd.ToString() }, 
                        { "cheminf", ns_cheminf.ToString() },
                        { "obo", ns_obo.ToString() }, 
                        { "ro", ns_obo_ro.ToString() }, 
                        { "csid", RDF_URI }, 
                        { "qudt", ns_qudt.ToString() }, 
                        { "void", ns_void.ToString() }
                    };

        //The prefixes required for the issues.
        public static Dictionary<string, string> issues_prefixes = new Dictionary<string, string>()
                    {
                        { "rdf", ns_rdf.ToString() }, 
                        { "rdfs", ns_rdfs.ToString() }, 
                        { "xsd", ns_xsd.ToString() }, 
                        { "cheminf", ns_cheminf.ToString() },
                        { "owl", ns_owl.ToString() },
                        { "void", ns_void.ToString() }
                    };

        //Add the comments at the end of the prefixes.
        public static List<string> props_comments = new List<string>()
                    {
                        "#",
                        "# obo:IAO_0000136 means is_about",
                        "# cheminf:CHEMINF_000055 is a connection table,",
                        "# cheminf:CHEMINF_000354 is an execution of the ACD/Labs software",
                        "# obo:OBO_0000293 and obo:OBO_0000299 specify input and output",
                        "#"
                    };

        //Maps properties to their predicates in ChemInf
        public static Dictionary<string, string> CHEMINFmapping = new Dictionary<string, string>()
        {
            { "LogP", "CHEMINF_000321"},
            { "RuleOf5", "CHEMINF_000367"},
            { "LogD_1", "CHEMINF_000344"}, // pH 5.5
            { "LogD_2", "CHEMINF_000323"}, // pH 7.4
            { "RuleOf5_FRB", "CHEMINF_000348"},
            { "RuleOf5_HAcceptors", "CHEMINF_000345"},
            { "RuleOf5_HDonors", "CHEMINF_000346"},
            { "BCF_1", "CHEMINF_000365"}, //pH 5.5
            { "BCF_2", "CHEMINF_000366"}, //pH 7.4
            { "KOC_1", "CHEMINF_000363"}, //pH 5.5
            { "KOC_2", "CHEMINF_000364"}, //pH 7.4
            { "RuleOf5_PSA", "CHEMINF_000349"},
            { "Index_Of_Refraction", "CHEMINF_000352"},
            { "Molar_Refractivity", "CHEMINF_000351"},
            { "Molar_Volume", "CHEMINF_000358"},
            { "Polarizability", "CHEMINF_000353"},
            { "Surface_Tension", "CHEMINF_000368"},
            { "Density", "CHEMINF_000359"},
            { "FP", "CHEMINF_000360"}, // flash point
            { "Enthalpy", "CHEMINF_000361"}, // enthalpy of vaporization
            { "VP", "CHEMINF_000362"}, // vapour pressure
            { "BP", "CHEMINF_000347"}, // boiling point
            { "Average_Mass", "CHEMINF_000484"}, // average mass
            { "Monoisotopic_Mass", "CHEMINF_000485"} // monoisotopic mass
        };

        //Maps properties to their units.
        public static Dictionary<string, string> unitMapping = new Dictionary<string, string>()
        {
            { "LogP", "qudt:DimensionlessUnit"},
            { "RuleOf5", "qudt:DimensionlessUnit"},
            { "LogD_1", "qudt:DimensionlessUnit"}, // pH 5.5
            { "LogD_2", "qudt:DimensionlessUnit"}, // pH 7.4
            { "RuleOf5_FRB", "qudt:DimensionlessUnit"},
            { "RuleOf5_HAcceptors", "qudt:DimensionlessUnit"},
            { "RuleOf5_HDonors", "qudt:DimensionlessUnit"},
            { "BCF_1", "qudt:DimensionlessUnit"}, //pH 5.5
            { "BCF_2", "qudt:DimensionlessUnit"}, //pH 7.4
            { "KOC_1", "qudt:DimensionlessUnit"}, //pH 5.5
            { "KOC_2", "qudt:DimensionlessUnit"}, //pH 7.4
           
            { "Index_Of_Refraction", "qudt:DimensionlessUnit"},
            { "Molar_Refractivity", "qudt:CubicMeter"}, // and multiply by 10^{-6}
            { "Molar_Volume", "qudt:CubicMeter"}, // and multiply by 10^{-6}
            { "Polarizability", "qudt:CubicMeter"}, // quoted in 10^{-24} cm^3 which is 10^{-30} m^3
            { "Surface_Tension", "qudt:NewtonPerMeter"}, // and multiply by whatever
            { "Density", "qudt:KilogramPerCubicMeter"}, // and scale
            { "FP", "qudt:DegreeCelsius"}, // flash point
            { "Enthalpy", "qudt:JoulePerMole"}, // enthalpy of vaporization, and scale
            { "VP", "qudt:MillimeterOfMercury"}, // vapour pressure
            { "BP", "qudt:DegreeCelsius"}, // boiling point
            { "Average_Mass", "qudt:Dalton"}, // average mass in Daltons
            { "Monoisotopic_Mass", "qudt:Dalton"}, // monoisotopic mass in Daltons
            { "RuleOf5_PSA", "obo:UO_0000324"} // polar surface area in square Angstroms
        };

        //The properties with errors.
        public static List<string> columnsWithQuotedErrors = new List<string>()
        {
            "Density", "Index_Of_Refraction", "Molar_Refractivity", "Molar_Volume", "Polarizability",
            "Surface_Tension", "FP", "Enthalpy", "BP", "VP", "LogP",
        };

        //The properties with scaling.
        public static Dictionary<string, double> scaling = new Dictionary<string, double>()
        {
            { "Molar_Refractivity", 0.000001 },
            { "Molar_Volume", 0.000001},
            { "Polarizability", 10E-30},
            { "Surface_Tension", 0.001 },
            { "Density", 0.001 }, // going from kg m-3 to g cm-3
            { "Enthalpy", 1000 } // going from kJ mol-1 to J mol-1
        };

        //The prefixes required for the synonyms.
        public static Dictionary<string, string> synonyms_prefixes = new Dictionary<string, string>()
        {
            { "rdf", ns_rdf.ToString() }, 
            { "void", ns_void.ToString() },
            { "owl", ns_owl.ToString() },
            { "cheminf", ns_cheminf.ToString() }
        };

        /// <summary>
        /// Returns a list of triples representing a single calculated value (overloaded without the error) for a compound.
        /// </summary>
        public static List<string> TurtleValue(string csid, string propnode, string column, string value, UnitStyle us)
        {
            return TurtleValue(csid, propnode, column, value, false, "", us);
        }

        /// <summary>
        /// Returns a list of triples representing a single calculated value and its error for a compound.
        /// </summary>
        /// <param name="csid">The chemspider id</param>
        /// <param name="propnode">The property node</param>
        /// <param name="column">The column name</param>
        /// <param name="value">The property value</param>
        /// <param name="errorQuoted">Boolean indicating whether there is an error</param>
        /// <param name="error">Value of the error</param>
        /// <param name="us">The unit style</param>
        /// <returns>List of triples representing the property value</returns>
        public static List<string> TurtleValue(string csid, string propnode, string column, string value, bool errorQuoted, string error, UnitStyle us)
        {
            List<string> result = new List<string>();
            // execution has specified output the particular property
            result.Add(String.Format(":CSID{0}execution obo:OBO_0000299 {1} . ", csid, propnode));
            result.Add(String.Format("{0} rdfs:label \"Compound {1} property {2} in {3}\"@en.", propnode, csid, column, unitMapping[column]));
            result.Add(String.Format("{0} obo:IAO_0000136 {1}:{2}{3} .", propnode, Turtle.RDF_URI_PREFIX.ToLower(), Turtle.RDF_URI_PREFIX, csid));
            result.Add(String.Format("{0} rdf:type cheminf:{1} .", propnode, CHEMINFmapping[column]));
            switch (us)
            {
                case UnitStyle.QUDT:
                    double scaledValue = scaling.ContainsKey(column) ? Convert.ToDouble(value) * scaling[column] : Convert.ToDouble(value);
                    result.Add(String.Format("{0} qudt:numericValue \"{1}\"^^xsd:double .", propnode, scaledValue));
                    result.Add(String.Format("{0} qudt:unit {1} .", propnode, unitMapping[column]));
                    if (errorQuoted)
                    {
                        double scaledError = scaling.ContainsKey(column) ? Convert.ToDouble(error) * scaling[column] : Convert.ToDouble(error);
                        result.Add(String.Format("{0} qudt:standardUncertainty \"{1}\"^^xsd:double .", propnode, error));
                    }
                    return result;
                default: throw new Exception("unsupported unit style");
            }
        }

        /// <summary>
        /// Convention is this: if nodeStart is :CSID0,
        /// :CSID0ct is the connection table
        /// :CSID0ct.execution is the execution of ACD/Labs
        /// :CSID0prop is the property
        /// </summary>
        /// <param name="csid">The chemspider id</param>
        /// <returns>List of triples for the compound</returns>
        public static List<string> TurtleProps(string csid)
        {
            List<string> result = new List<string>();
            result.Add(String.Format(":CSID{0}ct rdf:type cheminf:CHEMINF_000055 .", csid));
            // the connection table is_about the molecule
            result.Add(String.Format(":CSID{0}ct obo:IAO_0000136 {1}:{2}{0} .", csid, Turtle.RDF_URI_PREFIX.ToLower(), Turtle.RDF_URI_PREFIX));
            result.Add(String.Format(":CSID{0}execution rdf:type cheminf:CHEMINF_000354 .", csid));
            // execution has specified input the connection table
            result.Add(String.Format(":CSID{0}execution obo:OBO_0000293 :CSID{0}ct .", csid));
            return result;
        }

        /// <summary>
        /// Gets a list of numerical properties using qudt.
        /// </summary>
        /// <param name="csid">The chemspider id</param>
        /// <param name="values">Dictionary of the properties</param>
        /// <returns>List of numerical peropties</returns>
        public static List<string> AllTurtleProps(string csid, Dictionary<string, string> values)
        {
            List<string> result = new List<string>();
            result.AddRange(TurtleProps(csid));
            int c = 0;
            foreach (var pair in values.Where(p => !p.Key.EndsWith("_Error")).Where(p => !String.IsNullOrEmpty(p.Value)))
            {
                if (columnsWithQuotedErrors.Contains(pair.Key) && !String.IsNullOrEmpty(values[pair.Key + "_Error"]))
                {
                    result.AddRange(TurtleValue(csid, String.Format(":CSID{0}prop{1}", csid, c), pair.Key, pair.Value, true, values[pair.Key + "_Error"], UnitStyle.QUDT));
                }
                else
                {
                    result.AddRange(TurtleValue(csid, String.Format(":CSID{0}prop{1}", csid, c), pair.Key, pair.Value, UnitStyle.QUDT));
                }
                c++;
            }
            return result;
        }

        /// <summary>
        /// Converts a SQLDataReader to a Dictionary.
        /// </summary>
        /// <param name="r">SqlDataReader object</param>
        /// <param name="keys">List of keys</param>
        /// <returns>The converted Dictionary object.</returns>
        public static Dictionary<string, string> SqlDataReaderAsDictionary(SqlDataReader r, List<string> keys)
        {
            return (from k in keys select new KeyValuePair<string, string>(k, r[k].ToString())).ToDictionary(p => p.Key, p => p.Value);
        }

        /// <summary>
        /// Get the data source name using the correct case sensitivity (seems important to get it correct).
        /// </summary>
        /// <param name="dsn">The data source name</param>
        /// <returns>The data source name using correct case sensitivity.</returns>
        public static string getDSN(IDataExport exp, string dsn)
        {
            using (SqlConnection conn = new SqlConnection(exp.UsersDBConnection))
            {
                string dsn_case = conn.ExecuteScalar<string>("SELECT dsn_name FROM datasources WHERE dsn_name = @name_in", new { name_in = dsn });
                return dsn_case;
            }
        }

        /// <summary>
        /// Get the data source is using the data source name.
        /// </summary>
        /// <param name="Exp">The Data Export</param>
        /// <param name="dsn">The data source name</param>
        /// <returns>The data source id</returns>
        public static int getDSNId(IDataExport exp, string dsn)
        {
            using (SqlConnection conn = new SqlConnection(exp.UsersDBConnection))
            {
                int dsn_id = conn.ExecuteScalar<int>("SELECT dsn_id FROM datasources WHERE dsn_name = @name", new { name = dsn });
                return dsn_id;
            }
        }

        /// <summary>
        /// Translation of database id to the void header for the linkset.
        /// </summary>
        /// <param name="dsn">The data source</param>
        /// <param name="dataset_object">The rdf alias for the dataset</param>
        /// <param name="uri_file">The base filename</param>
        /// <returns></returns>
        public static string getDSNInDatasetPredicate(string dsn, string dataset_object, string uri_file)
        {
            //Add the predicate to describe which subset this dataset is in.
            DateTime now = DateTime.Now;
            string void_filename = string.Format("void_{0}.ttl", now.ToString("yyyy-MM-dd")); //E.g. void_2012-10-29.ttl
            return String.Format("<{0}> void:inDataset <{1}/{2}/{3}#{4}> .", uri_file, Turtle.FTP_PREFIX, now.ToString("yyyyMMdd"), void_filename, dataset_object);
        }

        /// <summary>
        /// Maps issue severity number to a textual description.
        /// </summary>
        /// <param name="issue_severity">The issue severity number.</param>
        /// <returns>The testual description of the issue severity.</returns>
        public static string getIssueSeverityDescription(int issue_severity)
        {
            switch (issue_severity)
            {
                case 0:
                    return "Information";
                case 1:
                    return "Warning";
                case 2:
                    return "Error";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Maps issue type number to a textual description.
        /// </summary>
        /// <param name="issue_type">The issue type number.</param>
        /// <returns>The textual description of the issue type.</returns>
        public static string getIssueTypeDescription(int issue_type)
        {
            switch (issue_type)
            {
                case 1:
                    return "Parsing";
                case 2:
                    return "Validation";
                case 3:
                    return "Standardization";
                default:
                    return string.Empty;
            }
        }
        
        /// <summary>
        /// Translation of the issue severity and the issue type to the Issue predicate.
        /// </summary>
        /// <param name="issue_severity">An integer representing the severity of the issue.</param>
        /// <param name="issue_type">An integer representing the type of the issue.</param>
        /// <returns>A string representing the Issue Predicate.</returns>
        public static string getIssuePredicate(int issue_severity, int issue_type)
        {
            //Uses the cheminf ontology.
            string predicate = string.Empty;
            switch (issue_type)
            {
                case 1:
                    //Parsing.
                    switch (issue_severity)
                    {
                        case 0:
                            predicate = Turtle.cheminf_issue_parsing_info;
                            break;
                        case 1:
                            predicate = Turtle.cheminf_issue_parsing_warning;
                            break;
                        case 2:
                            predicate = Turtle.cheminf_issue_parsing_error;
                            break;
                        default:
                            break;
                    }
                    break;
                case 2:
                    //Validation.
                    switch (issue_type)
                    {
                        case 0:
                            predicate = Turtle.cheminf_issue_validation_info;
                            break;
                        case 1:
                            predicate = Turtle.cheminf_issue_validation_warning;
                            break;
                        case 2:
                            predicate = Turtle.cheminf_issue_validation_error;
                            break;
                        default:
                            break;
                    }
                    break;
                case 3:
                    //Standardization.
                    switch (issue_type)
                    {
                        case 0:
                            predicate = Turtle.cheminf_issue_standardization_info;
                            break;
                        case 1:
                            predicate = Turtle.cheminf_issue_standardization_warning;
                            break;
                        case 2:
                            predicate = Turtle.cheminf_issue_standardization_error;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            
            //Append the cheminf ontology prefix.
            if (predicate != string.Empty)
            {
                predicate = String.Format("{0}:{1}", "cheminf", predicate);
            }
            return predicate;
        }

        #region hard-coded-dsn-stuff

        //*************************************************************************************************************************
        //Hard-coded stuff below. This should eventually be moved to the database and be editable from the front-end somehow.
        //*************************************************************************************************************************

        ////Hard-coded list of database uris used in the prefix, outputs the alias and the uri it is an alias for.
        //public static string getDSNPrefix(string dsn, out string alias, out bool use_full_uri)
        //{
        //    string uri = string.Empty;
        //    switch (dsn.ToLower())
        //    {
        //        case "chebi":
        //            alias = dsn.ToLower();
        //            uri = "http://purl.obolibrary.org/obo/";
        //            use_full_uri = false;
        //            break;
        //        case "chembl":
        //            alias = dsn.ToLower();
        //            uri = "http://data.kasabi.com/dataset/chembl-rdf/chemblid/";
        //            use_full_uri = false;
        //            break;
        //        case "drugbank":
        //            alias = dsn.ToLower();
        //            uri = "http://www4.wiwiss.fu-berlin.de/drugbank/resource/drugs/";
        //            use_full_uri = true;
        //            break;
        //        case "pdb":
        //            alias = dsn.ToLower();
        //            uri = "http://purl.uniprot.org/pdb/";
        //            use_full_uri = true;
        //            break;
        //        case "mesh":
        //            alias = dsn.ToLower();
        //            uri = "http://purl.bioontology.org/ontology/MSH/";
        //            use_full_uri = false;
        //            break;
        //        default:
        //            //Unsupported dsn.
        //            alias = string.Empty;
        //            uri = string.Empty;
        //            use_full_uri = false;
        //            break;
        //    }
        //    return uri;
        //}

        //Hard-coded translation of database ids into the Uri for that database.
        public static string getDSNUri(string dsn, string id, string dsn_alias, string dsn_prefix, bool use_full_uri)
        {
            string dsn_uri = string.Empty;
            string identifier = string.Empty;

            switch (dsn.ToLower())
            {
                case "chebi":
                    identifier = id.Replace(":", "_"); //We store CHEBI:12345 but need to change to CHEBI_12345 so perform replacement.
                    break;
                default:
                    //Just return the id if not in the list.
                    identifier = id;
                    break;
            }

            //If the identifier starts with a number then we cannot use the space-saving alias:identifer syntax, we must use the full uri.
            if (use_full_uri)
                return String.Format("<{0}{1}>", dsn_prefix, identifier);
            else
                return String.Format("{0}:{1}", dsn_alias, identifier);
        }

        //Hard-coded translation of database ids into whether they should map to skos:relatedMatch (as well as exactMatch).
        public static bool useSkosRelatedMatchForDSN(string dsn)
        {
            switch (dsn.ToLower())
            {
                case "pdb":
                    return true; //We use skos:relatedMatch for ligands which have 4 characters in the id rather than 3 for structures.
                default:
                    //Return false if not in the list.
                    return false;
            }
        }

        //Hard-coded translation of dsn and database id into whether they should map to skos:relatedMatch.
        public static bool useSkosRelatedMatchForId(string dsn, string id)
        {
            switch (dsn.ToLower())
            {
                case "pdb":
                    //We use skos:relatedMatch for ligands which have 4 characters in the id rather than 3 for structures.
                    return id.Length == 4;
                default:
                    //Return false if not in the list.
                    return false;
            }
        }

        //Hard-coded translation of dsn and database id into whether they should map to skos:relatedMatch.
        public static bool useSkosExactMatchForId(string dsn, string id)
        {
            switch (dsn.ToLower())
            {
                case "pdb":
                    //We use skos:exactMatch for structures which have 3 characters in the id.
                    return id.Length == 3;
                default:
                    //Return true if not in the list.
                    return true;
            }
        }

        #endregion
    }
}
