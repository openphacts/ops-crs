using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RSC;
using RSC.Compounds;
using RSC.Compounds.DataExport;
using RSC.Compounds.EntityFramework;
using RSC.Datasources;
using RSC.Properties;

namespace RSC.Compounds.DataExport_Tests
{
    public class MockCompoundStore : CompoundStore
    {
        public override IEnumerable<ParentChild> GetChildren(Guid id)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Guid> GetCompoundIds(int start = 0, int count = -1)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Compound> GetCompounds(IEnumerable<Guid> ids, IEnumerable<string> filter)
        {
            return new List<Compound>()
            {
                new Compound() {
                    Id = new Guid("23232323-1111-1111-1111-232323232323"), // same as below
                    ExternalReferences = new List<ExternalReference>() {
                        new ExternalReference() { 
                            Type = new ExternalReferenceType() { UriSpace = Constants.OPSUriSpace }
                        }
                    }
                }
            };
        }

        public override int GetCompoundsCount()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ParentChild> GetCompoundsParentChildren(IEnumerable<Guid> compoundIds, ParentChildRelationship relationship)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<Guid, IEnumerable<Guid>> GetCompoundsPropertiesIds(IEnumerable<Guid> guids)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Guid> GetDataSourceCompoundIds(Guid dataSourceId, int start = 0, int count = -1)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Guid> GetDataSourceRevisionIds(Guid dataSourceId, int start = 0, int count = -1)
        {
            // pro tem
            return new List<Guid>();
        }

        public override IEnumerable<Guid> GetDatasourceIds(Guid compoundId, int start = 0, int count = 1)
        {
            throw new NotImplementedException();
        }

        public override int GetDatasourcesCount(Guid compoundId)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ExternalReferenceType> GetExternalIdTypes()
        {
            throw new NotImplementedException();
        }

        public override string GetMol(Guid id)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ParentChild> GetParents(Guid id)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ParentChildRelationship> GetParentChildRelationships()
        {
            return new List<ParentChildRelationship>() {
                ParentChildRelationship.Fragment,
                ParentChildRelationship.ChargeInsensitive,
                ParentChildRelationship.StereoInsensitive,
                ParentChildRelationship.IsotopInsensitive,
                ParentChildRelationship.TautomerInsensitive,
                ParentChildRelationship.SuperInsensitive };
        }

        public override dynamic GetRecordPropertyValue(IDictionary<ExternalId, IEnumerable<Property>> recordProperties,
            ExternalId extId, string propertyName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// return one (1) revision Id.
        /// </summary>
        /// <param name="revisionIds"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public override IEnumerable<Revision> GetRevisions(IEnumerable<Guid> revisionIds, IEnumerable<string> filter = null)
        {
            return new List<Revision>() { 
                new Revision() {
                    Id = new Guid("12345678-abcd-abcd-1234-12345678abcd"),
                    CompoundId = new Guid("23232323-1111-1111-1111-232323232323") }
            };
        }

        public override IEnumerable<Guid> GetSubstanceIds(Guid compoundId)
        {
            throw new NotImplementedException();
        }

        public MockCompoundStore(IPropertyStore propertyStore)
        {

        }
	}

}
