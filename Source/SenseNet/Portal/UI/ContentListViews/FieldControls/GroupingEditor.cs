using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Search.Internal;
using SenseNet.Portal.UI.Controls;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.Portal.UI.ContentListViews.FieldControls
{
    public class GroupingEditor : FieldControl
    {
        //========================================================================= Properties

        private GenericContent _systemContext;
        public GenericContent SystemContext
        {
            get
            {
                if (_systemContext == null)
                {
                    var node = Content.ContentHandler as GenericContent;
                    _systemContext = (node != null) ? node.MostRelevantSystemContext : null;
                }

                return _systemContext;
            }
        }

        private List<FieldSetting> _availableFields;
        public IEnumerable<FieldSetting> AvailableFields
        {
            get
            {
                if (_availableFields == null)
                {
                    _availableFields = new List<FieldSetting>();

                    //get leaf settings to determine visibility using the most granted mode
                    var leafFieldSettings = SystemContext.GetAvailableFields(false);

                    foreach (var fieldSetting in leafFieldSettings)
                    {
                        var fs = fieldSetting;

                        while (fs != null)
                        {
                            if (fs.VisibleBrowse != FieldVisibility.Hide ||
                                fs.VisibleEdit != FieldVisibility.Hide ||
                                fs.VisibleNew != FieldVisibility.Hide)
                            {
                                //get the root field setting and add if it was not added before
                                var rootFs = FieldSetting.GetRoot(fs);
                                if (!_availableFields.Contains(rootFs))
                                    _availableFields.Add(rootFs);

                                break;
                            }

                            fs = fs.ParentFieldSetting;
                        }
                    }

                    _availableFields.Sort(CompareFields);
                }

                return _availableFields;
            }
        }

        private DropDownList _ddFieldName;
        private DropDownList FieldNameDropDown
        {
            get
            {
                if (_ddFieldName == null && this.Controls.Count > 0)
                {
                    _ddFieldName = this.Controls[0].FindControl("ddFieldName") as DropDownList;
                }

                return _ddFieldName;
            }
        }

        //========================================================================= FieldControl functions

        public override object GetData()
        {
            if (this.FieldNameDropDown == null || this.FieldNameDropDown.SelectedValue.Length == 0)
                return string.Empty;

            return this.FieldNameDropDown.SelectedValue;
        }

        public override void SetData(object data)
        {
            var groupExpression = data as string;
            var fieldTitles = new List<string>();
            var duplicatedTitles = new List<string>();

            if (this.FieldNameDropDown == null)
                return;

            this.FieldNameDropDown.Items.Clear();
            this.FieldNameDropDown.Items.Add(new ListItem("", ""));

            //collect duplicated titles first
            foreach (var fs in this.AvailableFields)
            {
                if (fieldTitles.Contains(fs.DisplayName))
                    duplicatedTitles.Add(fs.DisplayName);
                else
                    fieldTitles.Add(fs.DisplayName);
            }

            //fill the dropdown, marking the duplicated titles
            foreach (var fs in this.AvailableFields)
            {
                var title = duplicatedTitles.Contains(fs.DisplayName)
                                ? string.Format("{0} ({1})", fs.DisplayName, fs.Owner.DisplayName)
                                : fs.DisplayName;

                this.FieldNameDropDown.Items.Add(new ListItem(title, fs.Name));
            }

            if (string.IsNullOrEmpty(groupExpression))
                return;

            SelectDropDown(this.FieldNameDropDown, groupExpression);
        }

        //========================================================================= Control overrides

        protected override void OnInit(EventArgs e)
        {
            if (this.Controls.Count == 0)
            {
                var c = Page.LoadControl("/Root/System/SystemPlugins/ListView/GroupingEditorControl.ascx");
                if (c != null)
                {
                    this.Controls.Add(c);
                }
            }

            base.OnInit(e);
        }

        public override void DataBind()
        {
            //Do nothing here, to preserve the dropdown values 
            //selected by the user. 
            //base.DataBind overrides them
        }

        //========================================================================= Helper functions

        private static void SelectDropDown(ListControl dd, string value)
        {
            if (dd == null)
                return;

            for (var i = 0; i < dd.Items.Count; i++)
            {
                if (dd.Items[i].Value.CompareTo(value) != 0)
                    continue;

                dd.SelectedIndex = i;
                break;
            }
        }

        private static int CompareFields(FieldSetting x, FieldSetting y)
        {
            var left = x.DisplayName ?? string.Empty;
            var right = y.DisplayName ?? string.Empty;
            return left.CompareTo(right);
        }
    }
}
