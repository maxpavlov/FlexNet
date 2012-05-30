using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository;
using  SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Fields;
using System.Web.UI;
using System.Web;

namespace SenseNet.Portal.UI.Controls
{
	public abstract class ChoiceControl : FieldControl, INamingContainer
	{
        public enum SelectedValueTypes
        {
            Value = 0,
            Text = 1
        }

		private string _extraOptionValue = "__extravalue";
		private string _extraOptionText = HttpContext.GetGlobalResourceObject("Portal", "ChoiceControlExtraOptionText") as string;

		protected bool AllowExtraValue
		{
			get
			{
				var setting = this.Field.FieldSetting as ChoiceFieldSetting;
				if (setting == null)
					return false;
				return setting.AllowExtraValue == true && !DisableExtraValue;
			}
		}

		[PersistenceMode(PersistenceMode.Attribute)] 
		public string ExtraOptionValue
		{
			get { return _extraOptionValue; }
			set { _extraOptionValue = value; }
		}
		[PersistenceMode(PersistenceMode.Attribute)]
		public string ExtraOptionText
		{
			get { return _extraOptionText; }
			set { _extraOptionText = value; }
		}
		[PersistenceMode(PersistenceMode.Attribute)]
		public bool DisableExtraValue { get; set; }

        //display the text by default
	    private SelectedValueTypes _selectedValueType = SelectedValueTypes.Text;

	    [PersistenceMode(PersistenceMode.Attribute)]
	    public SelectedValueTypes SelectedValueType
	    {
	        get { return _selectedValueType; }
	        set { _selectedValueType = value; }
	    }

	    public override object GetData()
		{
			return GetSelectedOptions(InnerListItemCollection);
		}
		protected List<string> GetSelectedOptions(ListItemCollection listItems)
		{
		    return GetSelectedItems(listItems, true);
		}
        /// <summary>
        /// Returns a list with the selected items and the given extra value if specified.
        /// </summary>
        /// <param name="listItems">Collection of the items are given in the field.</param>
        /// <param name="returnWithValues">If true, the result collection is constructed with value of the ListItem. If false, the value of the text property of the ListItem is used.</param>
        /// <returns>Collection of the selected items</returns>
	    protected List<string> GetSelectedItems(ListItemCollection listItems, bool returnWithValues)
	    {
	        var selectedOptions = new List<string>();
	        foreach (ListItem item in listItems)
	        {
	            if (item.Selected)
	                selectedOptions.Add(returnWithValues ? item.Value : item.Text);
	        }
	        if (selectedOptions.Contains(ExtraOptionValue))
	        {
	            selectedOptions.Remove(ExtraOptionValue);
	            selectedOptions.Add(GetExtraValue());
	        }
	        return selectedOptions;
	    }

	    public override void SetData(object data)
		{
			BuildControl(InnerListItemCollection, (List<string>)data);
		}
		protected void BuildControl(ListItemCollection itemCollection, List<string> selectedItems)
		{
			var choiceField = this.Field as ChoiceField;
			if (choiceField == null)
				throw new InvalidCastException("ChoiceControl have to connect to a ChoiceField.");

			var setting = (ChoiceFieldSetting)this.Field.FieldSetting;
			BuildOptions(itemCollection, setting.Options, selectedItems);
		}
		protected void BuildOptions(ListItemCollection listItems, List<ChoiceOption> configuredOptions, List<string> values)
		{
			listItems.Clear();
            bool hasValues = values != null && values.Count != 0;
			List<string> valueList = hasValues ? new List<string>(values) : new List<string>();
			var extraItem = new ListItem(ExtraOptionText, ExtraOptionValue, true);

			foreach (ChoiceOption option in configuredOptions)
			{
				var newItem = new ListItem(option.Text, option.Value, option.Enabled);
				if (hasValues)
				{
					newItem.Selected = valueList.Contains(option.Value);
					valueList.Remove(option.Value);
				}
				else
				{
					newItem.Selected = option.Selected;
				}
				listItems.Add(newItem);
			}
			if (AllowExtraValue)
			{
				listItems.Add(extraItem);
			}
			if (hasValues && valueList.Count > 0)
			{
				extraItem.Selected = true;
				SetExtraValue(valueList[0]);
			}
		}

        protected virtual void FillBrowseControls()
        {
            var ic = GetBrowseControl() as Label;
            if (ic == null) 
                return;

            var setting = this.Field.FieldSetting as ChoiceFieldSetting;
            if (setting == null)
                return;

            var data = this.GetData() as List<string>;
            if (data == null)
                return;

            var displayedItems = new ChoiceOptionValueList<string>(data, setting, this.SelectedValueType.Equals(SelectedValueTypes.Value));
            ic.Text = displayedItems.ToString();
        }

        protected void AddChangeScript(WebControl listControl, Control extratextBox)
        {
            if (listControl == null || extratextBox == null)
                return;

            var selectedBehaviorJavaScript = "this.selectedIndex == -1 ? -1 : this.options[this.selectedIndex].value === '{0}' ? document.getElementById('{1}') ? document.getElementById('{1}').style.display = '' : -1 : document.getElementById('{1}') ? document.getElementById('{1}').style.display = 'none' : -1;if (this.options[this.selectedIndex].value !== '{0}') if (document.getElementById('{1}')) {{ document.getElementById('{1}').style.display = 'none'; document.getElementById('{1}').value = '';}}";
            var onchangeJavaScript = String.Format(selectedBehaviorJavaScript, ExtraOptionValue, extratextBox.ClientID);
            listControl.Attributes.Add("onchange", onchangeJavaScript);
        }

		protected abstract ListItemCollection InnerListItemCollection { get;}
		protected abstract string GetExtraValue();
		protected abstract void SetExtraValue(string value);

        public override void DoAutoConfigure(FieldSetting setting)
        {
            var choiceSetting = setting as ChoiceFieldSetting;
            if (choiceSetting == null)
                throw new ApplicationException("A Choice field control can only be used in conjunction with a Choice field.");

            if (choiceSetting.AllowExtraValue == true)
                DisableExtraValue = false;

            base.DoAutoConfigure(setting);
        }

        public Control GetBrowseControl() { return this.FindControlRecursive(InnerControlID); }
	}
}