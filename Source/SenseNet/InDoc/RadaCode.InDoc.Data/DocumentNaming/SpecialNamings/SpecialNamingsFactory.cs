using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RadaCode.InDoc.Data.DocumentNaming.SpecialNamings.Namings;

namespace RadaCode.InDoc.Data.DocumentNaming.SpecialNamings
{
    public static class SpecialNamingsFactory
    {
        public static SpecialNamingBase GetNamingProcessor(NamingApproach approach, string namingCode)
        {
            var asm = Assembly.GetExecutingAssembly();

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

        public static List<string> GetAllClasses(string nameSpace)
        {
            var asm = Assembly.GetExecutingAssembly();

            var namespaceList = (from type in asm.GetTypes() where type.Namespace == nameSpace select type.Name).ToList();

            return namespaceList.ToList();
        }
    }
}
