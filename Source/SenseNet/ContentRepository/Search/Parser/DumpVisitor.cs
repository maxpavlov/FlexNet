using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Search;
using Lucene.Net.Index;
using SenseNet.Search.Parser;
using SenseNet.Search.Indexing;

namespace SenseNet.Search.Parser
{
    internal class DumpVisitor : LucQueryVisitor
    {
        private StringBuilder _dump = new StringBuilder();

        public override Query VisitBooleanQuery(BooleanQuery booleanq)
        {
            _dump.Append("BoolQ(");
            var clauses = booleanq.GetClauses();
            var visitedClauses = VisitBooleanClauses(clauses);
            BooleanQuery newQuery = null;
            if (visitedClauses != clauses)
            {
                newQuery = new BooleanQuery(booleanq.IsCoordDisabled());
                for (int i = 0; i < visitedClauses.Length; i++)
                    newQuery.Add(clauses[i]);
            }
            _dump.Append(")");
            return newQuery ?? booleanq;
        }
        public override Query VisitPhraseQuery(PhraseQuery phraseq)
        {
            _dump.Append("PhraseQ(");

            var terms = phraseq.GetTerms();
            PhraseQuery newQuery = null;

            int index = 0;
            int count = terms.Length;
            while (index < count)
            {
                var visitedTerm = VisitTerm(terms[index]);
                if (newQuery != null)
                {
                    newQuery.Add(visitedTerm);
                }
                else if (visitedTerm != terms[index])
                {
                    newQuery = new PhraseQuery();
                    for (int i = 0; i < index; i++)
                        newQuery.Add(terms[i]);
                    newQuery.Add(visitedTerm);
                }
                index++;
                if (index < count)
                    _dump.Append(", ");
            }
            _dump.Append(", Slop:").Append(phraseq.GetSlop()).Append(BoostToString(phraseq)).Append(")");
            if (newQuery != null)
                return newQuery;
            return phraseq;
        }
        public override Query VisitPrefixQuery(PrefixQuery prefixq)
        {
            _dump.Append("PrefixQ(");
            var q = base.VisitPrefixQuery(prefixq);
            _dump.Append(BoostToString(q));
            _dump.Append(")");
            return q;
        }
        public override Query VisitFuzzyQuery(FuzzyQuery fuzzyq)
        {
            _dump.Append("FuzzyQ(");
            var q = base.VisitFuzzyQuery(fuzzyq);
            var fq = q as FuzzyQuery;
            if (fq != null)
            {
                _dump.Append(", minSimilarity:");
                _dump.Append(fq.GetMinSimilarity());
            }
            _dump.Append(BoostToString(q));
            _dump.Append(")");
            return q;
        }
        public override Query VisitWildcardQuery(WildcardQuery wildcardq)
        {
            _dump.Append("WildcardQ(");
            var q = base.VisitWildcardQuery(wildcardq);
            _dump.Append(BoostToString(q));
            _dump.Append(")");
            return q;
        }
        public override Query VisitTermQuery(TermQuery termq)
        {
            _dump.Append("TermQ(");
            var q = base.VisitTermQuery(termq);
            _dump.Append(BoostToString(q));
            _dump.Append(")");
            return q;
        }

        public override Query VisitConstantScoreQuery(ConstantScoreQuery constantScoreq) { throw new NotImplementedException(); }
        public override Query VisitConstantScoreRangeQuery(ConstantScoreRangeQuery constantScoreRangeq)
        {
            var q = (ConstantScoreRangeQuery)base.VisitConstantScoreRangeQuery(constantScoreRangeq);
            _dump.AppendFormat("ConstantScoreRangeQ({0}:{1}{2} TO {3}{4}{5})",
                q.GetField(), q.IncludesLower() ? "[" : "{",
                q.GetLowerVal(), q.GetUpperVal(), q.IncludesUpper() ? "]" : "}", BoostToString(q));
            return q;
        }
        public override Query VisitCustomScoreQuery(Lucene.Net.Search.Function.CustomScoreQuery customScoreq) { throw new NotImplementedException(); }
        public override Query VisitDisjunctionMaxQuery(DisjunctionMaxQuery disjunctionMaxq) { throw new NotImplementedException(); }
        public override Query VisitFieldScoreQuery(Lucene.Net.Search.Function.FieldScoreQuery fieldScoreq) { throw new NotImplementedException(); }
        public override Query VisitFilteredQuery(FilteredQuery filteredq) { throw new NotImplementedException(); }
        public override Query VisitMatchAllDocsQuery(MatchAllDocsQuery matchAllDocsq) { throw new NotImplementedException(); }
        public override Query VisitMultiPhraseQuery(MultiPhraseQuery multiPhraseq) { throw new NotImplementedException(); }
        public override Query VisitRangeQuery(RangeQuery rangeq) { throw new NotImplementedException(); }
        public override Query VisitSpanFirstQuery(Lucene.Net.Search.Spans.SpanFirstQuery spanFirstq) { throw new NotImplementedException(); }
        public override Query VisitSpanNearQuery(Lucene.Net.Search.Spans.SpanNearQuery spanNearq) { throw new NotImplementedException(); }
        public override Query VisitSpanNotQuery(Lucene.Net.Search.Spans.SpanNotQuery spanNotq) { throw new NotImplementedException(); }
        public override Query VisitSpanOrQuery(Lucene.Net.Search.Spans.SpanOrQuery spanOrq) { throw new NotImplementedException(); }
        public override Query VisitSpanTermQuery(Lucene.Net.Search.Spans.SpanTermQuery spanTermq) { throw new NotImplementedException(); }
        public override Query VisitValueSourceQuery(Lucene.Net.Search.Function.ValueSourceQuery valueSourceq) { throw new NotImplementedException(); }
        public override Query VisitTermRangeQuery(TermRangeQuery termRangeq)
        {
            var q = (TermRangeQuery)base.VisitTermRangeQuery(termRangeq);
            _dump.AppendFormat("TermRangeQ({0}:{1}{2} TO {3}{4}{5})",
                q.GetField(), q.IncludesLower() ? "[" : "{",
                q.GetLowerTerm(), q.GetUpperTerm(), q.IncludesUpper() ? "]" : "}", BoostToString(q));
            return q;
        }
        public override Query VisitNumericRangeQuery(NumericRangeQuery numericRangeq)
        {
            var q = (NumericRangeQuery)base.VisitNumericRangeQuery(numericRangeq);
            _dump.AppendFormat("NumericRangeQ({0}:{1}{2} TO {3}{4}{5})",
                q.GetField(), q.IncludesMin() ? "[" : "{",
                q.GetMin(), q.GetMax(), q.IncludesMax() ? "]" : "}", BoostToString(q));
            return q;
        }

        public override BooleanClause VisitBooleanClause(BooleanClause clause)
        {
            var cl = clause.GetOccur();
            var clString = cl == null ? " " : cl.ToString();
            if (clString == "")
                clString = " ";
            _dump.Append("Cl((").Append(clString).Append("), ");
            var c = base.VisitBooleanClause(clause);
            _dump.Append(")");
            return c;
        }
        public override Term VisitTerm(Term term)
        {
            _dump.Append("T(");
            var t = base.VisitTerm(term);
            _dump.Append(t);
            _dump.Append(")");
            return t;
        }

        private string BoostToString(Query query)
        {
            var sb = new StringBuilder();
            var boost = query.GetBoost();
            if (boost != 1.0)
                sb.Append(", Boost(").Append(boost).Append(")");
            return sb.ToString();
        }

        public override string ToString()
        {
            return _dump.ToString();
        }
    }

    internal class SnLucToSqlCompiler
    {
        public string Compile(Query query, int top, int skip, SortField[] orders, out SenseNet.ContentRepository.Storage.Search.NodeQueryParameter[] parameters)
        {
            throw new NotImplementedException("Partially implemented.");

            if(skip > 0)
                throw new NotImplementedException("Paging is not implemented (skip > 0).");

            var whereCompiler = new SqlWhereVisitor();
            whereCompiler.Visit(query);

            var sb = new StringBuilder();
            sb.Append("SELECT");
            if(top > 0)
                sb.Append(" TOP ").Append(top);
            sb.AppendLine(" NodeId FROM Nodes");
            sb.Append("WHERE ");

            sb.AppendLine(whereCompiler.ToString());

            if (orders.Count() > 0)
            {
                sb.Append("ORDER BY ");
                sb.AppendLine(String.Join(", ", orders.Select(o => o.GetField() + (o.GetReverse() ? " DESC" : String.Empty)).ToArray()));
            }

            parameters = whereCompiler.Parameters;
            return sb.ToString();
        }
    }
    internal class SqlWhereVisitor : LucQueryVisitor
    {
        private StringBuilder _sql = new StringBuilder();
        private List<string> _paramNames = new List<string>();
        private List<SenseNet.ContentRepository.Storage.Search.NodeQueryParameter> _parameters = new List<SenseNet.ContentRepository.Storage.Search.NodeQueryParameter>();
        public SenseNet.ContentRepository.Storage.Search.NodeQueryParameter[] Parameters
        {
            get { return _parameters.ToArray(); }
        }
        private Stack<string> _operators = new Stack<string>();

        public SqlWhereVisitor()
        {
            _operators.Push(" = ");
        }

        public override Query VisitPhraseQuery(PhraseQuery phraseq)
        {
            throw new NotSupportedException("Cannot compile PhraseQuery to SQL expression.");
        }
        public override Query VisitFuzzyQuery(FuzzyQuery fuzzyq)
        {
            throw new NotSupportedException("Cannot compile FuzzyQuery to SQL expression.");
        }
        public override Query VisitPrefixQuery(PrefixQuery prefixq)
        {
            _operators.Push("LIKE%");
            var q = base.VisitPrefixQuery(prefixq);
            _operators.Pop();
            return q;
        }
        public override Query VisitWildcardQuery(WildcardQuery wildcardq)
        {
            var pattern = wildcardq.GetTerm().Text();

            if (pattern.Contains("?"))
                throw new NotSupportedException("Cannot compile WildcardQuery, which contains '?', to SQL expression");

            if (pattern.StartsWith("*") && pattern.EndsWith("*"))
                _operators.Push("%LIKE%");
            else if (pattern.StartsWith("*"))
                _operators.Push("%LIKE");
            else if (pattern.EndsWith("*"))
                _operators.Push("LIKE%");

            var q = base.VisitWildcardQuery(wildcardq);
            _operators.Pop();
            return q;
        }
        public override Query VisitTermQuery(TermQuery termq)
        {
            var q = base.VisitTermQuery(termq);
            return q;
        }
        public override Query VisitConstantScoreRangeQuery(ConstantScoreRangeQuery constantScoreRangeq)
        {
            throw new NotSupportedException("Cannot compile ConstantScoreRangeQuery to SQL expression.");
        }
        public override Query VisitTermRangeQuery(TermRangeQuery termRangeq)
        {
            var q = (TermRangeQuery)base.VisitTermRangeQuery(termRangeq);
            CompileRange(q.GetField(), q.GetLowerTerm(), q.GetUpperTerm(), q.IncludesLower(), q.IncludesUpper());
            return q;
        }
        public override Query VisitNumericRangeQuery(NumericRangeQuery numericRangeq)
        {
            var q = (NumericRangeQuery)base.VisitNumericRangeQuery(numericRangeq);
            CompileRange(q.GetField(), q.GetMin().ToString(), q.GetMax().ToString(), q.IncludesMin(), q.IncludesMax());
            return q;
        }
        /*todo*/private void CompileRange(string fieldName, string lowerTerm, string upperTerm, bool incLower, bool incUpper)
        {
            if (lowerTerm != null && upperTerm != null)
            {
                //TODO: full range
                throw new NotImplementedException();
            }
            else
            {
                Term t = null;
                if (upperTerm == null)
                {
                    _operators.Push(incLower ? " >= " : " > ");
                    t = new Term(fieldName, lowerTerm);
                }
                else if (lowerTerm == null)
                {
                    _operators.Push(incUpper ? " <= " : " < ");
                    t = new Term(fieldName, upperTerm);
                }
                VisitTerm(t);
                _operators.Pop();
            }
        }
        /*todo*/public override BooleanClause[] VisitBooleanClauses(BooleanClause[] clauses)
        {
            List<BooleanClause> newList = null;

            clauses = new BooleanClauseOptimizer().VisitBooleanClauses(clauses);

            int index = 0;
            int count = clauses.Length;
            while (index < count)
            {
                var visitedClause = VisitBooleanClause(clauses[index]);
                if (newList != null)
                {
                    newList.Add(visitedClause);
                }
                else if (visitedClause != clauses[index])
                {
                    newList = new List<BooleanClause>();
                    for (int i = 0; i < index; i++)
                        newList.Add(clauses[i]);
                    newList.Add(visitedClause);
                }
                index++;
            }
            return newList != null ? newList.ToArray() : clauses;
        }

        public override Term VisitTerm(Term term)
        {
            var t = base.VisitTerm(term);
            CompileTerm(t);
            return t;
        }
        private void CompileTerm(Term t)
        {
            var fieldName = t.Field();
            var value = t.Text();

            //--
            string sqlName;
            string @operator = null;
            var inOperator = false;
            var needToApos = false;
            switch (fieldName)
            {
                case "Id":
                    sqlName = "NodeId";
                    break;
                case "Type":
                    sqlName = "NodeTypeId";
                    value = GetNodeTypeValue(value, false);
                    break;
                case "TypeIs":
                    sqlName = "NodeTypeId";
                    @operator = "IN";
                    inOperator = true;
                    value = GetNodeTypeValue(value, true);
                    break;
                case "InTree":
                    sqlName = "Path";
                    @operator = "LIKE%";
                    break;
                case "InFolder":
                    sqlName = "ParentNodeId";
                    value = SenseNet.ContentRepository.Storage.NodeHead.Get(value).Id.ToString();
                    break;
                case "CreationDate":
                case "ModificationDate":
                case "Name":
                case "DisplayName":
                case "Path":
                    sqlName = fieldName;
                    needToApos = true;
                    break;
                case "IsInherited":
                    sqlName = fieldName;
                    value = BooleanIndexHandler.YesList.Contains(value.ToLower()) ? "1" : "0";
                    break;
                case "LastMinorVersionId":
                case "LastMajorVersionId":
                case "Index":
                case "ParentNodeId":
                case "ContentListTypeId":
                case "ContentListId":
                case "LockedById":
                case "ModifiedById":
                case "CreatedById":
                    sqlName = fieldName; break;
                default:
                    throw new NotSupportedException("Cannot compile to SQL expression. Field is not supported: " + fieldName);
            }

            //--
            var paramName = sqlName;
            var index = 0;
            while (_paramNames.Contains(paramName))
                paramName = fieldName + ++index;
            paramName = "@" + paramName;

            //--

            if (@operator == null)
            {
                var peek = _operators.Peek();
                switch (peek)
                {
                    case "LIKE%":
                        @operator = " LIKE ";
                        value = String.Concat("'", value.Trim('*'), "%'");
                        needToApos = false;
                        break;
                    case "%LIKE":
                        @operator = " LIKE ";
                        value = String.Concat("'%", value.Trim('*'), "'");
                        needToApos = false;
                        break;
                    case "%LIKE%":
                        @operator = " LIKE ";
                        value = String.Concat("'%", value.Trim('*'), "%'");
                        needToApos = false;
                        break;
                    default:
                        @operator = peek;
                        break;
                }
            }
            if (needToApos)
                value = "'" + value + "'";
            if (inOperator)
            {
                _sql.Append(sqlName).Append(" IN ").Append(value);
            }
            else
            {
                _parameters.Add(new SenseNet.ContentRepository.Storage.Search.NodeQueryParameter { Name = paramName, Value = value });
                _sql.Append(sqlName).Append(@operator).Append(paramName);
            }
        }

        private string GetNodeTypeValue(string typeName, bool recursive)
        {
            var nodeType = SenseNet.ContentRepository.Storage.ActiveSchema.NodeTypes.Where(n => n.Name.ToLower() == typeName).FirstOrDefault();
            if (nodeType == null)
                throw new ApplicationException("Type is not found: " + typeName);
            if (!recursive)
                return nodeType.Id.ToString();
            return String.Concat(
                "(",
                String.Join(", ", nodeType.GetAllTypes().Select(t => t.Id.ToString()).ToArray()),
                ")");
        }

        public override string ToString()
        {
            return _sql.ToString();
        }

        private class BooleanClauseOptimizer : LucQueryVisitor
        {
            public override BooleanClause[] VisitBooleanClauses(BooleanClause[] clauses)
            {
                List<BooleanClause> newList = null;
                int index = 0;
                int count = clauses.Length;
                while (index < count)
                {
                    var visitedClause = VisitBooleanClause(clauses[index]);
                    if (newList != null)
                    {
                        newList.Add(visitedClause);
                    }
                    else if (visitedClause != clauses[index])
                    {
                        newList = new List<BooleanClause>();
                        for (int i = 0; i < index; i++)
                            newList.Add(clauses[i]);
                        newList.Add(visitedClause);
                    }
                    index++;
                }
                if (newList == null)
                    return OptimizeBooleanClauses(clauses);
                return OptimizeBooleanClauses(newList);
            }
            private BooleanClause[] OptimizeBooleanClauses(IEnumerable<BooleanClause> clauses)
            {
                var shouldCount = 0;
                var mustCount = 0;
                foreach (var clause in clauses)
                {
                    var occur = clause.GetOccur();
                    if (occur == null || occur == BooleanClause.Occur.SHOULD)
                        shouldCount++;
                    else if (occur == BooleanClause.Occur.MUST)
                        mustCount++;
                }
                if (mustCount * shouldCount == 0)
                    return clauses.ToArray();
                var newList = new List<BooleanClause>();
                foreach (var clause in clauses)
                {
                    var occur = clause.GetOccur();
                    if (occur != null && occur != BooleanClause.Occur.SHOULD)
                        newList.Add(clause);
                }
                return newList.ToArray();
            }
        }
    }

}
