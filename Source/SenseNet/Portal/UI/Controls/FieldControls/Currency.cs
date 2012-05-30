using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using SenseNet.ContentRepository.Fields;
using SenseNet.Diagnostics;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:Currency ID=\"Currency1\" runat=server></{0}:Currency>")]
    public class Currency : Number
    {
        protected string CurrencyControlID = "LabelForCurrency";

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate) 
                return;

            var lblCurrency = this.FindControlRecursive(CurrencyControlID) as Label;
            if (lblCurrency != null)
            {
                lblCurrency.Text = GetCurrencySign();
            }
        }

        protected override void RenderSimple(HtmlTextWriter writer)
        {
            RenderCurrencySign(writer);

            base.RenderSimple(writer);
        }

        protected override void RenderEditor(HtmlTextWriter writer)
        {
            RenderCurrencySign(writer);

            base.RenderEditor(writer);
        }

        private void RenderCurrencySign(HtmlTextWriter writer)
        {
            var cs = GetCurrencySign();

            if (!string.IsNullOrEmpty(cs))
                writer.Write(cs + " ");
        }

        private string GetCurrencySign()
        {
            var cfs = this.Field.FieldSetting as CurrencyFieldSetting;

            if (cfs == null)
                return string.Empty;

            try
            {
                return CurrencyFieldSetting.CurrencyTypes[cfs.Format];
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }

            return string.Empty;
        }
    }
}
