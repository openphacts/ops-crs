using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoleculeObjects
{
    /// <summary>
    /// We treat explicit fragments, those where all of the nodes and bonds are visible, differently
    /// from implicit fragments, those with parent::Node.
    /// </summary>
    public class CdxFragment : CdxObject
    {
        public IEnumerable<CdxNode> Nodes { get; protected set; }
        /// <summary>
        /// Keys are the CDX IDs, values are the V2000 IDs. Note that it has to be that
        /// way round as multiple CDX nodes can map on to the same atom in the result.
        /// </summary>
        public Dictionary<int, int> CdxCtAtomMapping { get; protected set; }
        private Dictionary<string, string> ctExpansions = new Dictionary<string, string>();
        private string m_diagnostic = String.Empty;

        /// <summary>
        /// We need to be able to access the bonds to reassign those bonds that point to Fragments rather
        /// than Atoms.
        /// </summary>
        public Dictionary<int, CdxBond> IndexedBonds { get; protected set; }
        public Dictionary<int, CdxAtom> IndexedAtoms { get; protected set; }
        public int AtomCount { get { return IndexedAtoms.Count; } }
        public IEnumerable<string> Labels { get; protected set; }

        public void SetLabels(CdxText label)
        {
            Labels = label.BoldFaceDictionary.Count > 0
                ? (from p in label.BoldFaceDictionary select 
                (from s in p.Value select p.Key.TrimEnd(new char[] { ':' }) + "|" + s)).
                SelectMany(l => l)
                : label.Value.Split(new char[] { '\r' });
        }

        /// <summary>
        /// Replaces existing fragment label, if any, with list of strings.
        /// </summary>
        public void SetLabels(List<string> labels)
        {
           Labels = labels;
        }

        public void AddAtom(CdxAtom a)
        {
            if (CdxCtAtomMapping.ContainsKey(a.ID))
            {
                throw new MoleculeException("trying to add already-existent atom");
            }
            else
            {
                IndexedAtoms.Add(a.ID, a);
                CdxCtAtomMapping.Add(a.ID, IndexedAtoms.Count);
            }
        }

        public void RemoveAtom(int atomCdxID)
        {
            IndexedAtoms = (from p in IndexedAtoms where p.Key != atomCdxID select p).ToDictionary(p => p.Key, p => p.Value);
            var resultMapping = new Dictionary<int, int>();
            foreach (var pair in CdxCtAtomMapping.Where(p => p.Key != atomCdxID))
            { // and shift everything down one
                resultMapping.Add(pair.Key, (pair.Key > atomCdxID) ? pair.Value - 1 : pair.Value);
            }
            CdxCtAtomMapping = resultMapping;
        }

        public void AddBond(CdxBond b)
        {
            // add 1 to the current maximum index
            IndexedBonds.Add(IndexedBonds.Any() ? IndexedBonds.Keys.Max() + 1 : 1, b);
        }

        /// <summary>
        /// Takes a list of bracketed atom IDs and duplicates not only those atoms but the bonds between them.
        /// </summary>
        /// <returns>dictionary mapping old atom CdxIDs to duplicated atom CdxIDs.</returns>
        public Dictionary<int, int> DuplicateBracketedSection(IEnumerable<int> bracketedAtomIDs)
        {
            var oldNewAtomMapping = new Dictionary<int, int>();
            // duplicate atoms
            var duplicanda = (from a in IndexedAtoms where bracketedAtomIDs.Contains(a.Key) select a).ToList();
            foreach (var duplicandum in duplicanda)
            {
                // horrible fudge - maybe we can increase the margin if we find cases where this breaks                
                int newAtomID = (from a in IndexedAtoms select a.Key).Max() + 2;
                AddAtom(new CdxAtom(duplicandum.Value, newAtomID, duplicandum.Value.PhysicalCharge));
                oldNewAtomMapping.Add(duplicandum.Key, newAtomID);
            }
            // duplicate internal bonds
            var bondDuplicanda = (IndexedBonds.Values.Where(b => bracketedAtomIDs.Contains(b.firstCdxAtom)
                                    && bracketedAtomIDs.Contains(b.secondCdxAtom))).ToList();
            foreach (CdxBond bdup in bondDuplicanda)
            {
                AddBond(new CdxBond(oldNewAtomMapping[bdup.firstCdxAtom], oldNewAtomMapping[bdup.secondCdxAtom], bdup.bondOrder, bdup.ctBondStereo));
            }
            return oldNewAtomMapping;
        }

        private Tuple<Tuple<int, int>, CdxBond> OrderedBond(CdxBond bond)
        {
            int firstatom = (bond.ctBond.Item1 < bond.ctBond.Item2)
                ? bond.ctBond.Item1 : bond.ctBond.Item2;
            int secondatom = (bond.ctBond.Item1 < bond.ctBond.Item2)
                ? bond.ctBond.Item2 : bond.ctBond.Item1;
            return Tuple.Create(Tuple.Create(firstatom, secondatom), bond);
        }

        public string Ct(string firstLinePrefix)
        {
            if (IndexedAtoms.Count > 0)
            {
                string secondline = String.Format("CBDIGESTER{0}", DateTime.Now.ToString("MMddyyhhmm"));

                List<string> headers = new List<string>() { firstLinePrefix, secondline, "" };

                // This indexes the atoms according to cdxCtAtomMapping rather than the order they appear in m_atoms!
                var ctIndexedAtoms = (from i in (from k in Enumerable.Range(1, IndexedAtoms.Count) select k - 1)
                                    select new KeyValuePair<int, Atom>(
                                        CdxCtAtomMapping[IndexedAtoms.ElementAt(i).Key], 
                                        IndexedAtoms.ElementAt(i).Value.Atom())).
                                    OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
                // this is where the bond deduplication happens
                // deduplicate on the sorted pair of first atom and second atom
                // but do not sort them in the result as bonds will point in wrong direction!
                var obonds = from bond in IndexedBonds.Values where bond.IsChemicalBond select OrderedBond(bond);
                var distinctBonds = new List<Tuple<int, int>>();
                Dictionary<int, Bond> ctIndexedBonds = new Dictionary<int, Bond>();
                int j = 0;
                // add check here to make sure that bonds to un-atomed nodes don't go in
                // potentially fishy, THINK about this.
                foreach (var obond in obonds
                   // .Where(o => !distinctBonds.Contains(o.Item1))
                    .Where(o => CdxCtAtomMapping.ContainsKey(o.Item2.ctBond.Item1))
                    .Where(o => CdxCtAtomMapping.ContainsKey(o.Item2.ctBond.Item2)))
                {
                    j++;
                    CdxBond bond = obond.Item2;
                    int i1 = CdxCtAtomMapping[bond.ctBond.Item1];
                    int i2 = CdxCtAtomMapping[bond.ctBond.Item2];

                    ctIndexedBonds.Add(j, new Bond(ctIndexedAtoms[i1], ctIndexedAtoms[i2], i1, i2, bond.ctBond.Item3, bond.ctBond.Item4));
                  //  distinctBonds.Add(obond.Item1);
                }

                char[] pipe = new char[]{'|'};
                char[] comma = new char[]{','};

                var labels = (from l in Labels
                              select l.Contains("|") ? l.Split(pipe)[0].Trim() : l).ToList();
                var enumerations = (from l in Labels
                                    where l.Contains("=")
                                    select l.Contains("|") ? l.Split(pipe)[1].TrimStart(comma).Trim() : l).ToList();

                var genericLabels = (from a in IndexedAtoms.Values 
                                     where a.IsGeneric select a.Element).Distinct().ToList();

                foreach (CdxAtom atom in IndexedAtoms.Values.Where(a => a.IsGeneric))
                {
                    if (atom.FullLabel != atom.Element) if (!ctExpansions.ContainsKey(atom.Element))
                            ctExpansions.Add(atom.Element, atom.FullLabel);
                }

                List<Dictionary<string, List<string>>> lprops = new List<Dictionary<string, List<string>>>()
                {
                    new Dictionary<string,List<string>>() { { "labels", labels } },
                    new Dictionary<string,List<string>>() { { "Enumeration", enumerations } },
                    new Dictionary<string,List<string>>() { { "Generics", genericLabels} },
                    new Dictionary<string,List<string>>() { { "Expansion", (from p in ctExpansions select p.Key + " = " + p.Value).ToList()}},
                    new Dictionary<string,List<string>>() { { "CDXDiagnostics", new List<string> { m_diagnostic } } }
                };

                // this combines all of the above dictionaries and removes any with empty lists
                Dictionary<string, List<string>> properties = (from d in lprops
                                                              where d.First().Value.Count > 0
                                                              select d).SelectMany(d => d).ToDictionary(p => p.Key, p => p.Value);
                GenericMolecule gm = new GenericMolecule(headers, ctIndexedAtoms, ctIndexedBonds, properties);
                return gm.ToString();
            }
            else { return String.Empty; }
        }

        public Tuple<double, double> Centroid()
        {
            int atomcount = IndexedAtoms.Count;
            return atomcount == 0 ? new Tuple<double, double>(0.0, 0.0) :
                Tuple.Create((from a in IndexedAtoms.Values select a.XYZ.Item1).Sum() / atomcount,
                    (from a in IndexedAtoms.Values select a.XYZ.Item2).Sum() / atomcount);
        }

        // copy constructor
        public CdxFragment(CdxFragment f, string diagnosticMessage = "")
            : base(f)
        {
            Nodes = new List<CdxNode>(f.Nodes);
            IndexedBonds = new Dictionary<int, CdxBond>(f.IndexedBonds);
            IndexedAtoms = new Dictionary<int, CdxAtom>(f.IndexedAtoms);
            CdxCtAtomMapping = new Dictionary<int, int>(f.CdxCtAtomMapping);
            Labels = new List<string>(f.Labels);
            m_diagnostic = diagnosticMessage;
        }

        // constructor constructor
        public CdxFragment(CdxObject o, List<CdxResinousBlob> ResinousBlobs, CdxDashedBondOptions cdxdashedbondoptions = CdxDashedBondOptions.Ignore)
            : base(o)
        {
            // vexingly we have to declare all the bonds before we declare the nodes
            Labels = new List<string>();
            CdxCtAtomMapping = new Dictionary<int, int>();
            IndexedAtoms = new Dictionary<int, CdxAtom>();
            IndexedBonds = (from b in this.CdxPathSelectObjects("/Bond", 0) select new KeyValuePair<int,CdxBond>(b.ID, new CdxBond(b, cdxdashedbondoptions))).ToDictionary(p => p.Key, p => p.Value);
            if (IndexedBonds == null) IndexedBonds = new Dictionary<int, CdxBond>();
            Nodes = (from a in this.CdxPathSelectObjects("/Node", 0) select new CdxNode(a, ResinousBlobs, this)).ToList();
            // sometimes we have invisible nodes! Only populate atom list if nodes visible.
            if (Nodes.Count() > 1 || (Nodes.Count() == 1 && !Nodes.First().IsInvisible()))
            {
                var bondsInNodes = (from b in
                                        (from n in Nodes select n.Bonds.Where(b => b.IsChemicalBond)).SelectMany(l => l)
                                    select new KeyValuePair<int, CdxBond>(b.ID, b)).ToDictionary(p => p.Key, p => p.Value);
                IndexedBonds = IndexedBonds.Concat(bondsInNodes).ToDictionary(p => p.Key, p => p.Value);
                // merge atoms
                int atomNo = 1;
                foreach (CdxNode node in Nodes.Where(n => !n.IsMultiAttachment))
                {
                    foreach (CdxAtom atom in node.Atoms)
                    {
                        IndexedAtoms.Add(atom.ID, atom);
                        CdxCtAtomMapping.Add(atom.ID, atomNo);
                        if (node.ReplacedNodeMapping.ContainsKey(atom.ID) && !CdxCtAtomMapping.ContainsKey(node.ReplacedNodeMapping[atom.ID]))
                        { // pro tem
                            CdxCtAtomMapping.Add(node.ReplacedNodeMapping[atom.ID], atomNo);
                        }
                        atomNo++;
                    }
                }
            }

            IEnumerable<CdxText> pluscandidates = (from t in this.Parent.CdxPathSelectObjects("//Text")
                                                   select new CdxText(t)).Where(t => t.Value == "+");

            foreach (var cand in pluscandidates)
            {
                var atomDistancePairs = (from a in IndexedAtoms.Values
                                         where !a.IsHidden // don't look at atoms in expanded abbns
                                         where a.XY.DistanceFrom(cand.Centroid) < 7
                                         select Tuple.Create(a, a.XY.DistanceFrom(cand.Centroid)));

                if (atomDistancePairs.Any())
                {
                    var first = atomDistancePairs.Where(a => a.Item2 == atomDistancePairs.Min(aa => aa.Item2)).First();
                    CdxAtom newAtom = new CdxAtom(first.Item1, first.Item1.ID, first.Item1.PhysicalCharge + 1);
                    IndexedAtoms = (from p in IndexedAtoms
                                    select p.Key == first.Item1.ID
                                    ? new KeyValuePair<int, CdxAtom>(first.Item1.ID, newAtom)
                                    : p).ToDictionary(p => p.Key, p => p.Value);
                }
            }
        }
    }
}
