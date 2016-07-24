namespace RSC.CVSP.EntityFramework.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;

	internal sealed class Configuration : DbMigrationsConfiguration<RSC.CVSP.EntityFramework.CVSPContext>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = true;
			AutomaticMigrationDataLossAllowed = true;

			CommandTimeout = 60 * 30;	//	30 mins
		}

		protected override void Seed(RSC.CVSP.EntityFramework.CVSPContext context)
		{
			//  This method will be called after migrating to the latest version.

			context.Annotations.AddOrUpdate(
				a => a.Name,
				new ef_Annotation { Name = "ExtId", Title = "External ID", IsRequired = true },
				new ef_Annotation { Name = "InChI", Title = "InChI" },
				new ef_Annotation { Name = "InChIKey", Title = "InChI Key" },
				new ef_Annotation { Name = "SMILES", Title = "SMILES" },
				new ef_Annotation { Name = "Synonym", Title = "Synonym" },
				new ef_Annotation { Name = "Xref", Title = "Xref" },
				new ef_Annotation { Name = "Comment", Title = "Comment" }
			);
		}
	}
}
