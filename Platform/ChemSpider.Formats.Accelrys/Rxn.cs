using System;
using System.Collections.Generic;
using System.Linq;

using com.ggasoftware.indigo;

namespace MoleculeObjects
{
    public class ReactionException : Exception
    {
        public ReactionException()
            : base() { }

        public ReactionException(string message)
            : base(message) { }

        public ReactionException(string message, Exception ex)
            : base(message, ex) { }
    }

    public class MappedRxn : Rxn
    {
        public override string ToString()
        {
            return m_reaction.rxnfile();
        }

        public void SMILESToErrorStream()
        {
            Console.Error.WriteLine("reactants");
            Console.Error.WriteLine(String.Join("; ", from r in IndigoReactants() select r.canonicalSmiles()));
            Console.Error.WriteLine("products");
            Console.Error.WriteLine(String.Join("; ", from p in IndigoProducts() select p.canonicalSmiles()));
        }

        public void Automap()
        {
            foreach (IndigoObject molecule in IndigoMolecules())
            {
                foreach (IndigoObject bond in molecule.iterateBonds())
                {
                    m_reaction.setReactingCenter(bond, (ReactingCenter)(Indigo.RC_MADE_OR_BROKEN | Indigo.RC_UNCHANGED | Indigo.RC_ORDER_CHANGED));
                }
            }
            m_reaction.automap("discard ignore_charges ignore_valence");
            Console.WriteLine();
            foreach (IndigoObject product in IndigoProducts())
            {
                foreach (IndigoObject atom in product.iterateAtoms())
                {
                    Console.WriteLine(atom.symbol() + " " + m_reaction.atomMappingNumber(atom));
                }
            }
            Console.WriteLine();
            if (HasUnmappedAtomsInProducts())
            {
                foreach (IndigoObject r in IndigoReactants())
                {
                    foreach (IndigoObject atom in r.iterateAtoms())
                    {
                        Console.WriteLine(atom.symbol() + " " + m_reaction.atomMappingNumber(atom));
                    }
                }
                BothSides.ToList().ForEach(r => m_reaction.addReactant(indigo.loadMolecule(r.ct())));
                throw new ReactionException("could not map reaction");
            }
        }

        public bool HasUnmappedAtomsInProducts()
        {
            foreach (IndigoObject product in IndigoProducts())
            {
                foreach (IndigoObject atom in product.iterateAtoms())
                {
                    if (m_reaction.atomMappingNumber(atom) == 0)
                    {
                       return true;
                    }
                }
            }
            return false;
        }

        public MappedRxn(IEnumerable<Molecule> reactants, IEnumerable<Molecule> products, IEnumerable<Molecule> bothsides) : base(reactants, products, bothsides)
        {
            indigo.setOption("aam-timeout", 60000);
            try
            {
                Automap();
                Reactants = from r in IndigoReactants() select MoleculeFactory.Molecule(r.molfile());
                Products = from p in IndigoProducts() select MoleculeFactory.Molecule(p.molfile());
            }
            catch (ReactionException r)
            {
                SMILESToErrorStream();
                throw new ReactionException("Indigo reaction mapping failed: " + r.Message);
            }
        }
    }

    public class Rxn
    {
        protected Indigo indigo;
        protected IndigoObject m_reaction;
        public IEnumerable<GenericMolecule> Reactants { get; protected set; }
        public IEnumerable<GenericMolecule> Products { get; protected set; }
        public IEnumerable<GenericMolecule> BothSides { get; protected set; }

        protected IEnumerable<IndigoObject> IndigoMolecules()
        {
            foreach (IndigoObject m in m_reaction.iterateMolecules())
            {
                yield return m;
            }
        }

        protected IEnumerable<IndigoObject> IndigoReactants()
        {
            foreach (IndigoObject r in m_reaction.iterateReactants())
            {
                yield return r;   
            }
        }

        protected IEnumerable<IndigoObject> IndigoProducts()
        {
            foreach (IndigoObject p in m_reaction.iterateProducts())
            {
                yield return p;
            }
        }

        /// <summary>
        /// Returns the .rxn file as a string. The reactants come first, followed by the products.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var headerlines = new List<string>();
            headerlines.Add("$RXN");
            headerlines.Add("");
            headerlines.Add("");
            headerlines.Add("");
            headerlines.Add(String.Format("{0}{1}",
                Reactants.Count().ToString().PadLeft(3),
                Products.Count().ToString().PadLeft(3)));
            var labelledReactants = from r in Reactants select "$MOL" + Environment.NewLine + r.ct();
            var labelledProducts = from p in Products select "$MOL" + Environment.NewLine + p.ct();
            return (String.Join(Environment.NewLine, headerlines) + Environment.NewLine
                + String.Concat(labelledReactants) 
                + String.Concat(labelledProducts));
        }

        // constructors
        private Rxn(IndigoObject reaction)
        {
            indigo = new Indigo();
            m_reaction = reaction;
        }

        public Rxn(IEnumerable<GenericMolecule> reactants, IEnumerable<GenericMolecule> products, IEnumerable<GenericMolecule> bothsides)
        {
            Reactants = reactants; Products = products; BothSides = bothsides;
            indigo = new Indigo(); m_reaction = indigo.createReaction();
            Reactants.ToList().ForEach(r => m_reaction.addReactant(indigo.loadMolecule(r.ct())));
            Products.ToList().ForEach(p => m_reaction.addProduct(indigo.loadMolecule(p.ct())));
        }

        /// <summary>
        /// If you want to generate a Rxn object in any other way, use RxnFactory.
        /// </summary>
        public Rxn(IEnumerable<GenericMolecule> reactants, IEnumerable<GenericMolecule> products)
        {
            Reactants = reactants; Products = products; BothSides = Enumerable.Empty<GenericMolecule>();
            indigo = new Indigo();
            m_reaction = indigo.createReaction();
            Reactants.ToList().ForEach(r => m_reaction.addReactant(indigo.loadMolecule(r.ct())));
            Products.ToList().ForEach(p => m_reaction.addProduct(indigo.loadMolecule(p.ct())));
        }
    }

    public static class RxnFactory
    {
        public static Rxn Rxn(Sdf reactants, Sdf products)
        {
            return new Rxn(reactants.allAsGenerics, products.allAsGenerics);
        }

        public static Tuple<List<GenericMolecule>, List<GenericMolecule>> ToReactantsAndProducts(string s)
        {
            List<string> lines = s.SplitOnNewLines();
            if (lines[0] != "$RXN") throw new Exception("Not an rxn file");
            int rcount = Convert.ToInt32(lines[4].Substring(0, 3));
            int pcount = Convert.ToInt32(lines[4].Substring(3, 3));
            List<string> molfiles = (from m in s.Split(new string[] { "$MOL" }, StringSplitOptions.None) select m.Substring(1)).ToList();
            var reactants = (from m in molfiles.Skip(1).Take(rcount) select MoleculeFactory.FromMolV2000(m)).ToList();
            var products = (from m in molfiles.Skip(1 + rcount).Take(pcount) select MoleculeFactory.FromMolV2000(m)).ToList();
            return Tuple.Create(reactants, products);
        }

        public static MappedRxn MappedRxn(string s)
        {
            var RaP = ToReactantsAndProducts(s);
            return new MappedRxn(from r in RaP.Item1 select new Molecule(r.Headers, r.IndexedAtoms, r.IndexedBonds, r.Properties),
                from p in RaP.Item2 select new Molecule(p.Headers, p.IndexedAtoms, p.IndexedBonds, p.Properties),
                Enumerable.Empty<Molecule>());
        }

        public static Rxn Rxn(string s)
        {
            var RaP = ToReactantsAndProducts(s);
            return new Rxn(RaP.Item1, RaP.Item2);
        }

        public static Rxn Rxn(Sdf reactants, Sdf products, Sdf bothsides)
        {
            return new Rxn(reactants.allAsGenerics.Concat(bothsides.allAsGenerics).ToList(),
                products.allAsGenerics.Concat(bothsides.allAsGenerics).ToList());
        }

        public static Rxn Rxn(Sdf sdf)
        {
            List<GenericMolecule> roleMolecules = sdf.allAsGenerics.Where(m => m.HasProperty("role")).ToList();

            var reactants = (from r in roleMolecules.Where(m => m.Property("role").Contains("reactant")) select r).ToList();
            var products = (from p in roleMolecules.Where(m => m.Property("role").Contains("product")) select p).ToList();

            return new Rxn(reactants, products);
        }
    }
}
