using RSC.Logging;
using RSC.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RSC.CVSP
{
	public interface ICVSPStore
	{
		#region annotations methods

		/// <summary>
		/// get list all annotations registered in the system
		/// </summary>
		/// <returns>list of annotations</returns>
		IEnumerable<Annotation> GetAllAnnotations();

		#endregion

		#region deposition methods

		/// <summary>
		/// get list of registred depositions' GUIDs
		/// </summary>
		/// <param name="status">deposition status</param>
		/// <param name="start">ordinal start number of depositions</param>
		/// <param name="count">counts of depositions to return</param>
		/// <returns>list of depositions' GUIDs</returns>
		IEnumerable<Guid> GetDepositionsByStatus(DepositionStatus status, int start = 0, int count = -1);

		/// <summary>
		/// get list of registred depositions' GUIDs
		/// </summary>
		/// <param name="start">ordinal start number of depositions</param>
		/// <param name="count">counts of depositions to return</param>
		/// <returns>list of depositions' GUIDs</returns>
		IEnumerable<Guid> GetDepositions(int start = 0, int count = -1);
		
		/// <summary>
		/// takes deposition guids and return deposition objects
		/// </summary>
		/// <param name="guids">IEnumerable of depositioin guids</param>
		/// <returns>IEnumerable of objects Deposition</returns>
		IEnumerable<Deposition> GetDepositions(IEnumerable<Guid> guids);

		/// <summary>
		/// takes deposition guid and returns a Deposition object
		/// </summary>
		/// <param name="guid">deposition guid</param>
		/// <returns>Deposition object</returns>
		Deposition GetDeposition(Guid guid);

		/// <summary>
		/// takes user guid and returns deposition guids
		/// </summary>
		/// <param name="guid">user guid</param>
		/// <returns>list of deposition guids</returns>
		//public IEnumerable<Guid> GetUserDepositions(Guid guid)
		//{
		//	return GetDepositions(new List<Guid> { guid }).Where(p => p.UserId == guid).Select(p => p.Id);
		//}

		/// <summary>
		/// returns deposition submitted numberOfDays days ago
		/// </summary>
		/// <param name="days">number of days</param>
		/// <returns>guids of depositions submitted more than numberOfDays ago</returns>
		//IEnumerable<Guid> GetDepositionsByAge(int numberOfDays);

		/// <summary>
		/// creates a deposition and return deposition guid
		/// </summary>
		/// <param name="deposition">deposition object to create</param>
		/// <returns>deposition's guid</returns>
		Guid CreateDeposition(Deposition deposition);

		/// <summary>
		/// Updates Deposition
		/// </summary>
		/// <param name="deposition">Deposition object</param>
		bool UpdateDeposition(Deposition deposition);

		/// <summary>
		/// updates deposition status
		/// </summary>
		/// <param name="guid">deposition guid</param>
		/// <param name="status"></param>
		/// <returns></returns>
		bool UpdateDepositionStatus(Guid guid, DepositionStatus status);
		
		/// <summary>
		/// Delete deposition with all records
		/// </summary>
		/// <param name="guid">deposition guid</param>
		bool DeleteDeposition(Guid guid);

		#endregion

		#region files
		/// <summary>
		/// Get list of deposition's files
		/// </summary>
		/// <param name="guid">deposition guid</param>
		/// <returns>List if deposition files</returns>
		IEnumerable<DepositionFile> GetDepositionFiles(Guid guid);

		#endregion

		#region fields
		/// <summary>
		/// Returns list of deposition's fields
		/// </summary>
		/// <param name="guid">deposition guid</param>
		/// <returns>List of fields</returns>
		IEnumerable<Field> GetDepositionFields(Guid guid);
		#endregion

		#region annotation
		/// <summary>
		/// Returns list of deposition's annotated fields
		/// </summary>
		/// <param name="guid">deposition guid</param>
		/// <returns>List of annotated fields</returns>
		IEnumerable<Field> GetDepositionAnnotations(Guid guid);

		/// <summary>
		/// Annotate deposition field
		/// </summary>
		/// <param name="guid">Deposition's GUID</param>
		/// <param name="field">Deposition's field name</param>
		/// <param name="annotation">Annotation's name</param>
		/// <returns></returns>
		bool AnnotateDepositionField(Guid guid, string field, string annotation);

		/// <summary>
		/// Delete deposition's annotation
		/// </summary>
		/// <param name="guid">Deposition's GUID</param>
		/// <param name="annotation">Annotation's name</param>
		/// <returns>True if operation was successfull</returns>
		bool DeleteDepositionAnnotation(Guid guid, string annotation);
		#endregion

		#region records methods

		/// <summary>
		/// returns number of records in deposition
		/// </summary>
		/// <param name="guid">deposition guid</param>
		/// <returns>number of records</returns>
		int GetDepositionRecordsCount(Guid guid);

		/// <summary>
		/// returns record guids by deposition guid, starting ordinal and count of records
		/// </summary>
		/// <param name="guid">deposition guid</param>
		/// <param name="start">ordinal start number of records</param>
		/// <param name="count">counts of records to return</param>
		/// <returns>record guids</returns>
		IEnumerable<Guid> GetDepositionRecords(Guid guid, int start = 0, int count = -1);

		/// <summary>
		/// Get Record objects for list of record guids
		/// </summary>
		/// <param name="guids">IEnumerable of record guids</param>
		/// <param name="full">Return full descriptive information about records. Default - false</param>
		/// <returns>Record collection</returns>
		IEnumerable<Record> GetRecords(IEnumerable<Guid> guids, IEnumerable<string> filter = null);
		Record GetRecord(Guid guid);

		/// <summary>
		/// adds records to deposition
		/// </summary>
		/// <param name="depositionGuid">deposition guid</param>
		/// /// <param name="fileGuid">file guid</param>
		/// <param name="records">list of records to add</param>
		/// <returns>list of new records' guids</returns>
		IEnumerable<Guid> CreateRecords(Guid depositionGuid, IEnumerable<Record> records);

		/// <summary>
		/// Updates a record: deletes a record by record guid and then creates it
		/// </summary>
		/// <param name="depositionGuid">deposition guid</param>
		/// <param name="record">Record object</param>
		/// <returns>new guid for the record</returns>
		bool UpdateRecord(Record record);

		/// <summary>
		/// Delete records by record guids
		/// </summary>
		/// <param name="guid">record guids</param>
		bool DeleteRecords(IEnumerable<Guid> guid);

		#region Record Issues
		/// <summary>
		/// Get Record issues
		/// </summary>
		/// <param name="guids">Record guids</param>
		/// <returns>Issues collection</returns>
		IEnumerable<Issue> GetRecordIssues(Guid guid);
		#endregion

		#region Record Fields
		/// <summary>
		/// Get record fields
		/// </summary>
		/// <param name="guid">Record guid</param>
		/// <returns>Fields collection</returns>
		IEnumerable<RecordField> GetRecordFields(Guid guid);
		#endregion

		#region Record Properties
		/// <summary>
		/// Get record properties
		/// </summary>
		/// <param name="guid">Record guid</param>
		/// <returns>Properties collection</returns>
		IEnumerable<Guid> GetRecordProperties(Guid guid);
		#endregion

		#endregion

		#region user profile
		/// <summary>
		/// Get UserProfile by profile guid
		/// </summary>
		/// <param name="guid">profile guid</param>
		/// <returns>UserProfile object</returns>
		UserProfile GetUserProfile(Guid guid);

		/// <summary>
		/// Create UserProfile by object
		/// </summary>
		/// <param name="guid"></param>
		/// <returns>UserProfile guid</returns>
		Guid CreateUserProfile(UserProfile profile);

		/// <summary>
		/// Delete CVSP's User Profile by guid
		/// </summary>
		/// <param name="guid">user guid</param>
		/// <returns></returns>
		bool DeleteUserProfile(Guid guid);

		/// <summary>
		/// Update User Profile
		/// </summary>
		/// <param name="profile">UserProfile object</param>
		bool UpdateUserProfile(UserProfile profile);

		/// <summary>
		/// returns all user profiles 
		/// </summary>
		/// <returns></returns>
		[Obsolete("Very bad practice. This method should be removed")]
		IEnumerable<UserProfile> GetUserProfiles();

		#endregion

		#region user variables
		/// <summary>
		/// get variables by user guid
		/// </summary>
		/// <param name="guid">user guid</param>
		/// <returns>list of variables</returns>
		IEnumerable<UserVariable> GetUserVariables(Guid guid);

		/// <summary>
		/// update user variables by user guid. Will update values of variables based on variable name
		/// </summary>
		/// <param name="guid">user guid</param>
		/// <param name="uv_list"></param>
		/// <returns>list of variable guids</returns>
		bool UpdateUserVariables(Guid guid, IEnumerable<UserVariable> uv_list);

		/// <summary>
		/// adds variables by user guid and list of variables. If variable exists (variable Name is present) it will be updated
		/// </summary>
		/// <param name="guid">user guid</param>
		/// <param name="uv_list"></param>
		/// <returns>list of variable guids</returns>
		bool CreateUserVariables(Guid guid, IEnumerable<UserVariable> uv_list);
		/// <summary>
		/// deletes variables by user guid
		/// </summary>
		/// <param name="guid">user guid</param>
		bool DeleteUserVariables(Guid guid);


		#endregion

		#region user content
		/// <summary>
		/// get user content by content guid
		/// </summary>
		/// <param name="guid">content guid</param>
		/// <returns>UserContent object</returns>
		RuleSet GetRuleSet(Guid guid);

		RuleSet GetDefaultRuleSet(RuleType type);

		/// <summary>
		/// creating user content
		/// </summary>
		/// <param name="content"></param>
		/// <returns>content guid</returns>
		Guid CreateRuleSet(Guid userGuid, RuleSet content);

		/// <summary>
		/// updating user content
		/// </summary>
		/// <param name="content"></param>
		bool UpdateRuleSet(Guid userGuid, RuleSet content);

		/// <summary>
		/// deleting user content by content guid
		/// </summary>
		/// <param name="guid">content guid</param>
		bool DeleteRuleSet(Guid guid);
		#endregion
	}

	public abstract class CVSPBulkUpload
	{
		/// <summary>
		/// Uploads (create/update) chunk of processed records
		/// </summary>
		/// <param name="dataSource">Data source guid</param>
		/// <param name="data">List of processed records</param>
		/// <returns>True if all records uploaded successfully</returns>
		public abstract bool BulkUpload(Guid datasource, IEnumerable<Record> records);
	}

}
