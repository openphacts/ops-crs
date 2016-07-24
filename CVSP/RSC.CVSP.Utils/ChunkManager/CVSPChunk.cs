using RSC.Process;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Linq;

namespace RSC.CVSP
{
	public static class CVSPChunkExtensions
	{
		public static CVSPChunk ToCVSPChunk(this Chunk chunk)
		{
			return new CVSPChunk()
			{
				Id = chunk.Id,
				Status = chunk.Status,
				NumberOfRecords = chunk.NumberOfRecords,
				Parameters = chunk.Parameters
			};
		}
	}

	public static class CVSPChunkManagerExtensions
	{
		public static Guid CreateChunk(this IChunkManager manager, Guid depositionId, ChunkType type, IEnumerable<Record> records)
		{
			return manager.CreateChunk(new CVSPChunk() { Deposition = depositionId, Type = type }, records);
		}

		public static IEnumerable<Guid> GetDepositionChunks(this IChunkManager manager, Guid depositionId, ChunkType type = ChunkType.All, int start = 0, int count = -1)
		{
			var parameters = new List<RSC.Process.Chunk.Parameter>();

			parameters.Add(new RSC.Process.Chunk.Parameter() { Name = "deposition", Value = depositionId.ToString() });
			if (type != ChunkType.All)
				parameters.Add(new RSC.Process.Chunk.Parameter() { Name = "chunktype", Value = type.ToString() });

			return manager.SearchChunks(parameters, ChunkStatus.All, start, count);
		}

		public static ChunksStatistics GetDepositionChunksStats(this IChunkManager manager, Guid depositionId, ChunkType type)
		{
			var parameters = new List<RSC.Process.Chunk.Parameter>();

			parameters.Add(new RSC.Process.Chunk.Parameter() { Name = "deposition", Value = depositionId.ToString() });
			parameters.Add(new RSC.Process.Chunk.Parameter() { Name = "chunktype", Value = type.ToString() });

			return manager.ChunksStatistics(parameters);
		}

		public static void DeleteDepositionChunks(this IChunkManager manager, Guid depositionId)
		{
			var guids = manager.SearchChunks(new List<RSC.Process.Chunk.Parameter>() { 
				new RSC.Process.Chunk.Parameter() { Name = "deposition", Value = depositionId.ToString() } 
			});

			foreach (var guid in guids.ToList())
				manager.DeleteChunk(guid);
		}
	}

	[DataContract]
	[Flags]
	public enum ChunkType
	{
		[Display(Name = "Original")]
		[EnumMember]
		Original = 1,

		[Display(Name = "Processed")]
		[EnumMember]
		Processed = 2,

		[Display(Name = "Upload to GCN")]
		[EnumMember]
		Upload2GCN = 4,

		//[Display(Name = "Processed for GCN")]
		//[EnumMember]
		//Processed4GCN = 8,

		[EnumMember]
		Unknown = 16,

		[EnumMember]
		All = 31
	}

	public class CVSPChunk : Chunk
	{
		public CVSPChunk()
		{
		}

		public Guid Deposition
		{
			get { return Guid.Parse(GetParameter("deposition")); }
			set { AddParameter("deposition", value.ToString()); }
		}

		public ChunkType Type
		{
			get
			{
				ChunkType type;
				return Enum.TryParse(GetParameter("chunktype"), out type) ? type : ChunkType.Unknown;
			}
			set { AddParameter("chunktype", value.ToString()); }
		}
	}
}
