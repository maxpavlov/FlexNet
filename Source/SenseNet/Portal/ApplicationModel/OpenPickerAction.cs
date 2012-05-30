using System;

namespace SenseNet.ApplicationModel
{
    public class OpenPickerAction : ClientAction
    {
        protected virtual string DefaultPath
        {
            get
            {
                if (this.Content == null)
                    return null;

                var parent = this.Content.ContentHandler.Parent;
                if (parent != null)
                    return parent.Path;

                return null;
            }
        }

        protected virtual string MultiSelectMode
        {
            get { return "none"; }
        }

        protected virtual string TargetActionName
        {
            get { throw new NotImplementedException(); }
        }

        protected virtual string TargetParameterName
        {
            get { return "sourceids"; }
        }

        protected virtual string GetOpenContentPickerScript()
        {
            if (Content == null || this.Forbidden)
                return string.Empty;

            string script;
            if (this.DefaultPath != null)
                script = string.Format("if ($(this).hasClass('sn-disabled')) return false; SN.PickerApplication.open({{ MultiSelectMode: '{0}', callBack: {1}, DefaultPath: '{2}' }});",
                    MultiSelectMode, GetCallBackScript(), DefaultPath);
            else
                script = string.Format("if ($(this).hasClass('sn-disabled')) return false; SN.PickerApplication.open({{ MultiSelectMode: '{0}', callBack: {1}}});",
                    MultiSelectMode, GetCallBackScript());

            return script;
        }

        protected virtual string GetCallBackScript()
        {
            return string.Format(@"function(resultData) {{if (!resultData) return; var targetPath = resultData[0].Path; var idlist = {0}; var requestPath = targetPath + '?action={1}&{2}=' + idlist + '&back=' + escape(window.location.href); window.location = requestPath;}}", GetIdList(), TargetActionName, TargetParameterName);
        }

        protected virtual string GetIdList()
        {
            return (Content == null ? string.Empty : Content.Id.ToString());
        }

        protected string GetIdListMethod()
        {
            var parameters = GetParameteres();
            var portletId = parameters.ContainsKey("PortletClientID") ? parameters["PortletClientID"] : string.Empty;

            return string.Format("SN.ListGrid.getSelectedIds('{0}')", portletId);
        }

        public override string Callback
        {
            get
            {
                return this.Forbidden ? string.Empty : GetOpenContentPickerScript();
            }
            set
            {
                base.Callback = value;
            }
        } 
    }
}
