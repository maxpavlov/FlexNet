using System;
using SenseNet.ContentRepository;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.ContentRepository.Schema;
using System.Linq;

namespace SenseNet.Portal.Portlets
{
    public class FieldMoverPortlet : ContextBoundPortlet
    {
        public FieldMoverPortlet()
        {
            this.Name = "Field mover";
            this.Description = "This portlet moves a field on a content list to the specified direction";
            this.Category = new PortletCategory(PortletCategoryType.ContentOperation);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            var fieldName = Page.Request.Params["FieldName"];
            var direction = Page.Request.Params["Direction"];

            string prevField = null;
            string currentField = null;
            string nextField = null;

            var found = false;
            var hasNext = false;

            var contentList = ContextNode as ContentList;
            if (contentList == null)
                return;

            var sortedFieldSettings = from fs in contentList.FieldSettingContents.ToList().Cast<FieldSettingContent>() 
                                      orderby fs.FieldIndex 
                                      select fs;

            //filling index values on all fields
            var index = 1;
            foreach (var fieldSetting in sortedFieldSettings)
            {
                if (!fieldSetting.Name.StartsWith("#")) 
                    continue;

                fieldSetting.FieldIndex = index;

                //Collecting relevant field names
                if (!found)
                {
                    prevField = currentField;
                    currentField = fieldSetting.Name; 
                }

                if (fieldSetting.Name == fieldName)
                {
                    found = true;
                }

                if (found && !hasNext && currentField != fieldSetting.Name)
                {
                    hasNext = true;
                    nextField = fieldSetting.Name;
                }

                index++;
            }

            //moving selected field
            switch (direction)
            {
                case "Up":
                    if (!String.IsNullOrEmpty(prevField) && !String.IsNullOrEmpty(currentField))
                    {
                        foreach (var fieldSetting in sortedFieldSettings)
                        {
                            if (fieldSetting.Name == prevField)
                            {
                                fieldSetting.FieldIndex++;
                            }
                            if (fieldSetting.Name == currentField)
                            {
                                fieldSetting.FieldIndex--;
                            }
                        }
                    }
                    break;
                case "Down":
                    if (!String.IsNullOrEmpty(nextField) && !String.IsNullOrEmpty(currentField))
                    {
                        foreach (var fieldSetting in sortedFieldSettings)
                        {
                            if (fieldSetting.Name == currentField)
                            {
                                fieldSetting.FieldIndex++;
                            }
                            if (fieldSetting.Name == nextField)
                            {
                                fieldSetting.FieldIndex--;
                            }
                        }

                    }
                    break;
            }

            contentList.UpdateContentListDefinition(sortedFieldSettings);

            ContextNode.Save();

            CallDone();
        }
    }
}
