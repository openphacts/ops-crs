using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChemSpider.Search
{
    public class CSPredictedPropertiesSearch : CSSqlSearch
    {
        public new PredictedPropertiesSearchOptions Options
        {
            get { return base.Options as PredictedPropertiesSearchOptions; }
        }

        protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
        {
            bool bAdded = false;

            tables.Add("left join v_acdlabs_props ap++ on ap++.cmp_id = c.cmp_id");

            // LogP
            if ( Options.LogPMin != null ) {
                predicates.Add("ap++.LogP >= " + Options.LogPMin);
                visual.Add("LogP >= " + Options.LogPMin);
                bAdded = true;
            }
            if ( Options.LogPMax != null ) {
                predicates.Add("ap++.LogP <= " + Options.LogPMax);
                visual.Add("LogP <= " + Options.LogPMax);
                bAdded = true;
            }

            // LogD 5.5
            if ( Options.LogD55Min != null ) {
                predicates.Add("ap++.LogD_1 >= " + Options.LogD55Min);
                visual.Add("LogD_1 >= " + Options.LogD55Min);
                bAdded = true;
            }

            if ( Options.LogD55Max != null ) {
                predicates.Add("ap++.LogD_1 <= " + Options.LogD55Min);
                visual.Add("LogD_1 <= " + Options.LogD55Min);
                bAdded = true;
            }

            // LogD 7.4
            if ( Options.LogD74Min != null ) {
                predicates.Add("ap++.LogD_2 >= " + Options.LogD74Min);
                visual.Add("LogD_2 >= " + Options.LogD74Min);
                bAdded = true;
            }
            if ( Options.LogD74Max != null ) {
                predicates.Add("ap++.LogD_2 <= " + Options.LogD74Max);
                visual.Add("LogD_2 <= " + Options.LogD74Max);
                bAdded = true;
            }

            // Rule of 5
            if ( Options.RuleOf5Min != null ) {
                predicates.Add("ap++.RuleOf5 >= " + Options.RuleOf5Min);
                visual.Add("RuleOf5 >= " + Options.RuleOf5Min);
                bAdded = true;
            }
            if ( Options.RuleOf5Max != null ) {
                predicates.Add("ap++.RuleOf5 <= " + Options.RuleOf5Max);
                visual.Add("RuleOf5 <= " + Options.RuleOf5Max);
                bAdded = true;
            }

            // H Acceptors
            if ( Options.HAcceptorsMin != null ) {
                predicates.Add("ap++.RuleOf5_HAcceptors >= " + Options.HAcceptorsMin);
                visual.Add("RuleOf5_HAcceptors >= " + Options.HAcceptorsMin);
                bAdded = true;
            }
            if ( Options.HAcceptorsMax != null ) {
                predicates.Add("ap++.RuleOf5_HAcceptors <= " + Options.HAcceptorsMax);
                visual.Add("RuleOf5_HAcceptors <= " + Options.HAcceptorsMax);
                bAdded = true;
            }

            // H Donors
            if ( Options.HDonorsMin != null ) {
                predicates.Add("ap++.RuleOf5_HDonors >= " + Options.HDonorsMin);
                visual.Add("RuleOf5_HDonors >= " + Options.HDonorsMin);
                bAdded = true;
            }
            if ( Options.HDonorsMax != null ) {
                predicates.Add("ap++.RuleOf5_HDonors <= " + Options.HDonorsMax);
                visual.Add("RuleOf5_HDonors <= " + Options.HDonorsMax);
                bAdded = true;
            }

            // Freely rotatable bonds
            if ( Options.FreelyRotatableBondsMin != null ) {
                predicates.Add("ap++.RuleOf5_FRB >= " + Options.FreelyRotatableBondsMin);
                visual.Add("RuleOf5_FRB >= " + Options.FreelyRotatableBondsMin);
                bAdded = true;
            }
            if ( Options.FreelyRotatableBondsMax != null ) {
                predicates.Add("ap++.RuleOf5_FRB <= " + Options.FreelyRotatableBondsMax);
                visual.Add("RuleOf5_FRB <= " + Options.FreelyRotatableBondsMax);
                bAdded = true;
            }

            // Polar surface area
            if ( Options.PolarSurfaceAreaMin != null ) {
                predicates.Add("ap++.RuleOf5_PSA >= " + Options.PolarSurfaceAreaMin);
                visual.Add("RuleOf5_PSA >= " + Options.PolarSurfaceAreaMin);
                bAdded = true;
            }
            if ( Options.PolarSurfaceAreaMax != null ) {
                predicates.Add("ap++.RuleOf5_PSA <= " + Options.PolarSurfaceAreaMax);
                visual.Add("RuleOf5_PSA <= " + Options.PolarSurfaceAreaMax);
                bAdded = true;
            }

            // Molar volume
            if ( Options.MolarVolumeMin != null ) {
                predicates.Add("ap++.Molar_Volume >= " + Options.MolarVolumeMin);
                visual.Add("Molar_Volume >= " + Options.MolarVolumeMin);
                bAdded = true;
            }
            if ( Options.MolarVolumeMax != null ) {
                predicates.Add("ap++.Molar_Volume <= " + Options.MolarVolumeMax);
                visual.Add("Molar_Volume <= " + Options.MolarVolumeMax);
                bAdded = true;
            }

            // Refractivity index
            if ( Options.RefractiveIndexMin != null ) {
                predicates.Add("ap++.Index_Of_Refraction >= " + Options.RefractiveIndexMin);
                visual.Add("Index_Of_Refraction >= " + Options.RefractiveIndexMin);
                bAdded = true;
            }
            if ( Options.RefractiveIndexMax != null ) {
                predicates.Add("ap++.Index_of_Refraction <= " + Options.RefractiveIndexMax);
                visual.Add("Index of Refraction <= " + Options.RefractiveIndexMax);
                bAdded = true;
            }

            // Boiling point
            if ( Options.BoilingPointMin != null ) {
                predicates.Add("ap++.BP >= " + Options.BoilingPointMin);
                visual.Add("Boiling Point >= " + Options.BoilingPointMin);
                bAdded = true;
            }
            if ( Options.BoilingPointMax != null ) {
                predicates.Add("ap++.BP <= " + Options.BoilingPointMax);
                visual.Add("Boiling Point <= " + Options.BoilingPointMax);
                bAdded = true;
            }

            // Flash point
            if ( Options.FlashPointMin != null ) {
                predicates.Add("ap++.FP >= " + Options.FlashPointMin);
                visual.Add("Flash Point >= " + Options.FlashPointMin);
                bAdded = true;
            }
            if ( Options.FlashPointMax != null ) {
                predicates.Add("ap++.FP <= " + Options.FlashPointMax);
                visual.Add("Flash Point <= " + Options.FlashPointMax);
                bAdded = true;
            }

            // Density
            if ( Options.DensityMin != null ) {
                predicates.Add("ap++.Density >= " + Options.DensityMin);
                visual.Add("Density >= " + Options.DensityMin);
                bAdded = true;
            }
            if ( Options.DensityMax != null ) {
                predicates.Add("ap++.Density <= " + Options.DensityMax);
                visual.Add("Density <= " + Options.DensityMax);
                bAdded = true;
            }

            // Surface tension
            if ( Options.SurfaceTensionMin != null ) {
                predicates.Add("ap++.Surface_Tension >= " + Options.SurfaceTensionMin);
                visual.Add("Surface Tension >= " + Options.SurfaceTensionMin);
                bAdded = true;
            }
            if ( Options.SurfaceTensionMax != null ) {
                predicates.Add("ap++.Surface_Tension <= " + Options.SurfaceTensionMax);
                visual.Add("Surface Tension <= " + Options.SurfaceTensionMax);
                bAdded = true;
            }

            return bAdded;
        }
    }
}
