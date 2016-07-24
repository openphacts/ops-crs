using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ChemSpider.Utilities;
using InChINet;
using OpenBabelNet;

namespace ChemSpider.Molecules
{
	public static class InChI
	{
        public static string InChIToSMILES(string inchi)
        {
            return OpenBabel.GetInstance().convert(inchi, "inchi", "smiles").Trim();
        }

        public static string InChIToMol(string inchi, bool clean = true)
        {
            string mol = OpenBabel.GetInstance().convert(inchi, "inchi", "mol");
            return clean ? MolUtils.Clean(mol) : mol;
        }
	}
}