using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.Portlets
{
    internal class ImportExportControlFacade : IDisposable
    {
        // Members ////////////////////////////////////////////////////////
        private UIControlsFactory UIControls { get; set; }
        private ControlCollection CurrentColllection { get; set; }
        
        public Button ImportButton { get; private set; }
        public TextBox ImportTextBox { get; private set; }
        public DropDownList ImportZoneList { get; private set; }
        public Button ExportButton { get; private set; }
        public DropDownList PortletList { get; private set; }
        public TextBox ExportResult { get; private set; }

        public Button ImportAllButton { get; private set; }
        public Button ExportAllButton { get; private set; }

        public Label ErrorMessage { get; private set; }
        public Label FeedbackMessage { get; private set; }
        public RadioButton OverwritePortletRadioButton { get; private set; }

        public ImportExportControlFacade()
        {

        }

        public void Init()
        {
            UIControls = new UIControlsFactory();
            ErrorMessage = UIControls.CreateErrorMessageLabel();
            FeedbackMessage = UIControls.CreateFeedbackLabel();
        }
        public void BuildImportControls(ControlCollection controls, System.Web.UI.Page page)
        {
            if (controls == null)
                throw new ArgumentNullException("controls");
            if (page == null) 
                throw new ArgumentNullException("page");

            var isMessageAdded = controls.Contains(ErrorMessage);
            if (!isMessageAdded)
            {
                controls.Add(ErrorMessage);
                controls.Add(FeedbackMessage);
            }
            CurrentColllection = controls;

            ImportButton = UIControls.CreateImportButton();
            ImportButton.Click += new EventHandler(ImportButton_Click);

            ImportTextBox = UIControls.CreateImportTextArea();
            ImportZoneList = GetZoneList(page);

            var importPanel = new Panel();
            importPanel.ID = "ImportControlsPanel";
            importPanel.GroupingText = "Import";


            importPanel.Controls.Add(UIControls.CreateLineBreak());
            importPanel.Controls.Add(ImportZoneList);
            importPanel.Controls.Add(UIControls.CreateLineBreak());
            importPanel.Controls.Add(ImportTextBox);
            importPanel.Controls.Add(UIControls.CreateLineBreak());
            importPanel.Controls.Add(ImportButton);
            controls.Add(importPanel);
        }
        public void BuildExportControls(ControlCollection controls, System.Web.UI.Page page)
        {
            if (controls == null) 
                throw new ArgumentNullException("controls");
            if (page == null) 
                throw new ArgumentNullException("page");

            var isMessageAdded = controls.Contains(ErrorMessage);
            if (!isMessageAdded)
            {
                controls.Add(ErrorMessage);
                controls.Add(FeedbackMessage);
            }
                

            CurrentColllection = controls;

            ExportButton = UIControls.CreateExportPortletButton();
            ExportButton.Click += new EventHandler(ExportButton_Click);

            PortletList = GetPortletList(page);

            var exportPanel = new Panel();
            exportPanel.ID = "ExportControlPanels";
            exportPanel.GroupingText = "Export";


            exportPanel.Controls.Add(UIControls.CreateLineBreak());
            exportPanel.Controls.Add(PortletList);
            exportPanel.Controls.Add(ExportButton);

            controls.Add(exportPanel);
        }
        public void BuildToolbarButtons(ControlCollection controls)
        {
            if (controls == null)
                throw new ArgumentNullException("controls");
            OverwritePortletRadioButton = UIControls.CreateOverwriteRadioButton();
            ImportAllButton = UIControls.CreateImportAllButton();
            ExportAllButton = UIControls.CreateExportAllButton();
            
            ImportAllButton.Click += new EventHandler(ImportAllButton_Click);
            ExportAllButton.Click += new EventHandler(ExportAllButton_Click);

            controls.Add(UIControls.CreateLineBreak());
            controls.Add(OverwritePortletRadioButton);
            
            controls.Add(ImportAllButton);
            
            controls.Add(UIControls.CreateLineBreak());
            controls.Add(ExportAllButton);
            
            controls.Add(UIControls.CreateLineBreak());
        }

        public void DetachEvents()
        {
            ImportClick = null;
            ImportButton.Click -= new EventHandler(ImportButton_Click);
            ExportClick = null;
            ExportButton.Click -= new EventHandler(ExportButton_Click);
            ImportAllButton = null;
            ImportAllButton.Click -= new EventHandler(ImportAllButton_Click);
            

        }

        // Internals //////////////////////////////////////////////////////
        private DropDownList GetPortletList(System.Web.UI.Page page)
        {
            if (page == null) 
                throw new ArgumentNullException("page");

            var wpm = WebPartManager.GetCurrentWebPartManager(page) as WebPartManager;
            var portletList = UIControls.CreatePortletList();
            
            foreach(WebPart wp in wpm.WebParts)
                portletList.Items.Add(new ListItem(wp.ID));

            return portletList;
        }
        private DropDownList GetZoneList(System.Web.UI.Page page)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            var wpm = WebPartManager.GetCurrentWebPartManager(page) as WebPartManager;
            var zoneList = UIControls.CreateZoneList();
            foreach(WebPartZone wpZone in wpm.Zones)
                zoneList.Items.Add(new ListItem(wpZone.ID));

            return zoneList;
        }
        private void CreateExportResultControl()
        {
            ExportResult = UIControls.CreateExportResultTextArea();
            ExportButton.Parent.Controls.Add(UIControls.CreateLineBreak());
            ExportButton.Parent.Controls.Add(ExportResult);
        }
        
        // Public methods /////////////////////////////////////////////////
        public void DisplayFeedback(string message)
        {
            FeedbackMessage.ForeColor = System.Drawing.Color.Green;
            FeedbackMessage.Text = message;
        }
        public void DisplayErrorMessage(Exception exc)
        {
            var messageFormat = @"Source: '{0}'<br />
                                ErrorMessage: '{1}'<br />
                                StackTrace: '2'<br />";
            DisplayErrorMessage(String.Format(messageFormat, exc.Source, exc.Message, exc.StackTrace));
        }
        public void DisplayErrorMessage(string message)
        {
            ErrorMessage.ForeColor = System.Drawing.Color.Red;
            ErrorMessage.Text += message;
        }

        // Events /////////////////////////////////////////////////////////
        void ExportButton_Click(object sender, EventArgs e)
        {
            try
            {
                CreateExportResultControl();
                OnExportclick(sender, e);
            }
            catch(Exception exc) //logged
            {
                Logger.WriteException(exc);
                DisplayErrorMessage(exc);
            }
        }
        void ExportAllButton_Click(object sender, EventArgs e)
        {
            try
            {
                CreateExportResultControl();
                OnExportAllclick(sender, e);
            }
            catch (Exception exc) //logged
            {
                Logger.WriteException(exc);
                DisplayErrorMessage(exc);
            }
        }
        void ImportButton_Click(object sender, EventArgs e)
        {
            try
            {
                OnImportClick(sender, e);
            }
            catch (Exception exc) //logged
            {
                Logger.WriteException(exc);
                DisplayErrorMessage(exc);
            }
        }
        void ImportAllButton_Click(object sender, EventArgs e)
        {
            try
            {
                OnImportAllClick(sender, e);    
            }
            catch(Exception exc) //logged
            {
                Logger.WriteException(exc);
                DisplayErrorMessage(exc);
            }
            
        }
        public event EventHandler ImportClick;
        public event EventHandler ImportAllClick;
        public event EventHandler ExportClick;
        public event EventHandler ExportAllClick;
        protected virtual void OnImportClick(object sender, EventArgs e)
        {
            if (ImportClick != null)
                ImportClick(sender, e);
        }
        protected virtual void OnExportclick(object sender, EventArgs e)
        {
            if (ExportClick != null)
                ExportClick(sender, e);
        }
        protected virtual void OnImportAllClick(object sender, EventArgs e)
        {
            if (ImportAllClick != null)
                ImportAllClick(sender, e);
        }
        protected virtual void OnExportAllclick(object sender, EventArgs e)
        {
            if (ExportAllClick != null)
                ExportAllClick(sender, e);
        }

        #region IDisposable Members

        public void Dispose()
        {
            DetachEvents();
        }

        #endregion
    }
}
