using MoleculeObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RSC.CVSP.Compounds
{
    public class InChIAnalyzer
    {
        private AnalysedInChI ai;

        /// <summary>
        /// takes inchi string and checks for mobile hydrogens
        /// </summary>
        public static bool MobileHydrogenPresent(string inchi)
        {
            return inchi.Contains("(H");
        }

        public InChIAnalyzer(string inchi)
        {
            ai = new AnalysedInChI(inchi);
        }

        private string capup(string s)
        {
           return s.Substring(0,1).ToUpper() + s.Substring(1);
        }
        
        private string exists(int n)
        {
            Dictionary<int,string> existStrings = new Dictionary<int,string>(){
                {0, " are no"},
                {1, " is a single"},
            };
            return "there " + (existStrings.ContainsKey(n) ? existStrings[n]
                : String.Format(" are {0}", n));
        }

        private string ordinal(int n)
        {
            return n == 1 ? "first" : n ==2 ? "second" : n == 3 ? "third" : (n + "th");
        }

        private string plural(int n, string s)
        {
            return n == 1 ? s : pluralize(s);
        }

        private string pluralize(string s)
        {
            return Regex.IsMatch("y$", s) ? s.Substring(0, s.Length - 1) + "ies"
                : Regex.IsMatch("[bdfghklmnprtw]$", s) ? s + "s"
                : Regex.IsMatch("[csxz]$", s) ? s + "es"
                : s;
        }

        private string fixedHlayer(string s, Dictionary<int,string> atomtypes)
        {
            // hydrogen layers need to explode on H first!
            string result = string.Empty;
            char[] H = new char[]{ 'H' };
            char[] comma = new char[] {','};
            if (s.Length > 0)
            {
                result += ". It has a fixed hydrogen layer, with ";
                var fatoms = s.Split(comma);
                foreach (string fatom in fatoms)
                {
                    var fato = fatom.Split(H);
                    string fatomindex = fato[0];
                    string fatomcount = (fato.Count() > 1) ? fato[1] : "1";
                    result += String.Format("{0} hydrogen atoms on atom {1}, which is a {2} atom",
                        fatomcount, fatomindex, atomtypes[Convert.ToInt32(fatomindex)]);
                }
            }
            return result;
        }

        char[] stop = new char[] { '.' };
        char[] semicolon = new char[] {';'};
        char[] comma = new char[] { ',' };

        public string Explanation()
        {
            Regex initialnumbers = new Regex("^(?<count>[0-9]+)");
            string result = string.Empty;
            var rawcompositions = ai.MainLayer.Split(stop);
            List<string> compositions = new List<string>();
            foreach (var rawcomposition in rawcompositions)
            {
                if (initialnumbers.IsMatch(rawcomposition))                   
                {
                    int count = Convert.ToInt32(initialnumbers.Match(rawcomposition).Groups["count"].Value);
                    for (int i = 0; i < count - 1; i++)
                    {
                        compositions.Add(rawcomposition);
                    }
                }
                                    else
                    {
                        compositions.Add(rawcomposition);
                }
            }
            var charges = ai.ChargeAndProtonLayers.Split(semicolon);
            var fixedHs = ai.FixedHLayer.Split(semicolon);
            var stereos = ai.StereoLayer.Split(semicolon);
            var isotopes = ai.IsotopicLayer.Split(semicolon);

            int fcount = ai.FixedHLayer.Length > 0 ? fixedHs.Count() + 1 : 0;
            int scount = ai.StereoLayer.Length > 0 ? stereos.Count() + 1 : 0;
            int icount = ai.IsotopicLayer.Length > 0 ? isotopes.Count() + 1: 0;
            int qcount = ai.ChargeAndProtonLayers.Length > 0 ? charges.Count() + 1 : 0;

            result += String.Format("This InChI describes the system {0}", ai.Formula);
            result += capup(exists(compositions.Count)) + " " + plural(compositions.Count, "moiety");
            var formulaexplanation = FormulaExplanation(ai.Formula);
            var atomtypes = formulaexplanation.Item1;
            string f = formulaexplanation.Item2;
            int ic = 1;
            foreach (string c in compositions)
            {
                result += " In the " + (compositions.Count > 1 ? ordinal(ic) + " " : "") + "moiety, ";

                result += (qcount > 1) ? String.Format(". The charge on moiety {0} is {1}", ic, charges[ic - 1]) : "";

                result += stereoExplanation(stereos[ic-1], atomtypes);
                string ftext = (fcount > 1) ? fixedHs[ic -1]: "";
                string itext = (icount > 1) ?isotopes[ic -1]: "";

                ic++;
            }

        
            result += ".";
            return result;
        }

        char[] D = new char[]{'D'};

        private string stereoExplanation(string s, Dictionary<string,string> atomtypes)
        {
            return (s.Length == 0)
                ? ". There are no chiral centres"
                : ". Atoms" + from a in s.Split(comma) select String.Format(" {0} ({1}), ", a, atomtypes[a]) + " are chiral centres";
        }

        private Tuple<Dictionary<string,string>, string> FormulaExplanation(string s)
        {
            Dictionary<string,string> atomTypes = new Dictionary<string,string>();
            string explanation = string.Empty;

            Regex atomAndCountPresence = new Regex("([A-Z][a-z]?[0-9]*)");
            Regex atomAndCountAnalysis = new Regex("(?<atom>[A-Z][a-z]?)(?<count>[0-9]*)");
            MatchCollection mc = atomAndCountPresence.Matches(s);
            explanation += exists(mc.Count) + " " + plural(mc.Count, "sort") + " of atom";
            int startcount = 1;
            foreach (Match m in mc)
            {
                string atom = atomAndCountAnalysis.Match(m.Value).Groups["atom"].Value;
                string count = atomAndCountAnalysis.Match(m.Value).Groups["count"].Value;
                if (atom != "H")
                {
                    if (count == "")
                    {
                        atomTypes.Add(startcount.ToString(), atom);
                        explanation += String.Format(". Atom {0} is an {1} atom", startcount, atom);
                    }
                    else
                    {
                        int endcount = startcount + Convert.ToInt32(count) - 1;
                        for (int i = startcount; i < endcount; i++)
                        {
                            atomTypes.Add(i.ToString(), atom);
                        }
                        explanation += String.Format(". Atoms {0} to {1} are {2} atoms", startcount, endcount, atom);
                        startcount = endcount + 1;
                    }
                }
            }
            return new Tuple<Dictionary<string,string>,string>(atomTypes, explanation);
        }

        private string isotopeExplanation(string s)
        {
            string result = string.Empty;
            var iatoms = s.Split(comma);
            foreach (string iatom in iatoms)
            {
                if (iatom.Contains("D"))
                {
                    var iato = iatom.Split(D);
                    result += String.Format(". There are {0} deuterium atoms on atom {1}", iato[0], iato[1]);
                }
                else
                {
                    Regex iso = new Regex(@"(?<atomindex>[0-9]+)(?<shift>[\+\-][0-9])");
                    if (iso.IsMatch(iatom))
                    {
                        result += String.Format(". Atom {0} has an isotopic shift of {1}", iso.Match(iatom).Groups["atomindex"], iso.Match(iatom).Groups["shift"]);
                    }
                }
            }
            return result;
        }
    }
}