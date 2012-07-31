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
            const string ns = "RadaCode.InDoc.Data.DocumentNaming.SpecialNamings.Namings";
            var dataAssembly = typeof(RadaCode.InDoc.Data.DocumentNaming.SpecialNamings.SpecialNamingBase).Assembly;

            var classes = SpecialNamingsFactory.GetAllClasses(ns);

            var res = new List<string>();
            foreach (var inst in classes.Select(namingClass => string.Format("{0}.{1}", ns, namingClass)).Select(toCreate => dataAssembly.GetType(toCreate)).Select(type => Activator.CreateInstance(type, approach)).Where(inst => ((SpecialNamingBase)inst).SpecialCode == namingCode))
            {
                return inst as SpecialNamingBase;
            }

            throw new Exception(string.Format("No class found that is able to process {0} code", namingCode));
        }

        public static List<string> ListAllNamingProcessorCodes()
        {
            const string ns = "RadaCode.InDoc.Data.DocumentNaming.SpecialNamings.Namings";
            var dataAssembly = typeof(RadaCode.InDoc.Data.DocumentNaming.SpecialNamings.SpecialNamingBase).Assembly;

            var classes = SpecialNamingsFactory.GetAllClasses(ns);

            var res = new List<string>();
            foreach (string namingClass in classes)
            {
                string toCreate = string.Format("{0}.{1}", ns, namingClass);
                Type type = dataAssembly.GetType(toCreate);
                var inst = Activator.CreateInstance(type, new NamingApproach());
                res.Add(((SpecialNamingBase)inst).SpecialCode);
            }

            return res;
        }

        public static List<string> GetAllClasses(string nameSpace)
        {
            var asm = Assembly.GetExecutingAssembly();

            var namespaceList = (from type in asm.GetTypes() where type.Namespace == nameSpace select type.Name).ToList();

            return namespaceList.ToList();
        }
    }
}
