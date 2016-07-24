using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.ggasoftware.indigo;
using ChemSpider.Formats.Accelrys;

namespace MoleculeObjects
{
    /// <summary>
    /// The idea is that this takes the sort of label that you find in ChemDraw files and
    /// parses it into Markush symbols and their replacements
    /// </summary>
    public class Enumeration
    {
        public Dictionary<string, List<string>> Mappings { get; protected set; }

        public List<string> Mapping(string key) { return new List<string>(Mappings[key]); }

        public bool Mapped(string key) { return Mappings.ContainsKey(key); }

        public bool HasMappings() { return Mappings.Count > 0; }

        public KeyValuePair<string, List<string>> TokenizeOnEquals(string s)
        {
            char[] equals = new char[] { '=' };
            string[] comma = new string[] { ", " }; // like this so as not to split chemical names!

            var tokenizedOnEquals = s.Split(equals);
            // case A. Assuming a single equals sign.
            if (tokenizedOnEquals.Count() == 2)
            {
                string before = tokenizedOnEquals[0].Trim();
                string after = tokenizedOnEquals[1].Trim();

                return new KeyValuePair<string, List<string>>(
                    // the notion here is that everything before the comma will be a name and not a generic
                    before.Contains(", ") ? before.Split(comma, StringSplitOptions.None).Last().Trim() : before.Trim(),
                    after.Contains(", ")
                                ? (from c in after.Split(comma, StringSplitOptions.None) where c.Length < 6 || c.Trim().EndsWith("yl") select c.Trim()).ToList()
                                 : new List<string> { after.Trim() });
            }
            // case B. more than one!
            else
            {
                throw new NotImplementedException("Haven't worked out how to handle multiple = yet!");
            }
        }

        public Enumeration Merge(Enumeration e2)
        {
            return new Enumeration(Mappings.Merge(e2.Mappings));
        }

        public KeyValuePair<string, List<string>> Head() { return Mappings.First(); }

        public Enumeration Tail()
        {
            return new Enumeration(Mappings.ToList<KeyValuePair<string, List<string>>>().Skip(1).ToDictionary(k => k.Key, k => k.Value));
        }

        private Enumeration(Dictionary<string, List<string>> d)
        {
            Mappings = d;
        }

        public Enumeration()
        {
            Mappings = new Dictionary<string, List<string>>();
        }

        public Enumeration(IEnumerable<string> l)
        {
            Mappings = new Dictionary<string, List<string>>();
            char[] semicolon = new char[] { ';' };
            foreach (var m in from s in
                              (from e in l select e.Split(semicolon)).SelectMany(i => i)
                              where !s.Trim().EndsWith("=")
                              select TokenizeOnEquals(s.Trim()))
            {
                if (Mappings.ContainsKey(m.Key))
                {
                    Mappings[m.Key].AddRange(m.Value);
                }
                else
                {
                    Mappings.Add(m.Key, m.Value.ToList());
                }
            }
        }
    }

    /// <summary>
    /// The CompoundEnumerator object provides methods for converting molfiles containing
    /// R groups (and the like) into molfiles representing InChIfiable molecules.
    /// 
    /// It runs off a fragment dictionary.  The fragments are mol files with a * representing the anchor point.
    /// </summary>
    public class CompoundEnumerator
    {
        private Dictionary<string, Fragment> m_fragments = new Dictionary<string, Fragment>();
        private Dictionary<string, Molecule> m_molecules = new Dictionary<string, Molecule>();

        /// <summary>
        /// This replaces the generics in the molfile with their expanded versions.
        /// </summary>
        public GenericMolecule SubstituteAllGenerics(GenericMolecule gm)
        {
            if (gm.HasProperty("Generics"))
            {
                var generics = gm.Property("Generics");
                if (generics.Any())
                {
                    var exMappings = gm.HasProperty("Expansion") ? new Enumeration(gm.Property("Expansion")) : new Enumeration();
                    var enMappings = gm.HasProperty("Enumeration") ? new Enumeration(gm.Property("Enumeration")) : new Enumeration();
                    var mappings = exMappings.Merge(enMappings);
                    // Generic mappings - map onto SELF
                    var unmapped = new Enumeration(from g in generics where !mappings.Mapped(g) select g + " = " + g);
                    // apply ALL of them.
                    var finalMappings = mappings.Merge(unmapped);
                    return SubstituteMarkushGeneric(gm, finalMappings, 0).First();
                }
                else
                {
                    return gm;
                }
            }
            else
            {
                return gm;
            }
        }

        /// <summary>
        /// Takes a connection table containing R groups of some description and parses them
        /// based on the list of specifications supplied.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Molecule> SubstituteMarkush(GenericMolecule scaffold, List<string> labels)
        {
            Enumeration e = new Enumeration(labels);
            return (from s in SubstituteMarkushGeneric(scaffold, e, 0) select new Molecule(s.Headers, s.IndexedAtoms, s.IndexedBonds, s.Properties));
        }

        public List<GenericMolecule> SubstituteMarkushGeneric(GenericMolecule scaffold, Enumeration mappings)
        {
            return SubstituteMarkushGeneric(scaffold, mappings, 0);
        }

        public List<GenericMolecule> SubstituteMarkushGeneric(GenericMolecule scaffold, Enumeration mappings, int depth)
        {
            if (mappings.HasMappings())
            {
                var result = new List<GenericMolecule>();
                var head = mappings.Head();
                // this is the tail of the list we pass back to ourselves having done the substitution
                var tail = mappings.Tail();                
                foreach (string r in head.Value)
                {
                    List<GenericMolecule> deeper = SubstituteMarkushGeneric(scaffold, tail, depth + 1);
                    result.AddRange(from d in deeper select SubstituteMarkush(d, head.Key, r));
                }
                return result.Any() ? result : new List<GenericMolecule>() { scaffold };
            }
            else
            {
                return new List<GenericMolecule>() { scaffold };
            }
        }

        public List<GenericMolecule> SubstituteMarkush(GenericMolecule scaffold, KeyValuePair<string,List<string>> mapping)
        {
            return (from s in mapping.Value select SubstituteMarkush(scaffold, mapping.Key, s)).ToList();
        }

        /// <summary>
        /// There is only one place we do the abbreviation–GenericMolecule mapping and that is HERE.
        /// </summary>
        public GenericMolecule SubstituteMarkush(GenericMolecule scaffold, string markush, string abbn)
        {
            if (scaffold.HasAtom(markush))
            {
                string canonicalabbn = abbn.ToLower();
                if (m_fragments.ContainsKey(canonicalabbn))
                {
                    return SubstituteMarkushFragment(scaffold, markush, canonicalabbn);
                }
                if (m_molecules.ContainsKey(canonicalabbn))
                {
                    return SubstituteMarkushMolecule(scaffold, markush, canonicalabbn);
                }
                throw new MoleculeException("Unrecognized substituent (" + abbn + "), for (" + markush + ") in scaffold:" + scaffold);
            }
            else
            {
                return scaffold;
            }
        }
       
        /// <summary>
        /// This dumps the molecule represented by abbreviation into the overall GenericMolecule.
        /// </summary>
        public GenericMolecule SubstituteMarkushMolecule(GenericMolecule scaffold,string markush, string abbreviation)
        {
            GenericMolecule abbreviated = m_molecules[abbreviation];
            return MoleculeFactory.Merge(scaffold, abbreviated);
        }

        /// <summary>
        /// Takes a GenericMolecule containing an R group specified by the string called markush
        /// and replaces it with the substituent specified by substituent.
        /// </summary>
        /// <param name="scaffold">Generic molecule scaffold.</param>
        /// <param name="markush">The string to be substituted (for example R).</param>
        /// <param name="abbn">What to substitute it with (for example Et).</param>
        /// <returns>The substituted GenericMolecule.</returns>
        public GenericMolecule SubstituteMarkushFragment(GenericMolecule scaffold, string markush, string abbn)
        {
            IEnumerable<int> markushAtomIDs = scaffold.AtomsByLabel(markush);

            Dictionary<int,Atom> newAtoms = new Dictionary<int,Atom>();
            Dictionary<int,Bond> newBonds = new Dictionary<int,Bond>();

            foreach (int markushAtomID in markushAtomIDs)
            {
                Fragment f = m_fragments[abbn];
                var posedFragment = f.PosedFragment(scaffold, markushAtomID);
                int fragAnchorAtom = f.FragmentAnchorIndex;
                int starNo = f.StarIndex;

                Dictionary<int, int> fragmentScaffoldAtomMapping = new Dictionary<int, int>();
                fragmentScaffoldAtomMapping.Add(fragAnchorAtom, markushAtomID);
                newAtoms.Add(markushAtomID, posedFragment.IndexedAtoms[fragAnchorAtom]);
                foreach (var pair in posedFragment.IndexedAtoms.Where(k => k.Key != starNo && k.Key != fragAnchorAtom))
                {
                    int index = scaffold.IndexedAtoms.Count + newAtoms.Where(k => !scaffold.IndexedAtoms.ContainsKey(k.Key)).Count() + 1;
                    newAtoms.Add(index, pair.Value);
                    fragmentScaffoldAtomMapping.Add(pair.Key, index); // maps initial atom index on to new atom index
                }

                foreach (Bond b in posedFragment.IndexedBonds.Values.Where(b => !b.EitherAtomIDEquals(starNo)))
                {
                    int index = scaffold.IndexedBonds.Count +  newBonds.Count + 1;
                    int i1 = fragmentScaffoldAtomMapping[b.firstatomID];
                    int i2 = fragmentScaffoldAtomMapping[b.secondatomID];
                    newBonds.Add(index, new Bond(newAtoms[i1], newAtoms[i2], i1, i2, b.order, b.bondStereo));
                }
            }
            var finalExistingAtoms = from p in scaffold.IndexedAtoms
                                     select newAtoms.Keys.Contains(p.Key)
                                        ? new KeyValuePair<int, Atom>(p.Key, newAtoms[p.Key])
                                        : p;
            var finalExtraAtoms = from q in newAtoms 
                                  where !scaffold.IndexedAtoms.Keys.Contains(q.Key)
                                  select new KeyValuePair<int,Atom>(q.Key, newAtoms[q.Key]);
            var finalAtoms = finalExistingAtoms.Concat(finalExtraAtoms).ToDictionary(p => p.Key, p => p.Value);
            Dictionary<int, Bond> finalBonds = scaffold.IndexedBonds.Concat(newBonds).ToDictionary(p => p.Key, p => p.Value);
            Dictionary<string, List<string>> finalProps = scaffold.Properties
                .Merge(new Dictionary<string,List<string>> {{ "CompoundEnumeration", 
                                                                 new List<string> { String.Format("markush = {0}, substituent = {1}", markush, abbn)}}}).ToDictionary(p => p.Key, p => p.Value);
            return new GenericMolecule(scaffold.Headers, finalAtoms, finalBonds, finalProps);
        }

        /// <summary>
        /// Generates a CompoundEnumerator with a built-in library of fragments
        /// </summary>
        public CompoundEnumerator()
        {
            var sdf = new Sdf(Resources.fragments);
            m_molecules = (from f in sdf.MoleculesIndexedByProperty("abbreviations")
                           select new KeyValuePair<string, Molecule>(f.Key.ToLower(), f.Value)).Distinct().ToDictionary(k => k.Key, k => k.Value);
            foreach (var f in sdf.GenericsIndexedByProperty("abbreviations"))
            {
                try
                {
                    m_fragments.Add(f.Key.ToLower(), new Fragment(f.Value));
                }
                catch
                {
                    Console.Error.WriteLine(f.Key + " " + f.Value);
                }
            }
        }
    }
}
