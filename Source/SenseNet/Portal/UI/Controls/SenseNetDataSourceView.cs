using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Search;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.UI.Controls
{
    internal class SenseNetDataSourceView : DataSourceView
    {
        public SenseNetDataSourceView(IDataSource owner)
            : base(owner, DefaultViewName)
        {
        }

        internal static string DefaultViewName = "SenseNetDefaultView";

        //======================================================= Properties

        internal Content Content { get; set; }

        internal string MemberName { get; set; }

        internal string FieldNames { get; set; }

        internal bool ShowHidden { get; set; }

        internal bool FlattenResults { get; set; }

        internal string DefaultOrdering { get; set; }

        internal string GroupBy { get; set; }

        internal Expression QueryFilter { get; set; }

        internal string QueryText { get; set; }

        internal QuerySettings Settings { get; set; }

        private ContentQuery ContentQuery
        {
            get
            {
                if (string.IsNullOrEmpty(QueryText))
                    return null;

                var cq = ContentQuery.CreateQuery(QueryText);
                if (cq.IsNodeQuery)
                    return null;

                var clauses = string.Empty;

                if (!this.ShowHidden)
                    clauses += "+Hidden:false";

                if (this.Content != null)
                {
                    //constructing query for a reference property is not possible
                    if (!string.IsNullOrEmpty(this.MemberName) && this.Content.Fields.ContainsKey(this.MemberName))
                        return null;

                    if (this.FlattenResults)
                    {
                        //path query
                        clauses += string.Format("+InTree:\"{0}\"", this.Content.Path);
                    }
                    else
                    {
                        //children query
                        clauses += string.Format("+InFolder:\"{0}\"", this.Content.Path);
                    }
                }

                if (!string.IsNullOrEmpty(clauses))
                    cq.AddClause(clauses, ChainOperator.And);

                if (this.Settings != null)
                    cq.Settings = this.Settings;

                return cq;
            }
        }

        internal NodeQuery Query
        {
            get
            {
                NodeQuery query;

                if (!string.IsNullOrEmpty(QueryText))
                {
                    try
                    {
                        query = NodeQuery.Parse(QueryText);
                    }
                    catch
                    {
                        //TODO handle wrong query syntax
                        query = new NodeQuery();
                    }
                }
                else
                {
                    query = new NodeQuery();
                }

                if (!this.ShowHidden)
                    query.Add(new IntExpression(ActiveSchema.PropertyTypes["Hidden"], ValueOperator.NotEqual, 1));

                if (this.Content != null)
                {
                    //constructing query for a reference property is not possible
                    if (!string.IsNullOrEmpty(this.MemberName) && this.Content.Fields.ContainsKey(this.MemberName))
                        return null;

                    if (this.FlattenResults)
                    {
                        //path query
                        query.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith,
                            RepositoryPath.Combine(this.Content.Path, RepositoryPath.PathSeparator)));

                    }
                    else
                    {
                        //children query
                        query.Add(new IntExpression(IntAttribute.ParentId, ValueOperator.Equal, this.Content.Id));
                    }

                    if (this.QueryFilter != null)
                    {
                        query.Add(this.QueryFilter);
                    }
                }
                else if (this.QueryFilter != null)
                {
                    query.Add(this.QueryFilter);
                }

                if (query != null && this.Settings != null && this.Settings.Top > 0)
                    query.Top = this.Settings.Top;

                return query;
            }
        }

        //======================================================== Execute select

        public IEnumerable<Content> Select(DataSourceSelectArguments selectArgs)
        {
            return ExecuteSelect(selectArgs).Cast<Content>();
        }

        protected override IEnumerable ExecuteSelect(DataSourceSelectArguments selectArgs)
        {
            //collect sorting information
            var sortExp = GetSortOrderInfo(selectArgs.SortExpression);
            if (sortExp.HasValue)
            {
                if (this.Settings == null)
                    this.Settings = new QuerySettings();

                this.Settings.Sort = GetSortInfoList(sortExp);
            }

            //results
            IEnumerable<Content> dataList = null;

            //"query" and "member" behavior cannot be merged,
            //return reference property values if Content and MemberName is given
            if (this.Content != null && !string.IsNullOrEmpty(this.MemberName))
            {
                //return the children of this content
                if (this.MemberName.Trim().ToLower().CompareTo("children") == 0)
                {
                    dataList = GetChildren();
                    if (dataList != null)
                        return dataList;
                }

                //return referenced contents
                if (this.Content.Fields.ContainsKey(this.MemberName))
                {
                    var members = this.Content[this.MemberName] as IEnumerable<Node>;
                    if (members != null)
                    {
                        return GetContents(members, true);
                    }
                }

                //not found, return empty list
                OnSelected(new SenseNetDataSourceStatusEventArgs(0));

                return new List<Content>();
            }

            //build query and return results
            var contentQuery = this.ContentQuery;
            if (contentQuery != null)
            {
                try
                {
                    //if lucene query text is given, use that
                    dataList = (from node in contentQuery.Execute().Nodes
                                select Content.Create(node)).ToList();
                }
                catch (Exception ex)
                {
                    //query parse or execution error
                    Logger.WriteException(ex);
                    dataList = new List<Content>();
                }

                if (dataList.Count() > 0)
                {
                    var propDesc = Content.GetPropertyDescriptors(GetFieldNames());

                    foreach (var content in dataList)
                    {
                        content.PropertyDescriptors = propDesc;
                    }
                }
            }
            else if (this.Content != null && string.IsNullOrEmpty(this.QueryText))
            {
                //if Content is given but no query exists, return the children collection
                dataList = GetChildren();
                if (dataList != null)
                    return dataList;
            }

            var query = dataList == null ? this.Query : null;
            if (query != null)
            {
                if (sortExp.HasValue)
                {
                    var fieldName = string.Empty;
                    var pt = GetPropertyTypeFromFullName(sortExp.Value.PropertyName, out fieldName);

                    if (pt != null)
                    {
                        query.Orders.Add(new SearchOrder(pt, sortExp.Value.Direction));
                    }
                    else
                    {
                        //no real property, try the Node attributes
                        if (Enum.GetNames(typeof(StringAttribute)).Contains(fieldName))
                        {
                            var sa = (StringAttribute)Enum.Parse(typeof(StringAttribute), fieldName);
                            query.Orders.Add(new SearchOrder(sa, sortExp.Value.Direction));
                        }
                        else if (Enum.GetNames(typeof(IntAttribute)).Contains(fieldName))
                        {
                            var sa = (IntAttribute)Enum.Parse(typeof(IntAttribute), fieldName);
                            query.Orders.Add(new SearchOrder(sa, sortExp.Value.Direction));
                        }
                        else if (Enum.GetNames(typeof(DateTimeAttribute)).Contains(fieldName))
                        {
                            var sa = (DateTimeAttribute)Enum.Parse(typeof(DateTimeAttribute), fieldName);
                            query.Orders.Add(new SearchOrder(sa, sortExp.Value.Direction));
                        }
                    }
                }

                dataList = Content.Query(query, GetFieldNames());
            }

            //TODO: check Count mechanism
            if (dataList != null)
                this.OnSelected(new SenseNetDataSourceStatusEventArgs(dataList.Count()));

            return dataList;
        }

        //======================================================== Helper methods

        private IEnumerable<string> GetFieldNames()
        {
            //collect field names and fullnames both
            var fieldNames = new List<string>();

            //collect ContentList fields if possible
            if (this.Content != null && string.IsNullOrEmpty(this.FieldNames))
            {
                var cl = this.Content.ContentHandler as ContentList ??
                         this.Content.ContentHandler.LoadContentList() as ContentList;

                if (cl != null)
                {
                    var allowedFields = GetFieldNames(cl.GetAvailableFields()).ToList();

                    //foreach (var fields in (List<Node>)this.Content.Fields["ContentTypes"].GetData())
                    //{
                    //    allowedFields.AddRange(GetFieldNames(ContentType.GetByName(fields.Name).FieldSettings));
                    //}
                    var gc = this.Content.ContentHandler as GenericContent;
                    if (gc != null)
                        foreach (var childType in gc.GetAllowedChildTypes())
                            allowedFields.AddRange(GetFieldNames(childType.FieldSettings));

                    return allowedFields.Distinct().ToList();
                }
            }

            if (string.IsNullOrEmpty(this.FieldNames))
            {
                //if explicit field names are not given, use _all_ the field names 
                //in the system - except the ContentList field names ("#ContentListField1,...")
                foreach (var contentType in ContentType.GetContentTypes())
                {
                    fieldNames.AddRange(GetFieldNames(contentType.FieldSettings));
                }
            }
            else
            {
                fieldNames = new List<string>(this.FieldNames.Split(new[] { ',', ';', ' ' },
                    StringSplitOptions.RemoveEmptyEntries));
            }

            return fieldNames.Distinct();
        }

        private static IEnumerable<string> GetFieldNames(IEnumerable<FieldSetting> fieldSettings)
        {
            var fieldNames = new List<string>();

            fieldNames.AddRange(from fs in fieldSettings
                                select fs.FullName);
            fieldNames.AddRange(from fs in fieldSettings
                                select fs.Name);

            return fieldNames.Distinct();
        }

        private PropertyType GetPropertyTypeFromFullName(string fullName, out string fieldName)
        {
            //fullName: "GenericContent.DisplayName", "ContentList.#ListField1", "Rating"
            var names = fullName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var typeName = names.Length == 2 ? names[0] : string.Empty;
            fieldName = names.Length == 1 ? names[0] : names[1];

            if (fieldName.StartsWith("#"))
            {
                //try to get the ContentList field property type name ("#String_0")
                if (this.Content != null)
                {
                    var cl = this.Content.ContentHandler as ContentList;
                    if (cl != null)
                        return PropertyType.GetByName(cl.GetPropertySingleId(fieldName));
                }
            }

            //without type, return from the active schema
            if (string.IsNullOrEmpty(typeName))
                return PropertyType.GetByName(fieldName);

            //return by the given content type
            var fn = fieldName;
            return (from pt in NodeType.GetByName(typeName).PropertyTypes
                    where pt.Name.CompareTo(fn) == 0
                    select pt).FirstOrDefault();
        }

        private SortOrderInfo? GetSortOrderInfo(string currentSortExpression)
        {
            var sortExp = string.IsNullOrEmpty(currentSortExpression) ?
                            this.DefaultOrdering : currentSortExpression;

            if (string.IsNullOrEmpty(sortExp))
                return null;

            var separatorIndex = sortExp.IndexOf('.');
            if (separatorIndex >= 0)
                sortExp = sortExp.Substring(separatorIndex + 1);

            var result = new SortOrderInfo { Direction = OrderDirection.Asc, PropertyName = sortExp };
            if (sortExp.EndsWith(" DESC"))
            {
                result.PropertyName = sortExp.Remove(sortExp.LastIndexOf(" DESC"));
                result.Direction = OrderDirection.Desc;
            }
            else if (sortExp.EndsWith(" ASC"))
            {
                result.PropertyName = sortExp.Remove(sortExp.LastIndexOf(" ASC"));
                result.Direction = OrderDirection.Asc;
            }

            return result;
        }

        private IEnumerable<SortInfo> GetSortInfoList(SortOrderInfo? sortOrderInfo)
        {
            var sortInfos = new List<SortInfo>();

            if (!string.IsNullOrEmpty(this.GroupBy))
                sortInfos.Add(new SortInfo{ FieldName = this.GroupBy });

            if (!sortOrderInfo.HasValue)
                return sortInfos;

            sortInfos.Add(new SortInfo
                              {
                                  FieldName = sortOrderInfo.Value.PropertyName,
                                  Reverse = sortOrderInfo.Value.Direction == OrderDirection.Desc
                              });

            return sortInfos;
        }

        private IEnumerable<Content> GetContents(IEnumerable<Node> nodes, bool enforceSettings)
        {
            //get contents and property descriptors
            var contents = (from node in nodes
                            select Content.Create(node)).ToList();
            var propDesc = Content.GetPropertyDescriptors(GetFieldNames());

            //set descriptor collection to each content
            foreach (var content in contents)
            {
                content.PropertyDescriptors = propDesc;
            }

            OnSelected(new SenseNetDataSourceStatusEventArgs(contents.Count));

            if (!enforceSettings || this.Settings == null)
                return contents;

            //sorting
            if (this.Settings.Sort != null && this.Settings.Sort.Count() > 0)
            {
                var sortInfo = this.Settings.Sort.First();

                contents.Sort(new ContentComparer(new SortOrderInfo
                                                      {
                                                          PropertyName = sortInfo.FieldName,
                                                          Direction = sortInfo.Reverse ? OrderDirection.Desc : OrderDirection.Asc
                                                      }));
            }

            return this.Settings.Top > 0 ? contents.Take(this.Settings.Top) : contents;
        }

        private IEnumerable<Content> GetChildren()
        {
            var gc = this.Content.ContentHandler as GenericContent;
            if (gc != null)
            {
                var smf = gc as SmartFolder;
                if (smf != null)
                {
                    if (smf.EnableAutofilters != FilterStatus.Default || smf.EnableLifespanFilter != FilterStatus.Default)
                    {
                        if (this.Settings == null)
                            this.Settings = new QuerySettings();

                        if (smf.EnableAutofilters != FilterStatus.Default)
                            this.Settings.EnableAutofilters = smf.EnableAutofilters == FilterStatus.Enabled;

                        if (smf.EnableLifespanFilter != FilterStatus.Default)
                            this.Settings.EnableLifespanFilter = smf.EnableLifespanFilter == FilterStatus.Enabled;
                    }
                }

                return GetContents(gc.GetChildren("-Id:" + this.Content.Id, this.Settings, this.FlattenResults).Nodes, false);
            }

            var ctype = this.Content.ContentHandler as ContentType;
            if (ctype != null)
            {
                return GetContents(ctype.GetChildren("-Id:" + this.Content.Id, this.Settings, this.FlattenResults).Nodes, false);
            }

            return null;
        }

        //======================================================== Events and handlers

        private static readonly object SelectedEventKey = new object();
        public event SqlDataSourceStatusEventHandler Selected
        {
            add { Events.AddHandler(SelectedEventKey, value); }
            remove { Events.RemoveHandler(SelectedEventKey, value); }
        }

        protected virtual void OnSelected(SenseNetDataSourceStatusEventArgs e)
        {
            var handler = Events[SelectedEventKey] as SqlDataSourceStatusEventHandler;
            if (handler != null)
                handler(this, e);
        }

        //======================================================== Overrides

        public override bool CanSort { get { return true; } }
        public override bool CanDelete
        {
            get
            {
                return false;
            }
        }
        protected override int ExecuteDelete(IDictionary keys, IDictionary values)
        {
            throw new NotSupportedException();
        }
        public override bool CanInsert
        {
            get
            {
                return false;
            }
        }
        protected override int ExecuteInsert(IDictionary values)
        {
            throw new NotSupportedException();
        }
        public override bool CanUpdate
        {
            get
            {
                return false;
            }
        }
        protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues)
        {
            throw new NotSupportedException();
        }

        //======================================================== Helper classes

        private struct SortOrderInfo
        {
            public string PropertyName { get; set; }
            public OrderDirection Direction { get; set; }
        }

        private class ContentComparer : IComparer<Content>
        {
            private SortOrderInfo _orderInfo;

            public ContentComparer(SortOrderInfo? orderInfo)
            {
                _orderInfo = orderInfo.HasValue ?
                    orderInfo.Value :
                    new SortOrderInfo { Direction = OrderDirection.Asc, PropertyName = "Name" };
            }

            #region IComparer Members

            public int Compare(Content x, Content y)
            {
                var result = 0;

                if (!x.Fields.ContainsKey(_orderInfo.PropertyName) || !y.Fields.ContainsKey(_orderInfo.PropertyName))
                    return result;

                //object class has no Compare function, need to convert values to something
                var valX = x[_orderInfo.PropertyName];
                var valY = y[_orderInfo.PropertyName];

                var xFieldType = x.Fields[_orderInfo.PropertyName].FieldSetting.FieldDataType;
                var yFieldType = y.Fields[_orderInfo.PropertyName].FieldSetting.FieldDataType;

                if (xFieldType == null || yFieldType == null || xFieldType.FullName != yFieldType.FullName)
                {
                    //if the types are null or different, convert the values to string
                    result = string.Compare(valX == null ? string.Empty : valX.ToString(), valY == null ? string.Empty : valY.ToString());
                }
                else
                {
                    //if we can use a type-specific compare method
                    switch (xFieldType.FullName)
                    {
                        case "System.Int32": 
                            result = Convert.ToInt32(valX).CompareTo(Convert.ToInt32(valY));
                            break;
                        default:
                            result = string.Compare(valX == null ? string.Empty : valX.ToString(), valY == null ? string.Empty : valY.ToString());
                            break;
                    }
                }

                return _orderInfo.Direction == OrderDirection.Asc ? result : -result;
            }

            #endregion
        }
    }

}
