using RadaCode.InDoc.Data.Extensions;

namespace RadaCode.InDoc.Data.DocumentNaming.SpecialNamings.Namings
{
    public class GlobalIntIncNaming : SpecialNamingBase
    {
        protected const string _namingCode = "{intInc_G}";

        public GlobalIntIncNaming(ref NamingApproach approach) : base(ref approach)
        {}

        public override void ProcessGetNextForNaming(ref string res, int braceStart, int braceEnd, int codeIndex)
        {
            var curValue = GetCurrentValueByIndex(codeIndex);
            var curIntG = int.Parse(curValue);
            curIntG++;
            res = res.SmartReplace(_namingCode, curIntG.ToString(), braceStart, 1);
            SaveNewValueByIndex(codeIndex, curIntG.ToString());
        }
    }
}