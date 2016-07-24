using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using ChemSpider.Utilities;
using ChemSpider.Database;

namespace ChemSpider.Data.Database
{
    public class SuppInfo
    {
        public enum PropertyType { Appearance, Safety, Solubility, MeltingPoint, BoilingPoint, FreezingPoint, FlashPoint };
        public enum Unit { F, C, K, Text }

        public class Property
        {
            public Property(PropertyType type, string value)
            {
                this.type = type;
                this.value = value;
            }

            public PropertyType type;
            public Unit unit = Unit.Text;

            public string value;
            public Substance substance;
        }

        public class Temperature : Property
        {
            public Temperature(PropertyType type, string value) : base(type, value)
            {
                Parse(value);
            }

            public double? min;
            public double? max;

            public double? Avg
            {
                get { return min == null || max == null ? null : (max + min) / 2; }
            }

            public double? Celcius
            {
                get
                {
                    if (Avg == null)
                        return null;

                    if (unit == Unit.C)
                        return Avg;
                    else if (unit == Unit.F)
                        //  C = (5/9)*(F-32)
                        return (Avg - 32) * 5 / 9;
                    else if (unit == Unit.K)
                        //  C = K - 273.15
                        return Avg - 273.15;

                    return null;
                }
            }

            public double? Fahrenheit
            {
                get
                {
                    if (Avg == null)
                        return null;

                    if (unit == Unit.C)
                        //F = C * 9 / 5 + 32
                        return Avg * 9 / 5 + 32;
                    else if (unit == Unit.F)
                        return Avg;
                    else if (unit == Unit.K)
                        //F = (K - 273.15) * 9/5 + 32
                        return (Avg - 273.15) * 9 / 5 + 32;

                    return null;
                }
            }

            public double? Kelvin
            {
                get
                {
                    if (Avg == null)
                        return null;

                    if (unit == Unit.C)
                        //K = C+273.15
                        return Avg + 273.15;
                    else if (unit == Unit.F)
                        //K = (F - 32) * 5/9 + 273.15
                        return (Avg - 32) * 5 / 9 + 273.15;
                    else if (unit == Unit.K)
                        return Avg;

                    return null;
                }
            }

            public double? GetValue(Unit unit)
            {
                return unit == Unit.F ? Fahrenheit : unit == Unit.K ? Kelvin : Celcius;
            }

            private void Parse(string value)
            {
                this.value = value;

                Regex regex = new Regex(@"^(?<less>[<>])?(?<prefix>[a-zA-Z\. ]+)?(?<first>[+-]?[0-9\.eE\+]+)(\s*(-|to)\s*(?<second>[+-]?[0-9\.eE\+]+))?\s*(?<unit>[CFK°]+)?\s*(?<postfix>[a-zA-Z0-9\. ()%#_,]+)?$");
                Match match = regex.Match(value);
                if (match.Success)
                {
                    // less:    match.Groups["less"].Value;
                    // prefix:  match.Groups["prefix"].Value;
                    // first:   match.Groups["first"].Value;
                    // second:  match.Groups["second"].Value;
                    // unit:    match.Groups["second"].Value;
                    // postfix: match.Groups["postfix"].Value;

                    double first;
                    double.TryParse(match.Groups["first"].Value, out first);
                    if (string.IsNullOrEmpty(match.Groups["less"].Value))
                    {
                        double second;
                        double.TryParse(match.Groups["second"].Value, out second);

                        min = first;
                        max = string.IsNullOrEmpty(match.Groups["second"].Value) ? first : second;
                    }
                    else
                    {
                        if (match.Groups["less"].Value.Equals("<"))
                            max = first;
                        else
                            min = first;
                    }

                    unit = StringToUnit(match.Groups["unit"].Value);
                }
            }

            private Unit StringToUnit(string val)
            {
                if (val.Equals("F", StringComparison.CurrentCultureIgnoreCase) ||
                    val.Equals("F°", StringComparison.CurrentCultureIgnoreCase))
                    return Unit.F;
                if (val.Equals("K", StringComparison.CurrentCultureIgnoreCase) ||
                    val.Equals("K°", StringComparison.CurrentCultureIgnoreCase))
                    return Unit.K;

                //  default unit for temperature is C
                return Unit.C;
            }
        }

        public class Substance
        {
            public int sub_id;
            public string url = string.Empty;
        }

        static ChemSpiderDB db = new ChemSpiderDB();

        private XNamespace userDataNS = "chemspider:xmlns:user-data";
        private XDocument suppInfo;

        private int cmp_id = -1;
        private List<Property> properties = new List<Property>();

        public SuppInfo(int cmp_id)
        {
            this.cmp_id = cmp_id;

            Hashtable args = new Hashtable();
            args.Add("cmp_id", cmp_id);

            using (SqlConnection conn = new SqlConnection(ChemSpiderDB.ConnectionString))
            {
                string xml = conn.ExecuteScalar<string>("cmp_get_supp_info_xml @cmp_id", new { cmp_id = cmp_id });
                Load(xml);
            }
        }

        public SuppInfo(string xml)
        {
            Load(xml);
        }

        private void Load(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return;

            suppInfo = XDocument.Parse(xml);

            LoadProperty(PropertyType.Appearance, "appearance");
            LoadProperty(PropertyType.Safety, "safety");
            LoadProperty(PropertyType.Solubility, "experimental-solubility");
            LoadTemperatureProperty(PropertyType.MeltingPoint, "experimental-melting-point");
            LoadTemperatureProperty(PropertyType.BoilingPoint, "experimental-boiling-point");
            LoadTemperatureProperty(PropertyType.FreezingPoint, "experimental-freezing-point");
            LoadTemperatureProperty(PropertyType.FlashPoint, "experimental-flash-point");
        }

        private void LoadProperty(PropertyType type, string xml_node_name)
        {
            var propertyNodes = suppInfo.Descendants(userDataNS + "categories").Descendants(userDataNS + xml_node_name);
            foreach (XElement node in propertyNodes)
            {
                Property prop = new Property(type, node.Attribute("value").Value);

                if (node.Attribute("sub_id") != null)
                {
                    int sub_id = int.Parse(node.Attribute("sub_id").Value);
                    prop.substance = GetSubstance(sub_id);
                }

                properties.Add(prop);
            }
        }

        private void LoadTemperatureProperty(PropertyType type, string xml_node_name)
        {
            var temperatureNodes = suppInfo.Descendants(userDataNS + "categories").Descendants(userDataNS + xml_node_name);
            foreach (XElement node in temperatureNodes)
            {
                Temperature temperature = new Temperature(type, node.Attribute("value").Value);

                if (node.Attribute("sub_id") != null)
                {
                    int sub_id = int.Parse(node.Attribute("sub_id").Value);
                    temperature.substance = GetSubstance(sub_id);
                }

                properties.Add(temperature);
            }
        }

        private Substance GetSubstance(int sub_id)
        {
            XElement substance = (from sub in suppInfo.Descendants("substance")
                                  where (int)sub.Attribute("sub_id") == sub_id
                                  select sub).FirstOrDefault();

            if (substance != null)
                return new Substance() { sub_id = sub_id, url = substance.Attribute("ext_url") != null ? substance.Attribute("ext_url").Value : String.Empty };

            return null;
        }

        public List<Property> GetProperties(PropertyType type)
        {
            return (from prop in properties
                    where prop.type == type
                    select prop).ToList();
        }

        public List<Property> GetUniqueProperties(PropertyType type)
        {
            return (from prop in properties
                    where prop.type == type
                    group prop by (string)prop.value into g
                    select g.First()).ToList();
        }

        public List<Temperature> GetTemperatures(PropertyType type)
        {
            return (from prop in properties
                    where prop.type == type
                    select prop as Temperature).ToList();
        }

        public List<Temperature> GetUniqueTemperatures(PropertyType type)
        {
            return (from prop in properties
                    where prop.type == type
                    group prop by (string)prop.value into g
                    select g.First() as Temperature).ToList();
        }

        public double? GetTemperature(PropertyType type, Unit unit)
        {
            List<Temperature> temperatures = GetTemperatures(type);

            if (temperatures.Count() > 0)
            {
                //  get the list of all temterature's values that are not null...
                IEnumerable<double> values = (  from prop in temperatures
                                                where prop.type == type && prop.GetValue(unit) != null
                                                select (double)prop.GetValue(unit));

                if (values.Count() > 0)
                {
                    //  calculate average...
                    double avg = values.Average();

                    // calculate standard deviation...
                    double dev = Math.Sqrt(values.Sum(d => (d - avg) * (d - avg)) / values.Count());

                    //  get the average from the points that inside the standard deviation...
                    IEnumerable<double> v = values.Where(d => d >= avg - dev && d <= avg + dev);
                    return v.Average();
                }
            }

            return null;
        }
    }
}
