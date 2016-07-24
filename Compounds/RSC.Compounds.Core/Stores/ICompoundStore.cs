using System;
using System.Linq;
using System.Collections.Generic;
using RSC.Properties;
using RSC.Logging;

namespace RSC.Compounds
{
	public interface ICompoundStore
	{
		/// <summary>
		/// Get all compound Ids ordered by registration date 
		/// </summary>
		/// <param name="start">Ordinal start number of compounds</param>
		/// <param name="count">Counts of compounds to return</param>
		/// <returns>list of compound Ids</returns>
		IEnumerable<Guid> GetCompounds(int start = 0, int count = -1);

		/// <summary>
		/// Get compound objects by the list of compound Ids
		/// </summary>
		/// <param name="compoundIds">Compound Ids</param>
		/// <param name="filter">Filtered data to return</param>
		/// <returns>The requested compounds</returns>
		IEnumerable<Compound> GetCompounds(IEnumerable<Guid> compoundIds, IEnumerable<string> filter = null);

		/// <summary>
		/// Returns a dictionary of compound Ids and their property Ids
		/// </summary>
		/// <param name="compoundIds">List of compound Ids</param>
		/// <returns>Dictionary of Compound Ids and their Property Ids</returns>
		IDictionary<Guid, IEnumerable<Guid>> GetCompoundsProperties(IEnumerable<Guid> compoundIds);

		/// <summary>
		/// Returns a dictionary of compound Ids and their issues
		/// </summary>
		/// <param name="compoundIds">List of compound Ids.</param>
		/// <returns>Dictionary of compound Ids and their issues</returns>
		IDictionary<Guid, IEnumerable<Issue>> GetCompoundsIssues(IEnumerable<Guid> compoundIds);

		/// <summary>
		/// Returns a dictionary of compound Ids and their synonyms
		/// </summary>
		/// <param name="compoundIds">List of compound Ids.</param>
		/// <returns>Dictionary of compound Ids and their synonyms</returns>
		IDictionary<Guid, IEnumerable<Synonym>> GetCompoundsSynonyms(IEnumerable<Guid> compoundIds);

		/// <summary>
		/// Returns substance Ids
		/// </summary>
		/// <param name="compoundId">Compound ID</param>
		/// <param name="start">Ordinal start number of subtances</param>
		/// <param name="count">Counts of substances to return</param>
		/// <returns>List of substance Ids</returns>
		IEnumerable<Guid> GetSubstances(Guid compoundId, int start = 0, int count = -1);

		/// /// <summary>
		/// Get collection of ParentChild using compound Id
		/// </summary>
		/// <param name="id">Compound Id</param>
		/// <returns>Collection of ParentChild</returns>
		IEnumerable<ParentChild> GetParents(Guid compoundId);

		/// <summary>
		/// Get collection of ParentChild using compound ID
		/// </summary>
		/// <param name="id">Compound ID</param>
		/// <returns>Collection of ParentChild</returns>
		IEnumerable<ParentChild> GetChildren(Guid compoundId);

		/// <summary>
		/// Returns list of data source Ids
		/// </summary>
		/// <param name="compoundId">Compound Id</param>
		/// <param name="start">Index where to start returning datasources from</param>
		/// <param name="count">Number of returned datasources</param>
		/// <returns>List of data source Ids</returns>
		IEnumerable<Guid> GetDatasourceIds(Guid compoundId, int start = 0, int count = -1);

		/// <summary>
		/// Returns list of all external reference types registered in the system
		/// </summary>
		/// <returns>List of external reference types</returns>
		IEnumerable<ExternalReferenceType> GetExternalReferenceTypes();

		/// <summary>
		/// Returns list of compound external references
		/// </summary>
		/// <param name="compoundIds">List of compound Ids</param>
		/// <param name="uri">URI that should be included into results list</param>
		/// <returns>List of external references</returns>
		IDictionary<Guid, IEnumerable<ExternalReference>> GetCompoundsExternalReferences(IEnumerable<Guid> compoundIds, string uri = null);

		/// <summary>
		/// Checks if revisions with provided Ids exist and returns list of existing revision Ids.
		/// </summary>
		/// <param name="revisionIds">The Ids of the revisions to check.</param>
		/// <returns>List of existing revisions.</returns>
		IEnumerable<Guid> GetExistingRevisions(IEnumerable<Guid> revisionIds);
	}
}
