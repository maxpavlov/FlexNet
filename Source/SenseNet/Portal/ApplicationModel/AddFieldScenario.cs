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
            var act3 = app.CreateAction(context, backUrl, new { ContentTypeName = "ChoiceFieldSetting" });
            var act4 = app.CreateAction(context, backUrl, new { ContentTypeName = "NumberFieldSetting" });
            var act5 = app.CreateAction(context, backUrl, new { ContentTypeName = "IntegerFieldSetting" });
            var act6 = app.CreateAction(context, backUrl, new { ContentTypeName = "CurrencyFieldSetting" });
            var act7 = app.CreateAction(context, backUrl, new { ContentTypeName = "DateTimeFieldSetting" });
            var act8 = app.CreateAction(context, backUrl, new { ContentTypeName = "ReferenceFieldSetting" });
            var act9 = app.CreateAction(context, backUrl, new { ContentTypeName = "YesNoFieldSetting" });
            var act10 = app.CreateAction(context, backUrl, new { ContentTypeName = "HyperLinkFieldSetting" });

            act1.Index = 0;
            act2.Index = 1;
            act3.Index = 2;
            act4.Index = 3;
            act5.Index = 4;
            act6.Index = 5;
            act7.Index = 6;
            act8.Index = 7;
            act9.Index = 8;
            act10.Index = 9;

            act1.Text = "Single line of text";
            act2.Text = "Multiple line of text";
            act3.Text = "Choice";
            act4.Text = "Number";
            act5.Text = "Integer";
            act6.Text = "Currency";
            act7.Text = "Date and Time";
            act8.Text = "Reference";
            act9.Text = "Yes/No";
            act10.Text = "Hyperlink or Picture field";

            act1.Icon = "addshorttextfield";
            act2.Icon = "addlongtextfield";
            act3.Icon = "addchoicefield";
            act4.Icon = "addnumberfield";
            act5.Icon = "addnumberfield";
            act6.Icon = "addcurrencyfield";
            act7.Icon = "adddatetimefield";
            act8.Icon = "addreferencefield";
            act9.Icon = "addyesnofield";
            act10.Icon = "addhyperlinkfield";
            
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

        public override IComparer<ActionBase> GetActionComparer()
        {
            return null;
        }
    }
}
