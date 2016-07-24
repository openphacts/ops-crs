using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using ChemSpider.Database;
using ChemSpider.Data.Database;
using ChemSpider.Molecules;

namespace ChemSpider.Search
{
    public class CSSuppInfoSearch : CSSqlSearch
    {
        protected ChemSpiderDB m_csdb = new ChemSpiderDB();

        public new SuppInfoSearchOptions Options
        {
            get { return base.Options as SuppInfoSearchOptions; }
        }

        protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
        {
            bool bTextPropertiesAdded = false;
            bool bNumericPropertiesAdded = false;
            bool bAnnotationsAdded = false;
            int count = 0;

            Options.NumericSearchOptions.ForEach(num_opt => {

                decimal low_value = 0;
                decimal high_value = 0;
                decimal secondary_value = 0;
                count++;

                // Plus/minus and delta value.
                if(!string.IsNullOrEmpty(num_opt.PropertyValue) && !string.IsNullOrEmpty(num_opt.PropertyValueDelta))
                {
                    decimal value;
                    decimal delta;
                    if(decimal.TryParse(num_opt.PropertyValue, out value))
                    {
                        if(decimal.TryParse(num_opt.PropertyValueDelta, out delta))
                        {
                            low_value = value - delta;
                            high_value = value + delta;
                            bNumericPropertiesAdded = true;
                        }
                    }
                }
                else if(!string.IsNullOrEmpty(num_opt.PropertyValueMin) && !string.IsNullOrEmpty(num_opt.PropertyValueMax))
                {
                    if (decimal.TryParse(num_opt.PropertyValueMin, out low_value))
                    {
                        if (decimal.TryParse(num_opt.PropertyValueMax, out high_value))
                        {
                            bNumericPropertiesAdded = true;
                        }
                    }
                }

                //Secondary Unit
                decimal.TryParse(num_opt.PropertySecondaryValue, out secondary_value);

                if (bNumericPropertiesAdded)
                {
                    string high_visual = string.Empty;
                    string low_visual = string.Empty;

                    //Add visual for the unconverted value.
                    low_visual = string.Format("{0} >= {1} {2}", num_opt.PropertyLabel, low_value, num_opt.PropertyPrimaryUnit);

                    //Add visual for the unconverted value.
                    high_visual = string.Format("{0} <= {1} {2}", num_opt.PropertyLabel, high_value, num_opt.PropertyPrimaryUnit);

                    //Convert the unitId to the value of the unit.
                    string PropertyDefaultPrimaryUnit = unitIdToValue(num_opt.PropertyDefaultPrimaryUnitId);

                    //Convert the values to the base unit id if required.
                    if (num_opt.PropertyPrimaryUnitId != num_opt.PropertyDefaultPrimaryUnitId)
                    {
                        low_value = convertValue(low_value, num_opt.PropertyPrimaryUnitId);
                        high_value = convertValue(high_value, num_opt.PropertyPrimaryUnitId);

                        //Add visuals for the converted value (max 4DPs).
                        low_visual += string.Format(" ({0} {1})", low_value.ToString("0.####"), PropertyDefaultPrimaryUnit);
                        high_visual += string.Format(" ({0} {1})", high_value.ToString("0.####"), PropertyDefaultPrimaryUnit);
                    }
                    
                    //Convert the secondary value to the base unit id if required.
                    if (num_opt.PropertySecondaryUnitId != num_opt.PropertyDefaultSecondaryUnitId)
                    {
                        //Convert the secondary_unit to the base unit.
                        secondary_value = convertValue(secondary_value, num_opt.PropertySecondaryUnitId);
                    }

                    //If this is Boiling Point then we must convert the high and low values to standard pressure.
                    if (num_opt.PropertyTypeId == 5) //5 == Boiling Point.
                    {
                        int mmHg_to_atm = 760;
                        decimal c_to_k = 273.15M;
                        
                        //Convert from C to K.
                        double low_value_k = Convert.ToDouble(low_value + c_to_k);
                        double high_value_k = Convert.ToDouble(high_value + c_to_k);

                        //Convert from mmHg to atm.
                        double secondary_value_atm = Convert.ToDouble(secondary_value / mmHg_to_atm);
                        
                        //Convert low and high values to Atmospheric Pressure - C.
                        if (secondary_value_atm > 0 && secondary_value_atm != 1)
                        {
                            if (low_value_k > 0)
                            {
                                //Add secondary value details to the visual.
                                low_visual += string.Format(" @ {0} {1}", num_opt.PropertySecondaryValue, num_opt.PropertySecondaryUnit);

                                //Convert low value to Atmospheric Pressure - C.
                                low_value = Convert.ToDecimal(Nomograph.BoilingPointAtAtmosphericPressure(secondary_value_atm, low_value_k)) - c_to_k;

                                //Add visual for the converted value (max 4DPs).
                                low_visual += string.Format(" ({0} {1} @ 1 atm)", low_value.ToString("0.####"), PropertyDefaultPrimaryUnit);
                            }

                            if (high_value_k > 0)
                            {
                                //Add secondary value details to the visual.
                                high_visual += string.Format(" @ {0} {1}", num_opt.PropertySecondaryValue, num_opt.PropertySecondaryUnit);

                                //Convert high value to Atmospheric Pressure - C.
                                high_value = Convert.ToDecimal(Nomograph.BoilingPointAtAtmosphericPressure(secondary_value_atm, high_value_k)) - c_to_k;

                                //Add visual for the converted value (max 4DPs).
                                high_visual += string.Format(" ({0} {1} @ 1 atm)", high_value.ToString("0.####"), PropertyDefaultPrimaryUnit);
                            }
                        }
                        else
                        {
                            //Add secondary value details to the visual.
                            high_visual += string.Format(" @ {0} {1}", num_opt.PropertySecondaryValue, num_opt.PropertySecondaryUnit);
                        }
                    }

                    //Add the low and high visual descriptions.
                    visual.Add(low_visual);
                    visual.Add(high_visual);

                    tables.Add(String.Format(@"JOIN (SELECT DISTINCT(subs.cmp_id) 
                                                FROM si_property prop_num 
                                                    LEFT JOIN substances subs
                                                        ON prop_num.sub_id = subs.sub_id
                                                            WHERE prop_num.si_property_sub_type_id = {0}
                                                                AND (prop_num.primary_unit_id = {1} OR prop_num.primary_unit_id IS NULL)
                                                                AND ( 
                                                                        (prop_num.low_value >= {2} AND prop_num.low_value <= {3})
                                                                    OR
                                                                        (prop_num.high_value <= {3} AND prop_num.high_value >= {2})
                                                                    ){5}) cmp_props{4}
                                                                        ON cmp_props{4}.cmp_id = c.cmp_id"
                                                                    , num_opt.PropertyId
                                                                    , num_opt.PropertyDefaultPrimaryUnitId
                                                                    , low_value
                                                                    , high_value
                                                                    , count
                                                                    , (num_opt.PropertyTypeId == 5 || num_opt.PropertyTypeId == 1) ? String.Format(" AND (prop_num.secondary_value IS NULL OR prop_num.secondary_value = {0})", num_opt.PropertyDefaultSecondaryValue) : string.Empty));
                }
            });

            //Reset the count.
            count = 0;
            Options.TextSearchOptions.ForEach(text_opt =>
            {
                count++;
                tables.Add(String.Format(@"JOIN (SELECT DISTINCT(subs.cmp_id) 
                            				FROM si_property_text prop_text
					                            LEFT JOIN substances subs
						                            ON prop_text.sub_id = subs.sub_id
									                    WHERE {0} subs.deleted_yn = 0
										                    AND {1}
											                    ) cmp_text_props{2}
												                    ON cmp_text_props{2}.cmp_id = c.cmp_id"
                                                                    , text_opt.PropertyId > -1 ? String.Format("prop_text.si_property_sub_type_id = {0} AND ", text_opt.PropertyId) : string.Empty
                                                                    //, "contains(prop_text.value, '" + "\"" + DbUtil.prepare4sqlRemoveMarkup(text_opt.PropertyValue) + "\"')"
                                                                    , "contains(prop_text.value, '" + DbUtil.prepare4FullTextSearch(text_opt.PropertyValue) + "')"
                                                                    , count));

                visual.Add(String.Format("{0} = '{1}'",text_opt.PropertyLabel, text_opt.PropertyValue));
                bTextPropertiesAdded = true;
            });

            count = 0;
            string annotations = string.Empty;
            string annotations_visual = string.Empty;
            Options.AnnotationSearchOptions.ForEach(annotation_opt =>
            {
                //Annotations can be OR'ed or AND'ed.
                string seperator = count > 0 ? Options.AnnotationSearchOr ? " OR " : " AND " : string.Empty;
                annotations += String.Format("{0}pa.si_annotation_id = {1}", seperator, annotation_opt.AnnotationId);
                annotations_visual += String.Format("{0}Annotation = '{1}'", seperator, annotation_opt.AnnotationValue);
                bAnnotationsAdded = true;
                count ++;
            });

            if (Options.AnnotationSearchOptions.Count > 0)
            {
                tables.Add(String.Format(@"JOIN (SELECT DISTINCT(subs.cmp_id)
			                                FROM si_property p
				                                LEFT JOIN substances subs
					                                ON p.sub_id = subs.sub_id
				                                JOIN si_property_annotation pa 
					                                ON pa.si_property_id = p.si_property_id
			                                WHERE {0}) cmp_annotations
				                                ON cmp_annotations.cmp_id = c.cmp_id"
                                            , annotations));
                visual.Add(String.Format("({0})", annotations_visual));
            }


            return bTextPropertiesAdded || bNumericPropertiesAdded || bAnnotationsAdded;
        }

        //Convert the value to the Base Unit when there is a non-default Primary Unit.
        private decimal convertValue(decimal value, int unit_id)
        {
            using (SqlDataReader reader = m_csdb.DBU.m_executeReader(String.Format("EXEC ConvertUsingUnitId {0}, {1}", value, unit_id)))
            {
                while (reader.Read())
                    return Convert.ToDecimal(reader[0].ToString());
            }
            //If something went wrong then return zero.
            return 0;
        }

        //Maps the unitId to the value of the unit for display purposes.
        private string unitIdToValue(int unit_id)
        {
            using (SqlDataReader reader = m_csdb.DBU.m_executeReader(String.Format("SELECT value FROM si_unit WHERE si_unit_id = {0}", unit_id)))
            {
                while (reader.Read())
                    return reader[0].ToString();
            }
            //If something went wrong then return empty string.
            return string.Empty;
        }
    }
}
