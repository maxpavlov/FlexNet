using System.Collections.Generic;
using System.Web;
using SenseNet.ApplicationModel;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.Virtualization;
using SenseNet.ContentRepository.Schema;
using System.Xml;
using System;
using System.Linq;
using System.Text;
using SenseNet.Search;

namespace SenseNet.Services
{
    [ContentHandler]
    public class ExportToCsvApplication : Application, IHttpHandler
    {
        public ExportToCsvApplication(Node parent) : this(parent, null) { }
        public ExportToCsvApplication(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected ExportToCsvApplication(NodeToken nt) : base(nt) { }

        //============================================================================================= Properties

        private static readonly string[] YESVALUES = new[] {"1", "yes", "true"};
        protected bool ExportSystemContent
        {
            get 
            { 
                var sc = HttpContext.Current.Request.QueryString["system"];
                return !string.IsNullOrEmpty(sc) && YESVALUES.Contains(sc.Trim().ToLower());
            }
        }

        protected string ExportType
        {
            get
            {
                var et = HttpContext.Current.Request.QueryString["type"];
                return et == null ? string.Empty : et.ToLower();
            }
        }

        private Content _contentToExport;
        protected Content ExportContent
        {
            get
            {
                if (_contentToExport == null)
                {
                    _contentToExport = Content.Create(PortalContext.Current.ContextNode);

                    //include system content or not
                    if (ExportSystemContent)
                    {
                        if (_contentToExport.ChildrenQuerySettings == null)
                            _contentToExport.ChildrenQuerySettings = new QuerySettings { EnableAutofilters = false };
                        else
                            _contentToExport.ChildrenQuerySettings.EnableAutofilters = false;
                    }
                }

                return _contentToExport;
            }
        }

        //============================================================================================= IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Clear();
            context.Response.ContentType = "Application/x-msexcel";

            var fileName = string.Format("{0}_{1}", PortalContext.Current.ContextNode.Name, DateTime.Now.ToString("yyyy-MM-dd_HHmm"));
            var b = new byte[] { 0xEF, 0xBB, 0xBF };

            context.Response.AddHeader("content-disposition", "attachment; filename=\"" + fileName + ".csv" + "\"");
            context.Response.Charset = "65001";
            context.Response.BinaryWrite(b);
            context.Response.Write(ToCsv());
            context.Response.End();
        }

        //============================================================================================= Helper methods

        private string ToCsv()
        {
            var result = new StringBuilder();

            var header = CreateHeader();

            //writing header
            result.AppendLine("\"" + string.Join("\";\"", header) + "\"");

            //for all children of the current content)
            foreach (var content in ExportContent.Children)
            {
                var cXml = new XmlDocument();
                cXml.Load(content.GetXml());

                //for all fields which the content may contain)
                foreach (var fieldName in header)
                {
                    if (content.Fields.ContainsKey(fieldName)
                        && content.Fields[fieldName].HasValue())
                    {
                        XmlNode fieldNode = null;
                        try
                        {
                            fieldNode = cXml.DocumentElement.SelectSingleNode("/Content/Fields/" + fieldName);
                        }
                        catch (System.Xml.XPath.XPathException)
                        {
                            fieldNode = null;
                        }

                        var fieldValue = fieldNode != null ? fieldNode.InnerText.Replace("\"", "\"\"") : content[fieldName].ToString().Replace("\"", "\"\"");

                        //to avoid coding errors in survey rules...
                        if (fieldName == "ContentListDefinition")
                        {
                            fieldValue = HttpUtility.HtmlDecode(fieldValue);
                            fieldValue = fieldValue.Replace("&amp;gt;", ">");
                            fieldValue = fieldValue.Replace("&amp;lt;", "<");
                            fieldValue = fieldValue.Replace("&gt;", ">");
                            fieldValue = fieldValue.Replace("&lt;", "<");
                        }
                        result.Append("\"" + fieldValue + "\"");
                    }

                    //inserting separator or line break
                    if (header.IndexOf(fieldName) < header.Count - 1)
                    {
                        result.Append(";");
                    }
                    else
                    {
                        result.AppendLine();
                    }
                }
            }

            return result.ToString();
        }

        private List<string> CreateHeader()
        {
            switch (ExportType)
            {
                case "visible": 
                    return GetVisibleFieldNames();
                default:
                    return GetAllFieldNames();
            }
        }

        private List<string> GetAllFieldNames()
        {
            var result = new List<string>();

            if (ExportContent.ChildCount > 0)
            {
                foreach (var content in ExportContent.Children)
                {
                    foreach (var field in content.Fields.Where(field => !result.Contains(field.Key)))
                    {
                        result.Add(field.Key);
                    }
                }
            }
            else
            {
                foreach (var field in ExportContent.Fields.Where(field => !result.Contains(field.Key)))
                {
                    result.Add(field.Key);
                }

                var contentList = ExportContent.ContentHandler as ContentList;
                if (contentList != null)
                {
                    foreach (var field in contentList.FieldSettingContents.Where(field => !result.Contains(field.Name)))
                    {
                        result.Add(field.Name);
                    }
                }
            }

            return result;
        }

        private List<string> GetVisibleFieldNames()
        {
            var availableFieldNames = new List<string> {"Name", "Type"};

            //get leaf settings to determine visibility using the most granted mode
            var gc = ExportContent.ContentHandler as GenericContent;
            if (gc == null)
                return availableFieldNames;

            var leafFieldSettings = gc.GetAvailableFields(false);

            foreach (var fieldSetting in leafFieldSettings)
            {
                var fs = fieldSetting;

                while (fs != null)
                {
                    if (fs.VisibleBrowse != FieldVisibility.Hide ||
                        fs.VisibleEdit != FieldVisibility.Hide ||
                        fs.VisibleNew != FieldVisibility.Hide)
                    {
                        //add field name if it was not added before
                        if (!string.IsNullOrEmpty(fs.Name) && !availableFieldNames.Contains(fs.Name))
                            availableFieldNames.Add(fs.Name);

                        break;
                    }

                    fs = fs.ParentFieldSetting;
                }
            }

            return availableFieldNames;
        }
    }
}
