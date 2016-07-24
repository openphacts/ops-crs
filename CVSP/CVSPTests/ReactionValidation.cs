using System;
using System.Data;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using RSC.CVSP.Compounds;
using com.ggasoftware.indigo;

namespace CVSPTests
{
    [TestClass]
    public class ReactionValidation
    {
		//temprary comment


//		[TestMethod]
//		public void TestReaction1()
//		{
//			string rxn = @"O=C1C2Cc3ccccc3N2CCCN1CCc1ccccc1>>O=C1C2Cc3c(OC)cccc3N2CCCN1";
//			Record r = new Record(DataDomain.Reactions, rxn);
//			r.Process(true, false,Resources.Vendor.Indigo,true,null,true,true,true);
//			Assert.IsTrue(r.Reaction.OriginalValidityScore  == 75);
			
//		}

//		[TestMethod]
//		public void TestReaction2()
//		{
//			string rxn = @"[CH3:1][c:2]1[nH:6][cH:5][n:4][c:3]1[C:7](O)=[O:8].S(=O)(=O)(O)O.C(=O)([O-])[O-].[K+].[K+].Cl>>Cl.[OH:8][CH2:7][c:3]1[n:4][cH:5][nH:6][c:2]1[CH3:1]";
//			Record r = new Record(DataDomain.Reactions, rxn);
//			r.Process(true, false, Resources.Vendor.Indigo, true, null, true, true, true);
//			Assert.IsTrue(r.Reaction.OriginalValidityScore  == 57);
			
//		}

//		[TestMethod]
//		public void TestReaction3()
//		{
//			string rxn = @"[CH3:1][c:2]1[n:3][cH:4][s:5][cH:6]1.[H][H].[Li].C([Li])CCC.[CH:15](=[O:20])[CH2:16][CH:17]([CH3:19])[CH3:18]>>[CH2:16]([CH:15]([c:4]1[s:5][cH:6][c:2]([CH3:1])[n:3]1)[OH:20])[CH:17]([CH3:19])[CH3:18]";
//			Record r = new Record(DataDomain.Reactions, rxn);
//			r.Process(true, false, Resources.Vendor.Indigo, true, null, true, true, true);
//			Assert.IsTrue(r.Reaction.OriginalValidityScore == 69);
			
//		}

//		[TestMethod]
//		public void TestReaction4()
//		{
//			string rxn = @"[C:1]([c:5]1[cH:6][c:7]([cH:17][c:18]([C:21]([CH3:24])([CH3:23])[CH3:22])[c:19]1[OH:20])[NH:8][c:9]1[cH:10][c:11]([cH:14][cH:15][cH:16]1)[C:12]#[N:13])([CH3:4])([CH3:3])[CH3:2].[Na+].[N-:26]=[N+:27]=[N-:28].[Cl-].[NH4+].[Li+].[Cl-].Cl>CN(C)C=O>[C:1]([c:5]1[cH:6][c:7]([cH:17][c:18]([C:21]([CH3:24])([CH3:23])[CH3:22])[c:19]1[OH:20])[NH:8][c:9]1[cH:10][c:11](-[c:12]2[nH:28][n:27][n:26][n:13]2)[cH:14][cH:15][cH:16]1)([CH3:4])([CH3:3])[CH3:2]";
//			Record r = new Record(DataDomain.Reactions, rxn);
//			r.Process(true, false, Resources.Vendor.Indigo, true, null, true, true, true);
//			Assert.IsTrue(r.Reaction.OriginalValidityScore == 80);
			
//		}

//		[TestMethod]
//		public void TestReaction5()
//		{
//			string rxn = @"[OH:1][C:2]([C:4](=[O:6])[OH:5])=[O:3]>CCO>[C:4]([O-:6])(=[O:5])[C:2]([O-:3])=[O:1]";
//			Record r = new Record(DataDomain.Reactions, rxn);
//			r.Process(true, false, Resources.Vendor.Indigo, true, null, true, true, true);
//			Assert.IsTrue(r.Reaction.OriginalValidityScore == 33);
			
//		}

//		[TestMethod]
//		public void TestReaction6_veryBadIndigoMapping()
//		{
//			//indigo assigns same mapping number to multiple atoms in the reactant
//			string rxn = @"[c:14]1(P([c:14]2[cH:19][cH:18][cH:17][cH:16][cH:15]2)[c:14]2[cH:19][cH:18][cH:17][cH:16][cH:15]2)[cH:19][cH:18][cH:17][cH:16][cH:15]1.[Br]>CC#N>CC1(C)[C@H:16]2[CH2:15][C@@H:14]1[CH2:19][CH2:18][C@H:17]2CCBr";
//			Record r = new Record(DataDomain.Reactions, rxn);
//			r.Process(true, false, Resources.Vendor.Indigo, true, null, true, true, true);
//			Assert.IsTrue(r.Reaction.OriginalValidityScore == 22);

//		}

//		[TestMethod]
//		public void TestMethod_Bad1()
//		{
//			Record r = new Record(DataDomain.Reactions, "Cl>c1ccccc1>Cl.CN");
//			r.Process(true, false, Resources.Vendor.Indigo, true, null, true, true, true);
//			Assert.IsTrue(r.Reaction.OriginalValidityScore == 0);
//			Assert.IsTrue(r.Reaction.StandardizedReactionScore == 0);
//			int ni = 0;
//			foreach (Issue dr in r.Reaction.IssueCollection)
//			{
				
//				if (ni == 0)
//				{
//					Assert.IsTrue(dr.IssueType == IssueType.validation);
//					Assert.IsTrue(dr.Code == (int)Resources.IssueCodes.validationReaction_DuplicateMoleculeInReactantsAndProducts);
//					Assert.IsTrue(dr.Severity == Severity.Warning);
//				}
//				else if (ni == 1)
//				{
//					Assert.IsTrue(dr.IssueType == IssueType.validation);
//					Assert.IsTrue(dr.Code == (int)Resources.IssueCodes.validationReaction_mapped_to_all_ratioLessThan20);
//					Assert.IsTrue(dr.Severity == Severity.Warning);
//				}
//				else if (ni == 2)
//				{
//					Assert.IsTrue(dr.IssueType == IssueType.validation);
//					Assert.IsTrue(dr.Code == (int)Resources.IssueCodes.validationReaction_0_BondMadeOrBroken_0_BondOrderChanged);
//					Assert.IsTrue(dr.Severity == Resources.IssueMessagesDictionary[Resources.IssueCodes.validationReaction_0_BondMadeOrBroken_0_BondOrderChanged].Item1);
//				}
//				ni++;
//			}
//		}

//		[TestMethod]
//		public void TestMethod_Bad2()
//		{
//			Record r = new Record(DataDomain.Reactions, "[CH3:1][c:2]1[nH:6][cH:5][n:4][c:3]1[C:7](O)=[O:8].S(=O)(=O)(O)O.C(=O)([O-])[O-].[K+].[K+]>>Cl.[OH:8][CH2:7][c:3]1[n:4][cH:5][nH:6][c:2]1[CH3:1]");
//			r.Process(true, true, Resources.Vendor.Indigo, true, null, true, true, true);
//			Assert.IsTrue(r.Reaction.OriginalValidityScore == 55);
//			Assert.IsTrue(r.Reaction.StandardizedReactionScore == 57);
//			int i= 0;
//			foreach (Issue dr in r.Reaction.IssueCollection)
//			{
//				i++;
//				if (i == 1)
//				{
//					Assert.IsTrue(dr.IssueType == IssueType.validation);
//					Assert.IsTrue(dr.Message.Equals(Resources.IssueMessagesDictionary[Resources.IssueCodes.validationReaction_DuplicateMoleculeInReactants].Item2));
//					Assert.IsTrue(dr.Severity == Resources.IssueMessagesDictionary[Resources.IssueCodes.validationReaction_DuplicateMoleculeInReactants].Item1);
                    
//				} 
//			}
//		}

//		[TestMethod]
//		public void TestMethod_Bad3()
//		{
//			Record r = new Record(DataDomain.Reactions, "C1NCCCN=1.Cl>>Cl.N(C1NCCCN=1)N");
//			r.Process(true, true, Resources.Vendor.Indigo, true, null, true, true, true);
//			Assert.IsTrue(r.Reaction.OriginalValidityScore == 86);
			
//			Assert.IsTrue(r.Reaction.StandardizedReactionScore == 86);
			

//			//IList<Issue> issues = Reaction.Validate("C1NCCCN=1.Cl>>Cl.N(C1NCCCN=1)N");
//			int i = 0;
//			foreach (Issue dr in r.Reaction.IssueCollection)
//			{
//				i++;
//				if (i == 1)
//				{
//					Assert.IsTrue(dr.IssueType == IssueType.validation);
//					Assert.IsTrue(dr.Message.Equals("same molecule found in reactants and products"));
//					Assert.IsTrue(dr.Severity == Severity.Warning);
//				}
               
//			}
//		}

//		//[TestMethod]
//		public void TestMethod_Bad3_automap_indigo_exception()
//		{
//			Indigo k = new Indigo();
//			IndigoObject reax = k.loadReactionSmarts("C1NCCCN=1.Cl>>Cl.N(C1NCCCN=1)N");
            
//			int i = 0;
//			foreach (IndigoObject reac in reax.iterateReactants())
//			{
//				if (i == 1) reac.remove();
//				i++;
//			}
//			try
//			{
//				reax.automap("discard");
//				Assert.IsTrue(!reax.rxnfile().Equals (""));
//			}
//			catch (IndigoException ex)
//			{
//				Assert.IsFalse(ex.Message.Equals("array: invalid index 3 (size=3)"));
//			}
            
//		}

//		[TestMethod]
//		public void TestMethod_Bad4()
//		{
//			Record r = new Record(DataDomain.Reactions, "CN1C=C(c2ccccc2)C(=O)C(c2cc(C(F)(F)F)ccc2)=N1.CN1C=C(c2ccccc2)C(=O)C(c2ccccc2)=N1.[c:45]1([C:51]2[C:56](=O)[C:55]([c:58]3[cH:63][cH:62][cH:61][cH:60][cH:59]3)=[CH:54][N:53]([CH2:64][CH2:65]C)[N:52]=2)[cH:50][cH:49][cH:48][cH:47][cH:46]1.C(N1C=C(c2ccccc2)C(=O)C(c2ccccc2)=N1)C.Brc1cc(C2C(=O)C(c3ccccc3)=CN(C)N=2)ccc1>>Cl[c:48]1[cH:49][cH:50][c:45]([C:51]2[C:56](=S)[C:55]([c:58]3[cH:59][c:60](C)[cH:61][cH:62][cH:63]3)=[CH:54][N:53]([CH2:64][CH3:65])[N:52]=2)[cH:46][cH:47]1");
			
//			r.Process(true, true, Resources.Vendor.Indigo, true, null, true, true, true);
//			Assert.IsTrue(r.Reaction.OriginalValidityScore == 31);
			
//			Assert.IsTrue(r.Reaction.StandardizedReactionScore == 31);
			
			
//			int i = 0;
//			foreach (Issue dr in r.Reaction.IssueCollection)
//			{
//				i++;
//				if (i == 1)
//				{
//					Assert.IsTrue(dr.IssueType == IssueType.validation);
//					Assert.IsTrue(dr.Code == (int)Resources.IssueCodes.validationReaction_mapped_to_all_ratioLessThan40);
//					Assert.IsTrue(dr.Severity == Severity.Warning);
//				}
//				else if (i == 2)
//				{
//					Assert.IsTrue(dr.IssueType == IssueType.validation);
//					Assert.IsTrue(dr.Code == (int)Resources.IssueCodes.validationReaction_0_BondMadeOrBroken_0_BondOrderChanged);
//					Assert.IsTrue(dr.Severity == Severity.Warning);
//				}
                
//			}
//		}

//		[TestMethod]
//		public void TestMethod_Good1()
//		{
//			string rdf = @"$RXN
//
//      Accord   0929061723
//
//  1  1
//$MOL
//
//  Accord  09290617232D
//
// 23 24  0  0  0  0  0  0  0  0999 V2000
//    8.5727    7.4768    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    8.5767    8.2957    0.0000 S   0  0  0  0  0  0  0  0  0  0  0  0
//    7.8566    7.0613    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    9.2888    7.0573    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    8.5767    9.1187    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
//    7.7497    8.2997    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
//    9.3996    8.2997    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
//    7.8566    6.2384    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    9.2888    6.2344    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    7.8605    9.5341    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
//    8.5727    5.8229    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    7.8605   10.3572    0.0000 C   0  0  0  0  0  0  0  0  0  1  0  0
//    8.5687    5.0000    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    7.1444   10.7686    0.0000 C   0  0  0  0  0  0  0  0  0  2  0  0
//    7.1444   11.5955    0.0000 C   0  0  0  0  0  0  0  0  0  3  0  0
//    6.4323   10.3572    0.0000 C   0  0  0  0  0  0  0  0  0  4  0  0
//    7.8605   12.0070    0.0000 C   0  0  0  0  0  0  0  0  0  5  0  0
//    6.4323   12.0070    0.0000 C   0  0  0  0  0  0  0  0  0  6  0  0
//    5.7161   10.7686    0.0000 C   0  0  0  0  0  0  0  0  0  7  0  0
//    7.8605   12.8300    0.0000 C   0  0  0  0  0  0  0  0  0  8  0  0
//    6.4323   12.8300    0.0000 C   0  0  0  0  0  0  0  0  0  9  0  0
//    5.0000   10.3572    0.0000 C   0  0  0  0  0  0  0  0  0 10  0  0
//    7.1444   13.2454    0.0000 C   0  0  0  0  0  0  0  0  0 11  0  0
//  1  2  1  0  0  0  0
//  1  3  1  0  0  0  0
//  1  4  2  0  0  0  0
//  2  5  1  0  0  0  0
//  2  6  2  0  0  0  0
//  2  7  2  0  0  0  0
//  3  8  2  0  0  0  0
//  4  9  1  0  0  0  0
//  5 10  1  0  0  0  0
//  8 11  1  0  0  0  0
// 10 12  1  0  0  0  4
// 11 13  1  0  0  0  0
// 12 14  1  0  0  0  0
// 14 15  1  0  0  0  0
// 14 16  1  0  0  0  0
// 15 17  1  0  0  0  0
// 15 18  2  0  0  0  0
// 16 19  1  0  0  0  0
// 17 20  2  0  0  0  0
// 18 21  1  0  0  0  0
// 19 22  2  0  0  0  0
// 20 23  1  0  0  0  0
//  9 11  2  0  0  0  0
// 21 23  2  0  0  0  0
//M  END
//$MOL
//
//  Accord  09290617232D
//
// 13 13  0  0  0  0  0  0  0  0999 V2000
//   14.6824    9.4108    0.0000 C   0  0  0  0  0  0  0  0  0  3  0  0
//   14.6824    8.5870    0.0000 C   0  0  0  0  0  0  0  0  0  2  0  0
//   15.3973    9.8247    0.0000 C   0  0  0  0  0  0  0  0  0  5  0  0
//   13.9676    9.8247    0.0000 C   0  0  0  0  0  0  0  0  0  6  0  0
//   13.9676    8.1731    0.0000 C   0  0  0  0  0  0  0  0  0  4  0  0
//   15.3973    8.1731    0.0000 C   0  0  0  0  0  0  0  0  0  1  0  0
//   15.3973   10.6486    0.0000 C   0  0  0  0  0  0  0  0  0  8  0  0
//   13.9676   10.6486    0.0000 C   0  0  0  0  0  0  0  0  0  9  0  0
//   13.2527    8.5870    0.0000 C   0  0  0  0  0  0  0  0  0  7  0  0
//   15.3973    7.3493    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
//   14.6824   11.0625    0.0000 C   0  0  0  0  0  0  0  0  0 11  0  0
//   12.5396    8.1731    0.0000 C   0  0  0  0  0  0  0  0  0 10  0  0
//   16.1122    6.9354    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
//  1  2  1  0  0  0  0
//  1  3  1  0  0  0  0
//  1  4  2  0  0  0  0
//  2  5  1  0  0  0  0
//  2  6  1  0  0  0  0
//  3  7  2  0  0  0  0
//  4  8  1  0  0  0  0
//  5  9  1  0  0  0  0
//  6 10  1  0  0  0  4
//  7 11  1  0  0  0  0
//  9 12  2  0  0  0  0
// 10 13  1  0  0  0  4
//  8 11  2  0  0  0  0
//M  END";
//			Record r = new Record(DataDomain.Reactions, rdf);
//			r.Process(true, true, Resources.Vendor.Indigo, true, null, true, true, true);
//			Assert.IsTrue(r.Reaction.OriginalValidityScore == 61);
			
//			Assert.IsTrue(r.Reaction.StandardizedReactionScore == 61);
			
//			int i = 0;
//			foreach (Issue dr in r.Reaction.IssueCollection)
//			{
//				i++;
//				if (i == 1)
//				{
//					Assert.IsTrue(dr.IssueType == IssueType.validation);
//					Assert.IsTrue(dr.Code == (int)Resources.IssueCodes.validationReaction_mapped_to_all_ratioLessThan80);
//					Assert.IsTrue(dr.Severity == Severity.Warning);
//				}
//				else if (i == 2)
//				{
//					Assert.IsTrue(dr.IssueType == IssueType.validation);
//					Assert.IsTrue(dr.Code == (int)Resources.IssueCodes.validationReaction_1_BondMadeOrBroken);
//					Assert.IsTrue(dr.Severity == Severity.Information);
//				}
                
//			}
//		}

//		[TestMethod]
//		public void TestRDF()
//		{
//			string rdf = @"$RDFILE
//$DATM
//$RFMT
//$RXN
//
//      Accord   0929061723
//
//  1  1
//$MOL
//
//  Accord  09290617232D
//
// 23 24  0  0  0  0  0  0  0  0999 V2000
//    8.5727    7.4768    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    8.5767    8.2957    0.0000 S   0  0  0  0  0  0  0  0  0  0  0  0
//    7.8566    7.0613    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    9.2888    7.0573    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    8.5767    9.1187    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
//    7.7497    8.2997    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
//    9.3996    8.2997    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
//    7.8566    6.2384    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    9.2888    6.2344    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    7.8605    9.5341    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
//    8.5727    5.8229    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    7.8605   10.3572    0.0000 C   0  0  0  0  0  0  0  0  0  1  0  0
//    8.5687    5.0000    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    7.1444   10.7686    0.0000 C   0  0  0  0  0  0  0  0  0  2  0  0
//    7.1444   11.5955    0.0000 C   0  0  0  0  0  0  0  0  0  3  0  0
//    6.4323   10.3572    0.0000 C   0  0  0  0  0  0  0  0  0  4  0  0
//    7.8605   12.0070    0.0000 C   0  0  0  0  0  0  0  0  0  5  0  0
//    6.4323   12.0070    0.0000 C   0  0  0  0  0  0  0  0  0  6  0  0
//    5.7161   10.7686    0.0000 C   0  0  0  0  0  0  0  0  0  7  0  0
//    7.8605   12.8300    0.0000 C   0  0  0  0  0  0  0  0  0  8  0  0
//    6.4323   12.8300    0.0000 C   0  0  0  0  0  0  0  0  0  9  0  0
//    5.0000   10.3572    0.0000 C   0  0  0  0  0  0  0  0  0 10  0  0
//    7.1444   13.2454    0.0000 C   0  0  0  0  0  0  0  0  0 11  0  0
//  1  2  1  0  0  0  0
//  1  3  1  0  0  0  0
//  1  4  2  0  0  0  0
//  2  5  1  0  0  0  0
//  2  6  2  0  0  0  0
//  2  7  2  0  0  0  0
//  3  8  2  0  0  0  0
//  4  9  1  0  0  0  0
//  5 10  1  0  0  0  0
//  8 11  1  0  0  0  0
// 10 12  1  0  0  0  4
// 11 13  1  0  0  0  0
// 12 14  1  0  0  0  0
// 14 15  1  0  0  0  0
// 14 16  1  0  0  0  0
// 15 17  1  0  0  0  0
// 15 18  2  0  0  0  0
// 16 19  1  0  0  0  0
// 17 20  2  0  0  0  0
// 18 21  1  0  0  0  0
// 19 22  2  0  0  0  0
// 20 23  1  0  0  0  0
//  9 11  2  0  0  0  0
// 21 23  2  0  0  0  0
//M  END
//$MOL
//
//  Accord  09290617232D
//
// 13 13  0  0  0  0  0  0  0  0999 V2000
//   14.6824    9.4108    0.0000 C   0  0  0  0  0  0  0  0  0  3  0  0
//   14.6824    8.5870    0.0000 C   0  0  0  0  0  0  0  0  0  2  0  0
//   15.3973    9.8247    0.0000 C   0  0  0  0  0  0  0  0  0  5  0  0
//   13.9676    9.8247    0.0000 C   0  0  0  0  0  0  0  0  0  6  0  0
//   13.9676    8.1731    0.0000 C   0  0  0  0  0  0  0  0  0  4  0  0
//   15.3973    8.1731    0.0000 C   0  0  0  0  0  0  0  0  0  1  0  0
//   15.3973   10.6486    0.0000 C   0  0  0  0  0  0  0  0  0  8  0  0
//   13.9676   10.6486    0.0000 C   0  0  0  0  0  0  0  0  0  9  0  0
//   13.2527    8.5870    0.0000 C   0  0  0  0  0  0  0  0  0  7  0  0
//   15.3973    7.3493    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
//   14.6824   11.0625    0.0000 C   0  0  0  0  0  0  0  0  0 11  0  0
//   12.5396    8.1731    0.0000 C   0  0  0  0  0  0  0  0  0 10  0  0
//   16.1122    6.9354    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
//  1  2  1  0  0  0  0
//  1  3  1  0  0  0  0
//  1  4  2  0  0  0  0
//  2  5  1  0  0  0  0
//  2  6  1  0  0  0  0
//  3  7  2  0  0  0  0
//  4  8  1  0  0  0  0
//  5  9  1  0  0  0  0
//  6 10  1  0  0  0  4
//  7 11  1  0  0  0  0
//  9 12  2  0  0  0  0
// 10 13  1  0  0  0  4
//  8 11  2  0  0  0  0
//M  END
//$DTYPE RXN:RXNREGNO
//$DATUM Baba1
//$DTYPE RXN:VARIATION(1):LITTEXT(1):LITTEXT
//$DATUM Bloodworth A J, Courtneidge J L, Curtis R J, Spencer M D, J Chem Soc Perk+
//in Trans 1, (11), p. 2951-2955, 1990
//$DTYPE RXN:VARIATION(1):LITREF(1):AUTHOR
//$DATUM Bloodworth A J; Courtneidge J L; Curtis R J; Spencer M D
//$DTYPE RXN:VARIATION(1):LITREF(1):JOURNAL_NO.
//$DATUM 11
//$DTYPE RXN:VARIATION(1):LITREF(1):JOURNAL_PG.
//$DATUM 2951-2955
//$DTYPE RXN:VARIATION(1):LITREF(1):JOURNAL_YEAR
//$DATUM 1990
//$DTYPE RXN:VARIATION(1):LITREF(1):JOURNAL_JRNL
//$DATUM J Chem Soc Perkin Trans 1
//$DTYPE RXN:VARIATION(1):LITREF(1):JOURNAL_CODEN
//$DATUM JCPRB4
//$DTYPE RXN:VARIATION(1):LITREF(1):TITLE
//$DATUM Preparation of unsaturated hydroperoxides from N-alkenyl-N'-p-tosylhydraz+
//ines. Second step of a two-step sequence.
//$DTYPE RXN:VARIATION(1):LITREF(1):FULL_CITATION
//$DATUM Bloodworth A J, Courtneidge J L, Curtis R J, Spencer M D, J Chem Soc Perk+
//in Trans 1, (11), p. 2951-2955, 1990
//$DTYPE RXN:VARIATION(1):COMMENTS
//$DATUM Five examples given. The starting material was prepared via reduction of +
//the corresponding tosylhydrazone.
//$DTYPE RXN:VARIATION(1):KEYPHRASES
//$DATUM Chemoselective. Oxidative cleavage. Peroxidation
//$DTYPE RXN:VARIATION(1):MDLNUMBER
//$DATUM RMOS00018016
//$DTYPE RXN:VARIATION(1):RXNINDEX
//$DATUM Oxidation.
//$DTYPE RXN:VARIATION(1):RCTINDEX
//$DATUM Alkenyl hydrazines.
//$DTYPE RXN:VARIATION(1):PROINDEX
//$DATUM Alkenyl hydroperoxides.
//$DTYPE RXN:VARIATION(1):RGTINDEX
//$DATUM Sodium peroxide.
//$DTYPE RXN:VARIATION(1):STEPNO(1):RXNTEXT(1):RXNTEXT
//$DATUM Na2O2
//$DTYPE RXN:VARIATION(1):STEPNO(1):RXNTEXT(2):RXNTEXT
//$DATUM H2O2
//$DTYPE RXN:VARIATION(1):STEPNO(1):RXNTEXT(3):RXNTEXT
//$DATUM THF
//$DTYPE RXN:VARIATION(1):STEPNO(1):RXNTEXT(4):RXNTEXT
//$DATUM 20 C/24 h
//$DTYPE RXN:VARIATION(1):STEPNO(1):CONDITIONS(1):TEMPERATURE
//$DATUM 20.0 - 20.0
//$DTYPE RXN:VARIATION(1):PRODUCT(1):PRODUCT_NO
//$DATUM 1
//$DTYPE RXN:VARIATION(1):PRODUCT(1):YIELD
//$DATUM 39.0 - 39.0
//$DTYPE RXN:VARIATION(1):RXNREF(1):EXTREG
//$DATUM 18016
//$DTYPE RXN:VARIATION(1):RXNREF(1):PATH
//$DATUM A
//$DTYPE RXN:VARIATION(1):RXNREF(1):STEP
//$DATUM 2 of 2
//$DTYPE RXN:VARIATION(1):STEPNO(1):SOLVENT(1):MOL(1):MOLSTRUCTURE
//$DATUM $MFMT
//
//  Accord  09290617232D
//
//  5  5  0  0  0  0  0  0  0  0999 V2000
//    5.2631    6.2700    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    6.0795    6.2700    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    6.3426    5.4899    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    5.6713    5.0000    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
//    5.0000    5.4899    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//  1  5  1  0  0  0  0
//  1  2  1  0  0  0  0
//  2  3  1  0  0  0  0
//  3  4  1  0  0  0  0
//  4  5  1  0  0  0  0
//M  END
//$DTYPE RXN:VARIATION(1):STEPNO(1):SOLVENT(1):MOL(1):RGTNO
//$DATUM 2909
//$DTYPE RXN:VARIATION(1):STEPNO(1):SOLVENT(1):MOL(1):DISPLAY
//$DATUM THF
//$DTYPE RXN:VARIATION(1):STEPNO(1):SOLVENT(1):MOL(1):SYMBOL(1):SYMBOL
//$DATUM Diethylene oxide
//$DTYPE RXN:VARIATION(1):STEPNO(1):SOLVENT(1):MOL(1):SYMBOL(2):SYMBOL
//$DATUM Oxolane
//$DTYPE RXN:VARIATION(1):STEPNO(1):SOLVENT(1):MOL(1):SYMBOL(3):SYMBOL
//$DATUM THF
//$DTYPE RXN:VARIATION(1):STEPNO(1):SOLVENT(1):MOL(1):SYMBOL(4):SYMBOL
//$DATUM Tetrahydrofuran
//$DTYPE RXN:VARIATION(1):STEPNO(1):SOLVENT(1):MOL(1):SYMBOL(5):SYMBOL
//$DATUM Tetramethylene oxide
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):MOLSTRUCTURE
//$DATUM $MFMT
//
//  Accord  09290617232D
//
//  2  1  0  0  0  0  0  0  0  0999 V2000
//    5.0000    5.8250    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
//    5.0000    5.0000    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
//  1  2  1  0  0  0  0
//M  END
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):RGTNO
//$DATUM 1847
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):DISPLAY
//$DATUM H2O2
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):SYMBOL(1):SYMBOL
//$DATUM H2O2
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):SYMBOL(2):SYMBOL
//$DATUM HO-OH
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):SYMBOL(3):SYMBOL
//$DATUM HOOH
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):SYMBOL(4):SYMBOL
//$DATUM Hydrogen peroxide
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):SYMBOL(5):SYMBOL
//$DATUM Perhydrol
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(2):MOL(1):MOLSTRUCTURE
//$DATUM $MFMT
//
//  Accord  09290617232D
//
//  4  1  0  0  0  0  0  0  0  0999 V2000
//    5.0000    5.8435    0.0000 Na  0  3  0  0  0 15  0  0  0  0  0  0
//    5.0000    5.0093    0.0000 Na  0  3  0  0  0 15  0  0  0  0  0  0
//    5.6860    5.8250    0.0000 O   0  5  0  0  0  0  0  0  0  0  0  0
//    5.6860    5.0000    0.0000 O   0  5  0  0  0  0  0  0  0  0  0  0
//  3  4  1  0  0  0  0
//M  CHG  4   1   1   2   1   3  -1   4  -1
//M  END
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(2):MOL(1):RGTNO
//$DATUM 2323
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(2):MOL(1):DISPLAY
//$DATUM Na2O2
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(2):MOL(1):SYMBOL(1):SYMBOL
//$DATUM Na2O2
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(2):MOL(1):SYMBOL(2):SYMBOL
//$DATUM Sodium peroxide
//$RFMT
//$RXN
//
//      Accord   0929061723
//
//  1  1
//$MOL
//
//  Accord  09290617232D
//
// 23 24  0  0  0  0  0  0  0  0999 V2000
//   10.7224    6.5983    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//   10.0016    6.1771    0.0000 S   0  0  0  0  0  0  0  0  0  0  0  0
//   11.4338    6.1833    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//   10.7224    7.4221    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    9.2871    5.7652    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
//    9.5991    6.8978    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
//   10.4166    5.4719    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
//   12.1483    6.5952    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//   11.4338    7.8339    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    8.5726    6.1771    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
//   12.1483    7.4221    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    7.8581    5.7652    0.0000 C   0  0  0  0  0  0  0  0  0  1  0  0
//   12.8628    7.8339    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    7.1404    6.1771    0.0000 C   0  0  0  0  0  0  0  0  0  2  0  0
//    7.1404    7.0039    0.0000 C   0  0  0  0  0  0  0  0  0  3  0  0
//    6.4290    5.7652    0.0000 C   0  0  0  0  0  0  0  0  0  4  0  0
//    7.8581    7.4158    0.0000 C   0  0  0  0  0  0  0  0  0  5  0  0
//    6.4290    7.4158    0.0000 C   0  0  0  0  0  0  0  0  0  6  0  0
//    5.7145    6.1771    0.0000 C   0  0  0  0  0  0  0  0  0  7  0  0
//    7.8581    8.2395    0.0000 C   0  0  0  0  0  0  0  0  0  8  0  0
//    6.4290    8.2395    0.0000 C   0  0  0  0  0  0  0  0  0  9  0  0
//    5.0000    5.7652    0.0000 C   0  0  0  0  0  0  0  0  0 10  0  0
//    7.1404    8.6545    0.0000 C   0  0  0  0  0  0  0  0  0 11  0  0
//  1  2  1  0  0  0  0
//  1  3  1  0  0  0  0
//  1  4  2  0  0  0  0
//  2  5  1  0  0  0  0
//  2  6  2  0  0  0  0
//  2  7  2  0  0  0  0
//  3  8  2  0  0  0  0
//  4  9  1  0  0  0  0
//  5 10  1  0  0  0  0
//  8 11  1  0  0  0  0
// 10 12  2  0  0  0  4
// 11 13  1  0  0  0  0
// 12 14  1  0  0  0  0
// 14 15  1  0  0  0  0
// 14 16  1  0  0  0  0
// 15 17  1  0  0  0  0
// 15 18  2  0  0  0  0
// 16 19  1  0  0  0  0
// 17 20  2  0  0  0  0
// 18 21  1  0  0  0  0
// 19 22  2  0  0  0  0
// 20 23  1  0  0  0  0
//  9 11  2  0  0  0  0
// 21 23  2  0  0  0  0
//M  END
//$MOL
//
//  Accord  09290617232D
//
// 13 13  0  0  0  0  0  0  0  0999 V2000
//   17.4802    7.4751    0.0000 C   0  0  0  0  0  0  0  0  0  3  0  0
//   17.4802    6.6514    0.0000 C   0  0  0  0  0  0  0  0  0  2  0  0
//   18.1950    7.8889    0.0000 C   0  0  0  0  0  0  0  0  0  5  0  0
//   16.7655    7.8889    0.0000 C   0  0  0  0  0  0  0  0  0  6  0  0
//   16.7655    6.2375    0.0000 C   0  0  0  0  0  0  0  0  0  4  0  0
//   18.1950    6.2375    0.0000 C   0  0  0  0  0  0  0  0  0  1  0  0
//   18.1950    8.7126    0.0000 C   0  0  0  0  0  0  0  0  0  8  0  0
//   16.7655    8.7126    0.0000 C   0  0  0  0  0  0  0  0  0  9  0  0
//   16.0506    6.6514    0.0000 C   0  0  0  0  0  0  0  0  0  7  0  0
//   18.1950    5.4138    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
//   17.4802    9.1264    0.0000 C   0  0  0  0  0  0  0  0  0 11  0  0
//   15.3378    6.2375    0.0000 C   0  0  0  0  0  0  0  0  0 10  0  0
//   18.9098    5.0000    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
//  1  2  1  0  0  0  0
//  1  3  1  0  0  0  0
//  1  4  2  0  0  0  0
//  2  5  1  0  0  0  0
//  2  6  1  0  0  0  0
//  3  7  2  0  0  0  0
//  4  8  1  0  0  0  0
//  5  9  1  0  0  0  0
//  6 10  1  0  0  0  4
//  7 11  1  0  0  0  0
//  9 12  2  0  0  0  0
// 10 13  1  0  0  0  4
//  8 11  2  0  0  0  0
//M  END
//$DTYPE RXN:RXNREGNO
//$DATUM 2
//$DTYPE RXN:VARIATION(1):LITTEXT(1):LITTEXT
//$DATUM Bloodworth A J, Courtneidge J L, Curtis R J, Spencer M D, J Chem Soc Perk+
//in Trans 1, (11), p. 2951-2955, 1990
//$DTYPE RXN:VARIATION(1):LITREF(1):AUTHOR
//$DATUM Bloodworth A J; Courtneidge J L; Curtis R J; Spencer M D
//$DTYPE RXN:VARIATION(1):LITREF(1):JOURNAL_NO.
//$DATUM 11
//$DTYPE RXN:VARIATION(1):LITREF(1):JOURNAL_PG.
//$DATUM 2951-2955
//$DTYPE RXN:VARIATION(1):LITREF(1):JOURNAL_YEAR
//$DATUM 1990
//$DTYPE RXN:VARIATION(1):LITREF(1):JOURNAL_JRNL
//$DATUM J Chem Soc Perkin Trans 1
//$DTYPE RXN:VARIATION(1):LITREF(1):JOURNAL_CODEN
//$DATUM JCPRB4
//$DTYPE RXN:VARIATION(1):LITREF(1):TITLE
//$DATUM Preparation of unsaturated hydroperoxides from N-alkenyl-N'-p-tosylhydraz+
//ines. Overall step of a two-step sequence.
//$DTYPE RXN:VARIATION(1):LITREF(1):FULL_CITATION
//$DATUM Bloodworth A J, Courtneidge J L, Curtis R J, Spencer M D, J Chem Soc Perk+
//in Trans 1, (11), p. 2951-2955, 1990
//$DTYPE RXN:VARIATION(1):COMMENTS
//$DATUM Five examples given.
//$DTYPE RXN:VARIATION(1):KEYPHRASES
//$DATUM Chemoselective. Oxidative cleavage. Peroxidation. Reduction
//$DTYPE RXN:VARIATION(1):MDLNUMBER
//$DATUM RMOS00018016
//$DTYPE RXN:VARIATION(1):RXNINDEX
//$DATUM Oxidation.
//$DTYPE RXN:VARIATION(1):RCTINDEX
//$DATUM Alkenyl hydrazines.
//$DTYPE RXN:VARIATION(1):PROINDEX
//$DATUM Alkenyl hydroperoxides.
//$DTYPE RXN:VARIATION(1):RGTINDEX
//$DATUM Sodium peroxide.
//$DTYPE RXN:VARIATION(1):STEPNO(1):RXNTEXT(1):RXNTEXT
//$DATUM 1) NaBH3CN
//$DTYPE RXN:VARIATION(1):STEPNO(1):RXNTEXT(2):RXNTEXT
//$DATUM THF
//$DTYPE RXN:VARIATION(1):STEPNO(1):RXNTEXT(3):RXNTEXT
//$DATUM pH 3.5
//$DTYPE RXN:VARIATION(1):STEPNO(1):RXNTEXT(4):RXNTEXT
//$DATUM 4 h.
//$DTYPE RXN:VARIATION(1):STEPNO(1):RXNTEXT(5):RXNTEXT
//$DATUM 2) Na2O2/H2O2
//$DTYPE RXN:VARIATION(1):STEPNO(1):RXNTEXT(6):RXNTEXT
//$DATUM THF
//$DTYPE RXN:VARIATION(1):STEPNO(1):RXNTEXT(7):RXNTEXT
//$DATUM r.t./24 h.
//$DTYPE RXN:VARIATION(1):STEPNO(1):CONDITIONS(1):TEMPERATURE
//$DATUM 20.0 - 20.0
//$DTYPE RXN:VARIATION(1):PRODUCT(1):PRODUCT_NO
//$DATUM 1
//$DTYPE RXN:VARIATION(1):PRODUCT(1):YIELD
//$DATUM 39.0 - 39.0
//$DTYPE RXN:VARIATION(1):RXNREF(1):EXTREG
//$DATUM 18016
//$DTYPE RXN:VARIATION(1):RXNREF(1):PATH
//$DATUM A
//$DTYPE RXN:VARIATION(1):RXNREF(1):STEP
//$DATUM 2 Steps
//$DTYPE RXN:VARIATION(1):STEPNO(1):SOLVENT(1):MOL(1):MOLSTRUCTURE
//$DATUM $MFMT
//
//  Accord  09290617232D
//
//  5  5  0  0  0  0  0  0  0  0999 V2000
//    5.2631    6.2700    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    6.0795    6.2700    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    6.3426    5.4899    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    5.6713    5.0000    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
//    5.0000    5.4899    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//  1  5  1  0  0  0  0
//  1  2  1  0  0  0  0
//  2  3  1  0  0  0  0
//  3  4  1  0  0  0  0
//  4  5  1  0  0  0  0
//M  END
//$DTYPE RXN:VARIATION(1):STEPNO(1):SOLVENT(1):MOL(1):RGTNO
//$DATUM 2909
//$DTYPE RXN:VARIATION(1):STEPNO(1):SOLVENT(1):MOL(1):DISPLAY
//$DATUM THF
//$DTYPE RXN:VARIATION(1):STEPNO(1):SOLVENT(1):MOL(1):SYMBOL(1):SYMBOL
//$DATUM Diethylene oxide
//$DTYPE RXN:VARIATION(1):STEPNO(1):SOLVENT(1):MOL(1):SYMBOL(2):SYMBOL
//$DATUM Oxolane
//$DTYPE RXN:VARIATION(1):STEPNO(1):SOLVENT(1):MOL(1):SYMBOL(3):SYMBOL
//$DATUM THF
//$DTYPE RXN:VARIATION(1):STEPNO(1):SOLVENT(1):MOL(1):SYMBOL(4):SYMBOL
//$DATUM Tetrahydrofuran
//$DTYPE RXN:VARIATION(1):STEPNO(1):SOLVENT(1):MOL(1):SYMBOL(5):SYMBOL
//$DATUM Tetramethylene oxide
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):MOLSTRUCTURE
//$DATUM $MFMT
//
//  Accord  09290617232D
//
//  4  2  0  0  0  0  0  0  0  0999 V2000
//    5.6832    5.7219    0.0000 Na  0  3  0  0  0 15  0  0  0  0  0  0
//    5.0000    6.6500    0.0000 N   0  0  0  0  0  0  0  0  0  0  0  0
//    5.0000    5.8250    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0
//    5.0000    5.0000    0.0000 B   0  5  0  0  0  0  0  0  0  0  0  0
//  2  3  3  0  0  0  0
//  3  4  1  0  0  0  0
//M  CHG  2   1   1   4  -1
//M  END
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):RGTNO
//$DATUM 47
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):DISPLAY
//$DATUM NaBH3CN
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):SYMBOL(1):SYMBOL
//$DATUM H3BCNNa
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):SYMBOL(2):SYMBOL
//$DATUM Na(CN)BH3
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):SYMBOL(3):SYMBOL
//$DATUM Na(CN)BH4
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):SYMBOL(4):SYMBOL
//$DATUM Na(NC)BH3
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):SYMBOL(5):SYMBOL
//$DATUM NaB(CN)H3
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):SYMBOL(6):SYMBOL
//$DATUM NaBCNH3
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):SYMBOL(7):SYMBOL
//$DATUM NaBH3(CN)
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):SYMBOL(8):SYMBOL
//$DATUM NaBH3CN
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):SYMBOL(9):SYMBOL
//$DATUM NaCN.BH3
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):SYMBOL(10):SYMBOL
//$DATUM NaCNBH3
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):SYMBOL(11):SYMBOL
//$DATUM NaH3BCN
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):SYMBOL(12):SYMBOL
//$DATUM Na[BH3CN]
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):SYMBOL(13):SYMBOL
//$DATUM Sodium cyanoborohydride
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(1):MOL(1):SYMBOL(14):SYMBOL
//$DATUM Sodium cyanotrihydidoborate
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(2):MOL(1):MOLSTRUCTURE
//$DATUM $MFMT
//
//  Accord  09290617232D
//
//  2  1  0  0  0  0  0  0  0  0999 V2000
//    5.0000    5.8250    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
//    5.0000    5.0000    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0
//  1  2  1  0  0  0  0
//M  END
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(2):MOL(1):RGTNO
//$DATUM 1847
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(2):MOL(1):DISPLAY
//$DATUM H2O2
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(2):MOL(1):SYMBOL(1):SYMBOL
//$DATUM H2O2
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(2):MOL(1):SYMBOL(2):SYMBOL
//$DATUM HO-OH
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(2):MOL(1):SYMBOL(3):SYMBOL
//$DATUM HOOH
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(2):MOL(1):SYMBOL(4):SYMBOL
//$DATUM Hydrogen peroxide
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(2):MOL(1):SYMBOL(5):SYMBOL
//$DATUM Perhydrol
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(3):MOL(1):MOLSTRUCTURE
//$DATUM $MFMT
//
//  Accord  09290617232D
//
//  4  1  0  0  0  0  0  0  0  0999 V2000
//    5.0000    5.8435    0.0000 Na  0  3  0  0  0 15  0  0  0  0  0  0
//    5.0000    5.0093    0.0000 Na  0  3  0  0  0 15  0  0  0  0  0  0
//    5.6860    5.8250    0.0000 O   0  5  0  0  0  0  0  0  0  0  0  0
//    5.6860    5.0000    0.0000 O   0  5  0  0  0  0  0  0  0  0  0  0
//  3  4  1  0  0  0  0
//M  CHG  4   1   1   2   1   3  -1   4  -1
//M  END
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(3):MOL(1):RGTNO
//$DATUM 2323
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(3):MOL(1):DISPLAY
//$DATUM Na2O2
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(3):MOL(1):SYMBOL(1):SYMBOL
//$DATUM Na2O2
//$DTYPE RXN:VARIATION(1):STEPNO(1):CATALYST(3):MOL(1):SYMBOL(2):SYMBOL
//$DATUM Sodium peroxide
//$RFMT
//";
//			string tempFile = Path.GetTempFileName();
//			using(StreamWriter sw = new StreamWriter(tempFile))
//				sw.Write(rdf);
//			Indigo i = new Indigo();
//			IndigoObject reader = i.iterateRDFile(tempFile);
//			Assert.IsTrue(reader.at(0).getProperty("RXN:RXNREGNO").Equals("Baba1"));
//			Assert.IsTrue(reader.at(1).getProperty("RXN:RXNREGNO").Equals("2"));
//		}
    }
}
