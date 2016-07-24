using RSC.Process;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace RSC.CVSP
{
	public class FileChunkStorage : IChunkStorage
	{
		public string Root { get; private set; }

		public FileChunkStorage()
		{
			Root = ConfigurationManager.AppSettings["chunks_storage_root"];

			if (string.IsNullOrEmpty(Root))
				throw new ArgumentNullException("Chunk storage root is not specified");
		}

		public Guid CreateChunk(Guid deposition, Chunk chunk)
		{
			var chunkId = Guid.NewGuid();

			chunk.Id = chunkId;
			//chunk.DepositionId = deposition;

			Serialize(chunk);

			return chunkId;
		}

		public bool Exist(Guid guid)
		{
			var files = Directory.GetFiles(Root, string.Format("{0}.json", guid), SearchOption.AllDirectories);

			return files.Length == 1;
		}

		public Chunk GetChunk(Guid guid)
		{
			var files = Directory.GetFiles(Root, string.Format("{0}.json", guid), SearchOption.AllDirectories);

			if (files.Length == 0)
				return null;

			var json = File.ReadAllText(files[0]);

			return new JavaScriptSerializer().Deserialize<Chunk>(json);
		}

		public IEnumerable<Chunk> GetChunks(IEnumerable<Guid> guids)
		{
			if (guids == null || guids.Count() == 0)
				return new List<Chunk>();

			JavaScriptSerializer serializer = new JavaScriptSerializer();

			var files = from g in guids select Directory.GetFiles(Root, string.Format("{0}.json", g), SearchOption.AllDirectories).FirstOrDefault();

			return from f in files select serializer.Deserialize<Chunk>(File.ReadAllText(f));
		}

		public IEnumerable<Guid> GetDepositionChunks(Guid guid, ChunkType type = ChunkType.All, int start = 0, int count = 10)
		{
			//	Empty request...
			if (count == 0)
				return new List<Guid>();
/*
			JavaScriptSerializer serializer = new JavaScriptSerializer();

			var depositionRoot = Path.Combine(Root, guid.ToString(), "Chunks");

			if(!Directory.Exists(depositionRoot))
				return new List<Guid>();

			var chunks = DepositionChunks(guid);

			if (type != ChunkType.All)
				chunks = chunks.Where(c => c.Type == type);

			if (start > 0)
				chunks = chunks.Skip(start);

			if (count > 0)
				chunks = chunks.Take(count);

			return from c in chunks select c.Id;
*/
			return new List<Guid>();
		}

		public bool UpdateChunk(Chunk chunk)
		{
			if (!Exist(chunk.Id))
				return false;

			Serialize(chunk);

			return true;
		}

		public bool DeleteChunk(Guid guid)
		{
			var files = Directory.GetFiles(Root, string.Format("{0}.json", guid), SearchOption.AllDirectories);

			foreach(var path in files)
				File.Delete(path);

			return true;
		}

		public bool DeleteChunks(Guid guid)
		{
			//	Nothing to do... everything will be deleted by FileChunkManager

			return true;
		}

		private void Serialize(Chunk chunk)
		{
/*
			string dir = Path.Combine(Root, chunk.DepositionId.ToString(), "Chunks");

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			var json = new JavaScriptSerializer().Serialize(chunk);
			File.WriteAllText(Path.Combine(dir, string.Format("{0}.json", chunk.Id)), json);
*/
		}

		private IEnumerable<Chunk> DepositionChunks(Guid guid)
		{
			JavaScriptSerializer serializer = new JavaScriptSerializer();

			string[] files = Directory.GetFiles(Path.Combine(Root, guid.ToString(), "Chunks"), "*.json");

			return from f in files select serializer.Deserialize<Chunk>(File.ReadAllText(f));
		}
	}
}
