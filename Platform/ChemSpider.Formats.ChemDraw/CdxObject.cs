using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoleculeObjects
{
    /// <summary>
    /// Based on CambridgeSoft documentation at
    /// http://www.cambridgesoft.com/services/documentation/sdk/chemdraw/cdx/IntroCDX.htm
    /// </summary>
    public class CdxObject : CdxEntity
    {
        protected List<CdxObject> m_objects = new List<CdxObject>();
        // keep this as private for now to force descendants to use the correct method
        private Dictionary<string, CdxProperty> m_props = new Dictionary<string, CdxProperty>();
        protected int m_index; // this is for CdxPath
        protected CdxObject m_parent;
        public int ID { get; protected set; }

        public List<CdxObject> Objects { get { return m_objects; } }
        public CdxObject Parent { get { return m_parent; } }
        public int Length { get { return m_length; } }
        public int CdxPathIndex { get { return m_index; } }

        public dynamic Property(string key)
        {
            try
            {
                return m_props[key].Value;
            }
            catch
            {
                throw new MoleculeException(key + " missing");
            }
        }

        public bool HasProperty(string propname)
        {
            return m_props.ContainsKey(propname);
        }

        public bool MatchesProperty(string propname, string query)
        {
            return HasProperty(propname) ?
                ((Property(propname).GetType() == "".GetType()) ? Property(propname) == query : (Property(propname).ToString() == query))
                : false;
        }

        public void SetIndex(int value)
        {
            m_index = value;
        }

        /// <summary>
        /// Returns TagName, properties and children as a multiline string. 
        /// </summary>
        public override string ToString()
        {
            return ToString(0);
        }

        /// <summary>
        /// Returns TagName, properties and children as an indented multiline string. 
        /// </summary>
        public string ToString(int depth)
        {
            string indent = "".PadLeft(depth);

            string result = String.Format("{0}[{1}]{2}", indent, this.TagName, Environment.NewLine);
            result += String.Format("{2}(id = {0}){1}", ID, Environment.NewLine, indent);
            foreach (KeyValuePair<string, CdxProperty> kv in m_props)
            {
                result += indent + "@" + kv.Key + ": " + kv.Value.ToString() + Environment.NewLine;
            }
            foreach (CdxObject o in m_objects)
            {
                result += o.ToString(depth + 1);
            }
            return result;
        }

        /// <summary>
        /// Note: this used to assume that you could have only one property of a given name
        /// per object. This is not true of the document root!  Only the first property of
        /// a given name will be processed.
        /// </summary>
        private void BuildMembers(byte[] bytes, int pointer)
        {
            byte[] oID = bytes.SubArray(pointer - 4, 4);
            byte[] fakeID = new byte[] { 0x0fe, 0x07f, 0x0004, 0x0000, oID[0], oID[1], oID[2], oID[3] };
            m_props.Add("id", new CdxProperty(fakeID, 0));
            int objectIndex = 0;
            while (bytes.Sixteen(pointer) != 0)
            {
                int childLength;
                if (bytes.Sixteen(pointer) > 0x7fff)
                {
                    CdxObject o = new CdxObject(bytes, pointer, this);
                    objectIndex++;
                    o.SetIndex(objectIndex);
                    m_objects.Add(o);
                    childLength = o.Length;
                }
                else
                {
                    CdxProperty p = new CdxProperty(bytes, pointer);
                    if (!m_props.ContainsKey(p.TagName))
                    {
                        m_props.Add(p.TagName, p);
                    }
                    childLength = p.Length;
                }
                m_length += childLength;
                pointer += childLength;
            }
        }

        /// <summary>
        /// This is a copy constructor.
        /// 
        /// If ever we set the Digester to generate cdx files as well as merely reading them,
        /// we/you will need to make sure this does deep copies rather than shallow copies
        /// as at present.
        /// </summary>
        public CdxObject(CdxObject e)
            : base(e)
        {
            ID = e.ID;
            m_index = e.m_index;
            m_objects = e.m_objects;
            m_parent = e.m_parent;
            m_props = e.m_props;
        }

        /// <summary>
        /// Constructor for non-root CdxEntities.
        /// </summary>
        public CdxObject(byte[] bytes, int pointer, CdxObject parent)
            : base(bytes, pointer)
        {
            // Properties are unordered; objects have an ordering.
            m_parent = parent;
            ID = bytes.ThirtyTwo(pointer + 2);
            m_length = 2 + 4;
            pointer += m_length;
            BuildMembers(bytes, pointer);
            m_length += 2; // to catch final sixteen
        }

        /// <summary>
        /// Constructor for the root CdxEntity.
        /// </summary>
        public CdxObject(byte[] bytes, int pointer)
            : base(bytes, pointer)
        {
            TagName = "DocumentObject";
            m_length = 0;
            BuildMembers(bytes, pointer);
        }

        /// <summary>
        /// Emergency constructor needed for generating new CdxBonds in ()n enumeration.
        /// Do not use this otherwise!
        /// </summary>
        public CdxObject()
        {
        }
    }
}
