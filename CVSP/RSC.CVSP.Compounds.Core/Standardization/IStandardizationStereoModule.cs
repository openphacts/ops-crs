
namespace RSC.CVSP.Compounds
{
    public interface IStandardizationStereoModule
    {
        string AddChiralFlag(string mol);

        /// <summary>
        /// no stereo bonds (up or down) but chiral flag is set
        /// in such cases OE Layout puts chiral flag for some reason
        /// </summary>
        string ClearChiralFlagOnFlatStructure(string mol);

        /// <summary>
        /// This converts double bonds with a given regiochemistry to double bonds without, willy-nilly.
        /// </summary>
        string ConvertDoubleBondsToEither(string mol);

        string ConvertDoubleBondWithAttachedEitherSingleBondStereoToEitherDoubleBond(string mol);

        /// <summary>
        /// Converts bond stereo on double bonds from CisTransEither to None.
        /// </summary>
        string ConvertEitherBondsToDefined(string mol);

        string ConvertUpOrDownBondsAdjacentToDoubleBondToNoStereoSingleBondsWithCrossedDoubleBond(string mol);

        string RemoveAlleneStereo(string mol);
        string RemoveChiralFlag(string fakeChiral);
        string RemoveSP3Stereo(string mol);

        /// <summary>
        ///  to clear stereocenters that are not real stereocenters like in `CC[C@@H](CN)CC`
        /// </summary>
        string ResetSymmetricStereoCenters(string mol);

        /// <summary>
        ///  to clear stereocenters that are not real stereocenters like in `CC[C@@H](CN)CC`
        /// </summary>
		string ResetSymmetricCisTrans(string mol);

    }
}
