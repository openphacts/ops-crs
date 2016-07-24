using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
using System.Net;

using ChemSpider.Web;
using ChemSpider.ObjectModel;
using ChemSpider.Utilities;
using ChemSpider.Database;
using ChemSpider.Molecules;
using ChemSpider.Search;
using System.Data.SqlClient;
using ChemSpider.Compounds.Database;
using System.Data;

//using ChemSpider.CVSP.Compounds;
using ChemSpider.Compounds.Search;
using ChemSpider.CVSP.Compounds;

[CSServices(Description = "RESTful service for integration with ChemSpider. Accepting requests and returning responses in JSON format",
	Serializer = typeof(DataContractJsonSerializer),
	SerializerSurrogate = typeof(FlexibleSerializerSurrogate),
	SerializerKnownTypes = new System.Type[] {
		typeof(List<PredictedProperty>),
		typeof(List<Synonym>),
		typeof(List<Identifier>),
		typeof(List<ExperimentalProperty>),
		typeof(List<Reference>),
		typeof(List<Blob>),
		typeof(ChemIdUtils.N2SResult),
		typeof(List<DatasourceType>),
		typeof(List<Datasource>),
		typeof(List<ResultRecord>) },
	XSLT = "JSON.xsl",
	ResourceFile = "JSONResource.resx")]
public class JSON : CSHttpHandler
{
	[CSService(Service = "JSON.__GetNamesSample",
		Description = "Internal operation - returns a sample of data from database to run tests")]
	[CSHiddenService]
	public List<KeyValuePair<int, string>> __GetNamesSample([CSParameter(DefaultValue = "10")] int limit)
	{
		using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
		{
			DataTable dt = conn.FillDataTable(String.Format(@"
				select top {0} c.csid, cs.synonym
				from chemspider_synonyms cs join compounds c on cs.chemspider_csid = c.chemspider_csid
				where cs.validated_yn = 1 and cs.dbid_yn = 0 and cs.compound_title_yn = 1", limit));
			return dt.Rows
				.Cast<DataRow>()
				.Select(dr => new KeyValuePair<int, string>((int)(long)dr[0], (string)dr[1]))
				.ToList();
		}
	}

	[CSService(Service = "JSON.__GetSMILESSample",
		Description = "Internal operation - returns a sample of data from database to run tests")]
	[CSHiddenService]
	public List<KeyValuePair<int, string>> __GetSMILESSample([CSParameter(DefaultValue = "10")] int limit)
	{
		using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
		{
			DataTable dt = conn.FillDataTable(String.Format(@"
				select top {0} cs.csid, cs.oe_abs_smiles
				from compounds_smiles cs
				where len(cs.oe_abs_smiles) > 10 and len(cs.oe_abs_smiles) < 50", limit));
			return dt.Rows
				.Cast<DataRow>()
				.Select(dr => new KeyValuePair<int, string>((int)(long)dr[0], (string)dr[1]))
				.ToList();
		}
	}

	[CSService(Service = "JSON.__GetSMARTSSample",
		Description = "Internal operation - returns a sample of data from database to run tests")]
	[CSHiddenService]
	public List<KeyValuePair<int, string>> __GetSMARTSSample([CSParameter(DefaultValue = "10")] int limit)
	{
		using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
		{
			DataTable dt = conn.FillDataTable(String.Format(@"
				select top {0} cs.csid, cs.oe_abs_smiles
				from compounds_smiles cs
				where len(cs.oe_abs_smiles) > 10 and len(cs.oe_abs_smiles) < 50 and cs.oe_abs_smiles not like '%C=%' and cs.oe_abs_smiles not like '%=C%'", limit));
			return dt.Rows
				.Cast<DataRow>()
				.Select(dr => new KeyValuePair<int, string>((int)(long)dr[0], (string)dr[1]))
				.ToList();
		}
	}

	[CSService(Service = "JSON.__GetInChISample",
		Description = "Internal operation - returns a sample of data from database to run tests")]
	[CSHiddenService]
	public List<KeyValuePair<int, KeyValuePair<string, string>>> __GetInChISample([CSParameter(DefaultValue = "10")] int limit)
	{
		using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
		{
			DataTable dt = conn.FillDataTable(String.Format(@"
				select top {0} c.csid, i.inchi_key, i.inchi
				from compounds c join inchis i on c.inc_id = c.inc_id", limit));
			return dt.Rows
				.Cast<DataRow>()
				.Select(dr => new KeyValuePair<int, KeyValuePair<string, string>>((int)(long)dr[0], new KeyValuePair<string, string>((string)dr[1], (string)dr[2])))
				.ToList();
		}
	}

	[CSService(Service = "JSON.SimpleSearch",
		Description = "Run a simple search which tries to interpret a query string as anything it can search by (Synonym, SMILES, InChI, ChemSpider ID etc.)")]
	public string SimpleSearch(
		[CSParameter(PresenceType = ParameterPresenceType.Mandatory)]
		SimpleSearchOptions searchOptions,
		CommonSearchOptions commonOptions,
		CSCSearchScopeOptions scopeOptions,
		SearchResultOptions resultOptions,
		[CSParameter(DefaultValue = "-1", Obsolete = "Please use property from SearchResultOptions object")]
		int limit)
	{
		if (limit != -1)
		{
			if (resultOptions == null) resultOptions = new SearchResultOptions();
			resultOptions.Limit = limit;
		}

		return SearchUtility.RunSearch(CSCSearchFactory.GetSimpleSearch(), searchOptions, commonOptions, scopeOptions, resultOptions).Rid;
	}

	[CSService(Service = "JSON.ExactStructureSearch",
		Description = "Run identical structure search")]
	public string ExactStructureSearch(
		[CSParameter(PresenceType = ParameterPresenceType.Mandatory)]
		ExactStructureSearchOptions searchOptions,
		CommonSearchOptions commonOptions,
		CSCSearchScopeOptions scopeOptions,
		SearchResultOptions resultOptions,
		[CSParameter(DefaultValue = "-1", Obsolete = "Please use property from SearchResultOptions object")]
		int limit)
	{
		if (limit != -1)
		{
			if (resultOptions == null) resultOptions = new SearchResultOptions();
			resultOptions.Limit = limit;
		}

		// Workaround for OpenEye/Indigo SMILES discrepancies
		if ( searchOptions.Molecule.SplitOnNewLines().Count == 1 ) {
			List<ChemSpider.ObjectModel.Issue> issues = new List<ChemSpider.ObjectModel.Issue>();
			string indigo_smiles = ValidationModules.ValidateSMILES(searchOptions.Molecule, issues, false, true, true);

			if ( !string.IsNullOrEmpty(indigo_smiles) )
				searchOptions.Molecule = indigo_smiles;
		}

		return SearchUtility.RunSearch(CSCSearchFactory.GetExactStructureSearch(), searchOptions, commonOptions, scopeOptions, resultOptions).Rid;
	}

	[CSService(Service = "JSON.SubstructureSearch",
		Description = "Run substructure search.")]
	public string SubstructureSearch(
		[CSParameter(PresenceType = ParameterPresenceType.Mandatory)]
		SubstructureSearchOptions searchOptions,
		CommonSearchOptions commonOptions,
		CSCSearchScopeOptions scopeOptions,
		SearchResultOptions resultOptions,
		[CSParameter(DefaultValue = "-1", Obsolete = "Please use property from SearchResultOptions object")]
		int limit)
	{
		if (limit != -1)
		{
			if (resultOptions == null) resultOptions = new SearchResultOptions();
			resultOptions.Limit = limit;
		}

		// Workaround for OpenEye/Indigo SMILES discrepancies
		if (searchOptions.Molecule.SplitOnNewLines().Count == 1)
		{
			List<ChemSpider.ObjectModel.Issue> issues = new List<ChemSpider.ObjectModel.Issue>();
			string indigo_smiles = ValidationModules.ValidateSMILES(searchOptions.Molecule, issues, false, true, true);

			if (!string.IsNullOrEmpty(indigo_smiles))
				searchOptions.Molecule = indigo_smiles;
		}

		return SearchUtility.RunSearch(CSCSearchFactory.GetSubstructureSearch(), searchOptions, commonOptions, scopeOptions, resultOptions).Rid;
	}

	[CSService(Service = "JSON.SimilaritySearch",
		Description = "Run similarity search.")]
	public string SimilaritySearch(
		[CSParameter(PresenceType = ParameterPresenceType.Mandatory)]
		SimilaritySearchOptions searchOptions,
		CommonSearchOptions commonOptions,
		CSCSearchScopeOptions scopeOptions,
		SearchResultOptions resultOptions,
		[CSParameter(DefaultValue = "-1", Obsolete = "Please use property from SearchResultOptions object")]
		int limit)
	{
		if (limit != -1)
		{
			if (resultOptions == null) resultOptions = new SearchResultOptions();
			resultOptions.Limit = limit;
		}

		// Workaround for OpenEye/Indigo SMILES discrepancies
		if (searchOptions.Molecule.SplitOnNewLines().Count == 1)
		{
			List<ChemSpider.ObjectModel.Issue> issues = new List<ChemSpider.ObjectModel.Issue>();
			string indigo_smiles = ValidationModules.ValidateSMILES(searchOptions.Molecule, issues, false, true, true);

			if (!string.IsNullOrEmpty(indigo_smiles))
				searchOptions.Molecule = indigo_smiles;
		}

		return SearchUtility.RunSearch(CSCSearchFactory.GetSimilarityStructureSearch(), searchOptions, commonOptions, scopeOptions, resultOptions).Rid;
	}
	/*
		[CSService(Service = "JSON.DataSourceSearch", 
			Description = "Run search by data source, data collection or data slice.")]
		public string DataSourceSearch(
			[CSParameter(PresenceType = ParameterPresenceType.Mandatory)]
			DataSourceSearchOptions searchOptions,
			CommonSearchOptions commonOptions,
			SearchResultOptions resultOptions,
			[CSParameter(DefaultValue = "-1", Obsolete = "Please use property from SearchResultOptions object")]
			int limit)
		{
			if (limit != -1)
			{
				if (resultOptions == null) resultOptions = new SearchResultOptions();
				resultOptions.Limit = limit;
			}

			return SearchUtility.RunSearch(CSSearchFactory.GetDataSourceSearch(), searchOptions, commonOptions, null, resultOptions).Rid;
		}
	*/
	[CSService(Service = "JSON.GetSearchStatus", Description = "Returns the status of request.")]
	public RequestStatus GetSearchStatus([CSParameter(PresenceType = ParameterPresenceType.Mandatory, Description = "Request ID that was returned by any other method that run search procedure")] string rid)
	{
		RequestStatus status = Request.getRequestStatus(rid);

		if (status.Status == ERequestStatus.Failed)
			throw new RESTServiceException(500, "GetSearchStatus error: {0}", status.Message);

		return status;
	}

	[CSService(Service = "JSON.GetSearchResult", Description = "Get search results by Request ID.")]
	public List<int> GetSearchResult(
		[CSParameter(PresenceType = ParameterPresenceType.Mandatory)] string rid,
		[CSParameter(PresenceType = ParameterPresenceType.Discretionary, DefaultValue = "0")] int start,
		[CSParameter(PresenceType = ParameterPresenceType.Discretionary, DefaultValue = "-1")] int count)
	{
		CSRequestSearchResult result = new CSRequestSearchResult(rid);
		List<int> found = result.Found.ToList();
		return found.Skip(start).Take(count < 0 ? found.Count - start : count).ToList();
	}

	[CSService(Service = "JSON.GetSearchResultWithRelevance", Description = "Get two columns (CSID and Relevance) search results by Request ID.")]
	public List<ResultRecord> GetSearchResultWithRelevance(
		[CSParameter(PresenceType = ParameterPresenceType.Mandatory)] string rid,
		[CSParameter(PresenceType = ParameterPresenceType.Discretionary, DefaultValue = "0")] int start,
		[CSParameter(PresenceType = ParameterPresenceType.Discretionary, DefaultValue = "-1")] int count)
	{
		CSRequestSearchResult result = new CSRequestSearchResult(rid);
		List<ResultRecord> res = result.Found.GetResultRecords();
		return res.Skip(start).Take(count < 0 ? res.Count - start : count).ToList();
	}

	[CSService(Service = "JSON.GetSearchResultAsCompounds", Description = "Returns search results by Request ID.")]
	public List<ChemSpider.OPS.ObjectModel.Compound> GetSearchResultAsCompounds(
		[CSParameter(PresenceType = ParameterPresenceType.Mandatory)] string rid,
		[CSParameter(PresenceType = ParameterPresenceType.Discretionary, DefaultValue = "0")] int start,
		[CSParameter(PresenceType = ParameterPresenceType.Discretionary, DefaultValue = "-1")] int count,
		[CSParameter(PresenceType = ParameterPresenceType.Discretionary)] string serfilter //   DO NOT REMOVE THIS FAKE PARAMETER!!! It's needed for help page generation!
		)
	{
		CSRequestSearchResult result = new CSRequestSearchResult(rid);
		List<int> found = result.Found.ToList();
		return found.Skip(start).Take(count < 0 ? found.Count - start : count).Select(cmp_id => new ChemSpider.OPS.ObjectModel.Compound(cmp_id)).ToList();
	}

	[CSService(Service = "JSON.GetSearchResultAsSdf", Description = "Returns results of the search identified by Request ID as SDF.")]
	public void GetSearchResultAsSdf(
		[CSParameter(PresenceType = ParameterPresenceType.Mandatory)] string rid,
		[CSParameter(PresenceType = ParameterPresenceType.Discretionary, DefaultValue = "0")] int start,
		[CSParameter(PresenceType = ParameterPresenceType.Discretionary, DefaultValue = "-1")] int count,
		TextWriter w)
	{
		CSRequestSearchResult result = new CSRequestSearchResult(rid);
		List<int> found = result.Found.ToList();
		IEnumerable<int> batch = found.Skip(start).Take(count < 0 ? found.Count - start : count);
		foreach (int cmp_id in batch)
		{
			Compound cmp = new Compound(cmp_id);
			w.Write(cmp.Sdf);
		}
	}

	[CSService(Service = "JSON.GetRecordsAsCompounds", Description = "Returns information about requested records specified as an array of CSIDs as array of Compound objects.")]
	public List<ChemSpider.OPS.ObjectModel.Compound> GetRecordsAsCompounds(
		[CSParameter(PresenceType = ParameterPresenceType.Mandatory)] int[] csids,
		[CSParameter(PresenceType = ParameterPresenceType.Discretionary)] string serfilter //   DO NOT REMOVE THIS FAKE PARAMETER!!! It's needed for help page generation!
		)
	{
		return csids.Select(cmp_id => new ChemSpider.OPS.ObjectModel.Compound(cmp_id)).ToList();
	}

	[CSService(Service = "JSON.GetRecordsAsSdf", Description = "Returns information about requested records specified as an array of CSIDs as SDF.")]
	public void GetRecordsAsSdf(
		[CSParameter(PresenceType = ParameterPresenceType.Mandatory)] int[] csids,
		TextWriter w)
	{
		foreach (int cmp_id in csids)
		{
			ChemSpider.OPS.ObjectModel.Compound cmp = new ChemSpider.OPS.ObjectModel.Compound(cmp_id);
			w.Write(cmp.Sdf);
		}
	}

	[CSService(Service = "JSON.ConvertTo", Description = "Convert Name, SMILES, InChI or ChemSpider ID into structure ")]
	public ChemIdUtils.N2SResult ConvertTo([CSParameter(PresenceType = ParameterPresenceType.Mandatory)] ChemIdUtils.ConvertOptions convertOptions)
	{
		ChemSpider.Compounds.IConvertToCompoundId convertService = new ChemSpider.Compounds.CompoundsService(new SQLCompoundsStore(new SQLSubstancesStore()));

		ChemIdUtils.N2SResult res = new ChemIdUtils.N2SResult
		{
			confidence = 0,
			message = string.Empty,
			mol = string.Empty
		};

		try
		{
			string mol = string.Empty;
			int compoundId = 0;
			IEnumerable<int> compoundIds = null;

			switch (convertOptions.Direction)
			{
				case ChemIdUtils.ConvertOptions.EDirection.InChi2CSID:
					compoundIds = convertService.InChIToCompoundId(convertOptions.Text);
					if (compoundIds.Count() > 0)
					{
						res.mol = compoundIds.First().ToString();
						res.confidence = 100;
					}
					break;
				case ChemIdUtils.ConvertOptions.EDirection.InChiKey2CSID:
					compoundIds = convertService.InChIKeyToCompoundId(convertOptions.Text);
					if (compoundIds.Count() > 0)
					{
						res.mol = compoundIds.First().ToString();
						res.confidence = 100;
					}
					break;
				case ChemIdUtils.ConvertOptions.EDirection.SMILES2CSID:
					compoundId = convertService.SMILESToCompoundId(convertOptions.Text);
					if (compoundId > 0)
					{
						res.mol = compoundId.ToString();
						res.confidence = 100;
					}
					break;
				default:
					ChemSpider.Parts.Client.JSONClient parts = new ChemSpider.Parts.Client.JSONClient() { RedirectCookies = false };

					res = parts.ConvertTo(convertOptions);
					break;
			}

			if (string.IsNullOrEmpty(res.mol))
			{
				res.confidence = 0;
				res.message = "Cannot do the convertion. Sorry!";
			}
		}
		catch (Exception ex)
		{
			res.confidence = 0;
			res.message = string.Format("Cannot convert: {0}", ex.Message);
		}

		return res;
	}
}
