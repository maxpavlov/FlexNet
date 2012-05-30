using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using SenseNet.ContentRepository.Schema;
using System.Collections.Specialized;

namespace SenseNet.ContentRepository.Fields
{
    public class CurrencyFieldSetting : NumberFieldSetting
    {
        public const string FormatName = "Format";

        private string _format;
        public string Format
        {
            get
            {
                return _format ?? (this.ParentFieldSetting == null ? null :
                    ((CurrencyFieldSetting)this.ParentFieldSetting).Format);
            }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting Format is not allowed within readonly instance.");
                _format = value;
            }
        }

        public static NameValueCollection CurrencyTypes
        {
            get
            {
                return System.Configuration.ConfigurationManager.GetSection("currencyValues") as NameValueCollection;
            }
        }

        protected override void ParseConfiguration(XPathNavigator configurationElement, IXmlNamespaceResolver xmlNamespaceResolver, ContentType contentType)
        {
            base.ParseConfiguration(configurationElement, xmlNamespaceResolver, contentType);

            foreach (XPathNavigator node in configurationElement.SelectChildren(XPathNodeType.Element))
            {
                switch (node.LocalName)
                {
                    case FormatName:
                        if (!string.IsNullOrEmpty(node.InnerXml))
                            _format = node.InnerXml;
                        break;
                }
            }
        }

        protected override void WriteConfiguration(XmlWriter writer)
        {
            base.WriteConfiguration(writer);

            WriteElement(writer, this._format, FormatName);
        }

        protected override void CopyPropertiesFrom(FieldSetting source)
        {
            base.CopyPropertiesFrom(source);

            var currentSource = (CurrencyFieldSetting)source;

            Format = currentSource.Format;
        }

        public override IDictionary<string, FieldMetadata> GetFieldMetadata()
        {
            var fmd = base.GetFieldMetadata();

            fmd[ShowAsPercentageName].FieldSetting.Visible = false;

            var fs = new ChoiceFieldSetting
                         {
                             Name = FormatName,
                             DisplayName = GetTitleString(FormatName),
                             Description = GetDescString(FormatName),
                             FieldClassName = typeof(ChoiceField).FullName,
                             AllowMultiple = false,
                             AllowExtraValue = false,
                             Options = (from c in CurrencyTypes.AllKeys
                                        select new ChoiceOption(c, GetCurrencyText(c, CurrencyTypes[c]))).ToList(),
                             DisplayChoice = DisplayChoice.DropDown
                         };

            fmd.Add(FormatName, new FieldMetadata
            {
                FieldName = FormatName,
                CanRead = true,
                CanWrite = true,
                FieldSetting = fs
            });

            return fmd;
        }

        public override object GetProperty(string name, out bool found)
        {
            var val = base.GetProperty(name, out found);

            if (!found)
            {
                switch (name)
                {
                    case FormatName:
                        found = true;
                        if (!string.IsNullOrEmpty(_format))
                            val = _format;
                        break;
                }
            }

            return found ? val : null;
        }

        public override bool SetProperty(string name, object value)
        {
            var found = base.SetProperty(name, value);

            if (!found)
            {
                switch (name)
                {
                    case FormatName:
                        found = true;
                        _format = value as string;
                        break;
                }
            }

            return found;
        }

        private static string GetCurrencyText(string key, string value)
        {
            return string.Format("{0} ({1})", value, key);
        }
    }
}
