using InChINet;

namespace RSC.CVSP.Compounds
{
	public interface IStandardizationChargesModule
	{
		/// <summary>
		/// retuns molfile and reference to new inchi and inchi key
		/// </summary>
		/// <param name="mol"></param>
		/// <param name="new_inchi"></param>
		/// <param name="new_inchi_key"></param>
		/// <returns></returns>
		string NeutralizeCharges(string mol, out string new_inchi, out string new_inchi_key, InChIFlags flags);
	}
}
