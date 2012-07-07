using System;

namespace SenseNet.Portal.UI.PortletFramework
{
    public enum ContentPickerMultiSelectMode
    {
        None = 0,
        Button, 
        Checkbox
    }

    public enum ContentPickerCommonType
    {
        Renderer = 0,
        Ascx,
        ContentView,
        ViewFrame,
        Icon,
        Css,
        Js,
        FirstManager
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ContentPickerEditorPartOptions : EditorOptions
    {
        /* ================================================================================================================= Properties */
        /// <summary>
        /// Define the allowed tree roots as semi-colon separated paths ("/Root/Global;/Root/Skins")
        /// </summary>
        public string TreeRoots { get; set; }
        /// <summary>
        /// Define the default path where to open the picker ("/Root/Global/scripts")
        /// </summary>
        public string DefaultPath { get; set; }
        /// <summary>
        /// Set the allowed content types as a semi-colon separated list of content type names ("User;Group")
        /// </summary>
        public string AllowedContentTypes { get; set; }
        /// <summary>
        /// Leave this property unset to initially show all allowed cotent types or set the default content types from the allowed ones as a semi-colon separated list of content type names ("User;Group")
        /// </summary>
        public string DefaultContentTypes { get; set; }
        /// <summary>
        /// Set the path of the target content if it determines allowed content types ("/Root/IMS")
        /// </summary>
        public string TargetPath { get; set; }
        /// <summary>
        /// Set the field name of the target content if it determines allowed content types ("Members"). Use this property together with TargetPath property.
        /// </summary>
        public string TargetField { get; set; }

        public PortletBase.PortletViewType ViewType { get; private set; }


        /* ================================================================================================================= Common constructors */
        public ContentPickerEditorPartOptions()
        {
        }
        public ContentPickerEditorPartOptions(ContentPickerCommonType commonType)
        {
            switch (commonType)
            {
                case ContentPickerCommonType.Renderer:
                    TreeRoots = "/Root/Global/renderers;/Root/System/Renderers;/Root/Global/contentviews;/Root/Skins;/Root/System/SystemPlugins;/Root";
                    AllowedContentTypes = "File;Folder";
                    ViewType = PortletBase.PortletViewType.Ascx;
                    break;
                case ContentPickerCommonType.Ascx:
                    TreeRoots = "/Root/Global/renderers;/Root/System/Renderers;/Root/Global/contentviews;/Root/Skins;/Root/System/SystemPlugins;/Root";
                    AllowedContentTypes = "File;Folder";
                    ViewType = PortletBase.PortletViewType.Ascx;
                    break;
                case ContentPickerCommonType.ContentView:
                    TreeRoots = "/Root/Global/contentviews;/Root/Skins;/Root";
                    AllowedContentTypes = "File;Folder";
                    ViewType = PortletBase.PortletViewType.Ascx;
                    break;
                case ContentPickerCommonType.ViewFrame:
                    TreeRoots = "/Root/System/SystemPlugins/ListView;/Root";
                    AllowedContentTypes = "File;Folder";
                    ViewType = PortletBase.PortletViewType.Ascx;
                    break;
                case ContentPickerCommonType.Icon:
                    TreeRoots = "/Root/Global/images/icons/16;/Root";
                    AllowedContentTypes = "File;Image;Folder";
                    break;
                case ContentPickerCommonType.Css:
                    TreeRoots = "/Root/Global/styles;/Root/Skins;/Root";
                    AllowedContentTypes = "File;Folder";
                    break;
                case ContentPickerCommonType.Js:
                    TreeRoots = "/Root/Global/scripts;/Root/Skins;/Root";
                    AllowedContentTypes = "File;Folder";
                    break;
                case ContentPickerCommonType.FirstManager:
                    TreeRoots = "/Root/IMS";
                    AllowedContentTypes = "User";
                    break;
            }
        }
        public ContentPickerEditorPartOptions(PortletBase.PortletViewType viewType) : this(ContentPickerCommonType.Renderer)
        {
            ViewType = viewType;
        }
        public ContentPickerEditorPartOptions(ContentPickerCommonType commonType, PortletBase.PortletViewType viewType)
            : this(commonType)
        {
            ViewType = viewType;
        }
    }
}
