
using System.Web;
using SenseNet.Portal.UI.PortletFramework;
using System.Linq;
using System;
using System.Xml.Serialization;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.Portlets
{
    [Serializable]
    public class ContentCollectionPortletState : PortletState
    {
        public virtual int SkipFirst
        {
            get { return Portlet.SkipFirst; }
            set { throw new NotImplementedException("You must override this property if you want to set the value"); }
        }
        
        public string PortletHash
        {
            get { return Math.Abs((PortalContext.Current.ContextNode.Path + Portlet.ID).GetHashCode()).ToString(); }
        }

        public ContentCollectionPortletState() { }
        private bool GetIntFromRequest(string paramName, out int value)
        {
            value = 0;

            if (!HttpContext.Current.Request.Params.AllKeys.Contains(paramName))
                return false;
            var svalue = HttpContext.Current.Request.Params[paramName];
            if (string.IsNullOrEmpty(svalue))
                return false;
            return int.TryParse(svalue, out value);

        }

        private bool GetStringFromRequest(string paramName, out string value)
        {
            value = string.Empty;

            if (!HttpContext.Current.Request.Params.AllKeys.Contains(paramName))
                return false;
            var svalue = HttpContext.Current.Request.Params[paramName];
            if (string.IsNullOrEmpty(svalue))
                return false;
            value = svalue;
            return true;

        }

        private bool GetBoolFromRequest(string paramName, out bool value)
        {
            value = false;

            if (!HttpContext.Current.Request.Params.AllKeys.Contains(paramName))
                return false;
            var svalue = HttpContext.Current.Request.Params[paramName];
            if (string.IsNullOrEmpty(svalue))
                return false;
            return bool.TryParse(svalue, out value);

        }

        [XmlIgnore]
        private new ContentCollectionPortlet Portlet
        {
            get
            {
                return base.Portlet as ContentCollectionPortlet;
            }
        }

        protected int? _top;
        protected int? _skip;

        private bool _dirty;
        public bool IsDirty { get { return _dirty; } }

        public override void CollectValues()
        {

            int skip;

            if (GetIntFromRequest(Portlet.GetPortletSpecificParamName("Skip"), out skip))
            {
                Skip = Portlet.SkipFirst != 0 ? skip + Portlet.SkipFirst : skip;
            }

            string sortCol;
            if (GetStringFromRequest(Portlet.GetPortletSpecificParamName("SortColumn"), out sortCol))
            {
                _sortColumn = sortCol;
            }

            bool sortDesc;
            if (GetBoolFromRequest(Portlet.GetPortletSpecificParamName("SortDescending"), out sortDesc))
            {
                _sortDescending = sortDesc;
            }

        }

        public ContentCollectionPortletState(PortletBase portlet)
            : base(portlet)
        {

        }

        public string[] VisibleFieldNames
        {
            get
            {
                return this.Portlet.VisisbleFieldNames;
            }

        }
        public int Top
        {
            get
            {
                return _top.HasValue ? _top.Value : Portlet.Top;
            }
            set
            {
                if (!_top.HasValue || _top.Value != value)
                    _dirty = true;
                _top = value;
            }
        }
        public int Skip
        {
            get { return _skip.HasValue ? _skip.Value : Portlet.SkipFirst; }
            set
            {
                if (!_skip.HasValue || _skip.Value != value)
                    _dirty = true;
                _skip = value;
            }
        }


        protected string _sortColumn = null;
        protected bool? _sortDescending;

        public string SortColumn
        {
            get
            {
                return _sortColumn != null ? _sortColumn : Portlet.SortBy;
            }
            set
            {

                if (_sortColumn == null || _sortColumn != value)
                    _dirty = true;
                _sortColumn = value;
            }
        }
        public bool SortDescending
        {
            get
            {
                return _sortDescending.HasValue ? _sortDescending.Value : Portlet.SortDescending;
            }
            set
            {
                if (!_sortDescending.HasValue || _sortDescending.Value != value)
                    _dirty = true;
                _sortDescending = value;
            }
        }
    }
}
