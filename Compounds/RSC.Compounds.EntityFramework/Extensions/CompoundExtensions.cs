using System.Linq;

namespace RSC.Compounds.EntityFramework.Extensions
{
    public static class CompoundExtensions
    {
        /// <summary>
        /// Converts an ef_Compound to a core Compound.
        /// </summary>
        /// <param name="ef">ef_Compound for conversion.</param>
        /// <returns>The converted core Compound.</returns>
        public static Compound ToCompound(this ef_Compound ef)
        {
            var compound = new Compound()
            {
                Id = ef.Id,
                Mol = ef.Mol,
                NonStandardInChI =
                    ef.NonStandardInChI == null
                        ? null
                        : new InChI(ef.NonStandardInChI.InChI, ef.NonStandardInChI.InChIKey),
                StandardInChI =
                    ef.StandardInChI == null
                        ? null
                        : new InChI(ef.StandardInChI.InChI, ef.StandardInChI.InChIKey),
                TautomericNonStdInChI =
                    ef.TautomericNonStdInChI == null
                        ? null
                        : new InChI(ef.TautomericNonStdInChI.InChI, ef.TautomericNonStdInChI.InChIKey),
                Smiles = ef.Smiles == null ? null : new Smiles(ef.Smiles.IndigoSmiles),
				IsVirtual = ef.Revisions == null || !ef.Revisions.Any()
                //This is a complex filter so removed.
                //ExternalIdentifiers = ef.ExternalIdentifiers == null
                //        ? null
                //        : ef.ExternalIdentifiers
                //            .ToList()
                //            .Select(i => new ExternalIdentifier()
                //        {
                //            Value = i.Value,
                //            Type = (IdentifierType)i.ExternalIdentifierTypeId,
                //            Uri = i.ToUri()
                //        })
                //This is a complex filter so removed.
                //Synonyms = ef.CompoundSynonyms == null 
                //    ? null
                //    : ef.CompoundSynonyms
                //        .Where(cs => cs.Synonym != null) //If not provided, Synonyms can be populated seperately as they are complex.
                //        .Select(cs => new Synonym()
                //{
                //    DateCreated = cs.Synonym.DateCreated,
                //    LanguageId = cs.Synonym.LanguageId,
                //    Name = cs.Synonym.Synonym,
                //    Flags =
                //        cs.Synonym.SynonymFlags == null
                //            ? null
                //            : cs.Synonym.SynonymFlags.Select(sf => new SynonymFlag()
                //            {
                //                Type = sf.SynonymFlag.Type,
                //                RegEx = sf.SynonymFlag.RegEx,
                //                IsUniquePerLanguage = sf.SynonymFlag.IsUniquePerLanguage,
                //                IsRestricted = sf.SynonymFlag.IsRestricted,
                //                Description = sf.SynonymFlag.Description,
                //                ExcludeFromTitle = sf.SynonymFlag.ExcludeFromTitle,
                //                Name = sf.SynonymFlag.Name,
                //                Rank = sf.SynonymFlag.Rank,
                //                Url = sf.SynonymFlag.Url
                //            }).ToList(),
                //    State = cs.SynonymState,
                //    IsTitle = cs.IsTitle,
                //    History = cs.History == null
                //        ? null
                //        : cs.History.Select(h => new SynonymHistory()
                //    {
                //        CuratorId = h.CuratorId,
                //        DateChanged = h.DateChanged,
                //        IsTitle = h.IsTitle,
                //        SynonymState = h.SynonymState
                //    }).ToList()
                //}).ToList()
            };
            return compound;
        }
    }

}
