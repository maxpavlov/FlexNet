using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Text.RegularExpressions;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:DisplayName ID=\"DisplayName1\" runat=server></{0}:DisplayName>")]
    public class DisplayName : ShortText
    {
        /* ================================================================================================ Members */
        // presence of name control is determined clientside, since commenting out a name fieldcontrol in a contenview leads to false functionality
        private readonly TextBox _nameAvailableControl;
        private string NameAvailableControlID = "NameAvailableControl";


        /* ================================================================================================ Properties */
        // when set to "true" from a contentview, the control will always update the name control, even if it is a new content
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool AlwaysUpdateName { get; set; }


        /* ================================================================================================ Constructor */
        public DisplayName()
        {
            _nameAvailableControl = new TextBox { ID = NameAvailableControlID };
        }


        /* ================================================================================================ Methods */
        protected override void OnInit(EventArgs e)
        {
            UITools.AddScript("$skin/scripts/sn/SN.ContentName.js");

            base.OnInit(e);

            if (this.ControlMode == FieldControlControlMode.Browse)
                return;

            // init javascripts
            var innerControl = _shortTextBox;
            var nameAvailableControl = _nameAvailableControl;
            if (IsTemplated)
            {
                innerControl = GetInnerControl() as TextBox;
                nameAvailableControl = GetNameAvailableControl();
            }

            // autofill enabled for new contents only.
            var originalName = this.Content.Name;
            if (this.Content.Id == 0 || AlwaysUpdateName)
                innerControl.Attributes.Add("onkeyup",
                                            string.Format("SN.ContentName.TextEnter('{0}', '{1}')", innerControl.ClientID, originalName));

            // this scripts sets state of nameAvailableControl, to indicate if name is visible in dom
            var initScript = string.Format("SN.ContentName.InitNameControl('{0}','{1}', '{2}');", nameAvailableControl.ClientID, RepositoryPath.InvalidNameCharsPatternForClient, ContentNamingHelper.PlaceholderSymbol);
            UITools.RegisterStartupScript("InitNameControl", initScript, this.Page);

            if (IsTemplated)
                return;

            _shortTextBox.Visible = false;
            _shortTextBox.Width = this.Width;
            _shortTextBox.MaxLength = this.MaxLength;
            _shortTextBox.CssClass = string.IsNullOrEmpty(this.CssClass) ? "sn-ctrl sn-ctrl-text" : this.CssClass;
            Controls.Add(_shortTextBox);
        }

        public override object GetData()
        {
            var nameAvailableControl = _nameAvailableControl;
            if (IsTemplated)
                nameAvailableControl = GetNameAvailableControl();

            // name control is available
            var nameControlAvailable = false;
            if (nameAvailableControl != null)
            {
                if (nameAvailableControl.Text != "0")
                    nameControlAvailable = true;
            }

            var displayName = string.Empty;
            if (!IsTemplated)
            {
                displayName = _shortTextBox.Text;
            }
            else
            {
                var innerControl = GetInnerControl() as TextBox;
                displayName = innerControl != null ? innerControl.Text : _shortTextBox.Text;
            }

            if (!nameControlAvailable && (this.Content.Id == 0 || AlwaysUpdateName))
            {
                // content name should be set automatically generated from displayname
                var newName = ContentNamingHelper.GetNameFromDisplayName(this.Content.Name, displayName);
                if (newName.Length > 0)
                    this.Content["Name"] = newName;
            }

            return displayName;
        }
        public TextBox GetNameAvailableControl()
        {
            return this.FindControlRecursive(NameAvailableControlID) as TextBox;
        }
    }
}
