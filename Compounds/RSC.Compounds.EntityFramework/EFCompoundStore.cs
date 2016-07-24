using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Data.Entity;
using RSC.Compounds.EntityFramework.Extensions;
using RSC.Properties;

namespace RSC.Compounds.EntityFramework
{
	public class EFCompoundStore : CompoundStore
	{
		private readonly IPropertyStore _propertyStore;

		public EFCompoundStore(IPropertyStore propertyStore)
		{
			if (propertyStore == null)
				throw new ArgumentNullException("propertyStore");

			_propertyStore = propertyStore;
		}

		/// <summary>
		/// Returns total number of compounds registered in the database
		/// </summary>
		/// <returns>Compound count</returns>
		public override int GetCompoundsCount()
		{
			using (var db = new CompoundsContext())
			{
				db.Configuration.AutoDetectChangesEnabled = false;
				return db.Compounds.Count();
			}
		}

		/// <summary>
		/// Get all compounds
		/// </summary>
		/// <param name="start">ordinal start number of records</param>
		/// <param name="count">counts of records to return</param>
		/// <returns>compound ids</returns>
		public override IEnumerable<Guid> GetCompoundIds(int start = 0, int count = -1)
		{
			using (var db = new CompoundsContext())
			{
				db.Configuration.AutoDetectChangesEnabled = false;

				if (count == 0)
					return new List<Guid>();

				var query = db.Compounds
					.AsNoTracking()
					.AsQueryable();

				if ( start > 0 || count > 0 )
					query = query.OrderBy(c => c.DateCreated);

				var query2 = query.Select(c => c.Id);

				if (start > 0)
					query2 = query2.Skip(start);

				if (count > 0)
					query2 = query2.Take(count);

				return query2.ToList();
			}
		}

		/// <summary>
		/// Get Compound objects for list of Compound Id guids
		/// </summary>
		/// <param name="compoundIds">Compound Ids</param>
		/// <param name="filter">Filtered data to return (corresponds to the named navigation properties and properties on ef_Compound)</param>
		/// <returns>The requested compounds</returns>
		public override IEnumerable<Compound> GetCompounds(IEnumerable<Guid> compoundIds, IEnumerable<string> filter = null)
		{
			//Complex filters must be processed seperately.
			var complexFilters = new string[] { "CompoundSynonyms", "ExternalReferences" };

			using ( var db = new CompoundsContext() ) {
				db.Configuration.AutoDetectChangesEnabled = false;
				db.Configuration.LazyLoadingEnabled = false;

				//Testing purposes.
				//db.Database.Log = s => { System.Diagnostics.Debug.WriteLine(s); };

				var bFiltered = filter != null && filter.Any();
				var results = new List<Compound>();
				var query = db.Compounds.AsQueryable();

				//Filter by the list of Compound Ids.
				if ( compoundIds != null )
					query = query.Where(c => compoundIds.Contains(c.Id));

				//Apply filters using extension method.
				var filteredQuery = query
					.ToFilteredIQueryable(filter, complexFilters)
					.AsNoTracking();

				//Return only the filtered information.
				if ( bFiltered ) {
					foreach ( var r in filteredQuery )
						results.Add(r.ToType<ef_Compound>().ToCompound());
				}
				else {
					results.AddRange(filteredQuery
										.Cast<ef_Compound>()
										.ToList()
										.Select(c => c.ToCompound()));
				}

				//If we must include CompoundSynonyms then add those to the records.
				if ( !bFiltered || filter.Contains("CompoundSynonyms") ) {
					var syn = db.CompoundSynonyms
						.AsNoTracking()
						.Where(cs => compoundIds.Contains(cs.CompoundId))
						.Include(i => i.CompoundSynonymSynonymFlags)
						.Include(i => i.History)
						.Include(i => i.Synonym);
					var syn1 = syn
						.ToList()
						.GroupBy(i => i.CompoundId);
					var syn2 = syn1
						.ToDictionary(g => g.Key, g => g.Select(c => c.ToSynonym()).ToList());

					foreach ( var r in results.Where(r => syn2.ContainsKey(r.Id)) )
						r.Synonyms = syn2[r.Id];
				}

				//If we must include ExternalIdentifiers then add those to the records.
				if ( !bFiltered || filter.Contains("ExternalReferences") ) {
					var ext = db.ExternalReferences
						.AsNoTracking()
						.Where(cs => compoundIds.Contains(cs.CompoundId))
						.Include(i => i.Type);
					var ext1 = ext
						.ToList()
						.GroupBy(i => i.CompoundId);
					var ext2 = ext1
						.ToDictionary(g => g.Key, g => g.Select(c => c.ToExternalReference()).ToList());

					foreach ( var r in results.Where(r => ext2.ContainsKey(r.Id)) )
						r.ExternalReferences = ext2[r.Id];
				}

				return results;
			}
		}

		/// <summary>
		/// Gets a list of Compound Id Guids for a particular Data source.
		/// </summary>
		/// <param name="dataSourceId">Data Source Id</param>
		/// <param name="start">ordinal start number of records</param>
		/// <param name="count">counts of records to return</param>
		/// <returns>Compound guids</returns>
		public override IEnumerable<Guid> GetDataSourceCompoundIds(Guid dataSourceId, int start = 0, int count = -1)
		{
			using (var db = new CompoundsContext())
			{
				db.Configuration.AutoDetectChangesEnabled = false;

				var guids = db.Revisions
					.AsNoTracking()
					.Where(r => r.Substance.DataSourceId == dataSourceId)
					.Select(r => r.CompoundId)
					.Distinct();

				if ( start > 0 || count > 0 )
					guids = guids.OrderBy(g => g);

				if (start > 0)
					guids = guids.Skip(start);

				if (count >= 0)
					guids = guids.Take(count);

				return guids.ToList();
			}
		}

		/// <summary>
		/// Returns substance IDs
		/// </summary>
		/// <param name="cmpId">compound ID</param>
		/// <returns>substance IDs</returns>
		public override IEnumerable<Guid> GetSubstanceIds(Guid cmpId)
		{
			using (var db = new CompoundsContext())
			{
				db.Configuration.AutoDetectChangesEnabled = false;
				return db.Revisions
					.AsNoTracking()
					.Where(r => r.CompoundId == cmpId)
					.Select(r => r.SubstanceId)
					.ToList();
			}
		}

		/// <summary>
		/// Get a list of Parents for a Compound.
		/// </summary>
		/// <param name="id">The id of the record.</param>
		/// <returns>A list of ParentChild Parents.</returns>
		public override IEnumerable<ParentChild> GetParents(Guid id)
		{
			using (var db = new CompoundsContext())
			{
				db.Configuration.AutoDetectChangesEnabled = false;
				return db.ParentChildren
					.AsNoTracking()
					.Where(p => p.ChildId == id)
					.Select(pc => new ParentChild()
					{
						Type = pc.Type,
						Parent = new Compound() { Id = pc.ParentId },
						Child = new Compound() { Id = pc.ChildId }
					})
					.ToList();
			}
		}

		/// <summary>
		/// Get a list of Children for a Compound.
		/// </summary>
		/// <param name="id">The id of the record.</param>
		/// <returns>A list of ParentChild Children.</returns>
		public override IEnumerable<ParentChild> GetChildren(Guid id)
		{
			using (var db = new CompoundsContext())
			{
				db.Configuration.AutoDetectChangesEnabled = false;

				return db.ParentChildren
					.AsNoTracking()
					.Where(p => p.ParentId == id)
					.Select(pc => new ParentChild()
					{
						Type = pc.Type,
						Parent = new Compound() { Id = pc.ParentId },
						Child = new Compound() { Id = pc.ChildId }
					})
					.ToList();
			}
		}

		/// <summary>
		/// Retrieves a Dictionary of Property Ids for a list of Compound Ids.
		/// </summary>
		/// <param name="guids">List of Compound Id guids.</param>
		/// <returns>The Dictionary of Compound Ids and Property Ids.</returns>
		public override IDictionary<Guid, IEnumerable<Guid>> GetCompoundsPropertiesIds(IEnumerable<Guid> guids)
		{
			using (var db = new CompoundsContext())
			{
				db.Configuration.AutoDetectChangesEnabled = false;

				var query =	db.Properties
					.AsNoTracking()
					.AsQueryable();

				//Filter by the list of Compound Ids.
				if (guids != null)
					query = query
						.Where(p => guids.Contains(p.Revision.CompoundId));

				// TODO: not finished
				return query
					.GroupBy(p => p.Revision.CompoundId)
					.ToDictionary(g => g.Key, g => g.Select(p => p.PropertyId));
			}
		}

		/// <summary>
		/// Returns total number of data sources registred in the system
		/// </summary>
		/// <param name="id">Compound ID</param>
		/// <returns>Total number of registred datasources</returns>
		public override int GetDatasourcesCount(Guid id)
		{
			using (var db = new CompoundsContext())
			{
				db.Configuration.AutoDetectChangesEnabled = false;

				return db.Revisions
					.AsNoTracking()
					.Where(r => r.CompoundId == id)
					.Select(r => r.Substance.DataSourceId)
					.Distinct()
					.Count();
			}
		}

		/// <summary>
		/// Returns list of data sources GUIDs
		/// </summary>
		/// <param name="cmpId">Compound ID</param>
		/// <param name="start">Index where to start returning datasources from</param>
		/// <param name="count">Number of returned datasources</param>
		/// <returns>List of GUIDs</returns>
		public override IEnumerable<Guid> GetDatasourceIds(Guid cmpId, int start = 0, int count = 10)
		{
			using (var db = new CompoundsContext())
			{
				db.Configuration.AutoDetectChangesEnabled = false;

				if (count == 0)
					return new List<Guid>();

				var query = db.Revisions
					.AsNoTracking()
					.Where(r => r.CompoundId == cmpId)
					.GroupBy(c => c.Substance.Id)
					.Select(g => g.OrderByDescending(p => p.Version).FirstOrDefault())
					.Select(r => r.Substance.DataSourceId)
					.Distinct();

				if ( start > 0 || count > 0 )
					query = query.OrderBy(g => g);

				if (start > 0)
					query = query.Skip(start);

				if (count > 0)
					query = query.Take(count);

				return query.ToList();
			}
		}

		/// <summary>
		/// Returns compound's MOL file
		/// </summary>
		/// <param name="id">Compound ID</param>
		/// <returns>MOL file</returns>
		public override string GetMol(Guid id)
		{
			var compound = GetCompound(id);
			return compound == null ? null : compound.Mol;
		}

		/// <summary>
		/// Convert InChI to Compound ID
		/// </summary>
		/// <param name="inchi">Standard on Non standard InChI</param>
		/// <returns>Internal compound ID</returns>
		//public override IEnumerable<int> InChIToCompoundId(string inchi)
		//{
		//	throw new NotImplementedException();
		//}

		/// <summary>
		/// Convert InChIKey to Compound ID
		/// </summary>
		/// <param name="inchikey">Standard on Non standard InChIKey</param>
		/// <returns>Internal compound ID</returns>
		//public override IEnumerable<int> InChIKeyToCompoundId(string inchikey)
		//{
		//	throw new NotImplementedException();
		//}

		/// <summary>
		/// Returns the Revision Ids associated with a Data Source.
		/// </summary>
		/// <param name="dsnId">Data Source Id</param>
		/// <param name="start">Start index</param>
		/// <param name="count">Size of the chunk</param>
		/// <returns>An enumerable of Revision Id guids.</returns>
		public override IEnumerable<Guid> GetDataSourceRevisionIds(Guid dsnId, int start = 0, int count = -1)
		{
			using (var db = new CompoundsContext())
			{
				db.Configuration.AutoDetectChangesEnabled = false;

				//Testing purposes.
				//db.Database.Log = s => { System.Diagnostics.Debug.WriteLine(s); };

				if (count == 0)
					return new List<Guid>();

				var query = db.Revisions
					.AsNoTracking()
					.Where(r => r.Substance.DataSourceId == dsnId && r.Version == db.Revisions.Where(rr => rr.SubstanceId == r.SubstanceId).Select(rr => rr.Version).Max())
					.Select(r => r.Id);

				if (start > 0)
					query = query.Skip(start);

				if (count > 0)
					query = query.Take(count);

				return query.ToList();
			}
		}

		/// <summary>
		/// Gets the revisions given a list of Revision Ids.
		/// </summary>
		/// <param name="revIds">The Ids of the revisions to retrieve.</param>
		/// <param name="filter">Filtered data to return (corresponds to the named navigation properties on ef_Revision)</param>
		/// <returns>An IEnumerable containing the requested revisions.</returns>
		public override IEnumerable<Revision> GetRevisions(IEnumerable<Guid> revIds, IEnumerable<string> filter = null)
		{
			using (var db = new CompoundsContext())
			{
				db.Configuration.AutoDetectChangesEnabled = false;
				db.Configuration.LazyLoadingEnabled = false;

				//Testing purposes.
				//db.Database.Log = s => { System.Diagnostics.Debug.WriteLine(s); };

				var bFiltered = filter != null && filter.Any();
				var results = new List<Revision>();
				var query = db.Revisions.AsQueryable();

				//Filter by the list of Revision Ids.
				if (revIds != null)
					query = query.Where(r => revIds.Contains(r.Id));

				//Apply filters using extension method.
				var filteredQuery = query.ToFilteredIQueryable(filter)
										 .AsNoTracking();

				//Return only the filtered information.
				if (bFiltered)
				{
					foreach (var r in filteredQuery)
					{
						var record = r.ToType<ef_Revision>();
						results.Add(record.ToRevision());
					}
				}
				else
					results.AddRange(filteredQuery
										.Cast<ef_Revision>()
										.ToList()
										.Select(c => c.ToRevision()));

				return results;
			}
		}

		/// <summary>
		/// Get the ParentChildren for a given list of Compound Ids and given ParentChildRelationship.
		/// </summary>
		/// <param name="compoundIds">list of compound ids</param>
		/// <param name="relationship">the parent child relationship</param>
		/// <returns>the list of ParentChildren</returns>
		public override IEnumerable<ParentChild> GetCompoundsParentChildren(IEnumerable<Guid> compoundIds, ParentChildRelationship relationship)
		{
			using (var db = new CompoundsContext())
			{
				db.Configuration.AutoDetectChangesEnabled = false;

				var parentChildren = new List<ParentChild>();

				//Add the parent children to the collection.
				parentChildren.AddRange(db.ParentChildren
					.AsNoTracking()
					.Where(
					pc =>
						pc.Type == relationship &&
						(compoundIds.Contains(pc.ChildId) || compoundIds.Contains(pc.ParentId)))
					.Select(pc => new ParentChild
					{
						Type = pc.Type,
						Parent = new Compound() { Id = pc.Parent.Id },
						Child = new Compound() { Id = pc.Child.Id }
					}));

				//Dedupe them here as many compounds can have the same parents/children.
				return parentChildren
					.GroupBy(p => new { p.Child, p.Parent })
					.Select(g => g.First())
					.ToList();
			}
		}

		/// <summary>
		/// Returns a list of the ParentChildRelationships
		/// </summary>
		/// <returns>A list of the ParentChildRelationships.</returns>
		public override IEnumerable<ParentChildRelationship> GetParentChildRelationships()
		{
			return new List<ParentChildRelationship>()
			{
				ParentChildRelationship.ChargeInsensitive,
				ParentChildRelationship.Fragment,
				ParentChildRelationship.IsotopInsensitive,
				ParentChildRelationship.StereoInsensitive,
				ParentChildRelationship.SuperInsensitive,
				ParentChildRelationship.TautomerInsensitive
			};
		}

		/// <summary>
		/// Given a Dictionary of Properties and External Ids, returns the Value of a named Property.
		/// </summary>
		/// <param name="recordsProperties">Records Properties</param>
		/// <param name="extId">External Id</param>
		/// <param name="propertyName">Named property</param>
		/// <returns>The named property value</returns>
		public override dynamic GetRecordPropertyValue(IDictionary<ExternalId, IEnumerable<Property>> recordsProperties, ExternalId extId, string propertyName)
		{
			if (recordsProperties.ContainsKey(extId))
			{
				if (recordsProperties[extId].Any(p => p.Name == propertyName))
					return recordsProperties[extId].FirstOrDefault(p => p.Name == propertyName).Value;
			}
			return null;
		}

		/// <summary>
		/// Returns a list of ExternalReferenceTypes.
		/// </summary>
		/// <returns>A list of ExternalReferenceTypes.</returns>
		public override IEnumerable<ExternalReferenceType> GetExternalIdTypes()
		{
			using (var db = new CompoundsContext())
			{
				db.Configuration.AutoDetectChangesEnabled = false;

				return db.ExternalReferenceTypes
					.Select(e => new ExternalReferenceType()
					{
						Description = e.Description,
						//Id = e.Id,
						UriSpace = e.UriSpace
					})
					.ToList();
			}
		}
	}

	//*******************************************************************************************
	//TODO: This should be moved somewhere global E.g. RSC Platform.
	//*******************************************************************************************

	/// <summary>
	/// Extension method for mapping a list of filters into an efficient SQL statement - only the requested data is returned.
	/// </summary>
	public static class IQueryableExtensions
	{
		/// <summary>
		/// Filters a generic IQueryable EF query given a supplied set of filters and complex filters.
		/// </summary>
		/// <typeparam name="T">Generic EF type</typeparam>
		/// <param name="query">Existing query (including any where clauses already applied)</param>
		/// <param name="filter">List of properties and navigation properties to include in the query.</param>
		/// <param name="complexFilters">List of properties which are more than 1 table join, they will be excluded from the request and must be dealt with afterwards.</param>
		/// <returns>The filtered IQueryable query</returns>
		public static IQueryable ToFilteredIQueryable<T>(this IQueryable<T> query, IEnumerable<string> filter = null, string[] complexFilters = null)
		{
			var bFiltered = filter != null && filter.Any();
			complexFilters = complexFilters ?? new string[] { };

			//Add the Includes for navigation properties specified in the filter.
			foreach ( var prop in typeof(T).GetProperties().Where(p => p.GetMethod.IsVirtual).ToArray() ) {
				if ( ( !bFiltered || filter.Contains(prop.Name, StringComparer.OrdinalIgnoreCase) ) && !complexFilters.Contains(prop.Name) ) {
					query = query.Include(prop.Name);
				}
			}

			return bFiltered
				//Use Dynamic linq to select the data specified in the filter.
				? query.Select(string.Format("new({0})", string.Join(",", filter.Where(f => !complexFilters.Contains(f)))))
				//Select all the data.
				: query;
		}
	}

}
