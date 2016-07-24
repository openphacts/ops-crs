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

using ChemSpider.Compounds.Search;
using Microsoft.Practices.ServiceLocation;
using System.Diagnostics;
//using ChemSpider.CVSP.Compounds;

public static class ChemSpiderSearchExtensions
{
	public static RSC.Compounds.Search.CompoundsCommonSearchOptions ToRSCCommonSearchOptions(this CommonSearchOptions options)
	{
		return new RSC.Compounds.Search.CompoundsCommonSearchOptions()
		{
		};
	}

	public static RSC.Compounds.Search.CompoundsSearchScopeOptions ToRSCSearchScopeOptions(this CSCSearchScopeOptions options)
	{
		return new RSC.Compounds.Search.CompoundsSearchScopeOptions()
		{
			RealOnly = options.RealOnly,
			//	TODO: convert datasources
			Datasources = new List<Guid>()
		};
	}

	public static RSC.Compounds.Search.CompoundsSearchResultOptions ToRSCSearchResultOptions(this SearchResultOptions options)
	{
		return new RSC.Compounds.Search.CompoundsSearchResultOptions()
		{
			Limit = options.Limit
		};
	}

	public static RSC.Compounds.Search.SimpleSearchOptions ToRSCSimpleSearchOptions(this SimpleSearchOptions options)
	{
		return new RSC.Compounds.Search.SimpleSearchOptions()
		{
			QueryText = options.QueryText
		};
	}

	public static RSC.Compounds.Search.ExactStructureSearchOptions ToRSCExactStructureSearchOptions(this ExactStructureSearchOptions options)
	{
		return new RSC.Compounds.Search.ExactStructureSearchOptions()
		{
			Molecule = options.Molecule,
			MatchType = (RSC.Compounds.Search.ExactStructureSearchOptions.EMatchType)options.MatchType
		};
	}

	public static RSC.Compounds.Search.SubStructureSearchOptions ToRSCSubStructureSearchOptions(this SubstructureSearchOptions options)
	{
		return new RSC.Compounds.Search.SubStructureSearchOptions()
		{
			Molecule = options.Molecule,
			MatchTautomers = options.MatchTautomers,
			MolType = (RSC.Compounds.Search.SubStructureSearchOptions.EMolType)options.MolType
		};
	}

	public static RSC.Compounds.Search.SimilarityStructureSearchOptions ToRSCSimilarityStructureSearchOptions(this SimilaritySearchOptions options)
	{
		return new RSC.Compounds.Search.SimilarityStructureSearchOptions()
		{
			Molecule = options.Molecule,
			Alpha = options.Alpha,
			Beta = options.Beta,
			Threshold = options.Threshold,
			SimilarityType = (RSC.Compounds.Search.SimilarityStructureSearchOptions.ESimilarityType)options.SimilarityType
		};
	}

	public static ChemSpider.OPS.ObjectModel.Compound ToOPSCompound(this RSC.Compounds.Compound compound)
	{
		return new ChemSpider.OPS.ObjectModel.Compound(compound)
		{
		};
	}
}

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
	private RSC.Compounds.ICompoundStore compoundStore = new RSC.Compounds.EntityFramework.EFCompoundStore2();
	private RSC.Compounds.ICompoundConvert compoundConvert = new RSC.Compounds.EntityFramework.EFCompoundConvert();

	private RSC.CVSP.Compounds.IValidationModule Validation
	{
		get
		{
			return ServiceLocator.Current.GetService(typeof(RSC.CVSP.Compounds.IValidationModule)) as RSC.CVSP.Compounds.IValidationModule;
		}
	}
/*
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
*/
	[CSService(Service = "JSON.SimpleSearch", Description = "Run a simple search which tries to interpret a query string as anything it can search by (Synonym, SMILES, InChI, ChemSpider ID etc.)")]
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

		ChemSpider.Parts.Client.JSONClient parts = new ChemSpider.Parts.Client.JSONClient() { RedirectCookies = false };

		//  convert name into InChI and try to search InChI in DB...
		ChemIdUtils.N2SResult res = parts.ConvertTo(new ChemIdUtils.ConvertOptions { Direction = ChemIdUtils.ConvertOptions.EDirection.Term2Mol, Text = searchOptions.QueryText });
		if (res.confidence == 100)
		{
			searchOptions.QueryText = InChINet.InChIUtils.mol2InChIKey(res.mol, InChINet.InChIFlags.Standard);
		}

		var rid = RSC.Search.SearchUtility.RunSearchAsync(typeof(RSC.Compounds.Search.SimpleSearch), searchOptions.ToRSCSimpleSearchOptions(), commonOptions.ToRSCCommonSearchOptions(), scopeOptions.ToRSCSearchScopeOptions(), resultOptions.ToRSCSearchResultOptions());

		return rid.ToString();
	}

	[CSService(Service = "JSON.ExactStructureSearch", Description = "Run identical structure search")]
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
			var issues = new List<RSC.Logging.Issue>();
			string indigo_smiles = Validation.ValidateSMILES(searchOptions.Molecule, issues, false, true, true);

			if ( !string.IsNullOrEmpty(indigo_smiles) )
				searchOptions.Molecule = indigo_smiles;
		}

		var rid = RSC.Search.SearchUtility.RunSearchAsync(typeof(RSC.Compounds.Search.ExactStructureSearch), searchOptions.ToRSCExactStructureSearchOptions(), commonOptions.ToRSCCommonSearchOptions(), scopeOptions.ToRSCSearchScopeOptions(), resultOptions.ToRSCSearchResultOptions());

		return rid.ToString();
	}

	[CSService(Service = "JSON.SubstructureSearch", Description = "Run substructure search.")]
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
			var issues = new List<RSC.Logging.Issue>();
			string indigo_smiles = Validation.ValidateSMILES(searchOptions.Molecule, issues, false, true, true);

			if (!string.IsNullOrEmpty(indigo_smiles))
				searchOptions.Molecule = indigo_smiles;
		}

		var rid = RSC.Search.SearchUtility.RunSearchAsync(typeof(RSC.Compounds.Search.SubStructureSearch), searchOptions.ToRSCSubStructureSearchOptions(), commonOptions.ToRSCCommonSearchOptions(), scopeOptions.ToRSCSearchScopeOptions(), resultOptions.ToRSCSearchResultOptions());

		return rid.ToString();
	}

	[CSService(Service = "JSON.SimilaritySearch", Description = "Run similarity search.")]
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
			var issues = new List<RSC.Logging.Issue>();
			string indigo_smiles = Validation.ValidateSMILES(searchOptions.Molecule, issues, false, true, true);

			if (!string.IsNullOrEmpty(indigo_smiles))
				searchOptions.Molecule = indigo_smiles;
		}

		var rid = RSC.Search.SearchUtility.RunSearchAsync(typeof(RSC.Compounds.Search.SimilarityStructureSearch), searchOptions.ToRSCSimilarityStructureSearchOptions(), commonOptions.ToRSCCommonSearchOptions(), scopeOptions.ToRSCSearchScopeOptions(), resultOptions.ToRSCSearchResultOptions());

		return rid.ToString();
	}

	[CSService(Service = "JSON.GetSearchStatus", Description = "Returns the status of request.")]
	public RSC.Search.RequestStatus GetSearchStatus([CSParameter(PresenceType = ParameterPresenceType.Mandatory, Description = "Request ID that was returned by any other method that run search procedure")] string rid)
	{
		var status = RSC.Search.RequestManager.Current.GetStatus(Guid.Parse(rid));

		if (status.Status == RSC.Search.ERequestStatus.Failed)
			throw new RESTServiceException(500, "GetSearchStatus error: {0}", status.Message);

		return status;
	}

	[CSService(Service = "JSON.GetSearchResult", Description = "Get search results by Request ID.")]
	public List<int> GetSearchResult(
		[CSParameter(PresenceType = ParameterPresenceType.Mandatory)] string rid,
		[CSParameter(PresenceType = ParameterPresenceType.Discretionary, DefaultValue = "0")] int start,
		[CSParameter(PresenceType = ParameterPresenceType.Discretionary, DefaultValue = "-1")] int count)
	{
		var guids = new List<Guid>();

		//	please do not try to we-write it using LINQ... it may cause some problems in LIVE
		foreach(var r in RSC.Search.RequestManager.Current.GetResults(Guid.Parse(rid), start, count))
		{
			if(r is RSC.Compounds.Search.SearchResult)
				guids.Add((r as RSC.Compounds.Search.SearchResult).Id);
		}

		var opsIds = compoundStore.GetCompoundsExternalReferences(guids, ChemSpider.OPS.ObjectModel.Compound.OPS_URI);

		return opsIds.Where(id => id.Value.Any()).Select(er => Convert.ToInt32(er.Value.First().Value)).ToList();
	}

	[CSService(Service = "JSON.GetSearchResultWithRelevance", Description = "Get two columns (CSID and Relevance) search results by Request ID.")]
	public List<ResultRecord> GetSearchResultWithRelevance(
		[CSParameter(PresenceType = ParameterPresenceType.Mandatory)] string rid,
		[CSParameter(PresenceType = ParameterPresenceType.Discretionary, DefaultValue = "0")] int start,
		[CSParameter(PresenceType = ParameterPresenceType.Discretionary, DefaultValue = "-1")] int count)
	{
		var results = new List<RSC.Compounds.Search.StructureSearchResult>();

		//	please do not try to we-write it using LINQ... it may cause some problems in LIVE
		foreach (var r in RSC.Search.RequestManager.Current.GetResults(Guid.Parse(rid), start, count))
		{
			if (r is RSC.Compounds.Search.StructureSearchResult)
				results.Add(r as RSC.Compounds.Search.StructureSearchResult);
			else if(r is RSC.Compounds.Search.SearchResult)
				results.Add(new RSC.Compounds.Search.StructureSearchResult() { Id = (r as RSC.Compounds.Search.SearchResult).Id, Similarity = 0 });
		}

		var opsIds = compoundStore.GetCompoundsExternalReferences(results.Select(r => r.Id), ChemSpider.OPS.ObjectModel.Compound.OPS_URI);

		return results
			.Join(opsIds, sr => sr.Id, opsId => opsId.Key, (sr, opsId) => new { Id = Convert.ToInt32(opsId.Value.First().Value), Similarity = sr.Similarity })
			.Select(r => new ResultRecord() { Id = r.Id, Relevance = r.Similarity })
			.ToList();
	}

	[CSService(Service = "JSON.GetSearchResultAsCompounds", Description = "Returns search results by Request ID.")]
	public List<ChemSpider.OPS.ObjectModel.Compound> GetSearchResultAsCompounds(
		[CSParameter(PresenceType = ParameterPresenceType.Mandatory)] string rid,
		[CSParameter(PresenceType = ParameterPresenceType.Discretionary, DefaultValue = "0")] int start,
		[CSParameter(PresenceType = ParameterPresenceType.Discretionary, DefaultValue = "-1")] int count,
		[CSParameter(PresenceType = ParameterPresenceType.Discretionary)] string serfilter //   DO NOT REMOVE THIS FAKE PARAMETER!!! It's needed for help page generation!
		)
	{
		var guids = RSC.Search.RequestManager.Current.GetResults(Guid.Parse(rid), start, count).Cast<RSC.Compounds.Search.SearchResult>().Select(r => r.Id).ToList();

		var compounds = compoundStore.GetCompounds(guids);

		return compounds.Select(c => c.ToOPSCompound()).ToList();
	}
/*
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
*/
	[CSService(Service = "JSON.GetRecordsAsCompounds", Description = "Returns information about requested records specified as an array of CSIDs as array of Compound objects.")]
	public List<ChemSpider.OPS.ObjectModel.Compound> GetRecordsAsCompounds(
		[CSParameter(PresenceType = ParameterPresenceType.Mandatory)] int[] csids,
		[CSParameter(PresenceType = ParameterPresenceType.Discretionary)] string serfilter //   DO NOT REMOVE THIS FAKE PARAMETER!!! It's needed for help page generation!
		)
	{
		var dictionary = compoundConvert.ExternalReferencesToCompoundIds(csids.Select(id => id.ToString()), ChemSpider.OPS.ObjectModel.Compound.OPS_URI);

		var compounds = compoundStore.GetCompounds(dictionary.Select(d => d.Value));

		return compounds.Select(c => c.ToOPSCompound()).ToList();
	}
/*
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
*/
	[CSService(Service = "JSON.ConvertTo", Description = "Convert Name, SMILES, InChI or ChemSpider ID into structure ")]
	public ChemIdUtils.N2SResult ConvertTo([CSParameter(PresenceType = ParameterPresenceType.Mandatory)] ChemIdUtils.ConvertOptions convertOptions)
	{
		ChemIdUtils.N2SResult res = new ChemIdUtils.N2SResult
		{
			confidence = 0,
			message = string.Empty,
			mol = string.Empty
		};

		try
		{
			IEnumerable<Guid> compoundIds = null;

			switch (convertOptions.Direction)
			{
				case ChemIdUtils.ConvertOptions.EDirection.InChi2CSID:
					compoundIds = compoundConvert.InChIToCompoundId(convertOptions.Text);
					if (compoundIds.Any())
					{
						var opsIds = compoundStore.GetCompoundsExternalReferences(compoundIds, ChemSpider.OPS.ObjectModel.Compound.OPS_URI).Select(er => Convert.ToInt32(er.Value.First().Value)).ToList();

						res.mol = opsIds.FirstOrDefault().ToString();
						res.confidence = 100;
					}
					break;
				case ChemIdUtils.ConvertOptions.EDirection.InChiKey2CSID:
					compoundIds = compoundConvert.InChIKeyToCompoundId(convertOptions.Text);
					if (compoundIds.Any())
					{
						var opsIds = compoundStore.GetCompoundsExternalReferences(compoundIds, ChemSpider.OPS.ObjectModel.Compound.OPS_URI).Select(er => Convert.ToInt32(er.Value.First().Value)).ToList();

						res.mol = opsIds.FirstOrDefault().ToString();
						res.confidence = 100;
					}
					break;
				case ChemIdUtils.ConvertOptions.EDirection.SMILES2CSID:
					compoundIds = compoundConvert.SMILESToCompoundId(convertOptions.Text);
					if (compoundIds.Any())
					{
						var opsIds = compoundStore.GetCompoundsExternalReferences(compoundIds, ChemSpider.OPS.ObjectModel.Compound.OPS_URI).Select(er => Convert.ToInt32(er.Value.First().Value)).ToList();

						res.mol = opsIds.FirstOrDefault().ToString();
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
