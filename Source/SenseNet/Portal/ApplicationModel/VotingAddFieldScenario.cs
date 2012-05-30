using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.Portal.Virtualization;

namespace SenseNet.ApplicationModel
{
    public class VotingAddFieldScenario : AddFieldScenario
    {
        public override string Name
        {
            get
            {
                return "VotingAddField";
            }
        }

        protected override IEnumerable<ActionBase> CollectActions(Content context, string backUrl)
        {
            var actList = new List<ActionBase>();

            if (context == null)
                return actList;

            var app = ApplicationStorage.Instance.GetApplication("AddField", context, PortalContext.Current.DeviceName);

            var act6 = app.CreateAction(context, backUrl, new { ContentTypeName = "YesNoFieldSetting" });
            var act9 = app.CreateAction(context, backUrl, new { ContentTypeName = "ChoiceFieldSetting" });

            act6.Text = "Yes/No field";
            act9.Text = "Choice field";

            act6.Icon = "addyesnofield";
            act9.Icon = "addchoicefield";

            actList.Add(act6);
            actList.Add(act9);

            return actList;
        }
    }
}
