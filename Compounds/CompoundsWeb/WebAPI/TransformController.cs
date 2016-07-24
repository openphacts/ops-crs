using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace CompoundsWeb.WebAPI
{
	public class XslTransformOptions
	{
		public string Xml { get; set; }
		public string Xsl { get; set; }
	}

	[EnableCors("*", "*", "POST")]
	public class TransformController : ApiController
	{
		// GET api/transform/xsl
		/// <summary>
		/// Transform XML document using XSL template
		/// </summary>
		/// <returns></returns>
		///
		[AcceptVerbs("POST")]
		[Route("api/transform/xsl")]
		public HttpResponseMessage XslTransform([FromBody] XslTransformOptions transformOptions)
		{
			using (var webClient = new System.Net.WebClient())
			{
				XDocument xmlDoc = XDocument.Parse(transformOptions.Xml);
				XDocument xslDoc = XDocument.Parse(transformOptions.Xsl);

				XmlWriterSettings settings = new XmlWriterSettings() {
					ConformanceLevel = ConformanceLevel.Fragment
				};

				MemoryStream ms = new MemoryStream();
				using (XmlWriter writer = XmlWriter.Create(ms, settings))
				{
					XslCompiledTransform transform = new XslCompiledTransform();
					transform.Load(xslDoc.CreateReader());
					transform.Transform(xmlDoc.CreateReader(), writer);
				}

				ms.Position = 0;

				HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StreamContent(ms)
				};

				return result;
			}
		}
	}
}
