using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using RadaCode.InDoc.Data.DocumentNaming;
using RadaCode.InDoc.Data.DocumentNaming.SpecialNamings;

namespace InDoc.Tests.Data
{
    [TestFixture]
    public class TypeManipulationTests
    {
        [Test]
        public void CreateFirstNamespaceTypeInstance()
        {
            const string ns = "RadaCode.InDoc.Data.DocumentNaming.SpecialNamings.Namings";
            var dataAssembly = typeof(RadaCode.InDoc.Data.DocumentNaming.SpecialNamings.SpecialNamingBase).Assembly;

            var classes = SpecialNamingsFactory.GetAllClasses(ns);
            var toCreate = string.Format("{0}.{1}", ns, classes[0]);
            var type = dataAssembly.GetType(toCreate);

            var inst = Activator.CreateInstance(type, new NamingApproach()) as SpecialNamingBase;

            Assert.IsNotNull(inst);
        }
    }
}
