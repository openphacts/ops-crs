using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using ChemSpider.Molecules;
using InChINet;
using OpenBabelNet;
using ChemSpider.Data.Database;
using MoleculeObjects;

namespace ChemSpider.Formats
{
    public enum ChemicalFormats
    {
        ChemicalName,
        SMILES,
        InChI,
        InChIKey,
        MOL,
        CSID,
    }

    public static class ChemConverter
    {
        private static readonly Dictionary<string, ChemicalFormats> _converters =
            Enum.GetNames(typeof(ChemicalFormats)).ToDictionary(s => s, s => (ChemicalFormats)Enum.Parse(typeof(ChemicalFormats), s));

        private static string name2mol(string source)
        {
            string mol;
            ChemIdUtils.name2str(source, out mol, true, false);
            return mol;
        }

        private static int InChIKey2CSID(string inchi_key)
        {
            ChemSpiderDB csdb = new ChemSpiderDB();
            return csdb.InChIKeyToCSID(inchi_key);
        }

        private static string CSID2Title(int csid)
        {
            ChemSpiderDB csdb = new ChemSpiderDB();
            return csdb.GetCompoundTitle(csid);
        }

        private static string CSID2Mol(int csid)
        {
            ChemSpiderBlobsDB csbdb = new ChemSpiderBlobsDB();
            return SdfRecord.FromString(csbdb.getSdf(csid)).Mol;
        }

        private static string CSID2InChI(int csid)
        {
            ChemSpiderDB csdb = new ChemSpiderDB();
            return csdb.CSID2InChI(csid);
        }

        private static string CSID2InChIKey(int csid)
        {
            ChemSpiderDB csdb = new ChemSpiderDB();
            return csdb.CSID2InChIKey(csid);
        }

        private static string CSID2SMILES(int csid)
        {
            ChemSpiderDB csdb = new ChemSpiderDB();
            return csdb.CSID2SMILES(csid);
        }

        public static string Convert(string source, string sourceFormat, string targetFormat)
        {
            switch ( _converters[sourceFormat] ) {
                case ChemicalFormats.ChemicalName:
                    switch ( _converters[targetFormat] ) {
                        case ChemicalFormats.ChemicalName:
                            return source;
                        case ChemicalFormats.SMILES:
                            return MolUtils.MolToSMILES(
                                name2mol(source));
                        case ChemicalFormats.InChI:
                            return InChIUtils.mol2InChI(
                                name2mol(source), InChIFlags.Standard);
                        case ChemicalFormats.InChIKey:
                            return InChIUtils.mol2InChIKey(
                                name2mol(source), InChIFlags.Standard);
                        case ChemicalFormats.MOL:
                            return name2mol(source);
                        case ChemicalFormats.CSID:
                            return InChIKey2CSID(
                                InChIUtils.mol2InChIKey(
                                name2mol(source), InChIFlags.Standard)).ToString();
                        default:
                            throw new NotImplementedException(String.Format("Conversion {0} => {1} not implemented", sourceFormat, targetFormat));
                        }
                case ChemicalFormats.SMILES:
                    switch ( _converters[targetFormat] ) {
                        case ChemicalFormats.ChemicalName:
                            return CSID2Title(
                                InChIKey2CSID(
                                InChIUtils.InChIToInChIKey(
                                MolUtils.SMILESToInChI(source))));
                        case ChemicalFormats.SMILES:
                            return source;
                        case ChemicalFormats.InChI:
                            return MolUtils.SMILESToInChI(source);
                        case ChemicalFormats.InChIKey:
                            return InChIUtils.InChIToInChIKey(
                                MolUtils.SMILESToInChI(source));
                        case ChemicalFormats.MOL:
                            return MolUtils.SMILESToMol(source);
                        case ChemicalFormats.CSID:
                            return InChIKey2CSID(
                                InChIUtils.InChIToInChIKey(
                                MolUtils.SMILESToInChI(source))).ToString();
                        default:
                            throw new NotImplementedException(String.Format("Conversion {0} => {1} not implemented", sourceFormat, targetFormat));
                    }
                case ChemicalFormats.InChI:
                    switch ( _converters[targetFormat] ) {
                        case ChemicalFormats.ChemicalName:
                            return CSID2Title(
                                InChIKey2CSID(
                                InChIUtils.InChIToInChIKey(source)));
                        case ChemicalFormats.SMILES:
                            return MolUtils.MolToSMILES(
                                InChI.InChIToMol(source)); // Thin place - OB is not good with conversions
                        case ChemicalFormats.InChI:
                            return source;
                        case ChemicalFormats.InChIKey:
                            return InChIUtils.InChIToInChIKey(source);
                        case ChemicalFormats.MOL:
                            return InChI.InChIToMol(source); // Thin place - OB is not good with conversions
                        case ChemicalFormats.CSID:
                            return InChIKey2CSID(
                                InChIUtils.InChIToInChIKey(source)).ToString();
                        default:
                            throw new NotImplementedException(String.Format("Conversion {0} => {1} not implemented", sourceFormat, targetFormat));
                    }
                case ChemicalFormats.InChIKey:
                    switch ( _converters[targetFormat] ) {
                        case ChemicalFormats.ChemicalName:
                            return CSID2Title(
                                InChIKey2CSID(source));
                        case ChemicalFormats.SMILES:
                            return MolUtils.MolToSMILES(
                                CSID2Mol(
                                InChIKey2CSID(source)));
                        case ChemicalFormats.InChI:
                            return CSID2InChI(
                                InChIKey2CSID(source));
                        case ChemicalFormats.InChIKey:
                            return source;
                        case ChemicalFormats.MOL:
                            return CSID2Mol(
                                InChIKey2CSID(source));
                        case ChemicalFormats.CSID:
                            return InChIKey2CSID(source).ToString();
                        default:
                            throw new NotImplementedException(String.Format("Conversion {0} => {1} not implemented", sourceFormat, targetFormat));
                    }
                case ChemicalFormats.MOL:
                    switch ( _converters[targetFormat] ) {
                        case ChemicalFormats.ChemicalName:
                            return CSID2Title(
                                InChIKey2CSID(
                                InChIUtils.mol2InChIKey(source, InChIFlags.Standard)));
                        case ChemicalFormats.SMILES:
                            return MolUtils.MolToSMILES(source);
                        case ChemicalFormats.InChI:
                            return InChIUtils.mol2InChI(source, InChIFlags.Standard);
                        case ChemicalFormats.InChIKey:
                            return InChIUtils.mol2InChIKey(source, InChIFlags.Standard);
                        case ChemicalFormats.MOL:
                            return source;
                        case ChemicalFormats.CSID:
                            return InChIKey2CSID(
                                InChIUtils.mol2InChIKey(source, InChIFlags.Standard)).ToString();
                        default:
                            throw new NotImplementedException(String.Format("Conversion {0} => {1} not implemented", sourceFormat, targetFormat));
                    }
                case ChemicalFormats.CSID:
                    switch ( _converters[targetFormat] ) {
                        case ChemicalFormats.ChemicalName:
                            return CSID2Title(int.Parse(source));
                        case ChemicalFormats.SMILES:
                            return CSID2SMILES(int.Parse(source));
                        case ChemicalFormats.InChI:
                            return CSID2InChI(int.Parse(source));
                        case ChemicalFormats.InChIKey:
                            return CSID2InChIKey(int.Parse(source));
                        case ChemicalFormats.MOL:
                            return CSID2Mol(int.Parse(source));
                        case ChemicalFormats.CSID:
                            return int.Parse(source).ToString();
                        default:
                            throw new NotImplementedException(String.Format("Conversion {0} => {1} not implemented", sourceFormat, targetFormat));
                    }
                default:
                    throw new NotImplementedException(String.Format("Conversion {0} => {1} not implemented", sourceFormat, targetFormat));
            }
        }

        public static string Convert(byte[] source, string sourceFormat, string targetFormat)
        {
            throw new NotImplementedException();
        }

        public static byte[] ConvertBinary(string source, string sourceFormat, string targetFormat)
        {
            throw new NotImplementedException();
        }

        public static byte[] ConvertBinary(byte[] source, string sourceFormat, string targetFormat)
        {
            throw new NotImplementedException();
        }
    }
}
