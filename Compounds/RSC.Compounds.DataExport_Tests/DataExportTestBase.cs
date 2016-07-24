using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Moq;

using Autofac;
using Autofac.Extras.CommonServiceLocator;
using Microsoft.Practices.ServiceLocation;

using RSC;
using RSC.Compounds;
using RSC.Compounds.DataExport;
using RSC.Compounds.EntityFramework;
using RSC.Datasources;
using RSC.Properties;
using RSC.Properties.EntityFramework;
using RSC.Logging;

namespace RSC.Compounds.DataExport_Tests
{
	public class DataExportTestBase
	{
		public DataExport.DataExport dataExport;
		public Dictionary<int, Guid> dataSources;
		public static ExternalReferenceType opsReferenceType;
		public static ExternalReferenceType csReferenceType;
		public Guid dsGuid;
		public IDataExportStore dataExportStore;
		public IDataSourcesClient dataSourcesClient;
		public IEnumerable<ExternalId> externalIds;
		public List<Property> properties;
		public List<PropertyDefinition> propertyDefs;
		public IEnumerable<Revision> revisions;
		public IEnumerable<string> propertyNames;
		public IPropertyStore propertyStore;

		/// <summary>
		/// Put in separate function for readability.
		/// </summary>
		/// <returns></returns>
		public IDataSourcesClient MockDataSourcesClient()
		{
			var mock_dsc = new Mock<IDataSourcesClient>();
			mock_dsc.Setup(c => c.GetDataSource(dsGuid)).Returns(new DataSource()
			{
				Guid = dsGuid,
				Name = "test"
			});
			return mock_dsc.Object;
		}

		public IDataExportStore MockDataExportStore()
		{
			List<string> filenames = new List<string>() {
				"LINKSET_EXACT_TEST00010101.ttl",
				"LINKSET_EXACT_OPS_CHEMSPIDER_TEST00010101.ttl",
				"LINKSET_CLOSE_PARENT_CHILD_CHARGE_INSENSITIVE_PARENT_TEST00010101.ttl",
				"LINKSET_CLOSE_PARENT_CHILD_ISOTOPE_INSENSITIVE_PARENT_TEST00010101.ttl",
				"LINKSET_CLOSE_PARENT_CHILD_STEREO_INSENSITIVE_PARENT_TEST00010101.ttl",
				"LINKSET_CLOSE_PARENT_CHILD_SUPER_INSENSITIVE_PARENT_TEST00010101.ttl",
				"LINKSET_CLOSE_PARENT_CHILD_TAUTOMER_INSENSITIVE_PARENT_TEST00010101.ttl",
				"LINKSET_RELATED_PARENT_CHILD_FRAGMENT_TEST00010101.ttl"};
			var mock_dataexportstore = new Mock<RSC.Compounds.DataExport.IDataExportStore>();
			mock_dataexportstore.Setup(c => c.GetCurrentDataVersion(dsGuid)).Returns(new DataVersion()
			{
				VoidUri = "http://www.rsc.org/void.ttl",
				UriSpace = "http://www.rsc.org/"
			});
			mock_dataexportstore.Setup(d => d.GetDataExportLog(22)).Returns(new DataExportLog()
			{
				Files = filenames.Select(n => new DataExportLogFile() { FileName = n, RecordCount = 0 }),
			});
			return mock_dataexportstore.Object;
		}

		public DataExportTestBase()
		{
			opsReferenceType = new ExternalReferenceType() { UriSpace = Constants.OPSUriSpace };
			csReferenceType = new ExternalReferenceType() { UriSpace = Constants.CSUriSpace };

			dsGuid = new Guid("1441d974-bd2c-44ae-8b27-110089dc8e44");
			dataSources = new Dictionary<int, Guid>() { { 22, dsGuid } };
			dataSourcesClient = MockDataSourcesClient();
			
			revisions = new List<Revision>() { 
				new Revision() {
					Id = new Guid("12345678-abcd-abcd-1234-12345678abcd"),
					Substance = new Substance() { 
						DataSourceId = dsGuid,                        
						ExternalIdentifier = "CHEBI_12345" } 
				} };
			externalIds = revisions.Select(r => new RSC.ExternalId() { DomainId = 1, ObjectId = r.Id });

			propertyStore = MockPropertyStore();

			dataExport = new DataExport.DataExport()
			{
				DownloadUrl = "ftp://ftp.rsc.org/ops/",
				DataExportStore =  MockDataExportStore(),
				CompoundsStore = new MockCompoundStore(propertyStore),
				PropertiesStore = propertyStore,
				LogStore = new Mock<ILogStore>().Object
			};
			//
			// either get this from the database or fake it up.
			//
			//Guid csMagicNumber = PropertyManager.Current.GetChemSpiderProvenanceId();
			Guid csMagicNumber = new Guid("87654321-abcd-abcd-abcd-12345678abcd");
			properties = Properties(csMagicNumber);

			// Lastly register the Property store with the service locator.
			var builder = new ContainerBuilder();
			builder.RegisterInstance(propertyStore).As<Properties.IPropertyStore>();
			var container = builder.Build();
			ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(container));
		}

		public List<Property> Properties(Guid csMagicNumber)
		{
			return new List<Property>()
			{
				// one without an error
				new Property() { Name = PropertyName.FLASH_POINT, Value = 300, 
					ProvenanceId = csMagicNumber},
				// one with an error
				new Property() { Name = PropertyName.LOG_P, Value = 5, 
					ProvenanceId = csMagicNumber },
				// one without an error but a condition
				new Property() { Name = PropertyName.LOG_D, Value = 3, 
					ProvenanceId = csMagicNumber,
					Conditions = new List<Condition>() { new Condition() { Name = PropertyName.PH, Value = 7.4 }}},
				// one with an error and a condition
				new Property() { Name = PropertyName.KOC, Value = 7, Error = 1, 
					ProvenanceId = csMagicNumber,
					Conditions = new List<Condition>() { new Condition() { Name = PropertyName.PH, Value = 5.5 }}}
			};
		}

		public IPropertyStore MockPropertyStore()
		{
			propertyDefs = new List<PropertyDefinition>()
			{
				new PropertyDefinition() { Name = PropertyName.BOILING_POINT, DisplayName = "boiling point", ValueType = PropertyValueType.Double },
				new PropertyDefinition() { Name = PropertyName.FLASH_POINT, DisplayName = "flash point",
					 ValueType = PropertyValueType.Double },
				new PropertyDefinition() { Name = PropertyName.KOC, DisplayName = "soil sorption partition coefficient",
					 ValueType = PropertyValueType.Double },
				new PropertyDefinition() { Name = PropertyName.LOG_D, DisplayName = "log D", ValueType = PropertyValueType.Double },
				new PropertyDefinition() { Name = PropertyName.LOG_P, DisplayName = "log P", ValueType = PropertyValueType.Double }
			};

			var mock_propertyStore = new Mock<IPropertyStore>();
			mock_propertyStore.Setup(p => p.PropertyDefinitionsList()).Returns(propertyDefs);
			mock_propertyStore.Setup(p => p.GetRecordsProperties(externalIds, null))
				.Returns(new Dictionary<ExternalId, IEnumerable<Property>>() { { 
								  externalIds.First(), properties}});
			return mock_propertyStore.Object;
		}
	}
}
