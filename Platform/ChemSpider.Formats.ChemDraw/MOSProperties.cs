using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MoleculeObjects
{
    public class MOSProperties : IReactionProperties
    {
        private PartitionedCdx m_cdx;
        private Dictionary<string, string> m_properties = new Dictionary<string, string>();
        public Dictionary<string, string> Properties { get { return m_properties; } }

        public MOSProperties(PartitionedCdx cdx)
        {
            m_cdx = cdx;

            try {
                List<string> yieldCandidates = (from l in cdx.Labels
                                                where !l.Value.Contains("examples")
                                                select l.Value.Trim()).ToList();
                List<string> plainYields = (from l in yieldCandidates where Regex.IsMatch(l, "^[0-9> ]+%$") select l).ToList();
                if ( plainYields.Count > 0 ) {
                    m_properties.Add("yield", plainYields.First());
                }
                else {
                    m_properties.Add("yield", yieldCandidates.Where(l => Regex.IsMatch(l, "^[0-9> ]+%") || l.Contains("yield")).First());
                }
                m_properties.Add("conditions", String.Join("; ", cdx.LabelsInBox(cdx.reagentBox)));
                List<string> exampleCandidates = (from l in cdx.Labels where l.Value.Contains("examples") select l.Value).ToList();
                // assume a single example if no examples listed
                m_properties.Add("exampleCount", (exampleCandidates.Count > 0) ? exampleCandidates.First() : "1");
            }
            catch ( Exception e ) {
                foreach ( var l in cdx.Labels ) {
                    Console.Error.WriteLine(l.Value);
                }
                throw new Exception("Problem with parsing labels in cdx. Is it really from MOS?"
                    + Environment.NewLine + e.ToString());
            }
        }
    }
}
