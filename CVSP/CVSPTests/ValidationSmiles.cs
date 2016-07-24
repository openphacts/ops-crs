using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChemSpider.Molecules;
using System.Linq;
using System.Diagnostics;
using RSC.CVSP.Compounds;
using System.Collections.Generic;
using com.ggasoftware.indigo;
using RSC.CVSP;
using RSC;
using RSC.Logging;

namespace CVSPTests
{
    [TestClass]
    public class ValidationSmiles : CVSPTestBase
    {
        //[TestMethod]
        public void ValidationSmiles_ValidateAndCountIssues()
        {
            TestSmilesValidation("CCCC1=NN(C2=C1N=C(NC2=O)C3=C(C=CC(=C3)S(=O)(=O)N4CCN(CC4)C)OCC)C",
                "CCCC1C2N=C(C3C=C(S(N4CCN(C)CC4)(=O)=O)C=CC=3OCC)NC(=O)C=2N(C)N=1", 0);
            TestSmilesValidation(@"O[C@@H]3C/C(=C\C=C1/[C@@H2]CC[C@]2([C@H]1CC[C@@H]2[C@@H](/C=C/[C@H](C)C(O)(C)C)C)C)C[C@@H](O)C3",
                null, 1);
            TestSmilesValidation("[Br-].O=C(O[C@H]1C[C@@H]2[N+](C)(C)[C@H](C1)[C@H]3O[C@@H]23)C(O)(c4cccs4)c5cccs5",
                "[Br-].O=C(C(C1SC=CC=1)(C1SC=CC=1)O)O[C@@H]1C[C@@H]2[C@@H]3[C@H]([C@@H]([N+]2(C)C)C1)O3",2);
            TestSmilesValidation(@"O[C@@H]3C/C(=C\C=C1/[C@@H2]CC[C@]2([C@H]1CC[C@@H]2[C@@H](/C=C/[C@H](C)C(O)(C)C)C)C)C[C@@H](O)C3",
                null, 1);
            TestSmilesValidation("C(CC(O)(P(=O)(O)O)P(=O)(O)O[Na])CN.O.O.O", "C(CN)CC(P(O[Na])(O)=O)(P(O)(O)=O)O.O.O.O", 2);
            TestSmilesValidation(@"[N@H]=C(\N=C(/N)N)N(C)C", null, 1); // not sure what this is driving at?
            TestSmilesValidation("Clc1ccc2cc(sc2n1)S(=O)(=O)N3CCN(Cc4cc5c[nH]ccc5n4)C(=O)C3",
                "ClC1N=C2C(C=C(S(N3CC(=O)N(Cc4nc5c(c[nH]cc5)c4)CC3)(=O)=O)S2)=CC=1",
                1);
        }

        [TestMethod]
        public void ValidationSmiles_QuickTest()
        {
            List<Issue> issues = new List<Issue>();
            string result = ValidationModule.ValidateSMILES("c1ccccc1", issues, false, true, true);
            Assert.AreEqual("C1C=CC=CC=1", result, "wrong result returned");
            Assert.AreEqual(0, issues.Count, "wrong number of issues: " + String.Join("; ", issues.Select(i => i.Code)));
            List<Issue> i2 = new List<Issue>();
            string r2 = ValidationModule.ValidateSMILES("patently not a SMILES", i2, false, true, true);
            Assert.IsNull(r2);
            Assert.AreEqual(1, i2.Count, "wrong number of issues: " + String.Join("; ", i2.Select(i => i.Code)));
        }

        //[TestMethod]
        public void ValidationSmiles_List()
        {
            List<string> smiles_l = new List<string>() { 
                
@"CCCC1=NN(C2=C1N=C(NC2=O)C3=C(C=CC(=C3)S(=O)(=O)N4CCN(CC4)C)OCC)C",
@"O=C(O)C(c1ccc(cc1)CCN4CCC(c2nc3ccccc3n2CCOCC)CC4)(C)C",
@"O=C2N(CCc1cc(OC)c(OC)cc1C2)CCCN(C[C@@H]4c3cc(OC)c(OC)cc3C4)C",
@"O=C(OC(CC1OC(=O)C1CCCCCC)CCCCCCCCCCC)C(NC=O)CC(C)C",
@"FC(F)(F)c1cc(cc(c1)C(F)(F)F)[C@H](O[C@H]4OCCN(CC/2=N/C(=O)NN\2)[C@H]4c3ccc(F)cc3)C",
@"Cl.O=C(c1c3ccc(O)cc3sc1c2ccc(O)cc2)c5ccc(OCCN4CCCCC4)cc5",
@"CC(C)(C)NCC(c1ccc(c(c1)CO)O)O.CC(C)(C)NCC(c1ccc(c(c1)CO)O)O.OS(=O)(=O)O",
@"O[C@@H]3C/C(=C\C=C1CCC[C@]2([C@H]1CC[C@@H]2[C@@H](C=C[C@H](C)C(O)(C)C)C)C)C[C@@H](O)C3",
@"O[C@@H]3C/C(=C\C=C1/[C@@H2]CC[C@]2([C@H]1CC[C@@H]2[C@@H](/C=C/[C@H](C)C(O)(C)C)C)C)C[C@@H](O)C3",
@"O=S(=O)(N(c1nc(c(c(n1)C(C)C)/C=C/C(O)CC(O)CC(=O)O)c2ccc(F)cc2)C)C",
@"CC1CCC2CC(C(=CC=CC=CC(CC(C(=O)C(C(C(=CC(C(=O)CC(OC(=O)C3CCCCN3C(=O)C(=O)C1(O2)O)C(C)CC4CCC(C(C4)OC)OC(=O)C(C)(CO)CO)C)C)O)OC)C)C)C)OC",
@"O=C1N(C(=O)[C@H]3[C@@H]1[C@@H]2CC[C@H]3C2)CCCCN5CCN(c4ncccn4)CC5",
@"O=S(=O)(N1CCN(C)CC1)c4cc(C\2=N\C(=O)c3c(N/2)c(nn3C)CCC)c(OCC)cc4.O=C(O)C(O)(CC(=O)O)CC(=O)O",
@"Cl.O=C(\C=C\c1ccoc1)N(C)[C@H]5[C@@H]6Oc2c3c(ccc2O)C[C@H]4N(CC[C@]36[C@@]4(O)CC5)CC7CC7",
@"O=C(O)\C(=C\c1cnc(n1Cc2ccc(C(=O)O)cc2)CCCC)Cc3sccc3.O=S(=O)(O)C",
@"[Br-].O=C(O[C@H]1C[C@@H]2[N+](C)(C)[C@H](C1)[C@H]3O[C@@H]23)C(O)(c4cccs4)c5cccs5",
@"O=C1N(C(=O)[C@H]2[C@@H]1CCCC2)CCCCN5CCN(c4nsc3ccccc34)CC5",
@"O=C(c3nccc(Oc2ccc(NC(=O)Nc1cc(c(Cl)cc1)C(F)(F)F)c(F)c2)c3)NC",
@"[Ca+2].[O-]C(=O)C[C@H](O)C[C@H](O)CCn4c(c(C(=O)Nc1ccccc1)c(c2ccccc2)c4c3ccc(F)cc3)C(C)C.[O-]C(=O)C[C@H](O)C[C@H](O)CCn2c(c(c(c2c1ccc(F)cc1)c3ccccc3)C(=O)Nc4ccccc4)C(C)C",
@"O[C@H]1CC(\C(=C)[C@H](O)[C@H]1OCCCO)=C\C=C2/CCC[C@]3([C@H]2CC[C@@H]3[C@H](C)CCCC(O)(C)C)C",
@"CC(C)(C)c1ccc(cc1)S(=O)(=O)Nc2c(c(nc(n2)c3ncccn3)OCCO)Oc4ccccc4OC",
@"CCCS(=O)(=O)Nc1ccc(c(c1F)C(=O)c2c[nH]c3c2cc(cn3)c4ccc(cc4)Cl)F",
@"O=P(O)(O)O.Fc1cc(c(F)cc1F)C[C@@H](N)CC(=O)N3Cc2nnc(n2CC3)C(F)(F)F.O",
@"Cc5nnc(n5[C@@H]1C[C@H]4CC[C@@H](C1)N4CC[C@H](NC(=O)C2CCC(F)(F)CC2)c3ccccc3)C(C)C",
@"O=C(c1ccccc1C)Nc2ccc(c(c2)C)C(=O)N4c3ccc(Cl)cc3C(O)CCC4",
@"Clc1ccc(nc1)NC(=O)C(=O)N[C@H]4CC[C@H](C(=O)N(C)C)C[C@H]4NC(=O)c2nc3c(s2)CN(CC3)C",
@"C[C@@H]1CC[C@H]2C[C@@H](/C(=C/C=C/C=C/[C@H](C[C@H](C(=O)[C@@H]([C@@H](/C(=C/[C@H](C(=O)C[C@H](OC(=O)[C@@H]3CCCCN3C(=O)C(=O)[C@@]1(O2)O)[C@@H](C)C[C@@H]4CC[C@H]([C@@H](C4)OC)OCCO)C)/C)O)OC)C)C)/C)OC",
@"O[C@@H]1CC(\C(=C)[C@@H](O)C1)=C\C=C2/CCC[C@]3([C@H]2CC[C@@H]3[C@@H](/C=C/[C@H](C)C(C)C)C)C",
@"CCCCCCCCCCCCCCCC(O)=O.CC=5\N=C1\C(O)CCCN1C(=O)C=5CCN2CCC(CC2)c4noc3cc(F)ccc34",
@"O=C2\N=C(/Nc1c(nn(c12)C)CCC)c3cc(ccc3OCCC)S(=O)(=O)NCCC4N(C)CCC4",
@"n3c(nc2c(nc(nc2N1CCCCC1)N(CCO)CCO)c3N4CCCCC4)N(CCO)CCO",
@"COC(=O)[C@@H]4C\C1=C\C(=O)CC[C@]1(C)[C@@]65O[C@@H]6C[C@@]3(C)[C@@H](CC[C@]23CCC(=O)O2)[C@H]45",
@"Cl[C@H]4CC[C@@H](/C=C(\C)[C@H]1OC(=O)[C@H]3N(C(=O)C(=O)[C@]2(O)O[C@H]([C@@H](OC)C[C@H](C/C(=C/[C@H](C(=O)C[C@H](O)[C@@H]1C)CC)C)C)[C@@H](OC)C[C@H]2C)CCCC3)C[C@H]4OC",
@"O=C1N(/C(=C\C(=O)N1C)N2CCC[C@@H](N)C2)Cc3ccccc3C#N.O=C(O)c1ccccc1",
@"O=C(OCCOC)\C2=C(\N/C(=C(/C(=O)OC\C=C\c1ccccc1)C2c3cccc([N+]([O-])=O)c3)C)C",
@"O=C(OCC)CCN(c1ncccc1)C(=O)c4ccc2c(nc(n2C)CNc3ccc(C(=N\C(=O)OCCCCCC)\N)cc3)c4",
@"O=C(OCC(=O)[C@]3(OC(=O)CC)[C@]2(C[C@H](O)[C@]4(Cl)[C@@]/1(\C(=C/C(=O)\C=C\1)CC[C@H]4[C@@H]2C[C@@H]3C)C)C)CC",
@"[Mg+2].O=S(c1[n-]c2ccc(OC)cc2n1)Cc3ncc(c(OC)c3C)C.O=S(c1[n-]c2ccc(OC)cc2n1)Cc3ncc(c(OC)c3C)C",
@"[Na+].[O-]C(=O)CC1(CC1)CS[C@@H](c2cccc(c2)\C=C\c3nc4cc(Cl)ccc4cc3)CCc5ccccc5C(O)(C)C",
@"O=P(O)(O)Oc1c(OC)cc(cc1OC)[C@@H]7c3cc2OCOc2cc3[C@@H](O[C@@H]5O[C@H]4[C@@H](O[C@@H](OC4)C)[C@H](O)[C@H]5O)[C@@H]6[C@@H]7C(=O)OC6",
@"O=C2N(c1nc(n(c1C(=O)N2Cc4nc3ccccc3c(n4)C)CC#CC)N5CCC[C@@H](N)C5)C",
@"FC(F)(F)c1ccc(cc1)c2ccccc2C(=O)NC6CCN(CCCCC5(C(=O)NCC(F)(F)F)c3ccccc3c4ccccc45)CC6",
@"O=C7O[C@@]6([C@@]3([C@H]([C@@H]2[C@@H]4[C@H](/C1=C/C(=O)CC[C@]1(C)[C@H]2CC3)C4)[C@@H]5C[C@@H]56)C)CC7",
@"O=C2\N=C(/Nc1c(cn(c12)CC)CCC)c3cc(ccc3OCCC)S(=O)(=O)N4CCN(CCO)CC4",
@"Cl.O=C1N(C(=O)[C@H]3[C@@H]1[C@@H]2CC[C@H]3C2)C[C@@H]4CCCC[C@H]4CN7CCN(c6nsc5ccccc56)CC7",
@"O=C(Nc1nc(C(=O)NCCN(C(C)C)C(C)C)cs1)c2cc(OC)c(OC)cc2O.Cl.O.O.O",
@"Fc1ccc(cc1F)[C@@H]5C[C@H]5Nc4nc(nc2c4nnn2[C@@H]3C[C@H](OCCO)[C@@H](O)[C@H]3O)SCCC",
@"O[C@@H]1CC(\C(=C)[C@@H](O)C1)=C\C=C2/CCC[C@]3([C@H]2CC[C@@H]3[C@H](C)CCCC(C)C)C",
@"COc1cc2c(ccnc2cc1OC)Oc3ccc(cc3)NC(=O)C4(CC4)C(=O)Nc5ccc(cc5)F",
@"O=C5\C=C3/C(=C2/[C@@H](c1ccc(N(C)C)cc1)C[C@@]4([C@@](OC(=O)C)(C(=O)C)CC[C@H]4[C@@H]2CC3)C)CC5",
@"FC(F)(F)COc3ccccc3OCCN[C@H](C)Cc1cc2c(c(c1)C(=O)N)N(CC2)CCCO",
@"C[C@H]1C(=O)N[C@H]2CSSC[C@H]3C(=O)N[C@H](C(=O)N[C@H](C(=O)N[C@@H](CSSC[C@@H](C(=O)N3)N)C(=O)N[C@@H](CSSC[C@H](NC(=O)CNC(=O)[C@@H](NC2=O)[C@@H](C)O)C(=O)N[C@@H](Cc4ccc(cc4)O)C(=O)O)C(=O)N[C@H](C(=O)N5CCC[C@H]5C(=O)N1)CC(=O)N)Cc6ccc(cc6)O)CCC(=O)O",
@"O=C5/C=C\N=C(\N(C)C4CCN(c1nc3ccccc3n1Cc2ccc(F)cc2)CC4)N5",
@"O[C@@H]1CC[C@H](C[C@H]1OC)C[C@@H](C)[C@@H]4CC(=O)[C@H](C)/C=C(\C)[C@@H](O)[C@@H](OC)C(=O)[C@H](C)C[C@H](C)\C=C\C=C\C=C(/C)[C@@H](OC)C[C@@H]2CC[C@@H](C)[C@@](O)(O2)C(=O)C(=O)N3CCCC[C@H]3C(=O)O4",
@"O=C1O/C(=C(\O1)C)COC(=O)c3cccc2nc(OCC)n(c23)Cc6ccc(c5ccccc5C\4=N\C(=O)ON/4)cc6",
@"O=C5N(c4ccc(N3C(=O)c1c(c(nn1c2ccc(OC)cc2)C(=O)N)CC3)cc4)CCCC5",
@"O[C@@H]1CC(\C(=C)CC1)=C\C=C2/CCC[C@]3([C@H]2CC[C@@H]3[C@H](C)CCCC(C)C)C",
@"O=C(O)CCC(=O)O.Clc2c(c1c(OCC1)c(c2)C(=O)NC3CCN(CCCOC)CC3)N",
@"CC1=C2[C@@]([C@]([C@H]([C@@H]3[C@]4([C@H](OC4)C[C@@H]([C@]3(C(=O)[C@@H]2OC(=O)C)C)O)OC(=O)C)OC(=O)c5ccccc5)(C[C@@H]1OC(=O)[C@H](O)[C@@H](NC(=O)c6ccccc6)c7ccccc7)O)(C)C",
@"O=C(O)c1ccccc1c2ccc(cc2)Cn3c4cc(cc(c4nc3CCC)C)c5nc6ccccc6n5C",
@"Cl.O=C(O)C(c1ccc(cc1)C(O)CCCN2CCC(CC2)C(O)(c3ccccc3)c4ccccc4)(C)C",
@"O=C(O)C(c1ccc(cc1)C(O)CCCN2CCC(CC2)C(O)(c3ccccc3)c4ccccc4)(C)C",
@"O=C(O)CCC(=O)O.O=C(O[C@@H]2C1CCN(CC1)C2)N5[C@@H](c3ccccc3)c4ccccc4CC5",
@"O=C(O[C@@H]1[C@H]3C(=C/[C@H](C)C1)\C=C/[C@@H]([C@@H]3CC[C@H]2OC(=O)C[C@H](O)C2)C)C(C)(C)CC",
@"Cl.O=C\1N4\C(=C/C2=C/1COC(=O)[C@]2(O)CC)c3nc5c(c(c3C4)CCNC(C)C)cccc5",
@"O=C1NC(=O)SC1Cc3ccc(OCCN(c2ncccc2)C)cc3.O=C(O)\C=C/C(=O)O",
@"Oc1ccc(cc1)c3c(c2cc(O)ccc2n3Cc5ccc(OCCN4CCCCCC4)cc5)C",
@"O=S(=O)(O)c1ccccc1.Clc1ccc(cc1)[C@H](OC2CCN(CCCC(=O)O)CC2)c3ncccc3",
@"c1ccc(cc1)C(c2ccccc2)C(=O)NC3=CN(CC3)CCc4ccc5c(c4)CCO5.Br",
@"O=C(O)CNC(=O)[C@@H](Cc1ccccc1)CN3CC[C@@](c2cccc(O)c2)([C@H](C3)C)C",
@"O=C(O[C@@H]2C/C1=C/C[C@@H]4[C@@H]([C@@]1(C)CC2)CC[C@@]3(C(=C/C[C@H]34)\c5cccnc5)C)C",
@"O=C4\C=C3\C(\Cl)=C/[C@@H]1[C@H](CC[C@@]2([C@@](OC(=O)C)(C(=O)C)CC[C@@H]12)C)[C@@]3(C)[C@H]5C[C@@H]45",
@"O=C(O)C[C@H](O)C[C@H](O)/C=C/c1c(c3ccccc3nc1C2CC2)c4ccc(F)cc4",
@"O=C\1\C=C/[C@]2(/C(=C/1)CC[C@H]3[C@H]4[C@](C[C@H](O)[C@H]23)([C@@]5(OC(O[C@@H]5C4)CCC)C(=O)CO)C)C",
@"O=C(O)[C@H](O)[C@@H](O)C(=O)O.O=C(N(C)C)Cc1c(nc2ccc(cn12)C)c3ccc(cc3)C.O=C(N(C)C)Cc1c(nc2ccc(cn12)C)c3ccc(cc3)C",
@"O=C(N1CCSC1)[C@H]5NC[C@@H](N4CCN(c3cc(nn3c2ccccc2)C)CC4)C5",
@"O=C(c1ccccc1C)Nc2ccc(cc2)C(=O)N4c3ccccc3C(N(C)C)CCC4",
@"FC(F)(F)c3cc(N2C(=O)C(N(c1cc(F)c(C(=O)NC)cc1)C2=S)(C)C)ccc3C#N",
@"Cl.O=C2\N=C(/Nn1c(nc(c12)C)CCC)c3cc(ccc3OCC)S(=O)(=O)N4CCN(CC)CC4",
@"O=S(=O)(O)c1ccc(cc1)C.O=S(=O)(O)c1ccc(cc1)C.Fc1cccc(c1)COc2ccc(cc2Cl)Nc5ncnc4c5cc(c3oc(cc3)CNCCS(=O)(=O)C)cc4.O",
@"FC(F)(F)C(O)(C(F)(F)F)CCC[C@@H](C)[C@H]3CC[C@H]2C(=C\C=C1/C(=C)[C@@H](O)C[C@H](O)C1)\CCC[C@@]23C",
@"O.CN(C)[C@@H]3C(\O)=C(\C(N)=O)C(=O)[C@@]4(O)C(/O)=C2/C(=O)c1c(cccc1O)[C@H](C)[C@H]2[C@H](O)[C@@H]34",
@"CC(C)c1c(c(c(n1CC[C@H](C[C@H](CC(=O)O)O)O)c2ccc(cc2)F)c3ccccc3)C(=O)Nc4ccccc4",
@"O=S(=O)(c1ccccc1C)NC(=O)c2ccc(c(OC)c2)Cc4c3cc(ccc3n(c4)C)NC(=O)OC5CCCC5",
@"[O-][N+](=O)c1cccc(c1)C5C(/C(=O)OC(C)C)=C(\NC(\N)=C5\C(=O)OC4CN(C(c2ccccc2)c3ccccc3)C4)C",
@"c1ccc2c(c1)C(Nc3ccccc3S2)N4CCN(CC4)CCOCCO.C(=CC(=O)O)C(=O)O",
@"O=S(=O)(O)c1ccccc1.Clc1ccccc1C2C(=C(/N/C(=C2/C(=O)OCC)COCCN)C)\C(=O)OC",
@"FC(F)(F)c1nc(nc3c1CCN(C(=O)C[C@H](N)CN2C(=O)CCC(F)(F)C2)C3)C(F)(F)F",
@"Cl.O=C(OCC)[C@@H](N[C@H](C(=O)N2[C@H](C(=O)O)CC1(SCCS1)C2)C)CCc3ccccc3",
@"O=C(N2[C@H](C(=O)O)C[C@H](Sc1ccccc1)C2)[C@H](C)CSC(=O)c3ccccc3",
@"Clc1ccc(cc1Cc2ccc(OCC)cc2)[C@@H]3O[C@H](CO)[C@@H](O)[C@H](O)[C@H]3O",
@"FC(F)(F)c1cc(c(cc1)C(F)(F)F)NC(=O)[C@@H]3[C@]2(CC[C@H]4[C@H]([C@@H]2CC3)CC[C@H]5NC(=O)\C=C/[C@]45C)C",
@"Cc1ccc(cc1C)N2C(=O)/C(=N\Nc3cccc(c3O)c4cccc(c4)C(=O)O)/C(=N2)C.C(CO)N",
@"O=C2N[C@@H](C(=O)N[C@H]1C(=O)N\C(=C/C)C(=O)N[C@H](C(=O)O[C@H](/C=C/CCSSC1)C2)C(C)C)C(C)C",
@"Cc1ccc(cc1Cc2ccc(s2)c3ccc(cc3)F)[C@H]4[C@@H]([C@H]([C@@H]([C@H](O4)CO)O)O)O",
                
@"O=C(c2ccc(OCCCCc1ccccc1)cc2)Nc5cccc3c5O/C(=C\C3=O)c4nnnn4",
@"Cl.O=C(c2ccccc2c1ccccc1)Nc3ccc(cc3)C(=O)N5c6c(c4nc(nc4CC5)C)cccc6",
@"O=C1O/C(=C(\O1)C)COC(=O)c2c(nc(n2Cc5ccc(c4ccccc4c3nnnn3)cc5)CCC)C(O)(C)C",
@"O=C1O/C(=C(\O1)C)COC(=O)c2c(nc(n2Cc5ccc(c4ccccc4c3nnnn3)cc5)CCC)CC",
@"O.NC(=O)[C@@H]1CCCN1C(=O)[C@@H](NC(=O)[C@@H]2CC(=O)N(C)C(=O)N2)Cc3cncn3",
@"O=C(c2c1ccccc1n(c2)C)C4Cc3ncnc3CC4",
@"O=C1N(\C(=N/C12CCCC2)CCCC)Cc5ccc(c3ccccc3c4nnnn4)cc5",
@"O=C(NC)c4ccccc4Sc1ccc2c(c1)nnc2\C=C\c3ncccc3",
@"O=S(c2nc1ccccc1n2)Cc3nccc(OCCCOC)c3C",
@"O=S(c1nc2c(n1)cc(cc2)n3cccc3)Cc4nccc(OC)c4C",
@"O=C(O)[C@@H](N(C(=O)CCCC)Cc3ccc(c1ccccc1c2nnnn2)cc3)C(C)C",
@"S=C(N(C)C)CC\1=C(\N=C(/N(C/1=O)Cc4ccc(c2ccccc2c3nnnn3)cc4)CCCC)C",
@"O=S(c2nc1ccc(OC)cc1n2)Cc3ncc(c(OC)c3C)C",
@"O=C(N[C@H]1C[C@H]2CN(C)C[C@@H](C1)N2C)c4nnc3ccccc34",
@"FC(F)(F)COc1c(c(ncc1)CS(=O)c3nc2ccccc2n3)C",

};
            string output_smiles = string.Empty;
            int errorCount = 0;
            foreach (string smiles in smiles_l)
            {
                List<Issue> issues = new List<Issue>();
//				string indigo_smiles = Validation.validateSmiles(smiles, issues);
                if((from issue in issues join  et in LogManager.Logger.EntryTypes on issue.Code equals et.Code where et.Severity == Severity.Error select issue).Count() > 0)
                {
                    errorCount++;
                    output_smiles += smiles + Environment.NewLine;
                }
                
            }
            Assert.AreEqual(17, errorCount, "wrong number of errors");
            Assert.IsTrue(output_smiles.Equals(@"O[C@@H]3C/C(=C\C=C1/[C@@H2]CC[C@]2([C@H]1CC[C@@H]2[C@@H](/C=C/[C@H](C)C(O)(C)C)C)C)C[C@@H](O)C3
CCCS(=O)(=O)Nc1ccc(c(c1F)C(=O)c2c[nH]c3c2cc(cn3)c4ccc(cc4)Cl)F
O=C(c2ccc(OCCCCc1ccccc1)cc2)Nc5cccc3c5O/C(=C\C3=O)c4nnnn4
Cl.O=C(c2ccccc2c1ccccc1)Nc3ccc(cc3)C(=O)N5c6c(c4nc(nc4CC5)C)cccc6
O=C1O/C(=C(\O1)C)COC(=O)c2c(nc(n2Cc5ccc(c4ccccc4c3nnnn3)cc5)CCC)C(O)(C)C
O=C1O/C(=C(\O1)C)COC(=O)c2c(nc(n2Cc5ccc(c4ccccc4c3nnnn3)cc5)CCC)CC
O.NC(=O)[C@@H]1CCCN1C(=O)[C@@H](NC(=O)[C@@H]2CC(=O)N(C)C(=O)N2)Cc3cncn3
O=C(c2c1ccccc1n(c2)C)C4Cc3ncnc3CC4
O=C1N(\C(=N/C12CCCC2)CCCC)Cc5ccc(c3ccccc3c4nnnn4)cc5
O=C(NC)c4ccccc4Sc1ccc2c(c1)nnc2\C=C\c3ncccc3
O=S(c2nc1ccccc1n2)Cc3nccc(OCCCOC)c3C
O=S(c1nc2c(n1)cc(cc2)n3cccc3)Cc4nccc(OC)c4C
O=C(O)[C@@H](N(C(=O)CCCC)Cc3ccc(c1ccccc1c2nnnn2)cc3)C(C)C
S=C(N(C)C)CC\1=C(\N=C(/N(C/1=O)Cc4ccc(c2ccccc2c3nnnn3)cc4)CCCC)C
O=S(c2nc1ccc(OC)cc1n2)Cc3ncc(c(OC)c3C)C
O=C(N[C@H]1C[C@H]2CN(C)C[C@@H](C1)N2C)c4nnc3ccccc34
FC(F)(F)COc1c(c(ncc1)CS(=O)c3nc2ccccc2n3)C
"));
        }

        public void TestSmilesValidation(string inputSMILES, string expected, int expectedCount)
        {
            List<Issue> issues = new List<Issue>();
            string result = ValidationModule.ValidateSMILES(inputSMILES, issues, false, true, true);
            Console.WriteLine(result);
            Assert.AreEqual(expectedCount, issues.Count, String.Join("; ", issues));
            Assert.AreEqual(expected, result, "wrong SMILES result");
        }
    }
}
