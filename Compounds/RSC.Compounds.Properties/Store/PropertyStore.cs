using System;
using System.Linq;
using System.Collections.Generic;

namespace RSC.Properties
{
	public abstract class PropertyStore
	{
		/// <summary>
		/// Get all record's properties 
		/// </summary>
		/// <param name="start"></param>
		/// <param name="count"></param>
		/// <returns>Properties IDs</returns>
		public abstract IEnumerable<int> GetCompounds(int start = 0, int count = -1);

		/// <summary>
		/// get compounds instances by compound IDs
		/// </summary>
		/// <param name="guids">compound IDs</param>
		/// <returns>compound instances</returns>
		public abstract IEnumerable<Property> GetCompounds(IEnumerable<int> ids);

		/// <summary>
		/// get compound instance by compound ID
		/// </summary>
		/// <param name="guids">compound ID</param>
		/// <returns>compound instance</returns>
		public Compound GetCompound(int id)
		{
			return GetCompounds(new List<int>() { id }).FirstOrDefault();
		}

		/// <summary>
		/// Returns substance IDs
		/// </summary>
		/// <param name="id">compound ID</param>
		/// <returns>substance IDs</returns>
		public abstract IEnumerable<int> GetSubstances(int id);


		/// /// <summary>
		/// get collection of ParentChild using compound ID
		/// </summary>
		/// <param name="guid">compound ID</param>
		/// <returns>collection of ParentChild</returns>
		public abstract IEnumerable<ParentChild> GetParents(int id);


		/// <summary>
		/// get collection of ParentChild using compound ID
		/// </summary>
		/// <param name="guid">compound ID</param>
		/// <returns>collection of ParentChild</returns>
		public abstract IEnumerable<ParentChild> GetChildren(int id);

		/// <summary>
		/// Returns total number of data sources registred in the system
		/// </summary>
		/// <param name="id">Compound ID</param>
		/// <returns>Total number of registred datasources</returns>
		public abstract int GetDatasourcesCount(int id);

		/// <summary>
		/// Returns list of data source GUIDs
		/// </summary>
		/// <param name="id">Compound ID</param>
		/// <param name="start">Index where to start returning datasources from</param>
		/// <param name="coun">Number of returned datasources</param>
		/// <returns>List of GUIDs</returns>
		public abstract IEnumerable<Guid> GetDatasources(int id, int start = 0, int count = 10);

		/// <summary>
		/// Returns compound's MOL file
		/// </summary>
		/// <param name="id">Compound ID</param>
		/// <returns>MOL file</returns>
		public abstract string GetMol(int id);

		/// <summary>
		/// Convert InChI to Compound ID
		/// </summary>
		/// <param name="inchi">Standard on Non standard InChI</param>
		/// <returns>Internal compound ID</returns>
		public abstract IEnumerable<int> InChIToCompoundId(string inchi);
		/// <summary>
		/// Convert InChIKey to Compound ID
		/// </summary>
		/// <param name="inchikey">Standard on Non standard InChIKey</param>
		/// <returns>Internal compound ID</returns>
		public abstract IEnumerable<int> InChIKeyToCompoundId(string inchekey);

/*
		/// <summary>
		/// get Synonym instances by compound guids
		/// </summary>
		/// <param name="guids">compound guids</param>
		/// <returns>Synonyms</returns>
		public abstract IEnumerable<Synonym> GetSynonyms(IEnumerable<Guid> guids);

		//setters
		/// <summary>
		/// create compounds
		/// </summary>
		/// <param name="data">IEnumerable<RecordData></param>
		/// <returns></returns>
		public abstract bool CreateCompounds(IEnumerable<Compound> data);
		public abstract bool CreateCompounds2(IEnumerable<Compound> data);

		/// <summary>
		/// create parent-child relations
		/// </summary>
		/// <param name="relations">EInumerable of ChemSpider.Compounds.Core.ParentChild</param>
		/// <returns></returns>
		public abstract bool CreateParentChildRelations(IEnumerable<RSC.Compounds.ParentChild> relations);
		
		/// <summary>
		/// create synonyms by compound guid and instances of Synonym
		/// </summary>
		/// <param name="guid">compound guid</param>
		/// <param name="synonyms"></param>
		public abstract void CreateSynonyms(Guid guid, IEnumerable<Synonym> synonyms);

		/// <summary>
		/// deletes compounds by guids
		/// also deletes associated ParentChild relations, Smiles, properties, and compound synonyms
		/// </summary>
		/// <param name="guids">compound guids</param>
		/// <returns></returns>
		public abstract bool DeleteCompounds(IEnumerable<Guid> guids);
*/
	}
}
