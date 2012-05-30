using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using SenseNet.ContentRepository.Workspaces;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:WikiEditor ID=\"WikiEditort1\" runat=server></{0}:WikiEditor>")]
    public class WikiEditor : RichText
    {
        //=========================================================================== Overrides

        public override object GetData()
        {
            return this.ControlMode == FieldControlControlMode.Edit ? 
                WikiTools.ConvertWikilinksToHtml(base.GetData() as string, this.Content.ContentHandler.Parent) : 
                base.GetData();
        }

        public override void SetData(object data)
        {
            base.SetData(this.ControlMode == FieldControlControlMode.Edit
                             ? WikiTools.ConvertHtmlToWikilinks(data as string)
                             : data);
        }

        protected override void InitImagePickerParams()
        {
            var ws = Workspace.GetWorkspaceForNode(this.ContentHandler);
            var imagesPath = RepositoryPath.Combine(ws.Path, "Images");
            var script = string.Format("SN.tinymceimagepickerparams = {{ TreeRoots:['{0}','/Root'] }};", imagesPath);
            UITools.RegisterStartupScript("tinymceimagepickerparams", script, this.Page);
        }
    }
}
