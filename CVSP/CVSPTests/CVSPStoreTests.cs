using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RSC.CVSP.EntityFramework;
using RSC.CVSP;
using RSC.CVSP.Compounds;
using System.Linq;
using System.Resources;
using System.Reflection;
using RSC.Logging;

namespace CVSPTests
{
	/// <summary>
	/// Summary description for CVSPStoreTests
	/// </summary>
	[TestClass]
	public class CVSPStoreTests : CVSPTestBase
	{
		public CVSPStoreTests()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion
		/*
		[TestMethod]
		public void EntityFrameworkTest()
		{
			CVSPStore db = CVSPStore;

			//create user profile
			RSC.CVSP.UserProfile up = new RSC.CVSP.UserProfile()
			{
				SendEmail = true,
				FtpDirectory = "PathFtpDir",
				Id = Guid.Parse("e4dd745a-1894-437f-869f-859931a59ac7")
			};
			//delete in case it is present
			db.DeleteUserProfile(Guid.Parse("e4dd745a-1894-437f-869f-859931a59ac7"));

			Guid userGuid = db.CreateUserProfile(up);
			//validate the created user profile
			RSC.CVSP.UserProfile up_res = db.GetUserProfile(userGuid);
			Assert.IsTrue(up_res != null);
			Assert.IsTrue(up_res.SendEmail == true);
			Assert.IsTrue(up_res.FtpDirectory.Equals("PathFtpDir"));

			//add user variables
			List<RSC.CVSP.UserVariable> UserVariableCollection = new List<RSC.CVSP.UserVariable>() {
					new RSC.CVSP.UserVariable() {Name = "var1", Value="value1", Description="descr1"} ,
					new RSC.CVSP.UserVariable() {Name = "var2", Value="value2", Description="descr2"} 
			};
			db.CreateUserVariables(userGuid, UserVariableCollection);


			//create user content
			RSC.CVSP.RuleSet rs1 = new RSC.CVSP.RuleSet(userGuid, RuleType.Validation, "Default Validation RuleSet", "Validation RuleSet", RSC.CVSP.Compounds.Properties.Resources.validation);
			
			Guid rs1_guid = db.CreateRuleSet(userGuid, rs1);
			RSC.CVSP.RuleSet rs1_res = db.GetRuleSet(rs1_guid);
			rs1_res.IsPlatformDefault = true;
			rs1_res.Title = "Updated title";
			db.UpdateRuleSet(userGuid, rs1_res);
			RSC.CVSP.RuleSet rs1_update = db.GetRuleSet(rs1_guid);
			Assert.IsTrue(rs1_update.Title.Equals("Updated title"));

			RSC.CVSP.RuleSet rs2 = new RSC.CVSP.RuleSet(userGuid, RuleType.AcidBase, "Default AcidBase RuleSet", "AcidBase RuleSet", RSC.CVSP.Compounds.Properties.Resources.acidgroups);
			Guid rs2_guid = db.CreateRuleSet(userGuid, rs2);
			RSC.CVSP.RuleSet rs2_res = db.GetRuleSet(rs2_guid);
			rs2_res.IsPlatformDefault = true;
			db.UpdateRuleSet(userGuid, rs2_res);

			RSC.CVSP.RuleSet rs3 = new RSC.CVSP.RuleSet(userGuid, RuleType.Standardization, "Default Standardization RuleSet", "Standardization RuleSet", RSC.CVSP.Compounds.Properties.Resources.standardization);
			Guid rs3_guid = db.CreateRuleSet(userGuid, rs3);
			RSC.CVSP.RuleSet rs3_res = db.GetRuleSet(rs3_guid);
			rs3_res.IsPlatformDefault = true;
			db.UpdateRuleSet(userGuid, rs3_res);

			//create deposition
			//RSC.CVSP.ProcessingParameters processingOptions = new RSC.CVSP.ProcessingParameters(false, db.GetDefaultRuleSet(RuleType.Validation).Id,
			//	db.GetDefaultRuleSet(RuleType.AcidBase).Id, db.GetDefaultRuleSet(RuleType.Standardization).Id);
			//Dictionary<string, SDTagOptions> dict = new Dictionary<string, SDTagOptions>();
			//dict["regid"] = SDTagOptions.DEPOSITOR_SUBSTANCE_REGID;
			//dict["name"] = SDTagOptions.DEPOSITOR_SUBSTANCE_SYNONYM;
			RSC.CVSP.Deposition deposition = new RSC.CVSP.Deposition(userGuid, DataDomain.Substances);

			Guid depGuid = db.CreateDeposition(deposition);
			Assert.IsTrue(depGuid != null);
			//check that deposition is created
			RSC.CVSP.Deposition dep_res = db.GetDeposition(depGuid);
			Assert.IsTrue(dep_res != null);
			//Assert.IsTrue(dep_res.SDFMappedPropertyCollection.Count == 2);	//	AP - we do not have SDF mapped properties yet
			Assert.IsTrue(dep_res.Status == RSC.CVSP.DepositionStatus.Locked);
			Assert.IsTrue(dep_res.UserGuid == userGuid);
			Assert.IsTrue(dep_res.DateSubmitted != null);

			//add records
			List<RSC.CVSP.Record> r_list = new List<RSC.CVSP.Record>();
			var r1 = new RSC.CVSP.Compounds.CompoundRecord()
			{
				Ordinal = 1,
				File = new RSC.CVSP.DepositionFile() {
					//Path = "C:/depositions/k/",
					Name = "1.sdf"
				},
				DataDomain = RSC.CVSP.DataDomain.Substances,
				//ExternalId = "kaka1",
				Original = "sdf StandardizedContent",
				//StdInchi = new RecordInchis() { OriginalInchi = "InChI=1S/C6H6/c1-2-4-6-5-3-1/h1-6H", OriginalInchiKey = "UHOVQNZJYSORNB-UHFFFAOYSA-N" },
				//Smiles = new RecordSmiles() { OriginalSmiles = "c1ccccc1", StandardizedSmiles = "c1ccccc1" }
			};
			r_list.Add(r1);
			var r_guids = db.CreateRecords(depGuid, r_list);
			Assert.IsTrue(r_guids.Count() == 1);
			//check files were created
			var retrievedFiles = db.GetDepositionFiles(depGuid).ToList();
			Assert.IsTrue(retrievedFiles.Count() == 1);

			//add issues to the record
			List<Issue> issues = new List<Issue>()
				{
					new Issue() 
					{ 
						Code = "200.3",
					},
					new Issue() 
					{ 
						Code = "200.17",
						Message =  "description 2",
						AuxInfo = "bad record 2",
					}
				};
			db.CreateRecordIssues(r_guids.First(), issues);

			//add record reaction extras
			RSC.CVSP.ReactionExtras reactionExtras = new RSC.CVSP.ReactionExtras()
			{
				OriginalValidationScore=50,
				OriginalMappedToUnmappedRation=60
			};
			//db.CreateRecordReactionExtras(r_guids.First(), reactionExtras);
			//RSC.CVSP.ReactionExtras reactionExtras_result = db.GetRecordReactionExtras(r_guids.First());
			//Assert.IsTrue(reactionExtras_result.OriginalValidationScore == 50);

			//add record annotation
			//IEnumerable<RSC.CVSP.RecordAnnotation> AnnotationCollection = new List<RSC.CVSP.RecordAnnotation>()
			//	{
			//		new RSC.CVSP.RecordAnnotation()
			//		{
			//			Type = RSC.CVSP.AnnotationType.InchiKey,
			//			Value = "UHOVQNZJYSORNB-UHFFFAOYSA-N"
			//		},
			//		new RSC.CVSP.RecordAnnotation()
			//		{
			//			Type = RSC.CVSP.AnnotationType.Inchi,
			//			Value = "InChI=1S/C6H6/c1-2-4-6-5-3-1/h1-6H"
			//		}
			//	};
			//db.CreateRecordAnnotations(r_guids.First(), AnnotationCollection);
					

			//update deposiiton
			//dep_res.TotalNumOfRecords = 100;
			dep_res.Status = DepositionStatus.Failed;
			dep_res.IsPublic = true;
			Dictionary<string, SDTagOptions> dict2 = new Dictionary<string, SDTagOptions>();
			dict2["regid_changed"] = SDTagOptions.DEPOSITOR_SUBSTANCE_REGID;
			dict2["name_changed"] = SDTagOptions.DEPOSITOR_SUBSTANCE_SYNONYM;
			//dep_res.SDFMappedPropertyCollection = dict2;
			//dep_res.ProcessingParameters = new RSC.CVSP.ProcessingParameters(false, db.GetDefaultRuleSet(RuleType.Validation).Id,
			//	db.GetDefaultRuleSet(RuleType.AcidBase).Id, null);
			
			db.UpdateDeposition(dep_res);
			RSC.CVSP.Deposition dep2_res = db.GetDeposition(depGuid);
			//Assert.IsTrue(dep2_res.TotalNumOfRecords == 100);
			Assert.IsTrue(dep_res.Status == DepositionStatus.Failed);
			Assert.IsTrue(dep2_res.Id == depGuid);

			db.UpdateDepositionStatus(dep_res.Id, DepositionStatus.Failed);

			//update record
			RSC.CVSP.Record r = db.GetRecord(r_guids.First());
			r.Ordinal = 5;
			//r.IsStandardized = false;
			//r.ExternalId = "kaka updated";
			r.RevisionDate = DateTime.Now;
			db.UpdateRecord(r);

			//delete deposition
			db.DeleteDeposition(depGuid);
			RSC.CVSP.Deposition dep3 = db.GetDeposition(depGuid);
			Assert.IsTrue(dep3 == null);

			//delete user content
			db.DeleteRuleSet(userGuid);

			//delete user profile
			db.DeleteUserProfile(userGuid);
		}

		[TestMethod]
		public void DeleteDepositionRecords()
		{
			var REMOVE_RECORDS_CHUNK = 1000;

			Guid guid = Guid.Parse("815899ee-74ba-4635-b5de-3cfb0b9686c4");

			while (true)
			{
				var guids = CVSPStore.GetDepositionRecords(guid, 0, REMOVE_RECORDS_CHUNK);

				if (!guids.Any())
					break;

				CVSPStore.DeleteRecords(guids);
			}
		}
*/
	}
}
