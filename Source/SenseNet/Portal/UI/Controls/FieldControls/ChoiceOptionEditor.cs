using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Xml;
using SenseNet.ContentRepository.Fields;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.UI.Controls
{
    public class ChoiceOptionEditor : GridEditor<ChoiceOption>
    {
        //========================================================================= FieldControl functions

        private string _controlPath = "/Root/System/SystemPlugins/ListView/ChoiceOptionEditor.ascx";
        public override string ControlPath
        {
            get { return _controlPath; }
            set { _controlPath = value; }
        }

        protected override object GetDataFromDataList()
        {
            if (this.DataList.Count == 0)
                return string.Empty;

            var result = string.Empty;
            var sw = new StringWriter();
            var ws = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                ConformanceLevel = ConformanceLevel.Fragment
            };

            using (var writer = XmlWriter.Create(sw, ws))
            {
                if (writer != null)
                {
                    writer.WriteStartElement("Options");

                    foreach (var option in this.DataList)
                    {
                        if (string.IsNullOrEmpty(option.Value) || string.IsNullOrEmpty(option.Text))
                            throw new InvalidOperationException("Empty option");

                        option.WriteXml(writer);
                    }

                    writer.WriteEndElement();
                    writer.Flush();

                    result = sw.ToString();
                }
            }

            return result;
        }

        protected override ChoiceOption GetNewDataItem()
        {
            return new ChoiceOption(string.Empty, string.Empty);
        }

        protected override ChoiceOption GetDataItemFromGui(ListViewDataItem dataItem, int index)
        {
            if (dataItem == null)
                return null;

            var tbOptValue = GetOptionValueControl(dataItem);
            var tbOptText = GetOptionTextControl(dataItem);
            var optValue = tbOptValue == null ? string.Empty : tbOptValue.Text;
            var optText = tbOptText == null ? string.Empty : tbOptText.Text;

            if (string.IsNullOrEmpty(optValue))
                optValue = optText;

            //if (string.IsNullOrEmpty(optValue) || string.IsNullOrEmpty(optText))
            //    return null;

            return new ChoiceOption(optValue, optText);
        }

        public override void SetData(object data)
        {
            if (this.ControlStateLoaded)
                return;

            var optString = data as string;

            if (string.IsNullOrEmpty(optString))
                return;

            try
            {
                var optDoc = new XmlDocument();
                optDoc.LoadXml(optString);

                var et = string.Empty;

                if (optDoc.DocumentElement != null)
                    ChoiceFieldSetting.ParseOptions(optDoc.DocumentElement.CreateNavigator(), this.DataList, out et);
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }
        }

        protected override void OnDataListViewItemDataBound(ListViewDataItem dataItem)
        {
            var myDataItem = dataItem.DataItem as ChoiceOption;
            if (myDataItem == null)
                return;

            var tbOptValue = GetOptionValueControl(dataItem);
            var tbOptText = GetOptionTextControl(dataItem);

            if (tbOptValue != null) tbOptValue.Text = myDataItem.Value;
            if (tbOptText != null) tbOptText.Text = myDataItem.Text;
        }

        //========================================================================= Helper functions

        private static TextBox GetOptionValueControl(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("tbOptionValue") as TextBox;
        }

        private static TextBox GetOptionTextControl(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("tbOptionText") as TextBox;
        }
    }
}
