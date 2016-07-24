using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Text;
using System.Configuration;
using MoleculeObjects;
using InChINet;
using RSC.Collections;

using com.ggasoftware.indigo;

using OpenEyeNet;
using RSC.CVSP;
using RSC.Logging;

namespace RSC.CVSP.Compounds
{

	/// <summary>
	/// Pre-canned collection of SMIRKS normalizations and (in due course) other things.
	/// </summary>
	public class Standardization
	{
		public enum StandardizationRuleType { smirks, module }

		private IAcidity acidity;
		private List<Tuple<StandardizationRuleType, string>> StandardizationModuleCollection = new List<Tuple<StandardizationRuleType, string>>();

		private IStandardizationModule standardizationModule = null;
		private IStandardizationChargesModule standardizationChargesModule = null;
		private IStandardizationFragmentsModule standardizationFragmentsModule = null;
		private IStandardizationStereoModule standardizationStereoModule = null;
		private IStandardizationMetalsModule standardizationMetalsModule = null;

		private Indigo s_indigo = new Indigo();
        private Reactor s_reactor = new Reactor();

		private int LayoutAtomLimit = 200;
		private int StandardizationAtomLimit = 1000;
		private int TautomerCanonicalizationAtomLimit = 100;

		public Standardization(
			IAcidity acidity, 
			IStandardizationModule standardizationModule, 
			IStandardizationChargesModule standardizationChargesModule,
			IStandardizationFragmentsModule standardizationFragmentsModule,
			IStandardizationStereoModule standardizationStereoModule,
			IStandardizationMetalsModule standardizationMetalsModule)
		{
			//	TODO: use predefined standardization rules. In the future it should be implemented as independent IStandardizationRules module
			string StdRulesXMLContent = RSC.CVSP.Compounds.Properties.Resources.standardization;

			if (acidity == null)
				throw new ArgumentNullException("acidity");

			if (standardizationModule == null)
				throw new ArgumentNullException("standardizationModule");

			if (standardizationChargesModule == null)
				throw new ArgumentNullException("standardizationChargesModule");

			if (standardizationFragmentsModule == null)
				throw new ArgumentNullException("standardizationFragmentsModule");

			if (standardizationStereoModule == null)
				throw new ArgumentNullException("standardizationStereoModule");

			if (standardizationMetalsModule == null)
				throw new ArgumentNullException("standardizationMetalsModule");

			this.acidity = acidity;
			this.standardizationModule = standardizationModule;
			this.standardizationChargesModule = standardizationChargesModule;
			this.standardizationFragmentsModule = standardizationFragmentsModule;
			this.standardizationStereoModule = standardizationStereoModule;
			this.standardizationMetalsModule = standardizationMetalsModule;

			s_indigo.setOption("ignore-stereochemistry-errors", "true");
			s_indigo.setOption("unique-dearomatization", "false");
			s_indigo.setOption("ignore-noncritical-query-features", "false");
			s_indigo.setOption("timeout", "60000");

			LayoutAtomLimit = ConfigurationManager.AppSettings.GetInt("MaxAtomLimit_4_Layout", 200);
			StandardizationAtomLimit = ConfigurationManager.AppSettings.GetInt("MaxAtomLimit_4_Standardization2Run", 1000);
			TautomerCanonicalizationAtomLimit = ConfigurationManager.AppSettings.GetInt("MaxAtomLimit_4_TautomerCanonicalization", 100);

			StandardizationModuleCollection.Clear();
			XElement x = XElement.Parse(StdRulesXMLContent, LoadOptions.None);

			var rules = x.XPathSelectElements("//rule");

			foreach (var rule in rules)
			{
				//string rule_id = rule.Attribute("id").Value;
				string type = rule.Attribute("type").Value;
				StandardizationRuleType rt;
				bool res = Enum.TryParse(type.ToLower(), out rt);
				string value = rule.Attribute("value").Value;
				if (!res)
					continue;
				//Console.WriteLine("Std rule added:" + rule_id + ": " + smirks_abbr);
				if (rt == StandardizationRuleType.module)
				{
					StandardizationModuleCollection.Add(new Tuple<StandardizationRuleType, string>(rt, value));
				}
				else if (rt == StandardizationRuleType.smirks)
				{
					string full_value = Molecule.ExpandSMARTSAbbn(value);
					StandardizationModuleCollection.Add(new Tuple<StandardizationRuleType, string>(rt, full_value));
				}
			}
		}

		/// <summary>
		/// Currently takes all metals apart from Group 1 into account. This may not be appropriate!
		/// Remove lines specifying different families of metals if inappropriate.
		/// </summary>
		public static Molecule MendCarbonMetalSigmaBonds(Molecule m)
		{
			if (m.Match("[C;-,--,---]") && m.Match("[{M};+,++,+++,++++]"))
			{
				List<Tuple<int, int>> atomchanges = new List<Tuple<int, int>>();

				Dictionary<int, Tuple<int, int>> newBonds = new Dictionary<int, Tuple<int, int>>();
				List<string> metals = new List<string>();
				metals.AddRange(AtomicProperties.Lanthanides);
				metals.AddRange(AtomicProperties.Actinides);
				metals.AddRange(AtomicProperties.Group2Atoms);
				metals.AddRange(AtomicProperties.pBlockMetals);
				metals.AddRange(AtomicProperties.TransitionMetalsLessHg);
				metals.Add("Hg");
				double range = m.MeanBondLength() * 2;
				var candidateMetals = from ap in m.IndexedAtoms
									  where ap.Value.Charge > 0
									  where metals.Contains(ap.Value.Element)
									  select ap.Key;
				foreach (int cNo in from ap in m.IndexedAtoms where ap.Value.Charge < 0 where ap.Value.Element == "C" select ap.Key)
				{
					foreach (int mNo in candidateMetals.Where(n => m.IndexedAtoms[n].xyz.DistanceFrom(m.IndexedAtoms[cNo].xyz) < range))
					{
						if (m.IndexedAtoms[cNo].Charge < 0)
						{
							int newBondIndex = m.IndexedBonds.Count + newBonds.Count + 1;
							newBonds.Add(newBondIndex, new Tuple<int, int>(cNo, mNo));
							atomchanges.Add(new Tuple<int, int>(cNo, 1)); // increase carbon charge by 1
							atomchanges.Add(new Tuple<int, int>(mNo, -1)); // decrease metal charge by 1
						}
					}
				}
				Dictionary<int, int> netChanges = (from g in atomchanges.GroupBy(x => x.Item1).Select(g => new { g.Key, sum = (g.Sum(x => x.Item2)) })
												   select new KeyValuePair<int, int>(g.Key, g.sum)).ToDictionary(p => p.Key, p => p.Value);
				Dictionary<int, Atom> finalAtoms = (from a in m.IndexedAtoms
													select netChanges.ContainsKey(a.Key)
													? new KeyValuePair<int, Atom>(a.Key, a.Value.UpdateCharge(a.Value.Charge + netChanges[a.Key]))
													: a).ToDictionary(p => p.Key, p => p.Value);
				Dictionary<int, Bond> finalBonds = m.IndexedBonds.Concat((from b in newBonds
																		  select new KeyValuePair<int, Bond>(b.Key,
																			  new Bond(m.IndexedAtoms[b.Value.Item1], m.IndexedAtoms[b.Value.Item2],
																			   b.Value.Item1, b.Value.Item2, BondOrder.Single, BondStereo.None)))).ToDictionary(p => p.Key, p => p.Value);

				return new Molecule(m.Headers, finalAtoms, finalBonds, new Dictionary<string, List<string>> ());
			}
			else
			{
				return m;
			}
		}

		public Molecule StandardizeHexagons(Molecule m, bool doLayout = true)
		{
			if (doLayout)
				return (m.HasHaworth() || m.HasChair())
					? LayoutStructure(StandardizeChairs(StandardizeHowarths(m)), Resources.LayoutOptions.Throw)
					: m;
			else
			{
				//will handle layout 
				if (m.HasHaworth() || m.HasChair())
					return StandardizeChairs(StandardizeHowarths(m));
				else return m;
			}
		}

		/// <summary>
		/// Lays out and checks whether InChIs are equal before and after layout 
		/// Throws exception on failure.
		/// </summary>
		public Molecule LayoutStructure(Molecule m, Resources.LayoutOptions lo)
		{
			string before = InChIUtils.mol2InChI(m.ct(), InChIFlags.Standard);
			lock (s_indigo)
			{
				IndigoObject clone = s_indigo.loadMolecule(m.ct());
				clone.layout();
				string after = InChIUtils.mol2InChI(clone.molfile(), InChIFlags.Standard);
				if (before != after)
				{
					if (lo == Resources.LayoutOptions.Throw)
					{
						throw new Exception(String.Format(@":: layout changed InChI:
<br />before: {0}
<br /> after: {1}",
						  before, after));
					}
					else
					{
						return m;
					}
				}
				else
				{
					return MoleculeFactory.Molecule(clone.molfile());
				}
			}
		}

		/// <summary>
		/// For each chair in the structure, uses FirstChairToHexagon to:
		///  (1) redraw any chair as a regular hexagon
		///  (2) reassign stereo of any bonds connected to the chair
		/// </summary>
		/// <returns></returns>
		public Molecule StandardizeChairs(Molecule m)
		{
			return m.HasChair() ? IterateTransform(mm => mm.HasChair(), mm => StandardizeFirstChair(mm), m) : m;
		}

		public Molecule StandardizeBoats(Molecule m)
		{
			throw new NotImplementedException("BOAT FIX NOT IMPLEMENTED YET");
		}

		public Molecule StandardizeHowarths(Molecule m)
		{
			return m.HasHaworth() ? IterateTransform(mm => mm.HasHaworth(), mm => StandardizeFirstHowarth(mm), m) : m;
		}

		public Molecule IterateTransform(Func<Molecule, bool> test, Func<Molecule, Molecule> transform, Molecule m)
		{
			return test(m) ? IterateTransform(test, transform, transform(m), 0) : m;
		}

		/// <summary>
		/// Contains protection against infinite loops (where infinite here is >12).
		/// </summary>
		public Molecule IterateTransform(Func<Molecule, bool> test, Func<Molecule, Molecule> transform, Molecule m, int counter)
		{
			if (counter > 12) throw new Exception("too many iterations");
			return test(m) ? IterateTransform(test, transform, transform(m), counter + 1) : m;
		}

		/// <summary>
		/// Converts Howarth sugar to machine-readable stereochemistry.
		/// </summary>
		public Molecule StandardizeFirstHowarth(Molecule m)
		{
			Hexagon h = (from hx in m.Hexagons()
						 where hx.IsHaworth(m)
						 select hx).First();
			Dictionary<int, Atom> newAtomList = new Dictionary<int, Atom>();
			Dictionary<int, Bond> newBondList = new Dictionary<int, Bond>();
			var ringAtomIDs = h.IndexedAtoms.Keys;
			// flatten ring bonds
			foreach (var pair in h.IndexedBonds)
			{
				newBondList.Add(pair.Key, new Bond(pair.Value.Atom1, pair.Value.Atom2,
					pair.Value.firstatomID, pair.Value.secondatomID, BondOrder.Single, BondStereo.None));
			}

			var COangles = from b in h.IndexedBonds where b.Value.HasElements("C", "O") select b.Value.AngleXYPlane(BondSense.Forward);
			foreach (var b in h.ExternalBonds(m).Where(b => b.Value.Item1.IsOrthogonal(COangles)))
			{
				var bond = b.Value.Item1;
				var bs = b.Value.Item2;

				var innernode = (bs == BondSense.Forward) ? bond.Atom1.xy : bond.Atom2.xy;
				var newstereo = bond.InterpretPerspectiveStereo(bs, HexagonGeometry.Regular);
				// rotate upward-pointing ones 30 degrees clockwise, downward 30 degrees anticlockwise
				var outernode = h.Centroid.Walk(h.Centroid.DistanceFrom(innernode) * 1.4,
					h.Centroid.AngleXYPlane(innernode) + (Math.PI / 12.0) * ((newstereo == BondStereo.Up) ? 1 : -1));

				newAtomList.Add((bs == BondSense.Forward) ? bond.secondatomID : bond.firstatomID,
					(bs == BondSense.Forward) ? bond.Atom2.Move(outernode.Item1, outernode.Item2) : bond.Atom1.Move(outernode.Item1, outernode.Item2));
				newBondList.Add(b.Key, new Bond(
					(bs == BondSense.Forward) ? bond.Atom1 : newAtomList[bond.firstatomID],
					(bs == BondSense.Forward) ? newAtomList[bond.secondatomID] : bond.Atom2,
					bond.firstatomID, bond.secondatomID, bond.order, newstereo));
			}
			return ReplaceAtomsAndBonds(m, newAtomList, newBondList);
		}

		/// <summary>
		/// Converts chair sugar ring with human-readable stereo to regular hexagon with machine-readable stereo.
		/// Non-layout-preserving.
		/// </summary>
		public Molecule StandardizeFirstChair(Molecule m)
		{
			Hexagon h = (from hx in m.Hexagons()
						 where hx.Geometry() == HexagonGeometry.Chair
						 select hx).First();
			Dictionary<int, Atom> newAtomList = new Dictionary<int, Atom>();
			Dictionary<int, Bond> newBondList = new Dictionary<int, Bond>();

			Dictionary<int, int> chairAtomMapping = h.ChairToRegularIDMapping();
			for (int i = 0; i < 6; i++)
			{
				var xy = h.InnerWheel(i, 6);
				Atom a = m.IndexedAtoms[chairAtomMapping[i]];
				newAtomList.Add(chairAtomMapping[i], a.Move(xy.Item1, xy.Item2));
			}
			// flatten INternal bonds
			foreach (var b in h.IndexedBonds.Where(b => b.Value.bondStereo != BondStereo.None))
			{
				newBondList.Add(b.Key, new Bond(b.Value.Atom1, b.Value.Atom2, b.Value.firstatomID, b.Value.secondatomID, b.Value.order, BondStereo.None));
			}
			// now EXternal bonds
			foreach (var b in h.ExternalBonds(m))
			{
				var bond = b.Value.Item1; var bs = b.Value.Item2;
				var newstereo = bond.InterpretPerspectiveStereo(bs, HexagonGeometry.Chair);
				int ringatomID = bs == BondSense.Forward ? bond.firstatomID : bond.secondatomID;
				int extatomID = bs == BondSense.Forward ? bond.secondatomID : bond.firstatomID;
				var outernode = newstereo == BondStereo.Up ? h.OuterWheelUp(extatomID, 6) : h.OuterWheelDown(extatomID, 6);
				newAtomList.Add(extatomID, m.IndexedAtoms[extatomID].Move(outernode.Item1, outernode.Item2));
				newBondList.Add(b.Key, new Bond(newAtomList[bond.firstatomID], newAtomList[bond.secondatomID], bond.firstatomID, bond.secondatomID, bond.order, newstereo));
			}
			return ReplaceAtomsAndBonds(m, newAtomList, newBondList);
		}

		/// <summary>
		/// Creates a new Molecule object with updates from the atom and bond lists supplied.
		/// </summary>
		public Molecule ReplaceAtomsAndBonds(Molecule m, Dictionary<int, Atom> newAtomList, Dictionary<int, Bond> newBondList)
		{
			Dictionary<int, Atom> finalAtoms = (from a in m.IndexedAtoms
												select newAtomList.ContainsKey(a.Key)
												? new KeyValuePair<int, Atom>(a.Key, newAtomList[a.Key])
												: a).ToDictionary(p => p.Key, p => p.Value);

			Dictionary<int, Bond> finalBonds = (from b in m.IndexedBonds
												select newBondList.ContainsKey(b.Key)
												? new KeyValuePair<int, Bond>(b.Key, newBondList[b.Key])
												: b).ToDictionary(p => p.Key, p => p.Value);
			return new Molecule(m.Headers, finalAtoms, finalBonds, new Dictionary<string, List<string>>());
		}


		public string StandardizeBySMIRKS(string mol_input, string smirks)
		{
			string mol_output = mol_input;
			Tuple<string, bool> transform = s_reactor.Product(mol_output, smirks);
			if (transform.Item2)
			{
				return transform.Item1;
			}
			return mol_input;
		}

		/// <summary>
		/// returns new mol file for standardized molecule
		/// </summary>
		public StandardizationResult Standardize(string sdf, Resources.Vendor vendor)
		{
			if (!StandardizationModuleCollection.Any())
				return new StandardizationResult() { Standardized = sdf };

			var issues = new List<Issue>();

			lock (s_indigo)
			{
				try
				{
					Trace.TraceInformation("\nStandardization started.." + DateTime.Now);

					IndigoObject obj_mol = null;
					try
					{
						obj_mol = s_indigo.loadMolecule(sdf);
						obj_mol.dearomatize();
					}
					catch (IndigoException ex)
					{
						Trace.TraceInformation("Record is not loadable to Indigo. No standardization performed: " + ex.Message + "\n" + ex.StackTrace);
						Trace.TraceError("Record is not loadable to Indigo. No standardization performed: " + ex.Message);
						issues.Add(new Issue
						{
							Code = "400.12",
							AuxInfo = ex.StackTrace,
							Message = ex.Message
						});

						return new StandardizationResult() { Issues = issues };
					}

					//if no atoms or more than max allow atom count in molfile then return null
					int atomCount = obj_mol.countAtoms();

					if (atomCount == 0)
					{
						Trace.TraceInformation("Record has no atoms. No standardization performed");
						issues.Add(new Issue { Code = "400.5" });

						return new StandardizationResult() { Issues = issues };
					}
					else if (atomCount > StandardizationAtomLimit)
					{
						Trace.TraceInformation("Record has >" + StandardizationAtomLimit + " atoms. No standardizaiton was performed");

						issues.Add(new Issue { Code = "400.6" });

						return new StandardizationResult() { Issues = issues };
					}

					string new_mol = obj_mol.molfile();
					foreach (Tuple<StandardizationRuleType, string> rule in StandardizationModuleCollection)
					{
						if (String.IsNullOrEmpty(new_mol))
							break;

						try
						{

							if (rule.Item1 == StandardizationRuleType.smirks)
							{
								new_mol = StandardizeBySMIRKS(new_mol, rule.Item2);
							}
							else if (rule.Item1 == StandardizationRuleType.module)
							{
								Enums.ProcessingModule module;
								bool res = Enum.TryParse(rule.Item2, out module);

								Trace.TraceInformation("Starting module " + module + ": " + DateTime.Now);

								if (module == Enums.ProcessingModule.Dearomatize)
									new_mol = standardizationModule.Kekulize(new_mol);
								else if (module == Enums.ProcessingModule.Aromatize)
									new_mol = standardizationModule.Aromatize(new_mol);
								else if (module == Enums.ProcessingModule.ConvertDoubleBondWithAttachedEitherSingleBondStereo2EitherDoubleBond)
									new_mol = standardizationStereoModule.ConvertDoubleBondWithAttachedEitherSingleBondStereoToEitherDoubleBond(new_mol);
								else if (module == Enums.ProcessingModule.Layout)
								{
									if (atomCount <= LayoutAtomLimit)
									{
										bool StdInChIChanged;
										string layout_mol = standardizationModule.Layout(new_mol, Resources.Vendor.OpenEye, out StdInChIChanged, issues);

										if (StdInChIChanged)
										{
											if (!issues.Where(i => i.Code == "400.2").Any())
												issues.Add(new Issue { Code = "400.2" });
										}
										else new_mol = layout_mol;
									}
									else
										issues.Add(new Issue { Code = "400.2" });
								}
								else if (module == Enums.ProcessingModule.StandardizeHexagons)
									new_mol = StandardizeHexagons(MoleculeFactory.FromMolFile(new_mol, null, ChemicalFormat.V2000), false).ct();
								else if (module == Enums.ProcessingModule.DisconnectMetalsFromNonMetals)
									new_mol = standardizationMetalsModule.DisconnectMetalsFromNonMetals(new_mol, true);
								else if (module == Enums.ProcessingModule.DisconnectMetalsFromNOF)
									new_mol = standardizationMetalsModule.DisconnectMetalsFromNOF(new_mol, true);
								else if (module == Enums.ProcessingModule.IonizeNeutralAlkalineMetalsWithCarboxylicAcids)
									new_mol = standardizationMetalsModule.IonizeFreeMetalWithCarboxylicAcid(new_mol, true);
								else if (module == Enums.ProcessingModule.ApplyCVSPAcidBaseSMIRKS)
									new_mol = StandardizeByAcidBaseSMIRKS(new_mol);
								else if (module == Enums.ProcessingModule.RemoveFreeMetals)
									new_mol = standardizationMetalsModule.RemoveFreeMetals(new_mol);
								else if (module == Enums.ProcessingModule.StandardInChINormalization)
									new_mol = standardizationModule.StandardizeByInChIRules(new_mol);
								else if (module == Enums.ProcessingModule.CanonicalizeTautomers)
								{
									if (standardizationModule.ShouldRunTautomerCanonicalizer(new_mol, TautomerCanonicalizationAtomLimit))
									{
										string new_inchi, new_inchi_key;
										new_mol = standardizationModule.CanonicalizeTautomer(new_mol, true, vendor,
											out new_inchi,
											out new_inchi_key, InChIFlags.CRS);
									}
									else
										issues.Add(new Issue { Code = "400.10" });
								}
								else if (module == Enums.ProcessingModule.RetainLargestOrganicFragment)
								{
									string new_inchi, new_inchi_key;
									new_mol = standardizationFragmentsModule.getLargestOrganicFragment(new_mol, out new_inchi, out new_inchi_key);
								}
								else if (module == Enums.ProcessingModule.NeutralizeCharges)
								{
									string new_inchi, new_inchi_key;
									new_mol = standardizationChargesModule.NeutralizeCharges(new_mol, out new_inchi, out new_inchi_key, InChIFlags.CRS);
								}
								//else if ((module == Enums.ProcessingModule.Apply_CVSP_SMIRKS || module == Enums.ProcessingModule.Apply_SMIRKS_From_User_Profile))
								//	new_mol = StandardizeBySMIRKS(new_mol, m_reactors);
								else if ((module == Enums.ProcessingModule.ConvertUpOrDownBondsAdjacentToDoubleBondToNoStereoSingleBondsWithCrossedDoubleBond))
									new_mol = standardizationStereoModule.ConvertUpOrDownBondsAdjacentToDoubleBondToNoStereoSingleBondsWithCrossedDoubleBond(new_mol);
								else if (module == Enums.ProcessingModule.RemoveWater)
									new_mol = standardizationFragmentsModule.removeWater(new_mol);
								else if (module == Enums.ProcessingModule.TreatAmmonia)
									new_mol = standardizationModule.TreatAmmonia(new_mol);
								else if (module == Enums.ProcessingModule.RemoveNeutralInorganicResidue)
									new_mol = standardizationFragmentsModule.removeNeutralInorganicAcidBaseResidues(new_mol);
								else if (module == Enums.ProcessingModule.RemoveOrganicSolvents)
									new_mol = standardizationFragmentsModule.removeOrganicSolvents(new_mol);
								else if (module == Enums.ProcessingModule.RemoveGaseousMolecules)
									new_mol = standardizationFragmentsModule.removeGasMolecules(new_mol);

									//stereo
								else if (module == Enums.ProcessingModule.FoldNonStereoHydrogens)
									new_mol = standardizationModule.FoldNonStereoHydrogens(new_mol);
								else if (module == Enums.ProcessingModule.StripAmbiguousSp3Stereo)
									new_mol = standardizationModule.RemoveAmbiguousSp3Stereo(new_mol);
								else if (module == Enums.ProcessingModule.FoldAllHydrogens)
									new_mol = standardizationModule.FoldAllHydrogens(new_mol);
								else if (module == Enums.ProcessingModule.RemoveSP3Stereo)
									new_mol = standardizationStereoModule.RemoveSP3Stereo(new_mol);
								else if (module == Enums.ProcessingModule.ResetSymmetricStereoCenters)
									new_mol = standardizationStereoModule.ResetSymmetricStereoCenters(new_mol);
								else if (module == Enums.ProcessingModule.ResetSymmetricCisTransBonds)
									new_mol = standardizationStereoModule.ResetSymmetricCisTrans(new_mol);
								else if (module == Enums.ProcessingModule.ConvertDoubleBondtoEither)
									new_mol = standardizationStereoModule.ConvertDoubleBondsToEither(new_mol);
								else if (module == Enums.ProcessingModule.ConvertEitherDoubleBondsToStereo)
									new_mol = standardizationStereoModule.ConvertEitherBondsToDefined(new_mol);
								else if (module == Enums.ProcessingModule.RemoveAlleneStereo)
									new_mol = standardizationStereoModule.RemoveAlleneStereo(new_mol);
								Trace.TraceInformation("Finishing module at " + DateTime.Now);
							}
						}
						catch (Exception ex)
						{
							issues.Add(new Issue
							{
								Code = "400.12",
								AuxInfo = ex.StackTrace,
								Message = ex.Message + Environment.NewLine + ex.InnerException
							});
						}
					}

					Trace.TraceInformation("\nStandardization finished.." + DateTime.Now + "\n");

					return new StandardizationResult() { Standardized = new_mol, Issues = issues };
				}
				catch (Exception ex)
				{
					Trace.TraceInformation("Record failed standardization: " + ex.Message + "\n" + ex.StackTrace + "\n" + ex.InnerException);
					Trace.TraceError("Record failed standardization: " + ex.Message + "\n" + ex.StackTrace + "\n" + ex.InnerException);
					issues.Add(new Issue
					{
						Code = "400.12",
						AuxInfo = ex.StackTrace,
						Message = ex.Message + Environment.NewLine + ex.InnerException
					});

					return new StandardizationResult() { Issues = issues };
				}
			}
		}

		/// <returns>Item1: standardized smiles, Item2: reactions applied.</returns>
		public string StandardizeByAcidBaseSMIRKS(string molFile)
		{
			List<string> reactionsApplied = new List<string>();
			string c_smiles_before = null;
			string c_smiles_after = null;
			string new_ct = molFile;
			int i = 0;
			lock (s_indigo)
			{
				do
				{
					i++;
					IndigoObject newmol = s_indigo.loadMolecule(new_ct);
					Dictionary<int, string> transforms = acidity.AcidBaseSMIRKS(new_ct);
					if (transforms.Count > 0)
					{
						newmol.dearomatize();
						c_smiles_before = newmol.canonicalSmiles();

						foreach (KeyValuePair<int, string> smirks in transforms)
						{
							Tuple<string, bool> res = s_reactor.Product(new_ct, smirks.Value);

							new_ct = res.Item1;
							if (res.Item2)
								reactionsApplied.Add(smirks.Value);
						}
						IndigoObject new_obj = s_indigo.loadMolecule(new_ct);
						new_obj.dearomatize();
						c_smiles_after = new_obj.canonicalSmiles();
					}
					else break;
				}
				while (c_smiles_before != c_smiles_after && i < 10);
			}
			if (reactionsApplied.Count > 0)
				return new_ct;
			else
				return molFile;
		}
	}
}
