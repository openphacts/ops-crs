using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds
{
	public interface ICompoundConvert
	{
		/// <summary>
		/// Convert InChI to Compound Id
		/// </summary>
		/// <param name="inchi">Standard on Non standard InChI</param>
		/// <returns>Internal compound ID</returns>
		IEnumerable<Guid> InChIToCompoundId(string inchi);
		/// <summary>
		/// Convert InChIKey to Compound Id
		/// </summary>
		/// <param name="inchikey">Standard on Non standard InChIKey</param>
		/// <returns>Internal compound ID</returns>
		IEnumerable<Guid> InChIKeyToCompoundId(string inchikey);
		/// <summary>
		/// Convert SMILES to Compound Id
		/// </summary>
		/// <param name="smiles">SMILES</param>
		/// <returns>Internal compound Id</returns>
		IEnumerable<Guid> SMILESToCompoundId(string smiles);
		/// <summary>
		/// Convert MOL to Compound Id
		/// </summary>
		/// <param name="mol">MOL</param>
		/// <returns>Internal compound Id</returns>
		//IEnumerable<Guid> MolToCompoundId(string mol);

		/// <summary>
		/// Convert list of external references to compound Ids
		/// </summary>
		/// <param name="references">List of external references</param>
		/// <param name="uri">Exterenal reference Uri</param>
		/// <returns>Dictionary of external reference and compound Id</returns>
		IDictionary<string, Guid> ExternalReferencesToCompoundIds(IEnumerable<string> references, string uri);
	}
}
