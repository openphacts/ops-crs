using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChemImageNet;
using com.ggasoftware.indigo;

namespace ChemSpider.Formats
{
	public static class ImageUtils
	{
		private static Indigo s_indigo;
		private static IndigoRenderer s_indigo_renderer;
		static ImageUtils()
		{
			s_indigo = new Indigo();
			s_indigo.setOption("ignore-stereochemistry-errors", true);
			s_indigo.setOption("ignore-noncritical-query-features", true);
			// Throws unknown option error... s_indigo.setOption("render-stereo-style", "ext");
			s_indigo.setOption("unique-dearomatization", false);
			s_indigo_renderer = new IndigoRenderer(s_indigo);
		}
		public enum ImagesGenerator { ChemImage, Indigo, OpenEye }

		public static byte[] Mol2Image(string mol, int width, int height, ImagesGenerator eGen = ImagesGenerator.ChemImage, GraphFormat eFmt = GraphFormat.Png)
		{
			switch ( eGen ) {
				case ImagesGenerator.ChemImage:
					return ChemImage.GetInstance().sdf2image(mol, width, height, eFmt);
				case ImagesGenerator.Indigo:
					lock (s_indigo)
					{
						//indigo supports PNG, SVG, or PDF only
						s_indigo.setOption("render-output-format", "png");
						IndigoObject obj = s_indigo.loadMolecule(mol);
						s_indigo.setOption("render-image-size", width, height);
						return s_indigo_renderer.renderToBuffer(obj);
					}
				case ImagesGenerator.OpenEye:
					return null;
			}

			return null;
		}
	}
}
