using System;
using System.Collections.Generic;
using System.Web;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.Portal.PortletFramework;
using SenseNet.Portal.UI.PortletFramework;
using System.Web.UI.WebControls;
using SenseNet.Portal.UI.ContentListViews;
using SenseNet.Portal.UI.ContentListViews.Handlers;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Diagnostics;
using System.Web.UI;

namespace SenseNet.Portal.Portlets
{
    public class FieldDeletePortlet : ContextBoundPortlet, IContentProvider
    {
        private FieldSettingContent FieldSettingNode { get; set; }

        public FieldDeletePortlet()
        {
            this.Name = "Field delete";
            this.Description = "This portlet allows a field to be deleted (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.System);
        }

        private Button _deleteButton;
        private Button DeleteButton
        {
            get
            {
                if (_deleteButton == null && this.Controls.Count > 0)
                {
                    _deleteButton = this.Controls[0].FindControl("btnDelete") as Button;

                    if (_deleteButton != null)
                        _deleteButton.Click += DeleteButton_Click;
                }

                return _deleteButton;
            }
        }

        private Button _cancelButton;
        private Button CancelButton
        {
            get
            {
                if (_cancelButton == null && this.Controls.Count > 0)
                {
                    _cancelButton = this.Controls[0].FindControl("btnCancel") as Button;

                    if (_cancelButton != null)
                        _cancelButton.Click += CancelButton_Click;
                }

                return _cancelButton;
            }
        }

        private Label _labelFieldName;
        private Label LabelFieldName
        {
            get
            {
                if (_labelFieldName == null && this.Controls.Count > 0)
                {
                    _labelFieldName = this.Controls[0].FindControl("lblFieldName") as Label;
                }

                return _labelFieldName;
            }
        }
        
        private string FieldName
        {
            get
            {
                var fn = HttpContext.Current.Request.Params["FieldName"];

                if (!string.IsNullOrEmpty(fn) && !fn.StartsWith("#"))
                    fn = string.Concat("#", fn);

                return fn;
            }
        }

        #region IContentProvider Members

        string IContentProvider.ContentTypeName
        {
            get; set;
        }

        string IContentProvider.ContentName
        {
            get
            {
                var ctn = FieldName;
                
                return string.IsNullOrEmpty(ctn) ? null : ctn;
            } 
            set { }
        }

        #endregion

        protected override void CreateChildControls()
        {
            if (Cacheable && CanCache && IsInCache)
                return;

            Controls.Clear();

            var contentList = GetContextNode() as ContentList;
            if (contentList == null)
                return;

            var fieldName = FieldName;
            if (string.IsNullOrEmpty(fieldName))
                return;

            foreach (FieldSettingContent fieldSetting in contentList.FieldSettingContents)
            {
                if (fieldSetting.FieldSetting.Name.CompareTo(fieldName) != 0)
                    continue;

                FieldSettingNode = fieldSetting;
                break;
            }

            if (FieldSettingNode == null)
                return;

            if (this.Controls.Count == 0)
            {
                var c = Page.LoadControl("/Root/System/SystemPlugins/ListView/DeleteField.ascx");
                if (c != null)
                {
                    this.Controls.Add(c);

                    if (LabelFieldName != null)
                        LabelFieldName.Text = FieldSettingNode.FieldSetting.DisplayName;

                    //init: add event handlers...
                    var db = this.DeleteButton;
                    var cb = this.CancelButton;
                }
            }

            ChildControlsCreated = true;
        }

        void DeleteButton_Click(object sender, EventArgs e)
        {
            foreach (var view in ViewManager.GetViewsForContainer(this.FieldSettingNode.ContentList))
            {
                var iv = view as IView;

                if (iv == null) 
                    continue;

                iv.RemoveColumn(this.FieldSettingNode.FieldSetting.FullName);
                ((SenseNet.Portal.UI.ContentListViews.Handlers.ViewBase) iv).Save();
            }

            //TEMP: if this is a reference field, remove all the values before deleting
            //The problem with this method is that it changes the 'modified by' and 'modified on'
            //values of _all_ the contents...
            if (FieldSettingNode.FieldSetting is ReferenceFieldSetting)
            {
                var contentList = GetContextNode() as ContentList;
                if (contentList != null)
                {
                    try
                    {
                        using (new SystemAccount())
                        {
                            var fn = contentList.GetPropertySingleId(this.FieldName);
                            var pt = contentList.PropertyTypes[fn];
                            var nq = new NodeQuery();
                            nq.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith, contentList.Path + RepositoryPath.PathSeparator));
                            //TODO: check while this doesn't work
                            //nq.Add(new ReferenceExpression(pt));

                            foreach (var node in nq.Execute().Nodes)
                            {
                                if (!node.HasProperty(fn))
                                    continue;

                                node.ClearReference(fn);
                                node.Save();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteException(ex);
                    }
                }
            }

            try
            {
                this.FieldSettingNode.Delete();
                CallDone();
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                this.Controls.Add(new LiteralControl(ex.ToString()));
            }
        }

        void CancelButton_Click(object sender, EventArgs e)
        {
            CallDone();
        }
    }
}
