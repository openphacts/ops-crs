namespace RSC.CVSP.EntityFramework
{
	using System.Data.Entity;
	using RSC.CVSP.EntityFramework.Migrations;

	internal class CVSPContext : DbContext
	{
		public CVSPContext()
		{
		}

		internal CVSPContext(string connectionString, int timeout)
			: base(connectionString)
		{
			Database.SetInitializer(new MigrateDatabaseToLatestVersion<CVSPContext, Configuration>(true));

			this.Database.CommandTimeout = timeout;
		}

		public virtual DbSet<ef_Annotation> Annotations { get; set; }

		public virtual DbSet<ef_Deposition> Depositions { get; set; }

		public virtual DbSet<ef_Field> Fields { get; set; }

		public virtual DbSet<ef_DepositionFile> Files { get; set; }

		public virtual DbSet<ef_Issue> Issues { get; set; }

		public virtual DbSet<ef_ProcessingParameter> ProcessingParameters { get; set; }

		public virtual DbSet<ef_Property> Properties { get; set; }

		public virtual DbSet<ef_RecordField> RecordFields { get; set; }

		public virtual DbSet<ef_Record> Records { get; set; }

		public virtual DbSet<ef_RuleSet> RuleSets { get; set; }

		public virtual DbSet<ef_UserProfile> UserProfiles { get; set; }

		public virtual DbSet<ef_UserVariable> UserVariables { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			////on cascade deletes for Record dependents
			//modelBuilder.Entity<Record>().HasRequired(r => r.Deposition);
			//modelBuilder.Entity<Record>().HasMany(r => r.RecordAnnotations).WithRequired(c => c.Record).WillCascadeOnDelete(true);
			modelBuilder.Entity<ef_Record>().HasMany(r => r.Issues).WithRequired(c => c.Record).WillCascadeOnDelete(true);

			//modelBuilder.Entity<Record>().HasOptional(r => r.ReactionExtras).WithRequired(c => c.Record).Map(m => m.MapKey("RecordId")).WillCascadeOnDelete(true);
			//modelBuilder.Entity<Record>().HasOptional(r => r.GeneratedInchis).WithRequired(c => c.Record).Map(m => m.MapKey("RecordId")).WillCascadeOnDelete(true);
			//modelBuilder.Entity<Record>().HasOptional(r => r.GeneratedSmiles).WithRequired(c => c.Record).Map(m => m.MapKey("RecordId")).WillCascadeOnDelete(true);

			////on cascade deletions for RuleSet
			modelBuilder.Entity<ef_RuleSet>().HasMany(r => r.Collaboraters).WithRequired(c => c.RuleSet).WillCascadeOnDelete(true);

			////on cascade deletions for Deposition
			modelBuilder.Entity<ef_Deposition>().HasMany(r => r.Records).WithRequired(c => c.Deposition).WillCascadeOnDelete(false);
			modelBuilder.Entity<ef_Deposition>().HasMany(r => r.Files).WithRequired(c => c.Deposition).WillCascadeOnDelete(true);

			//modelBuilder.Entity<Deposition>().HasMany(r => r.SDFFieldMaps).WithRequired(c => c.Deposition).WillCascadeOnDelete(true);
			//modelBuilder.Entity<Deposition>().HasRequired(r => r.ProcessingParameters).WithRequiredPrincipal(c => c.Deposition).Map(m => m.MapKey("DepositionId")).WillCascadeOnDelete(true);

			//on cascade deletions for UserProfile
			modelBuilder.Entity<ef_UserProfile>().HasMany(r => r.VariableCollection).WithRequired(c => c.UserProfile).WillCascadeOnDelete(true);
			modelBuilder.Entity<ef_UserProfile>().HasMany(r => r.RuleSetCollection).WithRequired(c => c.UserProfile).WillCascadeOnDelete(true);

			//on cascade for File
			modelBuilder.Entity<ef_DepositionFile>().HasMany(r => r.Records).WithRequired(c => c.File).WillCascadeOnDelete(true);

			//ProcessingParameters
			//modelBuilder.Entity<ProcessingParameters>().HasRequired(r => r.Deposition);

			//modelBuilder.Entity<Field>().HasRequired(f => f.File).WithMany().WillCascadeOnDelete(false);
			modelBuilder.Entity<ef_Field>().HasMany(f => f.Values).WithRequired(c => c.Field).WillCascadeOnDelete(false);
		}
	}
}
