using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSC.Compounds.NMRFeatures.Models;

namespace RSC.Compounds.NMRFeatures
{
	public class NMRFeaturesConfiguration : DbMigrationsConfiguration<NMRFeaturesDbContext>
	{
		public NMRFeaturesConfiguration()
		{
			AutomaticMigrationsEnabled = true;
			AutomaticMigrationDataLossAllowed = true;
		}

		protected override void Seed(NMRFeaturesDbContext context)
		{
			var features = new List<NMRFeature> {
				new NMRFeature { Name = "cc4", Description = "tetrasubstituted C=C (non-aromatic)" },
				new NMRFeature { Name = "co", Description = "C=O" },
				new NMRFeature { Name = "cao", Description = "all C-O but not including O-C-O or O-CH-O or O-CH2-O" },
				new NMRFeature { Name = "cn", Description = "C=N (not pyridine)" },
				new NMRFeature { Name = "met", Description = "all methyls" },
				new NMRFeature { Name = "cc3", Description = "trisubstituted C=C" },
				new NMRFeature { Name = "cch", Description = "1,1-disubstituted C=C" },
				new NMRFeature { Name = "vn", Description = "monosubstituted C=C (vinyl)" },
				new NMRFeature { Name = "cc", Description = "all C=C (not aromatic)" },
				new NMRFeature { Name = "dic", Description = "1,2-disubstituted C=C" },
				new NMRFeature { Name = "acy", Description = "CH3-C=O" },
				new NMRFeature { Name = "meo", Description = "O-CH3" },
				new NMRFeature { Name = "ac", Description = "carboxylic acid" },
				new NMRFeature { Name = "am", Description = "amide" },
				new NMRFeature { Name = "es", Description = "ester or lactone" },
				new NMRFeature { Name = "cho", Description = "HC=O aldehyde" },
				new NMRFeature { Name = "c1ho", Description = "CH-O" },
				new NMRFeature { Name = "c2ho", Description = "CH2-O" },
				new NMRFeature { Name = "men", Description = "N-CH3" },
				new NMRFeature { Name = "co2", Description = "O-C-O" },
				new NMRFeature { Name = "cho2", Description = "O-CH-O" },
				new NMRFeature { Name = "ch2o2", Description = "O-CH2-O" },
				new NMRFeature { Name = "aco", Description = "all esters, lactones, carboxylic acids (es+ac)" },
				new NMRFeature { Name = "mets", Description = "CH3-C singlet" },
				new NMRFeature { Name = "metd", Description = "CH3-C doublet" },
				new NMRFeature { Name = "mett", Description = "CH3-C triplet" },
				new NMRFeature { Name = "recnt", Description = "# of aromatic rings (error if 20)" },
				new NMRFeature { Name = "b1", Description = "monosubstituted benzene" },
				new NMRFeature { Name = "b12", Description = "1,2-disubstituted benzene" },
				new NMRFeature { Name = "b13", Description = "1,3-disubstituted benzene" },
				new NMRFeature { Name = "b14", Description = "1,4-disubstituted benzene" },
				new NMRFeature { Name = "b123", Description = "1,2,3-trisubstituted benzene" },
				new NMRFeature { Name = "b124", Description = "1,2,4-trisubstituted benzene" },
				new NMRFeature { Name = "b135", Description = "1,3,5-trisubstituted benzene" },
				new NMRFeature { Name = "b1234", Description = "1,2,3,4-tetrasubstituted benzene" },
				new NMRFeature { Name = "b1235", Description = "1,2,3,5-tetrasubstituted benzene" },
				new NMRFeature { Name = "b1245", Description = "1,2,4,5-tetrasubstituted benzene" },
				new NMRFeature { Name = "b12345", Description = "1,2,3,4,5-pentasubstituted benzene" },
				new NMRFeature { Name = "p2", Description = "2-monosubstituted pyridine" },
				new NMRFeature { Name = "p3", Description = "3-monosubstituted pyridine" },
				new NMRFeature { Name = "p4", Description = "4-monosubstituted pyridine" },
				new NMRFeature { Name = "p23", Description = "2,3-disubstituted pyridine" },
				new NMRFeature { Name = "p24", Description = "2,4-disubstituted pyridine" },
				new NMRFeature { Name = "p25", Description = "2.5-disubstituted pyridine" },
				new NMRFeature { Name = "p26", Description = "2,6-disubstituted pyridine" },
				new NMRFeature { Name = "p34", Description = "3,4-disubstituted pyridine" },
				new NMRFeature { Name = "p35", Description = "3,5-disubstituted pyridine" },
				new NMRFeature { Name = "p234", Description = "2,3,4-trisubstituted pyridine" },
				new NMRFeature { Name = "p235", Description = "2,3,5-trisubstituted pyridine" },
				new NMRFeature { Name = "p236", Description = "2,3,6-trisubstituted pyridine" },
				new NMRFeature { Name = "p246", Description = "2,4,6-trisubstituted pyridine" },
				new NMRFeature { Name = "p345", Description = "3,4,5-trisubstituted pyridine" },
				new NMRFeature { Name = "p245", Description = "2,4,5-trisubstituted pyridine" },
				new NMRFeature { Name = "p2345", Description = "2,3,4,5-tetrasubstituted pyridine" },
				new NMRFeature { Name = "p2346", Description = "2,3,4,6-tetrasubstituted pyridine" },
				new NMRFeature { Name = "p2356", Description = "2,3,5,6-tetrasubstituted pyridine" },
				new NMRFeature { Name = "mine", Description = "sp3 CH" },
				new NMRFeature { Name = "mene", Description = "-CH2-" },
				new NMRFeature { Name = "ben", Description = "all benzene rings" },
				new NMRFeature { Name = "vnme", Description = "CH3-C=C (non aromatic)" },
				new NMRFeature { Name = "tlk", Description = "terminal alkyne" },
				new NMRFeature { Name = "initr", Description = "isonitrile" },
				new NMRFeature { Name = "nit", Description = "nitrile" },
				new NMRFeature { Name = "sp2h", Description = "all sp2 Hs" },
				new NMRFeature { Name = "chn", Description = "-CH=N (not pyridine)" },
				new NMRFeature { Name = "mes", Description = "S-CH3" },
				new NMRFeature { Name = "arome", Description = "CH3-C=C (aromatic)" },
				new NMRFeature { Name = "b1_6", Description = "hexasubstituted benzene" },
				new NMRFeature { Name = "p2_6", Description = "pentasubstituted pyridine" }
			};

			var existing = from f in context.NMRFeatures select f.Name;
			features.Where(e => !existing.Contains(e.Name)).ToList().ForEach(f => context.NMRFeatures.Add(f));
			context.SaveChanges();
		}
	}
}
