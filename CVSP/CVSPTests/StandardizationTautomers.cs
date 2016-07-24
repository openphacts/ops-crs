using System;
using System.Text;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RSC.CVSP.Compounds;
using MoleculeObjects;
using com.ggasoftware.indigo;
using InChINet;
using RSC.CVSP;
using RSC.Logging;

namespace CVSPTests
{
    //[TestClass]
    public class StandardizationTautomers : CVSPTestBase
    {
		string new_inchi, new_inchi_key;
        [TestMethod]
        public void StandardizationTautomers_ChemAxon_TautomerUnsensitiveParent()
        {
            Indigo i = new Indigo();
            IndigoObject obj1 = i.loadMolecule("C[N+]1(C)CCC2=CC(OC)=C(OC)C3OC4C=CC(CC5C6=CC(OC7=CC(CC1C2=3)=CC=C7OC)=C(C=C6CC[N+]5(C)C)OC)=CC=4");
            obj1.layout();
            obj1.markEitherCisTrans();
            StandardizationModule.CanonicalizeTautomer(obj1.molfile(), false,
				Resources.Vendor.ChemAxon, out new_inchi, out new_inchi_key, InChIFlags.CRS);
            Assert.AreEqual("InChI=1/C40H48N2O6/c1-41(2)17-15-27-22-34(44-6)36-24-30(27)31(41)19-25-9-12-29(13-10-25)47-40-38-28(23-37(45-7)39(40)46-8)16-18-42(3,4)32(38)20-26-11-14-33(43-5)35(21-26)48-36/h9-14,21-24,31-32H,15-20H2,1-8H3/q+2/t31?,32?",
                new_inchi);
        }

        [TestMethod]
        public void StandardizationTautomers_ChemAxon_isCRSDeposition()
        {
            Indigo i = new Indigo();
            IndigoObject obj1 = i.loadMolecule("C[N+]1(C)CCC2=CC(OC)=C(OC)C3OC4C=CC(CC5C6=CC(OC7=CC(CC1C2=3)=CC=C7OC)=C(C=C6CC[N+]5(C)C)OC)=CC=4");
            obj1.layout();
            obj1.markEitherCisTrans();
			string output_mol = StandardizationModule.CanonicalizeTautomer(obj1.molfile(), true,
				Resources.Vendor.ChemAxon, out new_inchi, out new_inchi_key, InChIFlags.CRS);
            IndigoObject output = i.loadMolecule(output_mol);
            Assert.AreEqual("C[N+]1(C)CCC2=CC(OC)=C(OC)C3OC4C=CC(CC5C6=CC(OC7=CC(CC1C2=3)=CC=C7OC)=C(C=C6CC[N+]5(C)C)OC)=CC=4", output.canonicalSmiles(), 
                "problem with chemaxon tautomer tool");
        }

        [TestMethod]
        public void StandardizationTautomers_TestStereoOnSulfoxoStays()
        {
			string output_molfile = StandardizationModule.CanonicalizeTautomer(Resource1.stereosulfoxo, false,
				Resources.Vendor.ChemAxon, out new_inchi, out new_inchi_key, InChIFlags.CRS);
            string non_std_output_inchi = InChIUtils.mol2InChI(output_molfile, InChIFlags.CRS);
            Assert.AreEqual(non_std_output_inchi,"InChI=1/C13H13NO2S/c1-16-11-5-7-12(8-6-11)17(15)13-4-2-3-10(14)9-13/h2-9H,14H2,1H3/t17?");
        }

        
        [TestMethod]
        public void StandardizationTautomers_SteroidStereoNottoChange()
        {
            
            string molfile = @"
  Mrv0541 02231218172D          

 27 30  0  0  1  0            999 V2000
    0.8774   -0.1720    0.0000 C   0  0  1  0  0  0  0  0  0  0  0  0
    0.1618   -0.5747    0.0000 C   0  0  1  0  0  0  0  0  0  0  0  0
    0.8878    0.6538    0.0000 C   0  0  1  0  0  0  0  0  0  0  0  0
    1.6585   -0.4370    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -0.5505   -0.1583    0.0000 C   0  0  1  0  0  0  0  0  0  0  0  0
    0.1618   -1.4004    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    1.6757    0.9015    0.0000 C   0  0  2  0  0  0  0  0  0  0  0  0
    0.1755    1.0736    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    1.0976    1.4521    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    2.1506    0.2271    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -1.2663   -0.5678    0.0000 C   0  0  2  0  0  0  0  0  0  0  0  0
   -0.5436    0.6641    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -0.5574   -1.8133    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    1.9406    1.6792    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -1.2697   -1.3970    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -1.9819   -0.1583    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -1.2765    0.2547    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    2.7493    1.8408    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    1.4004    2.2984    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
   -1.9819   -1.8133    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -2.6976   -0.5678    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    3.2929    1.2215    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
   -2.6976   -1.3970    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -3.4133   -1.8133    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    0.8705   -0.9910    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0
   -0.0447    0.2202    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0
   -0.5574   -0.9807    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0
  1  2  1  0  0  0  0
  1  3  1  0  0  0  0
  1  4  1  0  0  0  0
  2  5  1  0  0  0  0
  2  6  1  0  0  0  0
  3  7  1  0  0  0  0
  3  8  1  0  0  0  0
  3  9  1  1  0  0  0
  4 10  1  0  0  0  0
  5 11  1  0  0  0  0
  5 12  1  0  0  0  0
  6 13  1  0  0  0  0
  7 14  1  1  0  0  0
 11 15  1  0  0  0  0
 11 16  1  0  0  0  0
 11 17  1  1  0  0  0
 14 18  1  0  0  0  0
 14 19  2  0  0  0  0
 15 20  2  0  0  0  0
 16 21  1  0  0  0  0
 18 22  1  0  0  0  0
 20 23  1  0  0  0  0
 23 24  2  0  0  0  0
  7 10  1  0  0  0  0
  8 12  1  0  0  0  0
 13 15  1  0  0  0  0
 21 23  1  0  0  0  0
  1 25  1  6  0  0  0
  2 26  1  1  0  0  0
  5 27  1  6  0  0  0
M  END";
            Indigo i = new Indigo();
            IndigoInchi i_inchi = new IndigoInchi(i);
            i.setOption("inchi-options", "/SUU /SLUUD /FixedH /SUCF");
            IndigoObject input_obj = i.loadMolecule(molfile);
            string non_std_input_inchi = i_inchi.getInchi(input_obj);
            //ChemSpider.CVSP.ObjectModel.Logger logger = Logger.CreateTraceSources(null, null, null);
			string output_molfile = StandardizationModule.CanonicalizeTautomer(molfile, false,
				Resources.Vendor.ChemAxon, out new_inchi, out new_inchi_key, InChIFlags.CRS);

            IndigoObject output_obj = i.loadMolecule(output_molfile);
            string non_std_output_inchi = i_inchi.getInchi(output_obj);
            Assert.AreEqual("InChI=1/C21H30O3/c1-20-9-7-14(23)11-13(20)3-4-15-16-5-6-18(19(24)12-22)21(16,2)10-8-17(15)20/h3,15-18,22H,4-12H2,1-2H3/t15-,16-,17-,18+,20-,21-/s2", non_std_output_inchi);
        }

        [TestMethod]
        public void StandardizationTautomers_checkCanonicalTautomerDiazepine()
        {

            string molfile1 = @"
  -INDIGO-02151310222D

 21 24  0  0  0  0  0  0  0  0999 V2000
    8.6985   -5.9765    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    9.5396   -5.5852    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   10.3771   -5.9989    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    8.4808   -6.8849    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   10.5785   -6.9073    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    9.0556   -7.6205    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    9.9886   -7.6205    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   11.4881   -7.0617    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   11.8111   -7.9293    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   10.3129   -8.4914    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   11.2258   -8.6401    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.3067   -7.4616    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    6.9098   -6.5065    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.0177   -5.3313    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.1244   -5.6022    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    9.3750   -4.0625    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
   10.1282   -3.5118    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   10.0280   -2.5842    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    9.1745   -2.2072    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.4212   -2.7579    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.5215   -3.6855    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
  6  7  1  0  0  0  0
  5  7  2  0  0  0  0
  1  2  1  0  0  0  0
  4  6  1  0  0  0  0
  2  3  2  0  0  0  0
  3  5  1  0  0  0  0
  1  4  1  0  0  0  0
 11  9  1  0  0  0  0
 10 11  2  0  0  0  0
  9  8  2  0  0  0  0
  7 10  1  0  0  0  0
  5  8  1  0  0  0  0
 15 13  2  0  0  0  0
 14 15  1  0  0  0  0
 13 12  1  0  0  0  0
  1 14  2  0  0  0  0
  4 12  2  0  0  0  0
  2 16  1  0  0  0  0
 20 21  1  0  0  0  0
 19 20  1  0  0  0  0
 18 19  1  0  0  0  0
 17 18  1  0  0  0  0
 21 16  1  0  0  0  0
 16 17  1  0  0  0  0
M  END
";
            Indigo i = new Indigo();
            IndigoInchi i_inchi = new IndigoInchi(i);
            i.setOption("inchi-options", "/SUU /SLUUD /FixedH /SUCF");

			string canonical_output_molfile1 = StandardizationModule.CanonicalizeTautomer(molfile1, false,
				Resources.Vendor.ChemAxon, out new_inchi, out new_inchi_key, InChIFlags.CRS);

            IndigoObject output_obj = i.loadMolecule(canonical_output_molfile1);
            string non_std_output_inchi = i_inchi.getInchi(output_obj);

            string molfile2 = @"
  -INDIGO-02151310232D

 21 24  0  0  0  0  0  0  0  0999 V2000
    8.6985   -5.9765    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    9.5396   -5.5852    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   10.3771   -5.9989    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    8.4808   -6.8849    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   10.5785   -6.9073    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    9.0556   -7.6205    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    9.9886   -7.6205    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   11.4881   -7.0617    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   11.8111   -7.9293    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   10.3129   -8.4914    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   11.2258   -8.6401    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.3067   -7.4616    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    6.9098   -6.5065    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.0177   -5.3313    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.1244   -5.6022    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    9.3750   -4.0625    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
   10.1282   -3.5118    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   10.0280   -2.5842    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    9.1745   -2.2072    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.4212   -2.7579    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.5215   -3.6855    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
  6  7  1  0  0  0  0
  5  7  2  0  0  0  0
  1  2  2  0  0  0  0
  4  6  2  0  0  0  0
  2  3  1  0  0  0  0
  3  5  1  0  0  0  0
  1  4  1  0  0  0  0
 11  9  1  0  0  0  0
 10 11  2  0  0  0  0
  9  8  2  0  0  0  0
  7 10  1  0  0  0  0
  5  8  1  0  0  0  0
 15 13  1  0  0  0  0
 14 15  2  0  0  0  0
 13 12  2  0  0  0  0
  1 14  1  0  0  0  0
  4 12  1  0  0  0  0
  2 16  1  0  0  0  0
 20 21  1  0  0  0  0
 19 20  1  0  0  0  0
 18 19  1  0  0  0  0
 17 18  1  0  0  0  0
 21 16  1  0  0  0  0
 16 17  1  0  0  0  0
M  END";
            //ChemSpider.CVSP.ObjectModel.Logger logger = ChemSpider.CVSP.ObjectModel.Logger.CreateTraceSources(null, null, null);
			string canonical_output_molfile2 = StandardizationModule.CanonicalizeTautomer(molfile2, false,
				Resources.Vendor.ChemAxon, out new_inchi, out new_inchi_key, InChIFlags.CRS);

            IndigoObject output_obj2 = i.loadMolecule(canonical_output_molfile2);
            string non_std_output_inchi2 = i_inchi.getInchi(output_obj2);

            Assert.AreEqual(non_std_output_inchi, non_std_output_inchi2);
        }

        [TestMethod]
        public void StandardizationTautomers_checkCanonicalTautomer_Indol1()
        {

            string molfile1 = @"
  -ISIS-  03141118062D

 13 14  0  0  0  0  0  0  0  0999 V2000
   -1.1598   -1.1875    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -1.1609   -2.0148    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -0.4461   -2.4277    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -0.4479   -0.7747    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    0.2675   -1.1838    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    0.2677   -2.0149    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    1.0582   -2.2715    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    1.5465   -1.5990    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    1.0578   -0.9268    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    1.3125   -0.1421    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    2.1194    0.0296    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    2.3741    0.8143    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    2.3715   -1.5987    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
  3  6  2  0  0  0  0
  1  2  2  0  0  0  0
  5  4  2  0  0  0  0
  6  7  1  0  0  0  0
  7  8  1  0  0  0  0
  8  9  1  0  0  0  0
  9  5  1  0  0  0  0
  4  1  1  0  0  0  0
  9 10  2  0  0  0  0
  5  6  1  0  0  0  0
 10 11  1  0  0  0  0
 11 12  1  0  0  0  0
  2  3  1  0  0  0  0
  8 13  2  0  0  0  0
M  END
";
            //ChemSpider.CVSP.ObjectModel.Logger logger = ChemSpider.CVSP.ObjectModel.Logger.CreateTraceSources(null, null, null);
			string non_std_output_inchi;
			string canonical_output_molfile1 = StandardizationModule.CanonicalizeTautomer(molfile1, false,
				Resources.Vendor.ChemAxon, out non_std_output_inchi, out new_inchi_key, InChIFlags.CRS);
            string molfile2 = @"
  -ISIS-  03141118062D

 13 14  0  0  0  0  0  0  0  0999 V2000
   -2.6306   -0.0375    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -2.6318   -0.8648    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -1.9169   -1.2777    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -1.9187    0.3753    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -1.2034   -0.0338    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -1.2031   -0.8649    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -0.4126   -1.1215    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    0.0757   -0.4490    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -0.4131    0.2232    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -0.1584    1.0079    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    0.6485    1.1796    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    0.9032    1.9643    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    0.9007   -0.4487    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
  3  6  2  0  0  0  0
  1  2  2  0  0  0  0
  5  4  2  0  0  0  0
  6  7  1  0  0  0  0
  7  8  1  0  0  0  0
  8  9  2  0  0  0  0
  9  5  1  0  0  0  0
  4  1  1  0  0  0  0
  9 10  1  0  0  0  0
  5  6  1  0  0  0  0
 10 11  2  0  0  0  0
 11 12  1  0  0  0  0
  2  3  1  0  0  0  0
  8 13  1  0  0  0  0
M  END";

			string canonical_output_molfile2 = StandardizationModule.CanonicalizeTautomer(molfile2, false,
				Resources.Vendor.ChemAxon, out new_inchi, out new_inchi_key, InChIFlags.CRS);
            string non_std_output_inchi2 = new_inchi;

            string molfile3 = @"
  -INDIGO-02181320042D

 13 14  0  0  0  0  0  0  0  0999 V2000
   -1.1014   -1.3791    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -1.1026   -2.2065    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -0.3878   -2.6194    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -0.3896   -0.9664    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    0.3258   -1.3755    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    0.3261   -2.2065    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    1.1165   -2.4631    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    1.6049   -1.7906    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    1.1161   -1.1185    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    1.3708   -0.3338    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    2.1777   -0.1620    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    2.4324    0.6227    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    2.4299   -1.7903    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
  3  6  2  0  0  0  0
  1  2  2  0  0  0  0
  5  4  2  0  0  0  0
  6  7  1  0  0  0  0
  7  8  2  0  0  0  0
  8  9  1  0  0  0  0
  9  5  1  0  0  0  0
  4  1  1  0  0  0  0
  9 10  2  0  0  0  0
  5  6  1  0  0  0  0
 10 11  1  0  0  0  0
 11 12  1  0  0  0  0
  2  3  1  0  0  0  0
  8 13  1  0  0  0  0
M  END
";

			string canonical_output_molfile3 = StandardizationModule.CanonicalizeTautomer(molfile3, false,
				Resources.Vendor.ChemAxon, out new_inchi, out new_inchi_key, InChIFlags.CRS);
            string non_std_output_inchi3 = new_inchi;

            Assert.AreEqual(non_std_output_inchi, non_std_output_inchi2);
            Assert.AreEqual(non_std_output_inchi, non_std_output_inchi3);
        }


        //check that either bonds are not converting to stereo double bonds
        [TestMethod]
        public void StandardizationTautomers_checkCanonicalTautomer_EitherBondConvertToStereo()
        {
            string molfile1 = @"132
  -INDIGO-02181319342D

 20 19  0  0  0  0  0  0  0  0999 V2000
    2.3645    3.2217    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    1.6500    1.9842    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    3.7935   -0.0784    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    4.5080   -0.4909    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    3.7935    0.7466    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    4.5080   -1.3159    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    3.0791    1.1592    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.2224   -1.7284    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    3.0791    1.9842    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.2224   -2.5534    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    2.3645    2.3967    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.9370   -2.9659    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    6.6515   -2.5534    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.3659   -2.9659    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.0804   -2.5534    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.0804   -1.7284    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.7949   -1.3159    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.7949   -0.4909    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.0804   -0.0784    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.0804    0.7466    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
  1 11  1  0  0  0  0
  2 11  2  3  0  0  0
  3  4  1  0  0  0  0
  3  5  1  0  0  0  0
  4  6  1  0  0  0  0
  5  7  1  0  0  0  0
  6  8  1  0  0  0  0
  7  9  1  0  0  0  0
  8 10  1  0  0  0  0
  9 11  1  0  0  0  0
 10 12  2  3  0  0  0
 12 13  1  0  0  0  0
 13 14  1  0  0  0  0
 14 15  2  3  0  0  0
 15 16  1  0  0  0  0
 16 17  1  0  0  0  0
 17 18  2  3  0  0  0
 18 19  1  0  0  0  0
 19 20  1  0  0  0  0
M  END";
			string canonical_output_molfile1 = StandardizationModule.CanonicalizeTautomer(molfile1, false,
				Resources.Vendor.ChemAxon, out new_inchi, out new_inchi_key, InChIFlags.CRS);
            Assert.AreEqual("InChI=1/C18H30O2/c1-2-3-4-5-6-7-8-9-10-11-12-13-14-15-16-17-18(19)20/h3-4,6-7,9-10H,2,5,8,11-17H2,1H3,(H,19,20)/b4-3u,7-6u,10-9u/f/h19H",
                new_inchi);

            molfile1 = @"
";
            // isn't CRS deposition
			StandardizationModule.CanonicalizeTautomer(molfile1, false,
				Resources.Vendor.ChemAxon, out new_inchi, out new_inchi_key, InChIFlags.CRS);
            Assert.AreEqual("InChI=1/C20H28O2/c1-15(8-6-9-16(2)14-19(21)22)11-12-18-17(3)10-7-13-20(18,4)5/h6,8-9,11-12,14H,7,10,13H2,1-5H3,(H,21,22)/b9-6u,12-11u,15-8u,16-14u/f/h21H",
                new_inchi);
            // is CRS deposition
			StandardizationModule.CanonicalizeTautomer(molfile1, true,
				Resources.Vendor.ChemAxon, out new_inchi, out new_inchi_key, InChIFlags.CRS);
            Assert.AreEqual("InChI=1/C20H28O2/c1-15(8-6-9-16(2)14-19(21)22)11-12-18-17(3)10-7-13-20(18,4)5/h6,8-9,11-12,14H,7,10,13H2,1-5H3,(H,21,22)/b9-6u,12-11u,15-8u,16-14u/f/h21H",
                new_inchi);
        

            molfile1 = @"
  -INDIGO-03241315252D

 16 17  0  0  0  0  0  0  0  0999 V2000
    9.2278   -5.6901    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   11.2701   -5.6896    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   10.2509   -5.0998    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
   11.2701   -6.8710    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    9.2278   -6.8763    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   10.2534   -7.4606    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.2033   -7.4669    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.1756   -6.8740    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.1960   -5.0948    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.1730   -5.6938    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   10.1563   -9.0000    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
   11.4063   -9.7500    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
   11.4063  -11.5938    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    9.7500  -12.3125    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    9.8125  -13.6250    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    8.7271  -11.7219    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
  6  4  1  0  0  0  0
  5  6  2  0  0  0  0
  2  3  1  0  0  0  0
  1  5  1  0  0  0  0
  4  2  2  0  0  0  0
  3  1  2  0  0  0  0
 10  8  1  0  0  0  0
  9 10  2  0  0  0  0
  8  7  2  0  0  0  0
  1  9  1  0  0  0  0
  5  7  1  0  0  0  0
  6 11  1  0  0  0  0
 11 12  2  3  0  0  0
 12 13  1  0  0  0  0
 13 14  1  0  0  0  0
 14 15  2  0  0  0  0
 14 16  1  0  0  0  0
M  END";
			StandardizationModule.CanonicalizeTautomer(molfile1, false,
				Resources.Vendor.ChemAxon, out new_inchi, out new_inchi_key, InChIFlags.CRS);
            Assert.AreEqual("InChI=1/C10H10N6/c11-10(12)15-16-14-9-5-6-13-8-4-2-1-3-7(8)9/h1-6H,(H4,11,12,13,14,15)/f/h11-12H2/b16-14u",
                new_inchi);
        }

        [TestMethod]
        public void StandardizationTautomers_CHEMBL50821_dottedBondAtDoubleBond()
        {
			StandardizationModule.CanonicalizeTautomer(Resource1.CHEMBL50821, true,
				Resources.Vendor.ChemAxon, out new_inchi, out new_inchi_key, InChIFlags.CRS);
            Assert.AreEqual("InChI=1/C17H24N2O7S/c1-6-10-9(7(2)21)15(22)19(10)11(17(24)25)13(6)27-14-8(5-20)26-12(14)16(23)18(3)4/h6-10,12,14,20-21H,5H2,1-4H3,(H,24,25)/p-1/t6-,7-,8-,9-,10?,12+,14-/m1/s1/fC17H23N2O7S/q-1",
                new_inchi);
        }

        [TestMethod]
        public void StandardizationTautomers_checkCanonicalTautomer_Porphyrine_1()
        {
            string molfile1 = @"
  -INDIGO-03051315002D

 43 46  0  0  1  0  0  0  0  0999 V2000
   14.6494    2.6404    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   13.1405    3.1726    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   12.8289    4.7420    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   13.2580    6.2834    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   13.0502    7.8698    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   12.2384    9.2486    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
   10.9520   10.2001    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    9.3960   10.5726    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.8182   10.3070    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    6.4700    9.4454    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    5.5661    8.1251    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.2506    6.5566    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.5737    4.9895    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    4.7737    3.6039    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    3.1825    3.4366    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    2.5317    1.9750    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    0.9405    1.8077    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    0.0000    3.1021    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    0.2897    0.3460    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    5.8443    2.4148    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.5117    0.8498    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    7.3060    3.0656    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    8.8307    2.5805    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   10.4236    2.7303    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   11.9132    2.1462    0.0000 C   0  0  2  0  0  0  0  0  0  0  0  0
   12.1703    0.5670    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   13.6665    0.0000    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   11.8311    3.4912    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
   18.8603    0.0000    0.0000 Fe  0  0  0  0  0  0  0  0  0  0  0  0
    7.1387    4.6569    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    5.1409    9.6676    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    3.5972   10.0883    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    3.1897   11.6356    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    1.6460   12.0564    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    0.5098   10.9299    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    1.2386   13.6036    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    6.2900   10.7809    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    5.9184   12.3371    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   12.5090   10.5687    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   12.9858   12.0960    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   13.5796    9.3797    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   15.1483    9.6943    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   15.6603   11.2102    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
  1  2  1  0  0  0  0
  2  3  2  0  0  0  0
  3  4  1  0  0  0  0
  4  5  2  0  0  0  0
  5  6  1  0  0  0  0
  6  7  1  0  0  0  0
  7  8  1  0  0  0  0
  8  9  2  0  0  0  0
  9 10  1  0  0  0  0
 10 11  1  0  0  0  0
 11 12  2  0  0  0  0
 12 13  1  0  0  0  0
 13 14  2  0  0  0  0
 14 15  1  0  0  0  0
 15 16  1  0  0  0  0
 16 17  1  0  0  0  0
 17 18  1  0  0  0  0
 17 19  2  0  0  0  0
 14 20  1  0  0  0  0
 20 21  1  0  0  0  0
 20 22  2  0  0  0  0
 22 23  1  0  0  0  0
 23 24  2  0  0  0  0
 24 25  1  0  0  0  0
  2 25  1  0  0  0  0
 25 26  1  1  0  0  0
 26 27  2  0  0  0  0
 28 24  1  0  0  0  0
  3 28  1  0  0  0  0
 13 30  1  0  0  0  0
 22 30  1  0  0  0  0
 11 31  1  0  0  0  0
 31 32  1  0  0  0  0
 32 33  1  0  0  0  0
 33 34  1  0  0  0  0
 34 35  1  0  0  0  0
 34 36  2  0  0  0  0
 31 37  2  0  0  0  0
  9 37  1  0  0  0  0
 37 38  1  0  0  0  0
  7 39  2  0  0  0  0
 39 40  1  0  0  0  0
 39 41  1  0  0  0  0
  5 41  1  0  0  0  0
 41 42  2  0  0  0  0
 42 43  1  0  0  0  0
M  CHG  5   6  -1  10  -1  28  -1  29   4  30  -1
M  END";
			string canonical_output_molfile1 = StandardizationModule.CanonicalizeTautomer(molfile1, true,
				Resources.Vendor.ChemAxon, out new_inchi, out new_inchi_key, InChIFlags.CRS);
            Assert.AreEqual("InChI=1/C33H34N4O5.Fe/c1-6-20-16(2)26-13-31-23(15-38)19(5)25(37-31)11-24-17(3)21(7-9-32(39)40)29(35-24)14-30-22(8-10-33(41)42)18(4)27(36-30)12-28(20)34-26;/h6,11-15,20,38H,1,7-10H2,2-5H3,(H4,34,35,36,37,39,40,41,42);/q-2;+4/p-2/t20?;/fC33H32N4O5.Fe/h39,41H;/q-4;m/b24-11-,25-11-,26-13-,27-12-,28-12-,29-14-,30-14-,31-13-;",
                new_inchi);
        }


        //check that double bonds are still converted crossed on R-N=N-X (bug of chemaxon)
        [TestMethod]
        public void StandardizationTautomers_checkCanonicalTautomer_Double2Either_2()
        {
            string molfile = @"400007
  -INDIGO-03211322272D

 16 16  0  0  0  0  0  0  0  0999 V2000
    5.4707    8.5245    0.0000 S   0  0  0  0  0  0  0  0  0  0  0  0
    1.5736    7.7745    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
   -1.0245    3.2745    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
    0.2745    5.5245    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    1.5736    3.2745    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    5.4707    5.5245    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    6.7697    6.2745    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    8.0688    8.5245    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    1.5736    6.2745    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    2.8726    5.5245    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    0.2745    4.0245    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    2.8726    4.0245    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    4.1716    6.2745    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    6.7697    7.7745    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
   -1.0245    6.2745    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0
    1.5736    1.7745    0.0000 H   0  0  0  0  0  0  0  0  0  0  0  0
  1 14  2  0  0  0  0
  2  9  2  0  0  0  0
  3 11  2  0  0  0  0
  4  9  1  0  0  0  0
  4 11  1  0  0  0  0
  4 15  1  0  0  0  0
  5 11  1  0  0  0  0
  5 12  1  0  0  0  0
  5 16  1  0  0  0  0
  6  7  1  0  0  0  0
  6 13  2  0  0  0  0
  7 14  1  0  0  0  0
  8 14  1  0  0  0  0
  9 10  1  0  0  0  0
 10 12  2  0  0  0  0
 10 13  1  0  0  0  0
M  END";
            Indigo i = new Indigo();
			string output_molfile = StandardizationModule.CanonicalizeTautomer(molfile, false,
				Resources.Vendor.ChemAxon, out new_inchi, out new_inchi_key, InChIFlags.CRS);
            IndigoObject output_obj = i.loadMolecule(output_molfile);
            string smiles_out = output_obj.canonicalSmiles();
            Assert.AreEqual("NC(=S)NN=CC1=CNC(=O)NC1=O", smiles_out);
        }


        //check that either bonds are not converting to stereo double bonds
        // TODO: commented as takes too long now
		// [TestMethod]
        public void StandardizationTautomers_checkCanonicalTautomer_falseTautomer_1()
        {

            string molfile = @"
  -INDIGO-03271307422D

 12 12  0  0  0  0  0  0  0  0999 V2000
    4.2868    0.0000    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    3.5724   -0.4125    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    3.5724   -1.2375    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    2.8579    0.0000    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
    2.1434   -0.4125    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    1.4289    0.0000    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    0.7145   -0.4125    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    0.0000    0.0000    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    0.0000    0.8250    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    0.7145    1.2375    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
    0.7145    2.0625    0.0000 I   0  0  0  0  0  0  0  0  0  0  0  0
    1.4289    0.8250    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
  1  2  1  0  0  0  0
  2  3  2  0  0  0  0
  2  4  1  0  0  0  0
  4  5  1  0  0  0  0
  5  6  1  0  0  0  0
  6  7  2  0  0  0  0
  7  8  1  0  0  0  0
  8  9  2  0  0  0  0
  9 10  1  0  0  0  0
 10 11  1  0  0  0  0
 10 12  2  0  0  0  0
  6 12  1  0  0  0  0
M  ISO  1  11 123
M  END";
            Indigo i = new Indigo();
			string output_molfile = StandardizationModule.CanonicalizeTautomer(molfile, false,
				Resources.Vendor.ChemAxon, out new_inchi, out new_inchi_key, InChIFlags.CRS);
            IndigoObject output_obj = i.loadMolecule(output_molfile);
            string smiles_out = output_obj.canonicalSmiles();
            Assert.AreEqual(smiles_out, "NC(=N)NCC1C=C([123I])C=CC=1");
        }
    }
}
