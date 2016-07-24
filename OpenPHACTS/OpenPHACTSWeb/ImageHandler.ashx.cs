using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MoleculeObjects;
using ChemSpider.Compounds.Database;
using ChemSpider.Compounds;

public class ImageHandler : IHttpHandler
{
	private ISubstancesService substanceProvider = new SubstancesService(new SQLSubstancesStore());
	private ICompoundsService compoundProvider = new CompoundsService(new SQLCompoundsStore(new SQLSubstancesStore()));

	public void ProcessRequest(HttpContext context)
	{
		int w = int.Parse(context.Request["w"] ?? "0");
		int h = int.Parse(context.Request["h"] ?? "0");

		context.Response.Clear();
		if (!String.IsNullOrEmpty(context.Request.QueryString["SubstanceID"]))
		{
			int sid;
			byte[] image = null;
			if (Int32.TryParse(context.Request.QueryString["SubstanceID"], out sid))
				image = generateSubstanceImage(sid, w, h);
			if (image != null)
			{
				context.Response.ContentType = "image/gif";
				context.Response.BinaryWrite(image);
				context.Response.Flush();
			}
		}
		else if (!String.IsNullOrEmpty(context.Request.QueryString["CompoundID"]))
		{
			int csid;
			byte[] image = null;
			if (Int32.TryParse(context.Request.QueryString["CompoundID"], out csid))
				image = generateCompoundImage(csid, w, h);
			if (image != null)
			{
				context.Response.ContentType = "image/gif";
				context.Response.BinaryWrite(image);
				context.Response.Flush();
			}
		}
	}

	private byte[] generateSubstanceImage(int sid, int w, int h)
	{
		byte[] sdf_byte = System.Text.Encoding.Default.GetBytes(substanceProvider.GetSDF(sid));
		return Renderers.ImageFromMolecule(sdf_byte, w <= 0 ? 500 : w, h <= 0 ? 500 : h);
	}

	private byte[] generateCompoundImage(int csid, int w, int h)
	{
		byte[] sdf_byte = System.Text.Encoding.Default.GetBytes(compoundProvider.GetSDF(csid));

		return Renderers.ImageFromMolecule(sdf_byte, w <= 0 ? 300 : w, h <= 0 ? 300 : h);
	}

	public bool IsReusable
	{
		get
		{
			return false;
		}
	}
}
