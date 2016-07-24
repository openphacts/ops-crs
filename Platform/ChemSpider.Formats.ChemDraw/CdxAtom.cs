using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoleculeObjects
{
    /// <summary>
    /// This class contains all the logic needed for an actual atom.
    /// </summary>
    public class CdxAtom : CdxObject
    {
        public Tuple<double, double, double> XYZ { get; protected set; }
        public Tuple<double, double> XY { get { return Tuple.Create(XYZ.Item1, XYZ.Item2); } }

        public string Element { get; protected set; }
        private string m_fullLabel;
        private int m_isotope;
        
        private bool m_explicitH;
        private int m_explicitHcount;
        private int m_radical;

        public bool IsECP { get; set; }
        public bool IsGeneric { get; protected set; }
        public bool IsHidden { get; protected set; }
        public int PhysicalCharge { get; protected set; }
        public string FullLabel { get { return (m_fullLabel != null) ? m_fullLabel : Element; } }


        private Dictionary<int, int> cdxPhysicalChargeMapping = new Dictionary<int, int>
        {
            {0,0}, {1,1}, {2,2}, {3,3},
            {4,4}, {5,5}, {6,6}, {7,7}, {8,8},
            {128, -1}, {255, -1}, {254, -2},
            {50331648, 3}, {-50331748, -3},
            {33554432, 2}, {-33554432, -2},
            {16777216, 1}, {-16777216, -1} 
        };

        public Atom Atom()
        {
            int isotope = (m_isotope == 0) ? 0 : AtomicProperties.AtomicMasses[Element] - m_isotope;
            return new Atom(XYZ.Item1, -XYZ.Item2, XYZ.Item3, Element, PhysicalCharge, m_radical, m_isotope, 0, 0);
        }

        private void Initialize(CdxObject e)
        {
            XYZ = HasProperty("2DPosition") ? Tuple.Create(Property("2DPosition").Item1, Property("2DPosition").Item2, 0.0)
                : HasProperty("3DPosition") ? Property("3DPosition")
                // assume something roughly central and let cleaning do the work
                : Tuple.Create(200.0, 200.0, 0.0);

            PhysicalCharge = HasProperty("Atom_Charge") ? cdxPhysicalChargeMapping[Property("Atom_Charge")] : 0;
            m_isotope = HasProperty("Atom_Isotope") ? Property("Atom_Isotope") : 0;

            m_radical = 0;
            // TODO: write code for handling the radical state

            m_explicitH = HasProperty("Atom_NumHydrogens");
            m_explicitHcount = m_explicitH ? Property("Atom_NumHydrogens") : 0;
        }

        //copy constructor
        public CdxAtom(CdxAtom a, int newCdxId, int newPhysicalCharge)
            : base(a)
        {
            ID = newCdxId; // new information
            // old information
            XYZ = a.XYZ;
            Element = a.Element;
            m_isotope = a.m_isotope;
            PhysicalCharge = newPhysicalCharge;
            m_explicitH = a.m_explicitH;
            m_explicitHcount = a.m_explicitHcount;
            m_radical = a.m_radical;
            IsGeneric = a.IsGeneric;
            IsHidden = a.IsHidden;
        }

        // non-copy constructors
        public CdxAtom(CdxObject e, string label)
            : base(e)
        {
            Initialize(e);
            Element = label.Trim();
            IsGeneric = !AtomicProperties.AtomicMasses.ContainsKey(label);
            IsHidden = true;
            if (IsGeneric && label.Length > 3)
            {
                m_fullLabel = label;
                Element = Element.Substring(0, 3);
            }
        }

        private static List<string> reallyGenerics = new List<string>(new string[] { "Ar", "Y" });
        private static List<string> reallyHydrogens = new List<string>(new string[] { "He", "Hs" });

        public CdxAtom(CdxObject e)
            : base(e)
        {
            Initialize(e);
            Element = "C";
            IsHidden = false;
            if (HasProperty("Node_Element"))
            {
                try
                {
                    Element = AtomicProperties.AtomicSymbols[Property("Node_Element")];
                    IsGeneric = (reallyGenerics.Contains(Element));
                }
                catch
                {
                    throw new MoleculeException("unrecognized element " + Property("Node_Element"));
                }
            }
        }
    }
}