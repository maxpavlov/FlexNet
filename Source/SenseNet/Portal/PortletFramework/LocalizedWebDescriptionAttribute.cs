using System;
using System.Globalization;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository.i18n;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.UI.PortletFramework
{
    public class LocalizedWebDescriptionAttribute : WebDescriptionAttribute
    {
        private readonly string _className;
        private readonly CultureInfo _cultureInfo;
        private readonly string _key;
        private bool _isLocalized;

        public LocalizedWebDescriptionAttribute(string displayName) : base(displayName)
        {
        }
        public LocalizedWebDescriptionAttribute(string className, string key)
            : this(className, key, string.Empty)
        {
        }
        public LocalizedWebDescriptionAttribute(string className, string key, string cultureName)
        {
            _className = className;
            _key = key;
            _cultureInfo = SenseNet.ContentRepository.Tools.GetUICultureByNameOrDefault(cultureName);
        }

        public override string Description
        {
            get
            {
                if (!_isLocalized && !string.IsNullOrEmpty(_className) && !string.IsNullOrEmpty(_key))
                {
                    SenseNetResourceManager srm = SenseNetResourceManager.Current;
                    DescriptionValue = srm.GetString(_className, _key, _cultureInfo);
                    _isLocalized = true;
                }
                return base.Description;
            }
        }
    }
}