using System.Data.Entity;

namespace RSC.Compounds.EntityFramework
{
	public partial class TestSynonymsContext : DbContext
	{
		public TestSynonymsContext()
			: base("CompoundsConnection")
		{
			
		}

		public virtual DbSet<ef_Synonym> Synonyms { get; set; }
		public virtual DbSet<ef_SynonymFlag> SynonymFlags { get; set; }
		public virtual DbSet<ef_CompoundSynonym> CompoundSynonyms { get; set; }
		public virtual DbSet<ef_CompoundSynonymHistory> CompoundSynonymsHistory { get; set; }

	}
}
