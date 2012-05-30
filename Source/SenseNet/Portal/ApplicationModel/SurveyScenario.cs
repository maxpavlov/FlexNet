using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.Portal.Virtualization;

namespace SenseNet.ApplicationModel
{
    public class SurveyScenario : AddFieldScenario
    {
        public override string Name
        {
            get
            {
                return "Survey";
            }
        }

        protected override IEnumerable<ActionBase> CollectActions(Content context, string backUrl)
        {
            var actions = base.CollectActions(context, backUrl).ToList();
            
            if (context == null)
                return actions;

            var app = ApplicationStorage.Instance.GetApplication("AddField", context, PortalContext.Current.DeviceName);
            var act = app.CreateAction(context, backUrl, new { ContentTypeName = "PageBreakFieldSetting" });
            act.Text = "Page break";
            act.Icon = "newpage";

            actions.Add(act);

            return actions;
        }
    }
}