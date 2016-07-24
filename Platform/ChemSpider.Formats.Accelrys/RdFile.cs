using System;
using System.Collections.Generic;
using System.Linq;

namespace MoleculeObjects
{
    public class RdFile
    {
        private List<Tuple<Rxn, Dictionary<string, string>>> m_rxns =
                new List<Tuple<Rxn, Dictionary<string, string>>>();
        private string m_datm;

        public override string ToString()
        {
            return String.Format("$RDFILE 1{0}{1}{0}{2}", Environment.NewLine, m_datm,
                                String.Concat(from r in m_rxns
                                              select String.Format("$RFMT{0}{1}{2}",
                                    Environment.NewLine, r.Item1.ToString(),
                                    String.Concat(from pair in r.Item2
                                                  select String.Format("$DTYPE {0}{1}$DATUM {2}{1}", pair.Key, Environment.NewLine, pair.Value)))));
        }

        public void Add(Rxn rxn, Dictionary<string, string> props)
        {
            m_rxns.Add(new Tuple<Rxn, Dictionary<string, string>>(rxn, props));
        }

        private void InitializeDate()
        {
            DateTime dt = DateTime.Now;
            m_datm = String.Format("$DATM  {0}/{1}/{2} {3}:{4}",
                dt.Month.ToString().PadLeft(2, '0'),
                dt.Day.ToString().PadLeft(2, '0'),
                dt.Year.ToString().PadLeft(2, '0'),
                dt.Hour.ToString().PadLeft(2, '0'),
                dt.Minute.ToString().PadLeft(2, '0'));
        }

        public List<Tuple<Rxn, Dictionary<string, string>>> Reactions()
        {
            return m_rxns;
        }

        public RdFile(List<Tuple<Rxn, Dictionary<string,string>>> rxns)
        {
            InitializeDate();
            m_rxns = rxns;
        }

        /// <summary>
        /// Builds an RdFile object from the text contents of an RdFile.
        /// Assumes currently that everything inside is an embedded Rxn.
        /// </summary>
        /// <param name="s"></param>
        public RdFile(string s)
        {
            List<string> rchunks = s.Split(new string[] { "$RFMT" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            m_datm = rchunks[0].Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[1];
            foreach (string rchunk in rchunks.Where(r => r != rchunks.First()))
            {
                if (!rchunk.Contains("$DTYPE"))
                {
                    m_rxns.Add(new Tuple<Rxn, Dictionary<string, string>>(RxnFactory.Rxn(rchunk), new Dictionary<string, string>()));
                }
                else
                {
                    string rxn = rchunk.Substring(0, rchunk.IndexOf("$DTYPE")).TrimStart();
                    string rest = rchunk.Substring(rchunk.IndexOf("$DTYPE"));

                    List<string[]> proplist = (from p in rest.Split(new string[] { "$DTYPE" }, StringSplitOptions.RemoveEmptyEntries) select p.Split(new string[] {"$DATUM"}, StringSplitOptions.None)).ToList();
                    Dictionary<string, string> props = (from l in proplist
                                                        select new KeyValuePair<string, string>(l[0].Trim(), l[1].Trim())).ToDictionary(p => p.Key, p => p.Value);
                    m_rxns.Add(new Tuple<Rxn, Dictionary<string, string>>(RxnFactory.Rxn(rxn), props));
                }
            }
        }
    }
}