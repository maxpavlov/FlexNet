using System.Collections.Generic;
using SenseNet.ContentRepository.Storage;
using  SenseNet.ContentRepository.Schema;
using SenseNet.Search;
using SenseNet.ContentRepository.Storage.Search;
using System;

namespace SenseNet.ContentRepository
{
	[ContentHandler]
	public class SmartFolder : Folder
	{
		//===================================================================================== Construction

        [Obsolete("Use typeof(SmartFolder).Name instead.", true)]
        public static readonly string NodeTypeName = typeof(SmartFolder).Name;

        public SmartFolder(Node parent) : this(parent, null) { }
		public SmartFolder(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected SmartFolder(NodeToken nt) : base(nt) { }

		//===================================================================================== Properties

		[RepositoryProperty("Query", RepositoryDataType.Text)]
		public string Query
		{
			get { return this.GetProperty<string>("Query"); }
			set { this["Query"] = value; }
		}

        [RepositoryProperty("EnableAutofilters", RepositoryDataType.String)]
        public virtual FilterStatus EnableAutofilters
        {
            get
            {
                var enumVal = base.GetProperty<string>("EnableAutofilters");
                if (string.IsNullOrEmpty(enumVal))
                    return FilterStatus.Default;

                return (FilterStatus)Enum.Parse(typeof(FilterStatus), enumVal); 
            }
            set
            {
                this["EnableAutofilters"] = Enum.GetName(typeof(FilterStatus), value);
            }
        }

        [RepositoryProperty("EnableLifespanFilter", RepositoryDataType.String)]
        public virtual FilterStatus EnableLifespanFilter
        {
            get
            {
                var enumVal = base.GetProperty<string>("EnableLifespanFilter");
                if (string.IsNullOrEmpty(enumVal))
                    return FilterStatus.Default;

                return (FilterStatus)Enum.Parse(typeof(FilterStatus), enumVal); 
            }
            set
            {
                this["EnableLifespanFilter"] = Enum.GetName(typeof(FilterStatus), value);
            }
        }

		//===================================================================================== Method

		protected override IEnumerable<Node> GetChildren()
		{
			if (!string.IsNullOrEmpty(Query))
			{
			    var cq = ContentQuery.CreateQuery(Query);

                if (EnableAutofilters != FilterStatus.Default)
                    cq.Settings.EnableAutofilters = (EnableAutofilters == FilterStatus.Enabled);
                if (EnableLifespanFilter != FilterStatus.Default)
                    cq.Settings.EnableLifespanFilter = (EnableLifespanFilter == FilterStatus.Enabled);

			    var result = cq.Execute();
			    return result.Nodes;
			}
			return base.GetChildren();
		}

        public override QueryResult GetChildren(string text, QuerySettings settings, bool getAllChildren)
        {
            if (!string.IsNullOrEmpty(Query) || !string.IsNullOrEmpty(text))
            {
                var cq = ContentQuery.CreateQuery(Query);
                if (EnableAutofilters != FilterStatus.Default)
                    cq.Settings.EnableAutofilters = (EnableAutofilters == FilterStatus.Enabled);
                if (EnableLifespanFilter != FilterStatus.Default)
                    cq.Settings.EnableLifespanFilter = (EnableLifespanFilter == FilterStatus.Enabled);

                if (settings != null)
                    cq.Settings = settings;

                if ((cq.IsContentQuery || string.IsNullOrEmpty(this.Query)) && !string.IsNullOrEmpty(text))
                    cq.AddClause(text);

                if (cq.IsContentQuery)
                {
                    //add SmartFolder's own children (with an Or clause)
                    var excapedPath = this.Path.Replace("(", "\\(").Replace(")", "\\)");
                    cq.AddClause((getAllChildren ? 
                        string.Format("InTree:\"{0}\"", excapedPath) : 
                        string.Format("InFolder:\"{0}\"", excapedPath)), ChainOperator.Or);
                }
                else
                    cq.AddClause(new IntExpression(IntAttribute.ParentId, ValueOperator.Equal, this.Id), ChainOperator.Or);

                var result = cq.Execute();
                return result;
            }
            return QueryResult.Empty;
        }
		public override object GetProperty(string name)
		{
			switch (name)
			{
				case "Query":
					return this.Query;
                case "EnableAutofilters":
                    return this.EnableAutofilters;
                case "EnableLifespanFilter":
                    return this.EnableLifespanFilter;
				default:
					return base.GetProperty(name);
			}
		}
		public override void SetProperty(string name, object value)
		{
			switch (name)
			{
				case "Query":
					this.Query = (string)value;
					break;
                case "EnableAutofilters":
                    this.EnableAutofilters = (FilterStatus)value;
                    break;
                case "EnableLifespanFilter":
                    this.EnableLifespanFilter = (FilterStatus)value;
                    break;
				default:
					base.SetProperty(name, value);
					break;
			}
		}

    }
}
