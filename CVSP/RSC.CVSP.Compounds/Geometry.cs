using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RSC.CVSP.Compounds
{
	public class Geometry
	{
        public static double AngleBetweenThreePoints(Tuple<double, double, double> vertex,
            Tuple<double, double, double> A, Tuple<double, double, double> B)
        {
            return AngleBetweenThreePoints(vertex.Item1, vertex.Item2, vertex.Item3, A.Item1, A.Item2, A.Item3,
                B.Item1, B.Item2, B.Item3);
        }

		/// <summary>
		/// Uses cosine theorem. Tested in GeometryAngleTest in ValidationByCodes.cs (which actually calls the
        /// tuple-based method above).
		/// </summary>
		public static double AngleBetweenThreePoints(double x_vertex, double y_vertex, double z_vertex,
			double x_pointA, double y_pointA, double z_pointA,
			double x_pointB, double y_pointB, double z_pointB)
		{
			double length_vertex_A = Math.Sqrt(
				Math.Pow(x_pointA - x_vertex, 2) + 
				Math.Pow(y_pointA - y_vertex, 2) + 
				Math.Pow(z_pointA - z_vertex, 2));

			double length_vertex_B = Math.Sqrt(
				Math.Pow(x_pointB - x_vertex, 2) + 
				Math.Pow(y_pointB - y_vertex, 2) + 
				Math.Pow(z_pointB - z_vertex, 2));

			double length_A_B = Math.Sqrt(
				Math.Pow(x_pointB - x_pointA, 2) + 
				Math.Pow(y_pointB - y_pointA, 2) + 
				Math.Pow(z_pointB - z_pointA, 2));

			double cosY = (Math.Pow(length_vertex_A, 2) + Math.Pow(length_vertex_B, 2) - Math.Pow(length_A_B, 2))
				/ (2 * length_vertex_A * length_vertex_B);

			return Math.Acos(cosY) * 180.0 / Math.PI;
		}
	}
}
