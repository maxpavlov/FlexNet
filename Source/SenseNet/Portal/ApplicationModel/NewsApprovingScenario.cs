using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ApplicationModel;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.ApplicationModel
{
    public class NewsApprovingScenario : GenericScenario
    {
        public override string Name
        {
            get
            {
                return "NewsApproving";
            }
        }

        protected override IEnumerable<ActionBase> CollectActions(SenseNet.ContentRepository.Content context, string backUrl)
        {
            var actList = new List<ActionBase>();

            if (context == null)
                return actList;

            actList.AddRange(base.CollectActions(context, backUrl));

            var appApprove = ApplicationStorage.Instance.GetApplication("Approve", context, PortalContext.Current.DeviceName);
            var appEdit = ApplicationStorage.Instance.GetApplication("Edit", context, PortalContext.Current.DeviceName);
            var appDelete = ApplicationStorage.Instance.GetApplication("Delete", context, PortalContext.Current.DeviceName);

            var action = appApprove.CreateAction(context, backUrl, null);
            action.Text = "Approve";
            actList.Add(action);

            action = appEdit.CreateAction(context, backUrl, null);
            action.Text = "Edit";
            actList.Add(action);

            action = appDelete.CreateAction(context, backUrl, null);
            action.Text = "Delete";
            actList.Add(action);

            return actList;
        }
    }
}
