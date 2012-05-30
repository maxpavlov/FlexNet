using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.Portal
{
    public class CaseInsensitiveEqualityComparer : IEqualityComparer<string>
    {
        #region IEqualityComparer<string> Members

        public bool Equals(string x, string y)
        {
            return x.Equals(y, StringComparison.CurrentCultureIgnoreCase);
        }

        public int GetHashCode(string obj)
        {
            var lower = obj.ToLower();
            return lower.GetHashCode();
        }

        #endregion
    }
}
