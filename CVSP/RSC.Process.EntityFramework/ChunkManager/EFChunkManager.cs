using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;

using RSC.Compression;
using Newtonsoft.Json;

namespace RSC.Process.EntityFramework
{
	public class EFChunkManager : IChunkManager
	{
		//private ChunkManagerContext db = new ChunkManagerContext();

		public EFChunkManager()
		{
		}

		/// <summary>
		/// Check if chunk with Id exists
		/// </summary>
		/// <param name="id">Chunk id</param>
		/// <returns>True is chunk exists</returns>
		public bool HasChunk(Guid id)
		{
			using (var db = new ChunkManagerContext())
			{
				return db.Chunks.Any(c => c.ExternalId == id);
			}
		}
		/// <summary>
		/// Create new chunk
		/// </summary>
		/// <param name="deposition">Deposition guid</param>
		/// <returns>New chunk guid</returns>
		public Guid CreateChunk(Chunk chunk, IEnumerable<object> records)
		{
			chunk.Id = Guid.NewGuid();
			chunk.Status = ChunkStatus.Created;
			chunk.NumberOfRecords = records.Count();

			using (var db = new ChunkManagerContext())
			{
				db.Chunks.Add(new RSC.Process.ChunkManager.EntityFramework.Chunk()
				{
					ExternalId = chunk.Id,
					Status = chunk.Status,
					NumberOfRecords = chunk.NumberOfRecords,
					Parameters = chunk.Parameters.Select(p => new RSC.Process.ChunkManager.EntityFramework.Parameter()
					{
						Name = p.Name,
						Value = p.Value,
						//ChunkId = chunk.Id,
					}).ToList(),
					Blob = new ChunkManager.EntityFramework.Blob()
					{
						//Id = chunk.Id,
						Data = Records2Bytes(records)
					}
				});

				db.SaveChanges();
			}

			return chunk.Id;
		}

		private byte[] Records2Bytes(IEnumerable<object> records)
		{
			if (records == null)
				return null;

			var json = JsonConvert.SerializeObject(records, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
			return Encoding.UTF8.GetBytes(json).GZipCompress();

			//BinaryFormatter bf = new BinaryFormatter();
			//using (MemoryStream ms = new MemoryStream())
			//{
			//	bf.Serialize(ms, records);

			//	return ms.ToArray().GZipCompress();
			//}
		}

		private IEnumerable<object> Bytes2Records(byte[] bytes)
		{
			var decompressed = bytes.GZipDecompress();

			return Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<object>>(Encoding.UTF8.GetString(decompressed), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

			//using (MemoryStream ms = new MemoryStream())
			//{
			//	BinaryFormatter bf = new BinaryFormatter();
			//	ms.Write(decompressed, 0, decompressed.Length);
			//	ms.Seek(0, SeekOrigin.Begin);
			//	return bf.Deserialize(ms) as IEnumerable<object>;
			//}
		}

		/// <summary>
		/// Returns chunk's information by guid
		/// </summary>
		/// <param name="guid">Chunk guid</param>
		/// <returns>Chunk object</returns>
		public Chunk GetChunk(Guid guid)
		{
			return GetChunks(new List<Guid>() { guid }).FirstOrDefault();
		}
		/// <summary>
		/// Returns chunks by guids
		/// </summary>
		/// <param name="guids">List of chunk guids</param>
		/// <returns>List of chanks</returns>
		public IEnumerable<Chunk> GetChunks(IEnumerable<Guid> guids)
		{
			using (var db = new ChunkManagerContext())
			{
				return db.Chunks.Where(c => guids.Any(id => id == c.ExternalId)).ToList().Select(c => new RSC.Process.Chunk()
				{
					Id = c.ExternalId,
					Status = c.Status,
					NumberOfRecords = c.NumberOfRecords,
					Parameters = c.Parameters.Select(p => new RSC.Process.Chunk.Parameter()
					{
						Name = p.Name,
						Value = p.Value
					}).ToList()
				}).ToList();
			}
		}
		/// <summary>
		/// Change chunk status
		/// </summary>
		/// <param name="guid">Chunk Id</param>
		/// <param name="status">New status</param>
		/// <returns>True if operation was successful</returns>
		public bool ChangeStatus(Guid guid, ChunkStatus status)
		{
			using (var db = new ChunkManagerContext())
			{
				var chunk = db.Chunks.FirstOrDefault(c => c.ExternalId == guid);

				if (chunk == null)
					return false;

				chunk.Status = status;

				return db.SaveChanges() > 0;
			}
		}
		/// <summary>
		/// Delete chunk by chunk Id
		/// </summary>
		/// <param name="guid">Chunkd Id</param>
		/// <returns>True if operation was successful</returns>
		public bool DeleteChunk(Guid guid)
		{
			using (var db = new ChunkManagerContext())
			{
				var chunk = db.Chunks.Where(c => c.ExternalId == guid).FirstOrDefault();

				if(chunk == null)
					return false;

				db.Blobs.Remove(chunk.Blob);
				db.Parameters.RemoveRange(chunk.Parameters);
				db.Chunks.Remove(chunk);

				return db.SaveChanges() > 0;
			}
		}
		/// <summary>
		/// Returns list of records by chunk ID
		/// </summary>
		/// <param name="guid">Chunk ID</param>
		/// <returns>List of records</returns>
		public IEnumerable<object> GetRecords(Guid guid)
		{
			using (var db = new ChunkManagerContext())
			{
				var blob = db.Blobs.FirstOrDefault(b=> b.Chunk.ExternalId == guid);

				if (blob == null)
					return null;

				return Bytes2Records(blob.Data);
			}
		}
		/// <summary>
		/// Search chunks
		/// </summary>
		/// <param name="parameters">List of parameters to query</param>
		/// <returns>List of chunks GUIDs</returns>
		public IEnumerable<Guid> SearchChunks(IEnumerable<RSC.Process.Chunk.Parameter> parameters, ChunkStatus status = ChunkStatus.All, int start = 0, int count = -1)
		{
			using (var db = new ChunkManagerContext())
			{
				IEnumerable<Guid> guids = null;

				foreach (var param in parameters)
				{
					if (guids == null)
						guids = db.Parameters.AsNoTracking().Where(p => p.Name == param.Name && p.Value == param.Value).Select(p => p.Chunk.ExternalId).ToList();
					else
						guids = db.Parameters.AsNoTracking().Where(p => p.Name == param.Name && p.Value == param.Value).Select(p => p.Chunk.ExternalId).Intersect(guids).ToList();
				}

				if (status != ChunkStatus.All)
					guids = db.Chunks.Where(c => c.Status == status && guids.Contains(c.ExternalId)).Select(c => c.ExternalId).ToList();

				if (start > 0)
					guids = guids.Skip(start);

				if (count > 0)
					guids = guids.Take(count);

				return guids.ToList();
			}
		}

		/// <summary>
		/// Chunks statistics
		/// </summary>
		/// <param name="parameters">List of parameters to query</param>
		/// <returns>Chunks statistics object</returns>
		public ChunksStatistics ChunksStatistics(IEnumerable<RSC.Process.Chunk.Parameter> parameters)
		{
			using (var db = new ChunkManagerContext())
			{
				db.Configuration.AutoDetectChangesEnabled = false;

				IEnumerable<Guid> guids = null;

				foreach (var param in parameters)
				{
					if (guids == null)
						guids = db.Parameters.AsNoTracking().Where(p => p.Name == param.Name && p.Value == param.Value).Select(p => p.Chunk.ExternalId).ToList();
					else
						guids = db.Parameters.AsNoTracking().Where(p => p.Name == param.Name && p.Value == param.Value).Select(p => p.Chunk.ExternalId).ToList().Intersect(guids).ToList();
				}

				var stats = new ChunksStatistics();

				stats.Total = db.Chunks.AsNoTracking().Where(c => guids.Contains(c.ExternalId)).Count();

				var statuses = db.Chunks.AsNoTracking().Where(c => guids.Contains(c.ExternalId)).GroupBy(c => c.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToList();

				foreach (var s in statuses)
				{
					if (s.Status == ChunkStatus.Created)
						stats.New = s.Count;
					else if (s.Status == ChunkStatus.Processing)
						stats.Processing = s.Count;
					else if (s.Status == ChunkStatus.Processed)
						stats.Processed = s.Count;
					else if (s.Status == ChunkStatus.Failed)
						stats.Failed = s.Count;
				}

				return stats;
			}
		}
	}
}
