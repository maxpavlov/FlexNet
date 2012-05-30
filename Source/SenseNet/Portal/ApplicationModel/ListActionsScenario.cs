using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;

namespace SenseNet.ApplicationModel
{
    public class ListActionsScenario : GenericScenario
    {
        public override string Name
        {
            get
            {
                return "ListActions";
            }
        }

        protected override IEnumerable<ActionBase> CollectActions(Content context, string backUrl)
        {
            var actList = base.CollectActions(context, backUrl).ToList();

            //if (context == null)
            //    return actList;

            //if (TrashBin.IsInTrash(context.ContentHandler as GenericContent))
            //    return actList;

            //var actWd = ActionFramework.GetAction("ClientAction", context, backUrl, null) as ClientAction;

            //if (actWd != null)
            //{
            //    actWd.MethodName = "BrowseFolder";
            //    actWd.ParameterList = string.Format(@"""{0}""", context.Path);
            //    actWd.Text = "Open in Windows Explorer";
            //    actWd.Icon = new ActionIcon { IconName = "folder" };
            //    actList.Add(actWd);
            //}

            return actList;
        }
    }
}
