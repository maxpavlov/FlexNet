using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using SenseNet.ContentRepository;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;
using System.Collections.Specialized;
using SenseNet.Portal.Workspaces;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Portal.UI.ContentListViews.FieldControls
{
    public enum LogicalOperator { And, Or, Not, None };

    public enum Operator
    {
        Equal,
        NotEqual,
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual,
        StartsWith,
        EndsWith,
        Contains,
        Inherits
    };

    public enum PredicateType
    {
        UnknowExpression,
        StringExpression,
        DateTimeExpression,
        IntExpression,
        ReferenceExpression,
        BooleanExpression,
        SearchExpression,
        TypeExpression,
        AllExpressions
    }

    public class ExpressionItem
    {
        [XmlAttribute("logicalOperator")]
        public LogicalOperator LogicalOperator { get; set; }
    }

    [XmlRoot(ElementName = "Query", Namespace = "http://schemas.sensenet.com/SenseNet/ContentRepository/QueryEditor")]
    [XmlInclude(typeof(Group))]
    [XmlInclude(typeof(Predicate))]
    public class Query
    {
        private static readonly NameValueCollection Tokens = new NameValueCollection
                                                                 {
                                                                     { "[Me]", @"<currentuser property=""Id"" />" },
                                                                     { "[Site]", @"<currentsite property=""Id"" />" },
                                                                     { "[Workspace]", @"<currentworkspace property=""Id"" />" },
                                                                     { "[Page]", @"<currentpage property=""Id"" />" },
                                                                     { "[Content]", @"<currentcontent property=""Id"" />" },
                                                                     { "[Today]", @"<thisday />" },
                                                                     { "[Yesterday]", @"<yesterday />" },
                                                                     { "[Tomorrow]", @"<tomorrow />" },
                                                                     { "[Now]", @"<now />" }
                                                                 };

        [XmlElement("ExpressionItem")]
        public ExpressionItem Root;

        public Group Group;
        public Predicate Predicate;

        /// <summary>
        /// This is a temporary solution before the gui xml parse moves to the official NodeQuery.Parse method
        /// </summary>
        public static string GetNodeQueryXml(string guiQuery)
        {
            if (string.IsNullOrEmpty(guiQuery))
                return guiQuery;

            guiQuery = ReplacePath(guiQuery);

            foreach (var token in Tokens.AllKeys)
            {
                try
                {
                    string repVal = null; 

                    switch (token)
                    {
                        case "[Me]": repVal = User.Current.Id.ToString(); break;
                        case "[Site]": repVal = Site.Current.Id.ToString(); break;
                        case "[Workspace]": repVal = PortalContext.Current.ContextWorkspace.Id.ToString(); break;
                        case "[Page]": repVal = Page.Current.Id.ToString(); break;
                        case "[Content]": repVal = PortalContext.Current.ContextNodeHead.Id.ToString(); break;
                        default: repVal = Tokens[token]; break;
                    }

                    guiQuery = guiQuery.Replace(token, repVal).Replace(token.ToLower(), repVal);
                }
                catch(Exception ex)
                {
                    Logger.WriteException(ex);
                }
            }

            var xsltTransform = Xslt.GetXslt("/Root/System/SystemPlugins/Renderers/Common/GuiToNodeQuery.xslt", false);
            var xDoc = new XmlDocument();
            xDoc.LoadXml(guiQuery);

            using (var sw = new StringWriter())
            {
                using (var writer = new XmlTextWriter(sw))
                {
                    xsltTransform.Transform(xDoc, null, writer);
                    writer.Flush();
                }

                return sw.ToString();
            }
        }

        private static string ReplacePath(string guiQueryXml)
        {
            if (string.IsNullOrEmpty(guiQueryXml))
                return string.Empty;

            var xs = new XmlSerializer(typeof(Query));
            Query query;
            string result;

            using (var sr = new StringReader(guiQueryXml))
            {
                using (var xr = new XmlTextReader(sr))
                {
                    query = xs.Deserialize(xr) as Query;
                }
            }

            if (query != null)
                ReplacePath(query.Root);

            using (var sw = new StringWriter())
            {
                using (var xw = new XmlTextWriter(sw))
                {
                    xs.Serialize(xw, query);
                    result = sw.ToString();
                }
            }

            return result;
        }

        private static void ReplacePath(ExpressionItem exp)
        {
            var pred = exp as Predicate;
            var gr = exp as Group;

            if (pred != null)
            {
                if (pred.PredicateType == PredicateType.ReferenceExpression)
                {
                    var id = 0;

                    //if it is a path --> replace to ID
                    if (!int.TryParse(pred.RightOperand, out id))
                    {
                        var node = Node.LoadNode(pred.RightOperand);
                        if (node != null)
                            pred.RightOperand = node.Id.ToString();
                    }
                }
            }
            else if (gr != null)
            {
                foreach (var item in gr.Items)
                {
                    ReplacePath(item);
                }
            }
    }
    }

    public class Predicate : ExpressionItem
    {
        private static PredicateType MapTypeName(string typename)
        {
            switch (typename)
            {
                case "ShortText":
                case "LongText":
                    return PredicateType.StringExpression;
                case "Reference":
                    return PredicateType.ReferenceExpression;
                case "Boolean":
                    return PredicateType.BooleanExpression;
                case "DateTime":
                    return PredicateType.DateTimeExpression;
                case "Number":
                case "Integer":
                    return PredicateType.IntExpression;
                default:
                    return PredicateType.UnknowExpression;
            }
        }

        [XmlAttribute("type")]
        public PredicateType PredicateType
        {
            get;
            set;
        }

        private string _leftOperand;

        public string LeftOperand
        {
            get
            {
                return _leftOperand;
            }
            set
            {
                _leftOperand = value;

                //var cl = PortalContext.Current.ContextNode as ContentList ??
                //         PortalContext.Current.ContextNode.LoadContentList() as ContentList;

                //if (cl == null)
                //    return;

                //PredicateType = (from field in cl.GetAvailableFields()
                //                     where field.Name == value
                //                     select MapTypeName(field.ShortName)).FirstOrDefault();
            }
        }

        [XmlAttribute("operator")]
        public Operator Operator { get; set; }

        public string RightOperand { get; set; }
    }

    [XmlInclude(typeof(Predicate))]
    public class Group : ExpressionItem
    {
        public Group()
        {
            Items = new ObservableCollection<ExpressionItem>();
        }
        public ObservableCollection<ExpressionItem> Items
        {
            get;
            set;
        }
    }
}