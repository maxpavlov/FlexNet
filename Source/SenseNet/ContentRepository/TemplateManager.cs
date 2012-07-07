using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.ContentRepository
{
    public abstract class TemplateReplacerBase
    {
        public virtual string TemplatePatternFormat
        {
            get { return TemplateManager.TEMPLATE_PATTERN_FORMAT; }
        }

        public abstract IEnumerable<string> TemplateNames { get; }

        public abstract string EvaluateTemplate(string templateName, string propertyName, object parameters);
    }

    public class TemplateManager
    {
        public static readonly string TEMPLATE_PATTERN_FORMAT = "@@{0}(\\.(?<PropertyName>[^@]+))?@@";

        //========================================================================= Replacers

        private static Dictionary<string, string> _templatePatternFormats;

        private static readonly object LOCK_OBJECT = new object();

        private static Dictionary<string, Dictionary<string, TemplateReplacerBase>> _templateReplacers;

        private static Dictionary<string, Dictionary<string, TemplateReplacerBase>> TemplateReplacers
        {
            get
            {
                if (_templateReplacers == null)
                {
                    lock (LOCK_OBJECT)
                    {
                        if (_templateReplacers == null)
                            _templateReplacers = DiscoverTemplateReplacers();
                    }
                }

                return _templateReplacers;
            }
        }

        private static Dictionary<string, Dictionary<string, TemplateReplacerBase>> DiscoverTemplateReplacers()
        {
            var trType = typeof(TemplateReplacerBase);
            var replacerTypes = TypeHandler.GetTypesByBaseType(trType);
            var replacers = new Dictionary<string, Dictionary<string, TemplateReplacerBase>>();

            _templatePatternFormats = new Dictionary<string, string>();

            foreach (var replacerType in replacerTypes)
            {
                var itr = replacerType;

                //Find the base type for this replacer type 'subtree' that
                //is the direct children of the abstract replacer base type
                while (itr.BaseType != null && itr.BaseType.FullName != trType.FullName)
                {
                    itr = itr.BaseType;
                }

                //store the different replace patterns for the replacer base types
                if (!replacers.ContainsKey(itr.FullName))
                {
                    var replacerBaseInstance = Activator.CreateInstance(itr) as TemplateReplacerBase;
                    if (replacerBaseInstance != null)
                        _templatePatternFormats.Add(itr.FullName, replacerBaseInstance.TemplatePatternFormat);

                    replacers.Add(itr.FullName, CollectTemplateReplacers(itr));
                }
            }

            return replacers;
        }

        private static Dictionary<string, TemplateReplacerBase> CollectTemplateReplacers(Type replacerBaseType)
        {
            var replacers = new Dictionary<string, TemplateReplacerBase>();
            var replacerTypes = new List<Type> { replacerBaseType };

            replacerTypes.AddRange(TypeHandler.GetTypesByBaseType(replacerBaseType));

            foreach (var replacerType in replacerTypes)
            {
                var replacerInstance = Activator.CreateInstance(replacerType) as TemplateReplacerBase;
                if (replacerInstance == null)
                    continue;

                foreach (var templateName in replacerInstance.TemplateNames)
                {
                    if (replacers.ContainsKey(templateName))
                    {
                        if (replacers[templateName].GetType().IsAssignableFrom(replacerType))
                            replacers[templateName] = replacerInstance;
                    }
                    else
                    {
                        replacers.Add(templateName, replacerInstance);
                    }
                }
            }

            return replacers;
        }
        
        //========================================================================= Replace methods

        public static string Replace(Type replacerBaseType, string text)
        {
            if (replacerBaseType == null)
                throw new ArgumentNullException("replacerBaseType");

            return Replace(replacerBaseType.FullName, text);
        }

        public static string Replace(string replacerBaseType, string text)
        {
            return Replace(replacerBaseType, text, null);
        }

        public static string Replace(Type replacerBaseType, string text, object parameters)
        {
            if (replacerBaseType == null)
                throw new ArgumentNullException("replacerBaseType");

            return Replace(replacerBaseType.FullName, text, parameters);
        }

        public static string Replace(string replacerBaseType, string text, object parameters)
        {
            if (!TemplateReplacers.ContainsKey(replacerBaseType))
                throw new InvalidOperationException("No template replacer found with the name " + replacerBaseType ?? string.Empty);

            return string.IsNullOrEmpty(text)
                       ? text
                       : ReplaceTemplates(_templatePatternFormats.ContainsKey(replacerBaseType) ? _templatePatternFormats[replacerBaseType] : TEMPLATE_PATTERN_FORMAT, 
                                          TemplateReplacers[replacerBaseType],
                                          text, parameters);
        }

        //========================================================================= Replace methods - private

        private static string ReplaceTemplates(string patternFormat, Dictionary<string, TemplateReplacerBase> replacers, string queryText, object parameters)
        {
            foreach (var templateName in replacers.Keys)
            {
                var templatePattern = string.Format(patternFormat ?? TEMPLATE_PATTERN_FORMAT, templateName);
                var index = 0;
                var regex = new Regex(templatePattern, RegexOptions.IgnoreCase);

                while (true)
                {
                    var match = regex.Match(queryText, index);
                    if (!match.Success)
                        break;

                    var propName = match.Groups["PropertyName"];
                    var templateValue = replacers[templateName].EvaluateTemplate(templateName, propName == null ? null : propName.Value, parameters) ?? string.Empty;

                    queryText = queryText
                        .Remove(match.Index, match.Length)
                        .Insert(match.Index, templateValue);

                    index = match.Index + templateValue.Length;

                    if (index >= queryText.Length)
                        break;
                }
            }

            return queryText;
        }

        //========================================================================= Helper methods

        public static void Init()
        {
            //init replacers
            var reps = TemplateReplacers;
        }

        public static string GetProperty(GenericContent content, string propertyName)
        {
            if (content == null)
                return string.Empty;
            if (string.IsNullOrEmpty(propertyName))
                return content.Id.ToString();

            var value = content.GetProperty(propertyName);
            return value == null ? string.Empty : value.ToString();
        }

        public static string GetProperty(Node node, string propertyName)
        {
            if (node == null)
                return string.Empty;
            if (string.IsNullOrEmpty(propertyName))
                return node.Id.ToString();

            var value = node[propertyName];
            return value == null ? string.Empty : value.ToString();
        }
    }
}
