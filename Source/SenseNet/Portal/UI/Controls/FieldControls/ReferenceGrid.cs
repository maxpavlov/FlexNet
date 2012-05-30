using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using System.Collections;
using SenseNet.ContentRepository.Fields;
using System.Web.UI;
using System.Runtime.Serialization.Json;
using SenseNet.Diagnostics;
using SNCS=SenseNet.Services.ContentStore;
using System.Xml;
using System.IO;
using System.Configuration;

namespace SenseNet.Portal.UI.Controls
{
    public class ReferenceGrid : FieldControl, INamingContainer, ITemplateFieldControl
    {
        /* =========================================================================== Members */
        private TextBox _inputTextBox;
        private Table _table;
        private Button _addButton;
        private Button _clearButton;
        private Button _changeButton;
        private IEnumerable<Node> _innerData;
        protected string TableControlID;
        protected string AddButtonControlID;
        protected string ClearButtonControlID;
        protected string ChangeButtonControlID;
        protected string PagerDivID;


        /* =========================================================================== Attributes */
        // default path in contentpicker
        [PersistenceMode(PersistenceMode.Attribute)]
        public string DefaultPath { get; set; }
        // comma separated string with contentpicker's default selection root paths
        [PersistenceMode(PersistenceMode.Attribute)]
        public string TreeRoots { get; set; }
        // comma separated string with contentpicker's default content types (not allowed content types!)
        [PersistenceMode(PersistenceMode.Attribute)]
        public string DefaultContentTypes { get; set; }


        /* =========================================================================== Properties */
        private bool IsMultiple
        {
            get
            {
                bool? allowMultiple = ((ReferenceFieldSetting)this.Field.FieldSetting).AllowMultiple;
                return allowMultiple == null ? false : (bool)allowMultiple;
            }
        }
        public TextBox ActualInputTextBox
        {
            get
            {
                if (IsTemplated)
                    return this.GetInnerControl() as TextBox;

                return _inputTextBox;
            }
        }
        public Table ActualTable
        {
            get
            {
                return IsTemplated ? this.GetTableControl() : _table;
            }
        }
        public Button ActualAddButton
        {
            get
            {
                return IsTemplated ? this.GetAddButtonControl() : _addButton;
            }
        }
        public Button ActualClearButton
        {
            get
            {
                return IsTemplated ? this.GetClearButtonControl() : _clearButton;
            }
        }
        public Button ActualChangeButton
        {
            get
            {
                return IsTemplated ? this.GetChangeButtonControl() : _changeButton;
            }
        }
        public List<string> _actualTreeRoots;
        public List<string> ActualTreeRoots
        {
            get
            {
                if (_actualTreeRoots == null)
                {
                    // tree roots come from attribute ? OR tree roots are defined in CTD
                    if (!string.IsNullOrEmpty(this.TreeRoots))
                        _actualTreeRoots = this.TreeRoots.Split(',', ';').ToList();
                    else
                        _actualTreeRoots = ((ReferenceFieldSetting)this.Field.FieldSetting).SelectionRoots;

                    if (_actualTreeRoots == null)
                    {
                        _actualTreeRoots = new List<string>();
                    }
                    else
                    {
                        // "." should be replaced with current path
                        // (when this is a new content, current path does not exist)
                        _actualTreeRoots = _actualTreeRoots.Where(t => t != "." || !this.Content.IsNew).Select(t => t == "." ? this.Content.Path : t).ToList();
                    }
                }
                return _actualTreeRoots;
            }
        }
        public string ActualDefaultPath
        {
            get
            {
                // default path comes from attribute
                if (!string.IsNullOrEmpty(this.DefaultPath))
                    return this.DefaultPath;

                // if default path is not set, only set it to current content if it is under first treeroot, or there are no treeroots given
                var parentPath = RepositoryPath.GetParentPath(this.Content.Path);
                if (parentPath != null && (ActualTreeRoots.Count == 0 || parentPath.StartsWith(ActualTreeRoots.First())))
                    return parentPath;

                return string.Empty;
            }
        }
        public bool ReadOnlyMode
        {
            get
            {
                return this.ReadOnly || this.Field.ReadOnly || this.ControlMode == FieldControlControlMode.Browse;
            }
        }


        /* =========================================================================== Constructor */
        public ReferenceGrid()
        {
            InnerControlID = "HiddenTextBoxControl";
            TableControlID = "TableControl";
            AddButtonControlID = "AddButtonControl";
            ChangeButtonControlID = "ChangeButtonControl";
            ClearButtonControlID = "ClearButtonControl";
            PagerDivID = "PagerDiv";

            _inputTextBox = new TextBox { ID = InnerControlID, TextMode = TextBoxMode.MultiLine };
            _inputTextBox.Style.Add(HtmlTextWriterStyle.Display, "none");
            _table = new Table { ID = TableControlID };
            _addButton = new Button { ID = AddButtonControlID, Text = "Add..." };
            _changeButton = new Button { ID = ChangeButtonControlID, Text = "Change..." };
            _clearButton = new Button { ID = ClearButtonControlID, Text = "Clear" };
        }

        
        /* =========================================================================== Methods */
        public override object GetData()
        {
            return GetDataInternal(this.ActualInputTextBox);
        }
        public override void SetData(object data)
        {
            SetDataInternal(data, this.ActualInputTextBox);
            var title = GetLabelForTitleControl() as Label;
            var desc = GetLabelForDescription() as Label;
            if (title != null) title.Text = this.Field.DisplayName;
            if (desc != null) desc.Text = this.Field.Description;
        }
        private object GetDataInternal(ITextControl inputTextBox)
        {
            if (IsMultiple)
            {
                string[] referencePaths = inputTextBox.Text.Split(',', ';');
                var list = new List<Node>();

                foreach (string item in referencePaths)
                {
                    string itemValue = item.Trim();
                    Node refNode = null;
                    if (itemValue.Length > 1)
                        refNode = Node.LoadNode(itemValue);
                    if (refNode != null)
                        list.Add(refNode);
                }
                return list;
            }

            return inputTextBox.Text.Length > 0 ? Node.LoadNode(inputTextBox.Text.Trim().Split(';', ',')[0]) : null;
        }
        private void SetDataInternal(object data, ITextControl inputTextBox)
        {
            Node dataNode = data as Node;
            if (dataNode != null)
            {
                _innerData = new List<Node>() { dataNode };
                inputTextBox.Text = Convert.ToString(dataNode.Path);
                return;
            }
            var nodes = data as IEnumerable<Node>;
            if (nodes != null)
            {
                _innerData = nodes;
                StringBuilder sb = new StringBuilder();
                foreach (Node item in nodes)
                {
                    if (sb.Length > 0)
                        sb.Append(";");
                    sb.Append(item.Path);
                }
                inputTextBox.Text = sb.ToString();
            }
        }
        protected override void OnInit(EventArgs e)
        {
            // loading css
            UITools.AddPickerCss();
            
            // loading scripts
            UITools.AddScript(UITools.ClientScriptConfigurations.SNReferenceGridPath);

            base.OnInit(e);

            if (!IsTemplated)
            {
                // original flow
                Controls.Add(_inputTextBox);
                Controls.Add(_table);
                Controls.Add(_addButton);
                Controls.Add(_changeButton);
                Controls.Add(_clearButton);
            }
            
            if (string.IsNullOrEmpty(this.ActualAddButton.OnClientClick))
                this.ActualAddButton.OnClientClick = GetAddButtonHandler();

            // change button has the same handler as add button !
            if (string.IsNullOrEmpty(this.ActualChangeButton.OnClientClick))
                this.ActualChangeButton.OnClientClick = GetAddButtonHandler();

            if (string.IsNullOrEmpty(ActualClearButton.OnClientClick))
                this.ActualClearButton.OnClientClick = GetClearButtonHandler();

            
        }
        protected override void OnPreRender(EventArgs e)
        {
            try
            {
                InitGrid();
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                
                this.Controls.Clear();
                this.Controls.Add(new LiteralControl(ex.Message));
            }
            
            base.OnPreRender(e);
        }
        protected override void RenderContents(HtmlTextWriter writer)
        {
            // templates
            if (UseBrowseTemplate)
            {
                base.RenderContents(writer);
                return;
            }
            if (UseEditTemplate)
            {
                ManipulateTemplateControls();
                base.RenderContents(writer);
                return;
            }
            if (UseInlineEditTemplate)
            { 
                ManipulateTemplateControls();
                base.RenderContents(writer);
                return;
            }

            // original flow
            if (this.ReadOnlyMode)
                RenderSimple(writer);
            else
                RenderEditor(writer);
        }
        protected virtual void RenderSimple(HtmlTextWriter writer)
        {
            _inputTextBox.RenderControl(writer);
            _table.RenderControl(writer);
        }
        protected virtual void RenderEditor(HtmlTextWriter writer)
        {
            _inputTextBox.RenderControl(writer);
            _table.RenderControl(writer);
            _addButton.RenderControl(writer);
            _changeButton.RenderControl(writer);
            _clearButton.RenderControl(writer);
        }
        private void ManipulateTemplateControls()
        {
            // in templated modes readonly logic is handled here!

            var innerControl = GetInnerControl();
            var addButtonControl = GetAddButtonControl();
            var changeButtonControl = GetChangeButtonControl();
            var clearButtonControl = GetClearButtonControl();
            var lt = GetLabelForTitleControl() as Label;
            var ld = GetLabelForDescription() as Label;

            if (this.ReadOnlyMode)
            {
                if (innerControl == null)
                    return;

                var p = innerControl.Parent;
                if (p != null)
                {
                    if (addButtonControl != null)
                        p.Controls.Remove(addButtonControl);

                    if (changeButtonControl != null)
                        p.Controls.Remove(changeButtonControl);

                    if (clearButtonControl != null)
                        p.Controls.Remove(clearButtonControl);

                    if (lt != null) lt.AssociatedControlID = string.Empty;
                    if (ld != null) ld.AssociatedControlID = string.Empty;
                }
            }
        }
        private void InitGrid()
        {
            string jsonData = string.Empty;

            using (var s = new MemoryStream())
            {
                var data = GetData();
                var fieldData = data as IEnumerable<Node>;
                if(fieldData == null)
                {
                    var n = data as Node;
                    if(n != null)
                    {
                        fieldData = new List<Node> {n};
                    }
                }
                if (fieldData != null)
                {
                    var workData = fieldData.Select<Node, SNCS.Content>(n => new SNCS.Content(n));
                    var serializer = new DataContractJsonSerializer(typeof(SNCS.Content[]));
                    serializer.WriteObject(s, workData.ToArray());
                    s.Flush();
                    s.Position = 0;
                    using (var sr = new StreamReader(s))
                    {
                        jsonData = sr.ReadToEnd();
                    }
                }
            }

            if (string.IsNullOrEmpty(jsonData))
                jsonData = "[]";

            var readOnlyStr = (this.ReadOnlyMode) ? "true" : "false";
            var pagerdiv = this.GetPagerDiv();
            var pagerdivid = pagerdiv == null ? string.Empty : pagerdiv.ClientID;
            var rownum = ConfigurationManager.AppSettings["SNReferenceGridRowNum"];

            var script = string.Format("SN.ReferenceGrid.init('{0}','{1}','{2}','{3}',({4}),{5},{6},'{7}',{8});",
                this.ActualTable.ClientID,          // displayAreaId
                this.ActualInputTextBox.ClientID,   // outputTextareaId
                this.ActualAddButton.ClientID,      // addButtonId
                this.ActualChangeButton.ClientID,   // changeButtonId
                jsonData,                           // initialSelection
                readOnlyStr,                        // readOnly
                IsMultiple ? "1" : "0",              // isMultiSelect
                pagerdivid,
                rownum
            );

            UITools.RegisterStartupScript("startup_" + this.ActualTable.ClientID, script, Page);
        }
        private string GetAddButtonHandler()
        {
            var script = string.Format("SN.ReferenceGrid.addButtonHandler({0}, {1}, {2}, {3}, {4}, {5}); return false;",
                "'" + this.ActualTable.ClientID + "'",  // displayAreaId
                GetTreeRootsParam(),                    // treeRoots
                GetDefaultPathParam(),                  // defaultPath
                GetMultiSelectModeParam(),              // multiSelectmode
                GetAllowedContentTypesParam(),          // allowedContentTypes
                GetDefaultContentTypesParam()           // default content types
                );

            return script;
        }
        private string GetClearButtonHandler()
        {
            return "SN.ReferenceGrid.clearButtonHandler(\"" + this.ActualTable.ClientID + "\"); return false;";
        }

        public override void DoAutoConfigure(FieldSetting setting)
        {
            //if the following fields are displayed with the generic control, they should be read-only 
            //because letting the user modify the values could lead to unwanted behavior otherwise
            switch (setting.Name)
            {
                case "ModifiedBy":
                case "CreatedBy":
                    this.ReadOnly = true;
                    break;
                default:
                    break;
            }

            base.DoAutoConfigure(setting);
        }
        
        /* =========================================================================== Picker init parameters */
        private string GetTreeRootsParam()
        {
            // ['/Root','/Root/IMS'] (or '' if not set)
            return GetStringListParam(this.ActualTreeRoots);
        }
        private string GetDefaultPathParam()
        {
            var defaultPath = this.ActualDefaultPath;

            // '/Root/IMS' (or '' if not set)
            var defaultPathStr = "'" + defaultPath + "'";

            // when treeroots are explicitely set default path may not be valid
            var rootList = this.ActualTreeRoots;
            if ((rootList == null) || (rootList.Count == 0))
                return defaultPathStr;

            // if default path falls under any of the treeroots, default path is ok.
            // otherwise there is no valid default path
            if (rootList.Any(s => defaultPath.StartsWith(s)))
                return defaultPathStr;

            return "null";
        }
        private string GetMultiSelectModeParam()
        {
            // 'button' or 'none'
            return IsMultiple ? "'button'" : "'none'";
        }
        private string GetAllowedContentTypesParam()
        {
            // ['User','Group'] (or '' if not set)
            var allowedList = ((ReferenceFieldSetting)this.Field.FieldSetting).AllowedTypes;
            return GetStringListParam(allowedList);
        }
        private string GetDefaultContentTypesParam()
        {
            if (string.IsNullOrEmpty(this.DefaultContentTypes))
                return "null";
                
            return GetStringListParam(this.DefaultContentTypes.Split(',',';').ToList());
        }
        private string GetStringListParam(List<string> list)
        {
            if ((list == null) || (list.Count == 0))
                return "null";

            return "[" + string.Join(",", list.Select(s => "'" + s + "'").ToArray()) + "]";
        }


        /* =========================================================================== ITemplateFieldControl */
        // hidden textbox
        public Control GetInnerControl()
        {
            return this.FindControlRecursive(InnerControlID);
        }
        public Table GetTableControl()
        {
            return this.FindControlRecursive(TableControlID) as Table;
        }
        public Panel GetPagerDiv()
        {
            return this.FindControlRecursive(PagerDivID) as Panel;
        }
        public Button GetAddButtonControl()
        {
            return this.FindControlRecursive(AddButtonControlID) as Button;
        }
        public Button GetChangeButtonControl()
        {
            return this.FindControlRecursive(ChangeButtonControlID) as Button;
        }
        public Button GetClearButtonControl()
        {
            return this.FindControlRecursive(ClearButtonControlID) as Button;
        }
        public Control GetLabelForDescription()
        {
            return this.FindControlRecursive(DescriptionControlID);
        }
        public Control GetLabelForTitleControl()
        {
            return this.FindControlRecursive(TitleControlID);
        }
    }
}
