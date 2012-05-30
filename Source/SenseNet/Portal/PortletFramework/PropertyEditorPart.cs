using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Diagnostics;
using SenseNet.Portal.PortletFramework;
using System.Text;
using SenseNet.ContentRepository.i18n;

namespace SenseNet.Portal.UI.PortletFramework
{
    public class PropertyEditorPart : EditorPart
    {
        // Fields /////////////////////////////////////////////////////////////////
        private readonly List<string> _categories = new List<string>();
        private ArrayList _editorControls;
        private int _errorNum = 0;
        private readonly List<string> _errorMessages = new List<string>();
        private Dictionary<string, List<PropertyDescriptor>> _groupProperties;
        private object EditableObject
        {
            get
            {
                WebPart webPartToEdit = base.WebPartToEdit;
                IWebEditable editable = webPartToEdit;
                if (editable != null)
                    return editable.WebBrowsableObject;
                return webPartToEdit;
            }
        }
        public ArrayList EditorControls
        {
            get
            {
                if (_editorControls == null)
                    _editorControls = new ArrayList();
                return _editorControls;
            }
        }
        private bool HasError
        {
            get
            {
                return _errorNum > 0;
            }
        }

        public List<string> HiddenCategories { get; set; }

        public List<string> HiddenProperties { get; set; }

        
        // Events /////////////////////////////////////////////////////////////////
        protected override void CreateChildControls()
        {
            object editableObject = EditableObject;
            if (editableObject != null)
            {
                CollectEditableProperties(editableObject);
                //_errorMessages = new string[EditorControls.Count];
            }

            foreach (Control control2 in Controls)
                control2.EnableViewState = false;
        }
        public override bool ApplyChanges()
        {
            object editableObject = EditableObject;
            if (editableObject == null)
                return true;

            EnsureChildControls();
            if (Controls.Count == 0)
                return true;
            
            for (var i = 0; i < EditorControls.Count; i++)
            {
                object control = EditorControls[i];
                var placeHolder = control as PlaceHolder;
                if (placeHolder == null)
                    continue;

                foreach (var entry in _groupProperties)
                {
                    string title = entry.Key;
                    List<PropertyDescriptor> descriptors = entry.Value;

                    if (String.IsNullOrEmpty(title) || descriptors == null)
                        continue;

                    foreach (PropertyDescriptor descriptor in descriptors)
                    {
                        var editorControl = CreateEditorControl(descriptor);
                        if (editorControl == null)
                            continue;
                        var c = GetControlRecursive(placeHolder, editorControl.ID);
                        if (c == null)
                            continue;
                        
                        try
                        {
                            object editorControlValue = GetEditorControlValue(c, descriptor);
                            descriptor.SetValue(editableObject,editorControlValue);
                        } 
                        catch(Exception ex)
                        {
                            var panel = c.Parent as PropertyFieldPanel;
                            var panelTitle = panel == null ? "Unknown" : panel.Title;
                            var eField = c as IEditorPartField;
                            var eFieldTitle = eField == null ? "unknown field" : eField.Title;
                            var errorMessage = string.Format("{0} ({1} panel, {2} field)", ex.Message, panelTitle, eFieldTitle);

                            Logger.WriteException(ex);
                            _errorMessages.Add(errorMessage);
                            _errorNum++;                            
                        }
                    }
                }
            }
            
            if (HasError)
            {
                var errorGroupFieldPanel = new PropertyFieldPanel { Title = "Error", EnableViewState = false };
                var erros = new StringBuilder();

                foreach (var errorMessage in _errorMessages)
                    erros.Append(errorMessage + "<br />");
                
                errorGroupFieldPanel.Controls.Add(new LiteralControl(erros.ToString()));

                var errorControl = Controls[0] as PlaceHolder;
                if (errorControl != null)
                    errorControl.Controls.Add(errorGroupFieldPanel);

            }

            return HasError;
        }
        public override void SyncChanges()
        {
            object editableObject = EditableObject;
            if (editableObject == null)
                return;

            EnsureChildControls();

            for (int i = 0; i < EditorControls.Count; i++)
            {
                object control = EditorControls[i];
                var placeHolder = control as PlaceHolder;
                if (placeHolder != null)
                {
                    foreach (var entry in _groupProperties)
                    {
                        string title = entry.Key;
                        List<PropertyDescriptor> descriptors = entry.Value;

                        if (String.IsNullOrEmpty(title) || descriptors == null)
                            continue;

                        foreach (PropertyDescriptor descriptor in descriptors)
                        {
                            Control editorControl = CreateEditorControl(descriptor);
                            if (editorControl == null)
                                continue;
                            Control c = GetControlRecursive(placeHolder, editorControl.ID);
                            if (c != null)
                                SyncChanges(c, descriptor, editableObject);
                        }
                    }
                }
            }
        }
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if ((Display && Visible) && !HasError)
                SyncChanges();
        }        


        // Internals //////////////////////////////////////////////////////////////
        private void CollectEditableProperties(object editableObject)
        {
            _groupProperties = new Dictionary<string, List<PropertyDescriptor>>();

            var controls = Controls;
            controls.Clear();
            EditorControls.Clear();
            controls.Add(new PlaceHolder());
            var properties = GetEditableProperties(editableObject);
            
            // grouping 
            foreach (PropertyDescriptor descriptor in properties)
            {
                if ((this.HiddenProperties != null) && (this.HiddenProperties.Contains(descriptor.Name)))
                    continue;

                var categoryTitle = GetCategoryTitle(descriptor);
                if (HiddenCategories == null || (HiddenCategories != null) && (!HiddenCategories.Contains(categoryTitle)))
                {
                    if (!String.IsNullOrEmpty(categoryTitle))
                    {
                        List<PropertyDescriptor> categoryValue = null;
                        categoryValue = _groupProperties.ContainsKey(categoryTitle) ? _groupProperties[categoryTitle] : new List<PropertyDescriptor>();

                        categoryValue.Add(descriptor);
                        if (!_groupProperties.ContainsKey(categoryTitle))
                            _groupProperties.Add(categoryTitle, categoryValue);
                    }
                }

                if (_categories.Contains(categoryTitle) || String.IsNullOrEmpty(categoryTitle))
                    continue;
                _categories.Add(categoryTitle);
            }

            // create controls
            var editorControls = new PlaceHolder {ID = "GroupProperties", EnableViewState = false};
            foreach (var entry in _groupProperties)
            {
                var title = entry.Key;
                entry.Value.Sort(new WebOrderComparer());
                var descriptors = entry.Value;

                if (String.IsNullOrEmpty(title) || descriptors == null)
                    continue;

                var groupControl = new PropertyFieldPanel { Title = title };
                groupControl.EnableViewState = false;
                foreach (var descriptor in descriptors)
                {
                    var editorControl = CreateEditorControl(descriptor);
                    if (editorControl == null)
                        continue;
                    editorControl.EnableViewState = false;
                    groupControl.Controls.Add(editorControl);
                }
                if (groupControl.Controls.Count > 0)
                    editorControls.Controls.Add(groupControl);
            }
            if (editorControls.Controls.Count == 0)
                return;
            EditorControls.Add(editorControls);
            controls.Add(editorControls);
        }
        private PropertyDescriptorCollection GetEditableProperties(object editableObject)
        {
            if (editableObject == null)
                throw new ArgumentNullException("editableObject");

            var webBrowsableFilter = new Attribute[] {WebBrowsableAttribute.Yes};
            var properties = TypeDescriptor.GetProperties(editableObject, webBrowsableFilter);
            var editableProperties = new PropertyDescriptorCollection(null);
            foreach (PropertyDescriptor descriptor in properties)
            {
                if (CanEditProperty(descriptor))
                    editableProperties.Add(descriptor);
            }
            return editableProperties.Sort(new WebCategoryComparer());
        }
        private bool CanEditProperty(PropertyDescriptor property)
        {
            if (property.IsReadOnly)
                return false;
            
            if (((WebPartManager != null) && (WebPartManager.Personalization != null)) &&
                ((WebPartManager.Personalization.Scope == PersonalizationScope.User) &&
                 property.Attributes.Contains(PersonalizableAttribute.SharedPersonalizable)))
                return false;
            
            return CanConvertToFrom(property.Converter, typeof (string));
        }
        public static object GetEditorControlValue(Control editorControl, PropertyDescriptor descriptor)
        {
            var box = editorControl as CheckBoxEditorPartField;
            if (box != null)
                return box.Checked;

            var list = editorControl as DropDownPartField;
            if (list != null)
            {
                string selectedValue = list.SelectedValue;
                return descriptor.Converter.ConvertFromString(selectedValue);
            }

            var textEditorPartField = editorControl as TextBox;
            return descriptor.Converter.ConvertFromString(textEditorPartField.Text);
        }
        
        // Internals (static) /////////////////////////////////////////////////////
        public static Control GetControlRecursive(Control control, string id)
        {
            if (control == null) 
                return null;

            var ctrl = control.FindControl(id);
            if (ctrl == null)
            {
                foreach (Control child in control.Controls)
                {
                    ctrl = GetControlRecursive(child, id);
                    if (ctrl != null) 
                        break;
                }
            }
            return ctrl;
        }
        private static void SyncChanges(Control control, PropertyDescriptor descriptor, object instance)
        {

            // TODO: need more thinking about server-side component design. It is not such a just-an-overriden-webcontrol.
            // TODO: Extend the IEditorPartField interface with getter and setter for manipulationg state of the control!

            var val = descriptor.GetValue(instance);

            var checkBox = control as CheckBox;
            if (checkBox != null)
            {
                if (val == null)
                    return;
                checkBox.Checked = (bool)val;
                return;
            }

            var list = control as DropDownList;
            if (list != null)
            {
                list.SelectedValue = val == null ? string.Empty : val.ToString();
                return;
            }

            var textBox = control as TextBox;
            if (textBox != null)
            {
                textBox.Text = descriptor.Converter.ConvertToString(val);
                return;
            }
        }
        public static string GetCategoryTitle(PropertyDescriptor propertyDescriptor)
        {
            if (propertyDescriptor == null)
                throw new ArgumentNullException("propertyDescriptor");
            var category = propertyDescriptor.Attributes[typeof (WebCategoryAttribute)] as WebCategoryAttribute;
            if (category == null)
            {
                // treat it as OtherCategory
                var srm = ContentRepository.i18n.SenseNetResourceManager.Current;
                var otherCategoryTitle = srm.GetString("PortletFramework", "OtherCategoryTitle");
                return otherCategoryTitle;
            }

            return String.IsNullOrEmpty(category.Title) ? string.Empty : category.Title;
        }
        public static string GetDisplayName(PropertyDescriptor propertyDescriptor)
        {
            if (propertyDescriptor == null)
                throw new ArgumentNullException("propertyDescriptor");
            var displayNameAttribute = propertyDescriptor.Attributes[typeof (LocalizedWebDisplayNameAttribute)] as LocalizedWebDisplayNameAttribute;
            string result = string.Empty;
            if (displayNameAttribute == null)
            {
                var originalDisplayNameAttribute = propertyDescriptor.Attributes[typeof (WebDisplayNameAttribute)] as WebDisplayNameAttribute;
                result = originalDisplayNameAttribute == null ? null : originalDisplayNameAttribute.DisplayName;
            }
            else 
                result = displayNameAttribute.DisplayName;
            return String.IsNullOrEmpty(result) ? string.Empty : result;
        }
        public static string GetDescription(PropertyDescriptor propertyDescriptor)
        {
            if (propertyDescriptor == null)
                throw new ArgumentNullException("propertyDescriptor");
            var webDescriptionAttribute = propertyDescriptor.Attributes[typeof (LocalizedWebDescriptionAttribute)] as LocalizedWebDescriptionAttribute;
            string result = string.Empty;
            if (webDescriptionAttribute == null)
            {
                var originalDescriptionAttribute = propertyDescriptor.Attributes[typeof(WebDescriptionAttribute)] as WebDescriptionAttribute;
                result = originalDescriptionAttribute == null ? null : originalDescriptionAttribute.Description;
            } else
                result = webDescriptionAttribute.Description;
            return String.IsNullOrEmpty(result) ? string.Empty : result;
        }
        private static EditorOptions GetEditorOptionsAttribute(System.ComponentModel.AttributeCollection attribCollection)
        {
            foreach (Attribute attrib in attribCollection)
            {
                if (attrib is EditorOptions)
                    return attrib as EditorOptions;
            }
            return null;
        }
        internal static Control CreateEditorControl(PropertyDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException("descriptor");

            var propertyType = descriptor.PropertyType;
            object editor = null;
            
            try
            {
                editor = descriptor.GetEditor(typeof(IEditorPartField));
            }
            catch (Exception e)
            {
                Logger.WriteException(e);
            }

            var propName = descriptor.Name;

            if (editor != null)
            {
                var partField = editor as IEditorPartField;
                if (partField != null)
                {
                    partField.Options = GetEditorOptionsAttribute(descriptor.Attributes);
                    partField.TitleContainerCssClass = "sn-iu-label";
                    partField.TitleCssClass = "sn-iu-title";
                    partField.DescriptionCssClass = "sn-iu-desc";
                    partField.ControlWrapperCssClass = "sn-iu-control";
                    partField.Title = GetDisplayName(descriptor);
                    partField.Description = GetDescription(descriptor);
                    partField.EditorPartCssClass = "sn-inputunit ui-helper-clearfix sn-custom-editorpart-text sn-editorpart-" + propName;
                    partField.PropertyName = propName;

                    if (propertyType == typeof (bool))
                        partField.EditorPartCssClass = "sn-inputunit ui-helper-clearfix sn-custom-editorpart-boolean sn-editorpart-" + propName;
                    if (typeof (Enum).IsAssignableFrom(propertyType))
                    {
                        // TODO: fill the instance of the EditorPart control with datas. Best solution is extend the IEditorPartField interface with Fill method 
                    }
                    var result = partField as Control;
                    result.ID = "Custom" + propName;
                    return result;
                }
            }

            if (propertyType == typeof (bool))
            {
                var checkBox = new CheckBoxEditorPartField();
                checkBox.ID = "CheckBox" + propName;
                checkBox.Options = GetEditorOptionsAttribute(descriptor.Attributes);
                checkBox.EditorPartCssClass = "sn-inputunit ui-helper-clearfix sn-custom-editorpart-boolean sn-editorpart-" + propName;
                checkBox.Title = GetDisplayName(descriptor);
                checkBox.Description = GetDescription(descriptor);
                checkBox.TitleContainerCssClass = "sn-iu-label";
                checkBox.TitleCssClass = "sn-iu-title";
                checkBox.DescriptionCssClass = "sn-iu-desc";
                checkBox.ControlWrapperCssClass = "sn-iu-control";
                checkBox.PropertyName = propName;

                return checkBox;
            }

            if (typeof (Enum).IsAssignableFrom(propertyType))
            {
                ICollection standardValues = descriptor.Converter.GetStandardValues();
                if (standardValues != null)
                {
                    var list = new DropDownPartField();
                    list.ID = "DropDown" + propName;
                    list.Options = GetEditorOptionsAttribute(descriptor.Attributes);
                    list.EditorPartCssClass = "sn-inputunit ui-helper-clearfix sn-custom-editorpart-enum sn-editorpart-" + propName;
                    list.Title = GetDisplayName(descriptor);
                    list.Description = GetDescription(descriptor);
                    list.TitleContainerCssClass = "sn-iu-label";
                    list.TitleCssClass = "sn-iu-title";
                    list.DescriptionCssClass = "sn-iu-desc";
                    list.ControlWrapperCssClass = "sn-iu-control";
                    list.PropertyName = propName;
                    foreach (object value in standardValues)
                    {
                        var resourceKey = String.Concat("Enum-", propName, "-", value.ToString());
                        var text = SenseNetResourceManager.Current.GetStringOrNull("PortletFramework", resourceKey) as string;
                        if (string.IsNullOrEmpty(text))
                            text = descriptor.Converter.ConvertToString(value);
                        list.Items.Add(new ListItem(text, value.ToString()));
                    }
                    return list;
                }

                return null;
            }

            var textBox = new TextEditorPartField();
            textBox.ID = "TextBox" + propName;
            textBox.Options = GetEditorOptionsAttribute(descriptor.Attributes);
            textBox.EditorPartCssClass = "sn-inputunit ui-helper-clearfix sn-custom-editorpart-text sn-editorpart-" + propName;
            textBox.Title = GetDisplayName(descriptor);
            textBox.Description = GetDescription(descriptor);
            textBox.TitleContainerCssClass = "sn-iu-label";
            textBox.TitleCssClass = "sn-iu-title";
            textBox.DescriptionCssClass = "sn-iu-desc";
            textBox.ControlWrapperCssClass = "sn-iu-control";
            textBox.Columns = 30;
            textBox.PropertyName = propName;
            return textBox;
        }
        internal static bool CanConvertToFrom(TypeConverter converter, Type type)
        {
            return ((((converter != null) && converter.CanConvertTo(type)) && converter.CanConvertFrom(type)) &&
                    !(converter is ReferenceConverter));
        }
    }
}
