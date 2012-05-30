using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Diagnostics;
using SenseNet.Search;

namespace SenseNet.Portal.UI.ContentListViews
{
    public class ListHelper
    {
        public static string RenderCell(string fieldFullName, string contentListPath)
        {
            if (string.IsNullOrEmpty(fieldFullName))
                return string.Empty;

            try
            {
                var bindingName = FieldSetting.GetBindingNameFromFullName(fieldFullName);
                FieldSetting fieldSetting;
                var pathList = GetCellTemplatePaths(fieldFullName, contentListPath, out fieldSetting);
                if (pathList.Count > 0)
                {
                    //get the template with the system account
                    using (new SystemAccount())
                    {
                        foreach (var templatePath in pathList)
                        {
                            var actualPath = SkinManager.Resolve(templatePath);
                            if (!Node.Exists(actualPath))
                                continue;

                            var template = Node.Load<File>(actualPath);
                            if (template == null) 
                                continue;

                            //replace the template parameters
                            var templateString = Tools.GetStreamString(template.Binary.GetStream())
                                .Replace("@@bindingName@@", bindingName)
                                .Replace("@@fullName@@", fieldFullName);

                            if (fieldSetting != null)
                                templateString = templateString.Replace("@@fieldName@@", fieldSetting.Name);

                            return templateString;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }

            //default behavior: simple text rendering
            return string.Format("<%# Eval(\"{0}\") %>", FieldSetting.GetBindingNameFromFullName(fieldFullName));
        }

        public static string GetRunningWorkflowsText(Node relatedContent)
        {
            if (!StorageContext.Search.IsOuterEngineEnabled)
                return string.Empty;

            var cl = ContentList.GetContentListForNode(relatedContent);
            if (cl == null)
                return string.Empty;

            var query = string.Format("+InTree:\"{0}\" +TypeIs:Workflow +WorkflowStatus:1 +RelatedContent:{1} .AUTOFILTERS:OFF .LIFESPAN:OFF",
                                          cl.Path + "/Workflows", relatedContent.Id);

            var result = ContentQuery.Query(query);
            var sb = new StringBuilder();

            foreach (var wfInstance in result.Nodes)
            {
                if (sb.Length > 0)
                    sb.Append(", ");

                sb.Append(wfInstance.DisplayName);
            }

            return sb.ToString();
        }

        public static string GetPathList(Content content, string fieldName)
        {
            return GetPathList(content, fieldName, ';');
        }

        public static string GetPathList(Content content, string fieldName, char separator)
        {
            var sb = new StringBuilder();
            if (content == null || string.IsNullOrEmpty(fieldName) || !content.Fields.ContainsKey(fieldName))
                return string.Empty;

            var refData = content[fieldName];
            var references = refData as IEnumerable<Node>;
            if (references == null)
            {
                var node = refData as Node;
                if (node != null)
                    references = new List<Node>(new[] {node});
                else
                    return string.Empty;
            }

            foreach (var refNode in references)
            {
                sb.AppendFormat("{0}{1}", refNode.Path, separator);
            }

            return sb.ToString().TrimEnd(separator);
        }

        private static List<string> GetCellTemplatePaths(string fieldFullName, string contentListPath, out FieldSetting fieldSetting)
        {
            //Path list examples:
            //Normal field:
            //"/Root/Sites/Default_Site/workspaces/Sales/chicagosalesworkspace/Document_Library/CellTemplates/DisplayName.ascx"
            //"/Root/Sites/Default_Site/workspaces/Sales/chicagosalesworkspace/Document_Library/CellTemplates/ShortTextField.ascx"
            //"$skin/celltemplates/DisplayName.ascx"
            //"$skin/celltemplates/ShortTextField.ascx"
            //"$skin/celltemplates/Generic.ascx"

            //Content List field
            //"/Root/Sites/Default_Site/workspaces/Sales/chicagosalesworkspace/Document_Library/CellTemplates/MyField1.ascx"
            //"/Root/Sites/Default_Site/workspaces/Sales/chicagosalesworkspace/Document_Library/CellTemplates/ShortTextField.ascx"
            //"$skin/celltemplates/ShortTextField.ascx"
            //"$skin/celltemplates/Generic.ascx"

            fieldSetting = null;
            var pathList = new List<string>();

            if (!string.IsNullOrEmpty(fieldFullName))
            {
                var listTemplateFolderPath = !string.IsNullOrEmpty(contentListPath)
                                                 ? RepositoryPath.Combine(contentListPath, "CellTemplates")
                                                 : string.Empty;

                string fieldName;
                fieldSetting = FieldSetting.GetFieldSettingFromFullName(fieldFullName, out fieldName);

                if (!string.IsNullOrEmpty(fieldName))
                {   
                    //field name template path
                    if (!string.IsNullOrEmpty(listTemplateFolderPath))
                    {
                        pathList.Add(fieldName.StartsWith("#")
                                         ? RepositoryPath.Combine(listTemplateFolderPath, fieldName.Remove(0, 1) + ".ascx")
                                         : RepositoryPath.Combine(listTemplateFolderPath, fieldName + ".ascx"));
                    }
                }

                //list field
                if (fieldSetting == null && !string.IsNullOrEmpty(fieldName) && fieldName.StartsWith("#") && !string.IsNullOrEmpty(contentListPath))
                {
                    var cl = Node.Load<ContentList>(contentListPath);
                    if (cl != null)
                    {
                        var fsNode = cl.FieldSettingContents.FirstOrDefault(f => f.Name == fieldName) as FieldSettingContent;
                        if (fsNode != null)
                            fieldSetting = fsNode.FieldSetting;
                    }
                }

                //field type template path
                if (fieldSetting != null)
                {
                    var fieldTypeName = fieldSetting.FieldClassName;
                    if (!string.IsNullOrEmpty(fieldTypeName))
                    {
                        var pointIndex = fieldTypeName.LastIndexOf('.');
                        if (pointIndex >= 0)
                            fieldTypeName = fieldTypeName.Substring(pointIndex + 1);

                        if (fieldName != null)
                        {
                            if (!string.IsNullOrEmpty(listTemplateFolderPath))
                            {
                                //try with field type name in the cell template folder under the content list
                                pathList.Add(RepositoryPath.Combine(listTemplateFolderPath, fieldTypeName + ".ascx"));
                            }

                            if (!fieldName.StartsWith("#"))
                            {
                                //normal field: add the skin-relative 'field name path'
                                pathList.Add(RepositoryPath.Combine(Repository.CellTemplatesPath, fieldName + ".ascx"));
                            }
                        }

                        //add the skin-relative 'type path' 
                        pathList.Add(RepositoryPath.Combine(Repository.CellTemplatesPath, fieldTypeName + ".ascx"));
                    }
                }
            }

            //add the generic cell template path
            pathList.Add(RepositoryPath.Combine(Repository.CellTemplatesPath, "Generic.ascx"));

            return pathList;
        }
    }
}
