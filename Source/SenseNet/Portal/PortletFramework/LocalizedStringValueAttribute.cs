using System;
using System.Globalization;
using SenseNet.ContentRepository.i18n;

namespace SenseNet.Portal.UI.PortletFramework
{
    /// <summary>
    /// This attribute is used to represent a string value
    /// for a value in an enum.
    /// </summary>
    public class LocalizedStringValueAttribute : Attribute
    {
        private readonly string _className;
        private readonly CultureInfo _cultureInfo;
        private readonly string _key;
        private bool _isLocalized;

        #region Constructor

        /// <summary>
        /// Constructor used to init a StringValue Attribute
        /// </summary>
        /// <param name="value"></param>
        public LocalizedStringValueAttribute(string value)
        {
            this.StringValueInternal = value;
        }

        public LocalizedStringValueAttribute(string className, string key)
            : this(className, key, string.Empty)
        {
        }

        public LocalizedStringValueAttribute(string className, string key, string cultureName)
        {
            _className = className;
            _key = key;
            _cultureInfo = SenseNet.ContentRepository.Tools.GetUICultureByNameOrDefault(cultureName);
        }

        #endregion

        /// <summary>
        /// Holds the stringvalue for a value in an enum.
        /// </summary>
        protected string StringValueInternal { get; set; }

        public virtual string StringValue
        {
            get
            {
                if (!_isLocalized && !string.IsNullOrEmpty(_className) && !string.IsNullOrEmpty(_key))
                {
                    SenseNetResourceManager srm = SenseNetResourceManager.Current;
                    StringValueInternal = srm.GetString(_className, _key, _cultureInfo);
                    _isLocalized = true;
                }
                return StringValueInternal;
            }
        }
    }
}