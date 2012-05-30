using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using SenseNet.Portal.Virtualization;
using System.Web;

namespace SenseNet.Portal.Portlets
{
    //TODO rename
    public abstract class PagerAction
    {
        public virtual string Url
        {
            get
            {
                return ToUrl();
            }

            set
            {
            }


        }
 
        protected abstract string ToUrl();
    }

    [XmlRoot("GoToPage")]
    public class GoToPageAction : PagerAction
    {
        public int Skip { get; set; }
        public int PageNumber { get; set; }
        public bool CurrentlyActive 
        { 
            get; 
            set; 
        }

        [NonSerialized]
        [XmlIgnore]
        public ContentCollectionPortlet Portlet;

        protected override string ToUrl()
        {
            var queryString = new StringBuilder("?");
            var skipParamName = Portlet.GetPortletSpecificParamName("Skip");
            var skipAdded = false;

            foreach (var param in HttpContext.Current.Request.QueryString.AllKeys)
            {
                if (queryString.Length > 1)
                    queryString.Append("&");

                if (param == skipParamName)
                {
                    queryString.AppendFormat("{0}={1}", skipParamName, Skip);
                    skipAdded = true;
                }
                else
                    queryString.AppendFormat("{0}={1}", param, HttpUtility.UrlEncode(HttpContext.Current.Request.QueryString[param]));
            }

            if (!skipAdded)
            {
                if (queryString.Length > 1)
                    queryString.Append("&");

                queryString.AppendFormat("{0}={1}", skipParamName, Skip);
            }

            return queryString.ToString();
        }
    }

    [XmlRoot("Sort")]
    public class SortByColumnAction : PagerAction
    {
        [XmlElement("Column")]
        public string SortColumn { get; set; }
        [XmlElement("Descending")]
        public bool SortDescending { get; set; }
        

        [NonSerialized]
        [XmlIgnore]
        public ContentCollectionPortlet Portlet;

        protected override string ToUrl()
        {
            var colpart = Portlet.GetPropertyActionUrlPart("SortColumn", SortColumn);
            var orderPart = Portlet.GetPropertyActionUrlPart("SortDescending", SortDescending.ToString());
            return "?" + colpart + "&" + orderPart;
        }
    }


}
