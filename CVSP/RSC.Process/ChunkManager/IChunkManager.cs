using System;
using System.Collections.Generic;

namespace RSC.Process
{
	public interface IChunkManager
	{
		/// <summary>
		/// Check if chunk with Id exists
		/// </summary>
		/// <param name="id">Chunk id</param>
		/// <returns>True is chunk exists</returns>
		bool HasChunk(Guid id);
		/// <summary>
		/// Create new chunk
		/// </summary>
		/// <param name="deposition">Deposition guid</param>
		/// <returns>New chunk guid</returns>
		Guid CreateChunk(Chunk chunk, IEnumerable<object> records);
		/// <summary>
		/// Returns chunk's information by guid
		/// </summary>
		/// <param name="guid">Chunk guid</param>
		/// <returns>Chunk object</returns>
		Chunk GetChunk(Guid guid);
		/// <summary>
		/// Returns chunks by guids
		/// </summary>
		/// <param name="guids">List of chunk guids</param>
		/// <returns>List of chanks</returns>
		IEnumerable<Chunk> GetChunks(IEnumerable<Guid> guids);
		/// <summary>
		/// Change chunk status
		/// </summary>
		/// <param name="guid">Chunk Id</param>
		/// <param name="status">New status</param>
		/// <returns>True if operation was successful</returns>
		bool ChangeStatus(Guid guid, ChunkStatus status);
		/// <summary>
		/// Delete chunk by chunk Id
		/// </summary>
		/// <param name="guid">Chunkd Id</param>
		/// <returns>True if operation was successful</returns>
		bool DeleteChunk(Guid guid);
		/// <summary>
		/// Returns list of records by chunk ID
		/// </summary>
		/// <param name="guid">Chunk ID</param>
		/// <returns>List of records</returns>
		IEnumerable<object> GetRecords(Guid guid);
		/// <summary>
		/// Search chunks
		/// </summary>
		/// <param name="parameters">List of parameters to query</param>
		/// <param name="start">Index where to start returning GUIDs</param>
		/// <param name="count">Number of returned GUIDs</param>
		/// <returns>List of chunks GUIDs</returns>
		IEnumerable<Guid> SearchChunks(IEnumerable<RSC.Process.Chunk.Parameter> parameters, ChunkStatus status = ChunkStatus.All, int start = 0, int count = -1);

		/// <summary>
		/// Chunks statistics
		/// </summary>
		/// <param name="parameters">List of parameters to query</param>
		/// <returns>Chunks statistics object</returns>
		ChunksStatistics ChunksStatistics(IEnumerable<RSC.Process.Chunk.Parameter> parameters);
	}
}
