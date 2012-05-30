using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using SenseNet.ContentRepository.Schema;

namespace SenseNet.ContentRepository.Fields
{
    public class BinaryFieldSetting : FieldSetting
    {
        public const string IsTextConfigString = "IsText";
        private bool? _isText;
 
        public bool? IsText
        {
            get
            {
                return _isText ??
                       (ParentFieldSetting == null ? null : ((BinaryFieldSetting) ParentFieldSetting).IsText);
            }
            set
            {
                if (!_mutable)
                    throw new InvalidOperationException("Setting IsText is not allowed within readonly instance.");
                _isText = value;
            }
        }

        protected override void ParseConfiguration(XPathNavigator configurationElement, IXmlNamespaceResolver xmlNamespaceResolver, ContentType contentType)
        {
            foreach (XPathNavigator element in configurationElement.SelectChildren(XPathNodeType.Element))
            {
                switch(element.LocalName)
                {
                    case IsTextConfigString:
                        _isText = element.InnerXml == "true";
                        break;
                }
            }
        }

        protected override void SetDefaults()
        {
            _isText = null;
        }

        public override FieldValidationResult ValidateData(object value, Field field)
        {
            return FieldValidationResult.Successful;
        }

        protected override void CopyPropertiesFrom(FieldSetting source)
        {
            base.CopyPropertiesFrom(source);
            var binaryFieldSettingSource = (BinaryFieldSetting)source;

            IsText = binaryFieldSettingSource.IsText;

        }
        
        protected override void WriteConfiguration(XmlWriter writer)
        {
            WriteElement(writer, this._isText, IsTextConfigString);
        }

        public override IDictionary<string, FieldMetadata> GetFieldMetadata()
        {
            var fmd = base.GetFieldMetadata();

            fmd.Add("IsText", new FieldMetadata
            {
                FieldName = "IsText",
                PropertyType = typeof(bool?),
                FieldType = DynamicContentTools.GetSuggestedFieldType(typeof(bool?)),
                DisplayName = GetTitleString("IsText"),
                Description = GetDescString("IsText"),
                CanRead = true,
                CanWrite = true
            });

            return fmd;
        }

        protected override SenseNet.Search.Indexing.FieldIndexHandler CreateDefaultIndexFieldHandler()
        {
            return new SenseNet.Search.Indexing.BinaryIndexHandler();
        }
    }
}
