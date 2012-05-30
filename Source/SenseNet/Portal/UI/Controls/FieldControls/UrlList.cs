using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.UI.Controls
{
	[ToolboxData("<{0}:UrlList ID=\"UrlList1\" runat=server></{0}:UrlList>")]
    public class UrlList : GridEditor<UrlItem>
    {
        private readonly TextBox _inputTextBox;

        public UrlList()
        {
            _inputTextBox = new TextBox { ID = InnerControlID };
        }

        //========================================================================= Properties

        private string _controlPath = "/Root/System/SystemPlugins/Controls/UrlListEditor.ascx";
        public override string ControlPath
        {
            get
            {
                //only use the fully funtcional control in inline mode...
                if (this.ContentView.ViewMode == ViewMode.Edit || this.ContentView.ViewMode == ViewMode.New)
                    return string.Empty;

                return _controlPath;
            }
            set { _controlPath = value; }
        }

        //========================================================================= FieldControl functions

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (Configuration.UrlListSection.Current.Sites.Count > 0)
            {
                // show urllist override info
                var pnlConfigInfo = this.FindControlRecursive("pnlConfigInfo") as Panel;
                pnlConfigInfo.Visible = true;
            }

            var tbUrlList = GetUrlListTextBoxControl();
            if (tbUrlList != null)
                tbUrlList.Text = GetUrlListStringFromData();

            var lblUrlList = GetUrlListLabelControl();
            if (lblUrlList != null)
                lblUrlList.Text = GetUrlListFormattedStringFromData();

            if (this.DataListView != null || tbUrlList != null) 
                return;

            //Content Explorer behavior: simple textarea
            _inputTextBox.TextMode = TextBoxMode.MultiLine;
            _inputTextBox.CssClass = String.IsNullOrEmpty(this.CssClass) ? "sn-ctrl sn-ctrl-number" : this.CssClass;
            _inputTextBox.Rows = 5;
            _inputTextBox.Columns = 50;

            this.Controls.Add(_inputTextBox);
        }

        protected override object GetDataFromDataList()
        {
            if (this.DataListView == null)
            {
                //no listview --> try textbox
                var tbUrlList = GetUrlListTextBoxControl();
                if (tbUrlList != null)
                {
                    var dict = SenseNet.Portal.Site.ParseUrlList(tbUrlList.Text);

                    ResetListData();

                    //refresh datalist to have the latest values from GUI
                    BuildDataList(dict);

                    return dict;
                }

                return SenseNet.Portal.Site.ParseUrlList(_inputTextBox.Text);
            }

            if (this.DataList.Count == 0)
                return null;

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
                    foreach (var urlItem in this.DataList)
                    {
                        if (string.IsNullOrEmpty(urlItem.SiteName))
                            throw new InvalidOperationException("Empty url");

                        urlItem.WriteXml(writer);
                    }

                    writer.Flush();

                    result = sw.ToString();
                }
            }

            return Portal.Site.ParseUrlList(result);
        }

        protected override UrlItem GetNewDataItem()
        {
            return new UrlItem { AuthenticationType = string.Empty, SiteName = string.Empty };
        }

        protected override List<UrlItem> GetDataListFromGui()
        {
            if (this.DataListView != null)
                return base.GetDataListFromGui();

            var tbUrlList = GetUrlListTextBoxControl();
            
            return tbUrlList != null ? GetUrlListDictionary(tbUrlList.Text) : new List<UrlItem>();
        }

	    protected override UrlItem GetDataItemFromGui(ListViewDataItem dataItem, int index)
        {
            if (dataItem == null)
                return null;

            var listAuthType = GetAuthTypeControl(dataItem);
            var tbSiteName = GetSiteNameControl(dataItem);
            var authType = listAuthType == null ? string.Empty : listAuthType.SelectedValue;
            var siteName = tbSiteName == null ? string.Empty : tbSiteName.Text;

            return new UrlItem { AuthenticationType = authType, SiteName = siteName };
        }

        public override void SetData(object data)
        {
            var dictUrls = data as IDictionary<string, string>;

            if (dictUrls == null)
                return;

            _inputTextBox.Text = SenseNet.Portal.Site.UrlListToString(dictUrls);

            if (this.ControlStateLoaded)
                return;

            try
            {
                BuildDataList(dictUrls);
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }
        }

        protected override void OnDataListViewItemDataBound(ListViewDataItem dataItem)
        {
            var myDataItem = dataItem.DataItem as UrlItem;
            if (myDataItem == null)
                return;

            var listAuthType = GetAuthTypeControl(dataItem);
            var labelAuthType = GetAuthTypeBrowseControl(dataItem);
            var tbSiteName = GetSiteNameControl(dataItem);
            var labelSiteName = GetSiteNameBrowseControl(dataItem);

            //edit mode controls
            if (listAuthType != null)
            {
                var index = 0;
                foreach (ListItem item in listAuthType.Items)
                {
                    if (item.Value.CompareTo(myDataItem.AuthenticationType) != 0)
                    {
                        index++;
                        continue;
                    }

                    listAuthType.SelectedIndex = index;
                    break;
                }
            }

            if (tbSiteName != null) tbSiteName.Text = myDataItem.SiteName;

            //browse mode controls
            if (labelAuthType != null)
                labelAuthType.Text = myDataItem.AuthenticationType;

            if (labelSiteName != null)
                labelSiteName.Text = myDataItem.SiteName;
        }

        //========================================================================= Helper functions

        private static ListControl GetAuthTypeControl(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("ListAuthenticationType") as ListControl;
        }

        private static Label GetAuthTypeBrowseControl(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("LabelAuthType") as Label;
        }

        private static TextBox GetSiteNameControl(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("TextBoxSiteName") as TextBox;
        }

        private static Label GetSiteNameBrowseControl(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("LabelSiteName") as Label;
        }

        public Label GetUrlListLabelControl()
        {
            return this.GetInnerControl() as Label;
        }

        public TextBox GetUrlListTextBoxControl()
        {
            return this.GetInnerControl() as TextBox;
        }

        private string GetUrlListStringFromData()
        {
            var dict = new Dictionary<string, string>();

            foreach (var item in this.DataList)
            {
                dict.Add(item.SiteName, item.AuthenticationType);
            }

            return SenseNet.Portal.Site.UrlListToString(dict);
        }

        private string GetUrlListFormattedStringFromData()
        {
            var sb = new StringBuilder();

            foreach (var item in this.DataList)
            {
                sb.AppendFormat("{0} ({1}); ", item.SiteName, item.AuthenticationType);
            }

            return sb.ToString();
        }

        private static List<UrlItem> GetUrlListDictionary(string data)
        {
            var dict = SenseNet.Portal.Site.ParseUrlList(data);

            return (from siteName in dict.Keys
                    select new UrlItem { AuthenticationType = dict[siteName], SiteName = siteName }).ToList();
        }

        private void BuildDataList(IDictionary<string, string> data)
        {
            this.DataList.Clear();

            if (data == null)
                return;

            this.DataList.AddRange(from siteName in data.Keys
                                   select new UrlItem { AuthenticationType = data[siteName], SiteName = siteName });  
        }
    }

    [Serializable]
    public class UrlItem
    {
        internal string SiteName { get; set; }
        internal string AuthenticationType { get; set; }

        internal void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Url");

            if (!string.IsNullOrEmpty(this.AuthenticationType))
                writer.WriteAttributeString("authType", this.AuthenticationType);

            writer.WriteString(this.SiteName);
            writer.WriteEndElement();
        }
    }
}