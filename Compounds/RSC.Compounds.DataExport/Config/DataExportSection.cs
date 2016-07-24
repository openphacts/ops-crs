using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.DataExport
{
    public class DataExportsCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DataExportElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DataExportElement)element).DataSourceId;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        public void Add(DataExportElement element)
        {
            this.BaseAdd(element);
        }

        public DataExportElement this[Guid id]
        {
            get
            {
                return this.OfType<DataExportElement>().FirstOrDefault(item => item.DataSourceId == id);
            }
        }
    }

    public class DataExportSection : ConfigurationSection
    {
        [ConfigurationProperty("DataExports")]
        [ConfigurationCollection(typeof(DataExportsCollection), AddItemName = "add")]
        public DataExportsCollection DataExports
        {
            get { return this["DataExports"] as DataExportsCollection; }
        }
    }

    public class DataExportElement : ConfigurationElement
    {
        [ConfigurationProperty("DataSourceId", IsKey = false, IsRequired = true)]
        public Guid DataSourceId
        {
            get { return new Guid(this["DataSourceId"].ToString()); }
        }

        [ConfigurationProperty("VersionName", IsKey = true, IsRequired = true)]
        public string VersionName
        {
            get { return this["VersionName"].ToString(); }
        }

        [ConfigurationProperty("LicenseUri", IsKey = false, IsRequired = true)]
        public string LicenseUri
        {
            get { return this["LicenseUri"].ToString(); }
        }

        [ConfigurationProperty("UriSpace", IsKey = false, IsRequired = true)]
        public string UriSpace
        {
            get { return this["UriSpace"].ToString(); }
        }

        [ConfigurationProperty("VoidUri", IsKey = false, IsRequired = false)]
        public string VoidUri
        {
            get { return this["VoidUri"].ToString(); }
        }

        [ConfigurationProperty("DownloadedBy", IsKey = false, IsRequired = true)]
        public string DownloadedBy
        {
            get { return this["DownloadedBy"].ToString(); }
        }

        [ConfigurationProperty("DownloadUri", IsKey = false, IsRequired = true)]
        public string DownloadUri
        {
            get { return this["DownloadUri"].ToString(); }
        }

        [ConfigurationProperty("CreatedDate", IsKey = false, IsRequired = true)]
        public DateTime CreatedDate
        {
            get { return Convert.ToDateTime(this["CreatedDate"].ToString()); }
        }

        [ConfigurationProperty("DownloadDate", IsKey = false, IsRequired = true)]
        public DateTime DownloadDate
        {
            get { return Convert.ToDateTime(this["DownloadDate"].ToString()); }
        }
    }
}
