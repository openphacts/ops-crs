using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds
{
	public static class ICompoundStoreExtensions
	{
		/// <summary>
		/// Returns list of compound's properties
		/// </summary>
		/// <param name="store">ICompoundStore</param>
		/// <param name="compoundId">Compound Id</param>
		/// <returns>List of compound property Ids</returns>
		public static IEnumerable<Guid> GetCompoundProperties(this ICompoundStore store, Guid compoundId)
		{
			var properties = store.GetCompoundsProperties(new Guid[] { compoundId });

			if (properties.ContainsKey(compoundId))
				return properties[compoundId];

			return null;
		}

		/// <summary>
		/// Returns list of compound's synonyms
		/// </summary>
		/// <param name="store">ICompoundStore</param>
		/// <param name="compoundId">Compound Id</param>
		/// <returns>List of compound synonyms</returns>
		public static IEnumerable<Synonym> GetCompoundSynonyms(this ICompoundStore store, Guid compoundId)
		{
			var synonyms = store.GetCompoundsSynonyms(new Guid[] { compoundId });

			if (synonyms.ContainsKey(compoundId))
				return synonyms[compoundId];

			return null;
		}
	}
}
