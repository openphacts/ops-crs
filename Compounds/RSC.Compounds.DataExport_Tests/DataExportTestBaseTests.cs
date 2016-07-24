using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;
using VDS.RDF;

using RSC.Compounds;
using RSC.Compounds.DataExport;
using RSC.Compounds.EntityFramework;
using RSC.Datasources;
using RSC.Properties;

namespace RSC.Compounds.DataExport_Tests
{
    [TestClass]
    public class DataExportTestBaseTests : DataExportTestBase
    {
        [TestMethod]
        public void MockPropertiesStore_PropertyDefinitionsList()
        {
            Assert.IsNotNull(dataExport.PropertiesStore, "properties store should not be null");
            Assert.IsNotNull(dataExport.PropertiesStore.PropertyDefinitionsList(), "mock property store should not have a null props definition list");
        }

        /// <summary>
        /// This one is more involved.
        /// </summary>
        [TestMethod]
        public void MockPropertiesStore_GetRecordsProperties()
        {
            var revisionIds = new List<Guid>() { new Guid("12345678-abcd-abcd-1234-12345678abcd") };
            var theseRevisions = dataExport.CompoundsStore.GetRevisions(revisionIds, new[] { "Id", "CompoundId" });
            Assert.IsNotNull(theseRevisions, "enumerable of revisions should not be null");
            var direct = propertyStore.GetRecordsProperties(externalIds);
            Assert.IsNotNull(direct, "dictionary of record properties (from propertyStore) should not be null");
            var indirect = dataExport.PropertiesStore.GetRecordsProperties(externalIds);
            Assert.IsNotNull(indirect, "dictionary of record properties (via DataExport) should not be null");
        }
    }
}
