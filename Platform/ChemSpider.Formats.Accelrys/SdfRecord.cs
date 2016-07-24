using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Configuration;
using com.ggasoftware.indigo;
using ChemSpider.Utilities;
using System.Security.Cryptography;
using OpenEyeNet;
using MoleculeObjects;

namespace ChemSpider.Molecules
{
	public class SdfRecord : MoleculeRecord
	{
		public string Mol
		{
			get { return Molecule == null ? null : Molecule.Mol; }
			set { Molecule =  new NewMolecule(value); }
		}

		public override int DataCount
		{
			get { return 0; }
		}

		public IEnumerable<string> this[string name]
		{
			get {
				if ( m_Properties == null )
					return null;

				// To support case-insensitive comparison
				var v = m_Properties.FirstOrDefault(kv => String.Compare(kv.Key, name, true) == 0);
				return v.Equals(default(KeyValuePair<string, List<string>>)) ? null : v.Value;
			}
		}

		public IEnumerable<string> this[Regex regex]
		{
			get
			{
				if ( m_Properties == null )
					return null;

				List<string> values = new List<string>();
				foreach ( List<string> vs in m_Properties.Where(kvp => regex.IsMatch(kvp.Key)).Select(kvp => kvp.Value) )
					values.AddRange(vs);
				return values.Count > 0 ? values : null;
			}
		}

		public string GetFieldValue(string name)
		{
			return this[name] == null ? null : String.Join(Environment.NewLine, this[name]);
		}

		public bool HasField(string name)
		{
			return this[name] != null;
		}

		////////////////////////////////////////////////////////////////////

		public SdfRecord AddField(string name, string value)
		{
			if ( m_Properties == null )
				m_Properties = new Dictionary<string, List<string>>();
			if ( !m_Properties.ContainsKey(name) )
				m_Properties.Add(name, new List<string>());
			m_Properties[name].Add(value);
			return this;
		}

		public SdfRecord AddFieldValues(string name, IEnumerable<string> values)
		{
			foreach ( string value in values )
				AddField(name, value);
			return this;
		}

		public SdfRecord()
		{

		}

		public SdfRecord(string mol, IEnumerable<Tuple<string, string>> data)
		{
			Mol = mol;
			if ( data != null )
				data.ForAll(p => { AddField(p.Item1, p.Item2); });
		}

		public static SdfRecord FromString(string sdf_or_mol)
		{
			if ( String.IsNullOrEmpty(sdf_or_mol) )
				return null;

			using ( StringReader sr = new StringReader(sdf_or_mol) )
			using ( SdfReader sdfReader = new SdfReader(sr) ) {
				SdfRecord rec1 = sdfReader.ReadSDFRecord();
				if ( sdfReader.ReadSDFRecord() != null )
					throw new Exception("String contains more than one SDF records");
				return rec1;
			}
		}

		public override string ToString()
		{
			//Replace any $$$$ that may exist.
			StringBuilder res = new StringBuilder(String.IsNullOrEmpty(Mol) ? "" : Mol.Replace("\n$$$$", ""));
			if ( m_Properties != null ) {
				foreach ( string key in m_Properties.Keys ) {
					if ( m_Properties[key].Count > 0 ) {
						res.AppendFormat("> <{0}>\n", key);
						foreach ( string value in m_Properties[key] ) {
							if ( !String.IsNullOrEmpty(value) )
								res.AppendLine(value);
						}
						res.AppendLine();
					}
				}
			}
			res.AppendLine("$$$$");
			return res.ToString();
		}

		private static void hashSdf(IndigoObject sdf, out byte[] hash, out byte[] str_hash, out byte[] data_hash)
		{
			MD5 md5 = MD5.Create();
			str_hash = md5.ComputeHash(Encoding.UTF8.GetBytes(sdf.molfile().TrimEnd()));
			data_hash = md5.ComputeHash(
				Encoding.UTF8.GetBytes(
					String.Join("",
						sdf.iterateProperties()
						.Cast<IndigoObject>()
						.Select(p => p.name().Trim() + p.rawData().Trim())
						.OrderBy(p => p))));
			hash = md5.ComputeHash(str_hash.Concat(data_hash).ToArray());
		}

		public static bool IsCorrect(string sdf)
		{
			if ( String.IsNullOrEmpty(sdf) )
				return false;

			// TODO: Create or use built-in validator
			return OpenEyeUtility.GetInstance().IsCorrectSdf(sdf);
		}
	}
}
