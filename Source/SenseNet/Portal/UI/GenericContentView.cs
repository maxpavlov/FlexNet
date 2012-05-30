using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Schema;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.Controls;
using System.Linq;

namespace SenseNet.Portal.UI
{
    public class GenericContentView : SingleContentView
    {
        private int _id;

        private Panel _advancedPanel;
        protected Panel AdvancedPanel
        {
            get
            {
                if (_advancedPanel == null)
                {
                    _advancedPanel = new Panel { CssClass = "sn-advancedfields" };
                    _advancedPanel.Style.Add("display", "none");
                }

                return _advancedPanel;
            }
        }

        private void AddControl(ControlCollection container, SenseNet.ContentRepository.Field field)
        {
            var control = GenericFieldControl.CreateDefaultFieldControl(field);
            control.ID = String.Concat("Generic", _id++);

            var fv = GenericFieldControl.GetFieldVisibility(ViewMode, field);

            if (fv == FieldVisibility.Advanced && AdvancedPanel != null)
            {
                AdvancedPanel.Controls.Add(control);
            }
            else
            {
                container.Add(control);
            }
        }

        protected override void OnViewInitialize()
        {
            this.ViewControlFrameMode = FieldControlFrameMode.ShowFrame;

            ControlCollection container;
            var c = this.FindControl("GenericControls") ?? this;

            try
            {
                var ivc = c.FindControl("InlineViewContent");
                if (ivc == null)
                    return;

                container = ivc.Controls; // c.Controls;
                container.Clear();

                var fields = this.Content.Fields;
                _id = 0;
                var fieldNames = GenericFieldControl.GetVisibleFieldNames(this.Content, ViewMode);

                // name and urlname comes first
                var nameField = fieldNames.Where(f => f == "DisplayName").FirstOrDefault();
                if (nameField != null)
                {
                    var field = fields[nameField];
                    AddControl(container, field);
                }
                var urlNameField = fieldNames.Where(f => f == "Name").FirstOrDefault();
                if (urlNameField != null)
                {
                    var field = fields[urlNameField];
                    AddControl(container, field);
                }

                // add the rest
                foreach (var fieldName in fieldNames)
                {
                    if (fieldName == "Name" || fieldName == "DisplayName")
                        continue;

                    var field = fields[fieldName];
                    AddControl(container, field);
                }

                if (AdvancedPanel != null && AdvancedPanel.Controls.Count > 0)
                {
                    var advancedButton = Page.LoadControl("/Root/System/SystemPlugins/Controls/AdvancedPanelButton.ascx") as AdvancedPanelButton;

                    if (advancedButton != null)
                    {
                        container.Add(advancedButton);
                        container.Add(AdvancedPanel);

                        advancedButton.AdvancedPanel = AdvancedPanel;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }
        }
    }
}