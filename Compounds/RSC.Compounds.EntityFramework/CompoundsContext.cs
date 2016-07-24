using System;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using RSC.Config;

namespace RSC.Compounds.EntityFramework
{
	public partial class CompoundsContext : DbContext
	{
		private const string connectionStringName = "CompoundsConnection";
		private const string efDebugName = "DebugEF";

		public CompoundsContext()
			: base(connectionStringName)
		{
			Database.SetInitializer<CompoundsContext>(new CreateDatabaseIfNotExists<CompoundsContext>());
			//Database.SetInitializer<CompoundsContext>(new MigrateDatabaseToLatestVersion<CompoundsContext, RSC.Compounds.EntityFramework.Migrations.Configuration>());

			//	http://stackoverflow.com/questions/6232633/entity-framework-timeouts
			if ( ConfigurationManager.ConnectionStrings[connectionStringName] != null )
				Database.CommandTimeout = ConfigurationManager.ConnectionStrings[connectionStringName].Timeout();

			if ( !String.IsNullOrEmpty(ConfigurationManager.AppSettings[efDebugName])
				&& ( String.Equals(ConfigurationManager.AppSettings[efDebugName], "1")
					|| String.Equals(ConfigurationManager.AppSettings[efDebugName], "true", StringComparison.InvariantCultureIgnoreCase) ) )
			{
				Database.Log = s => Trace.TraceInformation(s);
			}
		}

		public virtual DbSet<ef_Compound> Compounds { get; set; }
        public virtual DbSet<ef_InChI> InChIs { get; set; }
        public virtual DbSet<ef_InChIMD5> InChI_MD5s { get; set; }
        public virtual DbSet<ef_ParentChild> ParentChildren { get; set; }
        public virtual DbSet<ef_Smiles> Smiles { get; set; }
        public virtual DbSet<ef_Substance> Substances { get; set; }
        public virtual DbSet<ef_Annotation> Annotations { get; set; }
        public virtual DbSet<ef_Issue> Issues { get; set; }
		public virtual DbSet<ef_Property> Properties { get; set; }
		public virtual DbSet<ef_Revision> Revisions { get; set; }
        public virtual DbSet<ef_Synonym> Synonyms { get; set; }
        public virtual DbSet<ef_SynonymFlag> SynonymFlags { get; set; }
        public virtual DbSet<ef_CompoundSynonym> CompoundSynonyms { get; set; }
        public virtual DbSet<ef_CompoundSynonymSynonymFlag> CompoundSynonymSynonymFlags { get; set; }
        public virtual DbSet<ef_SynonymSynonymFlag> SynonymSynonymFlags { get; set; }
        public virtual DbSet<ef_CompoundSynonymHistory> CompoundSynonymHistory { get; set; }
        public virtual DbSet<ef_CompoundSynonymSynonymFlagHistory> CompoundSynonymSynonymFlagHistory { get; set; }
        public virtual DbSet<ef_SynonymHistory> SynonymHistory { get; set; }
        public virtual DbSet<ef_SynonymSynonymFlagHistory> SynonymSynonymFlagHistory { get; set; }
        public virtual DbSet<ef_ExternalReference> ExternalReferences { get; set; }
        public virtual DbSet<ef_ExternalReferenceType> ExternalReferenceTypes { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
            modelBuilder.Entity<ef_Issue>().HasRequired(c => c.Revision).WithMany(p => p.Issues).WillCascadeOnDelete(true);
            modelBuilder.Entity<ef_Annotation>().HasRequired(c => c.Revision).WithMany(p => p.Annotations).WillCascadeOnDelete(true);
            modelBuilder.Entity<ef_Revision>().HasMany(c => c.Annotations).WithRequired(p => p.Revision).WillCascadeOnDelete(true);
            modelBuilder.Entity<ef_Revision>().HasMany(c => c.Issues).WithRequired(p => p.Revision).WillCascadeOnDelete(true);
            
            //Inchis.
            modelBuilder.Entity<ef_Compound>().HasOptional(c => c.StandardInChI);
            modelBuilder.Entity<ef_Compound>().HasOptional(c => c.TautomericNonStdInChI);
            modelBuilder.Entity<ef_Compound>().HasOptional(c => c.NonStandardInChI);
		
            //Parent child.
            modelBuilder.Entity<ef_Compound>().HasMany(c => c.Children).WithRequired(p => p.Child).WillCascadeOnDelete(false);
            modelBuilder.Entity<ef_Compound>().HasMany(c => c.Parents).WithRequired(p => p.Parent).WillCascadeOnDelete(false);

            //Synonyms.
            modelBuilder.Entity<ef_Compound>().HasMany(c => c.CompoundSynonyms);
            modelBuilder.Entity<ef_Synonym>().HasMany(c => c.SynonymFlags);
            modelBuilder.Entity<ef_Synonym>().HasMany(c => c.Revisions);
            modelBuilder.Entity<ef_CompoundSynonym>().HasMany(c => c.CompoundSynonymSynonymFlags);
            modelBuilder.Entity<ef_CompoundSynonymHistory>().HasMany(c => c.SynonymFlags);
            modelBuilder.Entity<ef_SynonymHistory>().HasMany(c => c.SynonymFlags);

            //Identifiers.
            modelBuilder.Entity<ef_Compound>().HasMany(c => c.ExternalReferences);
		    modelBuilder.Entity<ef_ExternalReference>().HasRequired(c => c.Type);
		}

		public override int SaveChanges()
		{
			var changedEntities = ChangeTracker.Entries();

			foreach (var changedEntity in changedEntities)
			{
				if (changedEntity.Entity is IBeforeInsert && changedEntity.State == EntityState.Added)
				{
					var entity = changedEntity.Entity as IBeforeInsert;
					entity.OnBeforeInsert();
				}
				if (changedEntity.Entity is IBeforeUpdate && changedEntity.State == EntityState.Modified)
				{
					var entity = changedEntity.Entity as IBeforeUpdate;
					entity.OnBeforeUpdate();
				}
				if (changedEntity.Entity is IBeforeDelete && changedEntity.State == EntityState.Deleted)
				{
					var entity = changedEntity.Entity as IBeforeDelete;
					entity.OnBeforeDelete();
				}
			}

			return base.SaveChanges();
		}
	}
}
