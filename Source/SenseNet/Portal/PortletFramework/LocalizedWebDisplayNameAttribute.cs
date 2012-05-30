using System;
using System.Globalization;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository.i18n;
using SenseNet.Diagnostics;
using System.Linq;

namespace SenseNet.Portal.UI.PortletFramework
{
    public sealed class LocalizedWebDisplayNameAttribute : WebDisplayNameAttribute
    {
        private readonly string _className;
        private readonly CultureInfo _cultureInfo;
        private readonly string _key;
        private bool _isLocalized;

        public LocalizedWebDisplayNameAttribute(string displayName) : base(displayName)
        {
        }

        public LocalizedWebDisplayNameAttribute(string className, string key):this(className, key, string.Empty)
        {
        }
        public LocalizedWebDisplayNameAttribute(string className, string key, string cultureName)
        {
            _className = className;
            _key = key;
            _cultureInfo = SenseNet.ContentRepository.Tools.GetUICultureByNameOrDefault(cultureName);
        }

        public override string DisplayName
        {
            get
            {
                if (!_isLocalized && !string.IsNullOrEmpty(_className) && !string.IsNullOrEmpty(_key))
                {
                    SenseNetResourceManager srm = SenseNetResourceManager.Current;
                    DisplayNameValue = srm.GetString(_className, _key, _cultureInfo);
                    _isLocalized = true;
                }
                return base.DisplayName;
            }
        }
    }
}