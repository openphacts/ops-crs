using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using RSC.Logging;
using VDS.RDF;
using RSC.Properties;

namespace RSC.Compounds.DataExport
{
    public enum SkosPredicate { RELATED_MATCH, EXACT_MATCH, CLOSE_MATCH }

    public enum UnitStyle { QUDT, OBI }

    public class Turtle
    {
        //RDF Base prefix.
        public static Uri RDF_URI = new Uri(ConfigurationManager.AppSettings["rdf.uri"].ToString());  //E.g. http://ops.rsc-us.org/

        //ChemSpider RDF Base prefix.
        public static string CHEMSPIDER_RDF_URI = ConfigurationManager.AppSettings["rdf.uri.chemspider"].ToString();  //E.g. http://rdf.chemspider.com/

        //Identifier prefix.
        public static string RDF_URI_PREFIX = ConfigurationManager.AppSettings["rdf.uri_prefix"].ToString();  //E.g. /OPS

        //Dataset Label used for VOID and Linksets.
        public static string DATASET_LABEL = ConfigurationManager.AppSettings["void_dataset_label"].ToString();   //E.g. "ChemSpider" or "OpenPhacts"

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
        public static Uri void_exampleResource = new Uri(ns_void.ToString() + "exampleResource");
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

        //obo - OBI predicates
        public static string obo_isabout = "obo:IAO_0000136";
        public static string obo_hasspecifiedinput = "obo:OBI_0000293";
        public static string obo_hasspecifiedoutput = "obo:OBI_0000299";

        //Some ChemInf Uris used for expressing relationships.
        public static Uri dul_expressesInchi = new Uri(ns_cheminf.ToString() + "CHEMINF_000059");
        public static Uri dul_expressesLigates = new Uri(ns_cheminf.ToString() + "CHEMINF_000454");

        //Some ChemInf strings used for expressing predicates used in the Synonyms export file.
        public static string cheminf_validatedSynonym = "CHEMINF_000465";       //Validated Synonym
        public static string cheminf_unvalidatedSynonym = "CHEMINF_000466";     //Unvalidated Synonym
        public static string cheminf_validatedDbid = "CHEMINF_000467";          //Validated Database Identifier
        public static string cheminf_unvalidatedDbid = "CHEMINF_000464";        //Unvalidated Database Identifier
        public static string cheminf_stdInchi104 = "CHEMINF_000396";            //Std InChI 1.04
        public static string cheminf_stdInchiKey104 = "CHEMINF_000399";         //Std InChIKey 1.04
        public static string cheminf_csid = "CHEMINF_000405";                   //ChemSpider ID
        public static string cheminf_title = "CHEMINF_000476";                  //ChemSpider title
        public static string cheminf_smiles = "CHEMINF_000018";                 //SMILES
        public static string cheminf_mf = "CHEMINF_000490";                     //Molecular Formula

        //Some ChemInf strings used for expressing the predicates used in the Issues export file.
        public static string cheminf_issueParsingInfo = "CHEMINF_000558";             //Parsing - Information
        public static string cheminf_issueParsingWarning = "CHEMINF_000555";          //Parsing - Warning
        public static string cheminf_issueParsingError = "CHEMINF_000556";            //Parsing - Error
        public static string cheminf_issueValidationInfo = "CHEMINF_000560";          //Validation - Information
        public static string cheminf_issueValidationWarning = "CHEMINF_000425";       //Validation - Warning
        public static string cheminf_issueValidationError = "CHEMINF_000426";         //Validation - Error
        public static string cheminf_issueStandardizationInfo = "CHEMINF_000559";     //Standardization - Information
        public static string cheminf_issueStandardizationWarning = "CHEMINF_000554";  //Standardization - Warning
        public static string cheminf_issueStandardizationError = "CHEMINF_000553";    //Standardization - Error
        public static string cheminf_issueProcessingInfo = "CHEMINF_000557";          //Processing - Information
        public static string cheminf_issueProcessingWarning = "CHEMINF_000506";       //Processing - Warning
        public static string cheminf_issueProcessingError = "CHEMINF_000507";         //Processing - Error
        public static string cheminf_issueDefault = "CHEMINF_000505";                 //Default message.

        //Rdf predicates.
        public static Uri FragmentRdfPredicate = new Uri(ns_obo2.ToString() + "has_part");
        public static Uri ChargeInsensitiveRdfPredicate = new Uri(ns_cheminf.ToString() + "CHEMINF_000460");
        public static Uri IsotopeInsensitiveRdfPredicate = new Uri(ns_cheminf.ToString() + "CHEMINF_000459");
        public static Uri StereoInsensitiveRdfPredicate = new Uri(ns_cheminf.ToString() + "CHEMINF_000456");
        public static Uri SuperInsensitiveRdfPredicate = new Uri(ns_cheminf.ToString() + "CHEMINF_000458");
        public static Uri TautomerInsensitiveRdfPredicate = new Uri(ns_obo2.ToString() + "is_tautomer_of");

        /// <summary>
        /// Returns a string representing the Skos Predicate.
        /// </summary>
        /// <param name="predicate">The predicate enum</param>
        /// <returns>The predicate string</returns>
        /// 2015-10-26: we don't seem to be doing anything with this
        [Obsolete]
        public static SkosPredicate SkosStringToPredicate(string predicate)
        {
            switch (predicate)
            {
                case SkosPredicateExtensions.SKOS_EXACT_MATCH:
                    return SkosPredicate.EXACT_MATCH;
                case SkosPredicateExtensions.SKOS_CLOSE_MATCH:
                    return SkosPredicate.CLOSE_MATCH;
                case SkosPredicateExtensions.SKOS_RELATED_MATCH:
                    return SkosPredicate.RELATED_MATCH;
                default: //Default to exact match.
                    return SkosPredicate.EXACT_MATCH;
            }
        }

        public static List<ChemInfPropertyMapping> CheminfPropertyMappings = new List<ChemInfPropertyMapping>()
        {
            new ChemInfPropertyMapping(PropertyName.LOG_P, "CHEMINF_000321"),
            new ChemInfPropertyMapping(PropertyName.RULE_OF_5_VIOLATIONS, "CHEMINF_000367"),
            new ChemInfPropertyMapping(PropertyName.LOG_D, "CHEMINF_000344", 5.5),
            new ChemInfPropertyMapping(PropertyName.LOG_D, "CHEMINF_000323", 7.4),
            new ChemInfPropertyMapping(PropertyName.FREELY_ROTATING_BONDS, "CHEMINF_000348"),
            new ChemInfPropertyMapping(PropertyName.H_BOND_ACCEPTORS, "CHEMINF_000345"),
            new ChemInfPropertyMapping(PropertyName.H_BOND_DONORS, "CHEMINF_000346"),
            new ChemInfPropertyMapping(PropertyName.BCF, "CHEMINF_000365", 5.5),
            new ChemInfPropertyMapping(PropertyName.BCF, "CHEMINF_000366", 7.4),
            new ChemInfPropertyMapping(PropertyName.KOC, "CHEMINF_000363", 5.5),
            new ChemInfPropertyMapping(PropertyName.KOC, "CHEMINF_000364", 7.4),
            new ChemInfPropertyMapping(PropertyName.POLAR_SURFACE_AREA, "CHEMINF_000349", null, "obo:UO_0000324"),
            new ChemInfPropertyMapping(PropertyName.REFRACTION_INDEX, "CHEMINF_000352"),
            new ChemInfPropertyMapping(PropertyName.MOLAR_REFRACTIVITY, "CHEMINF_000351", null, "qudt:CubicMeter", 0.000001),
            new ChemInfPropertyMapping(PropertyName.MOLAR_VOLUME, "CHEMINF_000358", null, "qudt:CubicMeter", 0.000001),
            new ChemInfPropertyMapping(PropertyName.POLARIZABILITY, "CHEMINF_000353", null, "qudt:CubicMeter", 10E-30),
            new ChemInfPropertyMapping(PropertyName.SURFACE_TENSION, "CHEMINF_000368", null, "qudt:NewtonPerMeter", 0.001),
            new ChemInfPropertyMapping(PropertyName.DENSITY, "CHEMINF_000359", null, "qudt:KilogramPerCubicMeter", 0.001),
            new ChemInfPropertyMapping(PropertyName.FLASH_POINT, "CHEMINF_000360", null, "qudt:DegreeCelsius"),
            new ChemInfPropertyMapping(PropertyName.ENTHALPY_OF_VAPORIZATION, "CHEMINF_000361", null, "qudt:JoulePerMole", 1000),
            new ChemInfPropertyMapping(PropertyName.VAPOUR_PRESSURE, "CHEMINF_000362", null, "qudt:MillimeterOfMercury"),
            new ChemInfPropertyMapping(PropertyName.BOILING_POINT, "CHEMINF_000347", null, "qudt:DegreeCelsius"),
            new ChemInfPropertyMapping(PropertyName.AVERAGE_MASS, "CHEMINF_000484", null, "qudt:Dalton"),
            new ChemInfPropertyMapping(PropertyName.MONOISOTOPIC_MASS, "CHEMINF_000485", null, "qudt:Dalton")
        };

        /// <summary>
        /// Translation of database id to the void header for the linkset.
        /// </summary>
        /// <param name="dataSourceName">The data source</param>
        /// <param name="datasetObject">The rdf alias for the dataset</param>
        /// <param name="fileUri">The base filename</param>
        /// <param name="exportLocation">The location of the data export</param>
        /// <returns>The void header for the linkset.</returns>
        public static string VoidInDatasetLine(string dataSourceName, string datasetObject, string fileUri, string exportLocation, DateTime dateStamp)
        {
            //Add the predicate to describe which subset this dataset is in.
            var voidFilename = string.Format("void_{0}.ttl", dateStamp.ToString("yyyy-MM-dd")); //E.g. void_2012-10-29.ttl
            return string.Format("<{0}> void:inDataset <{1}/{2}/{3}#{4}> .", fileUri, exportLocation, dateStamp.ToString("yyyyMMdd"), voidFilename, datasetObject);
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


        /// <summary>
        /// Hard-coded translation of database ids into the Uri for that data source.
        /// </summary>
        /// <param name="dsn">The data source name.</param>
        /// <param name="substanceId">The substance id.</param>
        /// <param name="dsnAlias">The alias for the dsn.</param>
        /// <param name="dsnPrefix">The prefix for the dsn.</param>
        /// <param name="useFullUri">Whether to use the full uri or an alias.</param>
        /// <returns>The translated Uri.</returns>
        public static string GetDsnUri(string dsn, string substanceId, string dsnAlias, string dsnPrefix, bool useFullUri)
        {
            // previously we only did this for ChEBI but actually colons aren't valid for any data source.
            string identifier = substanceId.Replace(":", "_"); 

            //If the identifiers are already uris (containing "http:" then just use that).
            if (identifier.IndexOf("http:", StringComparison.OrdinalIgnoreCase) >= 0)
                return string.Format("<{0}>", identifier);

            //If the identifier starts with a number then we cannot use the space-saving alias:identifer syntax, we must use the full uri.
            return useFullUri ? string.Format("<{0}{1}>", dsnPrefix, identifier) : string.Format("{0}:{1}", dsnAlias, identifier);
        }

        /// <summary>
        /// Hard-coded translation of database ids into whether they should map to skos:relatedMatch (as well as exactMatch).
        /// </summary>
        /// <param name="dsn">The data source name.</param>
        /// <returns>A boolean indicating whether to use SkosRelated predicate for this data source.</returns>
        public static bool UseSkosRelatedMatchForDsn(string dsn)
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

        /// <summary>
        /// Hard-coded translation of dsn and database id into whether they should map to skos:relatedMatch.
        /// </summary>
        /// <param name="dsn">The data source name.</param>
        /// <param name="substanceId">The substance Id.</param>
        /// <returns>Whether to use SkosRelated predicate for this substanceId and data source.</returns>
        [Obsolete] // 2015-10-26 I think I understand why this is here but we're not using it.
        public static bool UseSkosRelatedMatchForId(string dsn, string substanceId)
        {
            switch (dsn.ToLower())
            {
                case "pdb":
                    //We use skos:relatedMatch for ligands which have 4 characters in the id rather than 3 for structures.
                    return substanceId.Length == 4;
                default:
                    //Return false if not in the list.
                    return false;
            }
        }

        /// <summary>
        /// Hard-coded translation of dsn and database id into whether they should map to skos:exactMatch.
        /// </summary>
        /// <param name="dsn">The data source name.</param>
        /// <param name="substanceId">The substance Id.</param>
        /// <returns>Whether to use SkosExactMatch predicate for this substanceId and data source.</returns>
        public static bool UseSkosExactMatchForId(string dsn, string substanceId)
        {
            switch (dsn.ToLower())
            {
                case "pdb":
                    //We use skos:exactMatch for structures which have 3 characters in the id.
                    return substanceId.Length == 3;
                default:
                    //Return true if not in the list.
                    return true;
            }
        }

        #endregion
    }
}
