using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds
{
    public enum CompoundSynonymState
    {
        /// <summary>
        /// Logically removed
        /// </summary>
        eDeleted,

        /// <summary>
        /// Disproved by curator
        /// </summary>
        eDisproved,

        /// <summary>
        /// Negative opinion expressed
        /// </summary>
        eUnconfirmed,

        /// <summary>
        /// Positive opinion expressed
        /// </summary>
        eConfirmed,

        /// <summary>
        /// Approved by curator
        /// </summary>
        eApproved,
    }

}
