using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Portal.UI.PortletFramework;
using System.Xml.XPath;

namespace SenseNet.Diagnostics
{
    [XsltCreatable]
    public class XsltUtilities
    {
        public void TraceWrite(object thing)
        {
            string result = string.Empty;
            if (thing is string)
            {
                result = (string)thing;
            }
            else if (thing is XPathNodeIterator)
            {
                var iter = (XPathNodeIterator)thing;
                iter.MoveNext();
                result = iter.Current.OuterXml;
            }
            Logger.Write(result);
        }
    }
}
