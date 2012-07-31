using System;
using RadaCode.InDoc.Data.Extensions;

namespace RadaCode.InDoc.Data.DocumentNaming.SpecialNamings.Namings
{
    public class YearNaming : SpecialNamingBase
    {
        protected const string _namingCode = "{yy}";

        public override string SpecialCode
        {
            get { return _namingCode; }
        }

        public YearNaming(ref NamingApproach approach)
            : base(ref approach){}

        public override void ProcessGetNextForNaming(ref string res, int braceStart, int braceEnd, int codeIndex)
        {
            res = res.SmartReplace(_namingCode, DateTime.UtcNow.ToString("yy"), braceStart, 1);
        }
    }
}