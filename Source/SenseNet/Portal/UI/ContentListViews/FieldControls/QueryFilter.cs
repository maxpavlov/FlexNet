using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using System.Xml;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.Portal.UI.Controls;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Specialized;
using SenseNet.ContentRepository.Storage.Search;
using System.Web.UI.HtmlControls;

namespace SenseNet.Portal.UI.ContentListViews.FieldControls
{
    public class QueryFilter : FieldControl
    {
        private const int MaxExpressionCount = 50;
        private static readonly NameValueCollection ReferenceFields = new NameValueCollection
                                                                              {
                                                                                  { "CreatedBy", "CreatedById" },
                                                                                  { "ModifiedBy", "ModifiedById" }
                                                                              };

        //========================================================================= Properties

        private Query _query;

        private List<ExpressionInfo> _expInfoList;
        private List<ExpressionInfo> ExpInfoList
        {
            get
            {
                if (_expInfoList == null)
                    _expInfoList = GetExpressionInfoListFromGui();

                return _expInfoList;
            }
        }

        private System.Web.UI.WebControls.ListView _expressionListView;
        public System.Web.UI.WebControls.ListView ExpressionListView
        {
            get
            {
                if (_expressionListView == null && this.Controls.Count > 0)
                {
                    _expressionListView =
                        this.Controls[0].FindControl("InnerListView") as System.Web.UI.WebControls.ListView;

                    if (_expressionListView != null)
                    {
                        _expressionListView.ItemDataBound += ExpressionListView_ItemDataBound;
                        _expressionListView.ItemCommand += ExpressionListView_ItemCommand;
                    }
                }

                return _expressionListView;
            }
        }
        
        private LinkButton _btnAddRule;
        public LinkButton ButtonAddRule
        {
            get
            {
                if (_btnAddRule == null && this.Controls.Count > 0)
                {
                    _btnAddRule = this.Controls[0].FindControl("ButtonAddRule") as LinkButton;

                    if (_btnAddRule != null)
                        _btnAddRule.Click += BtnAddRule_Click;
                }

                return _btnAddRule;
            }
        }

        private TextBox _tbLucene;
        public TextBox LuceneTextBox
        {
            get
            {
                if (_tbLucene == null && this.Controls.Count > 0)
                {
                    _tbLucene = this.Controls[0].FindControl("tbLucene") as TextBox;
                }

                return _tbLucene;
            }
        }

        private HtmlControl _panelLucene;
        public HtmlControl PanelLucene
        {
            get
            {
                if (_panelLucene == null && this.Controls.Count > 0)
                {
                    _panelLucene = this.Controls[0].FindControlRecursive("panelLucId") as HtmlControl;
                }

                return _panelLucene;
            }
        }

        private HtmlControl _panelNodeQuery;
        public HtmlControl PanelNodeQuery
        {
            get
            {
                if (_panelNodeQuery == null && this.Controls.Count > 0)
                {
                    _panelNodeQuery = this.Controls[0].FindControlRecursive("panelNQId") as HtmlControl;
                }

                return _panelNodeQuery;
            }
        }

        private GenericContent _systemContext;
        public GenericContent SystemContext
        {
            get
            {
                if (_systemContext == null)
                {
                    var node = Content.ContentHandler as GenericContent;
                    _systemContext = (node != null) ? node.MostRelevantSystemContext : null;
                }

                return _systemContext;
            }
        }

        private List<FieldSetting> _availableFields;
        public IEnumerable<FieldSetting> AvailableFields
        {
            get
            {
                if (_availableFields == null)
                {
                    _availableFields = SystemContext.GetAvailableFields();
                    _availableFields.Sort(CompareFields);
                }

                return _availableFields;
            }
        }

        //========================================================================= FieldControl functions

        public override object GetData()
        {
            ResetListData();

            if (LuceneTextBox != null && !string.IsNullOrEmpty(LuceneTextBox.Text))
                return LuceneTextBox.Text;

            if (this.ExpInfoList.Count == 0 || (this.ExpInfoList.Count == 1 && this.ExpInfoList[0].IsEmpty))
                return string.Empty;
            
            _query = new Query();

            //EXPLIST: a and b or c and d or e
            //QUERY  : (a & b) | (c & d) | (d & e) 

            try
            {
                ShiftOperatorsDown();

                if (this.ExpInfoList.Count == 1)
                {
                    _query.Root = GetExpressionItem(this.ExpInfoList[0]);
                    _query.Root.LogicalOperator = LogicalOperator.None;
                }
                else
                {
                    var co = ContainsOr(this.ExpInfoList);
                    var root = new Group { LogicalOperator = (co ? LogicalOperator.Or : LogicalOperator.None) };
                    var current = co ? new Group { LogicalOperator = LogicalOperator.And } : root;

                    foreach (var expInfo in this.ExpInfoList)
                    {
                        switch (expInfo.LogicalOperator)
                        {
                            case LogicalOperator.And:
                                break;
                            case LogicalOperator.Or:
                                //add current and open new block
                                if (current.Items.Count > 1)
                                    root.Items.Add(current);
                                else
                                    root.Items.Add(current.Items[0]);

                                current = new Group { LogicalOperator = LogicalOperator.And };
                                
                                break;
                        }

                        current.Items.Add(GetExpressionItem(expInfo));
                    }

                    //the last block
                    if (root != current)
                    {
                        if (current.Items.Count > 1)
                            root.Items.Add(current);
                        else
                            root.Items.Add(current.Items[0]);
                    }

                    _query.Root = root;
                }
            }
            catch(Exception ex)
            {
                throw new FieldControlDataException(this, "InvalidQuery", ex.Message);
            }

            var xs = new XmlSerializer(typeof(Query));
            var sw = new StringWriter();
            var xw = new XmlTextWriter(sw);

            xs.Serialize(xw, _query);

            var result = sw.ToString();

            xw.Close();

            var nodeQueryXml = Query.GetNodeQueryXml(result);

            //this is for validation
            NodeQuery.Parse(nodeQueryXml);

            return result;
        }
        
        public override void SetData(object data)
        {
            var queryString = data as string;

            if (string.IsNullOrEmpty(queryString))
                return;

            if (queryString.StartsWith("<"))
            {
                try
                {
                    var xs = new XmlSerializer(typeof (Query));

                    var sr = new StringReader(queryString);
                    var xr = new XmlTextReader(sr);

                    _query = xs.Deserialize(xr) as Query;

                    xr.Close();
                }
                catch
                {
                    //TODO: handle wrong query here (modified by hand somewhere else)
                }

                ShowNodeQueryPanel();
            }
            else
            {
                if (LuceneTextBox != null)
                    LuceneTextBox.Text = queryString;
            }
        }

        //========================================================================= Control overrides

        protected override void OnInit(EventArgs e)
        {
            if (this.ExpressionListView == null)
            {
                var c = Page.LoadControl("/Root/System/SystemPlugins/ListView/QueryFilterControl.ascx");
                if (c != null)
                {
                    this.Controls.Add(c);

                    InitControls();
                }
            }

            UITools.AddScript(UITools.ClientScriptConfigurations.jQueryPath);

            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this._expInfoList == null)
            {
                _expInfoList = GetExpressionInfoListFromQuery(_query);

                ShiftOperatorsUp();
            }

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
                        this._expInfoList = (List<ExpressionInfo>)state[1];
                }
            }
            else
                base.LoadControlState(savedState);
        }

        protected override object SaveControlState()
        {
            var state = new object[2];

            state[0] = base.SaveControlState();
            state[1] = this.ExpInfoList;

            return state;
        }

        //========================================================================= Event handlers

        protected void ExpressionListView_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            var dataItem = e.Item as ListViewDataItem;
            if (dataItem == null)
                return;

            var expInfo = (ExpressionInfo)dataItem.DataItem;

            var btnRemove = GetRemoveControl(dataItem);
            var ddField = GetFieldNameControl(dataItem);
            var ddRelOp = GetRelOpControl(dataItem);
            var tbExp = GetExpressionControl(dataItem);
            var ddChainOp = GetChainOpControl(dataItem);

            if (ddField != null)
            {
                ddField.DataSource = this.AvailableFields;
                ddField.DataTextField = "DisplayName";
                ddField.DataValueField = "Name";
                ddField.DataBind();

                ddField.Items.Insert(0, new ListItem("", ""));

                for (var i = 0; i < ddField.Items.Count; i++)
                {
                    if (ddField.Items[i].Value.CompareTo(expInfo.FieldName) != 0) 
                        continue;

                    ddField.SelectedIndex = i;
                    break;
                }
            }

            if (ddRelOp != null)
                ddRelOp.SelectedIndex = (int)expInfo.RelationalOperator;

            if (ddChainOp != null)
            {
                if (expInfo.Id == this.ExpInfoList.Count - 1)
                    ddChainOp.Visible = false;

                ddChainOp.SelectedIndex = (int) expInfo.LogicalOperator;
            }

            if (tbExp != null)
                tbExp.Text = expInfo.Expression;

            if (btnRemove != null)
                btnRemove.CommandArgument = expInfo.Id.ToString();
        }

        protected void ExpressionListView_ItemCommand(object sender, ListViewCommandEventArgs e)
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
                    this.ExpInfoList.RemoveAt(index);
                    for (var i = index; i < this.ExpInfoList.Count; i++)
                    {
                        this.ExpInfoList[i].Id--; 
                    }

                    this.RefreshListView();
                    break;
            }
        }

        protected void BtnAddRule_Click(object sender, EventArgs e)
        {
            if (this.ExpressionListView == null || this.ExpInfoList.Count == MaxExpressionCount)
                return;

            //need to refresh the list using the latest control data
            ResetListData();

            this.ExpInfoList.Add(new ExpressionInfo { Id = this.ExpInfoList.Count });
            this.RefreshListView();
        }

        //========================================================================= Helper functions

        private List<ExpressionInfo> GetExpressionInfoListFromGui()
        {
            var expInfoList = new List<ExpressionInfo>();
            if (this.ExpressionListView == null)
                return expInfoList;

            var index = 0;

            foreach (var dataItem in this.ExpressionListView.Items)
            {
                var expInfo = new ExpressionInfo { Id = index++ };

                var ddField = GetFieldNameControl(dataItem);
                var ddRelOp = GetRelOpControl(dataItem);
                var tbExp = GetExpressionControl(dataItem);
                var ddChainOp = GetChainOpControl(dataItem);

                if (ddField != null) expInfo.FieldName = ddField.SelectedValue;
                if (ddRelOp != null) expInfo.RelationalOperator = (Operator)Convert.ToInt32(ddRelOp.SelectedValue);
                if (tbExp != null) expInfo.Expression = tbExp.Text;
                if (ddChainOp != null) expInfo.LogicalOperator = (LogicalOperator)Convert.ToInt32(ddChainOp.SelectedValue);

                expInfoList.Add(expInfo);
            }

            return expInfoList;
        }

        private void InitControls()
        {
            //find button and add event handler
            var b1 = ButtonAddRule;
        }

        private static DropDownList GetFieldNameControl(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("ddField") as DropDownList;
        }

        private static DropDownList GetRelOpControl(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("ddRelOp") as DropDownList;
        }

        private static TextBox GetExpressionControl(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("tbExp") as TextBox;
        }

        private static DropDownList GetChainOpControl(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("ddChainOp") as DropDownList;
        }

        private static Button GetRemoveControl(ListViewDataItem dataItem)
        {
            return dataItem.FindControl("btnRemove") as Button;
        }

        private ExpressionItem GetExpressionItem(ExpressionInfo expInfo)
        {
            if (expInfo == null || expInfo.IsEmpty)
                return null;

            PropertyType pType = null;

            var pred = new Predicate
            {
                LeftOperand = expInfo.FieldName,
                LogicalOperator = LogicalOperator.None, //expInfo.LogicalOperator,
                Operator = expInfo.RelationalOperator,
                RightOperand = expInfo.Expression
            };

            TransformReferenceToInt(pred);

            switch (pred.LeftOperand)
            {
                case "Id": 
                case "ParentId":
                case "Index":
                case "IsDeleted":
                case "IsInherited":
                case "Locked":
                case "LockType":
                case "LockTimeout":
                case "CreatedById":
                case "ModifiedById":
                case "LockedById":
                    pred.PredicateType = PredicateType.IntExpression;
                    break;

                case "CreatedBy":
                case "ModifiedBy":
                    pred.PredicateType = PredicateType.ReferenceExpression;
                    break;
                
                case "Name":
                case "Path":
                case "LockToken":
                    pred.PredicateType = PredicateType.StringExpression;
                    break;

                case "CreationDate":
                case "ModificationDate":
                case "LockDate":
                case "LastLockUpdate":
                    pred.PredicateType = PredicateType.DateTimeExpression;
                    break;

                //case "NodeType":
                //case "ContentListId": 
                //case "ContentListType": 
                //case "Parent":
                //case "IsModified":
                //case "NodeCreationDate":
                //case "NodeCreatedBy":
                //case "Version": 
                //case "VersionId": 
                //case "Lock": 
                //case "LockedBy":
                //case "ETag": 
                //case "Security":

                default:
                    //#Text1 -> #String_0
                    if (expInfo.FieldName.StartsWith("#"))
                    {
                        var cList = SystemContext as ContentList;
                        pred.LeftOperand = cList.GetPropertySingleId(expInfo.FieldName);
                    }

                    pType = ActiveSchema.PropertyTypes[pred.LeftOperand];
                    break;
            }

            if (pType != null)
            {
                switch (pType.DataType)
                {
                    case DataType.String:
                        pred.PredicateType = PredicateType.StringExpression;
                        break;
                    case DataType.Reference:
                        pred.PredicateType = PredicateType.ReferenceExpression;
                        break;
                    case DataType.Int:
                        pred.PredicateType = PredicateType.IntExpression;
                        break;
                    case DataType.Text:
                        pred.PredicateType = PredicateType.SearchExpression;
                        break;
                    case DataType.DateTime:
                        pred.PredicateType = PredicateType.DateTimeExpression;
                        break;

                    default:
                        return null;
                }
            }

            return pred;
        }

        private ExpressionInfo GetExpressionInfo(Predicate pred)
        {
            TransformContentListFieldSingleId(pred);
            TransformIntToReference(pred);

            return new ExpressionInfo
                       {
                           LogicalOperator = pred.LogicalOperator,
                           Expression = pred.RightOperand,
                           FieldName = pred.LeftOperand,
                           RelationalOperator = pred.Operator
                       };
        }

        private List<ExpressionInfo> GetExpressionInfoListFromQuery(Query query)
        {
            var expInfoList = new List<ExpressionInfo>();
            var index = 0;

            if (query == null)
                return expInfoList;

            var gr1 = query.Root as Group;
            var single = query.Root as Predicate;
            if (single != null)
            {
                expInfoList.Add(GetExpressionInfo(single));

                return expInfoList;
            }

            if (gr1 == null)
                return expInfoList;

            foreach (var exp in gr1.Items)
            {
                var pred = exp as Predicate;
                var gr = exp as Group;

                if (pred != null)
                {
                    var expInfo = GetExpressionInfo(pred);
                    expInfo.Id = index++;
                    expInfoList.Add(expInfo);
                    continue;
                }

                if (gr == null) 
                    continue;

                var count = 0;

                foreach (var item in gr.Items)
                {
                    var pred2 = item as Predicate;
                    if (pred2 == null)
                        continue;

                    count++;

                    var expInfo = GetExpressionInfo(pred2);
                    expInfo.Id = index++;
                    expInfo.LogicalOperator = gr1.LogicalOperator == LogicalOperator.Or && count == 1
                                                  ? LogicalOperator.Or
                                                  : LogicalOperator.None;
                    expInfoList.Add(expInfo);
                }
            }

            return expInfoList;
        }

        private void ShiftOperatorsUp()
        {
            //on the GUI, the logical operators appear at the end of the line,
            //this means they are related to the next predicate!
            //But when stored, we need to know the relation with the previous
            //predicate, this is why we shift up and down...
            if (this.ExpInfoList == null || this.ExpInfoList.Count == 0)
                return;

            for (var i = 0; i < this.ExpInfoList.Count - 1; i++)
            {
                this.ExpInfoList[i].LogicalOperator = this.ExpInfoList[i + 1].LogicalOperator;
            }

            this.ExpInfoList[this.ExpInfoList.Count - 1].LogicalOperator = LogicalOperator.And;
        }

        private void ShiftOperatorsDown()
        {
            //on the GUI, the logical operators appear at the end of the line,
            //this means they are related to the next predicate!
            //But when stored, we need to know the relation with the previous
            //predicate, this is why we shift up and down...
            if (this.ExpInfoList == null || this.ExpInfoList.Count == 0)
                return;
            
            for (var i = this.ExpInfoList.Count - 1; i > 0; i--)
            {
                this.ExpInfoList[i].LogicalOperator = this.ExpInfoList[i - 1].LogicalOperator;
            }

            this.ExpInfoList[0].LogicalOperator = LogicalOperator.And;
        }

        private static void TransformReferenceToInt(Predicate pred)
        {
            if (ReferenceFields[pred.LeftOperand] != null)
                pred.LeftOperand = ReferenceFields[pred.LeftOperand];
        }

        private static void TransformIntToReference(Predicate pred)
        {
            foreach (var refFieldName in ReferenceFields.AllKeys)
            {
                if (ReferenceFields[refFieldName].CompareTo(pred.LeftOperand) == 0)
                    pred.LeftOperand = refFieldName;
            }
        }

        private void TransformContentListFieldSingleId(Predicate pred)
        {
            //#String_0 -> #Text1
            if (!pred.LeftOperand.StartsWith("#"))
                return;

            foreach (var fs in this.SystemContext.GetAvailableFields())
            {
                if (!fs.Name.StartsWith("#"))
                    continue;

                var cList = SystemContext as ContentList;
                var sing = cList.GetPropertySingleId(fs.Name);

                if (sing.CompareTo(pred.LeftOperand) != 0) 
                    continue;

                pred.LeftOperand = fs.Name;
                return;
            }
        }

        private static bool ContainsOr(IEnumerable<ExpressionInfo> expInfoList)
        {
            foreach (var exp in expInfoList)
            {
                if (exp.LogicalOperator == LogicalOperator.Or)
                    return true;
            }

            return false;
        }

        private void RefreshListView()
        {
            this.ExpressionListView.DataSource = this.ExpInfoList;
            this.ExpressionListView.DataBind();

            if (this.ExpInfoList.Count > 0)
                ShowNodeQueryPanel();
        }

        private void ResetListData()
        {
            this._expInfoList = null;
        }

        private void ShowNodeQueryPanel()
        {
            if (PanelLucene != null)
                PanelLucene.Attributes.CssStyle["display"] = "none";
            if (PanelNodeQuery != null)
                PanelNodeQuery.Attributes.CssStyle["display"] = "block";
        }

        private static int CompareFields(FieldSetting x, FieldSetting y)
        {
            return x.DisplayName.CompareTo(y.DisplayName);
        }

        [Serializable]
        private class ExpressionInfo
        {
            public int Id;
            public string FieldName;
            public Operator RelationalOperator;
            public string Expression;
            public LogicalOperator LogicalOperator;

            public bool IsEmpty
            {
                get { return string.IsNullOrEmpty(this.FieldName); }
            }
        }
    }
}
