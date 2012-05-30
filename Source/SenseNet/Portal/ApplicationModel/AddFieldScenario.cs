using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.Portal.Virtualization;

namespace SenseNet.ApplicationModel
{
    public class AddFieldScenario : GenericScenario
    {
        public override string Name
        {
            get
            {
                return "AddField";
            }
        }

        protected override IEnumerable<ActionBase> CollectActions(Content context, string backUrl)
        {
            var actList = new List<ActionBase>();

            if (context == null)
                return actList;

            var app = ApplicationStorage.Instance.GetApplication("AddField", context, PortalContext.Current.DeviceName);

            var act1 = app.CreateAction(context, backUrl, new {ContentTypeName = "ShortTextFieldSetting"});
            var act2 = app.CreateAction(context, backUrl, new { ContentTypeName = "LongTextFieldSetting" });
            var act3 = app.CreateAction(context, backUrl, new { ContentTypeName = "NumberFieldSetting" });
            var act4 = app.CreateAction(context, backUrl, new { ContentTypeName = "IntegerFieldSetting" });
            var act5 = app.CreateAction(context, backUrl, new { ContentTypeName = "CurrencyFieldSetting" });
            var act6 = app.CreateAction(context, backUrl, new { ContentTypeName = "YesNoFieldSetting" });
            var act7 = app.CreateAction(context, backUrl, new { ContentTypeName = "DateTimeFieldSetting" });
            var act8 = app.CreateAction(context, backUrl, new { ContentTypeName = "HyperLinkFieldSetting" });
            var act9 = app.CreateAction(context, backUrl, new { ContentTypeName = "ChoiceFieldSetting" });
            var act10 = app.CreateAction(context, backUrl, new { ContentTypeName = "ReferenceFieldSetting" });

            act1.Text = "Shorttext field";
            act2.Text = "Longtext field";
            act3.Text = "Number field";
            act4.Text = "Integer field";
            act5.Text = "Currency field";
            act6.Text = "Yes/No field";
            act7.Text = "DateTime field";
            act8.Text = "Hyperlink field";
            act9.Text = "Choice field";
            act10.Text = "Reference field";

            act1.Icon = "addshorttextfield";
            act2.Icon = "addlongtextfield";
            act3.Icon = "addnumberfield";
            act4.Icon = "addnumberfield";
            act5.Icon = "addcurrencyfield";
            act6.Icon = "addyesnofield";
            act7.Icon = "adddatetimefield";
            act8.Icon = "addhyperlinkfield";
            act9.Icon = "addchoicefield";
            act10.Icon = "addreferencefield";
            
            actList.Add(act1);
            actList.Add(act2);
            actList.Add(act3);
            actList.Add(act4);
            actList.Add(act5);
            actList.Add(act6);
            actList.Add(act7);
            actList.Add(act8);
            actList.Add(act9);
            actList.Add(act10);

            return actList;
        }
    }
}
