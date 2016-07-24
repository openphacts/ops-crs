using EntityFramework.BulkInsert;
using EntityFramework.BulkInsert.Extensions;
using EntityFramework.BulkInsert.Providers;
using EntityFramework.Extensions;
using Newtonsoft.Json;
using RSC.Logging;
using RSC.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Linq.Dynamic;
using RSC.Properties;
using System.Diagnostics;

namespace RSC.CVSP.EntityFramework
{
	public static class BulkInsertExtension
	{
		public static void BulkInsert<T>(this DbContext context, IEnumerable<T> entities, System.Data.IDbTransaction transaction, BulkInsertOptions options)
		{
			IEfBulkInsertProvider bulkInsert = ProviderFactory.Get(context);
			bulkInsert.Run<T>(entities, transaction, options);
		}
	}

	public class EFCVSPStore : ICVSPStore
	{
		private readonly string connectionString;
		private readonly int timeout;

		public EFCVSPStore(string connectionString, int timeout)
		{
			this.connectionString = connectionString;
			this.timeout = timeout;
		}

		//saves records to Database in chunks
		private const int SAVE_RECORD_CHUNK_SIZE = 100;

		#region annotations methods

		/// <summary>
		/// get list all annotations registered in the system
		/// </summary>
		/// <returns>list of annotations</returns>
		public IEnumerable<RSC.CVSP.Annotation> GetAllAnnotations()
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				return db.Annotations.Select(a => new RSC.CVSP.Annotation()
				{
					Name = a.Name,
					Title = a.Title,
					IsRequired = a.IsRequired
				}).ToList();
			}
		}

		#endregion

		#region deposition

		public IEnumerable<Guid> GetDepositions(int start = 0, int count = -1)
		{
			//	Empty request...
			if (count == 0)
				return new List<Guid>();

			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				var query = from d in db.Depositions
							where d.Status != (int)DepositionStatus.ToDelete	//	exclude depositions scheduled to be deleted
							orderby d.Id
							select d.Guid;

				if (start > 0)
					query = query.Skip(start);

				if (count > 0)
					return query.Take(count);

				return query.ToList();
			}
		}

		public IEnumerable<Deposition> GetDepositions(IEnumerable<Guid> guids)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				return db.Depositions
					.AsNoTracking()
					.Include(d => d.Files)
					.Include(d => d.ProcessingParameters)
					.Where(d => guids.Contains(d.Guid))
					.ToList()
					.Select(d => d.ToDeposition())
					.ToList();
			}
		}

		/// <summary>
		/// takes deposition guid and returns a Deposition object
		/// </summary>
		/// <param name="guid">deposition guid</param>
		/// <returns>Deposition object</returns>
		public Deposition GetDeposition(Guid guid)
		{
			return GetDepositions(new List<Guid> { guid }).FirstOrDefault();
		}

		/// <summary>
		/// get list of registred depositions' GUIDs
		/// </summary>
		/// <param name="status">deposition status</param>
		/// <param name="start">ordinal start number of depositions</param>
		/// <param name="count">counts of depositions to return</param>
		/// <returns>list of depositions' GUIDs</returns>
		public IEnumerable<Guid> GetDepositionsByStatus(DepositionStatus status, int start = 0, int count = -1)
		{
			//	Empty request...
			if (count == 0)
				return new List<Guid>();

			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				var query = from ds in db.Depositions
							where ds.Status == (int)status
							orderby ds.Id
							select ds.Guid;

				if (start > 0)
					query = query.Skip(start);

				if (count > 0)
					return query.Take(count);

				return query.ToList();
			}
		}

		public Guid CreateDeposition(RSC.CVSP.Deposition deposition)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				if (!db.UserProfiles.Any())
					return Guid.Empty;

				var profile = db.UserProfiles.Where(p => p.Guid == deposition.UserId).First();
				if (profile == null)
					return Guid.Empty;

				//check that deposition Id is indeed empty
				if (deposition.Id != Guid.Empty)
					return Guid.Empty;

				ef_Deposition ef_deposition = new ef_Deposition()
				{
					Guid = Guid.NewGuid(),
					DateSubmitted = DateTime.Now,
					Status = (int)deposition.Status,
					DataDomain = (int)deposition.DataDomain,
					IsPublic = deposition.IsPublic,
					UserProfile = profile,
					DatasourceId = deposition.DatasourceId,
					ProcessingParameters = deposition.Parameters.Select(p => new ef_ProcessingParameter()
					{
						Name = p.Name,
						Value = p.Value
					}).ToList(),
					Files = deposition.DepositionFiles.Select(f => new ef_DepositionFile()
					{
						Guid = Guid.NewGuid(),
						Name = f.Name,
					}).ToList()
				};

				db.Depositions.Add(ef_deposition);
				return db.SaveChanges() > 0 ? ef_deposition.Guid : Guid.Empty;
			}
		}

		public bool UpdateDeposition(RSC.CVSP.Deposition deposition)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				var profile = db.UserProfiles.FirstOrDefault(p => p.Guid == deposition.UserId);
				if (profile == null)
					return false;

				ef_Deposition ef_deposition = db.Depositions.FirstOrDefault(d => d.Guid == deposition.Id);
				if (ef_deposition == null)
					return false;

				db.Depositions.Attach(ef_deposition);

				ef_deposition.DatasourceId = deposition.DatasourceId;
				ef_deposition.DateReprocessed = deposition.DateReprocessed;
				ef_deposition.Status = (int)deposition.Status;

				ef_deposition.IsPublic = deposition.IsPublic;

				var existingParameters = db.ProcessingParameters.Where(p => p.Deposition.Guid == deposition.Id).ToList();
				db.ProcessingParameters.RemoveRange(existingParameters);
				ef_deposition.ProcessingParameters = deposition.Parameters.Select(p => new ef_ProcessingParameter() { Name = p.Name, Value = p.Value }).ToList();

				return db.SaveChanges() > 0;
			}
		}

		/// <summary>
		/// updates deposition status
		/// </summary>
		/// <param name="guid">deposition guid</param>
		/// <param name="status">new deposition's status</param>
		/// <returns></returns>
		public bool UpdateDepositionStatus(Guid guid, DepositionStatus status)
		{
			var deposition = GetDeposition(guid);
			if (deposition != null)
			{
				deposition.Status = status;
				return UpdateDeposition(deposition);
			}

			return false;
		}

		public bool DeleteDeposition(Guid guid)
		{
			int REMOVE_RECORDS_CHUNK = 1000;

			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				//	check if deposition exists...
				if (!db.Depositions.Where(d => d.Guid == guid).Any())
					return false;

				//first delete records in chunks
				while (true)
				{
					var guids = GetDepositionRecords(guid, 0, REMOVE_RECORDS_CHUNK);

					if (!guids.Any())
						break;

					DeleteRecords(guids);
				}

				//delete deposition processing parameters
				//db.ProcessingParametersCollection.Remove(db.ProcessingParametersCollection.Where(p => p.Deposition.Guid == guid).First());

				//delete deposition sdf filed pams
				//db.SDFFieldMapCollection.RemoveRange(db.SDFFieldMapCollection.Where(s => s.Deposition.Guid == guid));

				//delete deposition files
				db.Files.RemoveRange(db.Files.Where(f => f.Deposition.Guid == guid));

				//delete deposition itself
				db.Depositions.Remove(db.Depositions.Where(d => d.Guid == guid).First());

				return db.SaveChanges() > 0;
			}
		}

		#endregion

		#region files
		/// <summary>
		/// Get list of deposition's files
		/// </summary>
		/// <param name="guid">deposition guid</param>
		/// <returns>List if deposition files</returns>
		public IEnumerable<RSC.CVSP.DepositionFile> GetDepositionFiles(Guid guid)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				ef_Deposition deposition = db.Depositions.Where(d => d.Guid == guid).FirstOrDefault();
				if (deposition == null)
					return new List<RSC.CVSP.DepositionFile>();

				return deposition.Files.Select(f => new RSC.CVSP.DepositionFile()
				{
					Id = f.Guid,
					Name = f.Name,
					Fields = f.Fields.Select(fld => new RSC.CVSP.Field()
					{
						Name = fld.Name,
						Annotaition = fld.Annotation == null ? null : new RSC.CVSP.Annotation()
						{
							Name = fld.Annotation.Name,
							Title = fld.Annotation.Title,
							IsRequired = fld.Annotation.IsRequired
						}
					}).ToList()
				}).ToList();
			}
		}

		private bool CleanRecords(CVSPContext db, Guid depositionGuid, IEnumerable<RSC.CVSP.Record> records)
		{
			int REMOVE_RECORDS_CHUNK = 1000;

			var files = records.Select(r => r.File).Distinct(new DepositionFileComparer()).ToList();

			if (files == null || files.Count() == 0)
				return true;

			foreach (var file in files)
			{
				while (true)
				{
					var ordinals = records.Where(r => r.File.Name == file.Name).Select(r => r.Ordinal).Take(REMOVE_RECORDS_CHUNK).ToList();
					var guids = db.Records.Where(r => r.Deposition.Guid == depositionGuid && r.File.Name == file.Name && ordinals.Contains(r.Ordinal)).Select(r => (Guid)r.ExternalId.ObjectId).ToList();

					if (!guids.Any())
						break;

					DeleteRecords(guids);
				}
			}

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="guid">Deposition guid</param>
		/// <param name="files">List of files</param>
		/// <returns></returns>
		private bool CreateFilesAndFields(CVSPContext db, Guid guid, IEnumerable<RSC.CVSP.Record> records)
		{
			ef_Deposition deposition = db.Depositions.AsNoTracking().Where(d => d.Guid == guid).FirstOrDefault();
			if (deposition == null)
				return false;

			var files = records.Select(r => r.File).Distinct(new DepositionFileComparer()).ToList();

			if (files == null || files.Count() == 0)
				return false;

			using (DbContextTransaction scope = db.Database.BeginTransaction())
			{
				//	Lock the table during this transaction in order to ristrict access from diffrent threads...
				db.Database.ExecuteSqlCommand("SELECT TOP 1 Id FROM DepositionFiles WITH (TABLOCKX, HOLDLOCK)");

				var nextFileId = db.Files.Any() ? db.Files.Max(i => i.Id) + 1 : 1;

				var newFiles = new List<ef_DepositionFile>();
				var newFields = new List<ef_Field>();

				foreach (var file in files)
				{
					var dbFile = db.Files.AsNoTracking().Where(f => f.Deposition.Guid == guid && f.Name.ToLower() == file.Name.ToLower()).FirstOrDefault();

					if (dbFile == null)
					{
						dbFile = new ef_DepositionFile()
						{
							Id = nextFileId++,
							Guid = Guid.NewGuid(),
							Name = file.Name,
							DepositionId = deposition.Id
						};

						newFiles.Add(dbFile);
					}

					var allFields = records.Where(r => r.File.Name == file.Name && r.Fields != null).SelectMany(r => r.Fields).Select(f => f.Name).Distinct().ToList();

					var existingFields = (from f in db.Fields.AsNoTracking()
										  where f.File.Deposition.Guid == guid && f.File.Name == file.Name && allFields.Contains(f.Name)
										  select f.Name).ToList();

					//	get list of Fields that should be created...
					var fieldsToCreate = allFields.Where(n => !existingFields.Any(k => k == n)).ToList();

					newFields.AddRange(fieldsToCreate.Select(f => new ef_Field() { FileId = dbFile.Id, Name = f }));
				}

				db.BulkInsert(newFiles, scope.UnderlyingTransaction, SqlBulkCopyOptions.KeepIdentity);
				db.BulkInsert(newFields, new BulkInsertOptions() { EnableStreaming = true });

				db.SaveChanges();

				scope.Commit();
			}

			return true;
		}

		#endregion

		#region fields
		/// <summary>
		/// Returns list of deposition's fields
		/// </summary>
		/// <param name="guid">deposition guid</param>
		/// <returns>List of fields</returns>
		public IEnumerable<RSC.CVSP.Field> GetDepositionFields(Guid guid)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				return db.Files.Where(f => f.Deposition.Guid == guid).SelectMany(f => f.Fields).Select(f => new RSC.CVSP.Field() { Name = f.Name }).ToList();
			}
		}
		#endregion

		#region annotations
		/// <summary>
		/// Returns list of deposition's annotated fields
		/// </summary>
		/// <param name="guid">deposition guid</param>
		/// <returns>List of annotated fields</returns>
		public IEnumerable<RSC.CVSP.Field> GetDepositionAnnotations(Guid guid)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				return db.Files.Where(f => f.Deposition.Guid == guid).SelectMany(f => f.Fields).Where(f => f.Annotation != null).Select(f => new RSC.CVSP.Field()
				{
					Name = f.Name,
					Annotaition = new RSC.CVSP.Annotation() { Name = f.Annotation.Name, Title = f.Annotation.Title, IsRequired = f.Annotation.IsRequired }
				}).ToList();
			}
		}

		/// <summary>
		/// Annotate deposition field
		/// </summary>
		/// <param name="guid">Deposition's GUID</param>
		/// <param name="field">Deposition's field name</param>
		/// <param name="annotation">Annotation's name</param>
		/// <returns></returns>
		public bool AnnotateDepositionField(Guid guid, string field, string annotation)
		{
			DeleteDepositionAnnotation(guid, annotation);

			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				var dbField = db.Fields.Where(f => f.File.Deposition.Guid == guid && f.Name == field).FirstOrDefault();

				if (dbField == null)
					return false;

				var dbAnnotation = db.Annotations.Where(a => a.Name == annotation).FirstOrDefault();

				if (dbAnnotation == null)
					return false;

				dbField.Annotation = dbAnnotation;

				return db.SaveChanges() > 0;
			}
		}

		/// <summary>
		/// Delete deposition's annotation
		/// </summary>
		/// <param name="guid">Deposition's GUID</param>
		/// <param name="annotation">Annotation's name</param>
		/// <returns>True if operation was successfull</returns>
		public bool DeleteDepositionAnnotation(Guid guid, string annotation)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				var dbField = db.Fields.Where(f => f.File.Deposition.Guid == guid && f.Annotation != null && f.Annotation.Name == annotation).FirstOrDefault();

				if (dbField == null)
					return false;

				db.Entry(dbField).Reference(f => f.Annotation).CurrentValue = null;

				return db.SaveChanges() > 0;
			}
		}
		#endregion

		#region record
		/// <summary>
		/// get records by record guids
		/// </summary>
		/// <param name="guids">record guids</param>
		/// <returns>IEnumerable Record</returns>
		public IEnumerable<RSC.CVSP.Record> GetRecords(IEnumerable<Guid> guids, IEnumerable<string> filter = null)
		{
			if (guids == null || !guids.Any())
				return null;

			if (filter == null)
				filter = new string[] { "original", "standardized", "dynamic", "issues", "properties", "fields", "file" };

			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				db.Configuration.AutoDetectChangesEnabled = false;

				var records = db.Records.AsQueryable()
					.Where(r => guids.Contains((Guid)r.ExternalId.ObjectId))
					.Where(r => r.DataDomain == (int)DataDomain.Substances)
					.Select(r => new
					{
						Id = r.ExternalId,
						Ordinal = r.Ordinal,
						Original = filter.Contains("original") ? r.Original : null,
						Standardized = filter.Contains("standardized") ? r.Standardized : null,
						SubmissionDate = r.SubmissionDate,
						RevisionDate = r.RevisionDate,
						DataDomain = (RSC.CVSP.DataDomain)r.DataDomain,
						DepositionId = r.Deposition.Guid,
						File = filter.Contains("file") ? new RSC.CVSP.DepositionFile()
						{
							Id = r.File.Guid,
							Name = r.File.Name,
						} : null,
						Dynamic = filter.Contains("dynamic") ? r.Dynamic : null
					})
					.ToList()
					.Select(r => new RSC.CVSP.Compounds.CompoundRecord()
						{
							Id = r.Id,
							Ordinal = r.Ordinal,
							Original = r.Original,
							Standardized = r.Standardized,
							SubmissionDate = r.SubmissionDate,
							RevisionDate = r.RevisionDate,
							DataDomain = (RSC.CVSP.DataDomain)r.DataDomain,
							DepositionId = r.DepositionId,
							File = r.File,
							Dynamic = r.Dynamic != null ? JsonConvert.DeserializeObject<ICollection<DynamicMember>>(r.Dynamic, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Formatting = Newtonsoft.Json.Formatting.Indented }) : new List<DynamicMember>()
						}).ToList();

				IDictionary<ExternalId, List<Issue>> issuesDictionary = null;
				if (filter.Has("issues"))
				{
					issuesDictionary = db.Issues
						.Include(i => i.Record)
						.Where(i => guids.Contains((Guid)i.Record.ExternalId.ObjectId))
						.Select(i => new
						{
							RecordId = i.Record.ExternalId,
							Issue = i
						})
						.ToList()
						.GroupBy(i => new { i.RecordId.DomainId, i.RecordId.ObjectId })
						.ToDictionary(g => new ExternalId() { DomainId = g.Key.DomainId, ObjectId = g.Key.ObjectId }, g => g.Select(i => i.Issue.ToIssue()).ToList());
				}

				IDictionary<ExternalId, List<Guid>> propertiesDictionary = null;
				if (filter.Has("properties"))
				{
					propertiesDictionary = db.Properties
						 .Include(p => p.Record)
						 .Where(p => guids.Contains((Guid)p.Record.ExternalId.ObjectId))
						 .Select(p => new
						 {
							 RecordId = p.Record.ExternalId,
							 PropertyId = p.PropertyId
						 })
						 .ToList()
						 .GroupBy(p => new { p.RecordId.DomainId, p.RecordId.ObjectId })
						 .ToDictionary(g => new ExternalId() { DomainId = g.Key.DomainId, ObjectId = g.Key.ObjectId }, g => g.Select(p => p.PropertyId).ToList());
				}

				IDictionary<ExternalId, List<RecordField>> fieldsDictionary = null;
				if (filter.Has("fields"))
				{
					fieldsDictionary = db.RecordFields
						 .Include(f => f.Record)
						 .Where(f => guids.Contains((Guid)f.Record.ExternalId.ObjectId))
						 .Select(f => new
						 {
							 RecordId = f.Record.ExternalId,
							 Field = f
						 })
						 .ToList()
						 .GroupBy(i => new { i.RecordId.DomainId, i.RecordId.ObjectId })
						 .ToDictionary(g => new ExternalId() { DomainId = g.Key.DomainId, ObjectId = g.Key.ObjectId }, g => g.Select(f => f.Field.ToField()).ToList());
				}

				IDictionary<Guid, List<Field>> fileFields = null;
				if (filter.Has("file"))
				{
					var fileGuids = records.Select(r => r.File.Id).ToList();

					fileFields = db.Fields
						 .Include(f => f.File)
						 .Include(f => f.Annotation)
						 .Where(f => fileGuids.Contains(f.File.Guid))
						 .Select(f => new
						 {
							 FileId = f.File.Guid,
							 Field = f
						 })
						 .ToList()
						 .GroupBy(f => f.FileId)
						 .ToDictionary(g => g.Key, g => g.Select(f => f.Field.ToField()).ToList());
				}

				foreach (var r in records)
				{
					if (issuesDictionary != null && issuesDictionary.ContainsKey(r.Id))
						r.Issues = issuesDictionary[r.Id];

					if (propertiesDictionary != null && propertiesDictionary.ContainsKey(r.Id))
						r.PropertyIDs = propertiesDictionary[r.Id];

					if (fieldsDictionary != null && fieldsDictionary.ContainsKey(r.Id))
						r.Fields = fieldsDictionary[r.Id];

					if (fileFields != null && fileFields.ContainsKey(r.File.Id))
						r.File.Fields = fileFields[r.File.Id];
				}

				return records.Cast<Record>().ToList();

				/*foreach (var r in query.ToList().OrderBy(r => r.Ordinal))
				{
					recordIds.Add((Guid)r.ExternalId.ObjectId, r.Id);

					if (r.DataDomain == (int)DataDomain.Substances)
					{
						var record = new RSC.CVSP.Compounds.CompoundRecord()
						{
							Id = r.ExternalId,
							Ordinal = r.Ordinal,
							Original = r.Original,
							Standardized = r.Standardized,
							SubmissionDate = r.SubmissionDate,
							RevisionDate = r.RevisionDate,
							DataDomain = (RSC.CVSP.DataDomain)r.DataDomain,
							DepositionId = r.Deposition.Guid,
							File = new RSC.CVSP.DepositionFile()
							{
								Name = r.File.Name,
								Fields = r.File.Fields.Select(f => new RSC.CVSP.Field()
								{
									Name = f.Name,
									Annotaition = f.Annotation == null ? null : new RSC.CVSP.Annotation()
									{
										Name = f.Annotation.Name,
										Title = f.Annotation.Title,
										IsRequired = f.Annotation.IsRequired
									}
								}).ToList()
							},
							Issues = r.Issues.Select(i => new RSC.Logging.Issue()
							{
								Code = i.Code,
								Id = i.LogId
							}).ToList(),
							Fields = r.Fields.Select(f => new RSC.CVSP.RecordField()
							{
								Name = f.Field.Name,
								Value = f.Value
							}).ToList(),
							PropertyIDs = r.Properties.Select(p => p.PropertyId).ToList(),
							Dynamic = JsonConvert.DeserializeObject<ICollection<DynamicMember>>(r.Dynamic, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Formatting = Newtonsoft.Json.Formatting.Indented })
						};

						results.Add(record);
					}
				}*/
				//}
			}
		}

		public Record GetRecord(Guid guid)
		{
			return GetRecords(new List<Guid> { guid }).FirstOrDefault();
		}

		/// <summary>
		/// returns number of records in deposition
		/// </summary>
		/// <param name="guid">deposition guid</param>
		/// <returns>number of records</returns>
		public int GetDepositionRecordsCount(Guid guid)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
				return (from d in db.Depositions
						join r in db.Records on d.Guid equals r.Deposition.Guid
						where d.Guid == guid
						select r).Count();
		}

		/// <summary>
		/// returns record guids by deposition guid, starting ordinal and count of records
		/// </summary>
		/// <param name="guid">deposition guid</param>
		/// <param name="start">ordinal start number of records</param>
		/// <param name="count">counts of records to return</param>
		/// <returns>record guids</returns>
		public IEnumerable<Guid> GetDepositionRecords(Guid guid, int start = 0, int count = -1)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				db.Configuration.AutoDetectChangesEnabled = false;

				var guids = from d in db.Depositions.AsNoTracking()
							join r in db.Records.AsNoTracking() on d.Guid equals r.Deposition.Guid
							where d.Guid == guid && (start == 0 || r.Ordinal >= start) && (count == -1 || r.Ordinal < start + count)
							orderby r.Ordinal
							select (Guid)r.ExternalId.ObjectId;

				//if (start > 0)
				//	guids = guids.Skip(start);
				//if (count >= 0)
				//	guids = guids.Take(count);

				return guids.ToList();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="depositionGuid">deposition guid</param>
		/// <param name="records">records</param>
		/// <returns>list of record guids</returns>
		public IEnumerable<Guid> CreateRecords(Guid depositionGuid, IEnumerable<RSC.CVSP.Record> records)
		{
			var guids = new List<Guid>();

			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				db.Configuration.AutoDetectChangesEnabled = false;
				db.Configuration.ValidateOnSaveEnabled = false;

				var deposition = db.Depositions.AsNoTracking().Where(d => d.Guid == depositionGuid).FirstOrDefault();
				if (deposition == null)
					return null;

				Stopwatch watch = new Stopwatch();
				watch.Start();
				Trace.TraceInformation("Clean records...");

				CleanRecords(db, depositionGuid, records);

				Trace.TraceInformation("Clean records... done: {0}", watch.Elapsed.ToString());
				watch.Restart();
				Trace.TraceInformation("Create files and fields...");

				CreateFilesAndFields(db, depositionGuid, records);

				Trace.TraceInformation("Create files and fields... done: {0}", watch.Elapsed.ToString());
				watch.Restart();
				Trace.TraceInformation("Create records...");

				var depositionFiles = db.Files.AsNoTracking().Where(f => f.Deposition.Guid == depositionGuid).ToList();
				var depositionFields = depositionFiles.ToDictionary(f => f.Id, f => f.Fields.ToDictionary(fld => fld.Name, fld => fld.Id));

				var newRecords = new List<ef_Record>();
				var recordIssues = new Dictionary<ExternalId, IEnumerable<Issue>>();
				var recordFields = new Dictionary<ExternalId, IEnumerable<ef_RecordField>>();
				var recordProperties = new Dictionary<ExternalId, IEnumerable<Guid>>();

				foreach (var r in records)
				{
					var depositionFile = depositionFiles.Single(f => f.Name.ToLower() == r.File.Name.ToLower());

					newRecords.Add(new ef_Record()
					{
						ExternalId = r.Id,
						DepositionId = deposition.Id,
						FileId = depositionFile.Id,
						Ordinal = r.Ordinal,
						SubmissionDate = DateTime.Now,
						DataDomain = (int)r.DataDomain,
						Original = r.Original,
						Standardized = r.Standardized,
						Dynamic = JsonConvert.SerializeObject(r.Dynamic, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Formatting = Newtonsoft.Json.Formatting.Indented })
					});

					if (r.Issues != null && r.Issues.Any())
					{
						recordIssues.Add(r.Id, r.Issues);
					}

					if (r.Fields != null && r.Fields.Any())
					{
						recordFields.Add(r.Id, r.Fields.Select(f => new ef_RecordField()
						{
							FieldId = depositionFields[depositionFile.Id][f.Name],
							Value = f.Value
						}).ToList());
					}

					if (r.Properties != null && r.Properties.Any())
					{
						recordProperties.Add(r.Id, r.Properties.Select(p => p.Id));
					}

					guids.Add((Guid)r.Id.ObjectId);
				}

				Trace.TraceInformation("Create records... done: {0}", watch.Elapsed.ToString());
				watch.Restart();
				Trace.TraceInformation("Upload records...");

				var bulkInsertOptions = new BulkInsertOptions() { EnableStreaming = true , TimeOut = (int)db.Database.CommandTimeout };

				using (DbContextTransaction scope = db.Database.BeginTransaction())
				{
					//	Lock the table during this transaction in order to ristrict access from diffrent threads...
					db.Database.ExecuteSqlCommand("SELECT TOP 1 Id FROM Records WITH (TABLOCKX, HOLDLOCK)");

					db.BulkInsert(newRecords, scope.UnderlyingTransaction, bulkInsertOptions);

					scope.Commit();
				}

				Trace.TraceInformation("Upload records... done: {0}", watch.Elapsed.ToString());
				watch.Restart();
				Trace.TraceInformation("Upload issues...");

				var newRecordIds = db.Records.AsNoTracking().Where(r => guids.Contains((Guid)r.ExternalId.ObjectId)).Select(r => new { ExtId = r.ExternalId, Id = r.Id }).ToDictionary(r => r.ExtId, r => r.Id);

				using (DbContextTransaction scope = db.Database.BeginTransaction())
				{
					db.Database.ExecuteSqlCommand("SELECT TOP 1 Id FROM Issues WITH (TABLOCKX, HOLDLOCK)");

					db.BulkInsert(recordIssues.ToDictionary(r => r.Key, r => r.Value.Select(i => new ef_Issue()
					{
						Code = i.Code,
						LogId = i.Id,
						RecordId = newRecordIds[r.Key]
					})).SelectMany(r => r.Value).ToList(), scope.UnderlyingTransaction, bulkInsertOptions);

					scope.Commit();
				}

				Trace.TraceInformation("Upload issues... done: {0}", watch.Elapsed.ToString());
				watch.Restart();
				Trace.TraceInformation("Upload fields...");

				using (DbContextTransaction scope = db.Database.BeginTransaction())
				{
					db.Database.ExecuteSqlCommand("SELECT TOP 1 Id FROM RecordFields WITH (TABLOCKX, HOLDLOCK)");

					db.BulkInsert(recordFields.ToDictionary(r => r.Key, r => r.Value.Select(f => new ef_RecordField()
					{
						FieldId = f.FieldId,
						RecordId = newRecordIds[r.Key],
						Value = f.Value
					})).SelectMany(r => r.Value).ToList(), scope.UnderlyingTransaction, bulkInsertOptions);

					scope.Commit();
				}

				Trace.TraceInformation("Upload fields... done: {0}", watch.Elapsed.ToString());
				watch.Restart();
				Trace.TraceInformation("Upload properties...");

				using (DbContextTransaction scope = db.Database.BeginTransaction())
				{
					db.Database.ExecuteSqlCommand("SELECT TOP 1 Id FROM Properties WITH (TABLOCKX, HOLDLOCK)");

					db.BulkInsert(recordProperties.ToDictionary(r => r.Key, r => r.Value.Select(guid => new ef_Property()
					{
						PropertyId = guid,
						RecordId = newRecordIds[r.Key],
					})).SelectMany(r => r.Value).ToList(), scope.UnderlyingTransaction, bulkInsertOptions);

					scope.Commit();
				}

				Trace.TraceInformation("Upload properties... done: {0}", watch.Elapsed.ToString());

				db.SaveChanges();
			}

			return guids;
		}

		public bool UpdateRecord(RSC.CVSP.Record record)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				var ef_record = db.Records.Where(r => r.ExternalId == record.Id).FirstOrDefault();
				if (ef_record == null)
					return false;

				//ef_record.DepositorRegid = record.ExternalId;
				ef_record.RevisionDate = DateTime.Now;
				ef_record.Original = record.Original;
				ef_record.Standardized = record.Standardized;

				return db.SaveChanges() > 0;
			}
		}

		public bool DeleteRecords(IEnumerable<Guid> guids)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				var recordsToDelete = db.Records.Where(r => guids.Contains((Guid)r.ExternalId.ObjectId));

				//	remove Records... 
				recordsToDelete.Delete();

				db.SaveChanges();

				return true;
			}
		}

		#endregion

		#region record issues
		/// <summary>
		/// Get Record issues
		/// </summary>
		/// <param name="guids">Record guids</param>
		/// <returns>Issues collection</returns>
		public IEnumerable<RSC.Logging.Issue> GetRecordIssues(Guid guid)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				return (from i in db.Issues
						where i.Record.ExternalId.ObjectId == guid
						select new RSC.Logging.Issue()
						{
							Code = i.Code,
							Id = i.LogId
						}).ToList();
			}
		}
		#endregion

		#region record fields
		/// <summary>
		/// Get Record fields
		/// </summary>
		/// <param name="guid">Record guid</param>
		/// <returns>Fields collection</returns>
		public IEnumerable<RSC.CVSP.RecordField> GetRecordFields(Guid guid)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
				return (from f in db.RecordFields
						where f.Record.ExternalId.ObjectId == guid
						select new RSC.CVSP.RecordField()
						{
							Name = f.Field.Name,
							Value = f.Value
						}).ToList();
		}
		#endregion

		#region record properties
		/// <summary>
		/// Get record properties
		/// </summary>
		/// <param name="guid">Record guid</param>
		/// <returns>Fields collection</returns>
		public IEnumerable<Guid> GetRecordProperties(Guid guid)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				return (from p in db.Properties
						where p.Record.ExternalId.ObjectId == guid
						select p.PropertyId).ToList();
			}
		}
		#endregion

		#region profile
		public RSC.CVSP.UserProfile GetUserProfile(Guid guid)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
				return (from u in db.UserProfiles
						where u.Guid == guid
						select new RSC.CVSP.UserProfile()
						{
							Id = u.Guid,
							SendEmail = u.SendEmail,
							FtpDirectory = u.FtpDirectory,
							Datasource = u.Datasource
						}).FirstOrDefault();
		}

		public Guid CreateUserProfile(RSC.CVSP.UserProfile profile)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				ef_UserProfile up = new ef_UserProfile()
				{
					Guid = profile.Id,
					SendEmail = profile.SendEmail,
					FtpDirectory = profile.FtpDirectory,
					Datasource = profile.Datasource
				};
				db.UserProfiles.Add(up);
				db.SaveChanges();
				return up.Guid;
			}
		}

		public bool DeleteUserProfile(Guid guid)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				ef_UserProfile up = db.UserProfiles.Where(p => p.Guid == guid).FirstOrDefault();
				if (up == null)
					return false;
				db.UserProfiles.Remove(up);
				return db.SaveChanges() > 0;
			}
		}

		public bool UpdateUserProfile(RSC.CVSP.UserProfile profile)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				ef_UserProfile ef_profle = db.UserProfiles.Where(u => u.Guid == profile.Id).FirstOrDefault();
				if (ef_profle == null)
					return false;

				ef_profle.FtpDirectory = profile.FtpDirectory;
				ef_profle.SendEmail = profile.SendEmail;
				ef_profle.Datasource = profile.Datasource;

				return db.SaveChanges() > 0;
			}
		}


		/// <summary>
		/// returns all user profiles 
		/// </summary>
		/// <returns></returns>
		[Obsolete("Very bad practice. This method should be removed")]
		public IEnumerable<RSC.CVSP.UserProfile> GetUserProfiles()
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
				return (from u in db.UserProfiles
						select new RSC.CVSP.UserProfile()
						{
							Id = u.Guid,
							SendEmail = u.SendEmail,
							FtpDirectory = u.FtpDirectory,
						}).ToList();
		}

		#endregion

		#region user variables
		public IEnumerable<RSC.CVSP.UserVariable> GetUserVariables(Guid guid)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
				return (from v in db.UserVariables
						where v.UserProfile.Guid == guid
						select new RSC.CVSP.UserVariable()
						{
							Name = v.Name,
							Value = v.Value,
							Description = v.Description
						}).ToList();
		}

		public bool UpdateUserVariables(Guid userGuid, IEnumerable<RSC.CVSP.UserVariable> list)
		{
			DeleteUserVariables(userGuid);
			CreateUserVariables(userGuid, list);
			return true;
		}

		public bool CreateUserVariables(Guid guid, IEnumerable<RSC.CVSP.UserVariable> list)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				ef_UserProfile up = db.UserProfiles.Where(u => u.Guid == guid).FirstOrDefault();
				if (up == null)
					return false;

				list.ToList().ForEach(uv => up.VariableCollection.Add(new ef_UserVariable()
				{
					Name = uv.Name,
					Value = uv.Value,
					Description = uv.Description
				}));

				return db.SaveChanges() > 0;
			}
		}

		public bool DeleteUserVariables(Guid userGuid)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				var ef_vars = db.UserVariables.Where(u => u.UserProfile.Guid == userGuid);
				db.UserVariables.RemoveRange(ef_vars);
				return db.SaveChanges() > 0;
			}
		}


		#endregion

		#region RuleSet
		public RSC.CVSP.RuleSet GetRuleSet(Guid guid)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
				return (from r in db.RuleSets
						where r.Guid == guid
						select new RSC.CVSP.RuleSet()
						 {
							 Id = r.Guid,
							 Type = (RuleType)r.RuleType,
							 DateCreated = r.DateCreated,
							 DateRevised = r.DateRevised,
							 Title = r.Title,
							 Description = r.Description,
							 RuleSetBody = r.Body,
							 IsPlatformDefault = r.IsDefault,
							 IsApproved = r.IsApproved,
							 IsPublic = r.IsPublic,
							 CountOfCloned = r.CountOfCloned,
							 Collaboraters = from c in r.Collaboraters select c.UserGuid,
							 UserGuid = r.UserProfile.Guid
						 }).FirstOrDefault();
		}

		public RSC.CVSP.RuleSet GetDefaultRuleSet(RuleType type)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				var ruleSet = db.RuleSets.Where(c => c.RuleType == (int)type).Where(c => c.IsDefault == true).FirstOrDefault();
				if (ruleSet == null)
					return null;
				return GetRuleSet(ruleSet.Guid);
			}
		}

		public bool UpdateRuleSet(Guid userGuid, RSC.CVSP.RuleSet content)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				var profile = db.UserProfiles.Where(u => u.Guid == userGuid).First();
				if (profile == null)
					return false;

				var ef_content = db.RuleSets.Where(c => c.Guid == content.Id).First();
				if (ef_content == null)
					return false;

				ef_content.IsApproved = content.IsApproved;
				ef_content.IsDefault = content.IsPlatformDefault;
				ef_content.IsPublic = content.IsPublic;
				ef_content.Title = content.Title;
				ef_content.UserProfile = profile;
				ef_content.DateRevised = DateTime.Now;
				ef_content.Description = content.Description;
				ef_content.CountOfCloned = content.CountOfCloned;
				ef_content.Body = content.RuleSetBody;

				ef_content.Collaboraters.Clear();
				ef_content.Collaboraters = content.Collaboraters.Select(c => new ef_Collaborator()
				{
					UserGuid = c
				}).ToList();

				return db.SaveChanges() > 0;
			}
		}

		public Guid CreateRuleSet(Guid userGuid, RSC.CVSP.RuleSet content)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				var profile = db.UserProfiles.Where(u => u.Guid == userGuid).First();
				if (profile == null || content.Type == RuleType.None)
					return Guid.Empty;

				ef_RuleSet ef_uc = new ef_RuleSet()
				{
					Guid = Guid.NewGuid(),
					RuleType = (int)content.Type,
					DateCreated = DateTime.Now,
					UserProfile = profile,
					Title = content.Title,
					Description = content.Description,
					Body = content.RuleSetBody,
					IsDefault = false,
					IsApproved = false,
					IsPublic = false,
					CountOfCloned = 0
				};

				db.RuleSets.Add(ef_uc);
				db.SaveChanges();
				return ef_uc.Guid;
			}
		}

		public bool DeleteRuleSet(Guid guid)
		{
			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				//first check if the content is being used in any deposition
				//var processingParameters = db.ProcessingParametersCollection.Where(d => d.ValidationContent.Guid == guid || d.AcidBaseContent.Guid == guid || d.StandardizationContent.Guid == guid);
				//if (processingParameters != null)
				//	return false;
				var content = db.RuleSets.Where(c => c.Guid == guid);
				if (content == null)
					return false;
				db.RuleSets.RemoveRange(content);
				return db.SaveChanges() > 0;
			}
		}
		#endregion

	}
}
