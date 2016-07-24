using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MoleculeObjects;
using RSC.Logging;

namespace RSC.CVSP.Compounds
{
	public class ValidationGenericMolecule
	{
		public GenericMolecule gm;
		public ValidationGenericMolecule(string molfile, List<Issue> issues)
		{
            try
            {
                gm = MoleculeFactory.FromMolV2000(molfile);
            }
            catch (Exception e)
            {
                issues.Add("100.28", e.Message, e.StackTrace);
            }
		}

        public List<Issue> runTests()
		{
			List<Issue> issues = new List<Issue>();
			if (gm.HasNonInChIfiableAtoms())
				issues.Add(new Issue() { Code = "100.4" });

            if (gm.HasNonAromaticQueryBonds())
                issues.Add(new Issue() { Code = "100.5" });
            
            if (gm.HasArgonAtom() && gm.IndexedAtoms.Count > 1)
				issues.Add(new Issue() { Code = "100.6" });

            if (gm.HasFixedValence())
                issues.Add(new Issue() { Code = "100.7" });

			if (gm.ContainsStereoBondAdjacentToDoubleBond())
				issues.Add(new Issue() { Code = "100.17" });

            if (gm.Is0D() && gm.IndexedAtoms.Count > 1)
                issues.Add(new Issue() { Code = "100.22" });

            if (gm.Is3D())
                issues.Add(new Issue() { Code = "100.23" });

            if (gm.IsCongested())
                issues.Add(new Issue() { Code = "100.24" });

			if (gm.ContainsCrossedDoubleBond())
				issues.Add(new Issue() { Code = "100.38" });

			if (gm.ContainsWavyBondAdjacentToCrossedDoubleBond())
				issues.Add(new Issue() { Code = "100.39" });

            return issues;
		}
	}
}
