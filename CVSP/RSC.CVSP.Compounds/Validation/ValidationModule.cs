using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Configuration;

using com.ggasoftware.indigo;
using MoleculeObjects;

using InChINet;
using RSC.Logging;

namespace RSC.CVSP.Compounds
{
    public class ValidationModule : IValidationModule
    {
        private Indigo s_indigo = new Indigo();
        private Indigo s_indigo_IgnoreIndigoErrors = new Indigo();

        private List<string> ForbiddenFreeElementsInMixtures = new List<string>()
        {
            {"Li"}, {"Na"}, {"K"}, {"Rb"}, {"Cs"}, {"Fr"}, 
			{"Mg"},{"Ca"},{"Sr"},{"Ba"},
			{"F"},{"Br"},{"Cl"},{"I"}
        };

        private readonly IValidationStereoModule validationStereoModule = null;

        public ValidationModule(IValidationStereoModule validationStereoModule)
        {
            if (validationStereoModule == null)
                throw new ArgumentNullException("validationStereoModule");

            this.validationStereoModule = validationStereoModule;

            s_indigo.setOption("ignore-stereochemistry-errors", false);
            s_indigo.setOption("ignore-noncritical-query-features", false);
            s_indigo.setOption("unique-dearomatization", true);
            s_indigo.setOption("timeout", "600");

            s_indigo_IgnoreIndigoErrors.setOption("ignore-stereochemistry-errors", true);
            s_indigo_IgnoreIndigoErrors.setOption("ignore-noncritical-query-features", true);
            s_indigo_IgnoreIndigoErrors.setOption("unique-dearomatization", false);
            s_indigo_IgnoreIndigoErrors.setOption("timeout", "600");
        }

        public IndigoObject TryLoadingSdfToIndigo(string molfile, ICollection<Issue> issues)
        {
            try
            {
                lock (s_indigo)
                    return s_indigo.loadMolecule(molfile);
            }
            catch (IndigoException ex)
            {
                Issue i = ex.ParseIndigoException();
                if (i != null && !issues.Contains(i))
                    issues.Add(i);
                try
                {
                    lock (s_indigo_IgnoreIndigoErrors)
                        return s_indigo_IgnoreIndigoErrors.loadMolecule(molfile);
                }
                catch
                {
                    return null;
                }

            }
        }

        /// <summary>
        /// Relies on InChI.
        /// </summary>
        public bool ContainsDuplicateMolecules(string mol, ICollection<Issue> issues = null)
        {
            if (issues == null)
                issues = new List<Issue>();
            lock (s_indigo_IgnoreIndigoErrors)
            {
                IndigoObject record = s_indigo_IgnoreIndigoErrors.loadMolecule(mol);
                //List<string> inchis = new List<string>();
                List<string> l_smiles = new List<string>();
                if (record.countComponents() > 1)
                {
                    foreach (IndigoObject component in record.iterateComponents())
                    {
                        IndigoObject comp = component.clone();
                        //string inchi_component = InChINet.InChIUtils.mol2InChI(comp.molfile(), InChIFlags.Standard);
                        string smiles = comp.canonicalSmiles();
                        if (!l_smiles.Contains(smiles))
                            l_smiles.Add(smiles);
                        else
                        {
                            issues.AddByName("validationDuplicateMoleculesTest");
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool IsAtomCountZero(string molfile, ICollection<Issue> issues)
        {
            //using loadQueryMolecule to accomodate brader set of records as we just need atom count
            IndigoObject obj = s_indigo_IgnoreIndigoErrors.loadQueryMolecule(molfile);
            if (obj.countAtoms() == 0)
            {
                issues.Add(new Issue { Code = "100.31" });
                return true;
            }
            return false;

        }

        public bool ContainsRelativeStereoInV2000(string molfile, ICollection<Issue> issues)
        {
            bool isStereoRelative = validationStereoModule.ContainsUpAndDownBondsWithNoChiralFlag(molfile);
            if (isStereoRelative)
            {
                issues.Add(new Issue { Code = "100.33" });
                return true;
            }
            return false;
        }

        public bool ContainsForbiddenFreeNeutralElementInMixture(string molfile, List<Issue> issues)
        {
            lock (s_indigo_IgnoreIndigoErrors)
            {
                IndigoObject mol_input = s_indigo_IgnoreIndigoErrors.loadMolecule(molfile);
                if (mol_input == null)
                    return false;
                foreach (IndigoObject atom in mol_input.iterateAtoms())
                {
                    if (ForbiddenFreeElementsInMixtures.Contains(atom.symbol()) && atom.charge() == 0)
                    {
                        //chck that it is a free element
                        foreach (IndigoObject ne in atom.iterateNeighbors())
                            return false;

                        issues.Add(new Issue
                        {
                            Code = "100.34",
                            AuxInfo = atom.symbol()
                        });

                        return true;
                    }
                }
            }
            return false;
        }

        public bool CanIndigoGenerateSmiles(string molfile, out string Smiles, ICollection<Issue> issues = null)
        {
            if (issues == null) issues = new List<Issue>();
            Smiles = String.Empty;
            lock (s_indigo_IgnoreIndigoErrors)
            {
                try
                {
                    IndigoObject mol_input = s_indigo_IgnoreIndigoErrors.loadMolecule(molfile);
                    if (mol_input == null) return false;
                    Smiles = mol_input.smiles();
                    if (String.IsNullOrEmpty(Smiles))
                    {
                        issues.Add(new Issue { Code = "100.26" });
                        return false;
                    }
                    return true;
                }
                catch (IndigoException ex)
                {
                    Issue i = ex.ParseIndigoException();
                    if (i.Code == "200.18")
                    {
                        issues.Add(i);
                    }
                    else
                    {
                        issues.Add(new Issue { Code = "100.26" });
                    }
                    return false;
                }
            }
        }

        public bool CanIndigoGenerateCanonicalSmiles(string molfile, out string canonicalSmiles, ICollection<Issue> issues = null)
        {
            if (issues == null) issues = new List<Issue>();
            canonicalSmiles = "";
            try
            {
                IndigoObject mol_input = s_indigo_IgnoreIndigoErrors.loadMolecule(molfile);
                if (mol_input == null) return false;

                canonicalSmiles = mol_input.canonicalSmiles();//may time out
                if (String.IsNullOrEmpty(canonicalSmiles))
                {
                    issues.Add(new Issue { Code = "100.27" });
                    return false;
                }
                return true;
            }
            catch (IndigoException ex)
            {
                Issue iss = ex.ParseIndigoException();
                if (iss.Code == "200.18")
                {
                    issues.Add(iss);
                }
                else issues.Add(new Issue { Code = "100.27" });
                return false;

            }
        }

        public bool DoesAtomCountExceedMaximum(string molfile, int maxAtomCountAllowed, ICollection<Issue> issues = null)
        {
            Console.WriteLine(maxAtomCountAllowed);
            if (issues == null) issues = new List<Issue>();
            lock (s_indigo_IgnoreIndigoErrors)
            {
                // using indigo because we may be loading in a V3000 file.
                IndigoObject o = s_indigo_IgnoreIndigoErrors.loadMolecule(molfile);
                if (o.countAtoms() > maxAtomCountAllowed)
                {
                    issues.Add(new Issue { Code = "100.42", Message = "maximum atom count = " + maxAtomCountAllowed });
                    return true;
                }
                return false;

            }
        }

        public bool IndigoIsAmbiguousHydrogenPresent(string molfile, ICollection<Issue> issues)
        {
            lock (s_indigo_IgnoreIndigoErrors)
            {
                IndigoObject mol_input = s_indigo_IgnoreIndigoErrors.loadMolecule(molfile);
                if (mol_input == null)
                    return false;
                string AmbiguousH = mol_input.checkAmbiguousH();
                if (!String.IsNullOrEmpty(AmbiguousH))
                {
                    issues.Add(new Issue { Code = "200.20" });
                    return true;
                }
            }
            return false;

        }

        public bool ContainsRadicals(string molfile, ICollection<Issue> issues)
        {
            int radicalCount = CountRadicalCenters(molfile, issues);
            if (radicalCount == 1)
            {
                issues.Add("100.54");
                return true;
            }
            else if (radicalCount == 2)
            {
                issues.Add("100.55");
                return true;
            }
            else if (radicalCount > 2)
            {
                issues.Add("100.56");
                return true;
            }
            return false;
        }

        public bool ContainsEitherBond(string molfile)
        {
            GenericMolecule gm = MoleculeFactory.FromMolV2000(molfile);
            foreach (KeyValuePair<int, MoleculeObjects.Bond> kp in gm.IndexedBonds)
            {
                MoleculeObjects.Bond bd = kp.Value;
                if (bd.order == BondOrder.Double && bd.bondStereo == BondStereo.CisTransEither)
                    return true;

            }
            return false;
        }

        public bool ContainsNotKekulizedAromaticRings(string mol)
        {
            lock (s_indigo_IgnoreIndigoErrors)
            {
                IndigoObject obj = s_indigo_IgnoreIndigoErrors.loadMolecule(mol);
                foreach (IndigoObject bond in obj.iterateBonds())
                    if (bond.bondOrder() == 4)
                        return true;
            }
            return false;

        }

        public bool ContainsSmarts(string sdf, string smarts)
        {
            try
            {
                lock (s_indigo_IgnoreIndigoErrors)
                {
                    IndigoObject iq = s_indigo_IgnoreIndigoErrors.loadSmarts(smarts);
                    IndigoObject obj = s_indigo_IgnoreIndigoErrors.loadMolecule(sdf);
                    IndigoObject q_obj = s_indigo_IgnoreIndigoErrors.substructureMatcher(obj).match(iq);

                    if (q_obj != null)
                        return true;
                }
                return false;

            }
            catch (IndigoException ex)
            {
                throw new Exception("Problem in isSmartsPresent method: " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        public int CountRadicalCenters(string molfile, ICollection<Issue> issues)
        {
            int count = 0;
            lock (s_indigo_IgnoreIndigoErrors)
            {
                try
                {
                    IndigoObject obj = s_indigo_IgnoreIndigoErrors.loadMolecule(molfile);
                    foreach (IndigoObject atom in obj.iterateAtoms())
                        if (atom.radical() > 0)
                            count++;
                }
                catch (Exception ex)
                {
                    Issue iss = new Issue
                    {
                        Code = "100.30",
                        AuxInfo = ex.StackTrace,
                        Message = ex.Message
                    };
                    if (!issues.Contains(iss))
                        issues.Add(iss);
                    return 0;
                }
            }
            return count;
        }

        /// <summary>
        /// Relies on InChI.
        /// </summary>
        public bool ContainsOnlyMultipleInstancesOfSameMolecules(string molfile, ICollection<Issue> issues)
        {
            List<string> inchis = new List<string>();
            bool containDuplicates = false;
            lock (s_indigo_IgnoreIndigoErrors)
            {
                IndigoObject mol_input = s_indigo_IgnoreIndigoErrors.loadMolecule(molfile);
                if (mol_input == null || mol_input.countComponents() == 1)
                    return false;


                foreach (IndigoObject component in mol_input.iterateComponents())
                {
                    IndigoObject comp = component.clone();
                    string inchi_component;
                    try
                    {
                        inchi_component = InChIUtils.mol2InChI(comp.molfile(), InChIFlags.Standard);
                        if (inchis.Contains(inchi_component))
                            containDuplicates = true;
                        else
                            inchis.Add(inchi_component);
                    }
                    catch (Exception ex)
                    {
                        issues.Add(new Issue
                        {
                            Code = "100.29",
                            Message = ex.Message,
                            AuxInfo = ex.StackTrace
                        });
                    }
                }
            }
            if (containDuplicates && inchis.Count == 1)
            {
                issues.Add(new Issue { Code = "100.37" });
                return true;
            }
            return false;
        }

        public bool IsV3000(string molfile)
        {
            if ((from line in molfile.SplitOnNewLines() where line.Contains("M ") && line.Contains(" V30") select line).Any())
                return true;
            return false;
        }

        public bool ContainsUnevenLengthBonds(string molfile, ICollection<Issue> issues = null)
        {
            if (issues == null) issues = new List<Issue>();
            if (MoleculeFactory.FromMolV2000(molfile).HasUnevenLengthBonds())
            {
                issues.Add(new Issue { Code = "100.25" }); return true;
            }
            return false;
        }

        public bool IsMolfileFormatValid(string sdf_or_molfile_input, out string fixedSdf, bool trytofix, ICollection<Issue> issues = null)
        {
            if (issues == null) issues = new List<Issue>();
            bool isValid = true;

            fixedSdf = String.Empty;

            if (!(from line in sdf_or_molfile_input.SplitOnNewLines() where line.Contains("M ") && line.Contains(" END") select line).Any())
            {
                issues.Add(new Issue { Code = "300.5" });
                isValid = false;
            }

            string[] lines = sdf_or_molfile_input.Split(new string[] { "\r\n", "\n\r", "\r", "\n" }, StringSplitOptions.None);
            Dictionary<int, string> atoms = new Dictionary<int, string>();
            Dictionary<int, string> bonds = new Dictionary<int, string>();
            List<string> header_lines = new List<string>();
            string count_line = null;
            bool MTagfound = false;
            foreach (string line in lines)
            {
                string new_line = line;
                if (MTagfound ||
                    (!String.IsNullOrEmpty(count_line) && line.Length > 3 && line.Substring(0, 2).Contains("M ")))
                {
                    MTagfound = true;
                    fixedSdf += new_line + Environment.NewLine;
                    continue;
                }

                if (!String.IsNullOrEmpty(count_line) && line.Length > 1 &&
                    (line.Substring(0, 2).Contains("A ") || line.Substring(0, 2).Contains("R ") || line.Substring(0, 2).Contains("V ") || line.Substring(0, 2).Contains("G ") || line.Substring(0, 2).Contains("S ")))
                {
                    MTagfound = true;
                    fixedSdf += new_line + Environment.NewLine;
                    continue;
                }

                bool lineIdentified = false;
                if (!String.IsNullOrEmpty(count_line) && bonds.Count() == 0 && line.Length > 29)
                {
                    double x, y, z;
                    bool xParsed = Double.TryParse(line.Substring(0, 10), out x);
                    bool yParsed = Double.TryParse(line.Substring(10, 10), out y);
                    bool zParsed = Double.TryParse(line.Substring(20, 10), out z);
                    if (bonds.Count() == 0 && xParsed && yParsed && zParsed)
                    {
                        string atomSymbol = line.Substring(31, 2).TrimStart(' ').TrimEnd(' ');
                        if (!MoleculeObjects.AtomicProperties.AtomicSymbolsLowCaseList.Contains(atomSymbol.ToLower()))
                        {
                            isValid = false;
                            issues.Add(new Issue
                            {
                                Code = "100.4",
                                AuxInfo = atomSymbol
                            });
                        }

                        if (!trytofix)
                            atoms.Add(atoms.Count(), line);
                        else if (line.Substring(0, 10).Contains("E") || line.Substring(10, 10).Contains("E") || line.Substring(20, 10).Contains("E"))//scientific notation
                        {
                            string s_x = String.Format("{0:0.0000}", x);
                            while (s_x.Length < 10)
                                s_x = " " + s_x;
                            new_line = new_line.Replace(new_line.Substring(0, 10), s_x);

                            string s_y = String.Format("{0:0.0000}", y);
                            while (s_y.Length < 10)
                                s_y = " " + s_y;
                            new_line = new_line.Replace(new_line.Substring(10, 10), s_y);

                            string s_z = String.Format("{0:0.0000}", z);
                            while (s_z.Length < 10)
                                s_z = " " + s_z;
                            new_line = new_line.Replace(new_line.Substring(20, 10), s_z);

                            atoms.Add(atoms.Count(), new_line);

                            issues.Add(new Issue
                            {
                                Code = "300.7",
                                AuxInfo = line
                            });
                        }
                        else
                            atoms.Add(atoms.Count(), line);
                        lineIdentified = true;
                    }
                    else
                    {
                        isValid = false;
                        issues.Add(new Issue { Code = "300.6", AuxInfo = line });
                    }
                }


                if (!lineIdentified && line.Length > 11)
                {
                    int atom1, atom2, bondOrder, bondStereo;
                    bool atom1parsed = Int32.TryParse(line.Substring(0, 3), out atom1);
                    bool atom2parsed = Int32.TryParse(line.Substring(3, 3), out atom2);
                    bool bondOrderParsed = Int32.TryParse(line.Substring(6, 3), out bondOrder);
                    bool bondStereoParsed = Int32.TryParse(line.Substring(9, 3), out bondStereo);
                    if (atom1parsed && atom2parsed && bondOrderParsed && bondStereoParsed)
                    {
                        if (atoms.Count() == 0 && bonds.Count() == 0)
                        {
                            count_line = line;
                            lineIdentified = true;
                        }
                        else if (atoms.Count() > 0)
                        {
                            bonds.Add(bonds.Count(), line);
                            lineIdentified = true;
                        }
                    }
                }

                if (!lineIdentified && String.IsNullOrEmpty(count_line) && atoms.Count() == 0 && bonds.Count() == 0)
                {
                    header_lines.Add(line);
                    lineIdentified = true;
                }
                fixedSdf += new_line + Environment.NewLine;
            }


            if (header_lines.Count < 3)
            {
                isValid = false;
                issues.Add(new Issue { Code = "300.4" });
            }

            if (trytofix)
            {
                int i = header_lines.Count;
                while (i < 3)
                {
                    fixedSdf = Environment.NewLine + fixedSdf;
                    i++;
                }
            }


            return isValid;

        }

        public bool TryGeneratingStdInChI(string molfile, out string std_inchi, ICollection<Issue> issues = null)
        {
            if (issues == null)
            {
                issues = new List<Issue>();
            }
            std_inchi = String.Empty;
            try
            {
                std_inchi = InChINet.InChIUtils.mol2InChI(molfile, InChINet.InChIFlags.Standard);
            }
            catch (Exception ex)
            {
                issues.Add("100.29", ex.StackTrace, ex.Message);
                return false;
            }
            return true;
        }

        public bool ContainsMixtureWithFreeCO(string molfile, ICollection<Issue> issues = null)
        {
            if (issues == null)
                issues = new List<Issue>();
            if (ContainsSmarts(molfile, "[*].[CD1]=,#[OD1]"))//only C=O (or C#O) in multifragment record
            {
                issues.Add(new Issue { Code = "100.53" });
                return true;
            }
            return false;
        }

        public bool HasUniqueDearomatization(string molfile, ICollection<Issue> issues = null)
        {
            if (issues == null)
                issues = new List<Issue>();
            try{
                var molecule = s_indigo.loadMolecule(molfile);
                molecule.dearomatize();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("dearomatization"))
                {
                    issues.Add(e.ParseIndigoException());
                    return false;
                }
            }
            return true;
        }

        public bool ValidateValenceChargeRadicalAtAtoms(string molfile, ICollection<Issue> issues = null)
        {
            if (issues == null)
                issues = new List<Issue>();
            bool success = true;
            lock (s_indigo_IgnoreIndigoErrors)
            {
                IndigoObject cvsp_mol_input = s_indigo_IgnoreIndigoErrors.loadMolecule(molfile);
                //dearomatize prior to valence validation
                cvsp_mol_input.dearomatize();

                //check that there are no aromatic bond
                if (ContainsNotKekulizedAromaticRings(cvsp_mol_input.molfile()))
                    return false;

                //tuple of symbol, bond count, charge, and radical count
                List<Tuple<string, int, int, int>> atoms = new List<Tuple<string, int, int, int>>();
                foreach (IndigoObject atom in cvsp_mol_input.iterateAtoms())
                {
                    //deal with bad atom symbol or pseudo-atoms
                    if (!AtomicProperties.AtomicSymbolsLowCaseList.Contains(atom.symbol().ToLower()) || atom.isPseudoatom())
                        continue;

                    int bondCount = 0;
                    foreach (IndigoObject neighbor in atom.iterateNeighbors())
                        bondCount += neighbor.bond().bondOrder();
                    int eff_valence = 0;
                    int radical_count = 0, charge = 0;
                    if (atom.radicalElectrons().HasValue)
                        radical_count = atom.radicalElectrons().Value;
                    if (atom.charge().HasValue)
                        charge = atom.charge().Value;
                    eff_valence = bondCount + radical_count - charge;
                    if (
                        (AtomValenceProperties.MaximumValences.ContainsKey(atom.symbol()) && eff_valence > AtomValenceProperties.MaximumValences[atom.symbol()])
                        ||
                        (AtomValenceProperties.ForbiddenValences.ContainsKey(atom.symbol()) && AtomValenceProperties.ForbiddenValences[atom.symbol()].Contains(eff_valence))
                        )
                    {
                        issues.Add(new Issue
                        {
                            Code = "100.35",
                            Message = atom.symbol() + " has " + bondCount + " bond(s), charge of " + charge + ", " + radical_count + " radical(s)"
                        });
                        success = false;
                    }
                }
            }
            return success;
        }

        public AnalysedInChI AnalyzeStereo(string molfile, ICollection<Issue> issues = null)
        {
            if (String.IsNullOrEmpty(molfile))
                return null;
            string custom_inchi = InChIUtils.mol2InChI(molfile, InChIFlags.CRS);

            AnalysedInChI AnalyzedInChI = new AnalysedInChI(custom_inchi);

            if (issues == null)
                return AnalyzedInChI;

            if (AnalyzedInChI.UnknownStereoCenters > 0)
            {
                issues.Add(new Issue() { Code = "100.57" });
            }

            //more fine tuned methods are used in ValidationModulesStereo: ContainsCrossedDoubleBond and ContainsDoubleBondWithAdjacentWavyBond
            //if (AnalyzedInChI.UnknownStereoBond > 0)
            //{
            //	Issue i = new Issue()
            //	{
            //		Code = "100.40"
            //	};
            //	if (!issues.Contains(i))
            //		issues.Add(i);
            //}

            if (AnalyzedInChI.isEnantiomer)
            {
                issues.Add(new Issue() { Code = "100.58" });
            }
            if (AnalyzedInChI.isCompletelyUndefinedMixture)
            {
                issues.Add(new Issue() { Code = "100.59" });
            }
            if (AnalyzedInChI.isEpimer)
            {
                issues.Add(new Issue() { Code = "100.61" });
            }
            if (AnalyzedInChI.isPartiallyUndefinedMixture)
            {
                issues.Add(new Issue() { Code = "100.62" });
            }

            return AnalyzedInChI;
        }

        public bool IsOverallSystemCharged(string molfile, ICollection<Issue> issues = null)
        {
            if (issues == null) issues = new List<Issue>();
            int charge = 0;
            IndigoObject obj = s_indigo_IgnoreIndigoErrors.loadMolecule(molfile);
            foreach (IndigoObject atom in obj.iterateAtoms())
            {
                int? atomCharge = atom.charge();
                if (atomCharge.HasValue)
                    charge += atomCharge.Value;
            }
            if (charge == 0) return false;
            issues.Add(new Issue() { Code = "100.32" });
            return true;
        }

        /// <summary>
        /// Compares the molecule implied by a SMILES string to an InChI.
        /// </summary>
        public bool MatchingSMILES(string refInChI, string SMILES)
        {
            string impliedMol = s_indigo_IgnoreIndigoErrors.loadMolecule(SMILES).molfile();
            string comparisonInChI = InChIUtils.mol2InChI(impliedMol, InChIFlags.Standard);
            return comparisonInChI == refInChI;
        }

        /// <summary>
        /// for checking OPS Smiles query and returning Canonical smiles
        /// </summary>
        /// <param name="smiles">any smiles</param>
        /// <param name="issues">Issues object with Errors, Wanrings</param>
        /// <returns>Indigo canonical smiles (smiles converted to molfile, dearomatized, and then Indigo canonical smiles is generated</returns>
        public string ValidateSMILES(string smiles, ICollection<Issue> issues, bool ignoreIndigoStereoErrorException, bool ignoreIndigoUniqueDearomatizationException, bool deAromatizeReturnedSmiles)
        {
            try
            {
                Indigo i = new Indigo();

                if (ignoreIndigoStereoErrorException)
                    i.setOption("ignore-stereochemistry-errors", true);
                else i.setOption("ignore-stereochemistry-errors", false);
                i.setOption("ignore-noncritical-query-features", false);
                if (ignoreIndigoUniqueDearomatizationException)
                    i.setOption("unique-dearomatization", false);
                else i.setOption("unique-dearomatization", true);

                i.setOption("timeout", "60000");

                IndigoObject obj = i.loadMolecule(smiles);
                if (obj.countAtoms() < 100)
                    obj.layout();
                /* 2015-10-20: the way we do validation has changed since we last had this code working.
                var validation = Validation.Validate(obj.molfile() + Environment.NewLine + Environment.NewLine + "$$$$");
                validation.Issues = validation.Issues.Where(issue => issue.Code != "100.24").ToList();//since layout was done by us

                issues.AddRange(validation.Issues);
                */
                if (deAromatizeReturnedSmiles)
                    obj.dearomatize();
                return obj.smiles();
            }
            catch (IndigoException ex)
            {
                Issue i = ex.ParseIndigoException();
                if (!issues.Contains(i))
                    issues.Add(i);
            }

            return null;
        }

        /// <summary>
        /// validate that supplied SMILES in SD file match the structure
        /// Perform SMILES validation only if there are any supplied.
        /// perform smiles validation only if number of atoms < 200 because otherwise layout takes infinite time 
        /// (impossible to run smiles validation without layout as indigoobject loaded from smiles needs coordinates)
        /// </summary>
        /// <param name="molfile"></param>
        /// <param name="SMILESs"></param>
        /// <param name="issues"></param>
        /// <returns></returns>
        public bool ValidateSMILES(string molfile, string stdInChI, List<string> SMILESs, ICollection<Issue> issues)
        {
            if (!SMILESs.Any()) return true;
            try
            {
                GenericMolecule gm = MoleculeFactory.FromMolV2000(molfile);
                if (gm.IndexedAtoms.Count > Convert.ToInt32(ConfigurationManager.AppSettings["MaxAtomLimit_4_SmilesValidation2Run"]))
                {
                    issues.Add(new Issue
                    {
                        Code = "100.65",
                        Message = String.Join(", ", SMILESs)
                    });
                    return false;
                }
                List<string> InvalidSmiles = SMILESs.Where(s => !MatchingSMILES(stdInChI, s)).ToList();
                if (InvalidSmiles.Any())
                {
                    issues.Add(new Issue() { Code = "100.63", Message = String.Join(", ", InvalidSmiles) });
                    return false;
                }
                else
                {
                    issues.Add(new Issue() { Code = "100.64" });
                    return true;
                }
            }
            catch
            {
                issues.Add(new Issue() { Code = "300.2", Message = String.Join(", ", SMILESs) });

                issues.Add(new Issue
                {
                    Code = "100.65",
                    Message = String.Join(", ", SMILESs)
                });
                return false;
            }
        }

        public string ConvertV3000ToV2000(string sdf, ICollection<Issue> issues = null)
        {
            if (issues == null)
                issues = new List<Issue>();
            try
            {
                Indigo v3000conversionIndigo = new Indigo();
                v3000conversionIndigo.setOption("ignore-stereochemistry-errors", "true");
                v3000conversionIndigo.setOption("unique-dearomatization", "false");
                v3000conversionIndigo.setOption("ignore-noncritical-query-features", "true");
                v3000conversionIndigo.setOption("timeout", "600000");
                v3000conversionIndigo.setOption("molfile-saving-mode", "auto");
                IndigoObject obj = v3000conversionIndigo.loadMolecule(sdf);
                return obj.molfile();
            }
            catch (IndigoException ex)
            {
                Issue i = new Issue()
                {
                    Code = "400.12",
                    Message = ex.Message,
                    AuxInfo = ex.StackTrace,
                };
                issues.Add(i);
                return null;
            }
        }
    }
}
