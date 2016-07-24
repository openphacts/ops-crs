namespace RSC.Process.EntityFramework.ChunkManagerMigrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<RSC.Process.EntityFramework.ChunkManagerContext>
    {
        public Configuration()
        {
			AutomaticMigrationsEnabled = true;
			AutomaticMigrationDataLossAllowed = true;
			MigrationsDirectory = @"ChunkManagerMigrations";
        }

        protected override void Seed(RSC.Process.EntityFramework.ChunkManagerContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
