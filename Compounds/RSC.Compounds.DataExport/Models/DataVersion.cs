using System;

namespace RSC.Compounds.DataExport
{
    public class DataVersion
    {
        public int Id { get; set; }

        public Guid DataSourceId { get; set; }

        public string VersionName { get; set; }

        public string UriSpace { get; set; }

        public string LicenseUri { get; set; }

        public string VoidUri { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime DownloadDate { get; set; }

        public string DownloadedBy { get; set; }

        public string DownloadUri { get; set; }
    }
}
