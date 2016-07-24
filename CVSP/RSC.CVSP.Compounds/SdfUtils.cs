using System.Collections.Generic;
using ChemSpider.Molecules;
using System.IO;
using System;

namespace RSC.CVSP.Compounds
{
    /// <summary>
    /// See GCNCD-218. Marked as obsolete because nothing references this.
    /// It *may* be useful for when we reinstate identifier validation.
    /// </summary>
    [Obsolete]
	public class SdfUtils
	{
		/// <summary>
		/// takes sdf record and sd tag dictionary map and returns sdf record with replace sd field names
		/// </summary>
		/// <param name="sdf"></param>
		/// <param name="sdTagMap"></param>
		/// <returns></returns>
		public static SdfRecord FindAndReplaceSDTags(SdfRecord sdf, IDictionary<string, SDTagOptions> sdTagMap)
		{
            Console.WriteLine("inside findAndReplaceSDTags");
            foreach (var p in sdTagMap) Console.WriteLine(p.Key + " " + p.Value);
			if (sdTagMap == null)
				return sdf;
			SdfRecord new_sdf = new SdfRecord();
			new_sdf.Molecule = sdf.Molecule;
			if (sdf.Properties != null)
			{
				foreach (var kv in sdf.Properties)
				{
					if (sdTagMap.ContainsKey(kv.Key))
					{
						if (!new_sdf.Properties.ContainsKey(sdTagMap[kv.Key].ToString()))
							new_sdf.Properties[sdTagMap[kv.Key].ToString()] = kv.Value;
						else foreach (string value in kv.Value)
							new_sdf.Properties[sdTagMap[kv.Key].ToString()].Add(value);
					}
					else new_sdf.Properties[kv.Key] = kv.Value;
				}
			}
			return new_sdf;
		}
    }
}
