//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Text;
//using Lucene.Net.Documents;
//using SenseNet.ContentRepository.Storage;
//using SnD = SenseNet.Diagnostics;
//using SnField = SenseNet.ContentRepository.Field;
//using SnS = SenseNet.ContentRepository.Storage;
//using SnSS = SenseNet.ContentRepository.Storage.Schema;

//namespace SenseNet.Search.Indexing
//{
//    public class DocumentBuilder
//    {
//        private static readonly string[] ExpectedFieldNames = new string[] {
//            LucObject.FieldName.NodeId,
//            LucObject.FieldName.Name,
//            LucObject.FieldName.Path,
//            LucObject.FieldName.Depth,
//            LucObject.FieldName.InTree,
//            LucObject.FieldName.VersionId,
//            LucObject.FieldName.Version,
//            LucObject.FieldName.CreatedById,
//            LucObject.FieldName.ModifiedById,
//        };

//        private static InTreeIndexHandler __inTreeHandler;
//        private static InTreeIndexHandler InTreeIndexHandlerInstance
//        {
//            get
//            {
//                if (__inTreeHandler == null)
//                {
//                    var start = SenseNet.ContentRepository.Schema.ContentTypeManager.Current;
//                    __inTreeHandler = new InTreeIndexHandler { OwnerIndexingInfo = SenseNet.ContentRepository.Schema.ContentTypeManager.GetPerFieldIndexingInfo(LucObject.FieldName.InTree) };
//                }
//                return __inTreeHandler;
//            }
//        }

//        /// <summary>
//        /// Converts the permitted version of a <see cref="SenseNet.ContentRepository.Storage.Node"/> to <see cref="Lucene.Net.Documents.Document"/>. Node cannot be null.
//        /// </summary>
//        /// <param name="node"></param>
//        /// <returns>Converted <see cref="Lucene.Net.Documents.Document"/>.</returns>
//        /// <exception cref="System.ArgumentNullException" />
//        public static Document GetDocument(SnS.Node node)
//        {
//            var head = SnS.NodeHead.Get(node.Id);
//            return GetDocument(node, head);
//        }
//        /// <summary>
//        /// Converts the permitted version of a <see cref="SenseNet.ContentRepository.Storage.Node"/> to <see cref="Lucene.Net.Documents.Document"/>.
//        /// Node, and NodeHead cannot be null.
//        /// </summary>
//        /// <param name="node">A <see cref="SenseNet.ContentRepository.Storage.Node"/> that will be converted. Cannot be null.</param>
//        /// <param name="head"><see cref="SenseNet.ContentRepository.Storage.NodeHead"/> of the passed node. Cannot be null.</param>
//        /// <returns>Converted <see cref="Lucene.Net.Documents.Document"/>.</returns>
//        /// <exception cref="System.ArgumentNullException" />
//        /// <exception cref="System.InvalidOperationException" />
//        public static Document GetDocument(SnS.Node node, SnS.NodeHead head)
//        {
//            if(head == null)
//                throw new ArgumentNullException("head");
//            if (head.Id != node.Id)
//                throw new InvalidOperationException("Id of node and Id of head must be equal.");

//            bool isLastPublic = head.LastMajorVersionId == node.VersionId;
//            bool isLastDraft = head.LastMinorVersionId == node.VersionId;
//            //var isPublic = node.Version.IsMajor && (node.Version.Status != SnS.VersionStatus.Locked);
//            var isPublic = node.Version.Status == SenseNet.ContentRepository.Storage.VersionStatus.Approved;
//            return GetDocument(node, isPublic, isLastPublic, isLastDraft);
//        }
//        /// <summary>
//        /// Converts a <see cref="SenseNet.ContentRepository.Storage.Node"/> to <see cref="Lucene.Net.Documents.Document"/>. Node cannot be null.
//        /// </summary>
//        /// <param name="node">A <see cref="SenseNet.ContentRepository.Storage.Node"/> that will be converted. Cannot be null</param>
//        /// <param name="isPublic">Be true if this version of the node is approved.</param>
//        /// <param name="isLastPublic">Be true if this is the last approved version of the node.</param>
//        /// <param name="isLastDraft">Be true if this is the last version of the node.</param>
//        /// <returns>Converted <see cref="Lucene.Net.Documents.Document"/>.</returns>
//        /// <exception cref="System.ArgumentNullException"></exception>
//        public static Document GetDocument(SnS.Node node, bool isPublic, bool isLastPublic, bool isLastDraft)
//        {
//            if (node == null)
//                throw new ArgumentNullException("node");

//            var textEtract = new StringBuilder();
//            var doc = new Document();

//            var ixnode = node as IIndexableDocument;

//            Field f;
//            NumericField nf;
//            if (ixnode == null)
//            {
//                //LucObject.FieldName.NodeId
//                nf = new NumericField(LucObject.FieldName.NodeId, Field.Store.YES, true);
//                nf.SetIntValue(node.Id);
//                doc.Add(nf);

//                //LucObject.FieldName.Name
//                f = new Field(LucObject.FieldName.Name, node.Name.ToLower(), Field.Store.YES, Field.Index.ANALYZED);
//                doc.Add(f);

//                //LucObject.FieldName.Path
//                f = new Field(LucObject.FieldName.Path, node.Path.ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS);
//                doc.Add(f);

//                //LucObject.FieldName.Depth
//                nf = new NumericField(LucObject.FieldName.Depth, Field.Store.YES, true);
//                nf.SetIntValue(DepthIndexHandler.GetDepth(node.Path));
//                doc.Add(nf);

//                //LucObject.FieldName.VersionId
//                nf = new NumericField(LucObject.FieldName.VersionId, Field.Store.YES, true);
//                nf.SetIntValue(node.VersionId);
//                doc.Add(nf);

//                //LucObject.FieldName.Version
//                f = new Field(LucObject.FieldName.Version, node.Version.ToString().ToLower(), Field.Store.YES, Field.Index.ANALYZED);
//                doc.Add(f);

//                //LucObject.FieldName.CreatedById
//                nf = new NumericField(LucObject.FieldName.CreatedById, Field.Store.YES, true);
//                nf.SetIntValue(node.CreatedById);
//                doc.Add(nf);

//                //LucObject.FieldName.ModifiedById
//                nf = new NumericField(LucObject.FieldName.ModifiedById, Field.Store.YES, true);
//                nf.SetIntValue(node.ModifiedById);
//                doc.Add(nf);

//                //LucObject.FieldName.InTree
//                var fields = InTreeIndexHandlerInstance.GetIndexFields(LucObject.FieldName.InTree, node.Path);
//                foreach (var field in fields)
//                    doc.Add(field);
//            }
//            else
//            {
//                var fieldNames = new List<string>();
//                foreach (var field in ixnode.GetIndexableFields())
//                {
//                    string extract;
//                    var lucFields = field.GetIndexFields(out extract);
//                    textEtract.AppendLine(extract);
//                    if (lucFields != null)
//                    {
//                        foreach (var lucField in lucFields)
//                        {
//                            fieldNames.Add(lucField.Name());
//                            doc.Add(lucField);
//                        }
//                    }
//                }
//                if (ExpectedFieldNames.Except(fieldNames).Count() > 0)
//                    throw new ApplicationException("Cannot index the document.");
//            }
//            nf = new NumericField(LucObject.FieldName.NodeTimestamp, Field.Store.YES, true);
//            nf.SetLongValue(node.NodeTimestamp);
//            doc.Add(nf);

//            nf = new NumericField(LucObject.FieldName.VersionTimestamp, Field.Store.YES, true);
//            nf.SetLongValue(node.VersionTimestamp);
//            doc.Add(nf);

//            doc.Add(new Field(LucObject.FieldName.IsInherited, node.IsInherited ? BooleanIndexHandler.YES : BooleanIndexHandler.NO, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
//            doc.Add(new Field(LucObject.FieldName.IsMajor, node.Version.IsMajor ? BooleanIndexHandler.YES : BooleanIndexHandler.NO, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
//            doc.Add(new Field(LucObject.FieldName.IsPublic, isPublic ? BooleanIndexHandler.YES : BooleanIndexHandler.NO, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
//            doc.Add(new Field(LucObject.FieldName.IsLastPublic, isLastPublic ? BooleanIndexHandler.YES : BooleanIndexHandler.NO, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
//            doc.Add(new Field(LucObject.FieldName.IsLastDraft, isLastDraft ? BooleanIndexHandler.YES : BooleanIndexHandler.NO, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));

//            doc.Add(new Field(LucObject.FieldName.AllText, textEtract.ToString(), Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO));

//            return doc;
//        }

//        //public static Document GetDocument(SnS.Node node, bool isPublic, bool isLastPublic, bool isLastDraft)
//        //{
//        //    //<##>
//        //    return GetDocument2(node, isPublic, isLastPublic, isLastDraft);
//        //    //</##>

//        //    var fullText = new StringBuilder();

//        //    var doc = new Document();

//        //    // int, stored
//        //    CreateNodeIdField(doc, LucObject.FieldName.NodeId, node.Id, fullText);
//        //    CreateIntField(doc, LucObject.FieldName.CreatedById, node.CreatedById, fullText, true);
//        //    CreateIntField(doc, LucObject.FieldName.ModifiedById, node.ModifiedById, fullText, true);
//        //    CreateIntField(doc, LucObject.FieldName.VersionId, node.VersionId, fullText, true);

//        //    // int, not stored 
//        //    CreateParentIdField(doc, LucObject.FieldName.ParentId, node.ParentId, fullText);
//        //    CreateIntField(doc, LucObject.FieldName.NodeTypeId, node.NodeType.Id, fullText);
//        //    CreateIntField(doc, LucObject.FieldName.ContentListId, node.ContentListId, fullText);
//        //    CreateIntField(doc, LucObject.FieldName.ContentListTypeId, node.ContentListType != null ? node.ContentListType.Id : 0, fullText);
//        //    CreateIntField(doc, LucObject.FieldName.Index, node.Index, fullText);
//        //    CreateIntField(doc, LucObject.FieldName.LockedById, node.LockedById, fullText);
//        //    CreateIntField(doc, LucObject.FieldName.MajorNumber, node.Version.Major, fullText);
//        //    CreateIntField(doc, LucObject.FieldName.MinorNumber, node.Version.Minor, fullText);

//        //    // date
//        //    CreateDateTimeField(doc, LucObject.FieldName.CreationDate, node.CreationDate, fullText);
//        //    CreateDateTimeField(doc, LucObject.FieldName.LockDate, node.LockDate, fullText);
//        //    CreateDateTimeField(doc, LucObject.FieldName.ModificationDate, node.ModificationDate, fullText);

//        //    CreateShortTextField(doc, LucObject.FieldName.Name, node.Name, fullText, true);
//        //    CreateShortTextField(doc, LucObject.FieldName.Path, node.Path, fullText, true);
//        //    CreatePathField(doc, LucObject.FieldName.InTree, node.Path, fullText);
//        //    CreatePathField(doc, LucObject.FieldName.TypeIs, node.NodeType.NodeTypePath, fullText);
//        //    CreateVersionField(doc, LucObject.FieldName.Version, node.Version, fullText);
//        //    CreateVersionStatusField(doc, LucObject.FieldName.VersionStatus, node.Version, fullText);

//        //    CreateBooleanField(doc, LucObject.FieldName.IsInherited, node.IsInherited, fullText);
//        //    CreateBooleanField(doc, LucObject.FieldName.IsMajor, node.Version.IsMajor, fullText);
//        //    CreateBooleanField(doc, LucObject.FieldName.IsPublic, isPublic, fullText);
//        //    CreateBooleanField(doc, LucObject.FieldName.IsLastPublic, isLastPublic, fullText);
//        //    CreateBooleanField(doc, LucObject.FieldName.IsLastDraft, isLastDraft, fullText);

//        //    foreach (var propType in node.PropertyTypes)
//        //        CreateDynamicField(doc, node, propType, fullText);

//        //    CreateFormattedField(doc, LucObject.FieldName.AllText, fullText.ToString(), false, true, false);
//        //    return doc;
//        //}
//        //private static void CreateBooleanField(Document doc, string name, bool value, StringBuilder fullText)
//        //{
//        //    CreateFormattedField(doc, name, ValueFormatter.Format("Boolean", value), true, true, false);
//        //}
//        //private static void CreateNodeIdField(Document doc, string name, int nodeId, StringBuilder fullText)
//        //{
//        //    CreateFormattedField(doc, name, ValueFormatter.Format(SnSS.DataType.Int, nodeId), true, true, false);
//        //}
//        //private static void CreatePathField(Document doc, string name, string path, StringBuilder fullText)
//        //{
//        //    //CreateFormattedField(doc, name, ValueFormatter.Format("Path", path), true, true, true);
//        //    CreateFormattedField(doc, name, ValueFormatter.Format("Path", path), Field.Store.NO, Field.Index.NOT_ANALYZED_NO_NORMS, Field.TermVector.NO);
//        //}
//        //private static void CreateShortTextField(Document doc, string name, string value, StringBuilder fullText)
//        //{
//        //    CreateShortTextField(doc, name, value, fullText, false);
//        //}
//        //private static void CreateShortTextField(Document doc, string name, string value, StringBuilder fullText, bool stored)
//        //{
//        //    fullText.Append(value).Append(' ');
//        //    CreateFormattedField(doc, name, ValueFormatter.Format(SnSS.DataType.String, value), stored, true, false);
//        //}
//        //private static void CreateLongTextField(Document doc, string name, string value, StringBuilder fullText)
//        //{
//        //    fullText.Append(value).Append(' ');
//        //    CreateFormattedField(doc, name, ValueFormatter.Format(SnSS.DataType.String, value), false, true, false);
//        //}
//        //private static void CreateIntField(Document doc, string name, int value, StringBuilder fullText)
//        //{
//        //    CreateIntField(doc, name, value, fullText, false);
//        //}
//        //private static void CreateParentIdField(Document doc, string name, int parentId, StringBuilder fullText)
//        //{
//        //    if (parentId == 0)
//        //        CreateFormattedField(doc, name, ValueFormatter.Format(SnSS.DataType.String, String.Empty), false, true, false);
//        //    else
//        //        CreateIntField(doc, name, parentId, fullText, false);
//        //}
//        //private static void CreateParentIdField(Document doc, string name, SnS.Node parent, StringBuilder fullText)
//        //{
//        //    if (parent == null)
//        //        CreateFormattedField(doc, name, ValueFormatter.Format(SnSS.DataType.String, String.Empty), false, true, false);
//        //    else
//        //        CreateIntField(doc, name, parent.Id, fullText, false);
//        //}
//        //private static void CreateIntField(Document doc, string name, int value, StringBuilder fullText, bool stored)
//        //{
//        //    CreateFormattedField(doc, name, ValueFormatter.Format(SnSS.DataType.Int, value), stored, true, false);
//        //}
//        //private static void CreateNumberField(Document doc, string name, decimal value, StringBuilder fullText)
//        //{
//        //    CreateFormattedField(doc, name, ValueFormatter.Format(SnSS.DataType.Currency, value), false, true, false);
//        //}
//        //private static void CreateDateTimeField(Document doc, string name, DateTime value, StringBuilder fullText)
//        //{
//        //    CreateFormattedField(doc, name, ValueFormatter.Format(SnSS.DataType.DateTime, value), false, true, false);
//        //}
//        //private static void CreateVersionField(Document doc, string name, SnS.VersionNumber value, StringBuilder fullText)
//        //{
//        //    CreateFormattedField(doc, name, value.ToString(), true, true, false);
//        //}
//        //private static void CreateVersionStatusField(Document doc, string name, SnS.VersionNumber value, StringBuilder fullText)
//        //{
//        //    string status;
//        //    switch (value.Status)
//        //    {
//        //        case SenseNet.ContentRepository.Storage.VersionStatus.Approved: status = "a"; break;
//        //        case SenseNet.ContentRepository.Storage.VersionStatus.Locked: status = "l"; break;
//        //        case SenseNet.ContentRepository.Storage.VersionStatus.Draft: status = "d"; break;
//        //        case SenseNet.ContentRepository.Storage.VersionStatus.Rejected: status = "r"; break;
//        //        case SenseNet.ContentRepository.Storage.VersionStatus.Pending: status = "p"; break;
//        //        default:
//        //            throw new NotImplementedException("Unknown VersionStatus: " + value.Status);
//        //    }
//        //    CreateFormattedField(doc, name, status, true, true, false);
//        //}
//        //private static void CreateBinaryField(Document doc, string name, SnS.BinaryData value, StringBuilder fullText)
//        //{
//        //    var extract = TextExtractor.GetExtract(value);
//        //    fullText.Append(extract);
//        //}
//        //private static void CreateReferenceField(Document doc, string name, IEnumerable<SnS.Node> value, StringBuilder fullText)
//        //{
//        //    var nodeList = value as SnS.NodeList<SnS.Node>;
//        //    IEnumerable<int> idArray = null;
//        //    if (nodeList == null)
//        //        idArray = from n in value select n.Id;
//        //    else
//        //        idArray = nodeList.GetIdentifiers();

//        //    CreateFormattedField(doc, name, ValueFormatter.Format(SnSS.DataType.Reference, idArray), false, true, false);
//        //}
//        //private static void CreateDynamicField(Document doc, SnS.Node node, SnSS.PropertyType propType, StringBuilder fullText)
//        //{
//        //    switch (propType.DataType)
//        //    {
//        //        case SnSS.DataType.String:
//        //            CreateShortTextField(doc, propType.Name, node.GetProperty<string>(propType), fullText);
//        //            break;
//        //        case SnSS.DataType.Text:
//        //            CreateLongTextField(doc, propType.Name, node.GetProperty<string>(propType), fullText);
//        //            break;
//        //        case SnSS.DataType.DateTime:
//        //            CreateDateTimeField(doc, propType.Name, node.GetProperty<DateTime>(propType), fullText);
//        //            break;
//        //        case SnSS.DataType.Int:
//        //            CreateIntField(doc, propType.Name, node.GetProperty<int>(propType), fullText);
//        //            break;
//        //        case SnSS.DataType.Currency:
//        //            CreateNumberField(doc, propType.Name, node.GetProperty<decimal>(propType), fullText);
//        //            break;
//        //        case SnSS.DataType.Binary:
//        //            CreateBinaryField(doc, propType.Name, node.GetBinary(propType), fullText);
//        //            break;
//        //        case SnSS.DataType.Reference:
//        //            CreateReferenceField(doc, propType.Name, node.GetReferences(propType), fullText);
//        //            break;
//        //        default:
//        //            break;
//        //    }
//        //}

//        //private static void CreateFormattedField(Document doc, string name, string value, bool stored, bool analyzed, bool termVector)
//        //{
//        //    CreateFormattedField(doc, name, new string[] { value }, stored, analyzed, termVector);
//        //}
//        //private static void CreateFormattedField(Document doc, string name, string[] values, bool stored, bool analyzed, bool termVector)
//        //{
//        //    Field.Store store = stored ? Field.Store.YES : Field.Store.NO;
//        //    Field.Index index = analyzed ? Field.Index.ANALYZED : Field.Index.NOT_ANALYZED;
//        //    Field.TermVector vector = termVector ? Field.TermVector.YES : Field.TermVector.NO;
//        //    CreateFormattedField(doc, name, values, store, index, vector);
//        //}
//        //private static void CreateFormattedField(Document doc, string name, string[] values, Field.Store store, Field.Index index, Field.TermVector termVector)
//        //{
//        //    foreach (var value in values)
//        //        doc.Add(new Field(name, value ?? String.Empty, store, index, termVector));
//        //}
//    }
//}
