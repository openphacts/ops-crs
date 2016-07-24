using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Xml.XPath;
using System.Xml.Linq;
using com.ggasoftware.indigo;
using MoleculeObjects;

namespace RSC.CVSP.Compounds
{
	public enum ProtonationState { Acid, Base };

	public interface IAcidity
	{
        Dictionary<string, string> GetSubstructureTransformMapping();

		/// <summary>
		/// takes molecule and returns dictionary of acid-base rule_id and ordered acid-base smikrs 
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		Dictionary<int, string> AcidBaseSMIRKS(string molfile);

		/// <summary>
		/// Returns true if partially ionized acid has an acid ahead of the base on the list.
		/// </summary>
		/// <returns></returns>
		bool WeakerAcidIonizedBeforeStrongerAcid(Molecule m);

		List<Tuple<int, List<ProtonationState>>> AcidBaseMatches(Molecule m);
	}
}
