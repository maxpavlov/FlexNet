using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RadaCode.InDoc.Data.DocumentNaming;

namespace InDoc.Tests.Data
{
    [TestFixture]
    public class NamingApproachesManipulationsTest
    {
        [Test]
        public void CreateSetParamsWriteOutParams()
        {
            const string format = "{intInc_G}/{intInc_D}/02/-{yy}";
            const string typeName = "Orders";

            var initParams = new List<KeyValuePair<int, string>>
                                 {new KeyValuePair<int, string>(0, "2296"), new KeyValuePair<int, string>(1, "1")};

            var orderNamingApproach = new NamingApproach {Format = format, TypeName = typeName};
            orderNamingApproach.SaveCurrentParams(initParams);

            Console.WriteLine(orderNamingApproach.CurrentParamsCounters);

            Assert.IsNotEmpty(orderNamingApproach.CurrentParamsCounters);
        }

        [Test]
        public void CreateSetParamsGetNameParams()
        {
            const string format = "{intInc_G}/{intInc_D}/02-{yy}";
            const string typeName = "Orders";

            var initParams = new List<KeyValuePair<int, string>> { new KeyValuePair<int, string>(0, "2296"), new KeyValuePair<int, string>(1, "1") };

            var orderNamingApproach = new NamingApproach { Format = format, TypeName = typeName };
            orderNamingApproach.SaveCurrentParams(initParams);

            var newName = orderNamingApproach.GetNextName();
            Assert.AreEqual("2297/2/02-12", newName);
        }

        [Test]
        public void CreateGetBreakdown()
        {
            const string format = "{intInc_G}/{intInc_D}/02-{yy}";
            const string typeName = "Orders";

            var initParams = new List<KeyValuePair<int, string>> { new KeyValuePair<int, string>(0, "2296"), new KeyValuePair<int, string>(1, "1") };

            var orderNamingApproach = new NamingApproach { Format = format, TypeName = typeName };
            orderNamingApproach.SaveCurrentParams(initParams);

            var breakdown = orderNamingApproach.NameBlocks;
            Assert.AreEqual("{intInc_D}", breakdown[2]);
        }

        [Test]
        public void CreateGetParamBlocks()
        {
            const string format = "{intInc_G}/{intInc_D}/02-{yy}";
            const string typeName = "Orders";

            var initParams = new List<KeyValuePair<int, string>> { new KeyValuePair<int, string>(0, "2296"), new KeyValuePair<int, string>(1, "1") };

            var orderNamingApproach = new NamingApproach { Format = format, TypeName = typeName };
            orderNamingApproach.SaveCurrentParams(initParams);

            var breakdown = orderNamingApproach.ParamBlocks;
            Assert.AreEqual("1", breakdown[2]);
        }
    }
}
