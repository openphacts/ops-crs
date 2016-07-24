using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Text;
using System.Collections;
using System.Xml;
using System.IO;
using System.Collections.Specialized;

namespace ChemSpider.ObjectModel
{
	/// <summary>
	/// Summary description for Citation
	/// </summary>
	public class Citation : IComparable
	{
		ArrayList m_IDList = new ArrayList(); // list of strings
		Hashtable m_TrackingTags = new Hashtable();

		public Citation()
		{
		}

		public Citation(string aID, string aPaperTitle, string aAuthors, string aJournal)
		{
			/*if (aID == null || aID == "")
				aID = aPaperTitle + aAuthors + aJournal;*/
			AddID(aID);
			PaperTitle = aPaperTitle;
			Authors = aAuthors;
			Journal = aJournal;
		}

		public Hashtable TrackingTags
		{
			get
			{
				return m_TrackingTags;
			}
		}

		public string PaperTitle { get; set; }

		public string Authors { get; set; }

		public string Journal { get; set; }

		public string Description { get; set; }

        public string ID
		{
			get
			{
				return m_IDList.Count > 0 ? (string)m_IDList[0] : PaperTitle + Authors + Journal;
			}
			set
			{
				if ( m_IDList.Count > 0 )
					m_IDList[0] = value;
				else
					AddID(value);
			}
		}

		public string[] IDs
		{
			get
			{
				return (string[])m_IDList.ToArray(typeof(string));
			}
		}

		public string[] URLs
		{
			get
			{
				ArrayList urls = new ArrayList();
				foreach ( string id in m_IDList ) {
					if ( id.StartsWith("http://") || id.StartsWith("ftp://") || id.StartsWith("https://") )
						urls.Add(id);
				}
				if ( urls.Count == 0 )
					return null;
				return (string[])urls.ToArray(typeof(string));
			}
		}

		public string getID(string prefix)
		{
			foreach ( string s in m_IDList ) {
				if ( s.StartsWith(prefix) )
					return s;
			}
			return null;
		}

		public string PMID
		{
			get
			{
				string s = getID("pmid://");
				if ( s != null )
					return s.Substring(7);
				return null;
			}
		}

		public string DOI
		{
			get
			{
				string s = getID("doi://");
				if ( s != null )
					return s.Substring(6);
				return null;
			}
		}

		public void AddID(string id)
		{
			if ( !String.IsNullOrEmpty(id) )
				m_IDList.Add(id.Trim());
		}

		string encode(string s)
		{
			return string.IsNullOrEmpty(s) ? s :
					s.Replace("\\", "\\\\")
					 .Replace("|", "\\p")
					 .Replace("\n", "\\n")
					 .Replace("\r", "\\r")
					 ;
		}

		string decode(string s)
		{
			return string.IsNullOrEmpty(s) ? s :
					s.Replace("\\r", "\r")
					 .Replace("\\p", "|")
					 .Replace("\\n", "\n")
					 .Replace("\\\\", "\\")
					 ;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("title={0}", HttpUtility.UrlEncode(PaperTitle));
			sb.AppendFormat("&authors={0}", HttpUtility.UrlEncode(Authors));
			sb.AppendFormat("&journal={0}", HttpUtility.UrlEncode(Journal));
			sb.AppendFormat("&description={0}", HttpUtility.UrlEncode(Description));

			for ( int i = 0; i < m_IDList.Count; ++i ) {
				sb.AppendFormat("&id{0}={1}", i, HttpUtility.UrlEncode((string)m_IDList[i]));
			}

			return sb.ToString();
		}

		public void Parse(string s)
		{
			if ( string.IsNullOrEmpty(s) )
				return;
			NameValueCollection nvc = HttpUtility.ParseQueryString(s);
			if ( nvc.Count > 0 && s.Contains("=") ) {
				PaperTitle = nvc["title"];
				Authors = nvc["authors"];
				Journal = nvc["journal"];
				Description = nvc["description"];
				for ( int i = 0; i < 100; ++i ) {
					string name = string.Format("id{0}", i);
					if ( string.IsNullOrEmpty(nvc[name]) )
						break;
					m_IDList.Add(nvc[name]);
				}
			}
			else { // old format
				string[] ss = s.Split('|');
				if ( ss.Length >= 1 )
					PaperTitle = decode(ss[0]);
				if ( ss.Length >= 2 )
					Authors = decode(ss[1]);
				if ( ss.Length >= 3 )
					Journal = decode(ss[2]);
				for ( int i = 3; i < ss.Length; ++i ) {
					if ( ss[i] != null )
						m_IDList.Add(ss[i]);
				}
			}
		}

		public int CompareTo(object obj)
		{
			if ( obj is Citation ) {
				Citation c = (Citation)obj;

				return ID.CompareTo(c.ID);
			}

			throw new ArgumentException("object is not a Citation");
		}

		public string ToXml()
		{
			XmlDocument xd = new XmlDocument();
			XmlElement root = xd.CreateElement("reference");

			foreach ( DictionaryEntry de in m_TrackingTags ) {
				root.SetAttribute(de.Key.ToString(), de.Value.ToString());
			}

			if ( m_IDList.Count > 0 ) {
				XmlElement xIds = xd.CreateElement("identifiers");
				foreach ( string id in m_IDList ) {
					XmlElement xe = xd.CreateElement("id");
					xe.InnerText = id;
					xIds.AppendChild(xe);
				}
				root.AppendChild(xIds);
			}
			if ( !String.IsNullOrEmpty(Authors) ||
                !String.IsNullOrEmpty(PaperTitle) ||
                !String.IsNullOrEmpty(Journal) ) {
				XmlElement c = xd.CreateElement("citation");
				XmlElement x = xd.CreateElement("authors");
				if ( !string.IsNullOrEmpty(Authors) ) {
					x.InnerText = Authors;
					c.AppendChild(x);
				}
				x = xd.CreateElement("title");
				x.InnerText = PaperTitle;
				c.AppendChild(x);
				if ( !string.IsNullOrEmpty(Journal) ) {
					x = xd.CreateElement("journal");
					x.InnerText = Journal;
					c.AppendChild(x);
				}
				if ( !string.IsNullOrEmpty(Description) ) {
					x = xd.CreateElement("description");
					x.InnerText = Description;
					c.AppendChild(x);
				}
				root.AppendChild(c);
			}
			StringWriter sw = new StringWriter();
			XmlWriter xw = new XmlTextWriter(sw);
			xd.AppendChild(root);
			xd.WriteTo(xw);
			return sw.ToString();
		}

		string x_NodeText(XmlDocument xd, string xpath)
		{
			XmlNode xn = xd.SelectSingleNode(xpath);
			return xn != null ? xn.InnerText : "";
		}

		public void FromXml(string xml)
		{
			XmlDocument xd = new XmlDocument();
			xd.LoadXml(xml);
			XmlElement root = (XmlElement)xd.SelectSingleNode("/reference");
			foreach ( XmlAttribute xa in root.Attributes ) {
				m_TrackingTags[xa.Name] = xa.Value;
			}
			foreach ( XmlElement x in xd.SelectNodes("/reference/identifiers/id") ) {
				AddID(x.InnerText);
			}
			Authors = x_NodeText(xd, "/reference/citation/authors");
			PaperTitle = x_NodeText(xd, "/reference/citation/title");
			Journal = x_NodeText(xd, "/reference/citation/journal");
			Description = x_NodeText(xd, "/reference/citation/description");
		}
	}
}