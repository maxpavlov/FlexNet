using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.UI.Controls
{
    public class ContentTypeInstallerControl : UserControl
    {
        protected System.Web.UI.WebControls.Panel pnlInstall;
        protected System.Web.UI.WebControls.Panel pnlSuccess;

        protected ContentView ContentView { get; set; }
        protected Content Content { get; set; }


        protected void InstallerButton_Click(object sender, EventArgs e)
        {
            this.ContentView = this.Parent as ContentView;
            this.Content = this.ContentView.Content;

            // install ctd
            this.ContentView.UpdateContent();
            var result = ValidateAndInstallContentType();
            if (result)
            {
                var editorControl = this.ContentView.FindControlRecursive("Binary1") as FieldControl;
                editorControl.Visible = false;
                pnlInstall.Visible = false;
                pnlSuccess.Visible = true;
            }
            else
            {
                if (this.ContentView.ContentException == null)
                    this.ContentView.ContentException = new InvalidOperationException("An error occurred during installation of CTD.");
            }
        }

        private bool ValidateAndInstallContentType()
        {
            var isUserInputValid = this.ContentView.IsUserInputValid;
            var isValid = this.Content.IsValid;
            if (isUserInputValid && isValid)
                return InstallContentTypeInternal();
            return false;
        }

        private bool InstallContentTypeInternal()
        {
            try
            {
                var ctd = GetContentTypeFromField(this.Content);
                if (ctd == null)
                    return false;

                ContentTypeInstaller.InstallContentType(ctd);
            }
            catch (Exception exc) //logged
            {
                Logger.WriteException(exc);
                this.ContentView.ContentException = exc;
                return false;
            }
            return true;
        }
        private string GetContentTypeFromField(Content content)
        {
            var binaryData = this.Content.Fields["Binary"].GetData() as BinaryData;
            var stream = binaryData.GetStream();
            return SenseNet.ContentRepository.Tools.GetStreamString(stream);
        }
    }
}
