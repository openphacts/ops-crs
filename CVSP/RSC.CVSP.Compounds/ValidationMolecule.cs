using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MoleculeObjects;
using Microsoft.Practices.ServiceLocation;
using RSC.Logging;

namespace RSC.CVSP.Compounds
{
	public class ValidationMolecule
	{
		private Molecule m;

		private readonly IAcidity acidity = null;

		private readonly IValidationRuleModule validationRuleModule = null;

		public ValidationMolecule(GenericMolecule gm)
		{
			acidity = ServiceLocator.Current.GetService(typeof(IAcidity)) as IAcidity;

			validationRuleModule = ServiceLocator.Current.GetService(typeof(IValidationRuleModule)) as IValidationRuleModule;

			if (acidity == null)
				throw new ArgumentNullException("acidity");

			if (validationRuleModule == null)
				throw new ArgumentNullException("validation rule module");

			m = MoleculeFactory.FromGenericMolecule(gm);
		}

        /// <summary>
        /// Special version for unit testing.
        /// </summary>
        public ValidationMolecule(GenericMolecule gm, ValidationRuleModule vrm)
        {
            acidity = ServiceLocator.Current.GetService(typeof(IAcidity)) as IAcidity;
            validationRuleModule = vrm;
            if (acidity == null)
                throw new ArgumentNullException("acidity");
            m = MoleculeFactory.FromGenericMolecule(gm);
        }

		public List<Issue> runTests()
        {
            List<Issue> issues = new List<Issue>();
            issues.AddRange(runValidationFunctions(m, validationRuleModule.FunctionCollection()));

            if (!m.AllNeutralMolecules())
			{
				issues.Add(new Issue() { Code = "100.9" });
			}
			if (m.HasChair())
			{
				issues.Add(new Issue() { Code = "100.10" });
			}
			if (m.HasBoat())
			{
				issues.Add(new Issue() { Code = "100.11" });
			}
			if (m.HasHaworth())
			{
				issues.Add(new Issue() { Code = "100.12" });
			}
			if (m.HasLPyranose())
			{
				issues.Add(new Issue() { Code = "100.19" });
			}
			//------------------------------------------
			//non linear triple bonded centers
			if (m.HasNonlinearTripleBondedCentre("C"))
			{
				issues.Add(new Issue() { Code = "100.20" });
			}
			if (m.HasNonlinearTripleBondedCentre("N"))
			{
				issues.Add(new Issue() { Code = "100.21" });
			}
			//--------------------------------------------------
			if (acidity.WeakerAcidIonizedBeforeStrongerAcid(m))
			{
				issues.Add(new Issue() { Code = "100.8" });
			}
			return issues;
		}

		private IEnumerable<Issue> runValidationFunctions(Molecule m, List<Tuple<Func<Molecule, bool>, Issue>> list)
		{
			ICollection<Issue> issues = new List<Issue>();
			foreach (Tuple<Func<Molecule, bool>, Issue> w in list)
			{
				try
				{
					Func<Molecule, bool> f = w.Item1;
					bool res = f(m);
					if (res)
					{
						Issue i = w.Item2;
						lock (issues)
						{
							issues.Add(new Issue() { Code = i.Code, Message = i.Message, AuxInfo = i.AuxInfo });
						}
					}
				}
				catch (Exception ex)
				{
					lock (issues)
					{
						issues.Add(new Issue()
						{
							Code = "200.14",
							AuxInfo = ex.StackTrace,
							Message = ex.Message
						});
					}
				}
			}
			return issues;
		}
	}
}
