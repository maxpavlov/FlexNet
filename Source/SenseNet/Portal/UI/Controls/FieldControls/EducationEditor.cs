using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;
using SenseNet.ContentRepository.Storage;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:EducationEditor ID=\"EducationEditor1\" runat=server></{0}:UrlList>")]
    public class EducationEditor : GridEditor<SchoolItem>
    {
        private string _data;

        public EducationEditor()
        {
            this.InnerControlID = "InnerListView";
        }

        //========================================================================= Properties

        private string _controlPath = "/Root/System/SystemPlugins/Controls/EducationEditor.ascx";
        public override string ControlPath
        {
            get { return _controlPath; }
            set { _controlPath = value; }
        }

        //========================================================================= FieldControl methods

        protected override object GetDataFromDataList()
        {
            return this.DataList.Count == 0 ? 
                string.Empty : 
                SerializeDataList(this.DataList.Where(sc => !string.IsNullOrEmpty(sc.SchoolName)));
        }

        protected override void OnDataListViewItemDataBound(ListViewDataItem dataItem)
        {
            var myDataItem = dataItem.DataItem as SchoolItem;
            if (myDataItem == null)
                return;

            var tbSchoolName = GetSchoolNameControl(dataItem);
            if (tbSchoolName != null) 
                tbSchoolName.Text = myDataItem.SchoolName;
        }

        protected override SchoolItem GetNewDataItem()
        {
            return new SchoolItem { SchoolName = string.Empty };
        }

        protected override SchoolItem GetDataItemFromGui(ListViewDataItem dataItem, int index)
        {
            if (dataItem == null)
                return null;

            var tbSchoolName = GetSchoolNameControl(dataItem);
            var schoolName = tbSchoolName == null ? string.Empty : tbSchoolName.Text;

            return new SchoolItem { SchoolName = schoolName };
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (this.ControlMode == FieldControlControlMode.Browse && !string.IsNullOrEmpty(_data) 
                && !this.UseBrowseTemplate && this.DataListView == null)
            {
                //if we have no other option, render the raw value directly
                writer.Write(_data);
            }
            else
            {
                base.RenderContents(writer);
            }
        }

        public override void SetData(object data)
        {
            var schoolString = data as string;

            if (this.ControlMode == FieldControlControlMode.Browse)
                _data = schoolString;

            if (this.ControlStateLoaded || string.IsNullOrEmpty(schoolString))
                return;

            this.DeserializeToDataList(schoolString);
        }

        //========================================================================= Helper methods

        private static TextBox GetSchoolNameControl(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("tbSchoolName") as TextBox;
        }
    }

    [Serializable]
    public class SchoolItem
    {
        public string SchoolName { get; set; }
    }
}
