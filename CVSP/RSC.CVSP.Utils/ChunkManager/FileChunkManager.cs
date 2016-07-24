using RSC.Process;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace RSC.CVSP
{
	public class FileChunkManager : IChunkManager
	{
		private Object thisLock = new Object();

		private IChunkStorage storage = new FileChunkStorage();
		public string Root { get; private set; }

		public FileChunkManager()
		{
			Root = ConfigurationManager.AppSettings["chunks_storage_root"];

			if (string.IsNullOrEmpty(Root))
				throw new ArgumentNullException("Chunk storage root is not specified");
		}

		public bool HasChunk(Guid id)
		{
			throw new NotImplementedException();
		}

		public Guid CreateChunk(Chunk chunk, IEnumerable<object> records)
		{
/*
			Guid chunkId = storage.CreateChunk(deposition, new CVSPChunk() {
				Deposition = deposition,
				Status = ChunkStatus.Created,
				Type = type
			});

			string dir = Path.Combine(Root, deposition.ToString(), "Chunks");

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			//	output records to a chunk file...
			ICompressRecords compressor = new RecordsCompressor(Path.Combine(dir, string.Format("{0}.xml.gz", chunkId)));
			compressor.Compress(records);

			//	update number of records in chunk...
			Chunk chunk = GetChunk(chunkId);
			chunk.NumberOfRecords = records.Count();
			storage.UpdateChunk(chunk);

			return chunkId;
*/
			return Guid.Empty;
		}

		public Chunk GetChunk(Guid guid)
		{
			return storage.GetChunk(guid);
		}

		public IEnumerable<Chunk> GetChunks(IEnumerable<Guid> guids)
		{
			return storage.GetChunks(guids);
		}

		//public IEnumerable<Guid> GetDepositionChunks(Guid guid, ChunkType type = ChunkType.All, int start = 0, int count = -1)
		//{
		//	return storage.GetDepositionChunks(guid, type, start, count);
		//}

		public bool ChangeStatus(Guid guid, ChunkStatus status)
		{
			lock(thisLock)
			{
				Chunk chunk = GetChunk(guid);

				if (chunk == null)
					return false;

				chunk.Status = status;

				return storage.UpdateChunk(chunk);
			}
		}

		public bool DeleteChunk(Guid guid)
		{
			var files = Directory.GetFiles(Root, string.Format("{0}-*.xml.gz", guid), SearchOption.AllDirectories);

			foreach (var file in files)
				File.Delete(file);

			files = Directory.GetFiles(Root, string.Format("{0}.json", guid), SearchOption.AllDirectories);

			return storage.DeleteChunk(guid);
		}

		//public bool DeleteChunks(Guid guid)
		//{
		//	var path = Path.Combine(Root, guid.ToString(), "Chunks");

		//	if(Directory.Exists(path))
		//		Directory.Delete(path, true);

		//	return true;
		//}

		public IEnumerable<object> GetRecords(Guid guid)
		{
		//	var files = Directory.GetFiles(Root, string.Format("{0}.xml.gz", guid), SearchOption.AllDirectories);

		//	if (files.Length == 0)
		//		return new List<Record>();

		//	ICompressRecords compressor = new RecordsCompressor();
		//	return compressor.Uncompress(files[0]);
			throw new NotImplementedException();
		}
		public IEnumerable<Guid> SearchChunks(IEnumerable<RSC.Process.Chunk.Parameter> parameters, ChunkStatus status = ChunkStatus.All, int start = 0, int count = -1)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Chunks statistics
		/// </summary>
		/// <param name="parameters">List of parameters to query</param>
		/// <returns>Chunks statistics object</returns>
		public ChunksStatistics ChunksStatistics(IEnumerable<RSC.Process.Chunk.Parameter> parameters)
		{
			throw new NotImplementedException();
		}
	}
}
