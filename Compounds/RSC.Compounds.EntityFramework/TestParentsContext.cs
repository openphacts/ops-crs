using System.Data.Entity;

namespace RSC.Compounds.EntityFramework
{
	public partial class TestParentsContext : DbContext
	{
		public TestParentsContext()
			: base("CompoundsConnection")
		{
			
		}

		public virtual DbSet<ef_Compound> Compounds { get; set; }
		

	}
}
