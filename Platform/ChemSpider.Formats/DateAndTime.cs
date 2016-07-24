using System;

namespace ChemSpider.Formats
{
    public class DateAndTime
    {
        //Date Formats
        public enum DateFormat
        {
            Default = 0,
            LongDateTime,
            SortableDate,
            ShortAmericanDate
        }

        public const string LongDateTime = "HH:mm, MMM d, yyyy";
        public const string SortableDate = "yyyyMMdd";
        public const string ShortAmericanDate = "MM/dd/yyyy";

        //Default replacement text.
        public const string DefaultReplacementText = "N/A";

        /// <summary>
        /// Returns the IdentifierType when passed the message string returned from the SimpleSearch.
        /// </summary>
        /// <param name="format">The date formatting enumeration.</param>
        /// <returns>The date formatting string.</returns>
        private static string strDateFormatToFormat(DateFormat format)
        {
            switch (format)
            {
                case DateFormat.LongDateTime:
                    return LongDateTime;
                case DateFormat.SortableDate:
                    return SortableDate;
                case DateFormat.ShortAmericanDate:
                    return ShortAmericanDate;
                default:
                    //Default Date Format
                    return LongDateTime;
            }
        }

        /// <summary>
        /// The default formatting of a DateTime in the default format, using the default replacement text.
        /// </summary>
        /// <param name="dt">The Object to be formatted.</param>
        /// <returns>Either the formatted DateTime using the "" or the replacement value.</returns>
        public static string formatDateTime(Object dateTime)
        {
            return formatDateTime(dateTime, strDateFormatToFormat(DateFormat.Default), DefaultReplacementText);
        }

        /// <summary>
        /// Takes an Object, checks for null or empty string and if it's a valid date, converts to DateTime and formats according to provided format.
        /// </summary>
        /// <param name="dateTime">The DateTime object to be formatted.</param>
        /// <param name="format">The format string to apply.</param>
        /// <param name="replacement">If dateTime is null or empty string then it will use this replacement instead.</param>
        /// <returns></returns>
        public static string formatDateTime(Object dateTime, string format, string replacement)
        {
            string ret;
            DateTime dt;

            if(dateTime is DBNull)
            {
                ret = replacement;
            }
            else
            {
                if (dateTime is string)
                {
                    if ((string)dateTime != string.Empty)
                    {
                        dt = (DateTime)dateTime;
                        if (DateTime.TryParse((string)dateTime, out dt))
                        {
                            ret = dt.ToString(format);
                        }
                        else
                        {
                            ret = replacement;
                        }
                    }
                    else
                    {
                        ret = replacement;
                    }
                }
                else if (dateTime is DateTime)
                {
                    ret = ((DateTime)dateTime).ToString(format);
                }
                else
                {
                    ret = replacement;
                }
            }
            return ret;
        }
    }
}
