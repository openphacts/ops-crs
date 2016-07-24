using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoleculeObjects
{
    public abstract class CdxEntity
    {
        /// <summary>
        /// Taken from
        /// http://www.cambridgesoft.com/services/documentation/sdk/chemdraw/cdx/CDXConstants.h
        /// </summary>
        private static Dictionary<int, string> Labels = new Dictionary<int, string>
        {
            {0x0003, "CreationProgram"},
            {0x0008, "Name"},
            {0x000a, "ZOrder"},
            {0x000f, "IgnoreWarnings"},
            {0x0010, "ChemicalWarning"},
            {0x0100, "FontTable"},
            {0x0200, "2DPosition"},
            {0x0201, "3DPosition"},
            {0x0204, "BoundingBox"},
            {0x0205, "RotationAngle"},
            {0x0206, "BoundsInParent"},
            {0x0207, "3DHead"},
            {0x0208, "3DTail"},
            {0x0209, "TopLeft"},
            {0x020a, "TopRight"},
            {0x020b, "BottomRight"},
            {0x020c, "BottomLeft"},
            {0x020d, "3DCenter"},
            {0x020e, "3DMajorAxisEnd"},
            {0x020f, "3DMinorAxisEnd"},
            {0x0300, "ColorTable"},
            {0x0301, "ForegroundColor"},
            {0x0302, "BackgroundColor"},
            {0x0400, "Node_Type"},
            {0x0401, "Node_LabelDisplay"},
            {0x0402, "Node_Element"},
            {0x0420, "Atom_Isotope"},
            {0x0421, "Atom_Charge"},
            {0x042b, "Atom_NumHydrogens"},
            {0x0430, "Atom_Geometry"},
            {0x0431, "Atom_BondOrdering"},
            {0x0433, "Atom_GenericNickname"},
            {0x0437, "Atom_CIPStereochemistry"},
            {0x043a, "Atom_ShowQuery"},
            {0x043b, "Atom_ShowStereo"},
            {0x043c, "Atom_ShowAtomNumber"},
            {0x0442, "Atom_ShowTerminalCarbonLabels"},
            {0x0443, "Atom_ShowNonTerminalCarbonLabels"},
            {0x0444, "Atom_HideImplicitHydrogens"},
            {0x0445, "Atom_ShowEnhancedStereo"},
            {0x0446, "Atom_EnhancedStereoType"},
            {0x0447, "Atom_EnhancedStereoGroupNum"},
            {0x0505, "Frag_ConnectionOrder"},
            {0x0600, "Bond_Order"},
            {0x0601, "Bond_Display"},
            {0x0602, "Bond_Display2"},
            {0x0603, "Bond_DoublePosition"},
            {0x0604, "Bond_Begin"},
            {0x0605, "Bond_End"},
            {0x0606, "Bond_RestrictTopology"},
            {0x060a, "Bond_CIPStereoChemistry"},
            {0x060b, "Bond_BondOrdering"},
            {0x060c, "Bond_ShowQuery"},
            {0x060d, "Bond_ShowStereo"},
            {0x060e, "Bond_CrossingBonds"},
            {0x060f, "Bond_ShowRxn"},
            {0x0700, "Text"},
            {0x0701, "Justification"},
            {0x0702, "LineHeight"},
            {0x0703, "WordWrapWidth"},
            {0x0704, "LineStarts"},
            {0x0705, "LabelAlignment"},
            {0x0706, "LabelLineHeight"},
            {0x0707, "CaptionLineHeight"},
            {0x0708, "InterpretChemically"},
            {0x0800, "MacPrintInfo"},
            {0x0802, "PrintMargins"},
            {0x0803, "ChainAngle"},
            {0x0804, "BondSpacing"},
            {0x0805, "BondLength"},
            {0x0806, "BoldWidth"},
            {0x0807, "LineWidth"},
            {0x0808, "MarginWidth"},
            {0x0809, "HashSpacing"},
            {0x080a, "LabelStyle"},
            {0x080b, "CaptionStyle"},
            {0x080c, "CaptionJustification"},
            {0x080d, "FractionalWidths"},
            {0x080e, "Magnification"},
            {0x080f, "WidthPages"},
            {0x0810, "HeightPages"},
            {0x0811, "DrawingSpaceType"},
            {0x0812, "Width"},
            {0x0813, "Height"},
            {0x0814, "PageOverlap"},
            {0x0815, "Header"},
            {0x0816, "HeaderPosition"},
            {0x0817, "Footer"},
            {0x0818, "FooterPosition"},
            {0x0819, "PrintTrimMarks"},
            {0x0823, "LabelJustification"},
            {0x0900, "Window_IsZoomed"},
            {0x0901, "Window_Position"},
            {0x0902, "Window_Size"},
            {0x0a00, "Graphic_Type"},
            {0x0a01, "Line_Type"},
            {0x0a02, "Arrow_Type"},
            {0x0a04, "Oval_Type"},
            {0x0a05, "Orbital_Type"},
            {0x0a06, "Bracket_Type"},
            {0x0a08, "Curve_Type"},
            {0x0a20, "Arrow_HeadSize"},
            {0x0a22, "Bracket_LipSize"},
            {0x0a23, "Curve_Points"},
            {0x0a24, "Bracket_Usage"},
            {0x0a25, "Polymer_RepeatPattern"},
            {0x0a26, "Polymer_FlipType"},
            {0x0a27, "BracketedObjects"},
            {0x0a28, "Bracket_RepeatCount"},
            {0x0a29, "Bracket_ComponentOrder"},
            {0x0a2a, "Bracket_SRULabel"},
            {0x0a2b, "Bracket_GraphicID"},
            {0x0a2c, "Bracket_BondID"},
            {0x0a2d, "InnerAtomID"},
            {0x0a2f, "Arrowhead_Type"},
            {0x0a37, "Fill_Type"},
            {0x0a39, "Closed"},
            {0x0c01, "ReactionStep_Reactants"},
            {0x0c02, "ReactionStep_Products"},
            {0x0c03, "ReactionStep_Plusses"},
            {0x0c04, "ReactionStep_Arrows"},
            {0x0c05, "ReactionStep_ObjectsAboveArrow"},
            {0x0c06, "ReactionStep_ObjectsBelowArrow"},
            {0x0d00, "ObjectTag_Type"},
            {0x7ffe, "id"}, // again, this is a pseudo-property
            {0x7fff, "MalformedProperty"},
            {0x8001, "Page"},
            {0x8002, "Group"},
            {0x8003, "Fragment"},
            {0x8004, "Node"},
            {0x8005, "Bond"}, 
            {0x8006, "Text"},
            {0x8007, "Graphic"},
            {0x8008, "Curve"},
            {0x800d, "ReactionScheme"},
            {0x800e, "ReactionStep"},
            {0x8011, "ObjectTag"},
            {0x8017, "BracketedGroup"},
            {0x8018, "BracketAttachment"},
            {0x8019, "CrossingBond"},
            {0x8021, "Geometry"}
        };

        protected int m_length;
        protected int m_tag;

        public string TagName { get; protected set; }

        public CdxEntity(CdxEntity e)
        {
            m_length = e.m_length;
            m_tag = e.m_tag;
            TagName = e.TagName;
        }

        public CdxEntity(byte[] bytes, int pointer)
        {
            m_tag = bytes.Sixteen(pointer);
            TagName = (Labels.ContainsKey(m_tag)) ? Labels[m_tag] :
                String.Format("Unknown {0} 0x{1:x4}", (m_tag > 0x7fff) ? "object" : "property", m_tag);
        }

        /// <summary>
        /// Emergency constructor needed for generating new CdxBonds in ()n enumeration.
        /// Do not use this otherwise!
        /// </summary>
        public CdxEntity()
        {
        }
    }
}
