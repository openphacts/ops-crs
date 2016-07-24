using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using RSC.CVSP.Compounds.com.chemspider.www.PropertiesImport;
using RSC.CVSP.Compounds.com.chemspider.www.Synonyms;
using RSC.CVSP;
using RSC.Collections;
using RSC.Compounds;
using RSC.Properties;

namespace RSC.CVSP.Compounds.Operations
{
    public class ImportChemSpiderProperties : OperationBase
    {
        public override IEnumerable<Record> Process(IEnumerable<Record> records, IDictionary<string, object> options = null)
        {
            //	build list of InChIKeys...
            var inchiKeys = records.Cast<CompoundRecord>()
                .Where(c => c != null && !string.IsNullOrEmpty(c.Standardized))
                .Where(c => c.StandardizedStdInChI != null)
                .Select(c => c.StandardizedStdInChI.InChIKey)
                .ToList();

            //Call the Properties.asmx ChemSpider Web Service.
            var ws_properties = new RSC.CVSP.Compounds.com.chemspider.www.PropertiesImport.Properties()
            {
                Timeout = ConfigurationManager.AppSettings.GetInt("ws_timeout", 30000)
            };

            List<PropertiesInfo> results = null;

            //Get the number of retries from the config file.
            int ws_retries = ConfigurationManager.AppSettings.GetInt("ws_retries", 50);
            bool ws_success = false;

            //Keep retrying the web service until we have a success or number of retries exceeded.
            while (!ws_success)
            {
                try
                {
                    results = ws_properties.RetrieveByInChIKeyList(inchiKeys.ToArray(), ConfigurationManager.AppSettings["security_token"]).ToList();
                    ws_success = true;
                }
                catch (Exception ex)
                {
                    ws_retries--;
                    if (ws_retries == 0)
                        throw ex; //We have exceeded the retries count so throw this error.
                }
            }

            //Populate the Properties with the ChemSpider web service results.
            foreach (var compound in records.Cast<CompoundRecord>().Where(c => c != null && c.StandardizedStdInChI != null && !string.IsNullOrEmpty(c.Standardized)))
            {
                PropertiesInfo prop = results.Find(i => i.InChIKey == compound.StandardizedStdInChI.InChIKey);
                if (prop != null)
                {
                    compound.AddChemSpiderProperty(PropertyName.CSID, prop.CSID);
					compound.AddChemSpiderProperty(PropertyName.DENSITY, prop.Density, prop.DensityError);
					compound.AddChemSpiderProperty(PropertyName.REFRACTION_INDEX, prop.IndexOfRefraction, prop.IndexOfRefractionError);
                    compound.AddChemSpiderProperty(PropertyName.MOLAR_VOLUME, prop.MolarVolume, prop.MolarVolumeError);
					compound.AddChemSpiderProperty(PropertyName.POLARIZABILITY, prop.Polarizability, prop.PolarizabilityError);
					compound.AddChemSpiderProperty(PropertyName.SURFACE_TENSION, prop.SurfaceTension, prop.SurfaceTensionError);
					compound.AddChemSpiderProperty(PropertyName.ENTHALPY_OF_VAPORIZATION, prop.Enthalpy, prop.EnthalpyError);
					compound.AddChemSpiderProperty(PropertyName.FLASH_POINT, prop.FP, prop.FPError);
					compound.AddChemSpiderProperty(PropertyName.BOILING_POINT, prop.BP, prop.BPError);
					compound.AddChemSpiderProperty(PropertyName.VAPOUR_PRESSURE, prop.VP, prop.VPError);
					compound.AddChemSpiderProperty(PropertyName.LOG_P, prop.LogP, prop.LogPError);
					compound.AddChemSpiderProperty(PropertyName.MOLAR_REFRACTIVITY, prop.MolarRefactivity, prop.MolarRefactivityError);
					compound.AddChemSpiderProperty(PropertyName.AVERAGE_MASS, prop.AverageMass);
					compound.AddChemSpiderProperty(PropertyName.NOMINAL_MASS, prop.NominalMass);
					compound.AddChemSpiderProperty(PropertyName.MONOISOTOPIC_MASS, prop.MonoisotopicMass);
					compound.AddChemSpiderProperty(PropertyName.RULE_OF_5_VIOLATIONS, prop.RuleOf5);
					compound.AddChemSpiderProperty(PropertyName.H_BOND_DONORS, prop.RuleOf5HDonors);
					compound.AddChemSpiderProperty(PropertyName.H_BOND_ACCEPTORS, prop.RuleOf5HAcceptors);
					compound.AddChemSpiderProperty(PropertyName.FREELY_ROTATING_BONDS, prop.RuleOf5FRB);
					compound.AddChemSpiderProperty(PropertyName.MOLECULAR_WEIGHT, prop.RuleOf5MW);
					compound.AddChemSpiderProperty(PropertyName.POLAR_SURFACE_AREA, prop.RuleOf5PSA);

					compound.AddChemSpiderProperty(PropertyName.LOG_D, prop.LogD1, new Condition() { Name = PropertyName.PH, Value = 5.5 });
					compound.AddChemSpiderProperty(PropertyName.LOG_D, prop.LogD2, new Condition() { Name = PropertyName.PH, Value = 7.4 });
					compound.AddChemSpiderProperty(PropertyName.BCF, prop.BCF1, new Condition() { Name = PropertyName.PH, Value = 5.5 });
					compound.AddChemSpiderProperty(PropertyName.BCF, prop.BCF2, new Condition() { Name = PropertyName.PH, Value = 7.4 });
					compound.AddChemSpiderProperty(PropertyName.KOC, prop.KOC1, new Condition() { Name = PropertyName.PH, Value = 5.5 });
					compound.AddChemSpiderProperty(PropertyName.KOC, prop.KOC2, new Condition() { Name = PropertyName.PH, Value = 7.4 });

					/*
							double ALogP;
							double XLogP;

							double parachor;
							double parachorError;
					*/
                }
            }

            return records;
        }

        public override IEnumerable<OperationInfo> GetOperations()
        {
            return new List<OperationInfo>() {
				new OperationInfo() {
					Id = "ChemSpiderPropertiesImport",
					Name = "Import ChemSpider Properties",
					Description = "Import ChemSpider Properties"
				}
			};
        }
    }
}
