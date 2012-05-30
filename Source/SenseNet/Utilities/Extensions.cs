using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Web;
using System.Diagnostics;

namespace SenseNet.Utilities
{
    public static class Extensions
    {
        public static string FillTemplate(this string format, params object[] values)
        {
            return string.Format(CultureInfo.InvariantCulture,format, values);
        }

        public static void PrintTo(this string value, HttpContext httpContext)
        {
            httpContext.Response.Write(value);
        }

        public static void PrintToContext(this string value)
        {
            HttpContext.Current.Response.Write(value);
        }


        public static void RestartTimer(this Stopwatch sw)
        {
            sw.Reset(); sw.Start();
        }
    }
}
