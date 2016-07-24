using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoleculeObjects;
using com.ggasoftware.indigo;

namespace RSC.CVSP.Compounds
{
    public class StandardizationMetalsModule : IStandardizationMetalsModule
    {
        private static List<string> Metals;
        private static List<string> MetalsDisconnectedFromNonMetals;
        private static Indigo s_indigo;

		public StandardizationMetalsModule()
        {
            Metals = new List<string>();
            Metals.AddRange(AtomicProperties.Group1Atoms);
            Metals.AddRange(AtomicProperties.Group2Atoms);
            Metals.AddRange(AtomicProperties.TransitionMetalsLessHg);
            Metals.AddRange(AtomicProperties.Lanthanides);
            Metals.AddRange(AtomicProperties.Actinides);
            Metals.AddRange(AtomicProperties.pBlockMetals);

            //metals that need to be disconnected form non metals
            MetalsDisconnectedFromNonMetals = new List<string>();
            MetalsDisconnectedFromNonMetals.AddRange(AtomicProperties.Group1Atoms);
            MetalsDisconnectedFromNonMetals.AddRange(AtomicProperties.Group2Atoms);
            MetalsDisconnectedFromNonMetals.AddRange(AtomicProperties.TransitionMetalsLessHg);
            MetalsDisconnectedFromNonMetals.AddRange(AtomicProperties.Lanthanides);
            MetalsDisconnectedFromNonMetals.AddRange(AtomicProperties.Actinides);
            MetalsDisconnectedFromNonMetals.AddRange(AtomicProperties.pBlockMetals);
            MetalsDisconnectedFromNonMetals.Remove("Hg"); Metals.Remove("Ga"); Metals.Remove("Ge"); Metals.Remove("In"); Metals.Remove("Sn"); Metals.Remove("As"); Metals.Remove("Tl");
            MetalsDisconnectedFromNonMetals.Remove("Pb"); Metals.Remove("Bi");
            MetalsDisconnectedFromNonMetals.Remove("Po");

            s_indigo = new Indigo();
            s_indigo.setOption("ignore-stereochemistry-errors", "true");
            s_indigo.setOption("ignore-noncritical-query-features", "true");
        }

        public string DisconnectMetalsInCarboxylates(string mol, bool adjustCharges)
        {
            IndigoObject molecule = s_indigo.loadMolecule(mol);
            Dictionary<int, IndigoObject> bonds2Remove = new Dictionary<int, IndigoObject>();
            foreach (IndigoObject atom in molecule.iterateAtoms())
            {
                if (Metals.Contains(atom.symbol()))
                {
                    foreach (IndigoObject neighborAtom in atom.iterateNeighbors())
                    {
                        IndigoObject bond_O_Metal = neighborAtom.bond();

                        //if neighbor is oxygen (C~Metal) - disconnect
                        if (neighborAtom.symbol().Contains("O"))
                        {
                            foreach (IndigoObject neighborOfneighbor in neighborAtom.iterateNeighbors())
                            {
                                if (neighborOfneighbor.symbol().Contains("C"))
                                {
                                    foreach (IndigoObject neighborOfneighborOfneighbor in neighborOfneighbor.iterateNeighbors())
                                    {
                                        if (neighborOfneighborOfneighbor.symbol().Contains("O") && neighborOfneighborOfneighbor.bond().bondOrder() == 2)
                                        {
                                            if (adjustCharges)
                                            {
                                                atom.setCharge((int)atom.charge() + 1);
                                                neighborAtom.setCharge((int)neighborAtom.charge() - 1);
                                            }
                                            if (!bonds2Remove.ContainsKey(bond_O_Metal.index())) bonds2Remove[bond_O_Metal.index()] = bond_O_Metal;
                                        }
                                    }
                                }
                            }
                        }
                    }

                }

            }

            foreach (KeyValuePair<int, IndigoObject> kp in bonds2Remove)
                kp.Value.remove();

			return molecule.molfile();
        }

        /// <summary>
        /// FDA rule 19.a: disconnects metals from N, O, and F
        /// </summary>
        /// <param name="mol"></param>
        /// <param name="adjustCharges"></param>
        /// <param name="Transformations"></param>
        /// <returns></returns>
        public string DisconnectMetalsFromNOF(string mol, bool adjustCharges)
        {
            IndigoObject substance = s_indigo.loadMolecule(mol);
            Dictionary<int, IndigoObject> bonds2Remove = new Dictionary<int, IndigoObject>();
            IndigoObject new_obj = null;
            foreach (IndigoObject component in substance.iterateComponents())
            {
                IndigoObject molecule = component.clone();
                bool isOrganic = false;
                foreach (IndigoObject atom in molecule.iterateAtoms())
                {
                    if (Metals.Contains(atom.symbol()) && atom.radicalElectrons() == 0)
                    {

                        foreach (IndigoObject neighborAtom in atom.iterateNeighbors())
                        {
                            //determine that there is a carbon neighbor
                            foreach (IndigoObject nOfN in neighborAtom.iterateNeighbors())
                            {
                                if (nOfN.symbol().Equals("C"))
                                {
                                    isOrganic = true;
                                    break;
                                }
                            }
                        }
                        if (isOrganic)
                        {
                            foreach (IndigoObject neighborAtom in atom.iterateNeighbors())
                            {
                                IndigoObject bond_O_Metal = neighborAtom.bond();
                                if (neighborAtom.symbol().Equals("O") || neighborAtom.symbol().Equals("N") || neighborAtom.symbol().Equals("F"))
                                {
                                    foreach (IndigoObject NOF_neighborAtom in neighborAtom.iterateNeighbors())
                                    {
                                        if (NOF_neighborAtom.symbol().Equals("C"))
                                        {
                                            if (adjustCharges)
                                            {
                                                atom.setCharge((int)atom.charge() + neighborAtom.bond().bondOrder());
                                                neighborAtom.setCharge((int)neighborAtom.charge() - neighborAtom.bond().bondOrder());

                                            }
                                            if (!bonds2Remove.ContainsKey(bond_O_Metal.index())) bonds2Remove[bond_O_Metal.index()] = bond_O_Metal;
                                            break;
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
                foreach (KeyValuePair<int, IndigoObject> kp in bonds2Remove)
                    kp.Value.remove();
                if (new_obj == null)
                    new_obj = molecule;
                else
                    new_obj.merge(molecule);
            }
            return new_obj.molfile();
        }

        /// <summary>
        /// neutralize free metals ions - set charge to zero
        /// </summary>
        public string NeutralizeFreeMetals(string mol)
        {
            Indigo indigo = new Indigo();
            indigo.setOption("ignore-stereochemistry-errors", "true");
            indigo.setOption("ignore-noncritical-query-features", "true");
            IndigoObject obj = indigo.loadMolecule(mol);
            foreach (IndigoObject atom in obj.iterateAtoms())
            {
                if (atom.degree() == 0 && Metals.Contains(atom.symbol()))
                    atom.setCharge(0);
            }
            return obj.molfile();
        }

        /// <summary>
        /// FDA rule 19.b: disconnects metals (excluding Hg, Ga, Ge, In, Sn, As, Tl, Pb, Bi, Po) from non metals (exluding N,O, and F)
        /// </summary>
        public string DisconnectMetalsFromNonMetals(string mol, bool adjustCharges)
        {
            Indigo indigo = new Indigo();
            indigo.setOption("ignore-stereochemistry-errors", "true");
            indigo.setOption("ignore-noncritical-query-features", "true");
            IndigoObject substance = indigo.loadMolecule(mol);

            Dictionary<int, IndigoObject> bonds2Remove = new Dictionary<int, IndigoObject>();
            IndigoObject new_obj = null;
            foreach (IndigoObject component in substance.iterateComponents())
            {
                IndigoObject molecule = component.clone();
                foreach (IndigoObject atom in molecule.iterateAtoms())
                {
                    if (MetalsDisconnectedFromNonMetals.Contains(atom.symbol()))
                    {

                        foreach (IndigoObject neighborAtom in atom.iterateNeighbors())
                        {
                            //determine that there is a carbon neighbor
                            foreach (IndigoObject nOfN in neighborAtom.iterateNeighbors())
                            {
                                if (nOfN.symbol().Equals("C") && AtomicProperties.NonMetalsLessOFN.Contains(neighborAtom.symbol()))
                                {
                                    //is organic
                                    if (adjustCharges)
                                    {
                                        atom.setCharge((int)atom.charge() + neighborAtom.bond().bondOrder());
                                        neighborAtom.setCharge((int)neighborAtom.charge() - neighborAtom.bond().bondOrder());

                                    }
                                    if (!bonds2Remove.ContainsKey(neighborAtom.bond().index())) bonds2Remove[neighborAtom.bond().index()] = neighborAtom.bond();
                                    break;
                                }
                            }
                        }
                    }
                }
                foreach (KeyValuePair<int, IndigoObject> kp in bonds2Remove)
                    kp.Value.remove();
                if (new_obj == null)
                    new_obj = molecule;
                else
                    new_obj.merge(molecule);

            }

			return new_obj.molfile();
        }

        /// <summary>
        /// FDA rule 7.i.2: metal + carboxylic acid -> metal carboxylate
        /// </summary>
        /// <param name="mol"></param>
        /// <param name="adjustCharges"></param>
        /// <param name="Transformations"></param>
        /// <returns></returns>
        public string IonizeFreeMetalWithCarboxylicAcid(string mol, bool adjustCharges)
        {
            IndigoObject substance = s_indigo.loadMolecule(mol);
            List<string> Metals = new List<string>();
            Metals.AddRange(AtomicProperties.Group1Atoms);
            List<IndigoObject> freeMetalsFound = new List<IndigoObject>();

            //count neutral free metals
            foreach (IndigoObject atom in substance.iterateAtoms())
            {
                if (Metals.Contains(atom.symbol()) && atom.charge() == 0 && atom.degree() == 0)
                    freeMetalsFound.Add(atom);
            }
            //count CO2H groups
            IndigoObject co2_obj = s_indigo.loadSmarts("[O]=[C;X3][OH]");
            int co2h_count = s_indigo.substructureMatcher(substance).countMatches(co2_obj);


            //of more than one metal or carboxylic group then return inial mol
            if (freeMetalsFound.Count != 1 || co2h_count != 1)
                return mol;

            foreach (IndigoObject atom in substance.iterateAtoms())
            {
                if (atom.symbol() == "C")
                {
                    foreach (IndigoObject neighb in atom.iterateNeighbors())
                    {
                        if (neighb.symbol() == "O" && neighb.bond().bondOrder() == 2)
                        {
                            //carboxy found
                            foreach (IndigoObject atom_other_neighb in atom.iterateNeighbors())
                            {
                                if (atom_other_neighb.symbol() == "O" && atom_other_neighb.bond().bondOrder() == 1)
                                {
                                    //check that hydroxy oxygen doesn't have explicit H. If it does - remove it
                                    IndigoObject ExplicitH = null;
                                    foreach (IndigoObject hydrogen in atom_other_neighb.iterateNeighbors())
                                    {
                                        if (hydrogen.symbol() == "H")
                                            ExplicitH = hydrogen;
                                    }
                                    if (ExplicitH != null)
                                        ExplicitH.remove();
                                    //make charge of metal -1
                                    atom_other_neighb.setCharge(-1);
                                    freeMetalsFound[0].setCharge(+1);
                                    return substance.molfile();

                                }
                            }
                        }
                    }

                }
            }
            return mol;
        }

        public string RemoveFreeMetalCations(string molfile)
        {
            IndigoObject input_obj = s_indigo.loadMolecule(molfile);
            foreach (IndigoObject atom in input_obj.iterateAtoms())
            {
                if (atom.degree() == 0 && atom.charge() > 0 && Metals.Contains(atom.symbol()))
                    atom.remove();
            }
            return input_obj.molfile();

        }

        /// <summary>
        /// remove free metals (no bonds, charged or neutral)
        /// </summary>
        /// <param name="molfile"></param>
        /// <param name="Transformations"></param>
        /// <returns></returns>
        public string RemoveFreeMetals(string molfile)
        {
            IndigoObject input_obj = s_indigo.loadMolecule(molfile);
            foreach (IndigoObject atom in input_obj.iterateAtoms())
            {
                if (atom.degree() == 0 && Metals.Contains(atom.symbol()))
                    atom.remove();
            }
            return input_obj.molfile();
        }
    }
}
