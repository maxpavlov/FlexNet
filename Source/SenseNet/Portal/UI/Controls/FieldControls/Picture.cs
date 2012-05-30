//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Web.UI;
//using SenseNet.ContentRepository.Fields;
//using SenseNet.ContentRepository.Storage;
//using System.Web.UI.WebControls;
//using System.ComponentModel;

//namespace SenseNet.Portal.UI.Controls
//{
//    [ToolboxData("<{0}:Picture ID=\"Picture1\" runat=server></{0}:Picture>")]
//    public class Picture : FieldControl, INamingContainer, ITemplateFieldControl
//    {
//        // Members //////////////////////////////////////////////////////////////////////
//        private readonly System.Web.UI.WebControls.Image _imageControl;
//        private ImageAlign _imageAlign;
//        [PersistenceMode(PersistenceMode.Attribute)]
//        [DefaultValue(ImageAlign.NotSet)]
//        public ImageAlign ImageAlign
//        {
//            get { return _imageAlign; }
//            set { _imageAlign = value; }
//        }
//        [PersistenceMode(PersistenceMode.Attribute)]
//        public string AlternateText { get; set; }

//        // Constructor //////////////////////////////////////////////////////////////////
//        public Picture() { _imageControl = new System.Web.UI.WebControls.Image {ID = InnerControlID}; }

//        // Methods //////////////////////////////////////////////////////////////////////
//        public override object GetData()
//        {
//            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
//                return _imageControl.ImageUrl.Length > 0 ? Node.LoadNode(_imageControl.ImageUrl) : null;

//            var innerControl = GetInnerControl() as System.Web.UI.WebControls.Image;
//            if (innerControl == null)
//                return _imageControl.ImageUrl.Length > 0 ? Node.LoadNode(_imageControl.ImageUrl) : null; // simulate default process
//            return innerControl.ImageUrl.Length > 0 ? Node.LoadNode(innerControl.ImageUrl) : null;
//        }
//        public override void SetData(object data)
//        {
//            var dataNode = data as Node;
//            if (dataNode == null) 
//                return;
            
//            _imageControl.ImageUrl = Convert.ToString(dataNode.Path);
            
//            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
//                return;
//            var title = GetLabelForTitleControl() as Label;
//            var desc = GetLabelForDescription() as Label;
//            var innerControl = GetInnerControl() as System.Web.UI.WebControls.Image;
//            if (title != null) title.Text = Field.DisplayName;
//            if (desc != null) desc.Text = Field.Description;
//            if (innerControl != null) innerControl.ImageUrl = Convert.ToString(dataNode.Path);

//        }
//        protected override void OnInit(EventArgs e)
//        {
//            base.OnInit(e);
//            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
//                return;

//            #region original flow

//            _imageControl.CssClass = CssClass;
//            if (_imageAlign != ImageAlign.NotSet) _imageControl.ImageAlign = _imageAlign;
//            if (!Width.IsEmpty) _imageControl.Width = Width;
//            if (!Height.IsEmpty) _imageControl.Height = Height;

//            #endregion

//            //Controls.Add(_imageControl);
//        }
//        protected override void RenderContents(HtmlTextWriter writer)
//        {

//            #region template
//            if (UseBrowseTemplate)
//            {
//                base.RenderContents(writer);
//                return;
//            }
//            if (UseEditTemplate)
//            {
//                ManipulateTemplateControls();
//                base.RenderContents(writer);
//                return;
//            }
//            if (UseInlineEditTemplate)
//            {
//                ManipulateTemplateControls();
//                base.RenderContents(writer);
//                return;
//            }
//            #endregion

//            #region original flow

//            //_imageControl.CssClass = this.CssClass;
//            //if (_imageAlign != ImageAlign.NotSet)
//            //    _imageControl.ImageAlign = this._imageAlign;
//            //if(this.Width != null)
//            //    _imageControl.Width = this.Width;
//            //if (this.Height != null)
//            //    _imageControl.Height = this.Height;
//            if (_imageAlign != ImageAlign.NotSet) _imageControl.ImageAlign = _imageAlign;
//            if (!Width.IsEmpty) _imageControl.Width = Width;
//            if (!Height.IsEmpty) _imageControl.Height = Height;
//            _imageControl.RenderControl(writer);
//            base.RenderContents(writer);

//            #endregion
//        }

//        private void ManipulateTemplateControls()
//        {
//            var ic = GetInnerControl() as Image;
//            if (ic == null) return;
        
//        }

//        #region ITemplatedFieldControl members 

//        public Control GetInnerControl() { return this.FindControlRecursive(InnerControlID); }
//        public Control GetLabelForDescription() { return this.FindControlRecursive(DescriptionControlID); }
//        public Control GetLabelForTitleControl() { return this.FindControlRecursive(TitleControlID); }

//        #endregion
//    }
//}