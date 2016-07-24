using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.DataExport
{
    public static class SynonymExtensions
    {
        /// <summary>
        /// Returns the predicate-object part of a triple specifying a validated or unvalidated synonym or database ID.
        /// </summary>
        /// <param name="synonym">Synonym object.</param>
        public static string ToPredicateObject(this Synonym synonym)
        {
            //Add the chemSpider title.
            if (synonym.IsTitle)
            {
                return string.Format("cheminf:{0} \"{1}\"@{2}"
                    , Turtle.cheminf_title //0
                    , synonym.Name.RdfEncode() //1
                    , synonym.LanguageId //2
                    );
            }
            else
            {
                //Add the normal synonyms and set appropriate predicate.
                return string.Format("cheminf:{0} \"{1}\"@{2}"
                    , synonym.Flags.Any(i => i.Name == "DBID")
                        ? synonym.State == CompoundSynonymState.eApproved
                            ? Turtle.cheminf_validatedDbid
                            : Turtle.cheminf_unvalidatedDbid //Dbids
                        : synonym.State == CompoundSynonymState.eApproved
                            ? Turtle.cheminf_validatedSynonym
                            : Turtle.cheminf_unvalidatedSynonym //Synonyms
                    , synonym.Name.RdfEncode()
                    , synonym.LanguageId);
            }
        }
    }
}
