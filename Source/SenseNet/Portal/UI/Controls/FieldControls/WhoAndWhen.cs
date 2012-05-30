using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Globalization;
using System.Web.UI;
using SenseNet.ContentRepository.Fields;

using SenseNet.ContentRepository;

namespace SenseNet.Portal.UI.Controls
{
    [Obsolete("Use separate field controls for the user and the date information. (eg. CreatedBy and CreationDate)")]
    [ToolboxData("<{0}:WhoAndWhen ID=\"WhoAndWhen1\" runat=server></{0}:WhoAndWhen>")]
    public class WhoAndWhen : FieldControl
    {
        public Label label1;
        public Label label2;

        public WhoAndWhen()
        {
            label1 = new Label();
            label2 = new Label();
        }

        public override void SetData(object data)
        {
            WhoAndWhenField.WhoAndWhenData wdata = (WhoAndWhenField.WhoAndWhenData)data;

            User who = wdata.Who;
            var name = who == null ? "[unknown]" : who.Name;
            var userName = who == null ? "" : who.Username;
            label2.Text = String.Concat(" by ", name, "<br/>(", userName, ")");
            DateTime when = wdata.When;

            CultureInfo ci = CultureInfo.CurrentUICulture;

            if (ci.IsNeutralCulture)
                ci = CultureInfo.CreateSpecificCulture(ci.Name);

            label1.Text = when.ToString(ci);
        }
        public override object GetData()
        {
            throw new NotImplementedException("@@@@");
            //return null;
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            label1.RenderControl(writer);
            writer.Write(HtmlTextWriter.SpaceChar);
            label2.RenderControl(writer);
        }
    }
}