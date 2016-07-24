using System.Collections.Generic;
using System.Linq;

namespace RSC.Compounds.EntityFramework.Extensions
{
    public static class CompoundSynonymExtensions
    {
        /// <summary>
        /// Converts an ef_CompoundSynonym to a core Synonym.
        /// </summary>
        /// <param name="ef">ef_CompoundSynonym for conversion.</param>
        /// <returns>The converted core Synonym.</returns>
        public static Synonym ToSynonym(this ef_CompoundSynonym ef)
        {
            var synonym = new Synonym()
            {
                DateCreated = ef.Synonym.DateCreated,
                LanguageId = ef.Synonym.LanguageId,
                Name = ef.Synonym.Synonym,
                Flags =
                    ef.Synonym.SynonymFlags == null
                        ? new List<SynonymFlag>()
                        : ef.Synonym.SynonymFlags.Select(sf => new SynonymFlag()
                        {
                            Type = sf.SynonymFlag.Type,
                            RegEx = sf.SynonymFlag.RegEx,
                            IsUniquePerLanguage = sf.SynonymFlag.IsUniquePerLanguage,
                            IsRestricted = sf.SynonymFlag.IsRestricted,
                            Description = sf.SynonymFlag.Description,
                            ExcludeFromTitle = sf.SynonymFlag.ExcludeFromTitle,
                            Name = sf.SynonymFlag.Name,
                            Rank = sf.SynonymFlag.Rank,
                            Url = sf.SynonymFlag.Url
                        }),
                State = ef.SynonymState,
                IsTitle = ef.IsTitle,
                History = ef.History.Select(h => new SynonymHistory()
                {
                    CuratorId = h.CuratorId,
                    DateChanged = h.DateChanged,
                    IsTitle = h.IsTitle,
                    SynonymState = h.SynonymState
                })
            };
            return synonym;
        }
    }
}
