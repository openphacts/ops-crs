using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoleculeObjects;

using com.ggasoftware.indigo;
using RSC.Logging;

namespace RSC.CVSP.Compounds
{
	public interface IValidationStereoModule
	{
		bool ContainsUpAndDownBondsWithNoChiralFlag(string molfile);

		/// <summary>
		/// there is a chiral flag but no up or down bonds
		/// </summary>
		/// <param name="molfile"></param>
		/// <returns></returns>
		bool ContainsNoUpOrDownBondWithChiralFlag(string molfile);

		bool ContainsRelativeStereo(string mol);

		/// <summary>
		/// ST-1.1.4
		/// Three plain bonds and one wedged bond, with one pair of plain bonds separated by 180° or more and
		/// the wedged bond positioned within that largest space between plain bonds.
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="issues"></param>
		/// <returns></returns>
		bool Contains_3_PlainBondsAndStereoBond_ST_1_1_4(string mol, ICollection<Issue> issues = null);

		bool ContainsRingSP3StereoBond(string mol, ICollection<Issue> issues = null);

		bool ContainsStereoCentersWithMoreThan2StereoBonds(string mol, ICollection<Issue> issues = null);

		/// <summary>
		/// ST-2.3 T-shaped; should be no Z coordinate
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="issues"></param>
		/// <returns></returns>
		bool ContainsStereoCenterWith3Bonds_TShaped(string mol, ICollection<Issue> issues = null);

		/// <summary>
		/// ST-1.6
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="issues"></param>
		/// <returns></returns>
		bool ContainsBadAlleneStereo(string mol, ICollection<Issue> issues = null);

		/// <summary>
		/// ST-0.5. Stereobonds between stereocenters. Stereo-capable centers are identified only by stereo bonds. 
		/// Page 1908 in Graphical Representation of Stereo Centers
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="issues"></param>
		/// <returns></returns>
		bool ContainsStereoBondBetweenStereoCenters(string mol, ICollection<Issue> issues = null);

		bool ContainsDoubleBondWithAdjacentWavyBond(string mol, ICollection<Issue> issues = null);

        bool IsChiralFlagSet(string mol);
	}
}
