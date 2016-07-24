using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ChemSpider.Molecules;

namespace ChemSpider.Search
{
    /// <summary>
    /// Provides parts of the final SQL statement for search by MF and masses.
    /// Adds mass_defect calculated column.
    /// Only one mass_defect column is added if multiple mass search criteria are specified.
    /// If no other sorting specified the final result set will be sorted by mass_defect.
    /// </summary>
    public class CSIntrinsicPropertiesSearch : CSSqlSearch
    {
        public new IntrinsicPropertiesSearchOptions Options
        {
            get { return base.Options as IntrinsicPropertiesSearchOptions; }
        }

        protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
        {
            bool bMFAdded = false, bPropsAdded = false, bMassDefectAdded = false;
            string parsedFormula = string.Empty;

            // Molecular Formula
            if ( !string.IsNullOrEmpty(Options.EmpiricalFormula) ) {
                List<Tuple<string, short, short?>> rangeMf = MolecularFormula.prepareRangeMF(Options.EmpiricalFormula);
                if (rangeMf != null)
                {
                    //Add a predicate to join to compound elements and include the extracted range mf as where clause.
                    tables.Add("JOIN compounds_elements ce ON ce.cmp_id = c.cmp_id ");
                    foreach (Tuple<string, short, short?> i in rangeMf)
                    {
                        if (i.Item3 != null)
                        {
                            //We have a low and high value for the element.
                            predicates.Add(String.Format("(ce.el_{0} >= {1} and ce.el_{0} <= {2})", i.Item1, i.Item2, i.Item3));
                            //Generate the parsed formula for display.
                            parsedFormula += String.Format("{0}({1}-{2})", i.Item1, i.Item2, i.Item3);
                        }
                        else
                        {
                            //We only have a low value so must be an exact match.
                            predicates.Add(String.Format("(ce.el_{0} = {1})", i.Item1, i.Item2));
                            
                            //Generate the parsed formula for display.
                            if(i.Item2 > 0)
                                parsedFormula += String.Format("{0}{1}", i.Item1, i.Item2);
                        }
                    }
                }
                else
                {
                    //Treat as a standard MF.
                    predicates.Add("c.PUBCHEM_OPENEYE_MF = '" + (!Options.EmpiricalFormula.Contains('{') ? MolecularFormula.prepareMF(Options.EmpiricalFormula) : Options.EmpiricalFormula) + "'");
                    parsedFormula = Regex.Replace(Options.EmpiricalFormula, " ", "");
                }

                visual.Add("MF = '" + parsedFormula + "'");
                bMFAdded = true;
            }

            // Molecular Weight
            if ( Options.MolWeightMin != null ) {
                predicates.Add(string.Format("c.Molecular_Weight >= {0}", Options.MolWeightMin));
                visual.Add(string.Format("MW >= {0}", Options.MolWeightMin));
            }
            if ( Options.MolWeightMax != null ) {
                predicates.Add(string.Format("c.Molecular_Weight <= {0}", Options.MolWeightMax));
                visual.Add(string.Format("MW <= {0}", Options.MolWeightMax));
            }
            double? median = (Options.MolWeightMin + Options.MolWeightMax) / 2;
            if ( median != null && columns.Count == 0 ) {
                string mass_defect = string.Format("abs(Molecular_Weight - {0}) as mass_defect", median);
                columns.Add(mass_defect);
                visual.Add(mass_defect);
                bMassDefectAdded = true;
            }

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

            // Monoisotopic Mass
            if ( Options.MonoisotopicMassMin != null ) {
                predicates.Add(string.Format("ap++.Monoisotopic_Mass >= {0}", Options.MonoisotopicMassMin));
                visual.Add(string.Format("MM >= {0}", ((double)Options.MonoisotopicMassMin).ToString("#0.0#####")));
                bPropsAdded = true;
            }
            if ( Options.MonoisotopicMassMax != null ) {
                predicates.Add(string.Format("ap++.Monoisotopic_Mass <= {0}", Options.MonoisotopicMassMax));
                visual.Add(string.Format("MM <= {0}", ((double)Options.MonoisotopicMassMax).ToString("#0.0#####")));
                bPropsAdded = true;
            }
            median = (Options.MonoisotopicMassMin + Options.MonoisotopicMassMax) / 2;
            if ( median != null && columns.Count == 0 ) {
                string mass_defect = string.Format("abs(Monoisotopic_Mass - {0}) as mass_defect", median);
                columns.Add(mass_defect);
                visual.Add(mass_defect);
                bMassDefectAdded = true;
            }

            if ( bPropsAdded )
                tables.Add("left join v_acdlabs_props ap++ on ap++.cmp_id = c.cmp_id");

            if ( bMassDefectAdded )
                orderby.Add("mass_defect");

            return bMFAdded || bPropsAdded;
        }
    }
}
