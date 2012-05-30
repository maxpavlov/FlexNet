using System;
using System.ComponentModel;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using System.Linq;
using SNC = SenseNet.ContentRepository;
using SenseNet.Portal.UI.PortletFramework;
using System.Collections.Generic;

namespace SenseNet.Portal.Portlets
{
    public class ImportExportPortlet : PortletBase
    {
        private static readonly string PortletInfoFileNameExtension = ".portlet";

        [Personalizable(true)]
        [WebBrowsable(true)]
        [WebDisplayName("Portlet mode")]
        [WebDescription("Determines whether to use import or export portlet layout. 'Both' renders controls for both scenario")]
        [DefaultValue(ImportExportPortletState.Both)]
        [WebCategory(EditorCategory.ImportExport, EditorCategory.ImportExport_Order)]
        public ImportExportPortletState PortletMode { get; set; }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        // Members ////////////////////////////////////////////////////////
        private UpdatePanel _masterUpdatePanel;
        private ImportExportControlFacade UIControls = InitUIControls();

        private static ImportExportControlFacade InitUIControls()
        {
            var uiControls = new ImportExportControlFacade();
            uiControls.Init();
            return uiControls;
        }

        public ImportExportPortlet()
        {
            this.Name = "Portlet import/export";
            this.Description =
                "This is a pilot project for exporting, importing webparts (a.k.a. portlets), using Sense/Net 6.0 repository";
            this.Category = new PortletCategory(PortletCategoryType.System);
        }

        // Events /////////////////////////////////////////////////////////
        protected override void CreateChildControls()
        {
            Controls.Clear();
            UIControls.ErrorMessage.Text = string.Empty;

            _masterUpdatePanel = new UpdatePanel();
            _masterUpdatePanel.ID = "ImportExportUpdatePanel";
            _masterUpdatePanel.UpdateMode = UpdatePanelUpdateMode.Conditional;

            _masterUpdatePanel.ContentTemplateContainer.Controls.Clear();

            switch (PortletMode)
            {
                case ImportExportPortletState.Import:
                    CreateImportControls();
                    SetImportPostBackButton();
                    break;
                case ImportExportPortletState.Export:
                    CreateExportControls();
                    break;
                default:
                    CreateImportControls();
                    SetImportPostBackButton();
                    CreateExportControls();
                    CreateToolbar();
                    break;
            }


            Controls.Add(_masterUpdatePanel);

            ChildControlsCreated = true;
        }

        protected void UIControls_ImportAllClick(object sender, EventArgs e)
        {
            var currentPage = SenseNet.Portal.Page.Current;
            var portletInfoFileList = GetAllPortletDescriptionFiles(currentPage.Path);
            if (portletInfoFileList.Count == 0)
            {
                UIControls.DisplayFeedback("Couldn't find any portlet description files.");
                return;
            }
            ImportAllPortlets(portletInfoFileList);
        }

        protected void UIControls_ExportAllClick(object sender, EventArgs e)
        {
            ExportAllPortlet();
        }

        protected void UIControls_ExportClick(object sender, EventArgs e)
        {
            ExportPortletToScreen();
        }

        protected void UIControls_ImportClick(object sender, EventArgs e)
        {
            ImportPortlet();
        }

        // Internals //////////////////////////////////////////////////////

        private void CreateToolbar()
        {
            UIControls.BuildToolbarButtons(_masterUpdatePanel.ContentTemplateContainer.Controls);
            UIControls.ExportAllClick += new EventHandler(UIControls_ExportAllClick);
            UIControls.ImportAllClick += new EventHandler(UIControls_ImportAllClick);
        }

        private void CreateExportControls()
        {
            UIControls.BuildExportControls(_masterUpdatePanel.ContentTemplateContainer.Controls, Page);
            UIControls.ExportClick += new EventHandler(UIControls_ExportClick);
        }

        private void CreateImportControls()
        {
            UIControls.BuildImportControls(_masterUpdatePanel.ContentTemplateContainer.Controls, Page);
            UIControls.ImportClick += new EventHandler(UIControls_ImportClick);
        }

        private void ImportPortlet()
        {
            UIControls.ErrorMessage.Text = string.Empty;
            var importXml = UIControls.ImportTextBox.Text;
            if (String.IsNullOrEmpty(importXml))
            {
                UIControls.DisplayErrorMessage("Give the import XML fragment!");
                return;
            }


            string errorMessage;
            ImportPortletInternal(importXml, out errorMessage, true, null);
        }

        private void ImportPortletInternal(string importXml, out string errorMessage, bool feedback, string zoneId)
        {
            if (importXml == null)
                throw new ArgumentNullException("importXml");

            var wpm = WebPartManager.GetCurrentWebPartManager(Page);
            using (var sr = new StringReader(importXml))
            {
                using (XmlTextReader xmlReader = new XmlTextReader(sr))
                {
                    var webPart = wpm.ImportWebPart(xmlReader, out errorMessage);
                    if (String.IsNullOrEmpty(errorMessage))
                    {
                        WebPartZoneBase selectedWebPartZone = null;
                        selectedWebPartZone = String.IsNullOrEmpty(zoneId)
                                                  ? GetSelectedZoneInstance()
                                                  : wpm.Zones[zoneId];

                        if (selectedWebPartZone != null)
                        {
                            wpm.AddWebPart(webPart, selectedWebPartZone, selectedWebPartZone.WebParts.Count);
                            if (feedback)
                                UIControls.DisplayFeedback(
                                    String.Format("Portlet has been imported successfully. <br />"));
                        }
                        else
                        {
                            UIControls.DisplayErrorMessage(
                                String.Format("There is no '{0}' zone on the current page. <br />",
                                              UIControls.ImportZoneList.SelectedValue));
                        }
                    }
                    else
                        UIControls.DisplayErrorMessage(errorMessage);
                }
            }
        }

        private void ExportPortletToScreen()
        {
            var selectedIndex = UIControls.PortletList.SelectedIndex;
            if (selectedIndex == -1) return;

            var selectedWebPart = UIControls.PortletList.SelectedValue;
            var wpm = WebPartManager.GetCurrentWebPartManager(Page);
            var webPart = wpm.WebParts[selectedWebPart];
            var portlet = webPart as PortletBase;

            UIControls.ErrorMessage.Text = string.Empty;
            var portletDescription = GetPortletDescription(webPart);
            if (!String.IsNullOrEmpty(portletDescription))
            {
                UIControls.ExportResult.Text = portletDescription;
            }
        }

        private void ExportAllPortlet()
        {
            UIControls.ErrorMessage.Text = string.Empty;
            var wpm = WebPartManager.GetCurrentWebPartManager(Page);
            var portletCount = wpm.WebParts.Count;
            var savedPortletNum = 0;
            foreach (WebPart wp in wpm.WebParts)
            {
                var portletDescription = GetPortletDescription(wp);
                if (String.IsNullOrEmpty(portletDescription))
                    continue;

                SavePortletDefinition(wp, portletDescription);
                savedPortletNum++;
            }
            UIControls.DisplayFeedback(String.Format(" ({0} / {1}) portlet(s) has been exported.", savedPortletNum,
                                                     portletCount));
        }

        private void ImportAllPortlets(List<Node> portletInfoFileList)
        {
            foreach (var portletFile in portletInfoFileList)
            {
                var infoFile = portletFile as SNC.File;

                if (infoFile.Binary == null)
                    throw new InvalidOperationException(String.Format("{0} has no binary data!", infoFile.Path));


                var portletDescriptionStream = infoFile.Binary.GetStream();
                var portletDescriptionString = Tools.GetStreamString(portletDescriptionStream);

                var portletId = GetPortletId(infoFile.Name);
                var portletInfos = portletId.Split('$'); // [0] = portletId, [1] = zoneId
                var wpm = WebPartManager.GetCurrentWebPartManager(Page);

                var isOverwrite = UIControls.OverwritePortletRadioButton.Checked;
                if (isOverwrite)
                {
                    // delete portlet and import it again
                    var webPart = wpm.WebParts[portletId];
                    if (webPart != null)
                    {
                        wpm.DeleteWebPart(webPart);
                        string errorMessage;
                        ImportPortletInternal(portletDescriptionString, out errorMessage, false, portletInfos[1]);
                    }
                }
                else
                {
                    string errorMessage;
                    ImportPortletInternal(portletDescriptionString, out errorMessage, false, portletInfos[1]);
                }
            }
        }

        private void SetImportPostBackButton()
        {
            var importPostBackTrigger = new PostBackTrigger();
            importPostBackTrigger.ControlID = UIControls.ImportButton.ID;
            _masterUpdatePanel.Triggers.Add(importPostBackTrigger);
        }

        private WebPartZoneBase GetSelectedZoneInstance()
        {
            var selectedIndex = UIControls.ImportZoneList.SelectedIndex;
            if (selectedIndex == -1) return null;

            var selectedWebPartZone = UIControls.ImportZoneList.SelectedValue;
            var wpm = WebPartManager.GetCurrentWebPartManager(Page);
            var webPartZone = wpm.Zones[selectedWebPartZone];
            return webPartZone;
        }

        private string GetPortletDescription(Control webPart)
        {
            if (webPart == null)
                throw new ArgumentNullException("webPart");

            var portlet = webPart as PortletBase;
            if (portlet == null)
            {
                UIControls.DisplayErrorMessage(
                    String.Format("{0} portlet couldn't be exported because it is not derived from PortletBase.<br />",
                                  webPart.ID));
                return string.Empty;
            }
            switch (portlet.ExportMode)
            {
                case WebPartExportMode.None:
                    UIControls.DisplayErrorMessage(String.Format(
                                                       "Export mode of the {0} portlet is not enabled.<br />",
                                                       portlet.ID));
                    return string.Empty;
            }
            return portlet.GetExtractedPortletInfo();
        }

        internal static string GetPortletId(string fileName)
        {
            return fileName.Remove(fileName.IndexOf(PortletInfoFileNameExtension));
        }

        internal static void SavePortletDefinition(WebPart webPart, string contentString)
        {
            if (webPart == null)
                throw new ArgumentNullException("webPart");
            if (String.IsNullOrEmpty(contentString))
                throw new ArgumentException("contentString");

            var currentPage = SenseNet.Portal.Page.Current;
            var newPortletInfoFileName = String.Concat(webPart.ID, "$", webPart.Zone.ID, PortletInfoFileNameExtension);

            SNC.File newPortletInfoFile = null;
            BinaryData newPortletInfoBinary = null;

            var portletContentPath = RepositoryPath.Combine(currentPage.Path, newPortletInfoFileName);
            if (NodeHead.Get(portletContentPath) == null)
            {
				newPortletInfoFile = new SNC.File(currentPage);
                newPortletInfoBinary = new BinaryData();
            }
            else
            {
				newPortletInfoFile = Node.Load<SNC.File>(portletContentPath);
                newPortletInfoBinary = newPortletInfoFile.Binary;
            }

            var contentStream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(contentString));
            newPortletInfoBinary.SetStream(contentStream);

            newPortletInfoFile.Name = newPortletInfoFileName;
            newPortletInfoFile.Binary = newPortletInfoBinary;
            newPortletInfoFile.Save();
        }

        internal static List<Node> GetAllPortletDescriptionFiles(string path)
        {
            var query = new NodeQuery();
            query.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith,
                                           VirtualPathUtility.AppendTrailingSlash(path)));
            query.Add(new TypeExpression(ActiveSchema.NodeTypes["File"], true));

            return query.Execute().Nodes.ToList();
        }
    }
}