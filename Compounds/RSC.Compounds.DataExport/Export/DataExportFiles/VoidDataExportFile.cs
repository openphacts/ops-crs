using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using VDS.RDF;
using VDS.RDF.Writing;
using RSC.Datasources;

namespace RSC.Compounds.DataExport
{
	/// <summary>
	/// Used for generating the export file for a void.ttl void descriptor for a list of data sources that have been included in the export.
	/// The void descriptors are used to describe the provenance and versioning of the data export and the external data sources we link to.
	/// </summary>
	public class VoidDataExportFile : DataSourceListDataExportFile
	{
		/// <summary>
		/// Call the base constructor.
		/// </summary>
		public VoidDataExportFile(IDataExport exp, IDictionary<int, Guid> dataSourceExportIds, IDataSourcesClient dataSourcesClient = null)
			: base(dataSourceExportIds, dataSourcesClient)
		{
			FileName = "void_" + exp.ExportDate.ToString("yyyy-MM-dd") + ".ttl";
		}

		//Configuration settings.
		private readonly string _voidTitle = ConfigurationManager.AppSettings["void_title"].ToString();
		//E.g. "A VoID Description of the ChemSpider Dataset"

		private readonly string _datasetTitle = ConfigurationManager.AppSettings["void_dataset_title"].ToString();
		//E.g. "ChemSpider Dataset"

		private readonly string _datasetDescription = ConfigurationManager.AppSettings["void_dataset_description"].ToString();
		//E.g. "ChemSpider's Public Dataset"

		private readonly string _datasetCreatedOn = ConfigurationManager.AppSettings["void_dataset_created_on"].ToString();
		//E.g. "2012-10-24T10:49:00Z"

		private readonly string _provenanceCreatedOn = ConfigurationManager.AppSettings["void_provenance_created_on"].ToString();
		//E.g. "2007-03-01T00:00:00Z"

		private readonly string _provenanceAuthoredOn =
			ConfigurationManager.AppSettings["void_provenance_authored_on"].ToString(); //E.g. "2012-10-30T12:16:00Z"

		private readonly string _userProfileUrlPrefix =
			ConfigurationManager.AppSettings["void_user_profile_url_prefix"].ToString();
		//E.g. "http://www.chemspider.com/UserProfile.aspx?username={0}"

		private readonly Uri _provenanceAuthoredBy =
			new Uri(ConfigurationManager.AppSettings["void_provenance_authored_by"].ToString());
		//E.g. http://www.chemspider.com/UserProfile.aspx?username=jonsteele

		private readonly Uri _homePage = new Uri(ConfigurationManager.AppSettings["void_homepage"].ToString());
		//E.g. http://www.chemspider.com/

		private readonly Uri _license = new Uri(ConfigurationManager.AppSettings["void_license"].ToString());
		//E.g. http://creativecommons.org/licenses/by-sa/3.0/

		private readonly Uri _vocabularySubject = new Uri(ConfigurationManager.AppSettings["void_subject"].ToString());
		//E.g. http://dbpedia.org/resource/Molecule

		private readonly Uri _exampleResource = new Uri(ConfigurationManager.AppSettings["void_example_resource"].ToString());
		//E.g. http://rdf.chemspider.com/2157

		private readonly Uri _chemspiderLicense =
			new Uri(ConfigurationManager.AppSettings["void_chemspider_license"].ToString());
		//E.g. http://creativecommons.org/licenses/by/3.0/

		//Prefixes.
		private readonly Dictionary<string, Uri> _voidPrefixes = new Dictionary<string, Uri>()
		{
			{"dcterms", Turtle.ns_dcterms},
			{"dctype", Turtle.ns_dctype},
			{"foaf", Turtle.ns_foaf},
			{"freq", Turtle.ns_freq},
			{"pav", Turtle.ns_pav},
			{"rdf", Turtle.ns_rdf},
			{"rdfs", Turtle.ns_rdfs},
			{"voag", Turtle.ns_voag},
			{"void", Turtle.ns_void},
			{"xsd", Turtle.ns_xsd},
			{"skos", Turtle.ns_skos},
			{"dul", Turtle.ns_dul},
			{"prov", Turtle.ns_prov},
			{"obo2", Turtle.ns_obo2},
			{"cheminf", Turtle.ns_cheminf},
		};

		//Vocabularies - this is the vocabularies we are referencing in the referenced files, not just those used in the VoID file.
		private readonly List<string> _vocabularies = new List<string>
		{
			Turtle.ns_dcterms.ToString(),
			Turtle.ns_dctype.ToString(),
			Turtle.ns_pav.ToString(),
			Turtle.ns_void.ToString(),
			Turtle.ns_rdf.ToString(),
			Turtle.ns_xsd.ToString(),
			Turtle.ns_skos.ToString(),
			Turtle.ns_dul.ToString(),
			Turtle.ns_foaf.ToString(),
			Turtle.ns_obo.ToString(),
			Turtle.ns_obo_ro.ToString(),
			Turtle.ns_cheminf.ToString(),
			Turtle.ns_qudt.ToString(),
			Turtle.ns_owl.ToString(),
			Turtle.ns_obo2.ToString(),
		};

		public Graph VoidGraph(IDataExport exp)
		{
			var g = new Graph();
			//Get the void uri.
			string voidUri = String.Format("{0}/{1}/{2}", exp.DownloadUrl.TrimEnd('/'), exp.ExportDirectory, FileName);
			Uri nsVoidFile = new Uri(voidUri);
			//Add the prefix for this void file with the empty prefix.
			_voidPrefixes.Add("", new Uri(string.Format("{0}#", nsVoidFile)));
			//Add the namespaces/prefixes we shall refer to.
			foreach ( var prefix in _voidPrefixes ) {
				g.NamespaceMap.AddNamespace(prefix.Key, prefix.Value);
			}
			//DatasetDescription info.
			g.Assert(nsVoidFile, Turtle.has_type,
				new Uri(Turtle.ns_void.ToString() + "DatasetDescription"));
			g.Assert(nsVoidFile, Turtle.dcterms_title, _voidTitle);
			g.Assert(nsVoidFile, Turtle.dcterms_description, _datasetDescription);
			g.Assert(nsVoidFile, Turtle.pav_createdBy, _homePage);
			g.Assert(nsVoidFile, Turtle.pav_createdOn, _datasetCreatedOn, Turtle.xsd_dateTime);
			//Modified/updated dates.
			var datasetLastUpdatedOn = exp.ExportDate.ToString(Turtle.DATE_TIME_FORMAT);
			var provenanceModifiedOn = datasetLastUpdatedOn;
			g.Assert(nsVoidFile, Turtle.pav_lastUpdateOn, datasetLastUpdatedOn, Turtle.xsd_dateTime);
			//Uri to the chemspiderDataset.
			var uriChemspiderDataset = new Uri(nsVoidFile.ToString() + "#" + Turtle.DATASET_LABEL.ToLower() + "Dataset");
			g.Assert(nsVoidFile, Turtle.foaf_primaryTopic, uriChemspiderDataset);
			//Get the date of the last export so we can reference the previous version void file. TODO: Error here!!!
			var uriPreviousVersion = GetPreviousVoidVersion(exp, DataSourceExportIds.Keys.Min());
			if ( uriPreviousVersion != null )
				g.Assert(nsVoidFile, Turtle.pav_previousVersion, uriPreviousVersion);

			//ChemSpider Dataset info.
			g.Assert(uriChemspiderDataset, Turtle.has_type, Turtle.void_DataSet);
			g.Assert(uriChemspiderDataset, Turtle.dcterms_title, _datasetTitle);
			g.Assert(uriChemspiderDataset, Turtle.dcterms_description, _datasetDescription);
			g.Assert(uriChemspiderDataset, Turtle.foaf_homepage, Turtle.RDF_URI);
			g.Assert(uriChemspiderDataset, Turtle.dcterms_license, _license);
			g.Assert(uriChemspiderDataset, Turtle.void_uriSpace, Turtle.RDF_URI.ToString(), Turtle.xsd_string);

			//Provenance.
			g.Assert(uriChemspiderDataset, Turtle.dcterms_publisher, _homePage);
			g.Assert(uriChemspiderDataset, Turtle.dcterms_created, _provenanceCreatedOn,
				Turtle.xsd_dateTime);
			g.Assert(uriChemspiderDataset, Turtle.dcterms_modified, provenanceModifiedOn,
				Turtle.xsd_dateTime);
			//Subsets.
			foreach ( var dataSource in DataSources ) {
				var subset =
					new Uri(nsVoidFile.ToString() +
							String.Format("#{0}-{1}", Turtle.DATASET_LABEL.ToLower(), dataSource.GetLabel()));
				g.Assert(uriChemspiderDataset, Turtle.void_subset, subset);
			}
			//Vocabularies, topics, resources.
			_vocabularies.ForEach(v => g.Assert(uriChemspiderDataset, Turtle.void_vocabulary, new Uri(v)));
			g.Assert(uriChemspiderDataset, Turtle.dcterms_subject, _vocabularySubject);
			g.Assert(uriChemspiderDataset, Turtle.void_exampleResource, _exampleResource);

			//Update Frequency.
			g.Assert(uriChemspiderDataset, Turtle.voag_frequencyOfChange, Turtle.freq_monthly);

			//Technical features.
			g.Assert(uriChemspiderDataset, Turtle.void_feature, Turtle.ns_turtle);

			//Now write details of each subset.
			foreach ( var dataSource in DataSources ) {
				AddDsnVoidSubsetInfo(exp, g, dataSource, uriChemspiderDataset, nsVoidFile, uriPreviousVersion);
			}
			return g;
		}

		/// <summary>
		/// Exports the Void linkset.
		/// </summary>
		/// <param name="exp">The DataExport object</param>
		/// <param name="encoding">The output encoding for this export</param>
		public override void Export(IDataExport exp, Encoding encoding)
		{
			base.Export(exp, encoding);

			//Populate the graph.
			Graph g = VoidGraph(exp);

			//Write the output to file.
			using ( TextWriter w = new StreamWriter(FileFullPath, true, encoding) ) {
				var turtleWriter = new TurtleWriter();
				turtleWriter.Save(g, w);
			}
		}

		/// <summary>
		/// Returns the uri of the void of the previous export for this dsn_id.
		/// </summary>
		/// <param name="exp">The Data Export.</param>
		/// <param name="dataExportId">The lowest Export Id in this batch of exports.</param>
		/// <returns>The URI of the previous export.</returns>
		//private Uri GetPreviousVoidVersion(IDataExport exp, int DataExportId)
		private static Uri GetPreviousVoidVersion(IDataExport exp, int dataExportId)
		{
			var previousDataExportLog = exp.DataExportStore.GetPreviousDataExportLog(dataExportId);

			if ( previousDataExportLog != null && previousDataExportLog.ExportDate != null ) {
				//Generate the file name.
				var fileName = "void_" + ( (DateTime)previousDataExportLog.ExportDate ).ToString("yyyy-MM-dd") + ".ttl";
				//Put it into the root directory for the export date.
				var targetDirectory = ( (DateTime)previousDataExportLog.ExportDate ).ToString("yyyyMMdd");
				//Return the previous version void uri.
				return new Uri(string.Format("{0}/{1}/{2}", exp.DownloadUrl.TrimEnd('/'), targetDirectory.TrimEnd('/'), fileName));
			}
			return null;
		}

		/// <summary>
		/// Where we link to another dataset which has no rdf we must populate the information here.
		/// </summary>
		/// <param name="exp">The data export</param>
		/// <param name="g">The graph</param>
		/// <param name="dataSource">The data source</param>
		/// <param name="nsVoidFile">The void file uri</param>
		/// <param name="datasetVoidUri">The dataset uri</param>
		private void GetDsnDatasetInfo(IDataExport exp, Graph g, DataSource dataSource, Uri nsVoidFile,
			Uri datasetVoidUri)
		{
			var dsnDataset = new Uri(nsVoidFile.ToString() + string.Format("#{0}", dataSource.GetLabel()));
			var version = exp.DataExportStore.GetCurrentDataVersion(dataSource.Guid);
			var formattedImportedOn = version.DownloadDate.ToString(Turtle.DATE_TIME_FORMAT);

			g.Assert(dsnDataset, Turtle.has_type, Turtle.dctype_Dataset);

			if ( !string.IsNullOrEmpty(dataSource.Url) )
				g.Assert(dsnDataset, Turtle.foaf_homepage, new Uri(dataSource.Url));

			//Only include this information if we are linking to a dataset with no void uri.
			if ( datasetVoidUri == null ) {
				g.Assert(dsnDataset, Turtle.dcterms_title,
					string.Format("The {0} Dataset", dataSource.Name));

				if ( !string.IsNullOrEmpty(version.LicenseUri) )
					g.Assert(dsnDataset, Turtle.dcterms_license, new Uri(version.LicenseUri));

				if ( !string.IsNullOrEmpty(version.UriSpace) )
					g.Assert(dsnDataset, Turtle.void_uriSpace, version.UriSpace, Turtle.xsd_string);

				if ( !string.IsNullOrEmpty(dataSource.Url) )
					g.Assert(dsnDataset, Turtle.dcterms_publisher, new Uri(dataSource.Url));
			}

			if ( !string.IsNullOrEmpty(version.VersionName) )
				g.Assert(dsnDataset, Turtle.pav_version, version.VersionName, Turtle.xsd_string);

			if ( !string.IsNullOrEmpty(version.DownloadUri) )
				g.Assert(dsnDataset, Turtle.pav_retrievedFrom, new Uri(version.DownloadUri));

			if ( !string.IsNullOrEmpty(version.DownloadedBy) )
				g.Assert(dsnDataset, Turtle.pav_retrievedBy,
					new Uri(String.Format(_userProfileUrlPrefix, version.DownloadedBy)));

			if ( formattedImportedOn != string.Empty )
				g.Assert(dsnDataset, Turtle.pav_retrievedOn, formattedImportedOn, Turtle.xsd_dateTime);
		}

		/// <summary>
		///  Returns metadata relating to the linkset subsets.
		/// </summary>
		/// <param name="exp">The DataExport</param>
		/// <param name="g">The graph</param>
		/// <param name="subset">The subset uri</param>
		/// <param name="dataSource">The data source</param>
		/// <param name="predicateLabel">The predicate label</param>
		/// <param name="predicateDescription">A description of the predicate</param>
		/// <param name="linksetLabel">The label for the linkset</param>
		/// <param name="linksetDescription">The description of the linkset</param>
		/// <param name="nsVoidFile">The void file namespace</param>
		/// <param name="uriParentDataset">The uri of the parent dataset</param>
		/// <param name="skosPredicate">The skos predicate used to describe the linkset</param>
		/// <param name="isParentChild">Whether the linkset is a parent_child linkset</param>
		/// <param name="isChemspider">Whether the linkset links to ChemSpider records</param>
		/// <param name="dataExportLog">The data export log (for triple count retrieval)</param>
		/// <param name="previousVersion">The previous version of the linkset</param>
		/// <param name="datasetVoidUri">The void uri of the dataset</param>
		/// <param name="expresses">The predicate describing how the predicate is expressed</param>
		private void GetDsnVoidSubsetMatchInfo(IDataExport exp, Graph g, Uri subset
			, DataSource dataSource, string predicateLabel, string predicateDescription, string linksetLabel
			, string linksetDescription, Uri nsVoidFile, Uri uriParentDataset
			, SkosPredicate skosPredicate, bool isParentChild, bool isChemspider, DataExportLog dataExportLog
			, Uri previousVersion, Uri datasetVoidUri, Uri expresses)
		{
			string linksetTitle;
			Uri objectsTarget;
			Uri subjectsTarget;

			var filePart = skosPredicate.ToName().Replace("Match", "").ToLower();
			var matchDescription = String.Format("{0}Match", filePart);

			if ( isParentChild ) {
				matchDescription = string.Format("{0}_{1}_{2}", linksetLabel, predicateLabel, matchDescription);
				filePart = string.Format("{0}_{1}_{2}", filePart, linksetLabel, predicateLabel);
				linksetTitle = string.Format("{0} {1} {2} Parent-Child Linkset", Turtle.DATASET_LABEL, dataSource.Name,
					predicateDescription);
				linksetDescription = string.Format("{0}: {1}", predicateDescription, linksetDescription);
			}
			else if ( isChemspider ) {
				matchDescription = string.Format("{0}_{1}", linksetLabel, matchDescription);
				filePart = string.Format("{0}_{1}", filePart, linksetLabel);
				linksetTitle = string.Format("{0} {1} OPS-ChemSpider Linkset", Turtle.DATASET_LABEL, dataSource.Name);
				linksetDescription =
					string.Format("{0} linkset of compounds deposited into {1} that {2} ChemSpider compounds.",
						dataSource.Name, Turtle.DATASET_LABEL, skosPredicate.ToDescription());
			}
			else {
				linksetTitle = string.Format("{0} {1} {2} Linkset", Turtle.DATASET_LABEL, dataSource.Name,
					string.Format("{0} {1}", filePart, "match"));
				linksetDescription = string.Format(
					"{0} linkset of compounds deposited into {1} that {2} {1} compounds.", dataSource.Name,
					Turtle.DATASET_LABEL, skosPredicate.ToDescription());
			}

			var dsnMatch =
				new Uri(nsVoidFile.ToString() + string.Format("#{0}_{1}", dataSource.GetLabel(), matchDescription));
			g.Assert(subset, Turtle.void_subset, dsnMatch);
			g.Assert(dsnMatch, Turtle.dcterms_title, linksetTitle);
			g.Assert(dsnMatch, Turtle.dcterms_description, linksetDescription);
			g.Assert(dsnMatch, Turtle.has_type, Turtle.void_Linkset);
			g.Assert(dsnMatch, Turtle.dcterms_license, _chemspiderLicense);

			//Get the filename so we can use it to retrieve the Triple Count.
			var turtleFileName = string.Format("LINKSET_{0}_{1}{2}.ttl", filePart.ToUpper(), dataSource.GetLabel().ToUpper(), exp.ExportDate.ToString("yyyyMMdd"));
			//Get the data dump url (include .gz as all these files are compressed TODO: Store the file name in the compressed format).
			var dataDumpUrl = string.Format("{0}/{1}/{2}/{3}{4}", exp.DownloadUrl.TrimEnd('/'), exp.ExportDate.ToString("yyyyMMdd"), dataSource.GetLabel().ToUpper(), turtleFileName, exp.Compress ? ".gz" : "");

			//Get the Data Export for retrieving the Triples Counts.
			var noOfTriples = dataExportLog.Files.FirstOrDefault(f => f.FileName == turtleFileName).RecordCount;

			if ( isParentChild ) {
				//Parent-Child Linksets link OPS compounds together.
				objectsTarget = uriParentDataset;
				subjectsTarget = uriParentDataset;
			}
			else if ( isChemspider ) {
				//OPS-ChemSpider Linksets link OPS compounds to ChemSpider compounds.
				objectsTarget = new Uri(Turtle.CHEMSPIDER_RDF_URI + "/void.rdf");
				subjectsTarget = uriParentDataset;
			}
			else {
				//Data source Linksets.
				objectsTarget = new Uri(nsVoidFile.ToString() + string.Format("#{0}", dataSource.GetLabel()));
				subjectsTarget =
					new Uri(nsVoidFile.ToString() +
							string.Format("#{0}-{1}", Turtle.DATASET_LABEL.ToLower(), dataSource.GetLabel()));

				//Use the void file for the dataset if there is one.
				if ( datasetVoidUri != null )
					objectsTarget = datasetVoidUri;
			}

			//Link Information, subjectsTarget, objectsTarget and linkPredicate.
			g.Assert(dsnMatch, Turtle.void_objectsTarget, objectsTarget);
			g.Assert(dsnMatch, Turtle.void_subjectsTarget, subjectsTarget);

			//void:linkPredicate
			g.Assert(dsnMatch, Turtle.void_linkPredicate, skosPredicate.ToUri());

			//How can we express the relationship between the matches.
			g.Assert(dsnMatch, Turtle.dul_expresses, expresses);

			//Linkset Provenance.
			g.Assert(dsnMatch, Turtle.pav_authoredBy, _provenanceAuthoredBy);
			g.Assert(dsnMatch, Turtle.pav_authoredOn, _provenanceAuthoredOn, Turtle.xsd_dateTime);
			g.Assert(dsnMatch, Turtle.pav_createdWith, _homePage);
			g.Assert(dsnMatch, Turtle.pav_createdBy, _provenanceAuthoredBy);
			g.Assert(dsnMatch, Turtle.pav_createdOn,
				exp.ExportDate.ToString(Turtle.DATE_TIME_FORMAT), Turtle.xsd_dateTime);

			//The previous version of this void information.
			if ( previousVersion != null )
				g.Assert(dsnMatch, Turtle.pav_previousVersion,
					new Uri(previousVersion.ToString() +
							string.Format("#{0}_{1}", dataSource.GetLabel(), matchDescription)));

			//Linkset statistics - no of triples.
			g.Assert(dsnMatch, Turtle.void_triples, noOfTriples.ToString(), Turtle.xsd_integer);

			//Dataset access.
			g.Assert(dsnMatch, Turtle.void_dataDump, new Uri(dataDumpUrl));

			//Add the Dataset info.
			GetDsnDatasetInfo(exp, g, dataSource, nsVoidFile, datasetVoidUri);
		}

		/// <summary>
		/// Adds details of a dsn subset to the Graph.
		/// </summary>
		/// <param name="exp">The data export</param>
		/// <param name="g">The graph to be populated</param>
		/// <param name="dataSource">The dataSource</param>
		/// <param name="uriChemspiderDataset">Uri for the ChemSpider dataset</param>
		/// <param name="nsVoidFile">Uri for the Void file</param>
		/// <param name="uriPreviousVersion">Uri of the previous version</param>
		private void AddDsnVoidSubsetInfo(IDataExport exp, Graph g, DataSource dataSource, Uri uriChemspiderDataset,
			Uri nsVoidFile, Uri uriPreviousVersion)
		{
			var useSkosRelated = Turtle.UseSkosRelatedMatchForDsn(dataSource.Name);
			var dsnLabel = dataSource.Name.Replace(" ", "_").ToLower();
			var subset = new Uri(nsVoidFile.ToString() + string.Format("#{0}-{1}", Turtle.DATASET_LABEL.ToLower(), dsnLabel));

			//Get the DataVersion.
			var version = exp.DataExportStore.GetCurrentDataVersion(dataSource.Guid);
			if ( version == null )
				throw new Exception("null DataVersion");
			g.Assert(subset, Turtle.has_type, Turtle.void_DataSet);

			if ( !string.IsNullOrEmpty(dataSource.Name) ) {
				g.Assert(subset, Turtle.dcterms_title,
					string.Format("{0} {1} Subset", Turtle.DATASET_LABEL, dataSource.Name));
				g.Assert(subset, Turtle.dcterms_description,
					string.Format("The subset of {0} that contains {1} data.", Turtle.DATASET_LABEL, dataSource.Name));
			}

			if ( !string.IsNullOrEmpty(dataSource.Url) )
				g.Assert(subset, Turtle.foaf_page, new Uri(dataSource.Url));

			//Add the links to subsets

			//Provenance.
			g.Assert(subset, Turtle.prov_wasDerivedFrom,
				new Uri(nsVoidFile.ToString() + string.Format("#{0}", dsnLabel)));

			//Add the locations of the synonyms, properties and issues files.
			var synonymsUri =
				new Uri(string.Format("{0}/{1}/{2}/SYNONYMS_{3}{4}.ttl{5}", exp.DownloadUrl.TrimEnd('/'),
					exp.ExportDate.ToString("yyyyMMdd"), dsnLabel.ToUpper(),
					exp.ExportDate.ToString("yyyyMMdd"), dsnLabel.ToUpper(), exp.Compress ? ".gz" : ""));
			g.Assert(subset, Turtle.void_dataDump, synonymsUri);

			var propertiesUri =
				new Uri(string.Format("{0}/{1}/{2}/PROPERTIES_{3}{4}.ttl{5}", exp.DownloadUrl.TrimEnd('/'),
					exp.ExportDate.ToString("yyyyMMdd"), dsnLabel.ToUpper(),
					exp.ExportDate.ToString("yyyyMMdd"), dsnLabel.ToUpper(), exp.Compress ? ".gz" : ""));
			g.Assert(subset, Turtle.void_dataDump, propertiesUri);

			var issuesUri =
				new Uri(string.Format("{0}/{1}/{2}/ISSUES_{3}{4}.ttl{5}", exp.DownloadUrl.TrimEnd('/'),
					exp.ExportDate.ToString("yyyyMMdd"), dsnLabel.ToUpper(),
					exp.ExportDate.ToString("yyyyMMdd"), dsnLabel.ToUpper(), exp.Compress ? ".gz" : ""));
			g.Assert(subset, Turtle.void_dataDump, issuesUri);

			Uri uriDatasetVoid = null;
			if ( !String.IsNullOrEmpty(version.VoidUri) ) {
				uriDatasetVoid = new Uri(version.VoidUri);
			}

			//Get the Data Export for retrieving the Triples Counts.
			var dataExport = exp.DataExportStore.GetDataExportLog(DataSourceExportIds.Where(i => i.Value == dataSource.Guid).Select(i => i.Key).Single());
			if ( dataExport == null )
				throw new Exception("null data export file");
			//All dsns have exactMatch subsets.
			GetDsnVoidSubsetMatchInfo(exp, g, subset, dataSource, string.Empty, string.Empty, string.Empty, string.Empty
				, nsVoidFile, uriChemspiderDataset, SkosPredicate.EXACT_MATCH, false, false
				, dataExport
				, uriPreviousVersion
				, uriDatasetVoid
				, GetExpressesUri(dataSource.Name, useSkosRelated));

			//Some dsns have relatedMatch subsets.
			if ( useSkosRelated )
				GetDsnVoidSubsetMatchInfo(exp, g, subset, dataSource, string.Empty, string.Empty, string.Empty,
					string.Empty
					, nsVoidFile, uriChemspiderDataset, SkosPredicate.RELATED_MATCH, false, false
					, dataExport
					, uriPreviousVersion
					, uriDatasetVoid
					, GetExpressesUri(dataSource.Name, true));

			//Parent-child subsets.
			foreach ( var parentChildRelationship in exp.CompoundsStore.GetParentChildRelationships() ) {
				//Get the parentChild information.
				GetDsnVoidSubsetMatchInfo(exp
					, g
					, subset
					, dataSource
					, parentChildRelationship.GetName().Replace(" ", "_").Replace(".", "_").ToLower()
					, parentChildRelationship.GetName()
					, "parent_child"
					, parentChildRelationship.GetDescription()
					, nsVoidFile
					, uriChemspiderDataset
					, parentChildRelationship.GetPredicate()
					, true
					, false
					, dataExport
					, uriPreviousVersion
					, uriDatasetVoid
					, parentChildRelationship.GetRdfPredicate());
			}

			//ChemSpider-OPS exactMatch subset.
			GetDsnVoidSubsetMatchInfo
				(exp
					, g
					, subset
					, dataSource
					, string.Empty
					, string.Empty
					, "ops_chemspider"
					, string.Empty
					, nsVoidFile
					, uriChemspiderDataset
					, SkosPredicate.EXACT_MATCH
					, false
					, true
					, dataExport
					, uriPreviousVersion
					, uriDatasetVoid
					, GetExpressesUri(dataSource.Name, useSkosRelated));
		}

		//Hard-coded list of what the linkset expresses.
		private static Uri GetExpressesUri(string dsn, bool useSkosRelated)
		{
			switch ( dsn.ToLower() ) {
				case "pdb":
					return useSkosRelated ? Turtle.dul_expressesLigates : Turtle.dul_expressesInchi;
				default:
					//Unsupported dsn.
					return Turtle.dul_expressesInchi;
			}
		}
	}
}
