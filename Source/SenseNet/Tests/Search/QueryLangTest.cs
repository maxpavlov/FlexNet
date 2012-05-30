using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Tests;

namespace SenseNet.ContentRepository.Tests.Search
{
	[TestClass()]
    public class QueryLangTest : TestBase
	{
		#region Test infrastructure
		private TestContext testContextInstance;

		public override TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}
		#endregion

		#region Accessors
		private class NodeQueryAccessor : Accessor
		{
			public NodeQueryAccessor(NodeQuery target) : base(target) { }
			public NodeQuery Parse(string query, SchemaRoot schema)
			{
				return (NodeQuery)CallPrivateStaticMethod("Parse", new Type[] { typeof(string), typeof(SchemaRoot) }, new object[] { query, schema });
			}
		}
		#endregion

		[TestMethod]
		public void NodeQuery_BuildFromXml()
		{
			SchemaEditor editor = new SchemaEditor();
			NodeType nodeType1 = editor.CreateNodeType(null, "nodeType1");
			NodeType nodeType2 = editor.CreateNodeType(null, "nodeType2");
			PropertyType stringSlot1 = editor.CreatePropertyType("stringSlot1", DataType.String);
			PropertyType stringSlot2 = editor.CreatePropertyType("stringSlot2", DataType.String);
			PropertyType intSlot1 = editor.CreatePropertyType("intSlot1", DataType.Int);
			PropertyType intSlot2 = editor.CreatePropertyType("intSlot2", DataType.Int);
			PropertyType dateTimeSlot1 = editor.CreatePropertyType("dateTimeSlot1", DataType.DateTime);
			PropertyType dateTimeSlot2 = editor.CreatePropertyType("dateTimeSlot2", DataType.DateTime);
			PropertyType currencySlot1 = editor.CreatePropertyType("currencySlot1", DataType.Currency);
			PropertyType currencySlot2 = editor.CreatePropertyType("currencySlot2", DataType.Currency);
			PropertyType refSlot1 = editor.CreatePropertyType("refSlot1", DataType.Reference);
			PropertyType refSlot2 = editor.CreatePropertyType("refSlot2", DataType.Reference);

			NodeQuery query = new NodeQuery();

			//==== Operators
			ExpressionList strOpExp = new ExpressionList(ChainOperator.Or);
			query.Add(strOpExp);
			strOpExp.Add(new StringExpression(StringAttribute.Path, StringOperator.Contains, "{path}"));
			strOpExp.Add(new StringExpression(StringAttribute.Path, StringOperator.EndsWith, "{path}"));
			strOpExp.Add(new StringExpression(StringAttribute.Path, StringOperator.Equal, "{path}"));
			strOpExp.Add(new StringExpression(StringAttribute.Path, StringOperator.GreaterThan, "{path}"));
			strOpExp.Add(new StringExpression(StringAttribute.Path, StringOperator.GreaterThanOrEqual, "{path}"));
			strOpExp.Add(new StringExpression(StringAttribute.Path, StringOperator.LessThan, "{path}"));
			strOpExp.Add(new StringExpression(StringAttribute.Path, StringOperator.LessThanOrEqual, "{path}"));
			strOpExp.Add(new StringExpression(StringAttribute.Path, StringOperator.NotEqual, "{path}"));
			strOpExp.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith, "{path}"));

			//==== StringExpression
			ExpressionList strExp = new ExpressionList(ChainOperator.Or);
			query.Add(strExp);
			strExp.Add(new StringExpression(stringSlot1, StringOperator.Equal, "{value}"));
			strExp.Add(new StringExpression(stringSlot1, StringOperator.Equal, stringSlot2));
			strExp.Add(new StringExpression(stringSlot1, StringOperator.Equal, StringAttribute.Path));
			strExp.Add(new StringExpression(stringSlot1, StringOperator.Equal, (string)null));
			strExp.Add(new StringExpression(StringAttribute.Name, StringOperator.Equal, "{value}"));
			strExp.Add(new StringExpression(StringAttribute.Name, StringOperator.Equal, stringSlot2));
			strExp.Add(new StringExpression(StringAttribute.Name, StringOperator.Equal, StringAttribute.Path));
			strExp.Add(new StringExpression(StringAttribute.Name, StringOperator.Equal, (string)null));

			//==== IntExpression
			ExpressionList intExp = new ExpressionList(ChainOperator.Or);
			query.Add(intExp);
			intExp.Add(new IntExpression(IntAttribute.Index, ValueOperator.Equal, 123));
			intExp.Add(new IntExpression(IntAttribute.Index, ValueOperator.Equal, IntAttribute.MajorVersion));
			intExp.Add(new IntExpression(IntAttribute.Index, ValueOperator.Equal, intSlot2));
			intExp.Add(new IntExpression(IntAttribute.Index, ValueOperator.Equal, (int?)null));
			intExp.Add(new IntExpression(intSlot1, ValueOperator.Equal, 123));
			intExp.Add(new IntExpression(intSlot1, ValueOperator.Equal, IntAttribute.MajorVersion));
			intExp.Add(new IntExpression(intSlot1, ValueOperator.Equal, intSlot2));
			intExp.Add(new IntExpression(intSlot1, ValueOperator.Equal, (int?)null));

			//==== DateTimeExpression
			ExpressionList dtExp = new ExpressionList(ChainOperator.Or);
			query.Add(dtExp);
			dtExp.Add(new DateTimeExpression(DateTimeAttribute.CreationDate, ValueOperator.Equal, DateTime.Now));
			dtExp.Add(new DateTimeExpression(DateTimeAttribute.CreationDate, ValueOperator.Equal, DateTimeAttribute.ModificationDate));
			dtExp.Add(new DateTimeExpression(DateTimeAttribute.CreationDate, ValueOperator.Equal, dateTimeSlot2));
			dtExp.Add(new DateTimeExpression(DateTimeAttribute.CreationDate, ValueOperator.Equal, (DateTime?)null));
			dtExp.Add(new DateTimeExpression(dateTimeSlot1, ValueOperator.Equal, DateTime.Now));
			dtExp.Add(new DateTimeExpression(dateTimeSlot1, ValueOperator.Equal, DateTimeAttribute.ModificationDate));
			dtExp.Add(new DateTimeExpression(dateTimeSlot1, ValueOperator.Equal, dateTimeSlot2));
			dtExp.Add(new DateTimeExpression(dateTimeSlot1, ValueOperator.Equal, (DateTime?)null));

			//==== CurrencyExpression
			ExpressionList curExp = new ExpressionList(ChainOperator.Or);
			query.Add(curExp);
			curExp.Add(new CurrencyExpression(currencySlot1, ValueOperator.Equal, (decimal)123.456));
			curExp.Add(new CurrencyExpression(currencySlot1, ValueOperator.Equal, currencySlot2));
			curExp.Add(new CurrencyExpression(currencySlot1, ValueOperator.Equal, (decimal?)null));

			//==== ReferenceExpression
			ExpressionList subExp = new ExpressionList(ChainOperator.And);
			subExp.Add(new IntExpression(IntAttribute.Index, ValueOperator.GreaterThan, 123));
			subExp.Add(new DateTimeExpression(DateTimeAttribute.CreationDate, ValueOperator.GreaterThan, DateTime.Now));
			ExpressionList refExp = new ExpressionList(ChainOperator.Or);
			query.Add(refExp);
			refExp.Add(new ReferenceExpression(refSlot1));
			refExp.Add(new ReferenceExpression(ReferenceAttribute.LockedBy));
			refExp.Add(new ReferenceExpression(refSlot1, (Node)null));
			refExp.Add(new ReferenceExpression(refSlot1, Repository.Root));
			refExp.Add(new ReferenceExpression(ReferenceAttribute.LockedBy, (Node)null));
			refExp.Add(new ReferenceExpression(ReferenceAttribute.LockedBy, Repository.Root));
			refExp.Add(new ReferenceExpression(refSlot1, subExp));
			refExp.Add(new ReferenceExpression(ReferenceAttribute.LockedBy, subExp));

			//==== TypeExpression
			ExpressionList typeExp = new ExpressionList(ChainOperator.Or);
			query.Add(typeExp);
			typeExp.Add(new TypeExpression(nodeType1));
			typeExp.Add(new TypeExpression(nodeType2, true));

			//==== Negation
			Expression negExp = new NotExpression(
				new ExpressionList(ChainOperator.And,
					new StringExpression(StringAttribute.Path, StringOperator.StartsWith, "/Root1/"),
					new StringExpression(StringAttribute.Name, StringOperator.NotEqual, "name")
					));
			query.Add(negExp);

			//==== Orders
			query.Orders.Add(new SearchOrder(DateTimeAttribute.ModificationDate, OrderDirection.Desc));
			query.Orders.Add(new SearchOrder(IntAttribute.MajorVersion, OrderDirection.Asc));
			query.Orders.Add(new SearchOrder(StringAttribute.Name, OrderDirection.Asc));

			//==== Paging
			query.PageSize = 123;
			query.StartIndex = 987;

			string queryString = query.ToXml();
			NodeQueryAccessor queryAcc = new NodeQueryAccessor(new NodeQuery());
			NodeQuery newQuery = queryAcc.Parse(queryString, editor);
			string newQueryString = newQuery.ToXml();

			Assert.IsTrue(queryString != null && queryString == newQueryString);
		}

		[TestMethod]
        public void NodeQuery_Bug2125()
        {
            Expression exp;
            ExpressionList expList;

            var query1 = new NodeQuery();
            exp = new SearchExpression("dummy");
            query1.Add(exp);

            Assert.IsTrue(Object.ReferenceEquals( exp.Parent, query1), "#1");

            expList = new ExpressionList(ChainOperator.And);
            query1.Add(expList);

            Assert.IsTrue(Object.ReferenceEquals(expList.Parent, query1), "#2");

            exp = new StringExpression(StringAttribute.Name, StringOperator.Equal, "Root");
            expList.Add(exp);

            Assert.IsTrue(Object.ReferenceEquals(exp.Parent, expList), "#3");

            exp = new IntExpression(IntAttribute.Id, ValueOperator.Equal, 2);
            expList.Add(exp);

            Assert.IsTrue(Object.ReferenceEquals(exp.Parent, expList), "#4");

            //------------------------------------------------------------------------------------

            var query2 = new NodeQuery
            (
                new SearchExpression("dummy"),
                new ExpressionList
                (
                    ChainOperator.And,
                    new StringExpression(StringAttribute.Name, StringOperator.Equal, "Root"),
                    new IntExpression(IntAttribute.Id, ValueOperator.Equal, 2)
                )
            );

            Assert.IsTrue(Object.ReferenceEquals(query2.Expressions[0].Parent, query2), "#5");
            Assert.IsTrue(Object.ReferenceEquals(query2.Expressions[1].Parent, query2), "#6");
            Assert.IsTrue(Object.ReferenceEquals(((ExpressionList)query2.Expressions[1]).Expressions[0].Parent, query2.Expressions[1]), "#7");
            Assert.IsTrue(Object.ReferenceEquals(((ExpressionList)query2.Expressions[1]).Expressions[1].Parent, query2.Expressions[1]), "#8");
        }

    }
}