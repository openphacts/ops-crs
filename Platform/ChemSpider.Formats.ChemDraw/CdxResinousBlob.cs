using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoleculeObjects
{
    public class CdxResinousBlob
    {
        public Tuple<double,double,double,double> BoundingBox { get; protected set;}

        public CdxResinousBlob(CdxObject o)
        {
            var bb = o.Property("BoundingBox");
            Tuple<double,double> centre = Tuple.Create(bb.Item2.Item1, bb.Item2.Item2);
            Tuple<double,double> edge = Tuple.Create(bb.Item1.Item1, bb.Item1.Item2);
            double length = centre.DistanceFrom(edge);

            BoundingBox = Tuple.Create(centre.Item1 - length, centre.Item2 - length,
                centre.Item1 + length, centre.Item2 + length);
        }
    }
}
