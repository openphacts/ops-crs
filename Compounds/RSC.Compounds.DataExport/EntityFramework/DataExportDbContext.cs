using System;
using System.Configuration;
using System.Data.Entity;
using RSC.Compounds.DataExport.EntityFramework;
using RSC.Config;

namespace RSC.Compounds.DataExport
{
    public partial class DataExportContext : DbContext
    {
		private const string connectionStringName = "CompoundsConnection";

        public DataExportContext()
			: base(connectionStringName)
        {
            Database.SetInitializer<DataExportContext>(new MigrateDatabaseToLatestVersion<DataExportContext, RSC.Compounds.DataExport.Configuration>());

			if ( ConfigurationManager.ConnectionStrings[connectionStringName] != null )
				Database.CommandTimeout = ConfigurationManager.ConnectionStrings[connectionStringName].Timeout();

			if ( "true" == ConfigurationManager.AppSettings["ef.debug"] )
				Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
        }

        public virtual DbSet<ef_DataExportLog> Logs { get; set; }
        public virtual DbSet<ef_DataExportLogFile> Files { get; set; }
        public virtual DbSet<ef_DataVersion> DataVersions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ef_DataExportLog>().HasMany(l => l.Files);
        }
    }
}
