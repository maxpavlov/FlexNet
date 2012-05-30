using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using System.Runtime.Serialization;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.Portal
{
    [Obsolete("This class was obsoleted by the SenseNet.ApplicationModel namespace")]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay}")]
    public class Action : IAction
    {
        public string Name { get; set; }            // unique
        public string Uri { get; set; }            // resolved by IActionLinkResolver.
        public string Icon { get; set; }            // default null: fallback to name.
        public string StyleHint { get; set; }        // default null: none.
        public string Text { get; set; }            // default null: fallback to name. resource? resolved by?
        public string Tooltip { get; set; }         // default null: none              resource? resolved by?
        public string Description { get; set; }     // default null: none
        public string Scenario { get; set; }        // default null: none
        public bool Enabled { get; set; }           // resolved by advanced logic
        public bool Visible { get; set; }           // resolved by advanced logic
        [IgnoreDataMember]
        public IEnumerable<PermissionType> RequiredPermissions { get; set; } // parsed from xml. used by advanced logic

        public Action()
        {
          this.Enabled = true;
        }

        public Action Clone()
        {
            return new Action
            {
                Name = this.Name,
                Enabled = this.Enabled,
                Visible = this.Visible,
                Text = this.Text,
                Uri = this.Uri,
                Icon = this.Icon,
                StyleHint = this.StyleHint,
                Tooltip = this.Tooltip,
                Scenario=this.Scenario,
                Description=this.Description,
                RequiredPermissions = CloneRequiredPermissions()
            };
        }
        private IEnumerable<PermissionType> CloneRequiredPermissions()
        {
            if (this.RequiredPermissions == null)
                return new PermissionType[0];
            return new List<SenseNet.ContentRepository.Storage.Schema.PermissionType>(this.RequiredPermissions).AsReadOnly();
        }
        IAction IAction.Clone()
        {
            return Clone();
        }

        [IgnoreDataMember]
        private string DebuggerDisplay
        {
            get { return String.Concat(Name, " (", (Enabled ? "en" : "dis"), "abled, ", Visible ? "" : "in", "visible)"); }
        }
    }

}
