using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using SenseNet.Portal.UI.PortletFramework;

namespace SenseNet.Portal.Portlets
{
    [Serializable]
    public class PagerModel
    {
        public string PortletHash;
        public int TotalCount;
        public string PortletID;
        int _top;
        int _skip;
        public ContentCollectionPortletState PortletState;

      
        public PagerModel()
        {
        }
        public PagerModel(int totalCount, ContentCollectionPortletState state, string pageurl)
        {
            this._top = state.Top;
            this._skip = state.Skip;
            this.PortletState = state;
            this.TotalCount = totalCount - PortletState.SkipFirst;
            this.PortletHash = PortletState.PortletHash;
        }
        public int CurrentPage
        {
            get
            {
                if (_top == 0) return 0;
                return (int)Math.Round((double)((_skip - PortletState.SkipFirst) / _top)) + 1;
            }
            set
            {
            }
        }

        public int Pagecount
        {
            get
            {
                if (_top == 0) return 0;
                return (int)Math.Ceiling(TotalCount / (double)_top);
            }
            set
            {
            }
        }

        [XmlIgnore]
        public IEnumerable<GoToPageAction> PagerActions
        {
            get
            {
                for (int i = 0; i < Pagecount; i++)
                {
                    yield return new GoToPageAction()
                    {
                        CurrentlyActive = ( (i + 1) == CurrentPage),
                        Skip = i * _top,
                        PageNumber = i + 1,
                        Portlet = (ContentCollectionPortlet)PortletState.Portlet
                    };
                }
            }
            set
            {
            }
        }

        [XmlArray("GoToPageActions")]
        [XmlArrayItem("GoToPage")]
        public GoToPageAction[] Actions
        {
            get
            {
                return PagerActions.ToArray();
            }
            set
            {
            }
        }
    }

}
