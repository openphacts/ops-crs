using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RSC.CVSP.Compounds;
using MoleculeObjects;
using com.ggasoftware.indigo;

namespace CVSPTests
{
    [TestClass]
    public class StandardizationFragments : CVSPTestBase
    {
        static Indigo i;

        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            i = new Indigo();
        }

        [TestMethod]
        public void StandardizationFragments_RemoveChargedAcidBaseResidues()
        {
            string input = i.loadMolecule("C1C=CC=CC=C1.[Cl-].[Na+]").molfile();
            string result = StandardizationFragmentsModule.removeChargedAcidBaseResidues(input);
            Assert.AreEqual("[Na+].C1C=CC=CC=C1", i.loadMolecule(result).canonicalSmiles());
        }

        [TestMethod]
        public void StandardizationFragments_LargestOrganicFragment()
        {
            string molfile = @"375
  -INDIGO-02121313452D

 18 17  0  0  0  0  0  0  0  0999 V2000
    3.3626    0.8043    0.0000 Cl  0  0  0  0  0  0  0  0  0  0  0  0
    5.2040    1.5189    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    4.7299    3.6804    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    6.8734    4.0929    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    2.5864    4.0929    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    9.0168    3.6804    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    0.4430    3.6804    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    4.7915    0.8043    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.6166    0.8043    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    4.0771    0.3918    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.4444    4.0929    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    4.0154    4.0929    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    6.1588    3.6804    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    3.3009    3.6804    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.5879    3.6804    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    1.8720    3.6804    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.3023    4.0929    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    1.1575    4.0929    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
  1 10  1  0  0  0  0
  2  8  1  0  0  0  0
  2  9  1  0  0  0  0
  3 11  1  0  0  0  0
  3 12  1  0  0  0  0
  4 13  1  0  0  0  0
  4 15  1  0  0  0  0
  5 14  1  0  0  0  0
  5 16  1  0  0  0  0
  6 17  1  0  0  0  0
  7 18  1  0  0  0  0
  8  9  1  0  0  0  0
  8 10  1  0  0  0  0
 11 13  1  0  0  0  0
 12 14  1  0  0  0  0
 15 17  1  0  0  0  0
 16 18  1  0  0  0  0
M  END
";
            Indigo i = new Indigo();
            IndigoInchi i_inchi = new IndigoInchi(i);
            i.setOption("inchi-options", "/SUU /SLUUD /FixedH /SUCF");
            IndigoObject input_obj = i.loadMolecule(molfile);
            string non_std_input_inchi = i_inchi.getInchi(input_obj);
			string inchi, inchi_key;
            string output_molfile = StandardizationFragmentsModule.getLargestOrganicFragment(molfile, out inchi, out inchi_key);
            IndigoObject output_obj = i.loadMolecule(output_molfile);
            string non_std_output_inchi = i_inchi.getInchi(output_obj);

            Assert.AreEqual(non_std_output_inchi, "InChI=1/C8H23N5/c9-1-3-11-5-7-13-8-6-12-4-2-10/h11-13H,1-10H2");
        

            molfile = @"
  -INDIGO-02141310272D

 19 12  0  0  0  0  0  0  0  0999 V2000
    8.9063   -8.0042    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.9512   -7.3104    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.3160   -6.1875    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    9.4965   -6.1875    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    9.8613   -7.3104    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   11.8125   -8.1875    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   13.5625   -6.9688    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   13.3125   -4.2500    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    4.4063  -10.7813    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    4.2500   -3.6563    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
   18.4063   -4.3438    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   19.2188   -5.8438    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
   16.0625  -10.1250    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   17.9063   -9.5625    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   10.9688  -12.4688    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   11.9916  -11.8782    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   11.0625  -14.3125    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    9.8438   -1.9688    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    3.8750   -6.5000    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
  5  1  1  0  0  0  0
  4  5  1  0  0  0  0
  3  4  1  0  0  0  0
  1  2  1  0  0  0  0
  2  3  1  0  0  0  0
  5  6  1  0  0  0  0
  6  7  2  0  0  0  0
  7  8  1  0  0  0  0
 12 11  1  0  0  0  0
 14 13  1  0  0  0  0
 15 16  1  0  0  0  0
 17 15  1  0  0  0  0
M  END";
            i.setOption("inchi-options", "/SUU /SLUUD /FixedH /SUCF");
            input_obj = i.loadMolecule(molfile);
            non_std_input_inchi = i_inchi.getInchi(input_obj);
            output_molfile = StandardizationFragmentsModule.getLargestOrganicFragment(molfile, out inchi, out inchi_key);
            output_obj = i.loadMolecule(output_molfile);
            non_std_output_inchi = i_inchi.getInchi(output_obj);

            Assert.AreEqual(non_std_output_inchi, "InChI=1/C8H14/c1-2-5-8-6-3-4-7-8/h2,5,8H,3-4,6-7H2,1H3/b5-2-");
        }
    }
}
