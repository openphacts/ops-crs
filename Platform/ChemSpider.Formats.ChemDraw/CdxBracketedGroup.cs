using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MoleculeObjects
{
    public class CdxBracketedGroup : CdxObject
    {
        private List<int> m_bracketedobjectIDs = new List<int>();

        /// <summary>
        /// Bond ID, inner atom, outer atom. In that order.
        /// 
        /// The reason for using a bond ID rather than the bond itself is that we create a copy of the fragment.
        /// </summary>
        private List<Tuple<int, int, int>> m_xingBonds = new List<Tuple<int, int, int>>();
        private Dictionary<CdxObject,string> m_bracketGraphicsWithText = new Dictionary<CdxObject,string>();
        private Tuple<double, double> m_bottomright;
        private string m_count;

        public List<int> BracketedObjectIDs { get { return m_bracketedobjectIDs; } }
        public Tuple<double, double> BottomRight { get { return m_bottomright; } }
        public string Count { get { return m_count; } }

        private static string numberPattern = @"^\d+$";

        /// <summary>
        /// n is the number of times the bracketed sequence should be repeated.
        /// </summary>
        private CdxFragment ExpandBracket(CdxFragment f, int n, IEnumerable<int> atomIds)
        {
            var expansion = new Dictionary<int, Func<CdxFragment, int, IEnumerable<int>, CdxFragment>>() {
                { 0, (a,b,c) => Macrocycle(a,b,c) },
                { 1, (a,b,c) => ExpandOneEndedBracket(a,b,c) },  
                { 2, (a,b,c) => ExpandTwoEndedBracket(a,b,c) }
            };
            return expansion[m_xingBonds.Count](f, n, atomIds);
        }

        private CdxFragment Macrocycle(CdxFragment f, int n, IEnumerable<int> atomIDs)
        {
            if (n > 2)
            {
                CdxFragment resultFragment = new CdxFragment(f,
                    String.Format("Expanded brackets to form {0}-membered macrocycle.", n));
                int startID = (from a in resultFragment.IndexedAtoms select a.Key).Max() + 1;
                var outerAtomIDs = (from a in resultFragment.IndexedAtoms.Where(a => a.Value.IsECP) select a.Key);
                if (outerAtomIDs.Count() != 2)
                    throw new MoleculeException("wrong number of outer atom IDs for this macrocycle");
                var xBond1 = resultFragment.IndexedBonds.Values.Where(b => b.firstCdxAtom == outerAtomIDs.First() || b.secondCdxAtom == outerAtomIDs.First()).First();
                var xBond2 = resultFragment.IndexedBonds.Values.Where(b => b.firstCdxAtom == outerAtomIDs.ElementAt(1) || b.secondCdxAtom == outerAtomIDs.ElementAt(1)).First();
                int innerAtomID1 = xBond1.OtherEnd(outerAtomIDs.First());
                int innerAtomID2 = xBond2.OtherEnd(outerAtomIDs.ElementAt(1));
                List<int> starts = new List<int>();
                List<int> ends = new List<int>();
                for (int i = 2; i <= n; i++)
                {
                    var oldNewAtomMapping = resultFragment.DuplicateBracketedSection(atomIDs.Where(a => !outerAtomIDs.Contains(a)));
                    starts.Add(oldNewAtomMapping[innerAtomID1]);
                    ends.Add(oldNewAtomMapping[innerAtomID2]);
                }
                // whatever happens connect end to beginning.
                resultFragment.AddBond(new CdxBond(ends.Last(), innerAtomID1, xBond1.bondOrder, xBond1.ctBondStereo));
                resultFragment.AddBond(new CdxBond(innerAtomID2, starts.First(), xBond1.bondOrder, xBond1.ctBondStereo));
                int j = 0;
                foreach (int end in ends.Where(e => e != ends.Last()))
                {
                    resultFragment.AddBond(new CdxBond(end, starts[j + 1], xBond1.bondOrder, xBond1.ctBondStereo));
                    j++;
                }
                resultFragment.IndexedBonds.Remove(xBond1.ID);
                resultFragment.IndexedBonds.Remove(xBond2.ID);
                resultFragment.RemoveAtom(outerAtomIDs.First());
                resultFragment.RemoveAtom(outerAtomIDs.ElementAt(1));
                return resultFragment;
            }
            else
            {
                return f;
            }
        }

        private CdxFragment ExpandOneEndedBracket(CdxFragment f, int n, IEnumerable<int> atomIDs)
        {
            CdxFragment resultFragment = new CdxFragment(f,
                String.Format("Expanded one-ended bracket to form {0} substituents", n));

            switch (n)
            {
                case 0:
                    throw new NotImplementedException("haven't implemented one-ended bracket to be repeated 0 times");
                case 1:
                    break; // do nothing as it's a single repeat.
                default:
                    int newAtomID = (from a in resultFragment.IndexedAtoms select a.Key).Max() + 1;
                    var xingBond = m_xingBonds.First();
                    CdxBond xBond = resultFragment.IndexedBonds.Where(b => b.Key == xingBond.Item1).First().Value;
                    int innerAtomID = xingBond.Item2;
                    int outerAtomID = xingBond.Item3;
                    for (int i = 2; i <= n; i++)
                    {
                        var oldNewAtomMapping = resultFragment.DuplicateBracketedSection(atomIDs);
                        resultFragment.AddBond(new CdxBond(oldNewAtomMapping[innerAtomID], outerAtomID, xBond.bondOrder, xBond.ctBondStereo));
                    }
                    break;
            }
            return resultFragment;
        }

        /// <summary>
        /// Do the donkey work of enumeration.  For now, simply put all atoms on top
        /// of one another and rely on the cleaning to do the job at the far end.
        /// </summary>
        private CdxFragment ExpandTwoEndedBracket(CdxFragment f, int n, IEnumerable<int> bracketedAtomIDs)
        {
            CdxFragment rf = new CdxFragment(f, String.Format("Expanded two-ended bracket with {0} repeats", n));
            var xingBondMap1 = m_xingBonds.First();
            var xingBondMap2 = m_xingBonds.ElementAt(1);
            CdxBond xBond1 = rf.IndexedBonds[xingBondMap1.Item1];
            CdxBond xBond2 = rf.IndexedBonds[xingBondMap2.Item1];
            if (xBond1.bondOrder != xBond2.bondOrder) throw new MoleculeException("non-matching bond types on either side of bracket");
                
            int innerAtomID1 = xingBondMap1.Item2;
            int innerAtomID2 = xingBondMap2.Item2;
            int outerAtomID1 = xingBondMap1.Item3;
            int outerAtomID2 = xingBondMap2.Item3;

            switch (n)
            {
                case 0:
                    // remove atomIDs and all bonds to them from fragment
                    var delenda = (from a in rf.IndexedAtoms where bracketedAtomIDs.Contains(a.Key) select a).ToList();
                    foreach (var a in delenda)
                    {
                        var bondDelenda = rf.IndexedBonds.Where(b => b.Value.EitherAtomIDEquals(a.Key)).ToList();
                        bondDelenda.ForEach(b => rf.IndexedBonds.Remove(b.Key));
                    }
                    delenda.ForEach(a => rf.RemoveAtom(a.Key));
                    // now bond the attachment points at either end to each other
                    rf.AddBond(new CdxBond(outerAtomID1, outerAtomID2, xBond1.bondOrder, xBond1.ctBondStereo));
                    break;
                case 1:
                    break; //do nothing
                default:
                    var starts = new List<int>();
                    var ends = new List<int>();
                    for (int i = 2; i <= n; i++)
                    {
                        var oldNewAtomMapping = rf.DuplicateBracketedSection(bracketedAtomIDs);
                        starts.Add(oldNewAtomMapping[innerAtomID1]);
                        ends.Add(oldNewAtomMapping[innerAtomID2]);
                    }
                    rf.AddBond(new CdxBond(innerAtomID2, starts.First(), xBond2.bondOrder, xBond2.ctBondStereo));
                    rf.AddBond(new CdxBond(ends.Last(), outerAtomID2, xBond2.bondOrder, xBond2.ctBondStereo));
                    // deal with connecting bonds
                    int j = 0;
                    foreach (int end in ends.Where(e => e != ends.Last()))
                    {
                        rf.AddBond(new CdxBond(end, starts[j + 1], xBond2.bondOrder, xBond2.ctBondStereo));
                        j++;
                    }

                    // remove original bond at end
 
                    List<int> closingBondIDs = (from b in rf.IndexedBonds
                                                  where (b.Value.firstCdxAtom == xBond2.firstCdxAtom && b.Value.secondCdxAtom == xBond2.secondCdxAtom
                                                        || b.Value.secondCdxAtom == xBond2.firstCdxAtom && b.Value.firstCdxAtom == xBond2.firstCdxAtom)
                                                  select b.Key).ToList();
                    closingBondIDs.ForEach(b => rf.IndexedBonds.Remove(b));
                    break;
            }

            return rf;
        }

        /// <summary>
        /// The label has already been set as a member of the fragment. If the bracket multiplier is numeric, simply
        /// expand the bracketed group. If it is a variable, determine its values and only preserve those labels
        /// that contain enumerations that contain that multiplier.
        /// </summary>
        public List<CdxFragment> FragmentsByExpandedBrackets(CdxFragment f)
        {
            List<CdxFragment> result = new List<CdxFragment>();
            IEnumerable<int> intersection = m_bracketedobjectIDs.Intersect(from a in f.IndexedAtoms select a.Key);

            if (intersection.Count() == 0 || m_count == null)
            {
                return new List<CdxFragment>() { f };
            }
            // we have two cases, depending on m_count
            Match numberMatch = Regex.Match(m_count, numberPattern);
            if (numberMatch.Success)
            {
                int n = Convert.ToInt32(m_count);
                result.Add(ExpandBracket(f, n, intersection));
            }
            else if (String.Concat(f.Labels).Contains(m_count)) // if any of the labels contain it
            {
                string listPattern = m_count + @"\s?=\s?(?<ints>\d+(,\d+\s?)*)";
                var labelNumberMapping = new Dictionary<string, List<int>>();
                foreach (string label in f.Labels.Distinct().Where(l => l.Contains(m_count) && l.Contains("=")))
                {
                    Match listmatch = Regex.Match(label, listPattern, RegexOptions.ExplicitCapture);
                    if (listmatch.Success)
                    {
                        labelNumberMapping.Add(label, (from l in listmatch.Groups["ints"].Value.Split(',').ToList() select Convert.ToInt32(l.Trim())).ToList());
                    }
                    else
                    {
                        throw new MoleculeException("Unimplemented pattern for label: " + label);
                    }
                }
                foreach (string label in f.Labels.Distinct().Where(l => !l.Contains(m_count) || !l.Contains("=")))
                {
                    labelNumberMapping.Add(label, new List<int>());
                }
                var counts = new List<int>();
                foreach (var pair in labelNumberMapping)
                {
                    counts.AddRange(pair.Value);
                }
                foreach (int count in counts.Distinct())
                {
                    CdxFragment newF = ExpandBracket(f, count, intersection);
                    string fixedvariable = String.Format("{0} = {1}", m_count, count);
                    List<string> unaffectedLabels = (from p in labelNumberMapping where !p.Key.Contains(m_count) select p.Key).ToList();
                    List<string> affectedLabels = (from p in labelNumberMapping where p.Key.Contains(m_count) && p.Value.Contains(count) select p.Key).ToList();
                    List<string> trimmedLabels = (from a in affectedLabels select Regex.Replace(a, listPattern, "")).ToList();

                    List<string> newLabels = new List<string>(unaffectedLabels);
                    newLabels.AddRange(trimmedLabels);

                    newF.SetLabels(newLabels);
                    result.Add(newF);
                }
            }
            else
            {
                throw new NotImplementedException("haven't worked out how to interpret count expression '" + m_count + "' yet");
            }
            return result;
        }

        /// <summary>
        /// Iterates up the tree to find a matching graphic.
        /// If it fails then the cdx file is ill-formed.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        private CdxObject GraphicByID(CdxObject e, int ID)
        {
            List<CdxObject> graphics = e.Parent.CdxPathSelectObjects("//Graphic[@id=" + ID + "]");
            return (graphics.Count == 0) ? GraphicByID(e.Parent, ID) : graphics.First();
        }

        private void Initialize(CdxObject e, CdxDashedBondOptions cdbo)
        {
            List<CdxObject> bracketAttachments = e.CdxPathSelectObjects("/BracketAttachment");
            if (bracketAttachments.Count > 4)
            {
                throw new MoleculeException(String.Format("BracketedGroup ({0}) has more than four attachments", ID));
            }
            if (HasProperty("BracketedObjects"))
            {
                m_bottomright = new Tuple<double, double>(0, 0);
                m_bracketedobjectIDs = Property("BracketedObjects");
                foreach (CdxObject a in bracketAttachments)
                {
                    int gid = a.Property("Bracket_GraphicID");
                    CdxObject graphic = GraphicByID(e, gid);
                    if (graphic.CdxPathSelectObjects("/ObjectTag/Text").Count > 0)
                    {
                        m_bracketGraphicsWithText.Add(graphic, graphic.CdxPathSelectObjects("/ObjectTag/Text").First().Property("Text").Item1);
                    }
                    Tuple<double, double> graphicBR = graphic.Property("BoundingBox").Item2;
                    if (graphicBR.Item2 >= m_bottomright.Item1 && graphicBR.Item1 >= m_bottomright.Item2)
                    {
                        m_bottomright = new Tuple<double, double>(graphicBR.Item2, graphicBR.Item1);
                    }
                    List<CdxObject> crossingBonds = a.CdxPathSelectObjects("/CrossingBond", 0).ToList();
                    if (crossingBonds.Count > 2) throw new MoleculeException("More than two crossing bonds in attachment " + a.ID);
                    foreach (CdxObject cb in crossingBonds)
                    {
                        int bondID = cb.Property("Bracket_BondID");
                        int innerAtomID = cb.Property("InnerAtomID");
                        CdxBond b = new CdxBond(e.Parent.CdxPathSelectObjects("//Bond[@id=" + bondID + "]").First(), cdbo);
                        if (b.IsChemicalBond)
                        {
                            int outerAtomID = b.OtherEnd(innerAtomID);
                            m_xingBonds.Add(new Tuple<int, int, int>(bondID, innerAtomID, outerAtomID));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// overrides object-internal bracket count
        /// </summary>
        public CdxBracketedGroup(CdxObject e, string count, CdxDashedBondOptions cdbo = CdxDashedBondOptions.Ignore)
            : base(e)
        {
            m_count = count;
            Initialize(e, cdbo);
        }

        /// <summary>
        /// determines bracket count from object
        /// </summary>
        public CdxBracketedGroup(CdxObject e, CdxDashedBondOptions cdbo = CdxDashedBondOptions.Ignore)
            : base(e)
        {
            Initialize(e, cdbo);
            bool RepeatCountAsProperty = false;
            if (HasProperty("Bracket_RepeatCount"))
            {
                m_count = Property("Bracket_RepeatCount").ToString();
                RepeatCountAsProperty = m_count.Length > 0;
            }

            if ((!RepeatCountAsProperty || m_count == "2") && m_bracketGraphicsWithText.Count > 0)
            {
                m_count = m_bracketGraphicsWithText.First().Value;
            }
        }
    }
}                   
