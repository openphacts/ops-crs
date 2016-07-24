using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Compounds
{
    public class CtMonad
    {
        public string ConnectionTable { get; protected set; }
        public List<Issue> Issues { get; protected set; }

        /// <summary>
        /// contains SMIRKS strings applied.
        /// </summary>
        public List<string> Transformations { get; protected set; }

        /// <summary>
        /// Adds additional issues while preserving the connection table.
        /// </summary>
        public CtMonad AddIssues(List<Issue> newIssues)
        {
            var finalIssues = new List<Issue>(Issues);
            finalIssues.AddRange(newIssues);
            return new CtMonad(ConnectionTable, finalIssues.Distinct().ToList(), Transformations);
        }

        public CtMonad AddTransformation(string newCt, string SMIRKS)
        {
            var finalTransformations = new List<string>(Transformations);
            finalTransformations.Add(SMIRKS);
            return new CtMonad(newCt, Issues, finalTransformations);
        }

        /// <summary>
        /// This is to make sure we don't lose track of the issues that are raised during standardization.
        /// </summary>
        public CtMonad(string ct, List<Issue> issues, List<string> transformations)
        {
            ConnectionTable = ct;
            Issues = issues;
            Transformations = transformations;
        }
    }
}
