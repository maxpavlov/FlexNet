using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:FieldBound ID=\"FieldBound1\" runat=server />")]
    public class FieldBound : ViewControlBase
    {
        [PersistenceMode(PersistenceMode.Attribute)]
        public string FieldName { get; set; }

        [PersistenceMode(PersistenceMode.Attribute)]
        public FieldControlRenderMode RenderMode { get; set; }

        protected override void CreateChildControls()
        {
            try
            {
                var field = ContentView.Content.Fields[FieldName];

                var control = GenericFieldControl.CreateDefaultFieldControl(field);
                control.ID = string.Concat("Taxative_", Guid.NewGuid().ToString());
                if (RenderMode != null)
                    control.RenderMode = RenderMode;

                Controls.Add(control);
            }
            catch (Exception e)
            {
                Logger.WriteException(e);
                Controls.Add(new Label { Text = string.Concat("Could not create control for Field: ", FieldName) });
            }

            ChildControlsCreated = true;
        }
    }
}
