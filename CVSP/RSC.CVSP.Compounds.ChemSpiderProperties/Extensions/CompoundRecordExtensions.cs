using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RSC.Compounds;

namespace RSC.CVSP.Compounds
{
    public static class CompoundRecordExtensions
    {
        private const string DYNAMIC_PROPERTY_NAME_SYNONYMS = "ChemSpiderSynonyms";

        public static ICollection<Synonym> GetChemSpiderSynonyms(this CompoundRecord compound)
        {
            if (!compound.HasDynamicMember(DYNAMIC_PROPERTY_NAME_SYNONYMS))
                ((dynamic)compound).ChemSpiderSynonyms = new ChemSpiderSynonyms();

            return (((dynamic)compound).ChemSpiderSynonyms as ChemSpiderSynonyms).Synonyms;
        }

        public static void AddChemSpiderSynonym(this CompoundRecord compound, Synonym synonym)
        {
            compound.AddChemSpiderSynonyms(new Synonym[] { synonym });
        }

        public static void AddChemSpiderSynonyms(this CompoundRecord compound, IEnumerable<Synonym> newSynonyms)
        {
            if (!compound.HasDynamicMember(DYNAMIC_PROPERTY_NAME_SYNONYMS))
                ((dynamic)compound).ChemSpiderSynonyms = new ChemSpiderSynonyms();

            var synonyms = ((dynamic)compound).ChemSpiderSynonyms as ChemSpiderSynonyms;

            synonyms.Synonyms = synonyms.Synonyms.Concat(newSynonyms).ToList();

            ((dynamic)compound).ChemSpiderSynonyms = synonyms;
        }
    }
}