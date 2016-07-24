using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoleculeObjects
{
    public class CdxBond : CdxObject
    {
        private int m_firstCdxAtom;
        private int m_secondCdxAtom;
        private bool m_isChemicalBond;

        /// <summary>
        /// First atom, second atom, bond type, bond stereo. In that order.
        /// </summary>
        public Tuple<int, int, BondOrder, BondStereo> ctBond
        {
            get
            {
                return Tuple.Create<int, int, BondOrder, BondStereo>(m_firstCdxAtom, m_secondCdxAtom, bondOrder, ctBondStereo);
            }
        }

        public int firstCdxAtom { get { return m_firstCdxAtom; } }
        public int secondCdxAtom { get { return m_secondCdxAtom; } }
        public BondOrder bondOrder { get; private set; }
        public BondStereo ctBondStereo { get; private set; }

        public void updateFirstAtom(int newID) { m_firstCdxAtom = newID; }
        public void updateSecondAtom(int newID) { m_secondCdxAtom = newID; }

        public bool IsChemicalBond { get { return m_isChemicalBond; } }

        public bool EitherAtomIDEquals(int queryID)
        {
            return (m_firstCdxAtom == queryID) || (m_secondCdxAtom == queryID);
        }

        public void UpdateEitherAtom(int oldID, int newID)
        {
            if ( m_firstCdxAtom == oldID )
                m_firstCdxAtom = newID;
            if ( m_secondCdxAtom == oldID )
                m_secondCdxAtom = newID;
        }

        /// <summary>
        /// Returns the ID of the atom at the other end of the bond from the atom specified.
        /// </summary>
        public int OtherEnd(int queryID)
        {
            if (!EitherAtomIDEquals(queryID)) throw new MoleculeException("atom not found");
            return (m_firstCdxAtom == queryID) ? m_secondCdxAtom : m_firstCdxAtom;
        }

        /// <summary>
        /// Keys are cdx bond stereo types; values are ct bond stereo types.
        /// </summary>
        private static Dictionary<CdxBondStereo, BondStereo> cdxCtBondStereoMapping = new Dictionary<CdxBondStereo, BondStereo>
        {
            { CdxBondStereo.Solid, BondStereo.None},
            { CdxBondStereo.Dash, BondStereo.Down }, 
            { CdxBondStereo.Hash, BondStereo.None }, 
            { CdxBondStereo.Bold, BondStereo.None }, 
            { CdxBondStereo.WedgeBegin, BondStereo.Up}, 
            { CdxBondStereo.WedgeEnd, BondStereo.Up }, 
            { CdxBondStereo.WedgedHashBegin, BondStereo.Down }, 
            { CdxBondStereo.WedgedHashEnd, BondStereo.Down }, 
            { CdxBondStereo.Wavy, BondStereo.Either }
        };

        private static Dictionary<CdxBondStereo, bool> cdxCtBondStereoSwitch = new Dictionary<CdxBondStereo, bool>
        {
            { CdxBondStereo.Solid, false }, { CdxBondStereo.Dash, false }, { CdxBondStereo.Hash, false }, 
            { CdxBondStereo.WedgedHashBegin, false }, { CdxBondStereo.WedgedHashEnd, true}, 
            { CdxBondStereo.Bold,false }, 
            { CdxBondStereo.WedgeBegin, false }, { CdxBondStereo.WedgeEnd, true },
            { CdxBondStereo.Wavy, false }
        };

        /// <summary>
        /// Bond stereo types that we don't translate into bonds.
        /// If we set cdbo to CdxDashedBondOptions.Down, then we 
        /// </summary>
        private static List<int> cdxBondStereoStops(CdxDashedBondOptions cdbo)
        {
            return (cdbo == CdxDashedBondOptions.Ignore)
                ? new List<int> { (int)CdxBondStereo.Dash, 9, 10, 11, 12, 31, 14 }
                : new List<int> { 9, 10, 11, 12, 31, 14 };
        }

        /// <summary>
        /// Provides human-readable labels for the CDX stereo bond types
        /// </summary>
        public enum CdxBondStereo
        {
            Solid = 0, Dash = 1, Hash = 2, WedgedHashBegin = 3, WedgedHashEnd = 4,
            Bold = 5, WedgeBegin = 6, WedgeEnd = 7, Wavy = 8
        }

        /// <summary>
        /// http://www.cambridgesoft.com/services/documentation/sdk/chemdraw/cdx/properties/Bond_Order.htm
        /// 
        /// Tentatively: convert an aromatic bond to an aromatic query bond.
        /// Tentatively: convert a 2.5 order bond to a double bond.
        /// Treat a dative bond as a single bond.
        /// </summary>
        private static Dictionary<int, BondOrder> cdxCtBondOrderMapping = new Dictionary<int, BondOrder>
        {
            {1, BondOrder.Single}, {2,BondOrder.Double}, {4,BondOrder.Triple}, 
            {128, BondOrder.QueryAromatic}, {256, BondOrder.Double}, {4096, BondOrder.Single}
        };

        /// <summary>
        /// Requires all the information. Hope this is OK.
        /// </summary>
        public CdxBond(int firstCdxAtomID, int secondCdxAtomID, BondOrder order, BondStereo bondStereo)
            : base()
        {
            m_firstCdxAtom = firstCdxAtomID;
            m_secondCdxAtom = secondCdxAtomID;
            bondOrder = order;
            ctBondStereo = bondStereo;
            m_isChemicalBond = true;
        }

        public CdxBond(CdxObject e, CdxDashedBondOptions cdbo = CdxDashedBondOptions.Ignore)
            : base(e)
        {
            if (TagName != "Bond") throw new MoleculeException("trying to create a bond from a " + TagName);

            int cbs = HasProperty("Bond_Display") ? Property("Bond_Display") : 0;

            m_isChemicalBond = !cdxBondStereoStops(cdbo).Contains(cbs);

            if (m_isChemicalBond)
            {
                try
                {
                    bondOrder = (HasProperty("Bond_Order")) ? cdxCtBondOrderMapping[Property("Bond_Order")] : BondOrder.Single;
                }
                catch
                {
                    throw new MoleculeException("Molfiles do not support cdx bonds of type " + Property("Bond_Order"));
                }
                ctBondStereo = cdxCtBondStereoMapping[(CdxBondStereo)cbs];
                m_firstCdxAtom = !cdxCtBondStereoSwitch[(CdxBondStereo)cbs] ? Property("Bond_Begin") : Property("Bond_End");
                m_secondCdxAtom = !cdxCtBondStereoSwitch[(CdxBondStereo)cbs] ? Property("Bond_End") : Property("Bond_Begin");
            }
        }
    }
}
