using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Configuration;

namespace ChemSpider.Molecules
{
	/// <summary>
	/// A data-reader style interface for reading SDF files.
	/// </summary>
	public class SdfReader : IDisposable
	{
		public enum Options { SplitMultilineValues }

		/// <summary>
		/// Default splitter splits SD field value on EOLs
		/// </summary>
		public static Func<string, IEnumerable<string>> DefaultSplitter =
			s => s.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(_s => _s.Trim());

		private Stream m_Stream;
		private TextReader m_Reader;

		private IDictionary<string, string> m_FieldsMap;
		public IDictionary<string, string> FieldsMap
		{
			get { return m_FieldsMap; }
			set { m_FieldsMap = value; }
		}

		private Dictionary<string, Func<string, IEnumerable<string>>> m_Splitters;
		public Dictionary<string, Func<string, IEnumerable<string>>> Splitters
		{
			get { return m_Splitters; }
			set { m_Splitters = value; }
		}

		public ICollection<Func<SdfRecord, int, string, string>> LineFilters { get; set; }

		private int _counter;
		public int Counter
		{
			get { return _counter; }
		}

		/// <summary>
		/// Create a new reader for the given stream.
		/// </summary>
		/// <param name="s">The stream to read the SDF from.</param>
		public SdfReader(Stream s)
			: this(s, null)
		{
		}

		/// <summary>
		/// Create a new reader for the given stream and encoding.
		/// </summary>
		/// <param name="s">The stream to read the SDF from.</param>
		/// <param name="enc">The encoding used.</param>
		public SdfReader(Stream s, Encoding enc)
		{
			m_Stream = s;
			if ( !s.CanRead )
				throw new Exception("Could not read the given SDF stream!");

			m_Reader = ( enc != null ) ? new StreamReader(s, enc) : new StreamReader(s, Encoding.GetEncoding(1252));
		}

		public SdfReader(TextReader r)
		{
			m_Reader = r;
		}

		/// <summary>
		/// Creates a new reader for the given text file path.
		/// </summary>
		/// <param name="filename">The name of the file to be read.</param>
		public SdfReader(string filename)
			: this(filename, null)
		{
		}

		/// <summary>
		/// Creates a new reader for the given text file path and encoding.
		/// </summary>
		/// <param name="filename">The name of the file to be read.</param>
		/// <param name="enc">The encoding used.</param>
		public SdfReader(string filename, Encoding enc)
			: this(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read), enc ?? Encoding.GetEncoding(1252))
		{
		}

		private Regex _rxRecEnd = new Regex(@"^\$\$\$\$\s*$");
		private Regex _rxMEnd = new Regex(@"^M\s+END");
		private Regex _rxPropName = new Regex(@"^>[^<]*<([^/][^>]+)>");

		public SdfRecord ReadSDFRecord()
		{
			return readSDFRecord(LineFilters);
		}

		/// <summary>
		/// This method is the one that actually reads content from the stream and composes underlying collections
		/// </summary>
		/// <param name="lineFilter">
		/// Function that receives line number and a line from SDF and returns a modified one or completely removes it from the stream if null is returned
		/// </param>
		/// <returns></returns>
		private SdfRecord readSDFRecord(IEnumerable<Func<SdfRecord, int, string, string>> lineFilters)
		{
			SdfRecord record = new SdfRecord();

			StringBuilder buf = new StringBuilder();

			string key = null;
			bool bMolAssigned = false;
			string line = m_Reader.ReadLine();

			if ( line == null )
				return null;

			int nLine = 0;
			while ( line != null )
			{
				if ( lineFilters != null ) {
					bool skip = false;
					foreach ( var filter in lineFilters ) {
						line = filter(record, nLine++, line);
						if ( line == null ) {	// This is a way to remove the line completely from the input
							line = m_Reader.ReadLine();
							skip = true;
							break;
						}
					}

					if ( skip )
						continue;
				}

				// M END - assign MOL and continue reading
				if ( !bMolAssigned && _rxMEnd.IsMatch(line) ) {
					buf.AppendLine(line);
					record.Mol = buf.ToString();
					bMolAssigned = true;
					buf.Clear();
				}
				else {
					// > <key>
					Match mProp = _rxPropName.Match(line);
					if ( mProp.Success ) {
						// There is no MOL after this point, so assign whatever accumulated
						if ( !bMolAssigned ) {
							record.Mol = buf.ToString();
							bMolAssigned = true;
						}

						// Previous property was accumulating - add that one
						if ( key != null )
							addSdfField(record, key, buf.ToString().Trim());

						// Set new property key and start accumulation
						key = mProp.Groups[1].Value;
						buf.Clear();
					}
					else if ( _rxRecEnd.IsMatch(line) ) {
						if ( !bMolAssigned )
							record.Mol = buf.ToString();
						else if ( key != null ) {
							addSdfField(record, key, buf.ToString().Trim());
							key = null;
						}

						break;
					}
					else {
						buf.AppendLine(line);
					}
				}

				line = m_Reader.ReadLine();
			}

			if ( key != null )
				addSdfField(record, key, buf.ToString().Trim());

			if ( record.Molecule == null && record.Properties.Count == 0 )
				return null;

			_counter++;
			return record;
		}

		private void addSdfField(SdfRecord record, string key, string value)
		{
			if ( m_FieldsMap != null )
				key = mapPropName(key);

			if ( m_Splitters == null )
				record.AddField(key, value);
			else {
				var splitter = m_Splitters.FirstOrDefault(kv => String.Compare(kv.Key, key, true) == 0);
				if ( splitter.Equals(default(KeyValuePair<string, Func<string, IEnumerable<string>>>)) )
					record.AddField(key, value);
				else
					record.AddFieldValues(key, splitter.Value(value));
			}
		}

		private string mapPropName(string propname)
		{
			foreach ( string key in m_FieldsMap.Keys ) {
				if ( Regex.IsMatch(propname, key, RegexOptions.IgnoreCase) )
					propname = m_FieldsMap[key];
			}
			return propname;
		}

		/// <summary>
		/// Disposes the SDFReader. The underlying stream is closed.
		/// </summary>
		public void Dispose()
		{
			if ( m_Stream != null )
				m_Stream.Close(); // In case we failed before the reader was constructed
			m_Reader.Close();
		}

		private class SdfEnumerator : IEnumerator<SdfRecord>
		{
			private SdfReader _reader;
			internal SdfEnumerator(SdfReader r)
			{
				_reader = r;
			}

			private SdfRecord _record;
			public SdfRecord Current
			{
				get { return _record; }
			}

			public void Dispose()
			{

			}

			object System.Collections.IEnumerator.Current
			{
				get { return _record; }
			}

			public bool MoveNext()
			{
				lock ( _reader ) {
					_record = _reader.ReadSDFRecord();
					return _record != null;
				}
			}

			public void Reset()
			{
				throw new InvalidOperationException();
			}
		}

		private class SdfEnumerable : IEnumerable<SdfRecord>
		{
			private SdfReader _reader;
			private SdfEnumerator _enumerator;

			public SdfEnumerable(SdfReader reader)
			{
				_reader = reader;
			}

			public IEnumerator<SdfRecord> GetEnumerator()
			{
				return getEnumerator();
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return getEnumerator();
			}

			private IEnumerator<SdfRecord> getEnumerator()
			{
				if ( _enumerator != null && _reader._counter != 0 )
					throw new InvalidOperationException("Cannot create new enumerator on a reader in non-initial state");
				return _enumerator ?? ( _enumerator = new SdfEnumerator(_reader) );
			}
		}

		private SdfEnumerable _enumerable;
		public IEnumerable<SdfRecord> Records
		{
			get
			{
				return _enumerable ?? ( _enumerable = new SdfEnumerable(this) );
			}
		}
	}
}
