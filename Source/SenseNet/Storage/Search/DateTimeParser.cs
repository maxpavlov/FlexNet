using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;

namespace SenseNet.ContentRepository.Storage.Search
{
    public static class DateTimeParser
    {
        public static string GetDateTimeModifiedQuery(string query)
        {
            string[] tagNames = new string[]{"now","thisyear","thismonth","thisweek","thisday","thishour","thisminute","yesterday","tomorrow"};
            var containsTag = false;
            foreach(string tagName in tagNames)
            {
                if(query.Contains(tagName))
                {
                    containsTag = true;
                    break;
                }
            }
            if (containsTag)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(query);
                foreach (string tagName in tagNames)
                {
                    CreateDateValue(doc, tagName);
                }

                return doc.InnerXml;
            }
            else
            {
                return (query);
            }
        }

        private static void CreateDateValue(XmlDocument doc, string tagName)
        {
            DateTime baseTime = GetBaseDateTime(tagName);

            XmlNodeList list = doc.GetElementsByTagName(tagName);
            while (list.Count > 0)
            {
                DateTime dateTime = baseTime;

                /*if (list[0].Attributes["property"] != null)
                {
                    string replacedValue = GetPropertyValue(list[0].Attributes["property"].Value);
                    list[0].ParentNode.InnerXml = list[0].ParentNode.InnerXml.Replace(list[0].OuterXml, replacedValue);
                }
                list = doc.GetElementsByTagName("currentsite");*/

                foreach (XmlAttribute attr in list[0].Attributes)
                {
                    int value = GetValue(attr.Value);
                    dateTime = GetModifiedDateTime(dateTime, attr, value);
                }

                string dateformat = "yyyy-MM-ddTHH:mm:ss";
                if (list[0].Attributes["dateFormat"] != null)
                {
                    dateformat = list[0].Attributes["dateFormat"].Value.ToString();
                }
                if (list[0].Attributes["dateformat"] != null)
                {
                    dateformat = list[0].Attributes["dateformat"].Value.ToString();
                }

                string replacedValue = dateTime.ToString(dateformat);
                list[0].ParentNode.InnerXml = list[0].ParentNode.InnerXml.Replace(list[0].OuterXml, replacedValue);
                list = doc.GetElementsByTagName(tagName);
            }
        }

        private static DateTime GetModifiedDateTime(DateTime dateTime, XmlAttribute attr, int value)
        {
            switch (attr.Name.ToLower(CultureInfo.InvariantCulture))
            {
                case "year":
                case "addyear":
                    dateTime = dateTime.AddYears(value);
                    break;
                case "month":
                case "addmonth":
                    dateTime = dateTime.AddMonths(value);
                    break;
                case "week":
                case "addweek":
                    dateTime = dateTime.AddDays(value * 7);
                    break;
                case "day":
                case "adday":
                    dateTime = dateTime.AddDays(value);
                    break;
                case "hour":
                case "addhour":
                    dateTime = dateTime.AddHours(value);
                    break;
                case "min":
                case "addmin":
                    dateTime = dateTime.AddMinutes(value);
                    break;
                case "sec":
                case "addsec":
                    dateTime = dateTime.AddSeconds(value);
                    break;
            }
            return dateTime;
        }

        private static DateTime GetBaseDateTime(string tagName)
        {
            switch (tagName.ToLower(CultureInfo.InvariantCulture))
            {
                case "now": return DateTime.Now;
                case "thisyear": return new DateTime(DateTime.Now.Year, 1, 1);
                case "thismonth": return new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                case "thisweek": return DateTime.Now.DayOfWeek != 0 ? DateTime.Now.AddDays(-1 * ((int)DateTime.Now.DayOfWeek - 1)).Date : DateTime.Now.AddDays(-6).Date;
                case "thisday": return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                case "thishour": return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
                case "thisminute": return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
                case "yesterday": return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(-1);
                case "tomorrow": return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(1);
                default: return DateTime.Now;
            }
        }

        private static int GetValue(string value)
        {
            int result;
            if(int.TryParse(value, out result))
                return result;
            return 0;
        }
    }
}
