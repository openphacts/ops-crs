using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Compounds
{
	public static class Enums
	{
		[DataContract]
		public enum ProcessingModule
		{
			//validation modules
			None = 0,
			ValidateStructure,
			ValidateInChI,
			ValidateSmiles,
			ValidateSynonyms,
			ValidateAll,
			ZeroAtomCount,
			ExceedingMaxAtomCount,
			ContainsAmbiguousHydrogen,
			ContainsRelativeStereoInV2000,
			ContainsCrossedDoubleBond,
			ContainsDoubleBondWithAdjacentWavyBond,
			CanGenerateInChI,
			InChIInChemSpider,
			CanGenerateSmiles,
			ContainsDuplicateMolecules,
			ContainsOnlyMultipleInstancesOfSameMolecules,
			ContainsRadicals,
			ContainsMixtureWithFreeCO,

			ValidateRegIds,
			AnalyzeStereoByInChI,
			ValidateOverallCharge,
			ValidateValenceCharge,
			ContainsRingSP3StereoBond,
			ContainsStereoBondBetweenStereoCenters,
			ContainsStereoCentersWithMoreThan2StereoBonds,
			ContainsStereoCenterWith3BondsTShaped,
			ContainsBadAlleneStereo,
            ContainsUnevenLengthBonds,
			//standardization modules
			Layout,
			Dearomatize,
			Aromatize,
			DisconnectMetalsFromNOF,
			DisconnectMetalsFromNonMetals,//Disconnect metals (excluding Hg, Ga, Ge, In, Sn, As, Tl, Pb, Bi, Po) from non-metals (except N,O,F)
			IonizeNeutralAlkalineMetalsWithCarboxylicAcids,
			RemoveWater,
			RemoveFreeMetals,
			RemoveNeutralInorganicResidue,
			RemoveIonizedInorganicAcidsBases,
			RemoveGaseousMolecules,
			RemoveOrganicSolvents,
			TreatAmmonia,
			FoldAllHydrogens,
			FoldNonStereoHydrogens,
			StripAmbiguousSp3Stereo,
			RemoveSP3Stereo,
			RemoveAlleneStereo,
			ConvertDoubleBondtoEither,
			ConvertEitherDoubleBondsToStereo,
			ApplySMIRKSFromUserProfile,
			ApplyCVSPSMIRKS,
			ApplyCVSPAcidBaseSMIRKS,
			StandardInChINormalization,
			RetainLargestOrganicFragment,
			CanonicalizeTautomers,
			NeutralizeCharges,
			ReplaceIsotopeWithCommonElement,
			ResetSymmetricStereoCenters,
			ResetSymmetricCisTransBonds,
			ConvertDoubleBondWithAttachedEitherSingleBondStereo2EitherDoubleBond,//35
			ConvertUpOrDownBondsAdjacentToDoubleBondToNoStereoSingleBondsWithCrossedDoubleBond,
			StandardizeHexagons

		}
	}
}
