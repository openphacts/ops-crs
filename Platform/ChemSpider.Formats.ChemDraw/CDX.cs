using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ChemSpider.Formats.Accelrys;

namespace MoleculeObjects
{
    /// <summary>
    /// Some sources use dashed bonds to indicate stereochemistry.
    /// </summary>
    public enum CdxDashedBondOptions { Ignore, Down }
    public enum CdxEnumerateOptions { None, EnumerateMarkush }

    /// <summary>
    /// This is just enough to make a human-readable rendering of the cdx file.
    /// It does no processing, so will not fail if processing goes wrong.
    /// Normally you should use the Cdx object instead.
    /// </summary>
    public class RawCdx
    {
        protected CdxObject m_rootObject;

        public CdxObject Root { get { return m_rootObject; } }

        // normal constructor
        public RawCdx(byte[] bytes)
        {
            string magicnumber = String.Concat(from b in bytes.Take(8) select (char)b);
            if (magicnumber.StartsWith("VY")) throw new NotImplementedException("ChemDraw 3.x file");
            if (magicnumber != "VjCD0100") throw new MoleculeException("Not a recognized ChemDraw file");
            m_rootObject = new CdxObject(bytes, 28);
        }

        // copy constructor
        public RawCdx(RawCdx cdx)
        {
            m_rootObject = cdx.Root;
        }
    }

    public class Cdx : RawCdx
    {
        protected CompoundEnumerator ce;
        public List<CdxText> Labels { get; protected set; }
        protected List<CdxFragment> m_fragments = new List<CdxFragment>();
        public List<CdxResinousBlob> ResinousBlobs { get; protected set; }
        protected Dictionary<string, string> commonReagents = new Dictionary<string, string>();

        public IEnumerable<string> LabelsInBox(Tuple<double, double, double, double> box)
        {
            return from l in Labels where box.Contains(l.XY) select l.Value;
        }
        
        /// <summary>
        /// Takes a Tuple&lt;int,int,double&gt; and returns a Dictionary&lt;int,int&gt;.
        /// </summary>
        /// <param name="distances"></param>
        /// <returns></returns>
        private Dictionary<int, int> PairOffByNearest(List<Tuple<int, int, double>> distances)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            if (distances.Count == 0) return result;
            Tuple<int, int, double> pairing = distances.Where(t => t.Item3 == distances.Min(u => u.Item3)).First();
            List<Tuple<int, int, double>> remnant = distances.Where(t => t.Item1 != pairing.Item1).Where(t => t.Item2 != pairing.Item2).ToList();
            result.Add(pairing.Item1, pairing.Item2);
            foreach (var pair in PairOffByNearest(remnant)) result.Add(pair.Key, pair.Value);
            return result;
        }
        
        /// <summary>
        /// Takes list of fragments and list of bracketedgroups and merges them together
        /// </summary>
        private IEnumerable<CdxFragment> ExpandBrackets(IEnumerable<CdxFragment> ff, IEnumerable<CdxBracketedGroup> bb)
        {
            return bb.Any() 
                ? ExpandBrackets((from f in ff select bb.First().FragmentsByExpandedBrackets(f)).SelectMany(l => l), bb.Skip(1))
                : ff;
        }

        public List<CdxFragment> FragmentsInReadingOrder()
        {
            var result = new List<CdxFragment>();
            if (m_fragments.Count > 0)
            {
                var centroids = from f in m_fragments select f.Centroid();
                double topY = centroids.Min(c => c.Item2);
                var centroidsSnappedToGrid = from c in centroids
                                             select new Tuple<double, int>(c.Item1,
                                                 Convert.ToInt32((c.Item2 - topY) / 100.0));
                var indexedCentroids = new Dictionary<int, Tuple<double, int>>();
                int i = 0;

                foreach (var c in centroidsSnappedToGrid)
                {
                    indexedCentroids.Add(i, c);
                    i++;
                }
                var binnedCentroids = from c in indexedCentroids
                                      group c by c.Value.Item2 into cc
                                      select new { Bin = cc.Key, centroid = cc };
                foreach (var bin in binnedCentroids)
                {
                    foreach (var item in bin.centroid.OrderBy(c => c.Value.Item1))
                    {
                        result.Add(m_fragments[item.Key]);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// returns everything as a single molfile
        /// </summary>
        public string ToMolFile(CdxEnumerateOptions ceo = CdxEnumerateOptions.None)
        {
            var singlemolecule = MoleculeFactory.Merge(from s in ToMolFiles(ceo)
                                                       select MoleculeFactory.FromMolV2000(s));
            return new GenericMolecule(singlemolecule.Headers,
                singlemolecule.IndexedAtoms, singlemolecule.IndexedBonds,
                singlemolecule.Properties.Merge(new Dictionary<string, List<string>>() { { "labels", (from l in Labels select l.Value.Split(new char[] { '\r' })).SelectMany(i => i).ToList() } })).ToString();
        }

        /// <summary>
        /// Returns a connection table as a string for each Fragment
        /// and each enumerated Fragment based on the ()n notation in the cdx,
        /// ordered first by row then from left to right.
        /// </summary>
        public IEnumerable<string> ToMolFiles(CdxEnumerateOptions ceo = CdxEnumerateOptions.None)
        {
            var firo = FragmentsInReadingOrder();
            return from i in Enumerable.Range(0, firo.Count)
                   select (ceo == CdxEnumerateOptions.EnumerateMarkush)
                   ? ce.SubstituteAllGenerics(MoleculeFactory.FromMolV2000(firo[i].Ct(i.ToString()))).ToString()
                   : firo[i].Ct(i.ToString());
        }

        public IEnumerable<string> CommonReagentsInBox(Tuple<double, double, double, double> box)
        {
            return from t in
                       (from l in LabelsInBox(box) select l.Split(new char[] { ' ', '\r', '\n' })).SelectMany(l => l)
                   where commonReagents.ContainsKey(t.ToLower())
                   select commonReagents[t.ToLower()];
        }

        public IEnumerable<string> FragmentsInBoxToMolFiles(CdxEnumerateOptions ceo, Tuple<double, double, double, double> box, string headerline)
        {
            var fragments = from f in m_fragments.Where(f => box.Contains(f.Centroid()))
                   select (ceo == CdxEnumerateOptions.EnumerateMarkush)
                        ? ce.SubstituteAllGenerics(MoleculeFactory.FromMolV2000(f.Ct(headerline))).ToString() 
                        : f.Ct(headerline);
            var textOnlyMolecules = CommonReagentsInBox(box);
            return fragments.Concat(textOnlyMolecules);
        }
        
        /// <summary>
        /// Takes the labels and removes those which are actually the n in ()n.
        /// </summary>
        /// <returns>.Item1 is a list of winnowed CdxText objects. .Item2 is a dictionary of positions and bracket counts.</returns>
        private Tuple<List<CdxText>,Dictionary<int,string>> WinnowLabelsFindBracketCounts(List<Tuple<double,double,int>> bottomRights)
        {
            List<CdxText> result = (from o
                                    in this.CdxPathSelectObjects("//Text[not(parent::Node) and not(parent::ObjectTag)]")
                                    select new CdxText(o)).ToList();
            result.AddRange(from o
                            in this.CdxPathSelectObjects("//Node[@Node_Type=0 and not(preceding-sibling::*)]/Text")
                            select new CdxText(o));
            List<CdxText> hitlist = new List<CdxText>();

            var countpositions = new Dictionary<int, string>();

            foreach (var br in bottomRights)
            {
                // br appears to be the wrong way round. Not sure why.
                Tuple<double,double> brXY = new Tuple<double,double>(br.Item2, br.Item1);
                IEnumerable<CdxText> nLabels = result.Where(l => l.XY.DistanceFrom(brXY) < 5.0);
                if (nLabels.Count() > 0)
                {
                    countpositions.Add(br.Item3, nLabels.First().Value);
                    hitlist.Add(nLabels.First());
                }
            }
            return new Tuple<List<CdxText>, Dictionary<int,string>>
                ((from r in result where !hitlist.Contains(r) select r).ToList(),
                countpositions);
        }

        // Copy constructor
        public Cdx(Cdx cdx)
            : base(cdx)
        {
            ce = new CompoundEnumerator();
            Labels = cdx.Labels;
            m_fragments = cdx.m_fragments;
            ResinousBlobs = cdx.ResinousBlobs;
            m_rootObject = cdx.m_rootObject;
        }

        // Normal constructor
        public Cdx(byte[] bytes, CdxDashedBondOptions cdbo = CdxDashedBondOptions.Ignore) : base(bytes)
        {
            ce = new CompoundEnumerator();
            // actually probably do common reagents as a separate object like this
            var cr = new Sdf(Resources.commonreagents);
            commonReagents = (from r in cr.MoleculesIndexedByProperty("label") select new KeyValuePair<string, string>(r.Key.ToLower(), r.Value.ToString())).ToDictionary(k => k.Key, k => k.Value);

            // pass resinousblobs to fragment constructor to ensure that any atoms underneath a resinous blob correctly mapped to stars
            ResinousBlobs = (from o in this.CdxPathSelectObjects("//Graphic[@Graphic_Type=5]") select new CdxResinousBlob(o)).ToList();
            // 1. these are all the top-level fragments
            var originalFragments = (from o in this.CdxPathSelectObjects("//Fragment[not(parent::Node)]")
                                     select new CdxFragment(o, ResinousBlobs, cdbo)).Where(f => f.AtomCount > 0).ToList();
            // these are all the secret, hidden fragments
            originalFragments.AddRange((from o in this.CdxPathSelectObjects("//Fragment/Node[not(preceding-sibling::*) and not(following-sibling::*)]/Fragment")
                                        where o.CdxPathSelectObjects("/Node[@Atom_GenericNickname]").Count == 0
                                        select new CdxFragment(o, ResinousBlobs, cdbo)).Where(f => f.AtomCount > 0));
            // we don't want the bracketed groups themselves, because they have the non-unique atom IDs
            // just fetch their locations
            // and keep their IDs for later
            List<Tuple<double,double,int>> bottomRights = 
                    (from bg
                    in (from o in this.CdxPathSelectObjects("//BracketedGroup") select new CdxBracketedGroup(o))
                    select new Tuple<double,double,int>(bg.BottomRight.Item1, bg.BottomRight.Item2, bg.ID)).ToList();
            var wlfbc = WinnowLabelsFindBracketCounts(bottomRights);
            
            Labels = wlfbc.Item1;
            var bracketCountDictionary = wlfbc.Item2;

            // stage 1. Assign labels to nearest fragments.
            List<Tuple<int, int, double>> distances = new List<Tuple<int, int, double>>();
            Dictionary<int, CdxFragment> dFragments = new Dictionary<int, CdxFragment>();
            int i_f = 1;
            foreach (CdxFragment f in originalFragments)
            {
                dFragments.Add(i_f, f);
                int i_l = 0;
                foreach (CdxText label in Labels.Where(l => l.Value != "+"))
                {
                    distances.Add(new Tuple<int, int, double>(i_f, i_l, f.Centroid().DistanceFrom(label.XY)));
                    i_l++;
                }
                i_f++;
            }
            // the key is the index of the fragment and the value is the index of the label
            List<CdxFragment> labelledFragments = new List<CdxFragment>();
            Dictionary<int, int> fragLabelPairings = PairOffByNearest(distances);
            foreach (var pair in fragLabelPairings)
            {
                dFragments[pair.Key].SetLabels(Labels[pair.Value]);
                labelledFragments.Add(dFragments[pair.Key]);
            }
            foreach (var pair in dFragments.Where(k => !fragLabelPairings.ContainsKey(k.Key)))
            {
                labelledFragments.Add(pair.Value);
            }

            // CODE HERE TAKES THE ACTUAL TEXT FROM WINNOWED LABELS AND APPLIES IT TO THE BRACKETED GROUP OBJECT
            List<CdxBracketedGroup> bracketedGroups = new List<CdxBracketedGroup>();
            foreach (CdxObject bg in this.CdxPathSelectObjects("//BracketedGroup"))
            {
                int id = bg.ID;
                if (bracketCountDictionary.ContainsKey(id))
                {
                    bracketedGroups.Add(new CdxBracketedGroup(bg, bracketCountDictionary[id]));
                }
                else
                {
                    bracketedGroups.Add(new CdxBracketedGroup(bg));
                }
            }
            m_fragments = (bracketedGroups.Count > 0) ? ExpandBrackets(labelledFragments, bracketedGroups).ToList() : labelledFragments;
        }

        /// <summary>
        /// Builds an SDF object from a ChemDraw file.  May fall over horribly if you pass in something that isn't a ChemDraw file.
        /// </summary>
        public static Sdf ToSdf(byte[] bytearray)
        {
            Cdx cdx = new Cdx(bytearray);
            Sdf sdf = new Sdf(String.Concat(cdx.ToMolFiles()));
            Sdf newSdf = new Sdf();
            sdf.molecules.ToList().ForEach(m => newSdf.molecules.Add(m));
            sdf.genericMolecules.ToList().ForEach(gm => newSdf.genericMolecules.Add(gm));
            return newSdf;
        }
    }
}