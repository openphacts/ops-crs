using System;
using System.Collections.Generic;
using System.Web.Security;
using System.Linq;
using System.Xml.Linq;

namespace ChemSpider.Profile.Store
{

    /// <summary>
    /// 
    /// </summary>
    public partial class XmlProfileStore : XmlStore<List<XmlProfile>>
	{
		//public Dictionary<string, Dictionary<string, string>> Values = new Dictionary<string, Dictionary<string, string>>();

        #region Properties  /////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the profiles.
        /// </summary>
        /// <value>The profiles.</value>
        public virtual List<XmlProfile> Profiles {
            get { return Value; }
        }
        #endregion

        #region Construct  //////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public XmlProfileStore(string fileName)
            : base(fileName)
		{
        }

		private Dictionary<string, string> GetValues(XElement el)
		{
			Dictionary<string, string> values = new Dictionary<string, string>();

			if (el.HasElements)
			{
				foreach (var child in el.Elements())
				{
					GetValues(child).ToList().ForEach(x => values.Add(el.Name.LocalName + "." + x.Key, x.Value)); ;
				}
			}
			else
			{
				values.Add(el.Name.LocalName, el.Value);
			}

			return values;
		}

        #endregion

        #region Methods /////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the user by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public XmlProfile GetByUserKey(Guid key) {

            lock (SyncRoot) {
                return (Profiles != null)
                    ? Profiles.Find(delegate(XmlProfile profile) { return (profile.UserKey.CompareTo(key) == 0); })
                    : null;
            }
        }

        /// <summary>
        /// Gets the name of the by user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public XmlProfile GetByUserName(string userName) {

            MembershipUser user = Membership.GetUser(userName);
            return (user != null) ? GetByUserKey((Guid)user.ProviderUserKey) : null;
        }

        /// <summary>
        /// Removes the name of the by user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        public void RemoveByUserName(string userName) {

            lock (SyncRoot) {
                XmlProfile profile = GetByUserName(userName);
                if (profile != null)
                    Profiles.Remove(profile);
            }
        }

		/// <summary>
		/// Load profiles from Xml file
		/// </summary>
		protected override void Load()
		{
			_value = new List<XmlProfile>();

			foreach (var p in Xml.Root.Elements("XmlProfile"))
			{
				XmlProfile profile = new XmlProfile()
				{
					UserKey = Guid.Parse(p.Element("UserKey").Value),
					LastUpdated = Convert.ToDateTime(p.Element("LastUpdated").Value)
				};

				foreach (var e in p.Elements())
				{
					if (e.Name.LocalName.Equals("UserKey") || e.Name.LocalName.Equals("LastUpdated"))
					{
					}
					else
					{
						foreach (var val in GetValues(e))
							profile.Values.Add(val.Key, val.Value);
					}
				}

				_value.Add(profile);
			}
		}

		/// <summary>
		/// Save profiles to Xml file
		/// </summary>
		public override void Save()
		{
			Xml = new XDocument();
			var root = new XElement("XmlProfiles");
			Xml.Add(root);

			foreach (var profile in Profiles)
			{
				var xmlProfile = new XElement("XmlProfile");
				xmlProfile.Add(new XElement("UserKey", profile.UserKey));
				xmlProfile.Add(new XElement("LastUpdated", profile.LastUpdated));
				foreach(var property in profile.Values)
				{
					XElement node = FindNode(xmlProfile, property.Key);
					node.Value = property.Value;
				}

				root.Add(xmlProfile);
			}

			Xml.Save(FileName);
		}

		private XElement FindNode(XElement el, string nodeName)
		{
			var names = nodeName.Split('.');

			if (names.Length == 1)
			{
				var node = el.Element(names[0]);

				if (node == null)
				{
					node = new XElement(names[0]);
					el.Add(node);
				}

				return node;
			}
			else
			{
				var node = el.Element(names[0]);
				if (node == null)
				{
					node = new XElement(names[0]);
					el.Add(node);
				}

				return FindNode(node, string.Join(".", names.ToList().GetRange(1, names.Length - 1)));
			}
		}
        #endregion
    }
}
