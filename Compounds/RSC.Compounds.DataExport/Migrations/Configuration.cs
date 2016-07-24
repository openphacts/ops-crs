using RSC.Compounds.DataExport.EntityFramework;

namespace RSC.Compounds.DataExport
{
	using System;
	using System.Data.Entity.Migrations;

	internal sealed class Configuration : DbMigrationsConfiguration<RSC.Compounds.DataExport.DataExportContext>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = true;
		}

		protected override void Seed(RSC.Compounds.DataExport.DataExportContext context)
		{
			//Add the Data Export versions for each OPS data source, these must be referenced in the VoID information.

			//Add the previous data export version information.

			//ChEBI.
			context.DataVersions.AddOrUpdate(i => i.Id,
									new ef_DataVersion {
										Id = 1,
										UriSpace = "http://purl.obolibrary.org/obo/",
										DataSourceId = new Guid("73834460-e542-46d1-8aa5-b034ea2bf34a"),
										CreatedDate = DateTime.Parse("2013-06-15T00:00:00.000+01:00"),
										DownloadDate = DateTime.Parse("2013-06-15T00:00:00.000+01:00"),
										LicenseUri = "http://creativecommons.org/licenses/by-sa/3.0/",
										VersionName = "104",
										DownloadedBy = "Karen Karapetyan",
										DownloadUri = "ftp://ftp.ebi.ac.uk/pub/databases/chebi/SDF/ChEBI_complete_3star.sdf.gz",
										VoidUri = "https://github.com/openphacts/ops-platform-setup/blob/master/void/chebi/chebi104_void.ttl#chebi"
									});

			//MeSH.
			context.DataVersions.AddOrUpdate(i => i.Id,
									new ef_DataVersion {
										Id = 2,
										UriSpace = "http://purl.bioontology.org/ontology/MSH/",
										DataSourceId = new Guid("4be3451f-d80f-4f7d-8002-149691971594"),
										CreatedDate = DateTime.Parse("2013-07-10T00:00:00.000+01:00"),
										DownloadDate = DateTime.Parse("2013-07-10T00:00:00.000+01:00"),
										LicenseUri = "http://creativecommons.org/licenses/by-sa/3.0/",
										VersionName = "1",
										DownloadedBy = "Jon Steele",
										DownloadUri = "http://www.nlm.nih.gov/mesh/filelist.html",
										VoidUri = "http://MESH_UNKNOWN_VOID#compounds"
									});

			//HMDB.
			context.DataVersions.AddOrUpdate(i => i.Id,
									new ef_DataVersion {
										Id = 3,
										UriSpace = "http://www.hmdb.ca/metabolites/",
										DataSourceId = new Guid("B189BD3D-E8B3-466F-B925-894187341A0B"), //TODO: What is HMDB Guid?
										CreatedDate = DateTime.Parse("2013-07-05T00:00:00.000+01:00"),
										DownloadDate = DateTime.Parse("2013-07-05T00:00:00.000+01:00"),
										LicenseUri = "http://creativecommons.org/licenses/by-sa/3.0/",
										VersionName = "3.5",
										DownloadedBy = "Karen Karapetyan",
										DownloadUri = "http://www.hmdb.ca/downloads/structures.zip",
										VoidUri = "http://HMDB_UNKNOWN_VOID#compounds"
									});

			//Drugbank.
			context.DataVersions.AddOrUpdate(i => i.Id,
									new ef_DataVersion {
										Id = 4,
										UriSpace = "http://www4.wiwiss.fu-berlin.de/drugbank/resource/drugs/",
										DataSourceId = new Guid("047a5b10-e01b-4979-97f3-ca7d9723f5b5"),
										CreatedDate = DateTime.Parse("2013-06-15T00:00:00.000+01:00"),
										DownloadDate = DateTime.Parse("2013-06-15T00:00:00.000+01:00"),
										LicenseUri = "http://creativecommons.org/licenses/by-sa/3.0/",
										VersionName = "3",
										DownloadedBy = "Karen Karapetyan",
										DownloadUri = "http://www.drugbank.ca/system/downloads/current/structures/all.sdf.zip",
										VoidUri = "https://github.com/openphacts/ops-platform-setup/blob/master/void/drugbank_void.ttl#db-drugs"
									});

			//ChEMBL.
			context.DataVersions.AddOrUpdate(i => i.Id,
									new ef_DataVersion {
										Id = 5,
										UriSpace = "http://rdf.ebi.ac.uk/resource/chembl/molecule/",
										DataSourceId = new Guid("3384b886-67ea-4793-8240-5af5d033d5fc"),
										CreatedDate = DateTime.Parse("2013-06-15T00:00:00.000+01:00"),
										DownloadDate = DateTime.Parse("2013-06-15T00:00:00.000+01:00"),
										LicenseUri = "http://creativecommons.org/licenses/by-sa/3.0/",
										VersionName = "16",
										DownloadedBy = "Karen Karapetyan",
										DownloadUri = "ftp://ftp.ebi.ac.uk/pub/databases/chembl/ChEMBLdb/releases/chembl_16/chembl_16.sdf.gz",
										VoidUri = "http://linkedchemistry.info/void.ttl#chemblrdf_compounds"
									});

			//PDB.
			context.DataVersions.AddOrUpdate(i => i.Id,
									new ef_DataVersion {
										Id = 6,
										UriSpace = "http://purl.uniprot.org/pdb/",
										DataSourceId = new Guid("8dfae34b-fa65-4c71-88fb-79a9ed55b09b"),
										CreatedDate = DateTime.Parse("2013-07-10T00:00:00.000+01:00"),
										DownloadDate = DateTime.Parse("2013-07-10T00:00:00.000+01:00"),
										LicenseUri = "http://creativecommons.org/licenses/by-sa/3.0/",
										VersionName = "1",
										DownloadedBy = "Karen Karapetyan",
										DownloadUri = "http://ligand-expo.rcsb.org/dictionaries/Components-pub.sdf.gz",
										VoidUri = "http://PDB_UNKNOWN_VOID#compounds"
									});

			//Add all the previous exports - get the GUIDs from Data Sources database.

			//CHEBI.
			context.Logs.AddOrUpdate(i => i.Id,
									new ef_DataExportLog {
										Id = 1,
										DataVersionId = 1,
										DataSourceId = new Guid("73834460-e542-46d1-8aa5-b034ea2bf34a"),
										ExportDate = DateTime.Parse("2013-11-11T15:44:03Z"),
										ErrorMessage = null,
										ExportDirectory = "20131111/CHEBI",
										Status = DataExportStatus.Succeeded,
									});

			//MESH.
			context.Logs.AddOrUpdate(i => i.Id,
									new ef_DataExportLog {
										Id = 2,
										DataVersionId = 2,
										DataSourceId = new Guid("4be3451f-d80f-4f7d-8002-149691971594"),
										ExportDate = DateTime.Parse("2013-11-11T15:00:12Z"),
										ErrorMessage = null,
										ExportDirectory = "20131111/MESH",
										Status = DataExportStatus.Succeeded,
									});

			//HMDB.
			context.Logs.AddOrUpdate(i => i.Id,
						 new ef_DataExportLog {
							 Id = 3,
							 DataVersionId = 3,
							 DataSourceId = new Guid("B189BD3D-E8B3-466F-B925-894187341A0B"),
							 ExportDate = DateTime.Parse("2013-11-11T16:14:21Z"),
							 ErrorMessage = null,
							 ExportDirectory = "20131111/HMDB",
							 Status = DataExportStatus.Succeeded,
						 });

			//Drugbank.
			context.Logs.AddOrUpdate(i => i.Id,
			 new ef_DataExportLog {
				 Id = 4,
				 DataVersionId = 4,
				 DataSourceId = new Guid("047a5b10-e01b-4979-97f3-ca7d9723f5b5"),
				 ExportDate = DateTime.Parse("2013-11-11T15:39:51Z"),
				 ErrorMessage = null,
				 ExportDirectory = "20131111/DRUGBANK",
				 Status = DataExportStatus.Succeeded,
			 });

			//ChEMBL.
			context.Logs.AddOrUpdate(i => i.Id,
			 new ef_DataExportLog {
				 Id = 5,
				 DataVersionId = 5,
				 DataSourceId = new Guid("3384b886-67ea-4793-8240-5af5d033d5fc"),
				 ExportDate = DateTime.Parse("2013-11-11T12:43:14Z"),
				 ErrorMessage = null,
				 ExportDirectory = "20131111/CHEMBL",
				 Status = DataExportStatus.Succeeded,
			 });

			//PDB.
			context.Logs.AddOrUpdate(i => i.Id,
			 new ef_DataExportLog {
				 Id = 6,
				 DataVersionId = 6,
				 DataSourceId = new Guid("8dfae34b-fa65-4c71-88fb-79a9ed55b09b"),
				 ExportDate = DateTime.Parse("2013-11-11T14:40:54Z"),
				 ErrorMessage = null,
				 ExportDirectory = "20131111/PDB",
				 Status = DataExportStatus.Succeeded,
			 });
		}
	}
}
