using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;
using System.Xml.XPath;
using System.Xml.Linq;

namespace SenseNet.Portal.UI
{
    public class XmlFormatTools
    {
        #region datetimeFunctions
        public string FormatDate(string xmlDate)
        {
            var d = XmlConvert.ToDateTime(xmlDate, XmlDateTimeSerializationMode.Unspecified);
            var s = d.ToString(CultureInfo.CurrentCulture);
            return s;
        }
        public string FormatDate(string xmlDate, string format)
        {
            var d = XmlConvert.ToDateTime(xmlDate, XmlDateTimeSerializationMode.Unspecified);
            var s = d.ToString(format);
            return s;
        }
        public string ShortDate(string xmlDate)
        {
            return FormatDate(xmlDate, CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
        }
        public string ShortTime(string xmlDate)
        {
            return FormatDate(xmlDate, CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);
        }
        public string LongDate(string xmlDate)
        {
            return FormatDate(xmlDate, CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern);
        }
        public string LongTime(string xmlDate)
        {
            return FormatDate(xmlDate, CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern);
        }
        #endregion

        #region numberFunctions
        public decimal Abs(decimal number)
        {
            return Math.Abs(number);
        }
        #endregion
    }
}
