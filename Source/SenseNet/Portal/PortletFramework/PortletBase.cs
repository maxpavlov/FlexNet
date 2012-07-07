using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;
using SenseNet.Diagnostics;
using System.ComponentModel;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;
using System.Linq;

namespace SenseNet.Portal.UI.PortletFramework
{
    public abstract class PortletBase : WebPart, IWebEditable
    {
        public enum PortletViewType
        {
            All = 1,
            Ascx = 2,
            Xslt = 3
        }

        public static readonly string[] XmlFields = new [] { "FieldSerializationOption", "FieldNamesSerializationOption", "ActionSerializationOption" };

        private static readonly string TimerStringFormat = "Execution time of the {0} portlet was <b>{1:F10}</b>.<br />";

        [WebDisplayName("Portlet title")]
        [WebDescription("Sets the visible title of the current portlet. Title header visibility is controlled with Appearance property")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(0)]
        public String PortletTitle
        {
            get { return Title; }
            set { this.Title = value; }
        }

        [WebDisplayName("Appearance")]
        [WebDescription("Controls title and border visibility. By default both are visible")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(1)]
        public PartChromeType PortletChromeType
        {
            get
            {
                return this.ChromeType;
            }
            set { this.ChromeType = value; }
        }


        // Properties /////////////////////////////////////////////////////////////
        protected static bool ShowExecutionTime
        {
            get
            {
                if (HttpContext.Current == null)
                    return false;

                // admin csoport?
                var request = HttpContext.Current.Request;
                if (request != null && request.Params["ShowExecutionTime"] != null)
                {
                    var showExecutionTime = request.Params["ShowExecutionTime"] as string;
                    var showExecutionTimeValue = false;
                    return bool.TryParse(showExecutionTime, out showExecutionTimeValue) && showExecutionTimeValue;
                }
                return false;
            }
        }
        protected Stopwatch Timer { get; set; }
        protected bool HasError { get; set; }
        protected bool RenderTimer { get; set;}

        protected Exception RenderException { get; set; }
        
        private string _skinPreFix;
        
        // these properties is used in public interface
        [WebBrowsable(false), Personalizable(true)]
        public string Name { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Custom CSS class(es)")]
        [WebDescription("When set portlet container div appends extra css class(es)")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(1100)]
        public string SkinPreFix
        {
            get
            {
                return _skinPreFix;
            }
            set
            {
                _skinPreFix = value;
            }
        }

        public virtual RenderMode RenderingMode { get; set; }

        protected const string RENDERER_DISPLAYNAME = "RendererDisplayName";
        protected const string RENDERER_DESCRIPTION = "RendererDescription";

        protected const string FIELDS_DISPLAYNAME = "FieldsDisplayName";
        protected const string FIELDS_DESCRIPTION = "FieldsDescription";
        protected const string FIELDLIST_DISPLAYNAME = "FieldListDisplayName";
        protected const string FIELDLIST_DESCRIPTION = "FieldListDescription";
        protected const string ACTIONS_DISPLAYNAME = "ActionsDisplayName";
        protected const string ACTIONS_DESCRIPTION = "ActionsDescription";

        protected const string PORTLETFRAMEWORK_CLASSNAME = "PortletFramework";

        private string _renderer;
        [WebBrowsable(true), Personalizable(true)]
        [LocalizedWebDisplayName(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DISPLAYNAME)]
        [LocalizedWebDescription(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DESCRIPTION)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(1000)]
        [Editor(typeof(ViewPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(PortletViewType.Ascx)]
        public virtual string Renderer 
        {
            get
            {
                return _renderer;
            }
            set
            {
                _renderer = SkinManager.Resolve(value);
                if (string.IsNullOrEmpty(_renderer))
                    RenderingMode = RenderMode.Native;
                else if (_renderer.EndsWith("xslt"))
                    RenderingMode = RenderMode.Xslt;
                else
                    RenderingMode = RenderMode.Ascx;
            }
        }

        [WebBrowsable(true), Personalizable(true)]
        [LocalizedWebDisplayName(PORTLETFRAMEWORK_CLASSNAME, FIELDS_DISPLAYNAME)]
        [LocalizedWebDescription(PORTLETFRAMEWORK_CLASSNAME, FIELDS_DESCRIPTION)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(1010)]
        public virtual FieldSerializationOptions FieldSerializationOption { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [LocalizedWebDisplayName(PORTLETFRAMEWORK_CLASSNAME, FIELDLIST_DISPLAYNAME)]
        [LocalizedWebDescription(PORTLETFRAMEWORK_CLASSNAME, FIELDLIST_DESCRIPTION)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(1020)]
        [Editor(typeof(TextEditorPartField), typeof(IEditorPartField))]
        [TextEditorPartOptions(TextEditorCommonType.MiddleSize)]
        public virtual string FieldNamesSerializationOption { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [LocalizedWebDisplayName(PORTLETFRAMEWORK_CLASSNAME, ACTIONS_DISPLAYNAME)]
        [LocalizedWebDescription(PORTLETFRAMEWORK_CLASSNAME, ACTIONS_DESCRIPTION)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(1030)]
        public virtual ActionSerializationOptions ActionSerializationOption { get; set; }

        private PortletCategory _category = new PortletCategory(PortletCategoryType.Other);
        public PortletCategory Category
        {
            get { return _category; }
            set { _category = value; }
        }

        public List<string> HiddenPropertyCategories { get; set; }

        public List<string> HiddenProperties { get; set; }
        
        protected PortletBase()
        {
            if (ShowExecutionTime)
                Timer = new Stopwatch();

            this.HiddenProperties = new List<string>(XmlFields);
        }

        protected SenseNet.Portal.UI.PortletFramework.Xslt.XslTransformExecutionContext PinnedXsltContext;
        // Events /////////////////////////////////////////////////////////////////
        protected override void OnInit(EventArgs e)
        {
            if (ShowExecutionTime)
            {
                RenderTimer = true;
                Timer.Start();
            }

            base.OnInit(e);

            if (ShowExecutionTime)
                Timer.Stop();
        }

        protected override void OnLoad(EventArgs e)
        {
            if (ShowExecutionTime)
                Timer.Start();

            base.OnLoad(e);
            if (!string.IsNullOrEmpty(_renderer) && _renderer.ToLower().EndsWith("xslt"))
            {                
                if (Node.Exists(_renderer))
                {
                    PinnedXsltContext = UI.PortletFramework.Xslt.GetXslt(_renderer, true);
                }
            }

            ExportMode = WebPartExportMode.All;

            if (ShowExecutionTime)
                Timer.Stop();
        }
        protected override void CreateChildControls()
        {
            if (ShowExecutionTime)
                Timer.Start();

            base.CreateChildControls();

            if (ShowExecutionTime)
                Timer.Stop();
        }
        protected override void Render(HtmlTextWriter writer)
        {
            if (ShowExecutionTime)
                Timer.Start();

            var renderEndTag = false;

            try
            {
                if (RenderException == null)
                {
                    switch (RenderingMode)
                    {
                        case RenderMode.Native:
                            base.Render(writer);
                            break;
                        case RenderMode.Ascx:
                            RenderWithAscx(writer);
                            break;
                        case RenderMode.Xslt:
                            base.RenderBeginTag(writer);
                            renderEndTag = true;
                            RenderWithXslt(writer);
                            break;
                        case RenderMode.Debug:
                            RenderDebug(writer);
                            break;
                    }
                }
                else
                {
                    writer.Write(String.Concat(String.Concat("Portlet Error: ", RenderException.Message), RenderException.InnerException == null ? string.Empty : RenderException.InnerException.Message));
                }

                if (!ShowExecutionTime)
                    return;
            }
            catch (Exception exc) //logged
            {
                Logger.WriteException(exc);
                writer.Write(String.Concat(String.Concat("Portlet Error: ", exc.Message), exc.InnerException == null ? string.Empty : exc.InnerException.Message));
                HasError = true;
            }
            finally
            {
                if (renderEndTag)
                    base.RenderEndTag(writer);

                if (ShowExecutionTime && RenderTimer)
                {
                    Timer.Stop();
                    RenderTimerValue(writer, "PortletBase:Render");
                }
            }
        }

        // Virtuals ///////////////////////////////////////////////////////////////

        protected virtual object GetModel()
        {
            throw new NotImplementedException("This portlet doesn't support the rendering mode you have selected.");
            //return null;
        }

        protected virtual XsltArgumentList GetXsltArgumentList()
        {
            return null;
        }

        protected virtual Object SerializeModel(object model)
        {
            var serializer = new XmlSerializer(model.GetType());
            var xmldata = new MemoryStream();
            serializer.Serialize(xmldata, model);
            xmldata.Position = 0;
            return xmldata;
        }

        protected virtual void RenderWithAscx(HtmlTextWriter writer)
        {
            // This is a stub for future functionality
            throw new NotImplementedException("This portlet doesn't support the rendering mode you have selected.");
        }

        protected virtual void RenderDebug(HtmlTextWriter writer)
        {
            // This is a stub for future functionality
            throw new NotImplementedException("This portlet doesn't support the rendering mode you have selected.");
        }

        protected virtual void RenderWithXslt(HtmlTextWriter writer)
        {
            // Get the model first to check if it's available
            var model = GetModel();

            if (String.IsNullOrEmpty(Renderer))
                throw new InvalidOperationException("Renderer property is empty.");

            var xsltTransform = PinnedXsltContext ?? Xslt.GetXslt(Renderer, false);
            var xsltArguments = GetXsltArgumentList();

            var timer = Stopwatch.StartNew();
            IXPathNavigable xml = null;
            bool withNav = false;

            PrepareXsltRendering(model);
            var contentModel = model as SenseNet.ContentRepository.Content;
            if (contentModel != null && SenseNet.ContentRepository.Content.ContentNavigatorEnabled)
            {
                withNav = true;
                xml = new SenseNet.ContentRepository.Xpath.NavigableContent(contentModel);
            }
            else
            {
                xml = GetXmlModel(model);
                if (xml == null)
                    xml = GetXmlModel(SerializeModel(model));
                if (xml == null)
                    throw new InvalidOperationException("Serialized model is of unknown type");
            }
            xsltTransform.Transform(xml, xsltArguments, writer);
            timer.Stop();
            Debug.WriteLine(String.Format("#xslt> Rendering time with {0}: {1} ms ({2} ticks)", withNav ? "ContentNavigator" : "serialization", timer.ElapsedMilliseconds, timer.ElapsedTicks));
        }
        protected virtual void PrepareXsltRendering(object model)
        {
        }
        protected IXPathNavigable GetXmlModel(object model)
        {
            var xmodel = model as IXPathNavigable;
            if (xmodel != null)
                return xmodel;
            var smodel = model as Stream;
            if (smodel != null)
                return new XPathDocument(smodel);
            return null;
        }
        protected SerializationOptions GetContentSerializationOptions()
        {
            var fieldNames = string.IsNullOrEmpty(this.FieldNamesSerializationOption)
                                 ? new string[0]
                                 : this.FieldNamesSerializationOption.Split(new[] {','}, 
                                    StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

            return new SerializationOptions
            {
                Fields = this.FieldSerializationOption,
                FieldNames = fieldNames,
                Actions = this.ActionSerializationOption
            };
        }

        protected virtual void RenderTimerValue(HtmlTextWriter writer, string message)
        {

            var sb = new StringBuilder();
            sb.Append(@"<div style=""color:#fff;background:#c00;font-weight:bold,padding:2px"">");
            var msg = String.Format(TimerStringFormat, ID, Timer.Elapsed.TotalSeconds);
            if (!string.IsNullOrEmpty(message))
                msg = String.Concat(msg, "-", message);
            sb.Append(msg);
            sb.Append(@"</div>");
            writer.Write(sb.ToString());
        }

        // this is a test method
        public virtual string GetExtractedPortletInfo()
        {
            if (ExportMode == WebPartExportMode.None)
                return string.Empty;

            var result = string.Empty;
            var currentWebPartManager = WebPartManager.GetCurrentWebPartManager(Page);

            using (var sw = new StringWriter())
            {
                using (var xmlWriter = new XmlTextWriter(sw))
                    currentWebPartManager.ExportWebPart(this, xmlWriter);
                result = sw.ToString();
            }

            return result;
        }

        protected virtual EditorPartCollection GetDefaultEditorPart()
        {
            var originalEditors = base.CreateEditorParts();
            var editors = new List<EditorPart>();

            // copy original editors
            foreach (EditorPart editorPart in originalEditors)
                editors.Add(editorPart);               

            // add PropertyEditorPart as the only default editor
            var propertyEditorPart = new PropertyEditorPart();
            propertyEditorPart.ID = "PropertyEditorPartOnly";
            propertyEditorPart.HiddenCategories = this.HiddenPropertyCategories;
            propertyEditorPart.HiddenProperties = this.HiddenProperties;

            editors.Add(propertyEditorPart);

            return new EditorPartCollection(editors);

        }

        protected void CallDone()
        {
            var p = Page as PageBase;
            if (p != null)
                p.Done();
        }

        protected void CallDone(bool endResponse)
        {
            var p = Page as PageBase;
            if (p != null)
                p.Done(endResponse);
        }

        public virtual string FormatTitle(System.Globalization.CultureInfo cultureInfo, string titleFormat, string currentContentName)
        {
            return string.Format(cultureInfo, titleFormat, currentContentName);
        }

        // Static members ///////////////////////////////////////////////////////////////

        #region IWebEditable Members

        EditorPartCollection IWebEditable.CreateEditorParts()
        {
            return GetDefaultEditorPart();
        }

        object IWebEditable.WebBrowsableObject
        {
            get { return this; }
        }

        #endregion

    }
}
