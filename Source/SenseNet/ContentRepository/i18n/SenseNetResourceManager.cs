using System;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Web;
using System.Web.Compilation;
using SenseNet.ContentRepository;
using SenseNet.Diagnostics;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage;
using System.Xml;
using SenseNet.Communication.Messaging;

namespace SenseNet.ContentRepository.i18n
{
    public sealed class SenseNetResourceManager
    {
        //================================================================ Cross appdomain part
        [Serializable]
        internal sealed class ResourceManagerResetDistributedAction : DistributedAction
        {
            public override void DoAction(bool onRemote, bool isFromMe)
            {
                // Local echo of my action: Return without doing anything
                if (onRemote && isFromMe)
                    return;
                SenseNetResourceManager.ResetPrivate();
            }
        }

        internal static void Reset()
        {
            Logger.WriteInformation("ResourceManager.Reset called.", Logger.Categories(),
               new Dictionary<string, object> { { "AppDomain", AppDomain.CurrentDomain.FriendlyName } });
            new ResourceManagerResetDistributedAction().Execute();
        }
        private static void ResetPrivate()
        {
            Logger.WriteInformation("ResourceManager.Reset executed.", Logger.Categories(),
               new Dictionary<string, object> { { "AppDomain", AppDomain.CurrentDomain.FriendlyName } });
            lock (_syncRoot)
            {
                _current = null;
            }
        }

        //================================================================ Static part

        private const string ResourceLoadedKey = "IsResourceLoaded";
        public const string ResourceStartKey = "<%$";
        public const string ResourceEndKey = "%>";
        private static object _syncRoot = new Object();

        private static SenseNetResourceManager _current;
        public static SenseNetResourceManager Current
        {
            get
            {
                if (_current == null)
                {
                    lock (_syncRoot)
                    {
                        if (_current == null)
                        {
                            var current = new SenseNetResourceManager();
                            current.Load();
                            _current = current;
                            Logger.WriteInformation("ResourceManager created: " + _current.GetType().FullName);
                        }
                    }
                }

                return _current;
            }
        }

        private static string _fallbackCulture;
        public static string FallbackCulture
        {
            get { return _fallbackCulture ?? (_fallbackCulture = (System.Configuration.ConfigurationManager.AppSettings["FallbackCulture"] ?? string.Empty).ToLower()); }
        }

        private SenseNetResourceManager() { }

        //================================================================ Instance part

        Dictionary<CultureInfo, Dictionary<string, Dictionary<string, object>>> _items = new Dictionary<CultureInfo, Dictionary<string, Dictionary<string, object>>>();

        private void Load()
        {
            // search for all Resource content
            NodeQuery query = new NodeQuery();
            query.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith,
                    String.Concat(Repository.ResourceFolderPath, RepositoryPath.PathSeparator)));
            query.Add(new TypeExpression(ActiveSchema.NodeTypes["Resource"]));

            IEnumerable<Node> nodes = null;
            if (StorageContext.Search.IsOuterEngineEnabled && StorageContext.Search.SearchEngine != InternalSearchEngine.Instance)
            {
                nodes = query.Execute().Nodes.OrderBy(i => i.Index);
            }
            else
            {
                var r = NodeQuery.QueryNodesByTypeAndPath(ActiveSchema.NodeTypes["Resource"]
                    , false
                    , String.Concat(Repository.ResourceFolderPath, RepositoryPath.PathSeparator)
                    , true);
                nodes = r.Nodes.OrderBy(i => i.Index);
            }

            ParseAll(nodes);

            ////-- sort by index
            //NodeComparer<Node> resourceComparer = new NodeComparer<Node>();
            //result.Sort(resourceComparer);
        }
        private void ParseAll(IEnumerable<Node> nodes)
        {
            //<Resources>
            //  <ResourceClass name="Portal">
            //    <Languages>
            //      <Language cultureName="hu">
            //        <data name="CheckedOutBy" xml:space="preserve">
            //          <value>Kivette</value>
            try
            {
                foreach (Resource res in nodes)
                {
                    try
                    {
                        var xml = new XmlDocument();
                        xml.Load(res.Binary.GetStream());
                        foreach (XmlElement classElement in xml.SelectNodes("/Resources/ResourceClass"))
                        {
                            var className = classElement.Attributes["name"].Value;
                            foreach (XmlElement languageElement in classElement.SelectNodes("Languages/Language"))
                            {
                                var cultureName = languageElement.Attributes["cultureName"].Value;
                                var cultureInfo = CultureInfo.GetCultureInfo(cultureName);
                                foreach (XmlElement dataElement in languageElement.SelectNodes("data"))
                                {
                                    var key = dataElement.Attributes["name"].Value;
                                    var value = dataElement.SelectSingleNode("value").InnerXml;

                                    AddItem(cultureInfo, className, key, value);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteException(new Exception("Invalid resource: " + res.Path, ex));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }
        }
        private void AddItem(CultureInfo cultureInfo, string className, string key, string value)
        {
            Dictionary<string, Dictionary<string, object>> culture;
            Dictionary<string, object> category;
            object item;

            if (!_items.TryGetValue(cultureInfo, out culture))
            {
                culture = new Dictionary<string, Dictionary<string, object>>();
                _items.Add(cultureInfo, culture);
            }
            if (!culture.TryGetValue(className, out category))
            {
                category = new Dictionary<string, object>();
                culture.Add(className, category);
            }

            if (!category.TryGetValue(key, out item))
                category.Add(key, value);
            else
                category[key] = value;
        }

        //---------------------------------------------------------------- Resource editor
        private string GetEditorMarkup(string className, string name, string s)
        {
            var linkstart = "<a href='javascript:' onclick=\"SN.ResourceEditor.editResource('" + className + "','" + name + "');\">";
            var text = "<span style='background-color:#FFFFE6;color:#000;border:1px dashed #333;'>" + s + "</span>";
            return linkstart + text + "</a>";
        }
        public static bool IsResourceEditorAllowed
        {
            get
            {
                if (HttpContext.Current == null || User.Current == null)
                    return false;

                return HttpContext.Current.Request["resources"] != null && User.Current.IsInGroup(Group.Administrators);
            }
        }

        //---------------------------------------------------------------- Accessors

        public string GetString(string className, string name)
        {
            return GetString(className, name, CultureInfo.CurrentUICulture);
        }
        /// <summary>
        /// Gets the specified string resource of the given classname for the CurrentUICulture property of the current thread. 
        /// </summary>
        /// <param name="className">Name of the class. (Represents a categoryname)</param>
        /// <param name="name">Name of the resource.</param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public string GetString(string className, string name, CultureInfo cultureInfo)
        {
            return GetObject(className, name, cultureInfo) as string;
        }
        /// <summary>
        /// Gets the value of the resource for the specified culture and class.
        /// </summary>
        /// <param name="className">Name of the category.</param>
        /// <param name="name">Name of the resource.</param>
        /// <param name="cultureInfo"></param>
        /// <returns>The value of the resource, If a match is not possible, a generated resourcekey is returned.</returns>
        public object GetObject(string className, string name, CultureInfo cultureInfo)
        {
            var s = GetObjectInternal(cultureInfo, className, name);
            if (s == null)
            {
                if (!string.IsNullOrEmpty(FallbackCulture))
                {
                    try
                    {
                        //look for resource value using the fallback culture
                        var enCultureInfo = CultureInfo.GetCultureInfo(FallbackCulture);
                        s = GetObjectInternal(enCultureInfo, className, name);
                    }
                    catch (CultureNotFoundException ex)
                    {
                        Logger.WriteException(new Exception(string.Format("Invalid fallback culture: {0} ({1}, {2})", FallbackCulture, className, name), ex));
                    }
                }
                
                //no fallback resource, display the class and key instead
                if (s == null)
                    s = String.Concat(className, cultureInfo.Name, name);
            }

            if (!IsResourceEditorAllowed)
                return s;

            return GetEditorMarkup(className, name, s as string);
        }
        public string GetStringOrNull(string className, string name)
        {
            return GetStringOrNull(className, name, CultureInfo.CurrentUICulture);
        }
        public string GetStringOrNull(string className, string name, CultureInfo cultureInfo)
        {
            return GetObjectOrNull(className, name, cultureInfo) as string;
        }
        public object GetObjectOrNull(string className, string name, CultureInfo cultureInfo)
        {
            return GetObjectOrNull(className, name, cultureInfo, true);
        }
        public object GetObjectOrNull(string className, string name, CultureInfo cultureInfo, bool allowMarkup)
        {
            var s = GetObjectInternal(cultureInfo, className, name);
            if (!allowMarkup || !IsResourceEditorAllowed)
                return s;

            return GetEditorMarkup(className, name, s as string);
        }

        public string GetStringByExpression(string expression)
        {
            return IsExpression(expression) ? GetStringByExpressionInternal(expression) : null;
        }
        private static bool IsExpression(string expression)
        {
            return expression.StartsWith(ResourceStartKey) && expression.EndsWith(ResourceEndKey);
        }
        private string GetStringByExpressionInternal(string expression)
        {
            if (String.IsNullOrEmpty(expression))
                throw new ArgumentNullException("expression");

            expression = expression.Replace(" ", "");
            expression = expression.Replace(ResourceStartKey, "");
            expression = expression.Replace(ResourceEndKey, "");

            if (expression.Contains("Resources:"))
                expression = expression.Remove(expression.IndexOf("Resources:"), 10);

            var expressionFields = ResourceExpressionBuilder.ParseExpression(expression);
            if (expressionFields == null)
            {
                var context = HttpContext.Current;
                var msg = String.Format("{0} is not a valid string resource format.", expression);
                if (context == null)    
                    throw new ApplicationException(msg);
                return String.Format(msg);
            }
                

            return GetString(expressionFields.ClassKey, expressionFields.ResourceKey);
        }

        private object GetObjectInternal(CultureInfo cultureInfo, string className, string name)
        {
            var item = this.Get(cultureInfo, className, name);
            if (item != null)
                return item;

            var test = cultureInfo.IsNeutralCulture;
            if (cultureInfo == CultureInfo.InvariantCulture)
                return null;

            item = this.Get(cultureInfo.Parent, className, name);
            return item;
        }
        private object Get(CultureInfo cultureInfo, string className, string name)
        {
            Dictionary<string, Dictionary<string, object>> culture;
            Dictionary<string, object> category;
            object item;
            if (!_items.TryGetValue(cultureInfo, out culture))
                return null;
            if (!culture.TryGetValue(className, out category))
                return null;
            if (!category.TryGetValue(name, out item))
                return null;
            return item;
        }
        public Dictionary<string, object> GetClassItems(string className, CultureInfo cultureInfo)
        {
            Dictionary<string, Dictionary<string, object>> culture;
            Dictionary<string, object> category;
            if (!_items.TryGetValue(cultureInfo, out culture))
                return null;
            if (!culture.TryGetValue(className, out category))
                return null;
            return category;
        }
    }
}