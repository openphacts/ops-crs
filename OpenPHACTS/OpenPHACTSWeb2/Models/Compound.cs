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

using RSC.Compounds;
using RSC.Properties;

namespace ChemSpider.OPS.ObjectModel
{
	// ***********************************************************************************************************
	//  DEPRICATED CLASS!!! DO NOT USE!!! USE CLASSES FROM CHEMSPIDER.CRS.OBJECTMODEL NAMESPACE INSTEAD!!!
	// ***********************************************************************************************************
	[DataContract]
	public class Compound
	{
		public const string OPS_URI = "http://ops.rsc.org/";

		private RSC.Compounds.ICompoundStore compoundStore = new RSC.Compounds.EntityFramework.EFCompoundStore2();
		private RSC.Properties.IPropertyStore propertyStore = new RSC.Properties.EntityFramework.EFPropertyStore();

		private RSC.Compounds.Compound compound;

        private Hashtable _properties = null;
        private IEnumerable<Identifier> _identifiers = null;
        private IEnumerable<ChemSpider.ObjectModel.Synonym> _synonyms = null;
        private IEnumerable<Reference> _refs = null;
        private IEnumerable<Blob> _blobs = null;
        private IEnumerable<DatasourceType> _datasourceTypes = null;
        private SdfRecord _sdf = null;

		public Compound(RSC.Compounds.Compound compound)
		{
			this.compound = compound;

			if (compound.ExternalReferences.Where(er => er.Type.UriSpace == OPS_URI).Any())
				CSID = Convert.ToInt32(compound.ExternalReferences.Where(er => er.Type.UriSpace == OPS_URI).Single().Value);
		}

        [DataMember]
        [Description("Internal ChemSpider ID")]
        public int CSID { get; set; }

        public Hashtable Properties
        {
            get
            {
                if (_properties == null)
                    _properties = PullProperties();

                return _properties;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("List of alternative compound's identifications (InChi, SMILES etc.)")]
        public IEnumerable<Identifier> Identifiers
        {
            set { _identifiers = value; }
            get
            {
                if (_identifiers == null)
                    _identifiers = PullIdentifiers();

                return _identifiers;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("List of synonyms assigned to the compound")]
        public IEnumerable<ChemSpider.ObjectModel.Synonym> Synonyms
        {
            set { _synonyms = value; }
            get
            {
                if (_synonyms == null)
                    _synonyms = PullSynonyms();

                return _synonyms;
            }
        }

        public IEnumerable<Reference> GetReferences(int? dsn_id)
        {
            return PullReferences(dsn_id);
        }

        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        [Description("List of compound's references")]
        public IEnumerable<Reference> References
        {
            set { _refs = value; }
            get
            {
                if (_refs == null)
                    _refs = PullReferences(null);

                return _refs;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("List of blobs attached to compound (spectra, images, cifs etc.)")]
        public IEnumerable<Blob> Blobs
        {
            set { _blobs = value; }
            get
            {
                if (_blobs == null)
                    _blobs = PullBlobs();

                return _blobs;
            }
        }

        public IEnumerable<DatasourceType> GetDatasources(int? dsn_id, int? col_id, int? dst_id)
        {
            return PullDatasources(dsn_id, col_id, dst_id);
        }

        [DataMember(EmitDefaultValue = false)]
        [Description("List of compound's data sources types and data sources from that types and references from that data sources")]
        public IEnumerable<DatasourceType> Datasources
        {
            set { _datasourceTypes = value; }
            get
            {
                if (_datasourceTypes == null)
                    _datasourceTypes = PullDatasources(null, null, null);

                return _datasourceTypes;
            }
        }

        /// <summary>
        /// Returns the molfile.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [Description("MOL file")]
        public string Mol
        {
            set { _sdf = new SdfRecord(value, null); }
            get 
            {
                if (_sdf == null)
                    initSdf();

                return _sdf == null ? null : _sdf.Mol;
            }
        }

        private void initSdf()
        {
            string sdf = PullSDF();
            if (String.IsNullOrWhiteSpace(sdf))
                _sdf = null;
            else
            {
                _sdf = SdfRecord.FromString(sdf);
                if (_sdf.Properties != null)
                    _sdf.Properties.Clear();
            }
        }

        public string Sdf
        {
            get
            {
                if (_sdf == null)
                    initSdf();

                if (_sdf != null && (_sdf.Properties == null || (_sdf.Properties != null && _sdf.Properties.Count == 0)))
                {
                    _sdf.AddField("CSID", CSID.ToString());

                    foreach (var id in Identifiers.Where(id => id.IdentifierType == IdentifierType.SMILES))
                        _sdf.AddField("SMILES", id.ToString());

                    foreach (var id in Identifiers.Where(id => id.IdentifierType == IdentifierType.InChIKey))
                        _sdf.AddField("InChIKey", id.ToString());

                    foreach (var id in Identifiers.Where(id => id.IdentifierType == IdentifierType.InChI))
                        _sdf.AddField("InChI", id.ToString());

                    foreach (var synonym in Synonyms)
                        _sdf.AddField("Synonyms", synonym.ToString());

                    foreach (var r in References)
                        _sdf.AddField("References", r.ToString());
                }

                return _sdf == null ? null : _sdf.ToString();
            }
        }

		protected Hashtable PullProperties()
		{
			Hashtable properties = new Hashtable();

			if (compound.Synonyms != null)
			{
				properties["name"] = compound.Synonyms.Where(s => s.IsTitle).Select(s => s.Name).FirstOrDefault();
				//properties["sys_name"] = r["sys_name"] as string;
			}

			var guids = compoundStore.GetCompoundProperties(compound.Id);
			if (guids != null && guids.Any())
			{
				var props = propertyStore.GetProperties(guids);

				if (props.Any())
				{
					if (props.HasProperty(PropertyName.MOLECULAR_FORMULA))
						properties["MF"] = MolecularFormula.prepareMF(props.GetProperties(PropertyName.MOLECULAR_FORMULA).Select(p => p.Value.ToString()).FirstOrDefault());
					if (props.HasProperty(PropertyName.MOLECULAR_WEIGHT))
						properties["MW"] = props.GetProperties(PropertyName.MOLECULAR_WEIGHT).Select(p => p.Value as double?).FirstOrDefault();
					if (props.HasProperty(PropertyName.AVERAGE_MASS))
						properties["AM"] = props.GetProperties(PropertyName.AVERAGE_MASS).Select(p => p.Value as double?).FirstOrDefault();
					if (props.HasProperty(PropertyName.MONOISOTOPIC_MASS))
						properties["MM"] = props.GetProperties(PropertyName.MONOISOTOPIC_MASS).Select(p => p.Value as double?).FirstOrDefault();
					if (props.HasProperty(PropertyName.NOMINAL_MASS))
						properties["NM"] = props.GetProperties(PropertyName.NOMINAL_MASS).Select(p => p.Value as double?).FirstOrDefault();
				}
			}

/*
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
*/
			return properties;
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

		protected IEnumerable<Identifier> PullIdentifiers()
		{
			List<Identifier> list = new List<Identifier>();

			if (compound.NonStandardInChI != null)
			{
				list.Add(new Identifier { IdentifierType = IdentifierType.InChI, Version = "v1.04b", Value = compound.NonStandardInChI.Inchi });
				list.Add(new Identifier { IdentifierType = IdentifierType.InChIKey, Version = "v1.04b", Value = compound.NonStandardInChI.InChIKey });
			}
			if (compound.StandardInChI != null)
			{
				list.Add(new Identifier { IdentifierType = IdentifierType.InChI, Version = "v1.04s", Value = compound.StandardInChI.Inchi });
				list.Add(new Identifier { IdentifierType = IdentifierType.InChIKey, Version = "v1.04s", Value = compound.StandardInChI.InChIKey });
			}
			if (compound.Smiles != null)
				list.Add(new Identifier { IdentifierType = IdentifierType.SMILES, Version = "OEChem", Value = compound.Smiles.IndigoSmiles });

/*
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
*/
			return list;
		}

		protected IEnumerable<ChemSpider.ObjectModel.Synonym> PullSynonyms()
		{
			if (compound.Synonyms != null)
			{
				return compound.Synonyms.Select(s => new ChemSpider.ObjectModel.Synonym()
				{
					LangID = s.LanguageId,
					Name = s.Name
				});
			}

			return null;

/*
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
*/
		}

		protected IEnumerable<Reference> PullReferences(int? dsn_id)
		{
			List<Reference> list = new List<Reference>();
/*
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
*/
			return list;
		}

		protected IEnumerable<Blob> PullBlobs()
		{
			return null;
		}

		protected IEnumerable<DatasourceType> PullDatasources(int? dsn_id, int? col_id, int? dst_id)
		{
			List<Datasource> datasources = new List<Datasource>();
/*
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
*/
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

		protected string PullSDF()
		{
			//return compoundProvider.GetSDF(CSID);
			return compound.Mol;
		}
	}
}
