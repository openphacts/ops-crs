using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSC.Compounds.DataExport.EntityFramework
{
    [Table("DataVersion")]
    public class ef_DataVersion
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
        public int Id { get; set; }

        [Required]
        public Guid DataSourceId { get; set; } 

        [Required]
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
