using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using System.Globalization;

[assembly: WebResource("SenseNet.Portal.UI.Controls.SNDialog.js", "application/x-javascript")]
[assembly: TagPrefix("SenseNet.Portal.UI.Controls", "sn")]
namespace SenseNet.Portal.UI.Controls
{
    /// <summary>
    /// Class definition for Dialog.
    /// </summary>
    [ToolboxData("<{0}:SNDialog ID=\"SNDialog1\" runat=server></{0}:SNDialog>"),
    ParseChildren(false),
    Designer(typeof(SNDialogDesigner))]
    public class SNDialog : CompositeControl, IScriptControl
    {

        // Properties /////////////////////////////////////////////////////////////
        public string HeaderText { get; set; }
        public string DialogType { get; set; }
        public bool Modal { get; set; }

        public new int Width { get; set; }
        public new int Height { get; set; }

        // Events /////////////////////////////////////////////////////////////////
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            var currScriptManager = ScriptManager.GetCurrent(Page);
            currScriptManager.RegisterScriptControl(this);
            currScriptManager.RegisterScriptDescriptors(this);
        }
        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "snDialog");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.AddAttribute(HtmlTextWriterAttribute.Id, "winheader");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "x-window-header");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.Write(GetHeader());
            writer.RenderEndTag();


            writer.AddAttribute(HtmlTextWriterAttribute.Class, "x-window-body");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            //var sr = new StringBuilder();

            //sr.Append("<div id=\"" + this.ClientID + "\" class=\"snDialog\" name=\"" + this.UniqueID + "\" style=\"visibility: hidden;\">");
            ////sr.Append(String.Format(CultureInfo.InvariantCulture,@"<div class=""x-dlg-hd"">{0}</div>", GetHeader()));
            //sr.Append(String.Format(CultureInfo.InvariantCulture, @"<div id=""winheader"" class=""x-window-header"">{0}</div>", GetHeader()));
            //sr.Append(@"<div class=""x-window-body"">");
            //writer.Write(sr.ToString());
        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {
            writer.RenderEndTag();
            writer.RenderEndTag();
            //StringBuilder sr = new StringBuilder();
            //sr.Append("</div>");    // close x-dlg-bd
            //sr.Append("</div>");    // close container
            //writer.Write(sr.ToString());
        }
        protected virtual IEnumerable<ScriptReference> GetScriptReferences()
        {
            //ScriptReference reference = new ScriptReference();
            //reference.Path = this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SenseNet.Portal.UI.Controls.SNDialog.js");
            //return new ScriptReference[] { reference };
            return new ScriptReference[] { };
        }
        protected virtual IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            /*ScriptControlDescriptor descriptor = new ScriptControlDescriptor("SenseNet.Portal.UI.Controls.SNDialog", this.ClientID);
            //
            // Add properties here
            //
            descriptor.AddProperty("headerText", this.HeaderText);
            if (this.Modal)
                descriptor.AddProperty("modal", "true");
            else
                descriptor.AddProperty("modal", "false");

            descriptor.AddProperty("width", this.Width);
            descriptor.AddProperty("height", this.Height);*/

            //return new ScriptDescriptor[] { descriptor };
            return new ScriptDescriptor[] { };
        }
        IEnumerable<ScriptReference> IScriptControl.GetScriptReferences()
        {
            return GetScriptReferences();
        }
        IEnumerable<ScriptDescriptor> IScriptControl.GetScriptDescriptors()
        {
            return GetScriptDescriptors();
        }


        public void Show()
        {
            UITools.RegisterStartupScript(
                string.Concat("DialogShow", this.ClientID),
                String.Format(@"SN.Controls.Dialog.show('{0}','{1}', {2}, {3});", this.ClientID, this.HeaderText, this.Width, this.Height),
                this.Page
                );

        }
        public void Hide()
        {
			UITools.RegisterStartupScript(
                string.Concat("DialogClose", this.ClientID),
                String.Format(@"SN.Controls.Dialog.close('{0}');", this.ClientID),
                this.Page);
        }
        private string GetHeader()
        {
            return this.HeaderText ?? this.ClientID.ToString();
        }

    }
    /// <summary>
    /// Designer class for Dialog.
    /// </summary>
    public class SNDialogDesigner : ContainerControlDesigner
    {
    }

}