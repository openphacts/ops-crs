using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoleculeObjects
{
    public enum CdxPropertyType
    {
        CdxCurvePoints,
        CdxFloat64,
        CdxPoint2D,
        CdxPoint3D,
        CdxRectangle,
        CdxString,
        Int,
        Int16ListWithCounts, // map to IEnumerable<int>, probably
        CdxObjectIDArray,
        Unknown
    }

    public class CdxProperty : CdxEntity
    {
        public static Dictionary<string, CdxPropertyType> CdxPropertyTypes = new Dictionary<string, CdxPropertyType>
        {
            { "id", CdxPropertyType.Int}, // this is a pseudo-property
            { "3DCenter", CdxPropertyType.CdxPoint3D},
            { "3DMajorAxisEnd", CdxPropertyType.CdxPoint3D},
            { "3DMinorAxisEnd", CdxPropertyType.CdxPoint3D},
            { "3DExtent", CdxPropertyType.CdxPoint3D},
            { "2DPosition", CdxPropertyType.CdxPoint2D},
            { "3DPosition", CdxPropertyType.CdxPoint3D},
            { "Arrow_Type", CdxPropertyType.Int},
            { "Atom_CIPStereochemistry", CdxPropertyType.Int},
            { "Atom_Charge", CdxPropertyType.Int},
            { "Atom_GenericNickname", CdxPropertyType.CdxString},
            { "Atom_Isotope", CdxPropertyType.Int},
            { "Atom_NumHydrogens", CdxPropertyType.Int},
            { "Bond_Begin", CdxPropertyType.Int},
            { "Bond_BondOrdering", CdxPropertyType.CdxObjectIDArray},
            { "Bond_CIPStereoChemistry", CdxPropertyType.Int},
            { "Bond_DoublePosition", CdxPropertyType.Int},
            { "Bond_End", CdxPropertyType.Int},
            { "Bond_Display", CdxPropertyType.Int},
            { "Bond_Display2", CdxPropertyType.Int},
            { "Bond_Order", CdxPropertyType.Int},
            { "Bond_RestrictTopology", CdxPropertyType.Int},
            { "BoundingBox", CdxPropertyType.CdxRectangle},
            { "Bracket_BondID", CdxPropertyType.Int},
            { "Bracket_GraphicID", CdxPropertyType.Int},
            { "Bracket_RepeatCount", CdxPropertyType.CdxFloat64},            
            { "Bracket_SRULabel", CdxPropertyType.CdxString},
            { "BracketedObjects", CdxPropertyType.CdxObjectIDArray},
            { "Frag_ConnectionOrder", CdxPropertyType.CdxObjectIDArray},
            { "InnerAtomID", CdxPropertyType.Int},
            { "ChemicalWarning", CdxPropertyType.CdxString},
            { "CreationProgram", CdxPropertyType.CdxString},
            { "Curve_Points", CdxPropertyType.CdxCurvePoints},
            { "Graphic_Type", CdxPropertyType.Int},
            { "LineStarts", CdxPropertyType.Int16ListWithCounts},
            { "Name", CdxPropertyType.CdxString},
            { "Node_Element", CdxPropertyType.Int},
            { "Node_Type", CdxPropertyType.Int},
            { "Orbital_Type", CdxPropertyType.Int},
            { "Text", CdxPropertyType.CdxString },
            { "ZOrder", CdxPropertyType.Int }
        };

        private CdxPropertyType type;
        protected int m_datalength;
        protected byte[] m_propdata;

        public int Data { get { return AsInt(); } }
        public int Length { get { return m_length; } }
        /// <summary>Returns the length (in bytes) of the data, not of the property as a whole.</summary>
        public int DataLength { get { return m_datalength; } }
        public dynamic Value
        {
            get
            {
                switch (type)
                {
                    case CdxPropertyType.CdxFloat64:
                        return AsDouble();
                    case CdxPropertyType.CdxObjectIDArray:
                        return AsObjectIDArray();
                    case CdxPropertyType.CdxPoint2D:
                        return AsPosition2D();
                    case CdxPropertyType.CdxPoint3D:
                        return AsPosition3D();
                    case CdxPropertyType.CdxRectangle:
                        return AsCDXRectangle();
                    case CdxPropertyType.CdxString:
                        return AsCDXString();
                    case CdxPropertyType.CdxCurvePoints:
                        return AsCurvePoints();
                    case CdxPropertyType.Int:
                        return AsInt();
                    case CdxPropertyType.Unknown:
                        return AsHexString();
                    default:
                        return AsHexString();
                }
            }
        }

        public override string ToString()
        {
            switch (type)
            {
                case CdxPropertyType.CdxFloat64:
                    return AsDouble().ToString();
                case CdxPropertyType.CdxObjectIDArray:
                    return "[" + String.Join(", ", AsObjectIDArray()) + "]";
                case CdxPropertyType.Int16ListWithCounts:
                    return "[" + String.Join(", ", AsInt16List()) + "]";
                case CdxPropertyType.CdxPoint2D:
                    return AsPosition2D().ToString();
                case CdxPropertyType.CdxPoint3D:
                    return AsPosition3D().ToString();
                case CdxPropertyType.CdxRectangle:
                    return AsCDXRectangle().ToString();
                case CdxPropertyType.CdxString:
                    var cdxstring = AsCDXString();
                    return String.Format("\"{0}\" {1}", cdxstring.Item1,
                        String.Join("; ", from d in cdxstring.Item2
                                          select String.Format("{0}: {1}", d.Key, String.Join(", ", d.Value))));
                case CdxPropertyType.CdxCurvePoints:
                    return "[" + String.Join(", ", AsCurvePoints()) + "]";
                case CdxPropertyType.Int:
                    return AsInt().ToString();
                case CdxPropertyType.Unknown:
                    return AsHexString();    
                default: return AsHexString();
            }
        }

        // These actually need to be private so we can't coerce a property into being the wrong thing
        private double cdxCoord(int pos)
        {
            int raw = m_propdata.SubArray(pos, 4).ThirtyTwo();
            return ((double)raw) / 65536;
        }

        private IEnumerable<int> AsInt16List()
        {
            return from i in Enumerable.Range(1, m_propdata.Sixteen()) select m_propdata.Sixteen(i * 2);
        }

        private IEnumerable<int> AsObjectIDArray()
        {
            List<int> result = new List<int>();
            for ( int i = 0; i < m_datalength; i += 4 ) {
                result.Add(m_propdata.ThirtyTwo(i));
            }
            return result;
        }

        private IEnumerable<Tuple<double, double>> AsCurvePoints()
        {
            int count = m_propdata.Sixteen();
            return from i in Enumerable.Range(0, count - 1)
                   select new Tuple<double, double>(cdxCoord(2 + 4 + (i * 8)), cdxCoord(2 + (i * 8)));
        }

        private Tuple<double, double> AsPosition2D()
        {
            if ( m_datalength == 8 ) {
                double x = cdxCoord(4);
                double y = cdxCoord(0);
                return Tuple.Create<double, double>(x, y);
            }
            else {
                throw new Exception("not a proper 2d coordinate");
            }
        }

        private double AsDouble()
        {
            if ( m_datalength == 8 ) {
                return BitConverter.ToDouble(m_propdata, 0);
            }
            else {
                throw new Exception("not a proper FLOAT64");
            }
        }

        private Tuple<double, double, double> AsPosition3D()
        {
            if ( m_datalength == 12 ) {
                double x = cdxCoord(8);
                double y = cdxCoord(4);
                double z = cdxCoord(0);
                return Tuple.Create<double, double, double>(x, y, z);
            }
            else {
                throw new Exception("not a proper 3d coordinate");
            }
        }

        private Tuple<Tuple<double, double>, Tuple<double, double>> AsCDXRectangle()
        {
            if ( m_datalength == 16 ) {
                double t = cdxCoord(0);
                double l = cdxCoord(4);
                double b = cdxCoord(8);
                double r = cdxCoord(12);
                return new Tuple<Tuple<double, double>, Tuple<double, double>>
                    (new Tuple<double, double>(l, t), new Tuple<double, double>(r, b));
            }
            else {
                throw new Exception("not a proper CdxRectangle");
            }
        }

        private int AsInt()
        {
            switch ( m_datalength ) {
                case 1:
                    return m_propdata[0];
                case 2:
                    return m_propdata.Sixteen();
                case 4:
                    return m_propdata.ThirtyTwo();
                default:
                    throw new Exception("not an int");
            }
        }

        public string AsHexString()
        {
            string result = "0x";
            foreach ( byte b in m_propdata ) {
                result += String.Format("{0:x2}", (int)b);
            }
            return result;
        }

        private List<Tuple<string, Tuple<int, int, int, int>>> textdictionary(string plaintext)
        {
            var result = new List<Tuple<string, Tuple<int, int, int, int>>>();
            int styleRunCount = m_propdata.Sixteen();
            for ( int i = 0; i < styleRunCount; i++ ) {
                int p = 2 + (i * 10);
                int startPos = m_propdata.Sixteen(p);
                int font = m_propdata.Sixteen(p + 2);
                int style = m_propdata.Sixteen(p + 4);
                int size = m_propdata.Sixteen(p + 6);
                int colour = m_propdata.Sixteen(p + 8);
                string s = string.Empty;
                if ( i != styleRunCount - 1 ) {
                    int endPos = m_propdata.Sixteen(p + 10);
                    s = plaintext.Substring(startPos, endPos - startPos);
                }
                else {
                    s = plaintext.Substring(startPos);
                }
                result.Add(new Tuple<string, Tuple<int, int, int, int>>(s, new Tuple<int, int, int, int>(font, style, size, colour)));
            }
            return result;
        }

        // Item1 is the whole text
        // Item2 is a dictionary of bold-face labels and the text following them.
        private Tuple<string, Dictionary<string, List<string>>> AsCDXString()
        {
            int styleRunCount = m_propdata.Sixteen();
            if ( m_datalength < 13 || styleRunCount > 8192) // this is not a CDXString
            {
                string plaintext = string.Empty;
                for ( int i = 2; i < m_datalength; i++ ) {
                    plaintext += (char)m_propdata[i];
                }
                return new Tuple<string, Dictionary<string, List<string>>>(plaintext, new Dictionary<string, List<string>>());
            }
            else {
                // construct Item1
                string plaintext = string.Empty;
                for ( int i = 2 + (10 * styleRunCount); i < m_datalength; i++ ) {
                    plaintext += (char)m_propdata[i];
                }
                var td = textdictionary(plaintext);
                string value = string.Empty;
                string key = string.Empty;
                bool active = false;
                var BoldFaceDictionary = new Dictionary<string, List<string>>();
                foreach ( var chunk in td ) {
                    if ( (chunk.Item2.Item2 & 1) == 1 ) {
                        if ( active ) {
                            key = key.Trim();
                            value = value.Trim();
                            if ( BoldFaceDictionary.ContainsKey(key) ) {
                                BoldFaceDictionary[key].Add(value);
                            }
                            else {
                                BoldFaceDictionary.Add(key, new List<string>(new string[] { value }));
                            }
                        }
                        key = chunk.Item1;
                        value = string.Empty;
                        active = true;
                    }
                    else {
                        if ( active )
                            value += chunk.Item1;
                    }
                }
                if ( active ) {
                    key = key.Trim();
                    value = value.Trim();
                    if ( BoldFaceDictionary.ContainsKey(key) ) {
                        BoldFaceDictionary[key].Add(value);
                    }
                    else {
                        BoldFaceDictionary.Add(key, new List<string>(new string[] { value }));
                    }
                }
                return new Tuple<string, Dictionary<string, List<string>>>(plaintext, BoldFaceDictionary);
            }
        }

        public CdxProperty(byte[] bytes, int pointer)
            : base(bytes, pointer)
        {
            m_datalength = bytes.Sixteen(pointer + 2);
            if ( m_datalength == 65535 ) {
                m_datalength = bytes.ThirtyTwo(pointer + 4);
                // this is amazingly slow; let's not bother
                m_propdata = new byte[0];
                m_length = 2 + 6 + m_datalength;
            }
            else if ( m_datalength + pointer > bytes.Count() ) {
                // this won't actually happen if we've done the previous bit right.
                TagName = "MalformedProperty";
                m_propdata = new byte[0];
                m_length = bytes.Count() - pointer;
            }
            else {
                m_propdata = bytes.SubArray(pointer + 4, m_datalength);
                m_length = 2 + 2 + m_datalength;
            }
            type = CdxPropertyTypes.ContainsKey(TagName) ? CdxPropertyTypes[TagName] : CdxPropertyType.Unknown;
        }
    }
}
