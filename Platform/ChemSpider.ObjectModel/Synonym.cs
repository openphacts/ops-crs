using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace ChemSpider.ObjectModel
{
	public enum SynonymType
	{
		SystematicName,
		TrivialName,
		CASNO,
		EINECS,
		WLN,
		Synonym,
	}

	public enum Reliability
	{
		Uncertaint,
		Rejected,
		Deleted,
		Confirmed,
		Approved,
	}

    [DataContract(Namespace="")]
	public class Synonym
	{
        [DataMember]
		public string Name
		{
			get;
			set;
		}

        [DataMember(EmitDefaultValue = false)]
		public string LangID
		{
			get;
			set;
		}

        [DataMember]
		public SynonymType SynonymType
		{
			get;
			set;
		}

        [DataMember]
		public Reliability Reliability
		{
			get;
			set;
		}

        [DataMember(EmitDefaultValue = false)]
        public int? Rank
        {
            get;
            set;
        }

        private string _flags = string.Empty;

        public string Flags
        {
            get { return _flags; }
            set
            {
                _flags = value;

                parseFlags(_flags);
            }
        }

        private void parseFlags(string flags)
        {
            if (!string.IsNullOrEmpty(flags))
            {
                XElement root = XElement.Parse(flags);

                if ((from f in root.Elements("flags") where (string)f.Attribute("name") == "RN" select f).Any())
                    SynonymType = SynonymType.CASNO;
                else if ((from f in root.Elements("flags") where (string)f.Attribute("name") == "EINECS" select f).Any())
                    SynonymType = SynonymType.EINECS;
                else if ((from f in root.Elements("flags") where (string)f.Attribute("name") == "WLN" select f).Any())
                    SynonymType = SynonymType.WLN;
                else
                    SynonymType = SynonymType.Synonym;

                var ranks = (from f in root.Elements("flags") where f.Attribute("rank") != null select Convert.ToInt32(f.Attribute("rank").Value));
                if(ranks.Any())
                    Rank = ranks.Max();
            }
        }

        public override string ToString()
        {
            return String.Format("{0} [{1} {2}]", Name, SynonymType.ToString(), Reliability.ToString());
        }
	}
}
