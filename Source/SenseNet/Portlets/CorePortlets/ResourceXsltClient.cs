using System;
using System.Globalization;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.Portlets
{
    public class ResourceXsltClient
    {
        public string GetString(string className, string name)
        {
            var rm = SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current;
            return rm.GetString(className, name);
        }

        public string GetString(string className, string name, string cultureName)
        {
            var rm = SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current;
            CultureInfo cultureInfo = null;
            try
            {
                cultureInfo = CultureInfo.CreateSpecificCulture(cultureName);
            }
            catch (ArgumentException e) //logged
            {
                Logger.WriteException(e);
                return rm.GetString(className, name);
            }
            return rm.GetString(className, name, cultureInfo);
        }

    }
}
