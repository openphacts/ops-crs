using RSC.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Data.Entity;
using System.Reflection;
using Microsoft.Ajax.Utilities;
using RSC.Compounds.EntityFramework.Extensions;
using RSC.Logging;

namespace RSC.Compounds.EntityFramework
{
	public partial class EFCompoundStore2 : ICompoundStore
	{
		public EFCompoundStore2()
		{
		}

		private CompoundsContext GetCompoundsContext()
		{
			var db = new CompoundsContext();

			db.Configuration.AutoDetectChangesEnabled = false;

			return db;
		}

		/// <summary>
		/// Get all compound Ids ordered by registration date 
		/// </summary>
		/// <param name="start">Ordinal start number of compounds</param>
		/// <param name="count">Counts of compounds to return</param>
		/// <returns>list of compound Ids</returns>
		public IEnumerable<Guid> GetCompounds(int start = 0, int count = -1)
		{
			using (var db = GetCompoundsContext())
			{
				//	Empty request...
				if (count == 0)
					return new List<Guid>();

				IQueryable<ef_Compound> query = db.Compounds.OrderBy(c => c.DateCreated);

				if (start > 0)
					query = query.Skip(start);

				if (count > 0)
					query = query.Take(count);

				return query.Select(c => c.Id).ToList();
			}
		}

		/// <summary>
		/// Get compound objects by the list of compound Ids
		/// </summary>
		/// <param name="compoundIds">Compound Ids</param>
		/// <param name="filter">Filtered data to return</param>
		/// <returns>The requested compounds</returns>
		public IEnumerable<Compound> GetCompounds(IEnumerable<Guid> compoundIds, IEnumerable<string> filter = null)
		{
			//Complex filters must be processed seperately.
			var complexFilters = new string[] { "CompoundSynonyms", "ExternalReferences" };

			using (var db = GetCompoundsContext())
			{
				db.Configuration.AutoDetectChangesEnabled = false;
				db.Configuration.LazyLoadingEnabled = false;

				var bFiltered = filter != null && filter.Any();
				var results = new List<Compound>();
				var query = db.Compounds.AsQueryable();

				//Filter by the list of Compound Ids.
				if (compoundIds != null)
					query = query.Where(c => compoundIds.Contains(c.Id));

				//Apply filters using extension method.
				var filteredQuery = query.ToFilteredIQueryable(filter, complexFilters);

				//Return only the filtered information.
				if (bFiltered)
				{
					foreach (var r in filteredQuery)
						results.Add(r.ToType<ef_Compound>().ToCompound());
				}
				else
					results.AddRange(filteredQuery
										.Cast<ef_Compound>()
										.ToList()
										.Select(c => c.ToCompound()));

				//If we must include CompoundSynonyms then add those to the records.
				if (!bFiltered || filter.Contains("CompoundSynonyms"))
				{
					var synonymsDictionary = db.CompoundSynonyms
						.Where(cs => compoundIds.Contains(cs.CompoundId))
						.Include(i => i.CompoundSynonymSynonymFlags)
						.Include(i => i.History)
						.Include(i => i.Synonym)
						.ToList()
						.GroupBy(i => i.CompoundId)
						.ToDictionary(g => g.Key, g => g.Select(c => c.ToSynonym()).ToList());

					foreach (var r in results.Where(r => synonymsDictionary.ContainsKey(r.Id)))
						r.Synonyms = synonymsDictionary[r.Id];
				}

				//If we must include ExternalIdentifiers then add those to the records.
				if (!bFiltered || filter.Contains("ExternalReferences"))
				{
					var externalIdentifiersDictionary = db.ExternalReferences
						.AsNoTracking()
						.Where(cs => compoundIds.Contains(cs.CompoundId))
						.Include(i => i.Type)
						.ToList()
						.GroupBy(i => i.CompoundId)
						.ToDictionary(g => g.Key, g => g.Select(c => c.ToExternalReference()).ToList());

					foreach (var r in results.Where(r => externalIdentifiersDictionary.ContainsKey(r.Id)))
						r.ExternalReferences = externalIdentifiersDictionary[r.Id];
				}

				return results;
			}
		}

		/// <summary>
		/// Returns a dictionary of compound Ids and their property Ids.
		/// </summary>
		/// <param name="guids">List of Compound Ids.</param>
		/// <returns>Dictionary of Compound Ids and their Property Ids.</returns>
		public IDictionary<Guid, IEnumerable<Guid>> GetCompoundsProperties(IEnumerable<Guid> guids)
		{
			using (var db = GetCompoundsContext())
			{
				return db.Revisions
						.AsNoTracking()
						.Where(c => guids.Contains(c.CompoundId))
						.Join(db.Properties, revision => revision.Id, property => property.RevisionId, (revision, property) => new { RevisionId = revision.Id, PropertyId = property.PropertyId, CompoundId = revision.CompoundId })
						.ToList()
						.GroupBy(p => new { p.CompoundId })
						.ToDictionary(g => g.Key.CompoundId, g => g.Select(p => p.PropertyId).ToList().AsEnumerable());
			}
		}

		/// <summary>
		/// Returns a dictionary of compound Ids and their issues
		/// </summary>
		/// <param name="compoundIds">List of compound Ids.</param>
		/// <returns>Dictionary of compound Ids and their issues</returns>
		public IDictionary<Guid, IEnumerable<Issue>> GetCompoundsIssues(IEnumerable<Guid> compoundIds)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns a dictionary of compound Ids and their synonyms
		/// </summary>
		/// <param name="compoundIds">List of compound Ids.</param>
		/// <returns>Dictionary of compound Ids and their synonyms</returns>
		public IDictionary<Guid, IEnumerable<Synonym>> GetCompoundsSynonyms(IEnumerable<Guid> compoundIds)
		{
			using (var db = GetCompoundsContext())
			{
				return db.CompoundSynonyms
					.Where(cs => compoundIds.Contains(cs.CompoundId))
					.Include(i => i.CompoundSynonymSynonymFlags)
					.Include(i => i.History)
					.Include(i => i.Synonym)
					.ToList()
					.GroupBy(i => i.CompoundId)
					.ToDictionary(g => g.Key, g => g.Select(c => c.ToSynonym()).ToList() as IEnumerable<Synonym>);
			}
		}

		/// <summary>
		/// Returns substance Ids
		/// </summary>
		/// <param name="compoundId">Compound ID</param>
		/// <param name="start">Ordinal start number of subtances</param>
		/// <param name="count">Counts of substances to return</param>
		/// <returns>List of substance Ids</returns>
		public IEnumerable<Guid> GetSubstances(Guid compoundId, int start = 0, int count = -1)
		{
			using (var db = GetCompoundsContext())
			{
				var query = db.Revisions.Where(sr => sr.CompoundId == compoundId)
					.Join(db.Substances, sr => sr.SubstanceId, s => s.Id, (sr, s) => new { sr.SubstanceId, sr.CompoundId, sr.DateCreated, sr.Version, s.DataSourceId })
					.OrderByDescending(s => new { s.SubstanceId, s.Version })
					.GroupBy(s => s.SubstanceId)
					.Select(g => new { SubstanceId = g.Key, CompoundId = g.FirstOrDefault().CompoundId, DateCreated = g.FirstOrDefault().DateCreated, DataSourceId = g.FirstOrDefault().DataSourceId })
					.OrderBy(s => s.DateCreated)
					.Select(s => s.SubstanceId)
					.Distinct();

				if (start > 0)
					query = query.Skip(start);

				if (count > 0)
					query = query.Take(count);

				return query.ToList();
			}
		}

		/// /// <summary>
		/// Get collection of ParentChild using compound Id
		/// </summary>
		/// <param name="id">Compound Id</param>
		/// <returns>Collection of ParentChild</returns>
		public IEnumerable<ParentChild> GetParents(Guid compoundId)
		{
			using (var db = new CompoundsContext())
			{
				db.Configuration.AutoDetectChangesEnabled = false;
				return db.ParentChildren
					.AsNoTracking()
					.Where(p => p.Child.Id == compoundId)
					.Select(pc => new ParentChild()
					{
						Type = pc.Type,
						ParentId = pc.ParentId,
						ChildId = pc.ChildId,
						//Parent = new Compound() { Id = pc.Parent.Id },
						//Child = new Compound() { Id = pc.Child.Id }
					})
					.ToList();
			}
		}

		/// <summary>
		/// Get collection of ParentChild using compound ID
		/// </summary>
		/// <param name="id">Compound ID</param>
		/// <returns>Collection of ParentChild</returns>
		public IEnumerable<ParentChild> GetChildren(Guid compoundId)
		{
			using (var db = new CompoundsContext())
			{
				db.Configuration.AutoDetectChangesEnabled = false;

				return db.ParentChildren
					.AsNoTracking()
					.Where(p => p.ParentId == compoundId)
					.Select(pc => new ParentChild()
					{
						Type = pc.Type,
						ParentId = pc.ParentId,
						ChildId = pc.ChildId,
						//Parent = new Compound() { Id = pc.Parent.Id },
						//Child = new Compound() { Id = pc.Child.Id }
					})
					.ToList();
			}
		}

		/// <summary>
		/// Returns list of data source Ids
		/// </summary>
		/// <param name="compoundId">Compound ID</param>
		/// <param name="start">Index where to start returning datasources from</param>
		/// <param name="count">Number of returned datasources</param>
		/// <returns>List of data source Ids</returns>
		public IEnumerable<Guid> GetDatasourceIds(Guid compoundId, int start = 0, int count = -1)
		{
			using (var db = GetCompoundsContext())
			{
				var query = db.Revisions.Where(sr => sr.CompoundId == compoundId)
					.Join(db.Substances, sr => sr.SubstanceId, s => s.Id, (sr, s) => new { sr.SubstanceId, sr.CompoundId, sr.DateCreated, sr.Version, s.DataSourceId })
					.OrderByDescending(s => new { s.SubstanceId, s.Version })
					.GroupBy(s => s.SubstanceId)
					.Select(g => new { SubstanceId = g.Key, CompoundId = g.FirstOrDefault().CompoundId, DateCreated = g.FirstOrDefault().DateCreated, DataSourceId = g.FirstOrDefault().DataSourceId })
					.OrderBy(s => s.DateCreated)
					.Select(s => s.DataSourceId)
					.Distinct();

				if (start > 0)
					query = query.Skip(start);

				if (count > 0)
					query = query.Take(count);

				return query.ToList();
			}
		}

		/// <summary>
		/// Returns list of all external reference types registered in the system
		/// </summary>
		/// <returns>List of external reference types</returns>
		public IEnumerable<ExternalReferenceType> GetExternalReferenceTypes()
		{
			using (var db = GetCompoundsContext())
			{
				return db.ExternalReferenceTypes
					.ToList()
					.Select(t => t.ToExternalReferenceType())
					.ToList();
			}
		}

		/// <summary>
		/// Returns list of compound external references
		/// </summary>
		/// <param name="compoundIds">List of compound Ids</param>
		/// <param name="uri">URI that should be included into results list</param>
		/// <returns>List of external references</returns>
		public IDictionary<Guid, IEnumerable<ExternalReference>> GetCompoundsExternalReferences(IEnumerable<Guid> compoundIds, string uri = null)
		{
			using (var db = GetCompoundsContext())
			{
				var query = db.ExternalReferences
					.Include(er => er.Type)
					.Where(er => compoundIds.Contains(er.CompoundId));

				if(!string.IsNullOrEmpty(uri))
					query = query.Where(er => er.Type.UriSpace == uri);

				return query
						.ToList()
						.GroupBy(i => i.CompoundId)
						.ToDictionary(g => g.Key, g => g.Select(er => er.ToExternalReference()).ToList().AsEnumerable());
			}
		}

		/// <summary>
		/// Checks if revisions with provided Ids exist and returns list of existing revision Ids.
		/// </summary>
		/// <param name="revisionIds">The Ids of the revisions to check.</param>
		/// <returns>List of existing revisions.</returns>
		public IEnumerable<Guid> GetExistingRevisions(IEnumerable<Guid> revisionIds)
		{
			using (var db = GetCompoundsContext())
			{
				return db.Revisions.AsNoTracking().Where(r => revisionIds.Contains(r.Id)).Select(r => r.Id).ToList();
			}
		}
	}
}