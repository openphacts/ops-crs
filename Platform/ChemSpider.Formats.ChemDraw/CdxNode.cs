using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MoleculeObjects
{
    public class CdxNode : CdxObject
    {
        public List<CdxAtom> Atoms { get; protected set; }
        public List<CdxBond> Bonds { get; protected set; }
        /// <summary>
        /// Key is the atom ID, value is the parent node ID. (Caution: looks as if it's back-to-front.)
        /// This is needed because some of the bonds are written to a fragment instead of an atom!
        /// </summary>
        public Dictionary<int, int> ReplacedNodeMapping { get; protected set; }
        public bool IsExternalConnectionPoint { get; protected set; }
        /// <summary>
        /// True if this is the sort of dummy node we get at the centre of cyclopentadienyl rings in ferrocene, for example.
        /// In future this will be expanded.
        /// </summary>
        public bool IsMultiAttachment { get; protected set; }
        public string Label { get; protected set; }

        private CdxFragment m_parentFragment;
        private List<string> labels = new List<string>(); // add coordinates to this in the fullness

        /// <summary>
        /// If the atom to which the child fragment is attached is to the fragment's right on the diagram,
        /// then there are cases (for example nitriles) where ChemDraw interprets the label as if it is 
        /// back-to-front (resulting in an isonitrile).
        /// </summary>
        private bool ChildIsReversed(CdxBond b)
        {
            var otherEnd = (from n in m_parentFragment.CdxPathSelectObjects("/Node[@id=" + b.OtherEnd(ID) + "]")
                            select n.Property("2DPosition")).First();
            return otherEnd.Item1 > Property("2DPosition").Item1;
        }

        /// <summary>
        /// Extracts the atoms and bonds within child fragments and wires them up correctly.
        /// </summary>
        private void ProcessChildFragment(CdxObject f, CdxObject e)
        {
            // externalBond is the bond in the fragment we're currently in that goes to this node
            // ecpBond is the bond within the fragment that connects to the external connection point 
            var nodes = from n in f.CdxPathSelectObjects("/Node", 0)
                        select new CdxNode(n, new List<CdxResinousBlob>(), m_parentFragment);
            var ecps = nodes.Where(n => n.MatchesProperty("Node_Type", "12")).ToList();
            var externalBonds = m_parentFragment.IndexedBonds.Values.Where(b => b.EitherAtomIDEquals(ID)).ToList();

            // first special case: reverse nitrile
            if (externalBonds.Count == 1 && Label == "NC" && ChildIsReversed(externalBonds.First()))
            {
                Atoms.Add(new CdxAtom(e, "CN"));
            }
            // we are guarding against
            // (1) the case where there are more ecps than bonds out
            // (2) cases where "DMF", say, is stored internally as NONSENSE
            // (3) perfluoroalkyl substituents
            else if (ecps.Count() > externalBonds.Count()
                    || f.CdxPathSelectObjects("/Node[@Atom_GenericNickname]").Any()
                    || Regex.IsMatch(Label, @"C\d+F\d+"))
            {
                Atoms.Add(new CdxAtom(e, Label));
            } // here is the normal case
            else if (externalBonds.Any())
            {
                var fragAtoms = from a in nodes select new CdxAtom(a, a.Label);
                var fragBonds = from a in f.CdxPathSelectObjects("/Bond", 0) select new CdxBond(a);
                // it is possible that there may be more external bonds than there are ecps
                // in that case the surplus ones just get ignored
                foreach (CdxNode ecp in ecps)
                {
                    CdxBond externalBond = externalBonds.First();
                    CdxBond ecpBond = fragBonds.Where(b => b.EitherAtomIDEquals(ecp.ID)).First();
                    int scaffoldAnchorPoint = externalBond.OtherEnd(ID);
                    // determine whether the atom at the other end is to the RIGHT or to the LEFT 
                    if (!ReplacedNodeMapping.ContainsKey(ecpBond.OtherEnd(ecp.ID)))
                    { // pro tem
                        ReplacedNodeMapping.Add(ecpBond.OtherEnd(ecp.ID), ID);
                    }
                    ecpBond.UpdateEitherAtom(ecp.ID, scaffoldAnchorPoint);
                    externalBond.UpdateEitherAtom(ID, ecpBond.OtherEnd(scaffoldAnchorPoint));
                    externalBonds = externalBonds.Skip(1).ToList();
                }
                var ecpIDs = from ecp in ecps select ecp.ID;
                Atoms.AddRange(from a in nodes where !ecpIDs.Contains(a.ID) select new CdxAtom(a, a.Label));
                Bonds.AddRange(fragBonds);
            }
            // and lastly do nothing if there are no external bonds
        }

        private void ProcessChildFragments(CdxObject e)
        {
            Label = LabelFromChildTextNode();
            var childFragments = e.CdxPathSelectObjects("/Fragment");
            if (!childFragments.Any())
            {
                Atoms.Add(new CdxAtom(e, Label));
            }
            else if (childFragments.Count > 1) 
            {
                throw new MoleculeException("node contains more than one child fragment");
            }
            else
            {
                ProcessChildFragment(childFragments.First(), e);
            }
        }

        private string LabelFromChildTextNode()
        {
            var textObjects = this.CdxPathSelectObjects("/Text");
            return textObjects.Any() ? new CdxText(textObjects.First()).Property("Text").Item1 : "";
        }

        public bool IsInvisible()
        {
            return m_parentFragment.IndexedBonds.Count == 0 && !HasProperty("Node_Element");
        }

        public bool UnderneathBlob(List<CdxResinousBlob> ResinousBlobs)
        {
            // need to do this over two lines because Contains is an extension method
            Tuple<double, double> xy = Property("2DPosition");
            return (from r in ResinousBlobs where r.BoundingBox.Contains(xy) select r).Any();
        }

        private void ProcessAsElementList(CdxObject e)
        {
            string trimmedName = LabelFromChildTextNode().TrimStart('[').TrimEnd(']');
            Atoms.AddRange(trimmedName.Contains(",")
                ? from n in trimmedName.Split(new char[] { ',' }) select new CdxAtom(e, n.Trim())
                : new List<CdxAtom>() { new CdxAtom(e, trimmedName) });
        }

        /// <summary>
        /// Two cases: if at the top level then it's an explicit one which we turn into
        /// an atom, labelled say E12 or E21 for bracket enumeration. If within a fragment
        /// then it's an external connection point which is then used to construct the true
        /// molecule.
        /// </summary>
        private void ProcessAsExternalConnectionPoint(CdxObject e)
        {
            if (e.Parent.Parent.TagName == "Page")
            {
                Label = "E" + e.CdxPathIndex.ToString();
                CdxAtom ecp = new CdxAtom(e, Label);
                ecp.IsECP = true;
                Atoms.Add(ecp);
            }
            else
            {
                IsExternalConnectionPoint = true;
            }
        }

        /// <summary>
        /// Fetches node type but reassigns chemically-uninterpretable nodes with siblings
        /// to GenericNicknames.
        /// </summary>
        public int NodeType(CdxObject e)
        {
            return (Property("Node_Type") == 0)
                && e.CdxPath_SelfMatchesPredicate("preceding-sibling::*")
                && e.CdxPath_SelfMatchesPredicate("following-sibling::*")
                ? 7
                : Property("Node_Type");
        }

        public CdxNode(CdxObject e, List<CdxResinousBlob> ResinousBlobs, CdxFragment parentFragment)
            : base(e)
        {
            var nodeTypeActions = new Dictionary<int, Action<CdxObject>>()
            {
                {0, f => { Label = LabelFromChildTextNode(); labels.Add(Label);}},
                {1, f => { Label = AtomicProperties.AtomicSymbols[Property("Node_Element")]; Atoms.Add(new CdxAtom(f, Label));}},
                {2, f => ProcessAsElementList(f)}, // element list
                {3, f => { throw new MoleculeException("ElementListNickname node type not implemented");}},
                {4, f => ProcessChildFragments(f)}, // nickname
                {5, f => ProcessChildFragments(f)}, // fragment
                {6, f => { throw new MoleculeException("Formula node type not implemented"); }},
                {7, f => { Label = LabelFromChildTextNode(); Atoms.Add(new CdxAtom(f,Label)); }}, // genericnickname
                {8, f => {}}, // anonymous alternative group, do nothing for this
                {9, f => { throw new MoleculeException("NamedAlternativeGroup node type not implemented"); }},
                {10, f => IsMultiAttachment = true},
                {11, f => { throw new MoleculeException("VariableAttachment node type not implemented"); }},
                {12, f => ProcessAsExternalConnectionPoint(f)},
                {13, f => { throw new MoleculeException("LinkNode node type not implemented"); }}
            };

            Atoms = new List<CdxAtom>();
            Bonds = new List<CdxBond>();
            ReplacedNodeMapping = new Dictionary<int, int>();
            IsExternalConnectionPoint = false;
            IsMultiAttachment = false;
            m_parentFragment = parentFragment;
            // turn anything that's overwritten with a graphic into an attachment point
            if ((HasProperty("2DPosition") && ResinousBlobs.Count > 0) ? UnderneathBlob(ResinousBlobs) : false)
            {
                Atoms.Add(new CdxAtom(e, "*"));
                Label = "*";
            }
            else if (!HasProperty("Node_Type"))
            {
                CdxAtom atom = new CdxAtom(e);
                Atoms.Add(atom);
                Label = atom.Element;
            }
            else
            {
                nodeTypeActions[NodeType(e)](e);
            }
        }
    }
}
