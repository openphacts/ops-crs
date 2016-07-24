using RSC.Process;
using System;
using System.Collections.Generic;

namespace RSC.CVSP
{
	public interface IChunkStorage
	{
		Guid CreateChunk(Guid deposition, Chunk chunk);
		/// <summary>
		/// Check if chuck exists
		/// </summary>
		/// <param name="guid">Chunk Id</param>
		/// <returns>True if chunk exists, otherwire returns False</returns>
		bool Exist(Guid guid);
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
		/// Returns list of deposition's chunks GUIDs
		/// </summary>
		/// <param name="guid">Deposition Id</param>
		/// <param name="start">Index where to start returning GUIDs</param>
		/// <param name="count">Number of returned GUIDs</param>
		/// <returns>List of deposition's chunks GUIDs</returns>
		IEnumerable<Guid> GetDepositionChunks(Guid guid, ChunkType type = ChunkType.All, int start = 0, int count = 10);
		/// <summary>
		/// Update chunk information
		/// </summary>
		/// <param name="chunk">Updated chunk object</param>
		/// <returns>True if operation was successful</returns>
		bool UpdateChunk(Chunk chunk);
		/// <summary>
		/// Delete chunk by chunk Id
		/// </summary>
		/// <param name="guid">Chunkd Id</param>
		/// <returns>True if operation was successful</returns>
		bool DeleteChunk(Guid guid);
		/// <summary>
		/// Delete all chunks by deposition Id
		/// </summary>
		/// <param name="guid">Deposition Id</param>
		/// <returns>True if operation was successful</returns>
		bool DeleteChunks(Guid guid);
	}
}
