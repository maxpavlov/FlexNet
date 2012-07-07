using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.UI.ContentListViews;

namespace SenseNet.ApplicationModel
{
    public class ViewsScenario : GenericScenario
    {
        public override string Name
        {
            get
            {
                return "Views";
            }
        }

        public string PortletId { get; set; }
        public string DefaultView { get; set; }
        private List<int> _collectedViews;

        protected override IEnumerable<ActionBase> CollectActions(Content context, string backUrl)
        {
            var actList = new List<ActionBase>();
            _collectedViews = new List<int>();

            if (context == null)
                return actList;

            var list = context.ContentHandler as ContentList;
            var hash = ViewFrame.GetHashCode(context.Path, PortletId);
            var selectedView = string.IsNullOrEmpty(hash) ? string.Empty : ViewFrame.GetSelectedView(hash) ?? (DefaultView ?? string.Empty);
            if (string.IsNullOrEmpty(selectedView) && list != null)
                selectedView = list.DefaultView ?? string.Empty;

            if (!string.IsNullOrEmpty(DefaultView) && DefaultView.StartsWith("/Root/"))
            {
                var view = Node.Load<File>(DefaultView);
                if (view != null && !_collectedViews.Contains(view.Id))
                {
                    _collectedViews.Add(view.Id);
                    var act = GetServiceAction(context, view, true, PortletId, selectedView, backUrl);
                    if (act != null)
                        actList.Add(act);
                }
            }

            foreach (var view in ViewManager.GetViewsForContainer(context.ContentHandler))
            {
                if (_collectedViews.Contains(view.Id))
                    continue;

                _collectedViews.Add(view.Id);

                //add local views with only name
                var act = GetServiceAction(context, view, false, PortletId, selectedView, backUrl);
                if (act != null)
                    actList.Add(act);
            }

            var contentList = ContentList.GetContentListByParentWalk(context.ContentHandler);
            if (contentList != null)
            {
                foreach (var view in contentList.AvailableViews)
                {
                    if (_collectedViews.Contains(view.Id))
                        continue;

                    _collectedViews.Add(view.Id);

                    //add global views with full path
                    var act = GetServiceAction(context, view, true, PortletId, selectedView, backUrl);
                    if (act != null)
                        actList.Add(act);
                }
            }

            return actList;
        }

        public override void Initialize(Dictionary<string, object> parameters)
        {
            base.Initialize(parameters);

            if (parameters == null)
                return;

            if (parameters.ContainsKey("PortletID"))
                PortletId = parameters["PortletID"] as string;
            else
                PortletId = string.Empty;

            if (parameters.ContainsKey("DefaultView"))
                DefaultView = parameters["DefaultView"] as string ?? string.Empty;
            else
                DefaultView = string.Empty;
        }

        private static ServiceAction GetServiceAction(Content context, Node view, bool addFullPath, string portletId, string selectedView, string backUrl)
        {
            //create app-less action for view selection
            var act = ActionFramework.GetAction("ServiceAction", context, backUrl,
                new
                {
                    path = context.Path,
                    uiContextId = portletId ?? string.Empty,
                    view = addFullPath ? view.Path : view.Name
                }) as ServiceAction;

            if (act == null)
                return null;

            var gc = view as GenericContent;
            var icon = gc != null ? gc.Icon : string.Empty;
            if (string.IsNullOrEmpty(icon))
                icon = "views";

            act.Name = "ServiceAction";
            act.ServiceName = "ContentListViewHelper.mvc";
            act.MethodName = "SetView";
            act.Text = view.DisplayName;
            act.Icon = icon;

            if (selectedView.CompareTo(view.Name) == 0 || selectedView.CompareTo(view.Path) == 0)
            {
                act.CssClass = (act.CssClass + " sn-actionlink-selectedview").TrimStart(new[] { ' ' });
                act.Forbidden = true;
            }
            return act;
        }
    }
}
