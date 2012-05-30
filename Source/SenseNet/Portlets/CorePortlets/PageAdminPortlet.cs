using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.UI;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;
using SenseNet.Portal.PortletFramework;
using System.Xml.Xsl;
using System.Xml;
using System.Web.UI.WebControls;
using System.Web;
using System.IO;
using snc = SenseNet.ContentRepository;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Storage.Schema;
using System.Web.UI;

namespace SenseNet.Portal.Portlets
{
    public class PageAdminPortlet : ContextBoundPortlet
    {
        Page pageNode;
        XmlDocument pageXml;

        bool error = false;

        Label lblError;
        TextBox txtXml;
        Button btnSave;
        Button btnCancel;
        Button btnExport;
        Button btnImport;

        #region PrivateMembers

        private void environmentSetup()
        {
            var node = GetContextNode();
            pageNode = node as Page;
        }

        private void loadXml()
        {
            if ((pageNode != null) && (pageNode.Path != PortalContext.Current.Page.Path))
                pageXml = pageNode.GetPersonalizationXml(HttpContext.Current);
        }

        private void saveXml()
        {
            string error = String.Empty;
            XmlDocument modifiedXml = new XmlDocument();
            modifiedXml.LoadXml(txtXml.Text);
            pageNode.SetPersonalizationFromXml(HttpContext.Current, modifiedXml, out error);
        }

        private void redirectToBackUrl()
        {
            var backUrl = PortalContext.Current.BackUrl;
            if (String.IsNullOrEmpty(backUrl))
                backUrl = pageNode.Path;
            Logger.WriteVerbose("PageAdminPortlet: redirect to: " + backUrl);
            Context.Response.Redirect(backUrl);
        }

        private void exportToFile()
        {
            string fileName = "portlets.xml";

            snc.File newPortletInfoFile = null;
            BinaryData newPortletInfoBinary = null;

            var portletContentPath = RepositoryPath.Combine(pageNode.Path, fileName);
            if (NodeHead.Get(portletContentPath) == null)
            {
                newPortletInfoFile = new snc.File(pageNode);
                newPortletInfoBinary = new BinaryData();
            }
            else
            {
                newPortletInfoFile = Node.Load<snc.File>(portletContentPath);
                newPortletInfoBinary = newPortletInfoFile.Binary;
            }

            newPortletInfoFile.Name = fileName;
            newPortletInfoFile.Hidden = true;

            MemoryStream stream = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                if (pageXml == null)
                    loadXml();
                pageXml.WriteTo(writer);
                writer.Flush();
                newPortletInfoBinary.SetStream(stream);
                newPortletInfoFile.Binary = newPortletInfoBinary;
                newPortletInfoFile.Save();
            }
        }

        private void importFromFile()
        {
            string error = String.Empty;

            string fileName = "portlets.xml";
            var portletContentPath = RepositoryPath.Combine(pageNode.Path, fileName);

            snc.File xmlFile = Node.LoadNode(portletContentPath) as SenseNet.ContentRepository.File;

            if (xmlFile != null)
            {
                try
                {
                    Stream binstream = xmlFile.Binary.GetStream();

                    XmlDocument newXml = new XmlDocument();
                    using (XmlReader reader = XmlReader.Create(binstream))
                    {
                        newXml.Load(reader);
                    }
                    pageNode.SetPersonalizationFromXml(HttpContext.Current, newXml, out error);
                }
                catch (Exception e)
                {
                    Logger.WriteException(e);
                }
            }
        }

        private void writeXmlIndented(XmlDocument xml, StringWriter output)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = Encoding.Unicode;
            using (XmlWriter writer = XmlWriter.Create(output, settings))
            {
                xml.WriteContentTo(writer);
            }
        }

        #endregion

        public PageAdminPortlet()
        {
            this.Name = "Page administration";
            this.Description = "This portlet manages Portlets on a Page other than the current one (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.Portal);
        }

        protected override void OnInit(EventArgs e)
        {
            if (this.HiddenPropertyCategories == null)
                this.HiddenPropertyCategories = new List<string>();
            this.HiddenPropertyCategories.Add("Cache"); // this is an administrative portlet we don't need to use Cache functionality.

            lblError = new Label();
            txtXml = new TextBox();
            btnSave = new Button();
            btnCancel = new Button();
            btnImport = new Button();
            btnExport = new Button();

            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            lblError.Text = "This portlet operates only on Contents of the Sense/Net Page type, and doesn't work on the Page it is invoked from.";

            environmentSetup();

            if (pageNode != null)
            {
                if (!pageNode.Security.HasPermission(PermissionType.Save))
                {
                    lblError.Text = "Operation not permitted.";
                    error = true;
                }
                else if (!Page.IsPostBack)
                {
                    loadXml();

                    if (pageXml != null)
                    {
                        using (StringWriter output = new StringWriter())
                        {
                            writeXmlIndented(pageXml, output);
                            txtXml.Text = output.ToString();
                        }
                    }
                    else
                        error = true;
                }
                else if (pageNode.Path == PortalContext.Current.Page.Path)
                    error = true;
            }
            else
                error = true;

        }

        protected override void CreateChildControls()
        {
            Controls.Clear();

            Controls.Add(lblError);

            txtXml.TextMode = TextBoxMode.MultiLine;
            txtXml.Columns = 80;
            txtXml.Rows = 20;
            Controls.Add(txtXml);

            btnCancel.Click += new EventHandler(btnCancel_Click);
            btnCancel.Text = "Cancel";
            Controls.Add(btnCancel);

            btnSave.Click += new EventHandler(btnSave_Click);
            btnSave.Text = "Save";
            Controls.Add(btnSave);

            btnExport.Click += new EventHandler(btnExport_Click);
            btnExport.Text = "Export";
            btnExport.ToolTip = "Export to file";
            Controls.Add(btnExport);

            btnImport.Click += new EventHandler(btnImport_Click);
            btnImport.Text = "Import";
            btnImport.ToolTip = "Import from file";
            Controls.Add(btnImport);

            ChildControlsCreated = true;
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            if (error)
                lblError.RenderControl(writer);
            else
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                btnExport.RenderControl(writer);
                btnImport.RenderControl(writer);
                writer.RenderEndTag();
                txtXml.RenderControl(writer);
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                btnCancel.RenderControl(writer);
                btnSave.RenderControl(writer);
                writer.RenderEndTag();
            }
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            redirectToBackUrl();
        }

        void btnSave_Click(object sender, EventArgs e)
        {
            saveXml();
            redirectToBackUrl();
        }

        void btnImport_Click(object sender, EventArgs e)
        {
            importFromFile();
            redirectToBackUrl();
        }

        void btnExport_Click(object sender, EventArgs e)
        {
            exportToFile();
            redirectToBackUrl();
        }

    }
}