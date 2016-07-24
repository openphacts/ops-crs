using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSC.CVSP.Compounds;
using com.ggasoftware.indigo;

using ChemSpider.Molecules;


namespace CVSPTests
{
    /// <summary>
    /// 2015-08-27: now this is in RSCCheminfToolkit.
    /// </summary>
    [TestClass]
    public class CheminformaticsIndigoReaction
    {
        [TestMethod]
        public void CheminformaticsIndigoReaction_ApplySMIRKS()
        {
            string molfile = Resource1.mercaptotetrahydropyrimidinol;

            List<string> smirks_l = new List<string>();
            
            
            smirks_l.Add("[N-,P-,As-,Sb-,O-,S-,Se-,Te-:1][C:2]=[N+,P+,As+,Sb+,O+,S+,Se+,Te+:3]>>[N,P,As,Sb,O,S,Se,Te:1]=[C:2][N,P,As,Sb,O,S,Se,Te:3]");
            smirks_l.Add("[O-,S-,Se-,Te-:1][P+;D4:2][O-,S-,Se-,Te-:3]>>[O,S,Se,Te:1]=[P;D5:2][O-,S-,Se-,Te-:3]");
            smirks_l.Add("[F-,Cl-,Br-,I-,At-:1]=[O:2]>>[F,Cl,Br,I,At:1][O-:2]");
            smirks_l.Add("[S-;v7:1]=[O:2]>>[S:1][O-:2]");
            smirks_l.Add("[Se-;v7:1]=[S,O:2]>>[Se:1][S-,O-:2]");
            smirks_l.Add("[Te-;v7:1]=[Se,S,O:2]>>[Te:1][Se-,S-,O-:2]");
            smirks_l.Add("[*:1][C+,S+:2]([N:3][*:4])[N:5]([H:6])[*:7]>>[*:1][C,S:2]([N:3][*:4])=[N+:5]([H:6])[*:7]");
            smirks_l.Add("[*:1][P+:2]([*:7])([N:3][*:4])[N:5]([H:6])[*:7]>>[*:1][P:2]([*:7])([N:3][*:4])=[N+:5]([H:6])[*:7]");
            smirks_l.Add("[C;v2:1][N,P,As:2]=[O,S,Se,Te:3]>>[C:1]#[N,P,As:2]=[O,S,Se,Te:3]");
            smirks_l.Add("[C;v2:1][N,P,As:2]=[O,S,Se,Te:3]>>[C:1]#[N,P,As:2]=[O,S,Se,Te:3]");
            smirks_l.Add("[O,S,Se,Te:1]=[O+,S+,Se+,Te+:2][C-;v3:3]>>[O,S,Se,Te:1]=[O,S,Se,Te:2]=[C:3]");
            smirks_l.Add("[O-,S-,Se-,Te-:1][O+&v3,S+&v3,Se+&v3,Te+&v3:2]>>[O,S,Se,Te:1]=[O,S,Se,Te:2]");
            smirks_l.Add("[O-,S-,Se-,Te-:1][O,S,Se,Te:2][C+;v3]>>[O,S,Se,Te:1]=[O,S,Se,Te:2]=[C]");
            smirks_l.Add("[N&v3,P&v3,As&v3,Sb&v3:1]#[N&v4,P&v4,As&v4,Sb&v4:2]>>[N-,P-,As-,Sb-:1]=[N+,P+,As+,Sb+:2]");
            smirks_l.Add("[N-,P-,As-,Sb-:1]=[O+&v3,S+&v3,Se+&v3,Te+&v3:2]>>[N,P,As,Sb:1]#[O,S,Se,Te:2]");
            smirks_l.Add("[N-,P-,As-,Sb-:1]=[C+;v3:2]>>[N,P,As,Sb:1]#[C:2]");
            smirks_l.Add("[P+&X4,As+&X4,Sb+&X4:1][O-,S-,Se-,Te-:2]>>[P,As,Sb:1]=[O,S,Se,Te:2]");
            smirks_l.Add("[N+&v4,P+&v4,As+&v4,Sb+&v4:1][C-;v3:2]>>[N,P,As,Sb:1]=[C:2]");
            smirks_l.Add("[N+&v4,P+&v4,As+&v4,Sb+&v4:1]=[C-;v3:2]>>[N,P,As,Sb:1]#[C:2]");
            smirks_l.Add("[N+&v4,P+&v4,As+&v4,Sb+&v4:1][N-&v2,P-&v2,As-&v2,Sb-&v2:2]>>[N,P,As,Sb:1]=[N,P,As,Sb:2]");
            smirks_l.Add("[O+&v3,S+&v3,Se+&v3,Te+&v3:1][C-&v3:2]>>[O,S,Se,Te:1]=[C:2]");
            smirks_l.Add("[O+&v3,S+&v3,Se+&v3,Te+&v3:1]=[C-&v3:2]>>[O,S,Se,Te:1]#[C:2]");
            smirks_l.Add("[O+&v3,S+&v3,Se+&v3,Te+&v3:1][N-&v2,P-&v2,As-&v2:2]>>[O,S,Se,Te:1]=[N,P,As:2]");
            smirks_l.Add("[N-&v2,P-&v2,As-&v2,Sb-&v2:1][O:2][C+&v3:3]>>[N,P,As,Sb:1]=[O:2]=[C:3]");
            smirks_l.Add("[C-&v3:1][N,P,As,Sb:2]([*:3])[C+&v3:4]>>[C:1]=[N,P,As,Sb:2]([*:3])=[C:4]");
            smirks_l.Add("[C-&v3:1][N,P,As,Sb:2]=[C+&v3:3]>>[C:1]=[N,P,As,Sb:2]#[C:3]");
            smirks_l.Add("[C;v4:1]=[N,P,As,Sb:2][C;v2:3]>>[C:1]=[N,P,As,Sb:2]#[C:3]");
            smirks_l.Add("[*:1][N,P,As,Sb:2](=[O,S,Se,Te:3])=[O,S,Se,Te:4]>>[*:1][N+,P+,As+,Sb+:2]([O-,S-,Se-,Te-:3])=[O,S,Se,Te:4]");
            smirks_l.Add("[n:1]=[O:2]>>[n+:1][O-:2]");
            smirks_l.Add("[*,H:1][N:2]=[N:3]#[N:4]>>[*,H:1][N:2]=[N+:3]=[N-:4]");
            smirks_l.Add("[*:1]=[N:2]#[N:3]>>[*:1]=[N+:2]=[N-:3]");
            smirks_l.Add("[!O:1][S+0;X3:2](=[O:3])[!O:4]>>[!O:1][S+1;X3:2]([O-:3])[!O:4]");
            smirks_l.Add("[H:1][S:2][c:3]1[n:8][c:7]([H,*:13])[n:6][c:5]2[c:4]1[n:11][c:10]([H,*:12])[n:9]2>>[H:1][N:8]1[C:7]([H,*:13])=[N:6][C:5]2=[C:4]([N:11]=[C:10]([H,*:12])[N:9]2)[C:3]1=[S:2]");
            smirks_l.Add("[H:1][#7:8]-1-[#6:7](-[*:9])=[#7:6]-[#6:5]-2=[#6:4]-1-[#6:12](=[S:13])-[#7:11]-[#6:10](-[*:2])=[#7:3]-2>>[H:1][#7:8]-1-[#6:7](-[*:9])=[#7:6]-[#6:5]-2=[#6:4]-1-[#7:3]=[#6:10](-[*:2])-[#7:11]-[#6:12]-2=[S:13]");
            smirks_l.Add("[H:1][S:9][C:6]1=[N:5][C:4]([H,*:12])=[C:3]([H,*:11])[C:8](=[O:10])[N:7]1[H:2]>>[H:1][N:5]1[C:4]([H,*:12])=[C:3]([H,*:11])[C:8](=[O:10])[N:7]([H:2])[C:6]1=[S:9]");
            smirks_l.Add("[H:2][O:10][C:8]1=[N:7][C:6](=[S:9])[N:5]([H:1])[C:4]([H,*:12])=[C:3]1[H,*:11]>>[H:1][N:5]1[C:4]([H,*:12])=[C:3]([H,*:11])[C:8](=[O:10])[N:7]([H:2])[C:6]1=[S:9]");
            smirks_l.Add("[H:2][O:3][c:4]1[n:5][c:6]([S:7][H:1])[n:8][c:9]([H,*:10])[c:11]1[H,*:12]>>[H:1][N:8]1[C:9]([H,*:10])=[C:11]([H,*:12])[C:4](=[O:3])[N:5]([H:2])[C:6]1=[S:7]");
            smirks_l.Add("[H:23][C:13]1([H,*:25])[C:12](=[O:24])-[C:11](-[H,*:26])[C:10]2([H,*:27])[C:9]([H,*:28])([H,*:29])[C:8]3([H,*:30])[C:17]([H:21])([C:18](=[O:19])-[C:1]-4=[C:6](-[C:5](-[H,*:33])=[C:4](-[H,*:34])-[C:3](-[H,*:35])=[C:2]-4-[H,*:36])[C:7]3([H,*:32])[H,*:31])[C:16](=[O:20])[C:15]2([H,*:37])[C:14]1=[O:22]>>[H:23][O:24]-[C:12]-1=[C:13](-[H,*:25])-[C:14](=[O:22])[C:15]2([H,*:37])[C:16](-[O:20][H:21])=[C:17]3-[C:18](=[O:19])-[C:1]-4=[C:6](-[C:5](-[H,*:33])=[C:4](-[H,*:34])-[C:3](-[H,*:35])=[C:2]-4-[H,*:36])[C:7]([H,*:32])([H,*:31])[C:8]3([H,*:30])[C:9]([H,*:29])([H,*:28])[C:10]2([H,*:27])[C:11]-1-[H,*:26]");
            smirks_l.Add("[H:19][C:15]1([H,*:20])[C:14](=[O:21])[C:13]([H,*:22])[C:12]2([H,*:23])[C:11]([H,*:24])([H,*:25])[c:10]3[c:9]([H,*:26])[c:8]4[c:7]([H,*:27])[c:6]([H,*:28])[c:5]([H,*:29])[c:4]([H,*:30])[c:3]4[c:2]([H,*:31])[c:1]3[C:18](=[O:32])[C:17]2([H,*:34])[C:16]1=[O:33]>>[H:19][O:21][C:14]1=[C:15]([H,*:20])[C:16](=[O:33])[C:17]2([H,*:34])[C:18](=[O:32])[C:1]3=[C:2]([H,*:31])[C:3]4=[C:4]([H,*:30])[C:5]([H,*:29])=[C:6]([H,*:28])[C:7]([H,*:27])=[C:8]4[C:9]([H,*:26])=[C:10]3[C:11]([H,*:25])([H,*:24])[C:12]2([H,*:23])[C:13]1[H,*:22]");
            smirks_l.Add("[H:2][O:15][c:14]1[c:13]2[C:11](=[O:12])[C:10]3=[C:5]([C:4](=[O:24])[c:3]2[c:18]([O:19][H:1])[c:17]2[c:16]1[c:23]([H,*:36])[c:22]([H,*:35])[c:21]([H,*:34])[c:20]2[H,*:33])[C:6]([H,*:25])([H,*:32])[C:7]([H,*:30])([H,*:31])[C:8]([H,*:28])([H,*:29])[C:9]3([H,*:26])[H,*:27]>>[H:2][O:12][C:11]1=[C:10]2[C:5](=[C:4]([O:24][H:1])[C:3]3=[C:13]1[C:14](=[O:15])[C:16]1=[C:17]([C:20]([H,*:33])=[C:21]([H,*:34])[C:22]([H,*:35])=[C:23]1[H,*:36])[C:18]3=[O:19])[C:6]([H,*:25])([H,*:32])[C:7]([H,*:30])([H,*:31])[C:8]([H,*:28])([H,*:29])[C:9]2([H,*:26])[H,*:27]");
            smirks_l.Add("[H,*:6][c:2]1[n:1][n-:5][n:4][n:3]1>>[H,*:6][C:2]1=[N:1][N:5]=[N:4][N-:3]1");
            smirks_l.Add("[H:1][N:6]([H,*:7])[C:3]([O-:2])=[N:5][H,*:4]>>[H:1][N:6]([H,*:7])[C:3](=[O:2])[N-:5][H,*:4]");
            smirks_l.Add("[H:17][O:16][C:14](=[O:15])[c:1]1[c:6]([H,*:26])[c:5]([H,*:25])[c:4]([H,*:24])[c:3]([H,*:23])[c:2]1[C:7]([H,*:27])=[C:8]1[C:9]([H,*:21])=[C:10]([H,*:22])[C:11](=[O:18])[C:12]([H,*:20])=[C:13]1[H,*:19]>>[H:17][O:18][C:11]1=[C:10]([H,*:22])[C:9]([H,*:21])=[C:8]([C:13]([H,*:19])=[C:12]1[H,*:20])[C:7]1([H,*:27])[O:16][C:14](=[O:15])[C:1]2=[C:6]([H,*:26])[C:5]([H,*:25])=[C:4]([H,*:24])[C:3]([H,*:23])=[C:2]12");
            smirks_l.Add("[O-:17][c:11]1[c:10]([H,*:21])[c:9]([H,*:22])[c:8]([c:13]([H,*:19])[c:12]1[H,*:20])[C:7]1([H,*:18])[O:16][C:14](=[O:15])[c:1]2[c:2]([H,*:26])[c:3]([H,*:25])[c:4]([H,*:24])[c:5]([H,*:23])[c:6]12>>[O-:16][C:14](=[O:15])[C:1]1=[C:2]([H,*:26])[C:3]([H,*:25])=[C:4]([H,*:24])[C:5]([H,*:23])=[C:6]1[C:7]([H,*:18])=[C:8]1[C:13]([H,*:19])=[C:12]([H,*:20])[C:11](=[O:17])[C:10]([H,*:21])=[C:9]1[H,*:22]");
            smirks_l.Add("[H:1][#7:8]-1-[#6:7](-[*:9])=[#7:6]-[#6:5]-2=[#6:4]-1-[#6:12](=[S:13])-[#7:11]-[#6:10](-[*:2])=[#7:3]-2>>[H:1][#7:8]-1-[#6:7](-[*:9])=[#7:6]-[#6:5]-2=[#6:4]-1-[#7:3]=[#6:10](-[*:2])-[#7:11]-[#6:12]-2=[S:13]");
            Indigo i = new Indigo();
            i.setOption("ignore-stereochemistry-errors", "true");
            i.setOption("unique-dearomatization", "false");
            i.setOption("ignore-noncritical-query-features", "false");
            i.setOption("timeout", "60000");
            IndigoObject input = i.loadMolecule(molfile);    
            try
            {
                foreach (string smirks in smirks_l)
                {
                    IndigoObject reax = i.loadReactionSmarts(smirks);
                    Assert.IsTrue(!String.IsNullOrEmpty(reax.rxnfile()));
                    string smiles_before = input.canonicalSmiles();
                    i.transform(reax, input);
                    string smiles_after = input.canonicalSmiles();
                    Assert.IsTrue(!String.IsNullOrEmpty(input.molfile()));
                }
            }
            catch (IndigoException ex)
            {
                Console.WriteLine(ex);
            }
            Assert.IsTrue(!String.IsNullOrEmpty(input.molfile()));
        }

        
    }
}
