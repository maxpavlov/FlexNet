using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using SenseNet.ContentRepository.Schema;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository;

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


        /* ========================================================================================= Methods */
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            UITools.AddScript("$skin/scripts/sn/SN.AllowedChildTypes.js");
        }
        protected override void OnPreRender(EventArgs e)
        {
            string jsonData;

            using (var s = new MemoryStream())
            {
                var workData = ContentType.GetContentTypes().Select(n => new ContentTypeItem { value = n.Name, label = n.DisplayName, path = n.Path, icon = n.Icon }).OrderBy(n => n.label);
                var serializer = new DataContractJsonSerializer(typeof(ContentTypeItem[]));
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
            var control = this.InnerControl;
            if (control == null)
                return;

            // if empty, set types defined on CTD
            string contentTypeNames = string.Empty;
            if (contentTypes == null || contentTypes.Count() == 0)
            {
                contentTypeNames = string.Join(" ", CTDContentTypeNames.OrderBy(t => t));
            }
            else
            {
                contentTypeNames = string.Join(" ", contentTypes.Select(t => t.Name).OrderBy(t => t));
            }

            control.Text = contentTypeNames;
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
    }
}
