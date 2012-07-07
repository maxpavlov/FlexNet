using System;
using System.Runtime.Serialization;
using SenseNet.ContentRepository;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.ApplicationModel
{
    public abstract class ActionBase
    {
        public static readonly string BackUrlParameterName = "back";

        //================================================================================= Virtual methods

        public virtual void Initialize(Content context, string backUri,
            Application application, object parameters)
        {
            this.Application = application;
            this.Content = context;
            this.BackUri = backUri;
            this.Parameters = parameters;

            if (application != null)
            {
                this.Name = application.AppName;
                this.Text = application.DisplayName;

                if (!string.IsNullOrEmpty(application.Icon))
                    Icon = application.Icon;
            }

            if (this.Content == null || string.IsNullOrEmpty(this.Name))
                return;

            if (application != null && application.IncludeBackUrl != IncludeBackUrlMode.Default)
            {
                //value is set in the application
                this.IncludeBackUrl = (application.IncludeBackUrl == IncludeBackUrlMode.True);
            }
            else
            {
                //hide back url for Browse action by default
                if (this.Name.ToLower() == "browse")
                    this.IncludeBackUrl = false;
            }

            //temp solution for controls and view files
            if (this.Name.ToLower() == "browse" &&
                (this.Content.Name.ToLower().EndsWith(".ascx") ||
                this.Content.ContentHandler.NodeType.IsInstaceOfOrDerivedFrom("ViewBase")))
            {
                this.Forbidden = true;
            }

            if (this.Application != null)
            {
                var cups = this.Application.CustomParameters;
                foreach (var key in cups.Keys)
                {
                    AddParameter(key, cups[key]);
                }
            }
        }

        public virtual Dictionary<string, object> GetParameteres()
        {
            return GetParameteres(this.Parameters);
        }

        protected virtual Dictionary<string, object> GetParameteres(object parameters)
        {
            var dict = parameters as Dictionary<string, object>;

            if (dict == null)
            {
                dict = new Dictionary<string, object>();

                if (parameters != null)
                {
                    parameters.GetType().GetProperties().Select(p =>
                    {
                        dict.Add(p.Name, p.GetValue(parameters, null));
                        return true;
                    }).ToArray();
                }
            }

            return dict;
        }

        public virtual string SerializeParameters(Dictionary<string, object> parameters)
        {
            var sb = new StringBuilder();

            foreach (KeyValuePair<string, object> pair in parameters)
            {
                sb.Append("&");
                sb.Append(pair.Key);
                sb.Append("=");
                sb.Append(pair.Value.ToString());
            }
            return sb.ToString();
        }

        public void AddParameter(string name, object value)
        {
            //silent error
            if (string.IsNullOrEmpty(name))
                return;

            var dict = GetParameteres();

            if (!dict.ContainsKey(name))
                dict.Add(name, value);
            else
                dict[name] = value;

            this.Parameters = dict;
        }
        
        //================================================================================= Properties
        
        public virtual string Name { get; set; }

        public virtual string BackUri { get; set; }

        public string BackUrlWithParameter
        {
            get
            {
                if (string.IsNullOrEmpty(this.BackUri) || string.IsNullOrEmpty(this.Uri))
                    return string.Empty;

                return string.Format("{0}{1}={2}", this.Uri.Contains("?") ? "&" : "?", ActionBase.BackUrlParameterName, this.BackUri);
            }
        }

        private bool _includeBackUrl = true;
        public virtual bool IncludeBackUrl
        {
            get { return _includeBackUrl; }
            set { _includeBackUrl = value; }
        }

        private string _icon;
        public virtual string Icon
        {
            get { return _icon ?? this.Name; }
            set { _icon = value; }
        }

        private string _actionCssClass;

        public string CssClass
        {
            get { return _actionCssClass ?? string.Empty; }
            set { _actionCssClass = value; }
        }

        public abstract string Uri { get; }

        private string _text;
        public string Text
        {
            get { return _text ?? this.Name; }
            set
            {
                _text = value;
            }
        }

        public virtual string Description { get; set; }

        private int _index;
        public virtual int Index
        {
            get
            {
                return Application != null ? Application.Index : _index;
            }
            set { _index = value; }
        }

        private bool? _isModal;
        public virtual bool IsModal
        {
            get
            {
                return _isModal ?? (Application != null ? Application.IsModal : true);
            }
            set { _isModal = value; }
        }

        /// <summary>
        /// Application for the action. This property cannot be public because of serialization issues.
        /// </summary>
        [IgnoreDataMember]
        private Application Application { get; set; }

        [IgnoreDataMember]
        protected Content Content { get; set; }

        [IgnoreDataMember]
        private object Parameters { get; set; }

        public bool NeedsElevation
        {
            get { return false; }
        }

        public bool Forbidden
        {
            get; set;
        }

        public bool Active
        {
            get { return !NeedsElevation || !Forbidden; }
        }

        private bool _visible = true;
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        //================================================================================= Helper methods

        public Application GetApplication()
        {
            //This methods exists because the Application property 
            //cannot be public because of serialization issues
            return this.Application;
        }

        public Content GetContent()
        {
            //This methods exists because the Content property 
            //cannot be public because of serialization issues
            return this.Content;
        }
    }
}
