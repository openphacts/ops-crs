using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSC.CVSP;
using RSC.CVSP.Compounds;
using com.ggasoftware.indigo;
using System;

namespace CVSPTests
{
    /// <summary>
    /// 2015-08-27: copied to RSCCheminfToolkit.
    /// </summary>
	[TestClass]
	public class CheminformaticsSmarts
	{
		[TestMethod]
		public void CheminformaticsSmarts_MatchSubstructures()
		{
			string mol = @"
  Ketcher 11241409582D 1   1.00000     0.00000     0

 14 14  0     0  0            999 V2000
    3.5750   -3.8750    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    4.4410   -4.3750    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.3071   -3.8750    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    6.1731   -4.3750    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.0391   -3.8750    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.9051   -4.3750    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.7712   -3.8750    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.9051   -5.3750    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.7711   -5.8750    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.7711   -6.8750    0.0000 S   0  0  0  0  0  0  0  0  0  0  0  0
    7.9051   -7.3750    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.0391   -6.8750    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.0391   -5.8750    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    9.1391   -4.9250    0.0000 S   0  0  0  0  0  0  0  0  0  0  0  0
  1  2  1  0     0  0
  2  3  2  0     0  0
  3  4  1  0     0  0
  4  5  1  0     0  0
  5  6  1  0     0  0
  6  7  1  1     0  0
  6  8  1  0     0  0
  8  9  1  0     0  0
  9 10  1  0     0  0
 10 11  1  0     0  0
 11 12  1  0     0  0
 12 13  1  0     0  0
 13  8  1  0     0  0
  6 14  1  6     0  0
M  END";
			Indigo i = new Indigo();
			IndigoObject mol_obj = i.loadMolecule(mol);
			List<string> test_SMARTS = new List<string>() { 
				"[C&!r][C](\\[S])/[C&H3]", "[S&r6]","[X4&H0]"
			};

			foreach (string smarts in test_SMARTS)
			{
				try
				{
					IndigoObject smarts_obj = i.loadSmarts(smarts);
					int count = i.substructureMatcher(mol_obj).countMatches(smarts_obj);
					Assert.IsTrue(count != 0);
				}
				catch(Exception)
				{
					Assert.IsTrue(1 == 0);
				}
			}
			Assert.IsTrue(1 == 1);
		}
	}
}
