using System;
using System.Collections.Generic;
using System.Linq;

using com.ggasoftware.indigo;

namespace MoleculeObjects
{
    public class PartitionedCdx : Cdx
    {
        public enum ReactionProperties { NONE, MOS }

        public Tuple<double, double, double, double> reactantBox;
        public Tuple<double, double, double, double> productBox;
        public Tuple<double, double, double, double> reagentBox;

        private List<Tuple<double, double, double, double>> m_arrowBBs = new List<Tuple<double, double, double, double>>();
        private List<Tuple<double, double>> plusPositions = new List<Tuple<double, double>>();

        /// <summary>
        /// rp specifies whether not to add reaction properties, or add those according to
        /// the scheme in the popular RSC publication, Methods in Organic Synthesis
        /// 
        /// exampleCount: count of number of examples
        /// conditions: conditions under which the reaction was carried out
        /// yield: overall yield
        /// </summary>
        public IndigoObject ToIndigoObject(ReactionProperties rp)
        {
            Indigo i = new Indigo();
            IndigoObject rxn = i.createReaction();
            this.reactants().Item2.ForEach(r => rxn.addReactant(i.loadMolecule(r)));
            this.products().Item2.ForEach(p => rxn.addProduct(i.loadMolecule(p)));
            this.reagents().Item2.ForEach(c => rxn.addCatalyst(i.loadMolecule(c)));

            if (rp == ReactionProperties.MOS)
            {
                foreach (var propertyPair in new MOSProperties(this).Properties)
                {
                    rxn.setProperty(propertyPair.Key, propertyPair.Value);
                }
            }

            return rxn;
        }

        /// <summary>
        /// impliedly uses Molecule instead of GenericMolecule
        /// </summary>
        public MappedRxn ToMappedRxn()
        {
            var reactants = from r in this.reactants().Item2 select MoleculeFactory.Molecule(r);
            var products = from p in this.products().Item2 select MoleculeFactory.Molecule(p);
            var bothsides = from b in this.reagents().Item2 select MoleculeFactory.Molecule(b);
            return new MappedRxn(reactants, products, bothsides);
        }

        public Rxn ToRxn()
        {
            return new Rxn(from r in this.reactants().Item2 select MoleculeFactory.FromMolV2000(r),
                from p in this.products().Item2 select MoleculeFactory.FromMolV2000(p),
                from b in this.reagents().Item2 select MoleculeFactory.FromMolV2000(b));
        }

        public Tuple<int, List<string>> MoleculesInBox(Tuple<double, double, double, double> box, string label)
        {
            int expectedCount = plusPositions.Where(p => box.Contains(p)).Count() + 1;
            return new Tuple<int, List<string>>(expectedCount, FragmentsInBoxToMolFiles(CdxEnumerateOptions.EnumerateMarkush, box, label).ToList());
        }

        /// <summary>
        /// Returns count of expected products and molfiles for products found.
        /// </summary>
        /// <returns></returns>
        public Tuple<int, List<string>> products()
        {
            return MoleculesInBox(productBox, "PRODUCT");
        }

        /// <summary>
        /// Returns count of expected products and molfiles for reagents found.
        /// </summary>
        public Tuple<int, List<string>> reagents()
        {
            return MoleculesInBox(reagentBox, "REAGENT");
        }

        /// <summary>
        /// Returns count of expected products and molfiles for reactants found.
        /// </summary>
        public Tuple<int, List<string>> reactants()
        {
            return MoleculesInBox(reactantBox, "REACTANT");
        }

        /// <summary>
        /// Like a Cdx object, except it has been partitioned according to where all the arrows are.
        /// 
        /// Assume for the moment there is only one arrow.
        /// </summary>
        public PartitionedCdx(byte[] bytearray)
            : base(bytearray)
        {
            IEnumerable<CdxText> pluscandidates = (from t in this.CdxPathSelectObjects("//Text")
                                                   select new CdxText(t)).Where(t => t.Value == "+");
            plusPositions = (from t in pluscandidates select t.XY).ToList();
            IEnumerable<CdxObject> realarrows = (from g in this.CdxPathSelectObjects("//Graphic[@Arrow_Type]") select g);
            IEnumerable<CdxObject> bezierarrowsWithHeads = (from g in this.CdxPathSelectObjects("//Curve[@Arrowhead_Type and @Curve_Points]") select g);
            IEnumerable<CdxObject> bezierarrowsWithSiblings = (from g in this.CdxPathSelectObjects("//Curve[@Curve_Points and not(@Arrowhead_Type) and following-sibling::Curve]") select g);
            foreach (var a in realarrows)
            {
                var bb = a.Property("BoundingBox");
                m_arrowBBs.Add(new Tuple<double, double, double, double>(bb.Item1.Item1, bb.Item1.Item2, bb.Item2.Item1, bb.Item2.Item2));
            }
            // let's assume only five-point ones are arrows
            foreach (var b in bezierarrowsWithHeads.Concat(bezierarrowsWithSiblings))
            {
                IEnumerable<Tuple<double, double>> points = b.Property("Curve_Points");
                if (points.Count() == 5)
                {
                    m_arrowBBs.Add(Tuple.Create(points.First().Item1, points.First().Item2,
                        points.ElementAt(1).Item1, points.ElementAt(1).Item2));
                }
            }

            try
            {
                var arrow = m_arrowBBs.First();
                // left top, right bottom
                double lhs = (arrow.Item1 < arrow.Item3) ? arrow.Item1 : arrow.Item3;
                double rhs = (arrow.Item1 < arrow.Item3) ? arrow.Item3 : arrow.Item1;

                reactantBox = new Tuple<double, double, double, double>(0, 0, lhs, 720);
                productBox = new Tuple<double, double, double, double>(rhs, 0, 540, 720);
                reagentBox = new Tuple<double, double, double, double>(lhs - 36, 0, rhs + 36, 720);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                throw new MoleculeException("No identifiable bounding box " + Root.ToString());
            }
        }
    }
}
