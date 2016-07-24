using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using ChemSpider.Molecules;

namespace ChemSpider.ObjectModel
{
    [DataContract]
    public class CompoundBase
    {
        private Hashtable _properties = null;
        private IEnumerable<Identifier> _identifiers = null;
        private IEnumerable<Synonym> _synonyms = null;
        private IEnumerable<Reference> _refs = null;
        private IEnumerable<Blob> _blobs = null;
        private IEnumerable<DatasourceType> _datasourceTypes = null;
        private IEnumerable<Similarity> _similarities = null;
        private SdfRecord _sdf = null;

        public CompoundBase(int csid)
        {
            CSID = csid;
        }

        [DataMember]
        [Description("Internal ChemSpider ID")]
        public int CSID
        {
            get;
            set;
        }

        public Hashtable Properties
        {
            get
            {
                if (_properties == null)
                    _properties = PullProperties();

                return _properties;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("List of alternative compound's identifications (InChi, SMILES etc.)")]
        public IEnumerable<Identifier> Identifiers
        {
            set { _identifiers = value; }
            get
            {
                if (_identifiers == null)
                    _identifiers = PullIdentifiers();

                return _identifiers;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("List of synonyms assigned to the compound")]
        public IEnumerable<Synonym> Synonyms
        {
            set { _synonyms = value; }
            get
            {
                if (_synonyms == null)
                    _synonyms = PullSynonyms();

                return _synonyms;
            }
        }

        public IEnumerable<Reference> GetReferences(int? dsn_id)
        {
            return PullReferences(dsn_id);
        }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        [Description("List of compound's references")]
        public IEnumerable<Reference> References
        {
            set { _refs = value; }
            get
            {
                if (_refs == null)
                    _refs = PullReferences(null);

                return _refs;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("List of blobs attached to compound (spectra, images, cifs etc.)")]
        public IEnumerable<Blob> Blobs
        {
            set { _blobs = value; }
            get
            {
                if (_blobs == null)
                    _blobs = PullBlobs();

                return _blobs;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("List of similar compounds")]
        public IEnumerable<Similarity> Similarities
        {
            set { _similarities = value; }
            get
            {
                if (_similarities == null)
                    _similarities = PullSimilarities();

                return _similarities;
            }
        }

        public IEnumerable<DatasourceType> GetDatasources(int? dsn_id, int? col_id, int? dst_id)
        {
            return PullDatasources(dsn_id, col_id, dst_id);
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("List of compound's data sources types and data sources from that types and references from that data sources")]
        public IEnumerable<DatasourceType> Datasources
        {
            set { _datasourceTypes = value; }
            get
            {
                if (_datasourceTypes == null)
                    _datasourceTypes = PullDatasources(null, null, null);

                return _datasourceTypes;
            }
        }

        /// <summary>
        /// Returns the molfile.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [Description("MOL file")]
        public string Mol
        {
            set { _sdf = new SdfRecord(value, null); }
            get 
            {
                if (_sdf == null)
                    initSdf();

                return _sdf == null ? null : _sdf.Mol;
            }
        }

        private void initSdf()
        {
            string sdf = PullSDF();
            if (String.IsNullOrWhiteSpace(sdf))
                _sdf = null;
            else
            {
                _sdf = SdfRecord.FromString(sdf);
                if (_sdf.Properties != null)
                    _sdf.Properties.Clear();
            }
        }

        public string Sdf
        {
            get
            {
                if (_sdf == null)
                    initSdf();

                if (_sdf != null && (_sdf.Properties == null || (_sdf.Properties != null && _sdf.Properties.Count == 0)))
                {
                    _sdf.AddField("CSID", CSID.ToString());

                    foreach (Identifier id in Identifiers.Where(id => id.IdentifierType == IdentifierType.SMILES))
                        _sdf.AddField("SMILES", id.ToString());

                    foreach (Identifier id in Identifiers.Where(id => id.IdentifierType == IdentifierType.InChIKey))
                        _sdf.AddField("InChIKey", id.ToString());

                    foreach (Identifier id in Identifiers.Where(id => id.IdentifierType == IdentifierType.InChI))
                        _sdf.AddField("InChI", id.ToString());

                    foreach (Synonym synonym in Synonyms)
                        _sdf.AddField("Synonyms", synonym.ToString());

                    foreach (Reference r in References)
                        _sdf.AddField("References", r.ToString());
                }

                return _sdf == null ? null : _sdf.ToString();
            }
        }

        protected virtual Hashtable PullProperties()
        {
            throw new NotImplementedException("PullProperties is not implemented");
        }

        protected virtual IEnumerable<Identifier> PullIdentifiers()
        {
            throw new NotImplementedException("PullIdentifiers is not implemented");
        }

        protected virtual IEnumerable<Synonym> PullSynonyms()
        {
            throw new NotImplementedException("PullSynonyms is not implemented");
        }

        protected virtual IEnumerable<Reference> PullReferences(int? dsn_id)
        {
            throw new NotImplementedException("PullReferences is not implemented");
        }

        protected virtual IEnumerable<Blob> PullBlobs()
        {
            throw new NotImplementedException("PullBlobs is not implemented");
        }

        protected virtual IEnumerable<DatasourceType> PullDatasources(int? dsn_id, int? col_id, int? dst_id)
        {
            throw new NotImplementedException("PullDatasources is not implemented");
        }

        protected virtual IEnumerable<Similarity> PullSimilarities()
        {
            throw new NotImplementedException("PullSimilarities is not implemented");
        }

        protected virtual string PullSDF()
        {
            throw new NotImplementedException("PullSDF is not implemented");
        }
    }
}
