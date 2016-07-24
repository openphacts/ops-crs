using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using OpenEyeNet;
using ChemSpider.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace ChemSpider.Molecules
{
    // REFACTORING
    namespace Retired
    {
        /*
        /// <summary>
        /// Object for processing an sdf that is small enough to hold in memory.
        /// </summary>
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

            public List<GenericMolecule> allAsGenericMolecules
            {
                get
                {
                    var result = new List<GenericMolecule>(m_generics);
                    result.AddRange((from m in m_molecules select new GenericMolecule(m.ToString())).ToList());
                    return result;
                }
            }
            public List<GenericMolecule> genericMolecules { get { return m_generics; } }
            public List<Molecule> molecules { get { return m_molecules; } }

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
                            (from m in midpoints select a.Value.DistanceFrom(m)).ToList());
                    }
                    var nearbyAtoms = from p in atomEthaneDists
                                      where p.Value.Min() < molecule.MeanBondLength()
                                      where molecule.DepictedValence(p.Key) == 1
                                      select p.Key;
                    if ( nearbyAtoms.Count() > 0 ) {
                        var newGeneric = new GenericMolecule(molecule.ct);
                        foreach ( int nearbyAtom in nearbyAtoms ) {
                            newGeneric.UpdateAtomElement(nearbyAtom, "*");
                        }
                        resultGenerics.Add(newGeneric);
                    }
                    else {
                        resultMolecules.Add(molecule);
                    }
                    Console.WriteLine();
                }
                m_generics = resultGenerics;
                m_molecules = resultMolecules;
            }

            /// <summary>
            /// Builds an SDF object from the text contents of an SDF file.
            /// To build one from an external file, create a StreamReader and pass that in.
            /// 
            /// TODO: rationalize this with all the other code.
            /// </summary>
            public Sdf(string s)
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
                        try {
                            Molecule m = new Molecule(mol);
                            m_molecules.Add(m);
                        }
                        catch ( Exception e ) // need a better way of doing this
                        {
                            GenericMolecule gm = new GenericMolecule(mol);
                            gm.AddProperty("Exception", e.ToString());
                            m_generics.Add(gm);
                        }
                    }
                }
            }
        }
         * */

        public class SDFRecord
        {
            private string m_Molecule = null;
            private Hashtable m_Properties = null;

            public Hashtable Properties
            {
                get
                {
                    return m_Properties;
                }
            }
            public string Molecule
            {
                get
                {
                    return m_Molecule;
                }
                set
                {
                    m_Molecule = value;
                }
            }

            public string this[string name]
            {
                get
                {
                    return m_Properties != null ? (string)m_Properties[name] : null;
                }
                set
                {
                    if ( m_Properties == null )
                        m_Properties = new Hashtable();
                    m_Properties[name] = value;
                }
            }

            public SDFRecord appendFieldValue(string name, string value)
            {
                if ( m_Properties == null )
                    m_Properties = new Hashtable();
                if ( String.IsNullOrEmpty(m_Properties[name] as string) )
                    m_Properties[name] = value;
                else
                    m_Properties[name] = m_Properties[name] + "\r\n" + value;
                return this;
            }

            public SDFRecord()
            {
            }

            public SDFRecord(string sdf_or_mol)
            {
                using ( SDFReader sdfReader = new SDFReader(new StringReader(sdf_or_mol), true) ) {
                    if ( sdfReader.fillSDFRecord(this) == null )
                        throw new InvalidDataException();
                }
            }

            public static SDFRecord FromString(string sdf_or_mol)
            {
                if ( String.IsNullOrWhiteSpace(sdf_or_mol) )
                    return null;

                SDFReader sdfReader = new SDFReader(new StringReader(sdf_or_mol), true);
                return sdfReader.GetSDFRecord();
            }

            public override string ToString()
            {
                StringBuilder res = new StringBuilder();
                if ( !string.IsNullOrEmpty(m_Molecule) && m_Molecule.Contains("\n$$$$") )
                    m_Molecule = m_Molecule.Replace("\n$$$$", "");
                if ( !string.IsNullOrEmpty(m_Molecule) ) {
                    res.Append(m_Molecule);
                    if ( !m_Molecule.EndsWith("\n") )
                        res.AppendLine();
                }
                if ( m_Properties != null ) {
                    foreach ( DictionaryEntry de in m_Properties ) {
                        if ( !String.IsNullOrEmpty((string)de.Value) ) {
                            res.AppendFormat("> <{0}>\n", de.Key.ToString());
                            res.AppendLine(((string)de.Value).Replace("\n\n", "\n"));
                            res.AppendLine();
                        }
                    }
                }
                res.AppendLine("$$$$");
                return res.ToString();
            }

            public static bool IsCorrect(string sdf)
            {
                if ( String.IsNullOrEmpty(sdf) )
                    return false;

                return OpenEyeUtility.GetInstance().IsCorrectSdf(sdf);
            }

            public SDFRecord ClearProperties()
            {
                if ( Properties != null )
                    Properties.Clear();
                return this;
            }
        }

        /// <summary>
        /// A data-reader style interface for reading SDF files.
        /// </summary>
        public class SDFReader : IEnumerable<SDFRecord>, IDisposable
        {
            #region Private variables

            private Stream m_Stream;
            private TextReader m_Reader;
            private bool m_ReaderOwner = true;
            private StringBuilder m_Buffer = new StringBuilder();
            private char[] m_RawBuffer = new char[1024];
            private bool m_EmptyLine = false;

            #endregion

            /// <summary>
            /// Create a new reader for the given stream.
            /// </summary>
            /// <param name="s">The stream to read the SDF from.</param>
            public SDFReader(Stream s)
                : this(s, null)
            {
            }

            /// <summary>
            /// Create a new reader for the given stream and encoding.
            /// </summary>
            /// <param name="s">The stream to read the SDF from.</param>
            /// <param name="enc">The encoding used.</param>
            public SDFReader(Stream s, Encoding enc)
            {

                m_Stream = s;
                if ( !s.CanRead ) {
                    throw new SDFReaderException("Could not read the given SDF stream!");
                }
                m_Reader = (enc != null) ? new StreamReader(s, enc) : new StreamReader(s, Encoding.GetEncoding(1252));
            }

            public SDFReader(TextReader r, bool aTakeOwnership)
            {
                m_Reader = r;
                m_ReaderOwner = aTakeOwnership;
            }

            /// <summary>
            /// Creates a new reader for the given text file path.
            /// </summary>
            /// <param name="filename">The name of the file to be read.</param>
            public SDFReader(string filename)
                : this(filename, null)
            {
            }

            /// <summary>
            /// Creates a new reader for the given text file path and encoding.
            /// </summary>
            /// <param name="filename">The name of the file to be read.</param>
            /// <param name="enc">The encoding used.</param>
            public SDFReader(string filename, Encoding enc)
                : this(new FileStream(filename, FileMode.Open), enc ?? Encoding.GetEncoding(1252))
            {
            }

            private SDFRecord _current;

            /// <summary>
            /// Returns the next SDF record (or null if at eof)
            /// </summary>
            /// <returns>A string array of fields or null if at the end of file.</returns>
            public SDFRecord GetSDFRecord()
            {
                lock ( this ) {
                    SDFRecord rec = new SDFRecord();
                    _current = fillSDFRecord(rec);
                    return _current;
                }
            }

            public SDFRecord Current
            {
                get { return _enumerator == null ? _current : _enumerator.Current; }
            }

            internal SDFRecord fillSDFRecord(SDFRecord record)
            {
                const string SDF_MEND = "M  END";
                const int MOL = 0;
                const int PROP = 1;
                int mode = MOL;
                StringBuilder buf = new StringBuilder();
                Regex rxPropName = new Regex(@"^>[^<]+<([^>]+)>");
                string propname = null;
                bool emptyLine = false;

                string line, line_te;
                line = x_ReadLine();
                line_te = line != null ? line.TrimEnd() : null;
                while ( line != null && line_te != "$$$$" ) {
                    Match m = rxPropName.Match(line);
                    if ( mode == MOL ) {
                        if ( !m.Success ) {
                            buf.Append(line);
                            buf.Append('\n');
                        }
                        if ( line_te == SDF_MEND || m.Success ) {
                            mode = PROP;
                            if ( line_te == SDF_MEND ) {
                                record.Molecule = buf.ToString();
                            }
                            else
                                record.Molecule = String.Empty;

                            buf.Length = 0;
                        }
                    }

                    if ( mode == PROP ) {
                        if ( m.Success ) {
                            if ( propname != null )
                                record[propname] = buf.ToString();
                            buf.Length = 0;
                            emptyLine = false;
                            propname = m.Groups[1].Value;
                        }
                        else {
                            if ( emptyLine ) // delayed write of empty lines
                                buf.Append('\n');

                            emptyLine = line_te == "";
                            if ( !emptyLine ) {
                                if ( buf.Length > 0 )
                                    buf.Append('\n');
                                line = line.Trim('\r', '\n');
                                buf.Append(line);
                            }
                        }
                    }
                    line = x_ReadLine();
                    line_te = line != null ? line.TrimEnd() : null;
                }
                if ( propname != null )
                    record[propname] = buf.ToString();

                if ( mode != PROP )
                    return null;

                return record;
            }

            /// <summary>
            /// Disposes the SDFReader. The underlying stream is closed.
            /// </summary>
            public void Dispose()
            {
                // Closing the reader closes the underlying stream, too
                if ( m_Reader != null && m_ReaderOwner )
                    m_Reader.Close();
                else if ( m_Stream != null )
                    m_Stream.Close(); // In case we failed before the reader was constructed
                GC.SuppressFinalize(this);
            }

            string x_ReadLine()
            {
                // possible linebreaks: 
                // \r...
                // \r\n...
                // \n
                // \r\r\n
                if ( m_EmptyLine ) {
                    m_EmptyLine = false;
                    return "";
                }
                m_Buffer.Length = 0;
                int c;
                while ( (c = m_Reader.Read()) > 0 ) {
                    if ( c == '\n' ) {
                        return m_Buffer.ToString();
                    }
                    else if ( c == '\r' ) {
                        int d = m_Reader.Peek();
                        if ( d == -1 )
                            return m_Buffer.ToString();
                        else if ( (char)d == '\n' ) {
                            m_Reader.Read();
                            return m_Buffer.ToString();
                        }
                        else if ( (char)d == '\r' ) {
                            m_Reader.Read();
                            d = m_Reader.Peek();
                            if ( d == -1 )
                                return m_Buffer.ToString();
                            else if ( (char)d == '\n' ) {
                                m_Reader.Read();
                                return m_Buffer.ToString();
                            }
                            else {
                                m_EmptyLine = true;
                                return m_Buffer.ToString();
                            }
                        }
                        else {
                            return m_Buffer.ToString();
                        }

                    }
                    m_Buffer.Append((char)c);
                }
                if ( m_Buffer.Length != 0 )
                    return m_Buffer.ToString();
                else
                    return null; // eof
            }

            private class SdfEnumerator : IEnumerator<SDFRecord>
            {
                private SDFReader _reader;
                internal SdfEnumerator(SDFReader r)
                {
                    _reader = r;
                }

                private SDFRecord _record;
                public SDFRecord Current
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
                    _record = _reader.GetSDFRecord();
                    return _record != null;
                }

                public void Reset()
                {
                    throw new NotImplementedException();
                }
            }

            private SdfEnumerator _enumerator;

            public IEnumerator<SDFRecord> GetEnumerator()
            {
                if ( _enumerator == null )
                    _enumerator = new SdfEnumerator(this);
                return _enumerator;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                if ( _enumerator == null )
                    _enumerator = new SdfEnumerator(this);
                return _enumerator;
            }
        }

        /// <summary>
        /// Exception class for SDFReader exceptions.
        /// </summary>
        public class SDFReaderException : ApplicationException
        {
            /// <summary>
            /// Constructs a new exception object with the given message.
            /// </summary>
            /// <param name="message">The exception message.</param>
            public SDFReaderException(string message)
                : base(message)
            {
            }
        }
    }

    public class SdfSplitter : IDisposable
    {
        StreamReader m_sr;
        string m_OrigFile;
        string m_OrigDirectory;
        int m_SplitNo = 0;
        int m_CurrentRecord = 0;
        bool m_Eof;
        string m_CurrentSplitName;
        int m_SplitSize;
        int m_CurrentSplitSize = 0;
        string m_OutputExtension = null;

        public bool Eof
        {
            get
            {
                return m_Eof;
            }
        }
        public string CurrentSplitName
        {
            get
            {
                return m_CurrentSplitName;
            }
        }
        public int CurrentSplitSize
        {
            get
            {
                return m_CurrentSplitSize;
            }
        }
        public string OriginalDirectory
        {
            get
            {
                return m_OrigDirectory;
            }
        }
        public string OutputExtension
        {
            get
            {
                return m_OutputExtension;
            }
            set
            {
                m_OutputExtension = value;
            }
        }

        public SdfSplitter(string origfile, int splitsize, Encoding encoding)
        {
            m_sr = new StreamReader(origfile, encoding);

            m_OrigFile = origfile;
            m_OrigDirectory = Path.GetDirectoryName(m_OrigFile);
            m_SplitSize = splitsize;
            m_Eof = m_sr.EndOfStream;
        }

        public void Dispose()
        {
            if ( m_sr != null )
                m_sr.Close();
            GC.SuppressFinalize(this);
        }

        public bool NextSplit()
        {
            m_CurrentSplitSize = 0;
            m_CurrentSplitName = String.Empty;
            if ( m_Eof )
                return false;
            int imported = 0;
            m_CurrentSplitName = Path.Combine(m_OrigDirectory, string.Format("{0}_{1}{2}",
                Path.GetFileNameWithoutExtension(m_OrigFile), m_SplitNo,
                m_OutputExtension == null ? Path.GetExtension(m_OrigFile) : m_OutputExtension));
            using ( StreamWriter sw = new StreamWriter(m_CurrentSplitName, false, m_sr.CurrentEncoding) ) {
                while ( !m_Eof && imported < m_SplitSize ) {
                    string line, line_te = null;
                    int linesWritten = 0;
                    while ( (line = m_sr.ReadLine()) != null && (line_te = line.TrimEnd()) != "$$$$" ) {
                        foreach ( char c in line ) {
                            if ( !char.IsControl(c) || c == '\t' || c == '\r' || c == '\n' )
                                sw.Write(c);
                        }
                        sw.WriteLine();
                        if ( !string.IsNullOrEmpty(line) )
                            ++linesWritten;
                    }
                    if ( line_te == "$$$$" || linesWritten > 2 ) {
                        sw.WriteLine("$$$$");
                        ++imported;
                        ++m_CurrentRecord;
                    }
                    m_Eof = line == null;
                }
            }
            ++m_SplitNo;
            m_CurrentSplitSize = imported;
            return imported > 0;
        }
    }
}
