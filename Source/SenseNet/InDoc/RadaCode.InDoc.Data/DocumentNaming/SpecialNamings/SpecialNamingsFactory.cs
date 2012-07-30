using System;

namespace RadaCode.InDoc.Data.DocumentNaming.SpecialNamings
{
    public static class SpecialNamingsFactory
    {
        public static SpecialNaming GetNamingProcessor(NamingApproach approach, string namingCode)
        {
            switch (namingCode)
            {
                case "{intInc_G}":
                    return new GlobalIntIncNaming(ref approach); 
                case "{intInc_D}":
                    return new DailyIntIncNaming(ref approach);
                case "{yy}":
                    return new YearNaming(ref approach);
                default:
                    throw new Exception(String.Format("Unknown naming format encountered: {0}", namingCode));  
            }
            

        }
    }
}
