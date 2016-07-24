namespace RSC.Process.EntityFramework
{
	using System;
	using System.Data.Entity;
	using System.ComponentModel.DataAnnotations.Schema;
	using System.Linq;
	using System.Data.Entity.ModelConfiguration.Conventions;
	using RSC.Process.ChunkManager.EntityFramework;

	public partial class ChunkManagerContext : DbContext
	{
		public ChunkManagerContext()
			: base("ChunkManagerConnection")
		{
			//Database.SetInitializer<ChunkManagerContext>(new CreateDatabaseIfNotExists<ChunkManagerContext>());
			Database.SetInitializer<ChunkManagerContext>(new MigrateDatabaseToLatestVersion<ChunkManagerContext, RSC.Process.EntityFramework.ChunkManagerMigrations.Configuration>());

			//	http://stackoverflow.com/questions/6232633/entity-framework-timeouts
			Database.CommandTimeout = 300;
		}

		public virtual DbSet<Chunk> Chunks { get; set; }
		public virtual DbSet<Parameter> Parameters { get; set; }
		public virtual DbSet<Blob> Blobs { get; set; }
	}
}
