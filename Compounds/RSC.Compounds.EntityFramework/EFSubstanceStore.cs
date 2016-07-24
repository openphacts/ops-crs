using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.Extensions;
using RSC.Compounds.EntityFramework.Extensions;
using RSC.Logging;
using RSC.Properties;

namespace RSC.Compounds.EntityFramework
{
	public class EFSubstanceStore : SubstanceStore
	{
		private readonly CompoundsContext db = new CompoundsContext();

		private readonly IPropertyStore propertyStore = null;

		public EFSubstanceStore(IPropertyStore propertyStore)
		{
			if (propertyStore == null)
				throw new ArgumentNullException("propertyStore");

			this.propertyStore = propertyStore;
		}

		//saves records to Database in chunks
		private const int SAVE_RECORD_CHUNK_SIZE = 100;

		#region substance getters
		/// <summary>
		/// Returns number of substances registered in the system
		/// </summary>
		/// <returns>Number of total substances</returns>
		public override int GetSubstancesCount()
		{
			return db.Substances.Count();
		}

		/// <summary>
		/// get all substances 
		/// </summary>
		/// <param name="start"></param>
		/// <param name="count"></param>
		/// <returns>substance IDs</returns>
		public override IEnumerable<Guid> GetSubstances(int start = 0, int count = -1)
		{
			//	Empty request...
			if (count == 0)
				return new List<Guid>();

			IQueryable<ef_Substance> query = db.Substances.OrderBy(s => s.Id);

			if (start > 0)
				query = query.Skip(start);

			if (count > 0)
				query = query.Take(count);

			return query.Select(s => s.Id);
		}

		/// <summary>
		/// get substance instances by substance IDs
		/// </summary>
		/// <param name="guids">substance IDs</param>
		/// <param name="start"></param>
		/// <param name="count"></param>
		/// <returns>substance instances</returns>
		public override IEnumerable<RSC.Compounds.Substance> GetSubstances(IEnumerable<Guid> ids)
		{
			return db.Substances.Where(s => ids.Any(id => id == s.Id)).ToList().Select(s => new RSC.Compounds.Substance()
			{
				Id = s.Id,
				ExternalIdentifier = s.ExternalIdentifier,
				DataSourceId = s.DataSourceId
			});
		}

		/// <summary>
		/// Returns compound ID assigned to substance version
		/// </summary>
		/// <param name="id">substance ID</param>
		/// <param name="version">verison id</param>
		/// <returns>compound ID</returns>
		public override Guid GetCompound(Guid id, int? version = null)
		{
			if (version == null || !version.HasValue)
				return (from g in db.Revisions
						where g.Substance.Id == id
						select g)
					.GroupBy(c => c.Substance.Id)
					.Select(g => g.OrderByDescending(p => p.Version).FirstOrDefault())
					.Select(r => r.Compound.Id)
					.FirstOrDefault();
			else
				return (from g in db.Revisions
						where g.Substance.Id == id && g.Version == version
						select g)
					.Select(r => r.Compound.Id)
					.FirstOrDefault();
		}

		/// <summary>
		/// get substance revision by substance ID and version
		/// if version is null then return last revision
		/// </summary>
		/// <param name="guid">substance ID</param>
		/// <param name="version"></param>
		/// <returns>substance revision instance</returns>
		public override RSC.Compounds.Revision GetRevision(Guid id, int? version = null)
		{
			IEnumerable<ef_Revision> revisions = (from r in db.Revisions where r.Substance.Id == id orderby r.Version descending select r);

			ef_Revision revision;
			if (version == null || !version.HasValue)
				revision = revisions.FirstOrDefault();
			else revision = revisions.Where(r => r.Version == version.Value).FirstOrDefault();

			if (revision == null)
				return null;

			return revision.ToRevision();
		}

		/// <summary>
		/// Returns substance's SDF file
		/// </summary>
		/// <param name="id">Substance ID</param>
		/// <param name="version">Substance's version. If null - returns the latest version</param>
		/// <returns>SDF file</returns>
		public override string GetSDF(Guid id, int? version = null)
		{
			var revision = GetRevision(id, version);

			if (revision == null)
				return null;

			return revision.Sdf;
		}

		/// <summary>
		/// get annotations by substance id and version
		/// </summary>
		/// <param name="guid"></param>
		/// <param name="version"></param>
		/// <returns></returns>
		private IEnumerable<RSC.Compounds.Annotation> GetAnnotations(Guid id, int version)
		{
			ICollection<RSC.Compounds.Annotation> annotations = new List<RSC.Compounds.Annotation>();

			(from r in db.Annotations where r.Revision.Substance.Id == id select r)
			.Where(r => r.Revision.Version == version)
			.ToList()
			.ForEach(annotation => annotations.Add(new RSC.Compounds.Annotation()
			{
				Type = annotation.Type,
				Value = annotation.Value
			}));

			return annotations;
		}

		private IEnumerable<Issue> GetIssues(Guid id, int version)
		{
			ICollection<Issue> issues = new List<Issue>();

			//(from i in db.Issues where i.Revision.Substance.Id == id select i)
			//.Where(i => i.Revision.Version == version)
			//.ToList()
			//.ForEach(issue => issues.Add(new RSC.Issue()
			//{
			//	Severity = issue.Severity,
			//	Type = issue.Type,
			//	Code = (int)issue.Code,
			//	Description = issue.Description,
			//	Exception = issue.Exception,
			//	Message = issue.Message
			//}));

			return issues;
		}

		#endregion

		#region substance deletes
		/// <summary>
		/// Delete deposition and all compounds from the system
		/// </summary>
		/// <param name="guid">Deposition's guid</param>
		/// <returns>True if operation was successfull</returns>
		public override bool DeleteDeposition(Guid guid)
		{
			while (true)
			{
				//	remove Revisions first...
				var revisionsToRemove = db.Revisions.Where(r => r.DepositionId == guid).Take(1000);

				if (!revisionsToRemove.Any())
					break;

				//db.Revisions.RemoveRange(revisionsToRemove);
				revisionsToRemove.Delete();
				db.SaveChanges();

				//	find substances that do not belong any revision...
				db.Substances.Where(s => !db.Revisions.Select(r => r.SubstanceId).Contains(s.Id)).Delete();
				db.SaveChanges();

				//	find parent-child connections that do not belong to any revision...
				db.ParentChildren.Where(pc => !db.Revisions.Select(r => r.CompoundId).Contains(pc.ChildId)).Delete();
				db.SaveChanges();

				//	find compounds that do not belong any revision...
				var compoundsToDelete = db.Compounds.Where(c => !db.Revisions.Select(r => r.CompoundId).Contains(c.Id) && !db.ParentChildren.Select(pc => pc.ChildId).Contains(c.Id) && !db.ParentChildren.Select(pc => pc.ParentId).Contains(c.Id));

				//var compoundIds = compoundsToDelete.Select(c => c.Id).ToList();
				//propertyStore.ClearRecordProperties(compoundIds);

				//db.Compounds.RemoveRange(compoundsToDelete);
				compoundsToDelete.Delete();
				db.SaveChanges();

				//	find InChI and InChIMD5 that do not belong any compounds...
				db.InChI_MD5s.Where(i => !db.Compounds.Select(c => c.NonStandardInChIId).Contains(i.Id) && !db.Compounds.Select(c => c.StandardInChIId).Contains(i.Id) && !db.Compounds.Select(c => c.TautomericNonStdInChIId).Contains(i.Id)).Delete();
				db.SaveChanges();

				db.InChIs.Where(i => !db.Compounds.Select(c => c.NonStandardInChIId).Contains(i.Id) && !db.Compounds.Select(c => c.StandardInChIId).Contains(i.Id) && !db.Compounds.Select(c => c.TautomericNonStdInChIId).Contains(i.Id)).Delete();
				db.SaveChanges();

				//	find SMILES that do not belong any compound...
				db.Smiles.Where(s => !db.Compounds.Select(c => c.SmilesId).Contains(s.Id)).Delete();
				db.SaveChanges();
			}

			db.Substances.Where(s => !db.Revisions.Select(r => r.SubstanceId).Contains(s.Id)).Delete();
			db.ParentChildren.Where(pc => !db.Revisions.Select(r => r.CompoundId).Contains(pc.ChildId)).Delete();
			db.Compounds.Where(c => !db.Revisions.Select(r => r.CompoundId).Contains(c.Id) && !db.ParentChildren.Select(pc => pc.ChildId).Contains(c.Id) && !db.ParentChildren.Select(pc => pc.ParentId).Contains(c.Id));
			db.InChI_MD5s.Where(i => !db.Compounds.Select(c => c.NonStandardInChIId).Contains(i.Id) && !db.Compounds.Select(c => c.StandardInChIId).Contains(i.Id) && !db.Compounds.Select(c => c.TautomericNonStdInChIId).Contains(i.Id)).Delete();
			db.InChIs.Where(i => !db.Compounds.Select(c => c.NonStandardInChIId).Contains(i.Id) && !db.Compounds.Select(c => c.StandardInChIId).Contains(i.Id) && !db.Compounds.Select(c => c.TautomericNonStdInChIId).Contains(i.Id)).Delete();
			db.Smiles.Where(s => !db.Compounds.Select(c => c.SmilesId).Contains(s.Id)).Delete();
			db.SaveChanges();

			return true;
		}

		#endregion

	}
}
