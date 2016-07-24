using System;
using System.Linq;
using System.Collections.Generic;

namespace RSC.Compounds
{
	public abstract class SubstanceStore
	{
		#region substance getters
		/// <summary>
		/// Returns number of substances registered in the system
		/// </summary>
		/// <returns>Number of total substances</returns>
		public abstract int GetSubstancesCount();

		/// <summary>
		/// get all substances 
		/// </summary>
		/// <param name="start"></param>
		/// <param name="count"></param>
		/// <returns>substance IDs</returns>
		public abstract IEnumerable<Guid> GetSubstances(int start = 0, int count = -1);

		/// <summary>
		/// get substance instances by substance IDs
		/// </summary>
		/// <param name="ids">substance IDs</param>
		/// <param name="start"></param>
		/// <param name="count"></param>
		/// <returns>substance instances</returns>
		public abstract IEnumerable<Substance> GetSubstances(IEnumerable<Guid> ids);

		/// <summary>
		/// get substance instance by substance ID
		/// </summary>
		/// <param name="id">substance ID</param>
		/// <returns>substance instance</returns>
		public Substance GetSubstance(Guid id)
		{
			return GetSubstances(new List<Guid>() { id }).FirstOrDefault();
		}

		/// <summary>
		/// Returns compound ID assigned to substance version
		/// </summary>
		/// <param name="id">substance ID</param>
		/// <param name="version">version ID</param>
		/// <returns>compound ID</returns>
		public abstract Guid GetCompound(Guid id, int? version = null);
/*
		/// <summary>
		/// get substances by datasource guid and external ids
		/// </summary>
		/// <param name="dataSource">datasource guid</param>
		/// <param name="externalIds">external ids</param>
		/// <returns>colleciton of substance guids</returns>
		public abstract IDictionary<string, Guid> GetSubstances(Guid dataSource, IEnumerable<string> externalIds);
*/
		/// <summary>
		/// get substance revision by substance ID and version
		/// if version is null then return last revision
		/// </summary>
		/// <param name="guid">substance ID</param>
		/// <param name="version"></param>
		/// <returns>substance revision instance</returns>
		public abstract Revision GetRevision(Guid id, int? version = null);

		/// <summary>
		/// Returns substance's SDF file
		/// </summary>
		/// <param name="id">Substance ID</param>
		/// <param name="version">Substance's version. If null - returns the latest version</param>
		/// <returns>SDF file</returns>
		public abstract string GetSDF(Guid id, int? version = null);

		#endregion

		#region substance setters

		/// <summary>
		/// uploads chunk of processed data
		/// </summary>
		/// <param name="dataSource">data source guid</param>
		/// <param name="data">IEnumerable of RecordData</param>
		/// <returns></returns>
		//public abstract bool UploadChunk(Guid dataSource, Guid deposition, IEnumerable<RecordData> recordData);

		#endregion

		#region substance deletes
/*		
		/// <summary>
		/// delete substances and all their revisions
		/// </summary>
		/// <param name="guids">substance guids</param>
		/// <returns></returns>
		public abstract bool DeleteSubstances(IEnumerable<Guid> guids);

		/// <summary>
		/// delete a particular substance revision
		/// </summary>
		/// <param name="guid">substance guid</param>
		/// <param name="version">revision version</param>
		/// <returns></returns>
		public abstract bool DeleteRevision(Guid guid, int? version=null);
*/
		/// <summary>
		/// Delete deposition and all compounds from the system
		/// </summary>
		/// <param name="guid">Deposition's guid</param>
		/// <returns>True if operation was successfull</returns>
		public abstract bool DeleteDeposition(Guid guid);

		#endregion
	}

	public interface ISubstanceBulkUpload
	{
        /// <summary>
        /// Uploads chunk of processed data
        /// </summary>
        /// <param name="depositionId">Deposition id</param>
        /// <param name="data">List of RecordData</param>
        /// <returns>Boolean indicating success</returns>
        /// 
		bool BulkUpload(Guid depositionId, IEnumerable<RecordData> data);
	}
}
