using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ChemSpider.ObjectModel
{
    public enum RefType
    {
        WebLink,
        DataSourceLink,
        ArticleLink,
    }

    /// <summary>
    /// Machine readable identifiers
    /// </summary>
    [DataContract(Namespace = "")]
    public class Reference
    {
        [DataMember(EmitDefaultValue = false)]
        public string Source
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        public string Text
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        public Uri Link
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        public RefType RefType
        {
            get;
            set;
        }

        public override string ToString()
        {
            if ( Link != null && String.IsNullOrEmpty(Text) )
                return String.Format("{0}", Link);
            else if ( Link == null && !String.IsNullOrEmpty(Text) )
                return String.Format("{0}", Text);
            else if ( Link != null && !String.IsNullOrEmpty(Text) )
                return String.Format("{0} [{1}]", Link, Text);
            else
                return String.Empty;
        }
    }
}
