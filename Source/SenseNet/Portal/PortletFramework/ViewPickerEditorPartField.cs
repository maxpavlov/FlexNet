using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using SenseNet.ContentRepository.i18n;

namespace SenseNet.Portal.UI.PortletFramework
{
    public class ViewPickerEditorPartField : ContentPickerEditorPartField
    {
        protected override void RenderFooter(HtmlTextWriter writer)
        {
            switch (this.ContentPickerOptions.ViewType)
            {
                case PortletBase.PortletViewType.Ascx:
                    WriteAvailableViewType("ascx", writer);
                    break;
                case PortletBase.PortletViewType.Xslt:
                    WriteAvailableViewType("xslt", writer);
                    break;
                case PortletBase.PortletViewType.All:
                    WriteAvailableViewTypes(new[] { "ascx", "xslt" }, writer);
                    break;
            }
        }

        private static void WriteAvailableViewType(string viewType, HtmlTextWriter writer)
        {
            WriteAvailableViewTypes(new[] { viewType }, writer);
        }

        private static void WriteAvailableViewTypes(IEnumerable<string> viewTypes, HtmlTextWriter writer)
        {
            writer.Write("<br/><span class='viewtypes'>" + SenseNetResourceManager.Current.GetString("PortletFramework", "AvailableViewType"));
            writer.Write(string.Join(", ", viewTypes));
            writer.Write("</span>");
        }

        protected override void OnPreRender(EventArgs e)
        {
            var xmlFieldsScript = string.Format(
                @"window.showxmlfields=function(){{var path=$('#{0}').val();var xslt=path.indexOf('.xslt',path.length-5)!=-1;$('.sn-editorpart-FieldSerializationOption').toggle(xslt);$('.sn-editorpart-FieldNamesSerializationOption').toggle(xslt);$('.sn-editorpart-ActionSerializationOption').toggle(xslt);showCustomXmlFields($('.sn-editorpart-FieldSerializationOption select').val() == 'Custom' && xslt);}}; showxmlfields(); $('#{0}').live('keyup', function() {{ showxmlfields(); }});",
                this.ClientID);

            var customFieldsScript = @"window.showCustomXmlFields = function(show) {show ? $('.sn-editorpart-FieldNamesSerializationOption').show() : $('.sn-editorpart-FieldNamesSerializationOption').hide();};
                    showCustomXmlFields($('.sn-editorpart-FieldSerializationOption select').val() == 'Custom');
                    $('.sn-editorpart-FieldSerializationOption select').change(function() { showCustomXmlFields($(this).val() == 'Custom'); });";

            UITools.RegisterStartupScript("customxmlfields", customFieldsScript, this.Page);
            UITools.RegisterStartupScript("hidexmlfields", xmlFieldsScript, this.Page);

            base.OnPreRender(e);
        }

        protected override string CustomCallback()
        {
            return "showxmlfields();";
        }
    }
}
