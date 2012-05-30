using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.UI.PortletFramework
{
    public sealed class WebCategoryAttribute : Attribute
    {
        public static WebCategoryAttribute Default;
        // Fields /////////////////////////////////////////////////////////////////
        private string _title;
        private string _className;
        private string _key;
        private int _index;
        private CultureInfo _cultureInfo;

        // Constructors ///////////////////////////////////////////////////////////
        public WebCategoryAttribute()
        {
            Title = String.Empty;
        }
        public WebCategoryAttribute(string title) : this(title,0) {}
        public WebCategoryAttribute(string title, int index)
        {
            Title = title;
            Index = index;
        }
        public WebCategoryAttribute(string className, string key) :this(className, key, 0) { }
        public WebCategoryAttribute(string className, string key, int index)
        {
            Title = null;
            ClassName = className;
            Key = key;
            Index = index;
            _cultureInfo = CultureInfo.CurrentUICulture;
        }
        public WebCategoryAttribute(string className, string key, int index, string cultureInfo)
        {
            Title = null;
            ClassName = className;
            Key = key;
            Index = index;
            
            try
            {
                _cultureInfo = CultureInfo.CreateSpecificCulture(cultureInfo);
            } 
            catch(ArgumentException aex) //logged
            {
                Logger.WriteException(aex);
                _cultureInfo = CultureInfo.CurrentUICulture;
            } 
            catch(NullReferenceException nex) //logged
            {
                Logger.WriteException(nex);
                _cultureInfo = CultureInfo.CurrentUICulture;
            }

            
        }

        // Properties /////////////////////////////////////////////////////////////
        public string Title
        {
            get
            {
                if (String.IsNullOrEmpty(_title))
                {
                    var srm = SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current;
                    return srm.GetString(ClassName, Key, _cultureInfo);
                }
                return _title;
            }
            set { _title = value; }
        }

        public string ClassName
        {
            get { return _className; }
            set { _className = value; }
        }

        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }


    }
}
