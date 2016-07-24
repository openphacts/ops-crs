using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChemSpider.Molecules;

namespace ChemSpider.Formats
{
    public class EntityLocator
    {
        public string entity;
        public int offset_start;
        public int offset_end;
        public string type;
        public double confidence;
        public readonly string id;
        public string html;
        public bool? validated = null;
        public int validation_confidence = 0;
        public int group_id;

        public static Random _random = new Random();

        public EntityLocator(string aEntity, int aStart, int aEnd, string aType, double aConfidence, string aHtml, int aGroup)
        {
            entity = aEntity;
            offset_start = aStart;
            offset_end = aEnd;
            type = aType;
            confidence = aConfidence;
            html = aHtml;
            lock ( _random ) {
                id = string.Format("ent{0}_{1}", DateTime.Now.Ticks, _random.Next());
            }
            group_id = aGroup;
        }
    }

    interface IEntityValidator
    {
        void Validate(List<EntityLocator> entities);
    }

    class ChemicalEntityValidator : IEntityValidator
    {
        bool m_bUseACDN2S = true;
        public void Validate(List<EntityLocator> entities)
        {
            doValidate(entities);
        }

        class Pair
        {
            public List<EntityLocator> entities = new List<EntityLocator>();
            public List<int> suffix = new List<int>();
        }

        private void doValidate(List<EntityLocator> entities)
        {
            string[] suffix_list = new string[] { "es", "s" };
            Dictionary<string, ChemIdUtils.N2SResult> names = new Dictionary<string, ChemIdUtils.N2SResult>();
            Dictionary<string, Pair> names_entities = new Dictionary<string, Pair>();

            foreach ( EntityLocator e in entities ) {
                if ( !e.type.Equals("chemical-name") && !e.type.Equals("chemical-family") )
                    continue;
                names[e.entity] = null;
                e.validated = false;
                if ( !names_entities.ContainsKey(e.entity) ) {
                    names_entities[e.entity] = new Pair();
                }
                names_entities[e.entity].entities.Add(e);
                names_entities[e.entity].suffix.Add(0);

                foreach ( string suffix in suffix_list ) {
                    if ( e.entity.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase) ) {
                        string s = e.entity.Substring(0, e.entity.Length - suffix.Length);
                        names[s] = null;
                        if ( !names_entities.ContainsKey(s) ) {
                            names_entities[s] = new Pair();
                        }
                        names_entities[s].entities.Add(e);
                        names_entities[s].suffix.Add(suffix.Length);
                    }
                }
            }

            ChemIdUtils.name2strbat(names, m_bUseACDN2S, false);

            foreach ( KeyValuePair<string, ChemIdUtils.N2SResult> result in names ) {
                if ( result.Value == null )
                    continue;

                for ( int i = 0; i < names_entities[result.Key].entities.Count; ++i ) {
                    EntityLocator e = names_entities[result.Key].entities[i];
                    int suffix_len = names_entities[result.Key].suffix[i];
                    e.validation_confidence = result.Value.confidence;
                    if ( result.Value.confidence > 0 ) {
                        e.entity = result.Key;
                        e.validated = true;
                        if ( suffix_len > 0 )
                            e.type = "chemical-family";
                    }
                }
            }
        }
    }
}
