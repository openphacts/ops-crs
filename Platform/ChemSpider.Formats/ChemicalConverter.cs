using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.ggasoftware.indigo;
using System.IO;

namespace ChemSpider.Molecules
{
    public class ChemicalConverter
    {
        public enum Clean
        {
            DoClean,
            DontClean,
            AutoClean
        }

        public static void ConvertFile(string ifile, string ofile, string oformat, string clean)
        {
            Indigo indigo = new Indigo();
            indigo.setOption("molfile-saving-mode", oformat ?? "auto");

            Clean eClean;
            if ( clean == "yes" )
                eClean = Clean.DoClean;
            else if ( clean == "no" )
                eClean = Clean.DontClean;
            else
                eClean = Clean.AutoClean;

            int i = 0;
            StreamWriter sw = null;
            Action<IndigoObject> writeAction = (mol) => { };
            if ( ofile.EndsWith(".sdf", StringComparison.InvariantCultureIgnoreCase) ) {
                sw = new StreamWriter(ofile);
                writeAction = (mol) => {
                    sw.WriteLine(mol.molfile().TrimEnd());
                    sw.WriteLine("$$$$");
                };
            }
            else if ( ofile.EndsWith(".mol", StringComparison.InvariantCultureIgnoreCase) ) {
                writeAction = (mol) => {
                    using ( StreamWriter sw2 = new StreamWriter(String.Format("{0}-{1}.mol", Path.GetFileNameWithoutExtension(ofile), i++)) ) {
                        sw2.WriteLine(mol.molfile().TrimEnd());
                    }
                };
            }
            else if ( ofile.EndsWith(".smi", StringComparison.InvariantCultureIgnoreCase) || ofile.EndsWith(".sma", StringComparison.InvariantCultureIgnoreCase) ) {
                sw = new StreamWriter(ofile);
                writeAction = (mol) => {
                    sw.WriteLine(mol.smiles());
                };
            }

            Action<IndigoObject> cleanAction = (mol) => {
                if ( eClean == Clean.DoClean || eClean == Clean.AutoClean && !mol.hasCoord() )
                    mol.layout();
            };

            string last = null;
            try {
                i = 0;
                if ( ifile.EndsWith(".sdf", StringComparison.InvariantCultureIgnoreCase) ) {
                    foreach ( IndigoObject mol in indigo.iterateSDFile(ifile) ) {
                        last = mol.molfile();
                        cleanAction(mol);
                        writeAction(mol);
                        Console.Out.WriteLine(i++);
                    }
                }
                else if ( ifile.EndsWith(".smi", StringComparison.InvariantCultureIgnoreCase) ) {
                    foreach ( IndigoObject mol in indigo.iterateSmilesFile(ifile) ) {
                        last = mol.smiles();
                        cleanAction(mol);
                        writeAction(mol);
                        Console.Out.WriteLine(i++);
                    }
                }
                else if ( ifile.EndsWith(".sma", StringComparison.InvariantCultureIgnoreCase) ) {
                    using ( StreamReader sr = new StreamReader(ifile) ) {
                        while ( !sr.EndOfStream ) {
                            last = sr.ReadLine();
                            IndigoObject mol = indigo.loadSmarts(last);
                            cleanAction(mol);
                            writeAction(mol);
                            Console.Out.WriteLine(i++);
                        }
                    }
                }
            }
            catch ( Exception ex ) {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(ex.StackTrace);
                Console.Out.WriteLine(last);
            }
            finally {
                if ( sw != null )
                    sw.Close();
            }
        }
    }
}
