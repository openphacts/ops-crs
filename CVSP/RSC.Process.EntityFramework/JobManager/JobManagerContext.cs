namespace RSC.Process.EntityFramework
{
	using System;
	using System.Data.Entity;
	using System.ComponentModel.DataAnnotations.Schema;
	using System.Linq;
	using System.Data.Entity.ModelConfiguration.Conventions;
	using RSC.Process.JobManager.EntityFramework;
	using System.Data.Entity.Infrastructure;

	public partial class JobManagerContext : DbContext
	{
		public JobManagerContext()
			: base("JobManagerConnection")
		{
			Database.SetInitializer<JobManagerContext>(new CreateDatabaseIfNotExists<JobManagerContext>());

			//	http://stackoverflow.com/questions/6232633/entity-framework-timeouts
			Database.CommandTimeout = 300;
		}

		public virtual DbSet<Job> Jobs { get; set; }
		public virtual DbSet<Parameter> Parameters { get; set; }
		public virtual DbSet<Watch> Watches { get; set; }
	}
}
