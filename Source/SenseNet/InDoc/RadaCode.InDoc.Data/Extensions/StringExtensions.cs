using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadaCode.InDoc.Data.Extensions
{
    public static class StringExtensions
    {
        public static string SmartReplace(this String source, string org, string replace, int start, int max)
        {
            if (start < 0) throw new System.ArgumentOutOfRangeException("Start index is less then 0");
            if (max <= 0) return string.Empty;
            start = source.IndexOf(org, start);
            if (start < 0) return string.Empty;
            var sb = new StringBuilder(source, 0, start, source.Length);
            int found = 0;
            while (max-- > 0)
            {
                int index = source.IndexOf(org, start);
                if (index < 0) break;
                sb.Append(source, start, index - start).Append(replace);
                start = index + org.Length;
                found++;
            }
            sb.Append(source, start, source.Length - start);
            source = sb.ToString();
            return source;
        }
    }
}
