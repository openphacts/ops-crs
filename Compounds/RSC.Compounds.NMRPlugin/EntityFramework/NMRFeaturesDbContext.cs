using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSC.Compounds.NMRFeatures.Models;

namespace RSC.Compounds.NMRFeatures
{
	public class NMRFeaturesDbContext : DbContext
	{
		public NMRFeaturesDbContext()
			: base("Name=CompoundsConnectionString")
		{
			this.Configuration.ProxyCreationEnabled = false;
		}
		public DbSet<NMRFeature> NMRFeatures { get; set; }
		public DbSet<CompoundNMRFeature> CompoundNMRFeatures { get; set; }
	}
}
