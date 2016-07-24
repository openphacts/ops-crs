using System;
using System.Linq;
using System.Collections.Generic;
using RSC.Properties;

namespace RSC.Compounds
{
	public abstract class CompoundStore
	{
		/// <summary>
		/// Returns total number of compounds registered in the database
		/// </summary>
		/// <returns></returns>
		public abstract int GetCompoundsCount();

        /// <summary>
        /// Get all compounds
        /// </summary>
        /// <param name="start">ordinal start number of records</param>
        /// <param name="count">counts of records to return</param>
        /// <returns>compound ids</returns>
		public abstract IEnumerable<Guid> GetCompoundIds(int start = 0, int count = -1);

        /// <summary>
        /// Get Compound objects for list of Compound Id guids
        /// </summary>
        /// <param name="guids">Compound Ids</param>
        /// <param name="filter">Filtered data to return</param>
        /// <returns>The requested compounds</returns>
        public abstract IEnumerable<Compound> GetCompounds(IEnumerable<Guid> guids, IEnumerable<string> filter = null);

        /// <summary>
        /// Gets a list of Compound Id Guids for a particular Data source.
        /// </summary>
        /// <param name="dataSourceId">Data Source Id</param>
        /// <param name="start">ordinal start number of records</param>
        /// <param name="count">counts of records to return</param>
        /// <returns>Compound guids</returns>
        public abstract IEnumerable<Guid> GetDataSourceCompoundIds(Guid dataSourceId, int start = 0, int count = -1);

        /// <summary>
        /// Returns a Dictionary of Compound Ids and their Property Ids.
        /// </summary>
        /// <param name="guids">List of Compound Ids.</param>
        /// <returns>Dictionary of Compound Ids and their Property Ids.</returns>
	    public abstract IDictionary<Guid, IEnumerable<Guid>> GetCompoundsPropertiesIds(IEnumerable<Guid> guids);

		/// <summary>
		/// get compound instance by compound ID
		/// </summary>
        /// <param name="id">compound ID</param>
		/// <returns>compound instance</returns>
		public Compound GetCompound(Guid id)
		{
			return GetCompounds(new List<Guid>() { id }).FirstOrDefault();
		}

		/// <summary>
		/// Returns substance IDs
		/// </summary>
		/// <param name="id">compound ID</param>
		/// <returns>substance IDs</returns>
		public abstract IEnumerable<Guid> GetSubstanceIds(Guid id);

		/// /// <summary>
		/// get collection of ParentChild using compound ID
		/// </summary>
        /// <param name="id">compound ID</param>
		/// <returns>collection of ParentChild</returns>
		public abstract IEnumerable<ParentChild> GetParents(Guid id);
        
		/// <summary>
		/// get collection of ParentChild using compound ID
		/// </summary>
        /// <param name="id">compound ID</param>
		/// <returns>collection of ParentChild</returns>
		public abstract IEnumerable<ParentChild> GetChildren(Guid id);

		/// <summary>
		/// Returns total number of data sources registred in the system
		/// </summary>
		/// <param name="id">Compound ID</param>
		/// <returns>Total number of registred datasources</returns>
		public abstract int GetDatasourcesCount(Guid id);

		/// <summary>
		/// Returns list of data source GUIDs
		/// </summary>
		/// <param name="id">Compound ID</param>
		/// <param name="start">Index where to start returning datasources from</param>
		/// <param name="count">Number of returned datasources</param>
		/// <returns>List of GUIDs</returns>
		public abstract IEnumerable<Guid> GetDatasourceIds(Guid id, int start = 0, int count = 10);

		/// <summary>
		/// Returns compound's MOL file
		/// </summary>
		/// <param name="id">Compound ID</param>
		/// <returns>MOL file</returns>
		public abstract string GetMol(Guid id);

		/// <summary>
		/// Convert InChI to Compound ID
		/// </summary>
		/// <param name="inchi">Standard on Non standard InChI</param>
		/// <returns>Internal compound ID</returns>
		//public abstract IEnumerable<int> InChIToCompoundId(string inchi);

		/// <summary>
		/// Convert InChIKey to Compound ID
		/// </summary>
		/// <param name="inchikey">Standard on Non standard InChIKey</param>
		/// <returns>Internal compound ID</returns>
		//public abstract IEnumerable<int> InChIKeyToCompoundId(string inchikey);

        /// <summary>
        /// Get collection of Revision Ids for a given data source.
        /// </summary>
        /// <param name="dataSourceId">Data source id</param>
        /// <param name="start">ordinal start number of records</param>
        /// <param name="count">counts of records to return</param>
        /// <returns>list of Revision Id guids</returns>
        public abstract IEnumerable<Guid> GetDataSourceRevisionIds(Guid dataSourceId, int start = 0, int count = -1);

        /// <summary>
        /// Gets the revisions given a list of Revision Ids.
        /// </summary>
        /// <param name="revisionIds">The Ids of the revisions to retrieve.</param>
        /// <param name="filter">Filtered data to return (corresponds to the named navigation properties on ef_Revision)</param>
        /// <returns>An IEnumerable containing the requested revisions.</returns>
        public abstract IEnumerable<Revision> GetRevisions(IEnumerable<Guid> revisionIds, IEnumerable<string> filter = null);

        /// <summary>
        /// Retrieves a list of the Parent Child Relationships.
        /// </summary>
        /// <returns>list of the Parent Child Relationships</returns>
        public abstract IEnumerable<ParentChildRelationship> GetParentChildRelationships();

        /// <summary>
        /// Given a Dictionary of Properties and External Ids, returns the Value of a named Property.
        /// </summary>
        /// <param name="recordsProperties">Records Properties</param>
        /// <param name="extId">External Id</param>
        /// <param name="propertyName">Named property</param>
        /// <returns>The named property value</returns>
        public abstract dynamic GetRecordPropertyValue(IDictionary<RSC.ExternalId, IEnumerable<Property>> recordsProperties, RSC.ExternalId extId, string propertyName);

        ///TODO: Will refactor this so this method is not required - but would need to add Parents and Children to the Compound Core.
        /// <summary>
        /// Get the Parent Child compounds for a list of compound ids.
        /// </summary>
        /// <param name="compoundIds">list of compound ids</param>
        /// <param name="relationship">the relationship to return the parent children for</param>
        /// <returns>list of ParentChild</returns>
        public abstract IEnumerable<ParentChild> GetCompoundsParentChildren(IEnumerable<Guid> compoundIds, ParentChildRelationship relationship);

        public abstract IEnumerable<ExternalReferenceType> GetExternalIdTypes();
	}
}
