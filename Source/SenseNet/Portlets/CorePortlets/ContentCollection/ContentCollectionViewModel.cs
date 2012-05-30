using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using System.Xml.Serialization;
using SenseNet.Portal.Virtualization;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Portal.Portlets
{
    //TODO rename
    [XmlRoot("Model")]
    public class ContentCollectionViewModel : ContentViewModel
    {
        
        [XmlIgnore]
        public IEnumerable<Content> Items
        {
            get
            {
                if (this.Content == null)
                    return new List<Content>();

                if (string.IsNullOrEmpty(this.ReferenceAxisName))
                {
                    return this.Content.Children;
                }
                else
                {
                    var items = this.Content[ReferenceAxisName] as IEnumerable<Node>;
                    if (items == null)
                        return null;

                    return items.Select(n => Content.Create(n));
                }
            }
        }

        [XmlIgnore]
        public string ReferenceAxisName { get; set; }

        public PagerModel Pager { get; set; }

        public ContentCollectionPortletState State { get; set; }

        [XmlArray("VisibleFieldNames")]
        [XmlArrayItem("FieldName")]
        public string[] VisibleFieldNames
        {
            get
            {
                return State.VisibleFieldNames;
            }
            set
            {
            }
        }
        [XmlArray("SortActions")]
        [XmlArrayItem("Sort")]
        public SortByColumnAction[] FieldSortActions
        {
            get
            {
                return SortActions.ToArray();
            }
            set
            {
            }
        }

        [XmlIgnore]
        public IEnumerable<SortByColumnAction> SortActions
        {
            get
            {
                foreach (var field in State.VisibleFieldNames)
                {
                    yield return new SortByColumnAction()
                    {
                        Portlet = (ContentCollectionPortlet)State.Portlet,
                        SortColumn = field,
                        SortDescending = false
                    };
                    yield return new SortByColumnAction()
                    {
                        Portlet = (ContentCollectionPortlet)State.Portlet,
                        SortColumn = field,
                        SortDescending = true
                    };

                }
            }
        }
    }
}
