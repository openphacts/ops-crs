using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Compounds
{
	public class AtomValenceProperties
	{
		public static Dictionary<string, int> MaximumValences = new Dictionary<string, int>()
        {
            {"H", 1}, // but check this - what about boranes?
            { "Li", 1}, {"K", 1}, {"Na", 1}, {"Rb", 1}, {"Cs", 1}, {"Fr", 1},
            { "Be", 2}, {"Mg", 2}, {"Ca", 2}, {"Sr", 2}, {"Ba", 2}, {"Ra", 2},
            { "Sc", 3}, {"Ti", 8}, { "V", 5}, {"Cr",7}, {"Mn", 7}, { "Fe", 6}, {"Co", 6}, {"Ni", 6}, {"Cu", 6}, {"Zn", 6},
            { "Y", 3}, {"Zr", 4}, {"Nb", 6}, { "Mo", 6}, {"Tc", 7}, {"Ru", 8}, {"Rh", 6}, {"Pd", 8}, {"Ag", 4}, {"Cd", 2},
            { "Hf", 4},{ "Ta", 5}, {"W", 6}, {"Re", 8}, {"Os", 8}, {"Ir", 9}, {"Pt", 8}, {"Au", 5}, {"Hg", 4},
            { "B", 5}, {"Al", 5}, {"Ga", 4}, {"In", 3}, {"Tl", 3},
            { "C", 4}, { "Si", 6 }, { "Ge", 4 }, { "Sn", 4}, { "Pb", 4},
            { "N", 5}, {"P", 7}, { "As", 7}, {"Sb", 6}, {"Bi", 5},
            { "O", 2},  {"S", 6}, {"Se", 6}, {"Te", 6}, {"Po", 6},
            { "F", 1}, { "Cl", 7 }, { "Br", 7 }, { "I", 7}, { "At", 7},
            // noble gases
            { "He", 0}, {"Ne", 0}, {"Ar", 0}, {"Kr", 2}, {"Xe", 8}, {"Rn", 2},
            // lanthanides
            { "La", 3}, {"Ce", 4}, {"Pr", 4}, {"Nd", 3}, {"Pm", 3}, {"Sm", 3}, {"Eu", 3}, {"Gd", 3},
            {"Tb", 4}, {"Dy", 3}, { "Ho", 3}, {"Er", 3}, {"Tm", 3}, {"Yb", 3}, {"Lu", 3},
            // actinides, transactinides
            { "Ac", 3}, {"Th", 4}, {"Pa", 5}, {"U", 6}, {"Np", 7}, {"Pu", 8}, {"Am", 6}, {"Cm", 4}, {"Bk", 4}, {"Cf", 4},
            { "Es", 3}, {"Fm", 3}, {"Md", 3}, {"No", 3}, { "Lr", 3}, {"Rf", 4}, {"Db", 5}, {"Sg", 6}, {"Bh", 7}, {"Hs", 8},
            // complete guesses from now on
            { "Mt", 7}, {"Ds", 6}, {"Rg", 5}, {"Cn", 4}
        };

		private static List<int> none = new List<int>();
		private static List<int> zero = new List<int>() { 0 };
		private static List<int> one = new List<int>() { 1 };
		private static List<int> group2 = new List<int>() { 0, 1 };


		//do not include free elements (0 valency) - they are being checked referenced in ForbiddenFreeElementsInMixtures
		public static Dictionary<string, List<int>> ForbiddenValences = new Dictionary<string, List<int>>()
        {
            {"H", none}, // allow valence zero as it is a special case and handled oddly
            {"Li", none}, {"Na", none}, {"K", none}, {"Rb", none}, {"Cs", none}, {"Fr", none}, 
            {"Be", one}, {"Mg", none}, {"Ca", one}, {"Sr", one}, {"Ba", one}, {"Ra", one},
            // first row TMs
            {"Sc", none}, {"Ti", new List<int>(){1}}, {"V", new List<int>(){1}}, {"Cr",new List<int>(){1}}, {"Mn", none},
            {"Fe", new List<int>(){5}}, {"Co", none}, {"Ni", new List<int>(){1}}, {"Cu", none}, {"Zn", none},
            // second row TMs
            // It may well be that yttrium can have a valence of 1 but it doesn't do to say so.
            {"Y", new List<int>() {1,2}}, {"Zr", none}, {"Nb", one}, {"Mo", none}, 
            {"Tc", none}, {"Ru", none}, {"Rh", none},  {"Pd", new List<int>(){3}}, { "Ag", none}, {"Cd", none},
            // third row TMs
            {"Hf", one}, {"Ta", one}, {"W", none}, {"Re", none}, { "Os", none},
            {"Ir", none}, {"Pt", none}, { "Au", new List<int>() {4} }, {"Hg", new List<int>() {3}},
            // p-block
            { "B", none}, { "Al", none}, {"Ga", none}, {"In", none}, { "Tl", new List<int>() {2}},
            { "C", none }, { "Si", new List<int>{1,2,3} }, { "Ge", new List<int>{1,2,3} }, { "Sn", new List<int>() {1,3}}, { "Pb", new List<int>() {1,3}},
            { "N", new List<int>() {4} }, { "P", new List<int>(){ 1,2 }}, {"As", none }, { "Sb", new List<int>(){1,2,4}},{"Bi", new List<int>() {1}},
            { "O", none}, {"S", none}, {"Se", new List<int>() { 5 }}, { "Te", new List<int>() {1,3}}, {"Po", new List<int>() {1,3,5}},
            { "F", none}, { "Cl", new List<int>() { 2 }}, { "Br", new List<int>() { 4,6 }}, { "I", new List<int>() { 4,6 }}, { "At", new List<int>() {2,4,6}},
            // noble gases
            { "He", none}, { "Ne",none}, { "Ar",none}, {"Kr", none}, {"Xe",none}, {"Rn", none},
            // lanthanides
            { "La", one}, { "Ce", one}, {"Pr", one}, {"Nd", one}, {"Pm", one}, {"Sm", one}, {"Eu", one}, { "Gd", one},
            {"Tb", one}, {"Dy", one}, { "Ho", one}, { "Er", one}, {"Tm", one}, {"Yb", one}, {"Lu", one},
            // actinides, transactinides
            { "Ac", one}, { "Th", one}, {"Pa", one},
            { "U", one}, {"Np", new List<int>() {1,2}}, {"Pu", new List<int>() {1,2}}, {"Am", one},
            { "Cm", new List<int>() {1,2}}, {"Bk", new List<int>() {1,2}}, {"Cf", one}, { "Es", one}, { "Fm", one},
            { "Md", one}, { "No", one},
            { "Lr", new List<int>() {1,2}}, { "Rf", new List<int>() {1,2,3}}, { "Db", new List<int>() {1,2,3,4}}, {"Sg", new List<int>() {1,2,3,4,5}},
            { "Bh", new List<int>() {1,2,3,4,5,6}}, {"Hs", new List<int>() {1,2,3,4,5,6,7}},
            // allow everything for meitnerium onwards
            {"Mt", none}, { "Ds", none}, {"Rg", none}, {"Cn", none}
        };
	}
}
