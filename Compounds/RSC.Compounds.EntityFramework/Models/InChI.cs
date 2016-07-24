using ChemSpider.Molecules;
using ChemSpider.Utilities;
using InChINet;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.EntityFramework
{
	[Table("InChIs")]
	public class ef_InChI : IBeforeInsert, IBeforeUpdate
	{
		[DatabaseGenerated(DatabaseGeneratedOption.None), Key()]
		public Guid Id { get; set; }

		[Required]
		public string InChI { get; set; }

		[Required]
		[MaxLength(27)]
		[Index("InChIKey_idx", Order = 1, IsUnique=true)]
		public string InChIKey { get; set; }

		public virtual ef_InChIMD5 InChIMD5 { get; set; }

		public void OnBeforeInsert()
		{
			string[] inchi_layers = null;
			if (InChI.StartsWith("InChI="))
			{
				inchi_layers = InChIUtils.getInChILayers(InChI);
			}
			else if (InChIUtils.isValidInChI(InChI))
			{
				string message;
				string inchi = ChemIdUtils.anyId2InChI(InChI, out message, InChIFlags.v104);
				if (!string.IsNullOrEmpty(inchi))
					inchi_layers = InChIUtils.getInChILayers(inchi);
			}

			if (inchi_layers != null)
			{
				if (InChIMD5 == null)
					InChIMD5 = new ef_InChIMD5();

				InChIMD5.InChIKey_A = InChIKey.Substring(0, 14);
				InChIMD5.InChIKey_B = InChIKey.Substring(0, 23);
				InChIMD5.InChI_MD5 = InChI.GetMD5Hash();
				InChIMD5.InChI_C_MD5 = inchi_layers[2].GetMD5Hash();
				InChIMD5.InChI_CH_MD5 = inchi_layers[1].GetMD5Hash();
				InChIMD5.InChI_MF_MD5 = inchi_layers[3].GetMD5Hash();
				InChIMD5.InChI_CHSI_MD5 = inchi_layers[4].GetMD5Hash();
				InChIMD5.InChI = this;
			}
		}

		public void OnBeforeUpdate()
		{
			string[] inchi_layers = null;
			if (InChI.StartsWith("InChI="))
			{
				inchi_layers = InChIUtils.getInChILayers(InChI);
			}
			else if (InChIUtils.isValidInChI(InChI))
			{
				string message;
				string inchi = ChemIdUtils.anyId2InChI(InChI, out message, InChIFlags.v104);
				if (!string.IsNullOrEmpty(inchi))
					inchi_layers = InChIUtils.getInChILayers(inchi);
			}

			if (inchi_layers != null)
			{
				InChIMD5.InChIKey_A = InChIKey.Substring(0, 14);
				InChIMD5.InChIKey_B = InChIKey.Substring(0, 23);
				InChIMD5.InChI_MD5 = InChI.GetMD5Hash();
				InChIMD5.InChI_C_MD5 = inchi_layers[2].GetMD5Hash();
				InChIMD5.InChI_CH_MD5 = inchi_layers[1].GetMD5Hash();
				InChIMD5.InChI_MF_MD5 = inchi_layers[3].GetMD5Hash();
				InChIMD5.InChI_CHSI_MD5 = inchi_layers[4].GetMD5Hash();
				InChIMD5.InChI = this;
			}
		}
	}
}
