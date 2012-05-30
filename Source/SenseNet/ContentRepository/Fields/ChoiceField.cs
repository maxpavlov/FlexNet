using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Xml.XPath;

using SenseNet.ContentRepository.Schema;
using System.Collections;
using System.Linq;
using System.Globalization;

namespace SenseNet.ContentRepository.Fields
{
    [ShortName("Choice")]
    [DataSlot(0, RepositoryDataType.String, typeof(string), typeof(int), typeof(Enum))]
    [DefaultFieldSetting(typeof(ChoiceFieldSetting))]
    [DefaultFieldControl("SenseNet.Portal.UI.Controls.DropDown")]
    public class ChoiceField : Field
    {
        private static readonly char[] SplitChars = new char[] { ',', ';' };

        protected override bool HasExportData { get { return true; } }
        protected override void ExportData(System.Xml.XmlWriter writer, ExportContext context)
        {
            object data = GetData();

            var stringData = data as string;
            if (stringData != null)
            {
                writer.WriteString(stringData);
                return;
            }

            var listData = data as List<string>;
            if (listData != null)
            {
                string output = String.Join(";", listData.ToArray());
                writer.WriteString(output);
                return;
            }

            var enumerableData = data as IEnumerable;
            if (enumerableData != null)
            {
                var sb = new StringBuilder();
                foreach (var item in enumerableData)
                {
                    if (sb.Length != 0)
                        sb.Append(";");
                    sb.Append(Convert.ToString(item, CultureInfo.InvariantCulture));
                }
                writer.WriteString(sb.ToString());
                return;
            }

            throw ExportNotImplementedException(data);
        }
        protected override void WriteXmlData(System.Xml.XmlWriter writer)
        {
            ExportData(writer, null);
        }
        protected override void ImportData(System.Xml.XmlNode fieldNode, ImportContext context)
        {
            string value = fieldNode.InnerXml;
            if (value.Trim() == String.Empty)
                return;
            var values = new List<string>(value.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries));
            // dobsonl: this causes problem when importing choicefield values if option value starts with " " 
            // -> corresponding option value cannot be found if field value is trimmed
            //for (int i = 0; i < values.Count; i++)
            //    values[i] = values[i].Trim();
            SetData(values);
        }

        protected override string GetXmlData()
        {
            object data = GetData();

            var stringData = data as string;
            if (stringData != null)
                return stringData;

            var listData = data as List<string>;
            if (listData != null)
                return String.Join(";", listData.ToArray());

            var enumerableData = data as IEnumerable;
            if (enumerableData != null)
            {
                var sb = new StringBuilder();
                foreach (var item in enumerableData)
                {
                    if (sb.Length != 0)
                        sb.Append(";");
                    sb.Append(Convert.ToString(item, CultureInfo.InvariantCulture));
                }
                return sb.ToString();
            }

            throw ExportNotImplementedException(data);
        }

        protected override object ConvertTo(object[] handlerValues)
        {
            Type typeOfSlot = base.GetHandlerSlot(0);
            if (handlerValues[0] == null)
                return new List<string>();
            // return new object[] { null }; http://iso/bugtracking/mod.aspx?bugid=4143
            string valueAsString;
            if(typeOfSlot == typeof(Enum))
                valueAsString = ((int)handlerValues[0]).ToString();
            else
                valueAsString = handlerValues[0].ToString();
            List<string> valueAsList = valueAsString.Length == 0 ? new List<string>() : new List<string>(valueAsString.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries));
            return valueAsList;
        }
        protected override object[] ConvertFrom(object value)
        {
            return new object[] { ConvertFromControlInner(value) };
        }
        private object ConvertFromControlInner(object value)
        {
            Type type = base.GetHandlerSlot(0);
            string[] arrayValue = ConvertToStringList(value).ToArray();
            switch (type.FullName)
            {
                case "System.String":
                    return String.Join(";", arrayValue);
                case "System.Enum":
                case "System.Int32":
                    if (arrayValue.Length == 0)
                        return 0;
                    return Convert.ToInt32(arrayValue[0]);
                default:
                    throw new NotSupportedException(String.Concat(
                        "ChoiceField not supports this type: ", value.GetType().FullName
                        , ". FieldName: ", this.Name
                        , ", ContentType: ", this.Content.ContentHandler.NodeType.Name
                        , ", ContentName: ", this.Content.ContentHandler.Name
                        ));
            }
        }

        public static List<string> ConvertToStringList(object value)
        {
            List<string> list = value as List<string>;
            if (list != null)
                return list;

            list = new List<string>();
            string stringValue = value as string;
            if (stringValue != null)
            {
                list.Add(stringValue);
                return list;
            }

            IEnumerable enumerableValue = value as IEnumerable;
            if (enumerableValue == null)
            {
                list.Add(value.ToString());
                return list;
            }

            foreach (object item in enumerableValue)
                list.Add(item.ToString());
            return list;

        }

        protected override bool ParseValue(string value)
        {
            var list = value.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries).ToList();
            this.SetData(list);
            return true;
        }

        public override bool HasValue()
        {
            return ((List<string>)OriginalValue).Count > 0;
        }
    }
}
