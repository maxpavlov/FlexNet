using System;
using System.Collections.Generic;
using System.Linq;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.i18n;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Schema;
using SenseNet.Portal.Virtualization;

namespace SenseNet.ApplicationModel
{
    public class WorkspaceActionsScenario : GenericScenario
    {
        public override string Name
        {
            get
            {
                return "WorkspaceActions";
            }
        }

        protected override IEnumerable<ActionBase> CollectActions(Content context, string backUrl)
        {
            var actList = new List<ActionBase>();
            if (context == null)
                return actList;

            // Create other action
            var gc = context.ContentHandler as GenericContent;
            var contentTypes = gc == null ? new List<ContentType>() : gc.GetAllowedChildTypes().ToList();
            if (PortalContext.Current.ArbitraryContentTypeCreationAllowed || contentTypes.Count > 0)
            {
                var createOtherAction = ActionFramework.GetAction("Create", context, backUrl, null);
                if (createOtherAction != null)
                {
                    createOtherAction.Text = SenseNetResourceManager.Current.GetString("Portal", "CreateOtherActionText");
                    actList.Add(createOtherAction);
                }
            }

            // Open in Content Explorer action
            var exa = ActionFramework.GetAction("Explore", context, backUrl, null);
            if (exa != null)
            {
                var ea = new ExploreAction(context);
                ea.Initialize(context, backUrl, null, null);

                actList.Add(ea);
            }

            var app = ApplicationStorage.Instance.GetApplication("Add", context, PortalContext.Current.DeviceName);
            if (app != null)
            {
                new List<string> {"Workspace", "DocumentLibrary", "ItemList"}.ForEach(
                    delegate(String contentTypeName)
                        {
                            if (!contentTypes.Any(ct => ct.Name == contentTypeName))
                                return;

                            var cnt = ContentType.GetByName(contentTypeName);
                            var name = ContentTemplate.HasTemplate(contentTypeName) ? ContentTemplate.GetTemplate(contentTypeName).Path : cnt.Name;
                            var addNewAction = app.CreateAction(context, backUrl, new {ContentTypeName = name, backtarget = "newcontent" });
                            if (addNewAction != null)
                            {
                                addNewAction.Text = String.Concat( SenseNetResourceManager.Current.GetString("Portal", "AddNewActionPrefix"), cnt.DisplayName);
                                addNewAction.Icon = cnt.Icon;

                                actList.Add(addNewAction);
                            }
                        });
            }

            var notifAction = ActionFramework.GetAction("Notifications", Content.Create(User.Current as Node), backUrl, null);
            if (notifAction != null)
                actList.Add(notifAction);

            actList.AddRange(base.CollectActions(context, backUrl));

            return actList;
        }
    }
}
