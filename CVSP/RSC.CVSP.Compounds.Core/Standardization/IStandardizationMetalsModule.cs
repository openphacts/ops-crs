using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoleculeObjects;
using com.ggasoftware.indigo;

namespace RSC.CVSP.Compounds
{
    public interface IStandardizationMetalsModule
    {
        string DisconnectMetalsInCarboxylates(string mol, bool adjustCharges);

        /// <summary>
        /// FDA rule 19.a: disconnects metals from N, O, and F
        /// </summary>
        /// <param name="mol"></param>
        /// <param name="adjustCharges"></param>
        /// <param name="Transformations"></param>
        /// <returns></returns>
        string DisconnectMetalsFromNOF(string mol, bool adjustCharges);

        /// <summary>
        /// neutralize free metals ions - set charge to zero
        /// </summary>
        /// <param name="molecule"></param>
        /// <param name="Transformations"></param>
        /// <returns></returns>
        string NeutralizeFreeMetals(string molecule);

        /// <summary>
        /// FDA rule 19.b: disconnects metals (excluding Hg, Ga, Ge, In, Sn, As, Tl, Pb, Bi, Po) from non metals (exluding N,O, and F)
        /// </summary>
        /// <param name="mol"></param>
        /// <param name="adjustCharges"></param>
        /// <param name="Transformations"></param>
        /// <returns></returns>
        string DisconnectMetalsFromNonMetals(string mol, bool adjustCharges);

        /// <summary>
        /// FDA rule 7.i.2: metal + carboxylic acid -> metal carboxylate
        /// </summary>
        /// <param name="mol"></param>
        /// <param name="adjustCharges"></param>
        /// <param name="Transformations"></param>
        /// <returns></returns>
        string IonizeFreeMetalWithCarboxylicAcid(string mol, bool adjustCharges);

        string RemoveFreeMetalCations(string molfile);

        /// <summary>
        /// remove free metals (no bonds, charged or neutral)
        /// </summary>
        /// <param name="molfile"></param>
        /// <param name="Transformations"></param>
        /// <returns></returns>
		string RemoveFreeMetals(string molfile);
    }
}
