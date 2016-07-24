using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChemSpider.Molecules
{
    /// <summary>
    /// Class contains static functions for relating boiling points to pressure.
    /// </summary>
    public static class Nomograph
    {
        /// <summary>
        /// Gas constant in joules per kelvin per mole.
        /// </summary>
        public const double R = 8.31446;
 
        /// <summary>
        /// Returns a boiling point according to Clausius-Clapeyron assuming the
        /// Trouton-Everett-Hildebrand rule for the enthalpy of vaporization.
        /// As per Goodman, Kirby and Haustedt, Tetrahedron Lett., 2000, 41, 9879-9882.
        /// doi:10.1016/S0040-4039(00)01754-8
        /// </summary>
        /// <param name="pressure">Pressure in atmospheres.</param>
        /// <param name="bpAtAtmosphericPressure">Boiling point in kelvin.</param>
        /// <returns></returns>
        public static double BoilingPoint(double pressure, double bpAtAtmosphericPressure)
        {
            double DeltaHvap = EnthalpyOfVaporization_TroutonHildebrandEverett(bpAtAtmosphericPressure); // Trouton--Hildebrand--Everett rule
            return BoilingPoint(pressure, bpAtAtmosphericPressure, DeltaHvap);
        }

        /// <summary>
        /// Returns enthalpy of vaporization according to the Trouton--Hildebrand--Everett approximation.
        /// </summary>
        /// <param name="Tb_atm">Boiling point at atmospheric pressure.</param>
        public static double EnthalpyOfVaporization_TroutonHildebrandEverett(double Tb_atm)
        {
            return (4.4 + Math.Log(Tb_atm)) * R * Tb_atm;
        }

        /// <summary>
        /// Returns a boiling point according to Clausius-Clapeyron.
        /// As per Goodman, Kirby and Haustedt, Tetrahedron Lett., 2000, 41, 9879-9882.
        /// doi:10.1016/S0040-4039(00)01754-8
        /// </summary>
        /// <param name="pressure">Pressure in atmospheres</param>
        /// <param name="bpAtAtmosphericPressure">Boiling point in kelvin</param>
        /// <param name="EnthalpyOfVaporization">Enthalpy of vaporization in joules per kelvin per mole.</param>
        public static double BoilingPoint(double pressure, double bpAtAtmosphericPressure, double EnthalpyOfVaporization)
        {
            return 1 / ((1 / bpAtAtmosphericPressure) + Math.Log(pressure) / (-EnthalpyOfVaporization / R));
        }

        /// <summary>
        /// Returns an atmospheric boiling point according to Clausius-Clapeyron.
        /// As per Goodman, Kirby and Haustedt, Tetrahedron Lett., 2000, 41, 9879-9882.
        /// doi:10.1016/S0040-4039(00)01754-8
        /// </summary>
        /// <param name="experimentalPressure">Pressure in atmospheres</param>
        /// <param name="experimentalBp">Boiling point in kelvin</param>
        public static double BoilingPointAtAtmosphericPressure(double experimentalPressure, double experimentalBp)
        {
            double result = BoilingPoint(experimentalPressure, experimentalBp);
            double guess = experimentalBp;
            int iters = 0;
            while (Math.Abs(result - experimentalBp) > 0.01 && iters < 50)
            {
                iters++;
                if (result < experimentalBp) guess = guess + (experimentalBp - result) / 2;
                if (result > experimentalBp) guess = guess - (result - experimentalBp) / 2;
                result = BoilingPoint(experimentalPressure, guess);
            }
            return guess;
        }

    }
}
