using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoleculeObjects
{
    public class CdxText : CdxObject
    {
        public string Value { get; private set; }
        public Tuple<double, double> XY { get; private set; }
        public Tuple<double, double> Centroid { get; private set; }
        public Dictionary<string, List<string>> BoldFaceDictionary { get; private set; }

        public CdxText(CdxObject e)
            : base(e)
        {
            try
            {
                Value = HasProperty("Text") ? Property("Text").Item1 : "";
                BoldFaceDictionary = HasProperty("Text") ? Property("Text").Item2 : new Dictionary<string, List<string>>();
                XY = HasProperty("2DPosition") ? Property("2DPosition") : Parent.Property("2DPosition");
                Centroid = HasProperty("BoundingBox") 
                    ? Tuple.Create((Property("BoundingBox").Item1.Item1 + Property("BoundingBox").Item2.Item1)/2,
                    (Property("BoundingBox").Item1.Item2 + Property("BoundingBox").Item2.Item2)/2)
                    : XY;
            }
            catch (Exception x)
            {
                Console.Error.WriteLine("problem with textifying object" + e.ToString(0) + x);
            }
        }
    }
}
