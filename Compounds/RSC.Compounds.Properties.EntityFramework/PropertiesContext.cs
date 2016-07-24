using RSC.Compounds.Properties.EntityFramework.Migrations;
using System.Data.Entity;

namespace RSC.Compounds.Properties.EntityFramework
{
    public class PropertiesContext : DbContext
    {
		public PropertiesContext()
			: base("PropertiesConnection")
		{
			//Database.SetInitializer<CVSPContext>(new CreateDatabaseIfNotExists<CVSPContext>());
			Database.SetInitializer<PropertiesContext>(new MigrateDatabaseToLatestVersion<PropertiesContext, Configuration>());

			//	http://stackoverflow.com/questions/6232633/entity-framework-timeouts
			Database.CommandTimeout = 300;
		}
        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyValue> PropertyValues { get; set; }
        public DbSet<SoftwareInstrument> SoftwareInstruments { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Company> Companies { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Configure domain classes using Fluent API here
            base.OnModelCreating(modelBuilder);
        }
    }
}

