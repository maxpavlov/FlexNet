using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.Controls;

namespace SenseNet.Portal.Helpers
{
    public class UserProfiles
    {
        public static bool IsEducationEmpty(string education)
        {
            if (string.IsNullOrEmpty(education))
                return true;

            try
            {
                var ser = new XmlSerializer(typeof(List<SchoolItem>));
                using (var reader = new XmlTextReader(education, XmlNodeType.Element, new XmlParserContext(null, null, "", XmlSpace.Default)))
                {
                    var dataItems = ser.Deserialize(reader) as List<SchoolItem>;
                    if (dataItems != null)
                        return dataItems.Count == 0;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }

            return true;
        }
    }
}
