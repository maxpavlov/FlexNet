using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using SenseNet.ContentRepository.Storage;

using  SenseNet.ContentRepository.Schema;
using System.Xml;

namespace SenseNet.ContentRepository.Fields
{
	[ShortName("DateTime")]
	[DataSlot(0, RepositoryDataType.DateTime, typeof(DateTime))]
    [DefaultFieldSetting(typeof(DateTimeFieldSetting))]
	[DefaultFieldControl("SenseNet.Portal.UI.Controls.DatePicker")]
	public class DateTimeField : Field
	{
		protected override bool HasExportData { get { return true; } }
		protected override void ExportData(System.Xml.XmlWriter writer, ExportContext context)
		{
            writer.WriteString(GetXmlData());
		}
        protected override void WriteXmlData(XmlWriter writer)
        {
            ExportData(writer, null);
        }
		protected override void ImportData(System.Xml.XmlNode fieldNode, ImportContext context)
		{
			if (String.IsNullOrEmpty(fieldNode.InnerXml))
			{
				this.SetData(ActiveSchema.DateTimeMinValue);
				return;
			}
			DateTime value = Convert.ToDateTime(fieldNode.InnerXml);
			this.SetData(value < ActiveSchema.DateTimeMinValue ? ActiveSchema.DateTimeMinValue : value);
		}

        protected override string GetXmlData()
        {
            return XmlConvert.ToString((DateTime)GetData(), XmlDateTimeSerializationMode.Unspecified);
        }

        protected override bool ParseValue(string value)
        {
            DateTime dateTimeValue;
            if (!DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTimeValue))
                if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTimeValue))
                    if (!DateTime.TryParse(value, out dateTimeValue))
                        return false;
            this.SetData(dateTimeValue);
            return true;
        }
	}
}