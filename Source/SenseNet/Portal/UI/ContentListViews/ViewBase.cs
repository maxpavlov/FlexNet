using System;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.Portal.UI.ContentListViews.FieldControls;
using SenseNet.Portal.UI.Controls;
using SenseNet.Search;

namespace SenseNet.Portal.UI.ContentListViews
{
    public abstract class ViewBase : System.Web.UI.UserControl
    {
        #region properties

        private SenseNetDataSource _viewDataSource;
        protected SenseNetDataSource ViewDataSource
        {
            get
            {
                if (_viewDataSource == null)
                    _viewDataSource = this.FindControl("ViewDataSource") as SenseNetDataSource;

                return _viewDataSource;
            }
        }

        private ViewFrame _ownerFrame;
        public ViewFrame OwnerFrame
        {
            get
            {
                if (_ownerFrame == null)
                    _ownerFrame = ViewFrame.GetContainingViewFrame(this);

                return _ownerFrame;
            }
        }

        private Handlers.ViewBase _viewDefinition;
        public Handlers.ViewBase ViewDefinition
        {
            get { return _viewDefinition ?? (_viewDefinition = Node.Load<Handlers.ViewBase>(this.AppRelativeVirtualPath.Substring(1))); }
        }

        private Node _contextNode;
        public Node ContextNode
        {
            get
            {
                if (_contextNode == null)
                    _contextNode = OwnerFrame.ContextNode;

                return _contextNode;
            }

        }

        #endregion

        #region view_queries

        protected virtual NodeQuery GetFilter()
        {
            NodeQuery filter = null;
            if (!string.IsNullOrEmpty(ViewDefinition.FilterXml))
            {
                if (ViewDefinition.FilterIsContentQuery)
                    return null;
                
                filter = NodeQuery.Parse(Query.GetNodeQueryXml(ViewDefinition.FilterXml));
            }

            return filter;
        }

        #endregion

        #region aspnet_members

        protected override void OnLoad(EventArgs e)
        {
            ViewDataSource.ContentPath = ContextNode.Path;

            if (ViewDefinition != null)
            {
                if (ViewDefinition.FilterIsContentQuery)
                {
                    if (this.OwnerFrame == null || this.OwnerFrame.OwnerPortlet == null)
                        ViewDataSource.Query = ViewDefinition.FilterXml;
                    else
                        ViewDataSource.Query = this.OwnerFrame.OwnerPortlet.ReplaceTemplates(ViewDefinition.FilterXml);
                    
                }
                else
                {
                    ViewDataSource.QueryFilter = GetFilter();
                }
            }

            if (ViewDataSource.Settings == null)
                ViewDataSource.Settings = new QuerySettings();

            if (ViewDefinition != null)
            {
                switch (ViewDefinition.EnableAutofilters)
                {
                    case FilterStatus.Enabled:
                        ViewDataSource.Settings.EnableAutofilters = true;
                        break;
                    case FilterStatus.Disabled:
                        ViewDataSource.Settings.EnableAutofilters = false;
                        break;
                    default:
                        break;
                }

                switch (ViewDefinition.EnableLifespanFilter)
                {
                    case FilterStatus.Enabled:
                        ViewDataSource.Settings.EnableLifespanFilter = true;
                        break;
                    case FilterStatus.Disabled:
                        ViewDataSource.Settings.EnableLifespanFilter = false;
                        break;
                    default:
                        break;
                }

                if (ViewDefinition.QueryTop > 0)
                    ViewDataSource.Settings.Top = ViewDefinition.QueryTop;

                if (ViewDefinition.QuerySkip > 0)
                    ViewDataSource.Settings.Skip = ViewDefinition.QuerySkip;
            }

            base.OnLoad(e);
        }

        #endregion

        #region abstract_members

        protected abstract IEnumerable<string> GetFieldList();

        #endregion
    }
}
