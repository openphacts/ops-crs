using RSC.CVSP;
using RSC.CVSP.Compounds;
using RSC.Process;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Xml;

namespace CVSPWeb.WebAPI
{
	//[Authorize(Roles = "Administrator")]
	[EnableCors("*", "*", "*")]
	public class ChunksController : ApiController
	{
		private readonly IChunkManager chunkManager;

		public ChunksController(IChunkManager chunkManager)
		{
			if (chunkManager == null)
				throw new ArgumentNullException("chunkManager");

			this.chunkManager = chunkManager;
		}

		// GET api/chunks/851107d4-ec5d-4cdf-903b-8ed94aa4132d
		/// <summary>
		/// Get chunk by GUID
		/// </summary>
		/// <param name="guid">Chunk GUID</param>
		/// <returns>Chunk object</returns>
		[Route("api/chunks/{guid}")]
		public IHttpActionResult GetChunk(Guid guid)
		{
			var res = chunkManager.GetChunk(guid);

			return Ok(res);
		}

		// GET api/chunks/list?id={1}&id={2}
		/// <summary>
		/// Returns list of chunks by list of GUIDs
		/// </summary>
		/// <param name="guid">List of chunk GUIDs</param>
		/// <returns>List of chunks</returns>
		[Route("api/chunks/list")]
		[HttpPost]
		public IHttpActionResult GetChunksList([FromBody] Guid[] id)
		{
			var res = chunkManager.GetChunks(id).Select(c => c.ToCVSPChunk()).ToList();

			return Ok(res);
		}

		// GET api/depositions/{guid}/chunks
		/// <summary>
		/// Returns list of deposition's chunks GUIDs
		/// </summary>
		/// <param name="guid">Deposition GUID</param>
		/// <param name="type">Chunks type to return</param>
		/// <param name="start">Index where to start returning GUIDs</param>
		/// <param name="count">Number of returned GUIDs</param>
		/// <returns>List of deposition's chunks GUIDs</returns>
		[Route("api/depositions/{guid}/chunks")]
		public IHttpActionResult GetDepositionChunks(Guid guid, string type = "", int start = 0, int count = 10)
		{
			ChunkType chunkType = ChunkType.All;
			if (!Enum.TryParse<ChunkType>(type, out chunkType))
				chunkType = ChunkType.All;

			var chunks = chunkManager.GetDepositionChunks(guid, chunkType, start, count);

			return Ok(chunks);
		}

		[AcceptVerbs("GET")]
		[Route("api/chunks/{guid}/download")]
		public HttpResponseMessage DownloadChunkRecords(Guid guid)
		{
			var records = chunkManager.GetRecords(guid);

			using (StringWriter sw = new StringWriter())
			{
				var ds = new DataContractSerializer(typeof(IEnumerable<Record>), new[] { typeof(CompoundRecord), typeof(ReactionRecord) });
				using (XmlWriter w = XmlWriter.Create(sw, new XmlWriterSettings() { Indent = true }))
					ds.WriteObject(w, records);

				HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(sw.ToString()),
				};

				result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
				result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = string.Format("{0}.xml", guid) };

				return result;
			}
		}
	}

	public class DepositionStats
	{
		public int TotalChunks { get; set; }
		public int OriginalNew { get; set; }
		public int OriginalProcessing { get; set; }
		public int OriginalProcessed { get; set; }
		public int OriginalFailed { get; set; }
	}
}
