using System;
using System.Collections.Generic;
using System.Text;

using  SenseNet.ContentRepository.Schema;
using System.Xml;

namespace SenseNet.ContentRepository.Fields
{
	[ShortName("Password")]
	[DataSlot(0, RepositoryDataType.String, typeof(string))]
	[DefaultFieldSetting(typeof(PasswordFieldSetting))]
	[DefaultFieldControl("SenseNet.Portal.UI.Controls.Password")]
	public class PasswordField : Field
	{
		public class PasswordData
		{
			public string Text { get; set; }
			public string Hash { get; set; }
		}

        public class OldPasswordData
        {
            public DateTime ModificationDate { get; set; }
            public string Hash { get; set; }
        }

		protected override bool HasExportData { get { return OriginalValue != null; } }
		protected override void ExportData(System.Xml.XmlWriter writer, ExportContext context)
		{
			var data = GetData() as PasswordData;
			if(data == null)
				return;
			if(String.IsNullOrEmpty(data.Hash))
				return;
			writer.WriteElementString("Hash", data.Hash);
		}
		protected override void ImportData(XmlNode fieldNode, ImportContext context)
		{
			var data = new PasswordData();
			foreach (XmlNode dataNode in fieldNode.SelectNodes("*"))
			{
				switch (dataNode.LocalName)
				{
					case "Text": data.Text = dataNode.InnerText; break;
					case "Hash": data.Hash = dataNode.InnerText; break;
				}
			}
			this.SetData(data);
		}

		protected override object[] ConvertFrom(object value)
		{
            var passwordData = value as PasswordData;
            if (passwordData != null)
                return new object[] { EncodeTransferData(passwordData) };
            var stringData = value as string;
            return new object[] { EncodeTransferData(stringData) };
        }
		protected override object ConvertTo(object[] handlerValues)
		{
			return new PasswordData { Hash = (string)handlerValues[0] };
		}
		private string EncodeTransferData(PasswordData data)
		{
            var user = this.Content.ContentHandler as User;
            if (user != null)
                user.Password = data.Text;

			if (data.Text != null)
				data.Hash = User.EncodePassword(data.Text);
			return data.Hash;
		}
        private string EncodeTransferData(string text)
        {
            var user = this.Content.ContentHandler as User;
            if (user != null)
                user.Password = text;

            string hash = null;
            if (text != null)
                hash = User.EncodePassword(text);
            return hash;
        }

        protected override void WriteXmlData(XmlWriter writer)
        {
            ExportData(writer, null);
        }
    }
}