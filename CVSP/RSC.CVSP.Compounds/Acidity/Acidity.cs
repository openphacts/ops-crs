using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Xml.XPath;
using System.Xml.Linq;

using com.ggasoftware.indigo;
using MoleculeObjects;
using RSC.Logging;
using RSC.CVSP;

namespace RSC.CVSP.Compounds
{
    public class Acidity : IAcidity
    {
		private Indigo s_indigo = new Indigo();
		public Acidity()
        {
			s_indigo.setOption("ignore-stereochemistry-errors", "true");
			s_indigo.setOption("unique-dearomatization", "false");
			s_indigo.setOption("ignore-noncritical-query-features", "true");
			s_indigo.setOption("timeout", "60000");
        }

        List<AcidBaseRule> m_AcidBaseGroups = new List<AcidBaseRule>();
        Dictionary<int, Tuple<string, string>> m_AcidDict = new Dictionary<int, Tuple<string, string>>();

        public Dictionary<string, string> GetSubstructureTransformMapping()
        {
            return m_AcidDict.ToDictionary(p => p.Value.Item1, p => p.Value.Item2);
        }

        public Dictionary<int, string> AcidBaseSMIRKS(string ct)
        {
            return AcidBaseSMIRKS(new StandardizationResult()
            {
                Standardized = ct,
                Issues = new List<Issue>(),
                Transformations = new List<Transformation>() 
            });
        }

        /// <summary>
        /// takes molecule and returns dictionary of acid-base rule_id and ordered acid-base SMIRKS 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public Dictionary<int, string> AcidBaseSMIRKS(StandardizationResult sr)
        {
            Dictionary<int, string> smirks_list = new Dictionary<int, string>();
            Dictionary<int, string> acids = new Dictionary<int, string>();
            Dictionary<int, int> acidsCount = new Dictionary<int, int>();
            Dictionary<int, string> acidSMARTS = new Dictionary<int, string>();
            Dictionary<int, string> bases = new Dictionary<int, string>();
            Dictionary<int, int> basesCount = new Dictionary<int, int>();
            Dictionary<int, string> baseSMARTS = new Dictionary<int, string>();

			lock (s_indigo)
			{
				IndigoObject m = s_indigo.loadMolecule(sr.Standardized);
				IndigoObject matcher = s_indigo.substructureMatcher(m);
				IndigoObject protonQuery = s_indigo.loadSmarts("[H+]");
				int freeProtons = matcher.countMatches(protonQuery);

				foreach (AcidBaseRule abr in m_AcidBaseGroups)
				{
					IndigoObject chem_acid_query = s_indigo.loadQueryMolecule(abr.AcidSMARTS);
					if (matcher.countMatches(chem_acid_query) == 1)
					{
						acids.Add(abr.Id, abr.Acid2BaseSMIRKS);
						acidSMARTS.Add(abr.Id, abr.AcidSMARTS);
						acidsCount.Add(abr.Id, matcher.countMatches(chem_acid_query));
					}
					IndigoObject chem_base_query = s_indigo.loadQueryMolecule(abr.BaseSMARTS);
					if (matcher.countMatches(chem_base_query) == 1)
					{
						bases.Add(abr.Id, abr.Base2AcidSMIRKS);
						baseSMARTS.Add(abr.Id, abr.BaseSMARTS);
						basesCount.Add(abr.Id, matcher.countMatches(chem_base_query));
					}
				}

				if (acids.Any() && bases.Any() && acids.First().Key < bases.Last().Key)
				{
					if (!smirks_list.ContainsKey(acids.First().Key))
						smirks_list.Add(acids.First().Key, acids.First().Value);//strongest acid
					//TODO (Ken) add logic that if there are few instances of ionized acids and they are not equivalent then do not neutralize them
					//
					//for now just check if only one same base present then ionize
					if ((acidsCount[acids.First().Key] + freeProtons) >= basesCount[bases.Last().Key]
                        && !smirks_list.ContainsKey(bases.Last().Key))
						//if (!smirks_list.ContainsKey(bases.Last().Key))
						smirks_list.Add(bases.Last().Key, bases.Last().Value);//strongest base
				}
			}
            return smirks_list;
        }

        /// <summary>
        /// Returns true if partially ionized acid has an acid ahead of the base on the list.
        /// </summary>
        /// <returns></returns>
        public bool WeakerAcidIonizedBeforeStrongerAcid(Molecule m)
        {
            var results = AcidBaseMatches(m);
            if (!results.Any(r => r.Item2.Contains(ProtonationState.Acid)) || !results.Any(r => r.Item2.Contains(ProtonationState.Base))) return false;
            int firstacid = results.First(r => r.Item2.Contains(ProtonationState.Acid)).Item1;
            int lastbase = results.Last(r => r.Item2.Contains(ProtonationState.Base)).Item1;
            return (lastbase > firstacid);
        }

        public List<Tuple<int, List<ProtonationState>>> AcidBaseMatches(Molecule m)
        {
            var result = new List<Tuple<int, List<ProtonationState>>>();
            foreach (var entry in m_AcidDict)
            {
                var protonationstates = new List<ProtonationState>();
                if (m.Match(entry.Value.Item1,MoleculeObjects.Toolkit.OpenEye)) protonationstates.Add(ProtonationState.Acid);
				if (m.Match(entry.Value.Item2,MoleculeObjects.Toolkit.OpenEye)) protonationstates.Add(ProtonationState.Base);
                if (protonationstates.Any()) result.Add(Tuple.Create(entry.Key, protonationstates));
            }
            return result;
        }

        /// <summary>
        /// This is a constructor for initializing acid and bases from XML file
        /// </summary>
        public Acidity(string acidXmlContent) : this()
        {
            XElement x = XElement.Parse(acidXmlContent, LoadOptions.None);
            var acidgroups = x.XPathSelectElements("//acidgroup");

            int rule_id = 0;
            foreach (var acidgroup in acidgroups)
            {
                int rank = Convert.ToInt32(acidgroup.Attribute("rank").Value);
                string acidSMARTS = acidgroup.Attribute("acid").Value;
                string baseSMARTS = acidgroup.Attribute("base").Value;
                m_AcidDict.Add(rank, Tuple.Create(acidSMARTS, baseSMARTS));
                rule_id++;
                m_AcidBaseGroups.Add(new AcidBaseRule()
                {
                    Id = rule_id,
                    Rank = rank,
                    AcidSMARTS = acidSMARTS,
                    BaseSMARTS = baseSMARTS,
                    Acid2BaseSMIRKS = acidgroup.Attribute("acid2base").Value,
                    Base2AcidSMIRKS = acidgroup.Attribute("base2acid").Value
                });
            }
        }
    }

    public class AcidBaseRule
    {
        public int Id;
        public int Rank;
        public string AcidSMARTS;
        public string BaseSMARTS;
        public string Acid2BaseSMIRKS;
        public string Base2AcidSMIRKS;
    }
}
