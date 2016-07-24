using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RSC.Compounds
{
    [Serializable]
    [DataContract]
    public class Compound
    {
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// not required because for tautomers and super parents there is no Mol
        /// </summary>
        [DataMember]
        public string Mol { get; set; }

        //indexes will be created automatically by EF on foreign keys

        /// <summary>
        /// exists only for for real compounds with existsing Mol
        /// </summary>
        [DataMember]
        public InChI NonStandardInChI { get; set; }

        /// <summary>
        /// exists only for tautomers and super parents
        /// </summary>
        [DataMember]
        public InChI TautomericNonStdInChI { get; set; }

        /// <summary>
        /// exists only for real compounds with existing Molfile
        /// </summary>
        [DataMember]
        public InChI StandardInChI { get; set; }

        /// <summary>
        /// not required - because Smiles exists only when Mol is given
        /// </summary>
        /// 
        [DataMember]
        public Smiles Smiles { get; set; }

        [DataMember]
        public IEnumerable<Synonym> Synonyms { get; set; }

        [DataMember]
        public IEnumerable<ExternalReference> ExternalReferences { get; set; }

		[DataMember]
		public bool IsVirtual { get; set; }

        //TODO: Add Parents / Children - list of Parent / Children Ids.
    }

    public class CompoundComparer : IEqualityComparer<Compound>
    {
        bool IEqualityComparer<Compound>.Equals(Compound x, Compound y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            if (x.NonStandardInChI != null && y.NonStandardInChI != null)
                return x.NonStandardInChI.InChIKey == y.NonStandardInChI.InChIKey;
            else if (x.TautomericNonStdInChI != null && y.TautomericNonStdInChI != null)
                return x.TautomericNonStdInChI.InChIKey == y.TautomericNonStdInChI.InChIKey;
            else if (x.StandardInChI != null && y.StandardInChI != null)
                return x.StandardInChI.InChIKey == y.StandardInChI.InChIKey;

            return false;
        }

        int IEqualityComparer<Compound>.GetHashCode(Compound compound)
        {
            if (Object.ReferenceEquals(compound, null)) return 0;

            if (compound.NonStandardInChI != null)
                return compound.NonStandardInChI.InChIKey.GetHashCode();
            else if (compound.TautomericNonStdInChI != null)
                return compound.TautomericNonStdInChI.InChIKey.GetHashCode();
            else if (compound.StandardInChI != null)
                return compound.StandardInChI.InChIKey.GetHashCode();

            return 0;
        }
    }
}
