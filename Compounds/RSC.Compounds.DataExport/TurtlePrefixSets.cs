using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.DataExport
{
    public class TurtlePrefixSets
    {
        //The prefixes required for the issues.
        public static Dictionary<string, Uri> Issues = new Dictionary<string, Uri>()
        {
            { "rdf", Turtle.ns_rdf }, 
            { "rdfs", Turtle.ns_rdfs }, 
            { "xsd", Turtle.ns_xsd }, 
            { "cheminf", Turtle.ns_cheminf },
            { "owl", Turtle.ns_owl },
            { "void", Turtle.ns_void }
        };

        //The list of prefixes required for the linkset.
        public static Dictionary<string, Uri> Linkset = new Dictionary<string, Uri>()
        {
            { "rdf", Turtle.ns_rdf },
            { "rdfs", Turtle.ns_rdfs }, 
            { "xsd", Turtle.ns_xsd },
            { "void", Turtle.ns_void },
            { SkosPredicateExtensions.SKOS_PREFIX, Turtle.ns_skos },
        };

        //The list of prefixes required for parent child.
        public static Dictionary<string, Uri> ParentChild = new Dictionary<string, Uri>()
        {
             { "cheminf", Turtle.ns_cheminf },
             { "obo2", Turtle.ns_obo2 }
        };

        //The prefixes required for the properties.
        public static Dictionary<string, Uri> Properties = new Dictionary<string, Uri>()
        {
             { "rdf", Turtle.ns_rdf }, 
             { "rdfs", Turtle.ns_rdfs }, 
             { "xsd", Turtle.ns_xsd }, 
             { "cheminf", Turtle.ns_cheminf },
             { "obo", Turtle.ns_obo }, 
             { "ro", Turtle.ns_obo_ro }, 
             { "csid", Turtle.RDF_URI }, 
             { "qudt", Turtle.ns_qudt }, 
             { "void", Turtle.ns_void }
        };

        //The prefixes required for the synonyms.
        public static Dictionary<string, Uri> Synonyms = new Dictionary<string, Uri>()
        {
            { "rdf", Turtle.ns_rdf }, 
            { "void", Turtle.ns_void },
            { "owl", Turtle.ns_owl },
            { "cheminf", Turtle.ns_cheminf }
        };
    }
}
