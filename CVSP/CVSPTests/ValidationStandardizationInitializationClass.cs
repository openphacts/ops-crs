using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ChemValidator;
using ChemValidatorLib;


namespace ChemValidatorTests
{
    public class ValidationStandardizationInitializationClass
    {
        public static string StdRulesXMLFilePath;
        public static string AcidBaseRulesXMLFilePath;
        public static string ValidationRulesXMLFilePath;
        public static Standardization standardization;
        public static Validation validation;
        public static Acidity acidity;
        static ValidationStandardizationInitializationClass()
        {
            //StdRulesXMLFilePath = createChemValidatorLibXmlFile(Resource1.StandardizationRules);
            //StdRulesXMLFilePath = createChemValidatorLibXmlFile("StandardizationRules.xml");
            StdRulesXMLFilePath = "StandardizationRules.xml";
            //AcidBaseRulesXMLFilePath = createChemValidatorLibXmlFile(Resource1.acidgroups);
            //AcidBaseRulesXMLFilePath = createChemValidatorLibXmlFile("acidgroups.xml");
            AcidBaseRulesXMLFilePath = "acidgroups.xml";
            //ValidationRulesXMLFilePath = createChemValidatorLibXmlFile(Resource1.ValidationRules);
            //ValidationRulesXMLFilePath = createChemValidatorLibXmlFile("ValidationRules.xml");
            ValidationRulesXMLFilePath = "ValidationRules.xml";
            acidity = new Acidity(AcidBaseRulesXMLFilePath);
            standardization = new Standardization(StdRulesXMLFilePath, acidity);
            validation = new Validation(ValidationRulesXMLFilePath, acidity);
        }

        static string createChemValidatorLibXmlFile(string res)
        {
            string filename = Path.GetTempFileName();
            using (FileStream fs_xml = File.Create(filename))
            {
                byte[] cont = System.Text.Encoding.ASCII.GetBytes(res);
                fs_xml.Write(cont,0,cont.Length);
            }
            return filename;
        }
    }
}
