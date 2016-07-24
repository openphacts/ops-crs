using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;

using ChemSpider.ObjectModel;
using ChemSpider.Database;
using ChemSpider.Molecules;
using ChemSpider.Compounds;
using ChemSpider.Compounds.Database;

namespace ChemSpider.OPS.ObjectModel
{
	// ***********************************************************************************************************
	//  DEPRICATED CLASS!!! DO NOT USE!!! USE CLASSES FROM CHEMSPIDER.CRS.OBJECTMODEL NAMESPACE INSTEAD!!!
	// ***********************************************************************************************************
	[DataContract]
	public class Compound : CompoundBase
	{
		//private CompoundProvider compoundProvider = new CompoundProvider();
		private ICompoundsService compoundProvider = new CompoundsService(new SQLCompoundsStore(new SQLSubstancesStore()));

		protected override Hashtable PullProperties()
		{
			Hashtable properties = new Hashtable();

			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				//	get list of substances... 
				properties["Substances"] = conn.FetchColumn<int>("select [sid] from sid_csid where csid = @id", new { id = CSID });

				conn.ExecuteReader(String.Format(@"
					declare @parent_ids varchar(1000)
					select @parent_ids = COALESCE(@parent_ids + '|', '') + cast(id AS varchar(20))
					from (select distinct rel_type_id as id from parent_children where parent_id = {0}) t

					select  dbo.fGetCompoundTitle({0}) as title, 
							dbo.fGetSysName({0}) as sys_name,
							compounds_properties.mf_formatted as MF, 
							chemspider_properties.Average_Mass, 
							compounds_properties.mw_indigo as Molecular_Weight, 
							chemspider_properties.Monoisotopic_Mass, 
							chemspider_properties.Nominal_Mass,
							@parent_ids as parent_ids
					from compounds c
						left outer join chemspider_properties on c.chemspider_csid = chemspider_properties.chemspider_csid
						left outer join compounds_properties on compounds_properties.csid = c.csid
					where c.csid = {0}", CSID),
					r =>
					{
						properties["name"] = r["title"] as string ?? r["sys_name"] as string;
						properties["sys_name"] = r["sys_name"] as string;
						properties["MF"] = MolecularFormula.prepareMF(r["MF"] as string);
						properties["MW"] = r["Molecular_Weight"] is DBNull ? null : r["Molecular_Weight"] as double?;
						properties["AM"] = r["Average_Mass"] is DBNull ? null : r["Average_Mass"] as Single?;
						properties["MM"] = r["Monoisotopic_Mass"] is DBNull ? null : r["Monoisotopic_Mass"] as double?;
						properties["NM"] = r["Nominal_Mass"] is DBNull ? null : r["Nominal_Mass"] as Single?;

						string parent_ids = r["parent_ids"] as string;
						if (!string.IsNullOrEmpty(parent_ids))
						{
							properties["Types"] = from t in parent_ids.Split('|') select (CompoundType)Convert.ToInt32(t);
						}
					});
			}

			return properties;
		}

		public Compound(int csid) : base(csid)
		{
		}

		[DataMember]
		[Description("Compound's types")]
		public IEnumerable<CompoundType> CompoundTypes
		{
			get { return Properties["Types"] as IEnumerable<CompoundType>; } 
			set { }
		}

		[DataMember(EmitDefaultValue = false)]
		[Description("Common compound's name")]
		public string Name
		{
			get { return Properties["name"] as string; }
			set { }
		}

		[DataMember(EmitDefaultValue = false)]
		[Description("Systematic compound's name")]
		public string SysName
		{
			get { return Properties["sys_name"] as string; }
			set { }
		}

		[DataMember(EmitDefaultValue = false)]
		[Description("Molecule formula")]
		public string MF
		{
			get { return Properties["MF"] as string; }
			set { }
		}

		[DataMember(EmitDefaultValue = false)]
		[Description("Molecule weight")]
		public double? MW
		{
			get { return Properties["MW"] as double?; }
			set { }
		}

		[DataMember(EmitDefaultValue = false)]
		[Description("Average Mass")]
		public double? AM
		{
			get { return Properties["AM"] as double?; }
			set { }
		}

		[DataMember(EmitDefaultValue = false)]
		[Description("Monoisotopic Mass")]
		public double? MM
		{
			get { return Properties["MM"] as double?; }
			set { }
		}

		[DataMember(EmitDefaultValue = false)]
		[Description("Nominal Mass")]
		public double? NM
		{
			get { return Properties["NM"] as double?; }
			set { }
		}

		[DataMember(EmitDefaultValue = false)]
		public IEnumerable<int> Substances
		{
			get { return Properties["Substances"] as IEnumerable<int>; }
			set { }
		}

		protected override IEnumerable<Identifier> PullIdentifiers()
		{
			List<Identifier> list = new List<Identifier>();
			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				conn.ExecuteReader(string.Format(
									@"select i.inchi as inchi_std,
											i.inchi_key as inchi_key_std, 
											cni.non_std_inchi as inchi,
											cni.non_std_inchi_key as inchi_key, 
											oe_abs_smiles as SMILES 
									from compounds c
										inner join inchis i on c.inc_id = i.inc_id
										inner join compounds_smiles cs on c.csid = cs.csid
										inner join compounds_nonstd_inchi cni on cni.csid = c.csid
									where c.csid = {0}", CSID),
					r =>
					{
						list.Add(new Identifier { IdentifierType = IdentifierType.InChI, Version = "v1.04b", Value = r["inchi"] as string });
						list.Add(new Identifier { IdentifierType = IdentifierType.InChI, Version = "v1.04s", Value = r["inchi_std"] as string });
						list.Add(new Identifier { IdentifierType = IdentifierType.InChIKey, Version = "v1.04b", Value = r["inchi_key"] as string });
						list.Add(new Identifier { IdentifierType = IdentifierType.InChIKey, Version = "v1.04s", Value = r["inchi_key_std"] as string });
						list.Add(new Identifier { IdentifierType = IdentifierType.SMILES, Version = "OEChem", Value = r["SMILES"] as string });
					});
			}

			return list;
		}

		protected override IEnumerable<Synonym> PullSynonyms()
		{
			List<Synonym> list = new List<Synonym>();
			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				conn.ExecuteReader(
					String.Format(@"select distinct [synonym], [lang_id]
									from (
										select	syn.synonym,
												lang_id1 as lang_id
										from chemspider_synonyms syn join compounds c on c.chemspider_csid = syn.chemspider_csid
										where dbid_yn = 0
											and c.csid = {0}
										union all

										select 	synonym_display as [synonym],
												lang_id
										from substance_derived_synonyms syn 
											inner join substance_synonyms ss on syn.synonym_id = ss.synonym_id
											inner join sid_csid on sid_csid.sid = ss.sid
										where sid_csid.csid = {0}
									) t", CSID),
					r =>
					{
						list.Add(new Synonym
						{
							Name = r["synonym"] as string,
							Reliability = Reliability.Confirmed, //decodeReliability(r["opinion"] is DBNull ? null : r["opinion"] as string, (bool)r["approved_yn"]),
							LangID = r["lang_id"] as string
						});
					});
			}
			return list;
		}

		protected override IEnumerable<Reference> PullReferences(int? dsn_id)
		{
			List<Reference> list = new List<Reference>();
			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				string sql = String.Format(@"select distinct ds.dsn_name, s.ext_regid, s.ext_url
											from substances s 
												inner join sid_csid sc on sc.sid = s.sid
												inner join datasources ds on s.dsn_id = ds.dsn_id
											where sc.csid = {0}", CSID);

				if (dsn_id != null)
					sql += " and ds.dsn_id = " + dsn_id;

				conn.ExecuteReader(sql,
					r =>
					{
						Uri uri;
						Uri.TryCreate(r["ext_url"] is DBNull ? null : r["ext_url"] as string, UriKind.Absolute, out uri);

						list.Add(new Reference
						{
							Source = r["dsn_name"] as string,
							Text = r["ext_regid"] as string,
							Link = uri
						});
					});
			}

			return list;
		}

		protected override IEnumerable<Blob> PullBlobs()
		{
			return null;
		}

		protected override IEnumerable<DatasourceType> PullDatasources(int? dsn_id, int? col_id, int? dst_id)
		{
			List<Datasource> datasources = new List<Datasource>();
			using (SqlConnection conn = new SqlConnection(CompoundsDB.ConnectionString))
			{
				string sql = string.Format(@"select distinct ds.dsn_id, ds.dsn_name, ds.dsn_URI
									from substances s 
										inner join sid_csid sc on sc.sid = s.sid
										inner join datasources ds on s.dsn_id = ds.dsn_id
									where sc.csid = {0}", CSID);

				if (dsn_id != null)
					sql += string.Format(" and ds.dsn_id = {0}", dsn_id);

				conn.ExecuteReader(
					sql,
					r =>
					{
						datasources.Add(new Datasource
						{
							ID = r["dsn_id"] is DBNull ? 0 : Convert.ToInt32(r["dsn_id"]),
							Name = r["dsn_name"] as string,
							Url = r["dsn_URI"] as string
						});
					});
			}

			return new List<DatasourceType>() {
				new DatasourceType
				{
					ID = 1,
					Name = "Biological Properties",
					ShortName = "Biological Data",
					Datasources = datasources
				}
			};
		}

		protected override IEnumerable<ChemSpider.ObjectModel.Similarity> PullSimilarities()
		{
			return null;
		}

		protected override string PullSDF()
		{
			return compoundProvider.GetSDF(CSID);
		}
	}
}
