using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.UI;
using SenseNet.ContentRepository.Schema;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.UI.Controls
{
    public class AllowedChildTypes : FieldControl
    {

        [DataContract]
        private class ContentTypeItem
        {
            [DataMember]
            public string label { get; set; }
            [DataMember]
            public string value { get; set; }
            [DataMember]
            public string path { get; set; }
            [DataMember]
            public string icon { get; set; }
        }

        
        /* ========================================================================================= Members */
        private IEnumerable<ContentType> _contentTypes;


        /* ========================================================================================= Properties */
        private const string INNERDATAID = "InnerData";
        private TextBox InnerControl
        {
            get
            {
                return this.FindControlRecursive(INNERDATAID) as TextBox;
            }
        }

        private List<string> CTDContentTypeNames
        {
            get
            {
                return this.Content.ContentType.AllowedChildTypeNames.ToList();
            }
        }

        public bool ReadOnlyMode
        {
            get
            {
                return this.ReadOnly || this.Field.ReadOnly || this.ControlMode == FieldControlControlMode.Browse;
            }
        }

        /* ========================================================================================= Methods */
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            UITools.AddScript("$skin/scripts/sn/SN.AllowedChildTypes.js");
            UITools.AddStyleSheetToHeader(UITools.GetHeader(), "$skin/styles/icons.css");
        }
        protected override void OnPreRender(EventArgs e)
        {
            if (!this.ReadOnlyMode && !UseBrowseTemplate)
            {
                string jsonData;

                using (var s = new MemoryStream())
                {
                    var workData = ContentType.GetContentTypes()
                        .Select(n => new ContentTypeItem {value = n.Name, label = n.DisplayName, path = n.Path, icon = n.Icon})
                        .OrderBy(n => n.label);

                    var serializer = new DataContractJsonSerializer(typeof (ContentTypeItem[]));
                    serializer.WriteObject(s, workData.ToArray());
                    s.Flush();
                    s.Position = 0;
                    using (var sr = new StreamReader(s))
                    {
                        jsonData = sr.ReadToEnd();
                    }
                }

                // init control happens in prerender to handle postbacks (eg. pressing 'inherit from ctd' button)
                var contentTypes = _contentTypes ?? GetContentTypesFromControl();
                InitControl(contentTypes);
                var inherit = contentTypes == null || contentTypes.Count() == 0 ? 0 : 1;

                UITools.RegisterStartupScript("initdropboxautocomplete", string.Format("SN.ACT.init({0},{1})", jsonData, inherit), this.Page);
            }

            base.OnPreRender(e);
        }
        public override object GetData()
        {
            _contentTypes = GetContentTypesFromControl();
            return _contentTypes;
        }
        public override void SetData(object data)
        {
            var contentTypes = data as IEnumerable<ContentType>;
            InitControl(contentTypes);
        }
        private void InitControl(IEnumerable<ContentType> contentTypes)
        {
            // if empty, set types defined on CTD
            string contentTypeNames;
            if (contentTypes == null || contentTypes.Count() == 0)
            {
                contentTypeNames = string.Join(" ", CTDContentTypeNames.OrderBy(t => t));
            }
            else
            {
                contentTypeNames = string.Join(" ", contentTypes.Select(t => t.Name).OrderBy(t => t));
            }

            var editControl = this.InnerControl;
            if (editControl != null)
                editControl.Text = contentTypeNames;

            var browseControl = GetInnerControl() as Label;
            if (browseControl != null)
                browseControl.Text = contentTypeNames;
        }
        private IEnumerable<ContentType> GetContentTypesFromControl()
        {
            var control = this.InnerControl;
            if (control == null)
                return null;

            var contentTypeNames = control.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            // check if list is the same as defined in CTD
            var ctdContentTypeNames = this.CTDContentTypeNames;
            if (contentTypeNames.Count == ctdContentTypeNames.Count)
            {
                var equal = string.Join(" ", contentTypeNames.OrderBy(t => t)) == string.Join(" ", ctdContentTypeNames.OrderBy(t => t));
                if (equal)
                {
                    return null;
                }
            }

            var contentTypes = contentTypeNames.Select(name => ContentType.GetByName(name));
            return contentTypes;
        }

        public Control GetInnerControl() { return this.FindControlRecursive(InnerControlID); }
    }
}
