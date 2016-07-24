using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.ggasoftware.indigo;
using System.IO;
using RSC.CVSP;

namespace RSC.CVSP.Compounds
{
    public class Reactor
    {
        static Indigo s_indigo;

        public Tuple<string,bool> Product(string ct, string SMIRKS)
        {
            //not use inchi for comparison before and after - inchi does some normalization that makes transform unnoticed
            try
            {
                IndigoObject m_reaction = null;
                if (!SMIRKS.Contains("#"))
                    m_reaction = s_indigo.loadQueryReaction(SMIRKS);
                else m_reaction = s_indigo.loadReactionSmarts(SMIRKS);

                IndigoObject input = s_indigo.loadMolecule(ct);
                string smiles_before = input.canonicalSmiles();
                s_indigo.transform(m_reaction, input);
                string smiles_after = input.canonicalSmiles();
                if (!smiles_before.Equals(smiles_after))
                    return new Tuple<string, bool>(input.molfile(), true);
                else return new Tuple<string, bool>(ct, false);

            }
            catch (IndigoException ex)
            {
                Console.WriteLine(ex);
                return new Tuple<string, bool>(ct, false);
            }
        }

        /// <summary>
        /// Returns tuple of string(molfile) and boolean indicating whether the smiles was changed by the transform.
        /// </summary>
        /// <param name="reactant"></param>
        /// 
        /// <returns></returns>
        public StandardizationResult Product(StandardizationResult reactantCt, string SMIRKS)
        {
            lock (s_indigo)
            {
                IndigoObject m_reaction = s_indigo.loadReactionSmarts(SMIRKS);
                IndigoObject input = s_indigo.loadMolecule(reactantCt.Standardized);
                string smiles_before = input.canonicalSmiles();
            //not use inchi for comparison before and after - inchi does some normalization that makes transform unnoticed
            try
            {
                s_indigo.transform(m_reaction, input);
                string smiles_after = input.canonicalSmiles();
                if (!smiles_before.Equals(smiles_after))
                        return reactantCt.AddTransformation(input.molfile(), SMIRKS);
                    else return reactantCt;
            
            }
            catch (IndigoException ex)
            {
                    Console.Error.WriteLine("SMILES BEFORE = " + smiles_before);
                    Console.Error.WriteLine(" problem with transform " + SMIRKS + ": " + ex);
                    // should add an Issue here but need to check.
                    return reactantCt;
                }
            }
            }

        public StandardizationResult Product(StandardizationResult reactantCt, IEnumerable<string> reactionSMIRKSs)
        {
            return (reactionSMIRKSs.Any())
                ? Product(Product(reactantCt, reactionSMIRKSs.First()), reactionSMIRKSs.Skip(1))
                : reactantCt;
        }

        public Reactor()
        {
            s_indigo = new Indigo();
            s_indigo.setOption("ignore-stereochemistry-errors", "true");
            s_indigo.setOption("unique-dearomatization", "false");
            s_indigo.setOption("ignore-noncritical-query-features", "false");
            s_indigo.setOption("timeout", "600000");
        }
    }
}
