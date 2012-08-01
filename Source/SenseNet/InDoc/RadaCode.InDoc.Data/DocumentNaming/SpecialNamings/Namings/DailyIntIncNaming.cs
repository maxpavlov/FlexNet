using System;
using RadaCode.InDoc.Data.Extensions;

namespace RadaCode.InDoc.Data.DocumentNaming.SpecialNamings.Namings
{
    public class DailyIntIncNaming : SpecialNamingBase
    {
        protected const string _namingCode = "{intInc_D}";

        public DailyIntIncNaming(ref NamingApproach approach) : base(ref approach)
        {}

        public override string SpecialCode
        {
            get { return _namingCode; }
        }

        public override bool HasValue
        {
            get { return true; }
        }

        public override void ProcessGetNextForNaming(ref string res, int braceStart, int braceEnd, int codeIndex)
        {
            var currentD = GetCurrentValueByIndex(codeIndex);
            var curIntD = int.Parse(currentD);
            if (DateTime.UtcNow.Date == Approach.UpdateTime.Date)
            {
                curIntD++;
            }
            else curIntD = 1;
            res = res.SmartReplace(_namingCode, curIntD.ToString(), braceStart, 1);
            SaveNewValueByIndex(codeIndex, curIntD.ToString());
        }
    }
}