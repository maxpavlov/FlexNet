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
    public abstract class GridEditor<T> : FieldControl, ITemplateFieldControl where T : class 
    {
        protected GridEditor()
        {
            InnerControlID = "InnerListView";
        }

        //========================================================================= Properties

        private int _maxRowCount = 50;
        protected virtual int MaxRowCount
        {
            get { return _maxRowCount; }
            set { _maxRowCount = Math.Max(0, value); }
        }

        public abstract string ControlPath { get; set; }

        protected virtual bool ControlStateLoaded { get; set; }

        private Dictionary<T, int> _dataItemIndexes;
        protected Dictionary<T, int> DataItemIndexes
        {
            get
            {
                if (_dataItemIndexes == null)
                {
                    var index = 0;
                    _dataItemIndexes = new Dictionary<T, int>();

                    foreach (var dataItem in DataList)
                    {
                        _dataItemIndexes.Add(dataItem, index++);
                    }
                }

                return _dataItemIndexes;
            }
        }

        private List<T> _dataList;
        protected List<T> DataList
        {
            get
            {
                if (_dataList == null)
                    _dataList = GetDataListFromGui();

                return _dataList;
            }
        }

        private ListView _dataListView;
        protected ListView DataListView
        {
            get
            {
                if (_dataListView == null && this.Controls.Count > 0)
                {
                    _dataListView = GetInnerControl() as ListView;

                    if (_dataListView != null)
                    {
                        _dataListView.ItemDataBound += DataListView_ItemDataBound;
                        _dataListView.ItemCommand += DataListView_ItemCommand;
                    }
                }

                return _dataListView;
            }
        }

        private IButtonControl _btnAddRow;
        protected IButtonControl ButtonAddRow
        {
            get
            {
                if (_btnAddRow == null && this.Controls.Count > 0)
                {
                    _btnAddRow = GetAddRowControl();

                    if (_btnAddRow != null)
                    {
                        _btnAddRow.Click += BtnAddRow_Click;
                    }
                }

                return _btnAddRow;
            }
        }

        //========================================================================= FieldControl functions

        public override object GetData()
        {
            ResetListData();

            return GetDataFromDataList();
        }

        protected abstract object GetDataFromDataList();

        public override void SetData(object data)
        {
            throw new NotImplementedException("Implement SetData method in derived classes");
        }

        //========================================================================= Control overrides

        protected override void OnInit(EventArgs e)
        {
            this.ControlStateLoaded = false;
            Page.RegisterRequiresControlState(this);

            base.OnInit(e);

            if (!UseBrowseTemplate && !UseEditTemplate && !UseInlineEditTemplate && 
                !string.IsNullOrEmpty(this.ControlPath) && Node.Exists(this.ControlPath))
            {
                var c = Page.LoadControl(this.ControlPath);
                if (c != null)
                {
                    this.Controls.Add(c);
                }
            }

            //find button and add event handler
            var b1 = ButtonAddRow;

            SetTitleControls();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.RefreshListView();
        }

        protected override void LoadControlState(object savedState)
        {
            if (savedState != null)
            {
                var state = savedState as object[];
                if (state != null && state.Length == 2)
                {
                    base.LoadControlState(state[0]);

                    if (state[1] != null)
                    {
                        this._dataList = (List<T>) state[1];
                        this.ControlStateLoaded = true;
                    }
                }
            }
            else
                base.LoadControlState(savedState);
        }

        protected override object SaveControlState()
        {
            var state = new object[2];

            state[0] = base.SaveControlState();
            state[1] = this.DataList;

            return state;
        }

        //========================================================================= Event handlers

        protected void DataListView_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            var dataItem = e.Item as ListViewDataItem;
            if (dataItem == null)
                return;

            var myDataItem = dataItem.DataItem as T;
            if (myDataItem == null)
                return;

            var btnRemove = GetRemoveControl(dataItem);
            if (btnRemove != null)
                btnRemove.CommandArgument = DataItemIndexes[myDataItem].ToString();

            OnDataListViewItemDataBound(dataItem);
        }

        /// <summary>
        /// Method for handling item databound event: filling controls, modifying ascx values
        /// </summary>
        /// <param name="dataItem"></param>
        protected abstract void OnDataListViewItemDataBound(ListViewDataItem dataItem);

        protected void DataListView_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            var di = e.Item as ListViewDataItem;
            if (di == null)
                return;

            switch (e.CommandName)
            {
                case "Remove":
                    //need to refresh the list using the latest control data
                    ResetListData();

                    var index = Convert.ToInt32(e.CommandArgument);
                    this.DataList.RemoveAt(index);
                    //for (var i = index; i < this.DataList.Count; i++)
                    //{
                    //    this.DataList[i].Id--;
                    //}

                    this.RefreshListView();
                    break;
                default:
                    OnDataListViewItemCommand(di);
                    break;
            }
        }

        /// <summary>
        /// Method for handling custom item commands
        /// </summary>
        /// <param name="dataItem"></param>
        protected virtual void OnDataListViewItemCommand(ListViewDataItem dataItem)
        {
        }

        protected void BtnAddRow_Click(object sender, EventArgs e)
        {
            if (this.DataListView == null || this.DataList.Count == MaxRowCount)
                return;

            //need to refresh the list using the latest control data
            ResetListData();

            this.DataList.Add(this.GetNewDataItem());
            this.RefreshListView();
        }

        //========================================================================= Helper functions

        protected abstract T GetNewDataItem();

        protected virtual List<T> GetDataListFromGui()
        {
            if (this.DataListView == null)
                return new List<T>();

            var dataList = new List<T>();

            var index = 0;

            foreach (var dataItem in this.DataListView.Items)
            {
                var myDataItem = GetDataItemFromGui(dataItem, index++);

                if (myDataItem != null)
                    dataList.Add(myDataItem);
            }

            return dataList;
        }

        protected abstract T GetDataItemFromGui(ListViewDataItem dataItem, int index);

        private void RefreshListView()
        {
            if (this.DataListView == null)
                return;

            this.DataListView.DataSource = this.DataList;
            this.DataListView.DataBind();
        }

        protected void ResetListData()
        {
            this._dataList = null;
            this._dataItemIndexes = null;
        }

        protected virtual string SerializeDataList(IEnumerable<T> dataList)
        {
            var ws = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                ConformanceLevel = ConformanceLevel.Auto
            };

            var ser = new XmlSerializer(typeof(List<T>));
            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb, ws))
            {
                if (writer != null)
                {
                    ser.Serialize(writer, dataList.ToList());
                    writer.Flush();
                }
            }

            return sb.ToString();
        }

        protected virtual void DeserializeToDataList(string data)
        {
            try
            {
                this.DataList.Clear();

                var ser = new XmlSerializer(typeof(List<T>));
                using (var reader = new XmlTextReader(data, XmlNodeType.Element, new XmlParserContext(null, null, "", XmlSpace.Default)))
                {
                    var dataItems = ser.Deserialize(reader) as List<T>;
                    if (dataItems != null)
                        this.DataList.AddRange(dataItems);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }
        }

        protected virtual void SetTitleControls()
        {
            // synchronize data with controls are given in the template
            var title = GetLabelForTitleControl() as Label;
            var desc = GetLabelForDescription() as Label;

            if (title != null)
                title.Text = this.Field.DisplayName;
            if (desc != null)
                desc.Text = this.Field.Description;
        }

        #region ITemplateFieldControl Members

        public Control GetInnerControl()
        {
            return this.FindControlRecursive(InnerControlID);
        }

        public Control GetLabelForDescription()
        {
            return this.FindControlRecursive(DescriptionControlID);
        }

        public Control GetLabelForTitleControl()
        {
            return this.FindControlRecursive(TitleControlID);
        }

        public IButtonControl GetAddRowControl()
        {
            return this.FindControlRecursive("ButtonAddRow") as IButtonControl;
        }

        protected static IButtonControl GetRemoveControl(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("ButtonRemoveRow") as IButtonControl;
        }

        #endregion
    }
}
