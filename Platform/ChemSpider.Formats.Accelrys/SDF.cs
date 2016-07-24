using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using com.ggasoftware.indigo;

namespace MoleculeObjects
{
	public class Sdf
	{
		private List<Molecule> m_molecules = new List<Molecule>();
		private List<GenericMolecule> m_generics = new List<GenericMolecule>();

		public void RemoveMoleculesByProperty(string property, string value)
		{
			List<Molecule> candidates = m_molecules.Where(m => m.HasProperty(property)).ToList();
			List<Molecule> delenda = candidates.Where(m => m.Property(property).Contains(value)).ToList();
			delenda.ForEach(m => m_molecules.Remove(m));
		}

		public void RemoveGenericsByProperty(string property, string value)
		{
			List<GenericMolecule> candidates = m_generics.Where(m => m.HasProperty(property)).ToList();
			List<GenericMolecule> delenda = candidates.Where(m => m.Property(property).Contains(value)).ToList();
			delenda.ForEach(m => m_generics.Remove(m));
		}

		public Dictionary<string, GenericMolecule> GenericsIndexedByProperty(string property)
		{
			Dictionary<string, GenericMolecule> sdfrecords = new Dictionary<string, GenericMolecule>();
			m_generics.ForEach(gm => gm.Property(property).Where(p => !sdfrecords.ContainsKey(p)).ToList().ForEach(p => sdfrecords.Add(p, gm)));
			return sdfrecords;
		}

		public Dictionary<string, Molecule> MoleculesIndexedByProperty(string property)
		{
			Dictionary<string, Molecule> sdfrecords = new Dictionary<string, Molecule>();
			foreach ( Molecule mol in m_molecules ) {
				List<string> propertyvalues = mol.Property(property);
				if ( propertyvalues != null ) {
					foreach ( string propertyvalue in propertyvalues ) {
						if ( !sdfrecords.ContainsKey(propertyvalue) ) {
							sdfrecords.Add(propertyvalue, mol);
						}
					}
				}
			}
			return sdfrecords;
		}

		public List<GenericMolecule> genericMolecules { get { return m_generics; } }
		public List<Molecule> molecules { get { return m_molecules; } }
		public List<GenericMolecule> allAsGenerics
		{
			get
			{
				List<GenericMolecule> result = new List<GenericMolecule>(m_generics);
				result.AddRange(( from m in m_molecules select new GenericMolecule(m.Headers, m.IndexedAtoms, m.IndexedBonds, m.Properties) ));
				return result;
			}
		}

		public void AddGenericMolecule(GenericMolecule gm)
		{
			m_generics.Add(gm);
		}

		public void AddMolecule(Molecule molecule)
		{
			m_molecules.Add(molecule);
		}

		public List<Substructure> Scaffold()
		{
			var result = new List<Substructure>();
			Indigo i = new Indigo();
			var array = i.createArray();
			foreach ( var molecule in this.molecules ) {
				try {
					array.arrayAdd(i.loadMolecule(molecule.ct()));
				}
				catch {
					Console.Error.WriteLine("problem with molecule " + molecule.FirstProperty("COMPOUND"));
				}
			}
			var scaffolds = i.extractCommonScaffold(array, "exact");
			foreach ( IndigoObject scaffold in scaffolds.allScaffolds().iterateArray() ) {
				Console.WriteLine(scaffold.smiles());
				result.Add(new Substructure(scaffold));
			}
			return result;
		}

		/// <summary>
		/// Appends another sdf object to the end. Doesn't dedupe.
		/// </summary>
		/// <param name="sdf"></param>
		public void AddSdf(Sdf sdf)
		{
			m_molecules.AddRange(sdf.molecules);
			m_generics.AddRange(sdf.genericMolecules);
		}

		/// <summary>
		/// Returns the sdf as you would find it in a file.
		/// </summary>
		public override string ToString()
		{
			return String.Join("", from m in m_molecules select m.ToString())
				+ String.Join("", from m in m_generics select m.ToString());
		}

		/// <summary>
		/// Takes ethane molecules that are very close to a bond, deletes them and changes the terminal atom to a *.
		/// </summary>
		public void ProcessEthanesToFragments()
		{
			var ethanes = m_molecules.Where(m => m.IsEthane());
			var midpoints = from e in ethanes select e.IndexedBonds[1].Midpoint();
			var everythingElse = m_molecules.Where(m => !m.IsEthane());
			var resultGenerics = new List<GenericMolecule>();
			var resultMolecules = new List<Molecule>();
			foreach ( var molecule in everythingElse ) {
				var atomEthaneDists = new Dictionary<int, List<double>>();
				foreach ( var a in molecule.IndexedAtoms ) {
					atomEthaneDists.Add(a.Key,
						( from m in midpoints select a.Value.xyz.DistanceFrom(m) ).ToList());
				}
				var nearbyAtoms = from p in atomEthaneDists
								  where p.Value.Min() < molecule.MeanBondLength()
								  where molecule.DepictedValence(p.Key) == 1
								  select p.Key;
				if ( nearbyAtoms.Count() > 0 ) {
					var m = MoleculeFactory.FromMolV2000(molecule.ct());
					Dictionary<int, Atom> newAtoms = ( from p in m.IndexedAtoms
													   select
													   nearbyAtoms.Contains(p.Key)
													   ? new KeyValuePair<int, Atom>(p.Key, p.Value.UpdateElement("*"))
													   : p ).ToDictionary(p => p.Key, p => p.Value);
					resultGenerics.Add(new GenericMolecule(m.Headers, newAtoms, m.IndexedBonds, m.Properties));
				}
				else {
					resultMolecules.Add(molecule);
				}
			}
			m_generics = resultGenerics;
			m_molecules = resultMolecules;
		}

		// constructors
		public Sdf(Sdf sdf)
		{
			sdf.molecules.ToList().ForEach(m => m_molecules.Add(m));
			sdf.genericMolecules.ToList().ForEach(gm => m_generics.Add(gm));
		}

		public Sdf(List<GenericMolecule> generics)
		{
			generics.ForEach(gm => m_generics.Add(gm));
		}

		public Sdf()
		{
		}

		/// <summary>
		/// Builds an SDF object from the text contents of an SDF file.
		/// To build one from an external file, create a StreamReader and pass that in.
		/// if need to generate only GenericMolecules set "onlyProduceGenericMolecules" as true
		/// if needed to try egenrating Molecule objects that set "onlyProduceGenericMolecules" to false or do not provide that parameter
		/// </summary>
		public Sdf(string s, bool onlyProduceGenericMolecules = false)
		{
			string mol;
			List<string> lines = s.SplitOnNewLines();
			IEnumerator<string> enumerator = lines.GetEnumerator();
			while ( enumerator.MoveNext() ) {
				mol = string.Empty;
				mol += enumerator.Current + Environment.NewLine;
				while ( enumerator.MoveNext() && enumerator.Current != "$$$$" ) {
					mol += enumerator.Current + Environment.NewLine;
				}
				mol += enumerator.Current + Environment.NewLine;
				if ( mol.Length > 40 ) {

					if ( !onlyProduceGenericMolecules ) {
						try {
							Molecule m = MoleculeFactory.Molecule(mol);
							m_molecules.Add(m);
						}
						catch ( IndigoException ) {
							GenericMolecule gm = MoleculeFactory.FromMolV2000(mol);
							m_generics.Add(gm);
						}
					}
					else {
						GenericMolecule gm = MoleculeFactory.FromMolV2000(mol);
						m_generics.Add(gm);
					}
				}
			}
		}


		public static string generateSdfRevokeRecord(string regid)
		{
			throw new NotImplementedException("De-implemented after SDTagOptions moved somewhere");
			/* TODO: DO we need it??
			string revokeSdf = "\n  CVSP auto-generated revoke record\n\n  0  0  0  0  0  0  0  0  0  0999 V2000\nM  END\n\n> <" + SDTagOptions.DEPOSITOR_SUBSTANCE_REGID.ToString() + ">\n" + regid;
			revokeSdf += "\n\n> <" + SDTagOptions.DEPOSITOR_SUBSTANCE_REVOKE.ToString()+ ">\nauto-revoked when collection was replaced\n\n$$$$\n";

			return revokeSdf; */
		}

		public static bool areSdfRecordsSame(string sdf1, string sdf2)
		{
			try {
				if ( String.IsNullOrEmpty(sdf1) && String.IsNullOrEmpty(sdf2) )
					return true;
				if ( ( String.IsNullOrEmpty(sdf1) && !String.IsNullOrEmpty(sdf2) ) || ( !String.IsNullOrEmpty(sdf1) && String.IsNullOrEmpty(sdf2) ) )
					return false;
				GenericMolecule gm1 = MoleculeFactory.FromMolV2000(sdf1);
				GenericMolecule gm2 = MoleculeFactory.FromMolV2000(sdf2);
				if ( gm1 == null || gm2 == null )
					return false;
				if ( !gm1.ct().Equals(gm2.ct()) )
					return false;
				else if ( gm1.Properties.Count != gm2.Properties.Count )
					return false;
				foreach ( KeyValuePair<string, List<string>> kp in gm1.Properties ) {
					//bool containKey = gm2.Properties.ContainsKey(kp.Key);
					List<string> mol_new_values;
					if ( !gm2.Properties.TryGetValue(kp.Key, out mol_new_values) )
						return false;
					bool areEquivalent = ( gm2.Properties[kp.Key].Count() == kp.Value.Count() ) && !gm2.Properties[kp.Key].Except(kp.Value).Any();
					if ( !areEquivalent )
						return false;

				}
				return true;
			}
			catch ( Exception ) {
				return false;
			}
		}

	}

	/*
	public class SdfReader : IEnumerable<GenericMolecule>
	{
		private StreamReader m_sr;
		private GenericMolecule m_current;

		public SdfReader(string filename) : this(filename, Encoding.UTF8) { }

		public SdfReader(string filename, Encoding encoding)
		{
			m_sr = new StreamReader(filename, encoding);
		}

		public GenericMolecule Current
		{
			get { return (_enumerator == null) ? m_current : _enumerator.Current; }
		}

		public void Dispose()
		{
			if (m_sr != null) m_sr.Close();
		}
		
		public GenericMolecule GetGenericMolecule()
		{
			var lines = new List<string>();
			string line;
			line = m_sr.ReadLine();
			while (line != "$$$$" && !m_sr.EndOfStream)
			{
				lines.Add(line);
				line = m_sr.ReadLine();
			}
			lines.Add("$$$$");
			return (lines.Count > 1)
					? MoleculeFactory.FromMolV2000(String.Join(Environment.NewLine, lines))
					: null;
		}

		private SdfEnumerator _enumerator;

		private class SdfEnumerator : IEnumerator<GenericMolecule>
		{
			private SdfReader m_r;
			internal SdfEnumerator(SdfReader r)
			{
				m_r = r;
			}

			private GenericMolecule m_gm;
			public GenericMolecule Current { get { return m_gm; } }

			public void Dispose() { }
			public void Reset() { }

			object IEnumerator.Current { get { return m_gm; } }

			public bool MoveNext()
			{
				m_gm = m_r.GetGenericMolecule();
				return m_gm != null;
			}
		}

		public IEnumerator<GenericMolecule> GetEnumerator()
		{
			if (_enumerator == null) _enumerator = new SdfEnumerator(this);
			return _enumerator;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			if (_enumerator == null) _enumerator = new SdfEnumerator(this);
			return _enumerator;
		}
	}
	*/
}
