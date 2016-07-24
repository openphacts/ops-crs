using System.Collections.Generic;
using ChemSpider.Search;

namespace RSC.Compounds.Search.Old
{
	/// <summary>
	/// Provides parts of the final SQL statement for search by MF and masses.
	/// Adds mass_defect calculated column.
	/// Only one mass_defect column is added if multiple mass search criteria are specified.
	/// If no other sorting specified the final result set will be sorted by mass_defect.
	/// </summary>
	public class CSCIntrinsicPropertiesSearch : CSIntrinsicPropertiesSearch
	{
		public CSCIntrinsicPropertiesSearch()
		{
			m_sqlProvider = new CSCSqlSearchProvider();
		}

		protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
		{
			bool bMFAdded = false, bPropsAdded = false, bMassDefectAdded = false;
			string parsedFormula = string.Empty;

			tables.Add("JOIN compound_properties cp on cp.csid = c.csid");

			// Molecular Weight
			if ( Options.MolWeightMin != null ) {
				predicates.Add(string.Format("cp.mw_indigo >= {0}", Options.MolWeightMin));
				visual.Add(string.Format("MW >= {0}", Options.MolWeightMin));
				bPropsAdded = true;
			}
			if ( Options.MolWeightMax != null ) {
				predicates.Add(string.Format("cp.mw_indigo <= {0}", Options.MolWeightMax));
				visual.Add(string.Format("MW <= {0}", Options.MolWeightMax));
				bPropsAdded = true;
			}
			double? median = (Options.MolWeightMin + Options.MolWeightMax) / 2;
			if ( median != null && columns.Count == 0 ) {
				string mass_defect = string.Format("abs(cp.mw_indigo - {0}) as mass_defect", median);
				columns.Add(mass_defect);
				visual.Add(mass_defect);
				bMassDefectAdded = true;
			}
/*
			// Nominal Mass
			if ( Options.NominalMassMin != null ) {
				predicates.Add(string.Format("ap++.Nominal_Mass >= {0}", Options.NominalMassMin));
				visual.Add(string.Format("NM >= {0}", Options.NominalMassMin));
				bPropsAdded = true;
			}
			if ( Options.NominalMassMax != null ) {
				predicates.Add(string.Format("ap++.Nominal_Mass <= {0}", Options.NominalMassMax));
				visual.Add(string.Format("NM <= {0}", Options.NominalMassMax));
				bPropsAdded = true;
			}
			median = (Options.NominalMassMin + Options.NominalMassMax) / 2;
			if ( median != null && columns.Count == 0 ) {
				string mass_defect = string.Format("abs(Nominal_Mass - {0}) as mass_defect", median);
				columns.Add(mass_defect);
				visual.Add(mass_defect);
				bMassDefectAdded = true;
			}

			// Average Mass
			if ( Options.AverageMassMin != null ) {
				predicates.Add(string.Format("ap++.Average_Mass >= {0}", Options.AverageMassMin));
				visual.Add(string.Format("AM >= {0}", Options.AverageMassMax));
				bPropsAdded = true;
			}
			if ( Options.AverageMassMax != null ) {
				predicates.Add(string.Format("ap++.Average_Mass <= {0}", Options.AverageMassMax));
				visual.Add(string.Format("AM <= {0}", Options.AverageMassMax));
				bPropsAdded = true;
			}
			median = (Options.AverageMassMin + Options.AverageMassMax) / 2;
			if ( median != null && columns.Count == 0 ) {
				string mass_defect = string.Format("abs(Average_Mass - {0}) as mass_defect", median);
				columns.Add(mass_defect);
				visual.Add(mass_defect);
				bMassDefectAdded = true;
			}
*/
			// Monoisotopic Mass
			if ( Options.MonoisotopicMassMin != null ) {
				predicates.Add(string.Format("cp.monoisotopicMass_indigo >= {0}", Options.MonoisotopicMassMin));
				visual.Add(string.Format("MM >= {0}", ((double)Options.MonoisotopicMassMin).ToString("#0.0#####")));
				bPropsAdded = true;
			}
			if ( Options.MonoisotopicMassMax != null ) {
				predicates.Add(string.Format("cp.monoisotopicMass_indigo <= {0}", Options.MonoisotopicMassMax));
				visual.Add(string.Format("MM <= {0}", ((double)Options.MonoisotopicMassMax).ToString("#0.0#####")));
				bPropsAdded = true;
			}
			median = (Options.MonoisotopicMassMin + Options.MonoisotopicMassMax) / 2;
			if ( median != null && columns.Count == 0 ) {
				string mass_defect = string.Format("abs(cp.monoisotopicMass_indigo - {0}) as mass_defect", median);
				columns.Add(mass_defect);
				visual.Add(mass_defect);
				bMassDefectAdded = true;
			}

			if ( bMassDefectAdded )
				orderby.Add("mass_defect");

			return bMFAdded || bPropsAdded;
		}
	}
}
