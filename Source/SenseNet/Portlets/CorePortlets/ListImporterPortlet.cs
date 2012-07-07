using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.PortletFramework;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI;
using System.IO;
using SenseNet.Portal.Virtualization;
using SenseNet.ContentRepository;
using Content = SenseNet.ContentRepository.Content;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.Portlets.Controls;
using System.Data;
using SenseNet.ContentRepository.i18n;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Fields;

namespace SenseNet.Portal.Portlets
{
    public class ListImporterPortlet : ContextBoundPortlet
    {
        private FileUpload inputFile;
        private DropDownList encodingSelector;
        private Button btnImport;
        private ListView importedResultList;
        private ListView notImportedResultList;
        private CheckBox updateExisting;
        private CheckBox importNew;
        private Panel pnlResults;
        private Panel pnlError;
        private Label lblError;
        private TextBox tbExclude;

        private string contentViewPath = "/Root/System/SystemPlugins/Portlets/ListImporter/ListImporterControl.ascx";

        #region Classes

        protected struct ImportData
        {
            public string Name;
            public string Error;
            public ImportData(string name, string error)
            {
                Name = name;
                Error = error;
            }
        }
        protected class ImportResult
        {
            public bool Successful;
            public List<ImportData> Imported;
            public List<ImportData> NotImported;

            public ImportResult(bool successful, List<ImportData> imported, List<ImportData> notImported)
            {
                Successful = successful;
                Imported = imported;
                NotImported = notImported;
            }
            public ImportResult()
            {
                Successful = false;
                Imported = new List<ImportData>();
                NotImported = new List<ImportData>();
            }
        }

        #endregion

        #region Properties

        [WebBrowsable(true), Personalizable(true)]
        [LocalizedWebDisplayName(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DISPLAYNAME)]
        [LocalizedWebDescription(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DESCRIPTION)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ViewPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        [WebOrder(100)]
        public string ContentViewPath
        {
            get { return contentViewPath; }
            set { contentViewPath = value; }
        }


        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }


        #endregion


        public static string[] FieldsNotToImport = 
        {   
            "Name", 
            "Type", 
            "TypeIs", 
            "Version", 
            "Rate",
            "Binary",
            "PersonalizationSettings",
            "ImageData",
            "Image"            
        };

        public ListImporterPortlet()
        {
            this.Name = SenseNetResourceManager.Current.GetString("ListImporterPortlet", "PortletTitle");
            this.Description = SenseNetResourceManager.Current.GetString("ListImporterPortlet", "PortletDescription");
            this.Category = new PortletCategory(PortletCategoryType.ContentOperation);
        }


        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            CreateControls();
        }

        private void CreateControls()
        {
            var ctrl = Page.LoadControl(contentViewPath) as UserControl;
            Controls.Add(ctrl);

            inputFile = ctrl.FindControl("fuInputfile") as FileUpload;
            encodingSelector = ctrl.FindControl("ddlEncoding") as DropDownList;
            btnImport = ctrl.FindControl("btnImport") as Button;

            importedResultList = ctrl.FindControl("lvImported") as ListView;
            notImportedResultList = ctrl.FindControl("lvNotImported") as ListView;

            updateExisting = ctrl.FindControl("cbUpdateExisting") as CheckBox;
            importNew = ctrl.FindControl("cbImportNew") as CheckBox;

            pnlResults = ctrl.FindControl("pnlResults") as Panel;
            btnImport.Click += btnImport_Click;

            tbExclude = ctrl.FindControl("tbExclude") as TextBox;

            pnlError = ctrl.FindControl("pnlError") as Panel;
            lblError = ctrl.FindControl("lblError") as Label;
        }

        private static char DetectSplitChar(string line)
        {
            if (line.Split(';').Count() > line.Split('\t').Count())
                return ';';
            else
                return '\t';
        }

        private static List<string> SplitLine(string line, char separator)
        {
            int qc = 0;
            int prevSeparatorIndex = -1;
            var result = new List<string>();
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"') qc++;
                if (((qc % 2 == 0) && (line[i] == separator)) || i == line.Length - 1)
                {
                    var start = prevSeparatorIndex + 1;
                    int length = Math.Max(i - start, 0);

                    if (i == line.Length - 1)
                    {
                        length++;
                    }

                    result.Add(line.Substring(start, length).Trim('"'));
                    prevSeparatorIndex = i;
                }
            }
            return result;
        }

        private ImportResult DoImport(Node parent, Stream inputStream, bool update, bool import)
        {
            var successful = new List<ImportData>();
            var unsuccessful = new List<ImportData>();

            var lines = new List<Dictionary<string, string>>();
            var headers = new List<string>();
            var parentNode = parent;
            var splitChar = ';';

            using (var reader = OpenStream(inputStream))
            {
                //extracting header from stream
                if (!reader.EndOfStream)
                {
                    var header = reader.ReadLine();
                    splitChar = DetectSplitChar(header);
                    headers = SplitLine(header, splitChar);
                }

                //rewrite strem reading
                string lineString;
                var lineIndex = 1;
                while ((lineString = reader.ReadLine()) != null)
                {
                    var lineData = SplitLine(lineString, splitChar);
                    var lineDict = new Dictionary<string, string>();

                    if (lineData.Count != headers.Count)
                        throw new InvalidOperationException(string.Format(SenseNetResourceManager.Current.GetString("ListImporterPortlet", "ErrorInvalidColumns"), lineIndex));

                    for (var i = 0; i < lineData.Count; i++)
                        lineDict.Add(headers[i].Trim('"'), lineData[i].Replace("\"\"", "\"").Trim('"'));

                    lines.Add(lineDict);
                    lineIndex++;
                }
            }

            //creating content
            foreach (var line in lines)
            {
                if (!line.ContainsKey("Type"))
                    throw new Exception(String.Format(SenseNetResourceManager.Current.GetString("ListImporterPortlet", "MissingRequiredColumnFromCSV"), "Type"));

                var name = line.ContainsKey("Name") ? line["Name"] : string.Empty;
                var contentType = line["Type"];

                //excluding restricted or null valued fields (this excludes fields not related to the current type of content as well)
                var excludedFields = (from item in line
                                      where FieldsNotToImport.Contains(item.Key) || string.IsNullOrEmpty(item.Value)
                                      select item.Key).ToList();

                //removing excluded fields from the current line
                foreach (var field in excludedFields)
                {
                    line.Remove(field);
                }

                //creating, parsing and saving imported content
                var existingChildren = from c in parentNode.PhysicalChildArray select c.Name;
                var existingName = existingChildren.Contains(name);

                if (!existingName && import)
                {
                    //if new content get the content type
                    if (string.IsNullOrEmpty(contentType))
                        throw new Exception(String.Format(SenseNetResourceManager.Current.GetString("ListImporterPortlet", "MissingRequiredColumnFromCSV"), "ContentType (type)"));

                    //create new content)
                    try
                    {
                        //content type non existent fields add to list custom fields
                        var nonExistentListFields = CreateNonExistentListFields(contentType, parentNode, line);

                        //if non existent fields doesn't has # mark in name add # mark to name
                        foreach (KeyValuePair<string, string> nonExistentListField in nonExistentListFields)
                            if (!nonExistentListField.Key.Contains('#'))
                            {
                                line.Remove(nonExistentListField.Key);
                                line.Add(string.Format("#{0}", nonExistentListField.Key), nonExistentListField.Value);
                            }

                        var newContent = Content.CreateNewAndParse(contentType, parentNode, name, line);

                        //because of the empty name we need to ensure 
                        //that we do not try to use existing name
                        newContent.ContentHandler.AllowIncrementalNaming = true;

                        newContent.Save();
                        successful.Add(new ImportData(newContent.Name, ""));
                    }
                    catch (Exception e)
                    {
                        unsuccessful.Add(new ImportData(name, e.Message));
                    }
                }
                else if (existingName && update)
                {
                    //updating existing content
                    try
                    {
                        var oldNode =
                        (from c in parentNode.PhysicalChildArray where c.Name == name select c).FirstOrDefault();

                        if (oldNode == null)
                            throw new Exception(String.Format(SenseNetResourceManager.Current.GetString("ListImporterPortlet", "UpdatableNodeIsMissing"), name));

                        var nonExistentListFields = CreateNonExistentListFields(oldNode.NodeType.Name, parentNode, line);

                        //if non existent fields doesn't has # mark in name add # mark to name
                        foreach (KeyValuePair<string, string> nonExistentListField in nonExistentListFields)
                            if (!nonExistentListField.Key.Contains('#'))
                            {
                                line.Remove(nonExistentListField.Key);
                                line.Add(string.Format("#{0}", nonExistentListField.Key), nonExistentListField.Value);
                            }

                        if (Update(oldNode, line))
                            successful.Add(new ImportData(name, SenseNetResourceManager.Current.GetString("ListImporterPortlet", "UpdateSuccessfulMessage")));
                        else
                            unsuccessful.Add(new ImportData(name, SenseNetResourceManager.Current.GetString("ListImporterPortlet", "UpdateFailedMessage")));
                    }
                    catch (Exception e)
                    {
                        unsuccessful.Add(new ImportData(name, e.Message));
                    }
                }
                else
                {
                    //doing nothing
                    unsuccessful.Add(new ImportData(name, SenseNetResourceManager.Current.GetString("ListImporterPortlet", "NotImportedMessage")));
                }
            }

            return new ImportResult(unsuccessful.Count == 0, successful, unsuccessful);
        }

        private StreamReader OpenStream(Stream inputStream)
        {
            if (encodingSelector == null)
                return OpenStreamWithEncodingDetection(inputStream);

            try
            {
                string encodingName = encodingSelector.SelectedValue;
                Encoding encoding = Encoding.GetEncoding(encodingName);

                var reader = new StreamReader(inputStream, encoding);
                return reader;
            }
            catch (ArgumentException)
            {
                return OpenStreamWithEncodingDetection(inputStream);
            }
        }

        private static StreamReader OpenStreamWithEncodingDetection(Stream inputStream)
        {
            var defaultReader = new StreamReader(inputStream, true);
            return defaultReader;
        }

        private Dictionary<string, string> CreateNonExistentListFields(string contentTypeName, Node parent, Dictionary<string, string> fieldData)
        {
            var nonExistentListFields = new Dictionary<string, string>();

            if (parent is ContentList)
            {
                var cList = parent as ContentList;
                ContentType cType = ContentType.GetByName(contentTypeName);
                foreach (KeyValuePair<string, string> fData in fieldData)
                {
                    if (!IsFieldOnCTD(cType.FieldSettings, fData.Key))
                    {
                        var fSetting = new ShortTextFieldSetting
                        {
                            Name = string.Format("#{0}", fData.Key),
                            ShortName = "ShortText",
                            DisplayName = fData.Key
                        };
                        if (!IsFieldOnList(cList.FieldSettingContents, fData.Key))
                            cList.AddField(fSetting);
                        nonExistentListFields.Add(fData.Key, fData.Value);
                    }
                }
                cList.Save(SavingMode.KeepVersion);
            }
            return nonExistentListFields;
        }

        private static bool IsFieldOnCTD(IEnumerable<FieldSetting> fieldSettings, string fDataKey)
        {
            //return fieldSettings.Any(fs => fs.Name == fDataKey || fs.Name == string.Format("#{0}", fDataKey));
            return IsInFieldList(fieldSettings.Select(fs => fs.Name), fDataKey);
        }

        private static bool IsFieldOnList(IEnumerable<Node> fieldSettingContents, string fDataKey)
        {
            //return fieldSettingContents.Any(fs => fs.Name == fDataKey || fs.Name == string.Format("#{0}", fDataKey));
            return IsInFieldList(fieldSettingContents.Select(fsc => fsc.Name), fDataKey);
        }

        private static bool IsInFieldList(IEnumerable<string> fieldNameList, string fDataKey)
        {
            return fieldNameList.Any(fn => fn == fDataKey || fn == string.Format("#{0}", fDataKey));
        }

        private bool Update(Node original, Dictionary<string, string> import)
        {
            var content = Content.Create(original);
            bool success = false;

            foreach (var field in content.Fields)
            {
                if (import.ContainsKey(field.Key) && !string.IsNullOrEmpty(import[field.Key]))
                {
                    success = field.Value.Parse(import[field.Key]);
                }
            }
            content.Save();
            return success;
        }

        void btnImport_Click(object sender, EventArgs e)
        {
            if (pnlError != null)
                pnlError.Visible = false;

            var file = inputFile.PostedFile;
            var result = new ImportResult();

            //additional fields to exclude
            FieldsNotToImport = FieldsNotToImport.Concat(tbExclude.Text.Split(',')).ToArray();

            try
            {
                if (file != null)
                {
                    result = DoImport(GetBindingRoot(), file.InputStream, updateExisting.Checked,
                                      importNew.Checked);
                }

                notImportedResultList.DataSource = GetDataSource(result.NotImported);
                importedResultList.DataSource = GetDataSource(result.Imported);
                notImportedResultList.DataBind();
                importedResultList.DataBind();

                pnlResults.Visible = true;
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                SetError(ex.Message);
            }
            finally
            {
                if (file != null && file.InputStream != null)
                    file.InputStream.Close();
            }
        }

        private void SetError(string message)
        {
            pnlResults.Visible = false;

            if (pnlError == null || lblError == null) 
                return;
            
            pnlError.Visible = true;
            lblError.Text = message;
        }

        private static DataView GetDataSource(IEnumerable<ImportData> source)
        {
            var dt = new DataTable();

            dt.Columns.AddRange(new[] { 
                                        new DataColumn("Name", typeof(String)),
                                        new DataColumn("Error", typeof(String)), 
                                });
            foreach (var item in source)
            {
                dt.Rows.Add(new[]
                                {
                                    item.Name,
                                    item.Error
                                });
            }
            return dt.DefaultView;
        }
    }
}

