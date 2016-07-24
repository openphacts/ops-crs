using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using ChemSpider.Database;
using ChemSpider.Molecules;
using System.Text.RegularExpressions;
using RSC.Compounds.Old;

namespace RSC.Compounds.Database
{
	public class SQLCompoundsStore : ICompoundsStore
	{
		private readonly ISubstancesStore substancesStore;

		public SQLCompoundsStore(ISubstancesStore substancesStore)
		{
			if (substancesStore == null)
			{
				throw new ArgumentNullException("substancesStore");
			}

			this.substancesStore = substancesStore;
		}

		/// <summary>
		/// Returns compound by Id
		/// </summary>
		/// <param name="id">Internal compound ID</param>
		/// <param name="peer_id">Id of related compound that should be used to narrow the information</param>
		/// <returns>Compound object</returns>
		public RSC.Compounds.Old.Compound GetCompound(int id, int? peer_id = null)
		{
			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				//	check that compound exists
				int compoundId = conn.ExecuteScalar<int>("select csid from compounds where csid = @id", new { id = id });
				if (compoundId == 0)
					return null;

				//	get list of substances... 
				List<int> substances = conn.FetchColumn<int>("select [sid] from sid_csid where csid = @id", new { id = id });

				//	get list of types...
				List<int> types = conn.FetchColumn<int>(@"
							select distinct parent_type_id
							from parent_children
							where parent_id = @id 
								and (@peer_id is null or child_id = @peer_id)", new { id = id, peer_id = peer_id });

				var compound = new RSC.Compounds.Old.Compound()
				{
					Substances = substances,
					CompoundTypes = (from t in types select (CompoundType)t).ToList()
				};

				//	get other properties...
				conn.ExecuteReader(String.Format(@"
					select  compound_properties.mf_formatted,
							isnull(compound_properties.mw_indigo, 0) as mw_indigo,
							isnull(compound_properties.monoisotopicMass_indigo, 0) as monoisotopicMass_indigo, 
							isnull(compound_properties.mostAbundantMass_indigo, 0) as mostAbundantMass_indigo, 
							isnull(compound_properties.mw_oe, 0) as mw_oe, 
							isnull(compound_properties.monoisotopicMass_oe, 0) as monoisotopicMass_oe, 
							compound_smiles.indigo_abs_smiles,
							compound_smiles.oe_abs_smiles,
							nonstd_inchi.inchi as non_std_inchi,
							nonstd_inchi.inchi_key as non_std_inchi_key,
							nonstd_taut_inchi.inchi as non_std_taut_inchi,
							nonstd_taut_inchi.inchi_key as non_std_taut_inchi_key,
							std_inchi.inchi as std_inchi,
							std_inchi.inchi_key as std_inchi_key
					from compounds c
						left outer join compound_smiles on c.csid = compound_smiles.csid
						left outer join compound_properties on compound_properties.csid = c.csid
						left outer join inchi as nonstd_inchi on c.inchi_id_fixedh = nonstd_inchi.inc_id
						left outer join inchi as nonstd_taut_inchi on c.inchi_id_taut = nonstd_taut_inchi.inc_id
						left outer join inchi as std_inchi on c.inchi_id_std = std_inchi.inc_id
					where c.csid = {0}", id),
					r =>
					{
						compound.COMPOUND_ID = id;
						compound.MolecularFormula = MolecularFormula.prepareMF(r["mf_formatted"] as string);
						compound.Indigo_MolecularWeight = (double)r["mw_indigo"];
						compound.Indigo_MonoisotopicMass = (double)r["monoisotopicMass_indigo"];
						compound.Indigo_MostAbundantMass = (double)r["mostAbundantMass_indigo"];
						compound.OE_MolecularWeight = (double)r["mw_oe"];
						compound.OE_MonoisotopicMass = (double)r["monoisotopicMass_oe"];
						compound.INDIGO_CANONICAL_SMILES = r["indigo_abs_smiles"] as string;
						compound.OPENEYE_ABS_SMILES = r["oe_abs_smiles"] as string;
						compound.NON_STD_INCHI = r["non_std_inchi"] as string;
						compound.NON_STD_INCHI_KEY = r["non_std_inchi_key"] as string;
						compound.NON_STD_TAUT_INCHI = r["non_std_taut_inchi"] as string;
						compound.NON_STD_TAUT_INCHI_KEY = r["non_std_taut_inchi_key"] as string;
						compound.STD_INCHI = r["std_inchi"] as string;
						compound.STD_INCHI_KEY = r["std_inchi_key"] as string;
					});

				//var sdfRaw = conn.ExecuteScalar<byte[]>("select mol from compound_mol where csid = @id", new { id = id });
				var sdf = conn.ExecuteScalar<string>("select mol from compound_mol where csid = @id", new { id = id });

				//if (sdfRaw != null) {
				//string sdf = System.Text.Encoding.UTF8.GetString(sdfRaw);
				if (!String.IsNullOrWhiteSpace(sdf))
				{
					SdfRecord _sdf = SdfRecord.FromString(sdf);
					if (_sdf.Properties != null)
						_sdf.Properties.Clear();

					compound.MOL = _sdf.Mol;
				}
				//}

				return compound;
			}
		}

		/// <summary>
		/// Returns compound's SDF in binary format
		/// </summary>
		/// <param name="id">Compound ID</param>
		/// <returns>SDF in binary format</returns>
		public string GetSDF(int id)
		{
			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				return conn.ExecuteScalar<string>("select mol from compound_mol where csid = @id", new { id = id });
			}
		}

		/// <summary>
		/// Returns compounds' IDs page by page
		/// </summary>
		/// <param name="start">Index where to start returning compounds from</param>
		/// <param name="coun">Number of returned compounds</param>
		/// <returns>List of compounds' IDs</returns>
		public IEnumerable<int> GetCompoundsID(int start = 0, int count = 10)
		{
			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				return conn.FetchColumn<int>(@"with cte as (
													SELECT csid, ROW_NUMBER() OVER(ORDER BY csid) as rn
													FROM compounds
													WHERE isSubstanceDerived = 1
												)
												SELECT csid FROM cte 
												WHERE rn BETWEEN @start and @start + @count - 1", new { start = start + 1, count = count });
			}
		}

		/// <summary>
		/// Returns compounds page by page
		/// </summary>
		/// <param name="start">Index where to start returning compounds from</param>
		/// <param name="coun">Number of returned compounds</param>
		/// <returns>List of compounds</returns>
		public IEnumerable<RSC.Compounds.Old.Compound> GetCompounds(int start = 0, int count = 10)
		{
			var ids = GetCompoundsID(start, count);

			return from id in ids select GetCompound(id);
		}

		/// <summary>
		/// Returns list of fragments
		/// </summary>
		/// <param name="id">Compound ID</param>
		/// <returns>List of fragments</returns>
		public IEnumerable<RSC.Compounds.Old.Compound> GetFragments(int id)
		{
			var fragments = new List<RSC.Compounds.Old.Compound>();

			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				foreach (var fragmentId in conn.FetchColumn<int>("select distinct child_id from parent_children where parent_id = @id and rel_type_id = 0", new { id = id }))
				{
					var fragment = GetCompound(fragmentId, id);

					fragments.Add(fragment);
				}
			}

			return fragments;
		}

		/// <summary>
		/// Returns list of children
		/// </summary>
		/// <param name="id">Compound ID</param>
		/// <returns>List of children</returns>
		public IEnumerable<RSC.Compounds.Old.Compound> GetChildren(int id)
		{
			var children = new List<RSC.Compounds.Old.Compound>();

			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				foreach (var childId in conn.FetchColumn<int>("select distinct child_id from parent_children where parent_id = @id", new { id = id }))
				{
					var child = GetCompound(childId, id);

					children.Add(child);
				}
			}

			return children;
		}

		/// <summary>
		/// Returns list of parents
		/// </summary>
		/// <param name="id">Compound ID</param>
		/// <returns>List of parents</returns>
		public IEnumerable<RSC.Compounds.Old.Compound> GetParents(int id)
		{
			var parents = new List<RSC.Compounds.Old.Compound>();

			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				foreach (var parentId in conn.FetchColumn<int>("select distinct parent_id from parent_children where child_id = @id", new { id = id }))
				{
					var parent = GetCompound(parentId, id);

					parents.Add(parent);
				}
			}

			return parents;
		}

		/// <summary>
		/// Returns list of substances
		/// </summary>
		/// <param name="id">Compound ID</param>
		/// <returns>List of substances</returns>
		public IEnumerable<RSC.Compounds.Old.Substance> GetSubstances(int id)
		{
			var substances = new List<RSC.Compounds.Old.Substance>();

			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				foreach (var sid in conn.FetchColumn<int>("select [sid] from sid_csid where csid = @id", new { id = id }))
				{
					var substance = substancesStore.GetSubstance(sid);

					substances.Add(substance);
				}
			}

			return substances;
		}

		public int GetDatasourcesCount(int id)
		{
			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				return conn.ExecuteScalar<int>(@"select count(distinct s.dsn_guid)
												from sid_csid sc join substances s on sc.sid = s.sid
												where sc.csid = @id", new { id = id });
			}
		}

		/// <summary>
		/// Returns list of data sources GUIDs
		/// </summary>
		/// <param name="id">Compound ID</param>
		/// <returns>List of GUIDs</returns>
		public IEnumerable<Guid> GetDatasources(int id, int start = 0, int count = 10)
		{
			if (start == 0 && count == 0)
			{
				using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
				{
					return conn.FetchColumn<Guid>(@"select distinct s.dsn_guid
												from sid_csid sc join substances s on sc.sid = s.sid
												where sc.csid = @id", new { id = id });
				}
			}
			else
			{
				using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
				{
					return conn.FetchColumn<Guid>(@"with cte as (
													SELECT dsn_guid, ROW_NUMBER() OVER(ORDER BY dsn_guid) as rn
													FROM (
														SELECT distinct s.dsn_guid
														FROM sid_csid sc join substances s on sc.sid = s.sid
														WHERE sc.csid = @id
													) t
												)
												SELECT dsn_guid FROM cte 
												WHERE rn BETWEEN @start and @start + @count - 1", new { id = id, start = start + 1, count = count });
				}
			}
		}


		/// <summary>
		/// Returns compound's similarities
		/// </summary>
		/// <param name="id">Compound ID</param>
		/// <param name="threshold">The lower limit of the desired similarity</param>
		/// <returns>List of similarities</returns>
		public IEnumerable<Similarity> GetSimilarities(int id, double threshold = 0.95)
		{
			List<Similarity> list = new List<Similarity>();

			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				conn.ExecuteReader(
					string.Format(@"select	csid_from, 
											csid_to,
											tanimoto
									from similarities 
									where csid_from = {0} and tanimoto >= {1}
									order by tanimoto desc", id, threshold),
					r =>
					{
						list.Add(new Similarity
						{
							ID = Convert.ToInt32(r["csid_to"]),
							Score = Convert.ToDouble(r["tanimoto"]),
							SimilarityType = SimilarityType.Tanimoto
						});
					});
			}

			return list;
		}

		/// <summary>
		/// Returns total number of compounds registered in the database
		/// </summary>
		/// <returns></returns>
		public int GetCompoundsCount()
		{
			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				return conn.ExecuteScalar<int>("select count(distinct csid) from sid_csid");
			}
		}

		/// <summary>
		/// Convert InChI to Compound ID
		/// </summary>
		/// <param name="inchi">Standard on Non standard InChI</param>
		/// <returns>Internal compound ID</returns>
		public IEnumerable<int> InChIToCompoundId(string inchi)
		{
			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				if (inchi.StartsWith("InChI=1S/"))
				{
					//	standard InChI
					return conn.FetchColumn<int>(string.Format(@"select c.csid 
																from compounds c 
																	join inchi i on c.inchi_id_std = i.inc_id
																	join inchi_md5 md5 on i.inc_id = md5.inc_id
																where md5.inchi_md5 = HashBytes('md5', '{0}') and c.isSubstanceDerived = 1
																order by c.csid", inchi));
				}
				else if (inchi.StartsWith("InChI=1/"))
				{
					//	non standard InChI
					return conn.FetchColumn<int>(string.Format(@"select c.csid 
																from compounds c 
																	join inchi i on c.inchi_id_fixedh = i.inc_id 
																	join inchi_md5 md5 on i.inc_id = md5.inc_id
																where md5.inchi_md5 = HashBytes('md5', '{0}') and c.isSubstanceDerived = 1
																order by c.csid", inchi));
				}
			}

			//	return empty list...
			return new List<int>();
		}

		/// <summary>
		/// Convert InChIKey to Compound ID
		/// </summary>
		/// <param name="inchikey">Standard on Non standard InChIKey</param>
		/// <returns>Internal compound ID</returns>
		public IEnumerable<int> InChIKeyToCompoundId(string inchikey)
		{
			Regex range = new Regex("(?<first>[a-zA-Z]+)-(?<second>[a-zA-Z]+)-(?<third>[a-zA-Z]+)");

			Match match = range.Match(inchikey);

			if (match.Success)
			{
				var second = match.Groups["second"].Value;

				using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
				{
					if (second[8] == 'S')
					{
						//	standard inchi key
						return conn.FetchColumn<int>(string.Format(@"select c.csid 
																	from compounds c 
																		join inchi i on c.inchi_id_std = i.inc_id 
																	where i.inchi_key = '{0}' and c.isSubstanceDerived = 1 
																	order by c.csid", inchikey));
					}
					else if(second[8] == 'N') 
					{
						//	non standard inchi key
						return conn.FetchColumn<int>(string.Format(@"select c.csid 
																	from compounds c 
																		join inchi i on c.inchi_id_fixedh = i.inc_id 
																	where i.inchi_key = '{0}' and c.isSubstanceDerived = 1 
																	order by c.csid", inchikey));
					}
				}
			}

			//	return empty list...
			return new List<int>();
		}
	}
}
