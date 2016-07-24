using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using System.Xml.Linq;

using MoleculeObjects;
using RSC.Logging;

namespace RSC.CVSP.Compounds
{
    public class ValidationRuleModule : IValidationRuleModule
    {
        //	TODO: THIS THREE COLLECTIONS AND RELATED FUNCTIONALITY MUST BE REFACTORED!!!
        public List<Tuple<Func<Molecule, bool>, Issue>> FunctionCollection()
        {
            return functionCollection;
        }
        List<Tuple<Func<Molecule,bool>, Issue>> functionCollection;

        /// <summary>
        /// Takes a function which takes a molecule and returns a bool.
        /// </summary>
        Tuple<Func<Molecule, bool>, Issue> MakeAnalysis(Func<Molecule, bool> fn, Issue ci)
        {
            return new Tuple<Func<Molecule, bool>, Issue>(fn, ci);
        }

        private Func<Molecule, bool> MakeFunctionFromXmlTest(XElement x)
        {
            string name = x.Attribute("name").Value;
            string param = x.Attribute("param").Value;
            return Uncurry(unaryFunctionDictionary[name], param);
        }

        public static Func<Molecule, bool> Uncurry(Func<Molecule, string, bool> fn, string param)
        {
            return m => fn(m, param);
        }

        public Dictionary<string, Func<List<Func<Molecule, bool>>, Func<Molecule, bool>>> ops = new Dictionary<string, Func<List<Func<Molecule, bool>>, Func<Molecule, bool>>>()
        {
            {"and", fns => (m => fns.Aggregate(true, (result, fn) => fn(m) && result))},
            {"or", fns => (m => fns.Aggregate(false, (result, fn)=> fn(m) || result))}
        };

        public Tuple<Func<Molecule, bool>, Issue> MakeRuleFromXml(XElement x, string issuecode)
        {
            Issue ci = new Issue()
            {
                Code = issuecode,
                Message = x.Attribute("message").Value
            };

            var op = x.XPathSelectElement("or|and");
            if (op != null)
            {
                var tests = op.XPathSelectElements("test");
                return MakeAnalysis(ops[op.Name.LocalName]((from t in tests select MakeFunctionFromXmlTest(t)).ToList()), ci);
            }
            else
            {
                var test = x.XPathSelectElement("test");
                return MakeAnalysis(MakeFunctionFromXmlTest(test), ci);
            }
        }

        Dictionary<string, Func<Molecule, string, bool>> unaryFunctionDictionary = new Dictionary<string, Func<Molecule, string, bool>>()
        {
            {"SMILEStest", (m,s) => m.FirstProperty("SMILES") == s},
            {"SMARTStest", (m,s) => m.Match(s,MoleculeObjects.Toolkit.OpenEye)}
        };

        public ValidationRuleModule(string xmlfileContent)
        {
            // RSC.CVSP.Compounds.Properties.Resources.validation;
            XElement x = XElement.Parse(xmlfileContent, LoadOptions.None);
            var moleculerules = x.XPathSelectElements("//moleculerules/*").ToList();
            functionCollection = new List<Tuple<Func<Molecule, bool>, Issue>>();
            foreach (var m in moleculerules.Where(y => y.Name == "Warning"))
                functionCollection.Add(MakeRuleFromXml(m, "100.71"));

            foreach (var m in moleculerules.Where(y => y.Name == "Information"))
                functionCollection.Add(MakeRuleFromXml(m, "100.70"));

            foreach (var m in moleculerules.Where(y => y.Name == "Error"))
                functionCollection.Add(MakeRuleFromXml(m, "100.72"));
        }
    }
}
