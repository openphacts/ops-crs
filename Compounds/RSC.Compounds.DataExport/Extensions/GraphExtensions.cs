using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VDS.RDF;

namespace RSC.Compounds.DataExport
{
    public static class GraphExtensions
    {
        /// <summary>
        /// Asserts a Literal Triple object for a given Uri - if that Uri does not exist in the Graph then add it.
        /// </summary>
        /// <param name="g">The graph.</param>
        /// <param name="subject">The subject Uri.</param>
        /// <param name="pred">The predicate Uri.</param>
        /// <param name="obj">The uri object.</param>
        /// <param name="type">The uri type.</param>
        public static void Assert(this Graph g, Uri subject, Uri pred, string obj, Uri type = null)
        {
            var subjectNode = g.CreateUriNode(subject);
            var predicateNode = g.CreateUriNode(pred);
            //Either set the data type of the string or set the language.
            ILiteralNode objectNode = type == null ? g.CreateLiteralNode(obj, "en") : g.CreateLiteralNode(obj, type);
            g.Assert(new Triple(subjectNode, predicateNode, objectNode));
        }

        /// <summary>
        /// Asserts a Uri Triple object for a given Uri - if that Uri does not exist in the Graph then add it.
        /// </summary>
        /// <param name="g">The graph.</param>
        /// <param name="subject">The subject Uri.</param>
        /// <param name="pred">The predicate Uri.</param>
        /// <param name="obj">The uri object.</param>
        public static void Assert(this Graph g, Uri subject, Uri pred, Uri obj)
        {
            var subjectNode = g.CreateUriNode(subject);
            var predicateNode = g.CreateUriNode(pred);
            var objectNode = g.CreateUriNode(obj);
            g.Assert(new Triple(subjectNode, predicateNode, objectNode));
        }
    }
}
