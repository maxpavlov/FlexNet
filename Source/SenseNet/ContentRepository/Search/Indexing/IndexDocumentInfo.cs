using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using SenseNet.ContentRepository.Storage;
using SnD = SenseNet.Diagnostics;
using SnField = SenseNet.ContentRepository.Field;
using SnS = SenseNet.ContentRepository.Storage;
using SnSS = SenseNet.ContentRepository.Storage.Schema;
using System.Diagnostics;
using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.Search.Indexing
{
    public class IndexDocumentProvider : SenseNet.ContentRepository.Storage.Search.IIndexDocumentProvider
    {
        public object GetIndexDocumentInfo(Node node)
        {
            return IndexDocumentInfo.Create(node);
        }
    }
    public enum FieldInfoType
    {
        StringField, IntField, LongField, SingleField, DoubleField
    }
    [Serializable]
    [DebuggerDisplay("{Name}:{Type}={Value} | Store:{Store} Index:{Index} TermVector:{TermVector}")]
    public class IndexFieldInfo
    {
        public string Name { get; private set; }
        public string Value { get; private set; }
        public FieldInfoType Type { get; private set; }
        public Field.Store Store { get; private set; }
        public Field.Index Index { get; private set; }
        public Field.TermVector TermVector { get; private set; }

        public IndexFieldInfo(string name, string value, FieldInfoType type, Field.Store store, Field.Index index, Field.TermVector termVector)
        {
            Name = name;
            Value = value;
            Type = type;
            Store = store;
            Index = index;
            TermVector = termVector;
        }
    }
    [Serializable]
    public class IndexDocumentInfo
    {
        [NonSerialized]
        private static PerFieldIndexingInfo __nameFieldIndexingInfo;
        [NonSerialized]
        private static PerFieldIndexingInfo __pathFieldIndexingInfo;
        [NonSerialized]
        private static PerFieldIndexingInfo __inTreeFieldIndexingInfo;
        [NonSerialized]
        private static PerFieldIndexingInfo __inFolderFieldIndexingInfo;

        internal static PerFieldIndexingInfo NameFieldIndexingInfo
        {
            get
            {
                if (__nameFieldIndexingInfo == null)
                {
                    var start = SenseNet.ContentRepository.Schema.ContentTypeManager.Current;
                    __nameFieldIndexingInfo = SenseNet.ContentRepository.Schema.ContentTypeManager.GetPerFieldIndexingInfo(LucObject.FieldName.Path);
                }
                return __nameFieldIndexingInfo;
            }
        }
        internal static PerFieldIndexingInfo PathFieldIndexingInfo
        {
            get
            {
                if (__pathFieldIndexingInfo == null)
                {
                    var start = SenseNet.ContentRepository.Schema.ContentTypeManager.Current;
                    __pathFieldIndexingInfo = SenseNet.ContentRepository.Schema.ContentTypeManager.GetPerFieldIndexingInfo(LucObject.FieldName.Path);
                }
                return __pathFieldIndexingInfo;
            }
        }
        internal static PerFieldIndexingInfo InTreeFieldIndexingInfo
        {
            get
            {
                if (__inTreeFieldIndexingInfo == null)
                {
                    var start = SenseNet.ContentRepository.Schema.ContentTypeManager.Current;
                    __inTreeFieldIndexingInfo = SenseNet.ContentRepository.Schema.ContentTypeManager.GetPerFieldIndexingInfo(LucObject.FieldName.InTree);
                }
                return __inTreeFieldIndexingInfo;
            }
        }
        internal static PerFieldIndexingInfo InFolderFieldIndexingInfo
        {
            get
            {
                if (__inFolderFieldIndexingInfo == null)
                {
                    var start = SenseNet.ContentRepository.Schema.ContentTypeManager.Current;
                    __inFolderFieldIndexingInfo = SenseNet.ContentRepository.Schema.ContentTypeManager.GetPerFieldIndexingInfo(LucObject.FieldName.InFolder);
                }
                return __inFolderFieldIndexingInfo;
            }
        }

        [NonSerialized]
        private static readonly IEnumerable<Document> EmptyDocuments = new Document[0];

        List<IndexFieldInfo> fields = new List<IndexFieldInfo>();
        public List<IndexFieldInfo> Fields
        {
            get { return fields; }
        }

        private bool _hasCustomField;
        public bool HasCustomField
        {
            get { return _hasCustomField; }
        }


        public void AddField(string name, string value, Field.Store store, Field.Index index)
        {
            AddField(name, value, store, index, Field.TermVector.NO);
        }
        public void AddField(string name, string value, Field.Store store, Field.Index index, Field.TermVector termVector)
        {
            fields.Add(new IndexFieldInfo(name, value, FieldInfoType.StringField, store, index, termVector));
        }
        public void AddField(string name, int value, Field.Store store, bool isIndexed)
        {
            fields.Add(new IndexFieldInfo(name, value.ToString(CultureInfo.InvariantCulture), FieldInfoType.IntField, store, isIndexed ? Field.Index.ANALYZED_NO_NORMS : Field.Index.NO, Field.TermVector.NO));
        }
        public void AddField(string name, long value, Field.Store store, bool isIndexed)
        {
            fields.Add(new IndexFieldInfo(name, value.ToString(CultureInfo.InvariantCulture), FieldInfoType.LongField, store, isIndexed ? Field.Index.ANALYZED_NO_NORMS : Field.Index.NO, Field.TermVector.NO));
        }
        public void AddField(string name, double value, Field.Store store, bool isIndexed)
        {
            fields.Add(new IndexFieldInfo(name, value.ToString(CultureInfo.InvariantCulture), FieldInfoType.DoubleField, store, isIndexed ? Field.Index.ANALYZED_NO_NORMS : Field.Index.NO, Field.TermVector.NO));
        }
        public void AddField(IndexFieldInfo fieldInfo)
        {
            fields.Add(fieldInfo);
        }

        [NonSerialized]
        private static List<string> PostponedFields = new List<string>(new string[] {
            LucObject.FieldName.Name, LucObject.FieldName.Path, LucObject.FieldName.InTree, LucObject.FieldName.InFolder, LucObject.FieldName.Depth, LucObject.FieldName.ParentId,
        });
        public static IndexDocumentInfo Create(Node node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            var textEtract = new StringBuilder();
            var doc = new IndexDocumentInfo();

            doc._hasCustomField = node is IHasCustomIndexField;

            var ixnode = node as IIndexableDocument;

            if (ixnode == null)
            {
                doc.AddField(LucObject.FieldName.NodeId, node.Id, Field.Store.YES, true);
                doc.AddField(LucObject.FieldName.VersionId, node.VersionId, Field.Store.YES, true);
                doc.AddField(LucObject.FieldName.Version, node.Version.ToString().ToLower(), Field.Store.YES, Field.Index.ANALYZED);
                doc.AddField(LucObject.FieldName.CreatedById, node.CreatedById, Field.Store.YES, true);
                doc.AddField(LucObject.FieldName.ModifiedById, node.ModifiedById, Field.Store.YES, true);
            }
            else
            {
                var fieldNames = new List<string>();
                foreach (var field in ixnode.GetIndexableFields())
                {
                    if (PostponedFields.Contains(field.Name))
                        continue;
                    string extract;
                    var lucFields = field.GetIndexFieldInfos(out extract);
                    textEtract.AppendLine(extract);
                    if (lucFields != null)
                    {
                        foreach (var lucField in lucFields)
                        {
                            fieldNames.Add(lucField.Name);
                            doc.AddField(lucField);
                        }
                    }
                }
            }

            //doc.AddField(LucObject.FieldName.NodeTimestamp, node.NodeTimestamp, Field.Store.YES, true);
            //doc.AddField(LucObject.FieldName.VersionTimestamp, node.VersionTimestamp, Field.Store.YES, true);
            doc.AddField(LucObject.FieldName.IsInherited, node.IsInherited ? BooleanIndexHandler.YES : BooleanIndexHandler.NO, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.AddField(LucObject.FieldName.IsMajor, node.Version.IsMajor ? BooleanIndexHandler.YES : BooleanIndexHandler.NO, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.AddField(LucObject.FieldName.IsPublic, node.Version.Status == VersionStatus.Approved ? BooleanIndexHandler.YES : BooleanIndexHandler.NO, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.AddField(LucObject.FieldName.AllText, textEtract.ToString(), Field.Store.NO, Field.Index.ANALYZED);

            return doc;
        }

        internal static Document GetDocument(int versionId)
        {
            var docData = StorageContext.Search.LoadIndexDocumentByVersionId(versionId);
            if (docData == null)
                return null;
            return GetDocument(docData);
        }
        internal static IEnumerable<Document> GetDocuments(IEnumerable<int> versionIdSet)
        {
            var vset = versionIdSet.ToArray();
            if (vset.Length == 0)
                return EmptyDocuments;
            var docData = StorageContext.Search.LoadIndexDocumentByVersionId(versionIdSet);
            var result = docData.Select(d => GetDocument(d)).ToArray();
            return result;
        }

        internal static Document GetDocument(IndexDocumentData docData)
        {
            var buffer = docData.IndexDocumentInfoBytes;
            //if (buffer.Length == 0)
            //    return null;

            var docStream = new System.IO.MemoryStream(buffer);
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            var info = (IndexDocumentInfo)formatter.Deserialize(docStream);

            return CreateDocument(info, docData);
        }
        public static Document CreateDocument(Node node) //caller: tests
        {
            var info = Create(node);
            var data = new IndexDocumentData
            {
                Path = node.Path,
                ParentId = node.ParentId,
                IsLastPublic = node.IsLastPublicVersion,
                IsLastDraft = node.IsLatestVersion
            };
            return CreateDocument(info, data);
        }
        internal static Document CreateDocument(IndexDocumentInfo info, IndexDocumentData docData)
        {
            var doc = new Document();
            foreach (var fieldInfo in info.fields)
                doc.Add(CreateField(fieldInfo));

            var path = docData.Path.ToLower();

            //doc.Add(new Field(LucObject.FieldName.Name, RepositoryPath.GetFileName(path), Field.Store.YES, Field.Index.ANALYZED));
            //doc.Add(new Field(LucObject.FieldName.Path, path, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));

            doc.Add(CreateStringField(LucObject.FieldName.Name, RepositoryPath.GetFileName(path), NameFieldIndexingInfo));
            doc.Add(CreateStringField(LucObject.FieldName.Path, path, NameFieldIndexingInfo));

            //LucObject.FieldName.Depth
            var nf = new NumericField(LucObject.FieldName.Depth, Field.Store.YES, true);
            nf.SetIntValue(DepthIndexHandler.GetDepth(docData.Path));
            doc.Add(nf);

            //LucObject.FieldName.InTree
            //var fields = InTreeIndexHandlerInstance.GetIndexFields(LucObject.FieldName.InTree, docData.Path);
            var fields = CreateInTreeFields(LucObject.FieldName.InTree, docData.Path);
            foreach (var field in fields)
                doc.Add(field);

            //LucObject.FieldName.InFolder
            doc.Add(CreateInFolderField(LucObject.FieldName.InFolder, path));

            //LucObject.FieldName.ParentId
            nf = new NumericField(LucObject.FieldName.ParentId, Field.Store.NO, true);
            nf.SetIntValue(docData.ParentId);
            doc.Add(nf);

            // flags
            doc.Add(new Field(LucObject.FieldName.IsLastPublic, docData.IsLastPublic ? BooleanIndexHandler.YES : BooleanIndexHandler.NO, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field(LucObject.FieldName.IsLastDraft, docData.IsLastDraft ? BooleanIndexHandler.YES : BooleanIndexHandler.NO, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));

            // timestamps
            nf = new NumericField(LucObject.FieldName.NodeTimestamp, Field.Store.YES, true);
            nf.SetLongValue(docData.NodeTimestamp);
            doc.Add(nf);
            nf = new NumericField(LucObject.FieldName.VersionTimestamp, Field.Store.YES, true);
            nf.SetLongValue(docData.VersionTimestamp);
            doc.Add(nf);

            // custom fields
            if (info.HasCustomField)
            {
                var customFields = CustomIndexFieldManager.GetFields(info, docData);
                if (customFields != null)
                    foreach (var field in customFields)
                        doc.Add(field);
            }

            return doc;
        }
        private static AbstractField CreateField(IndexFieldInfo fieldInfo)
        {
            NumericField nf;
            switch (fieldInfo.Type)
            {
                case FieldInfoType.StringField:
                    return new Field(fieldInfo.Name, fieldInfo.Value, fieldInfo.Store, fieldInfo.Index, fieldInfo.TermVector);
                case FieldInfoType.IntField:
                    nf = new NumericField(fieldInfo.Name, fieldInfo.Store, fieldInfo.Index != Field.Index.NO);
                    nf.SetIntValue(Int32.Parse(fieldInfo.Value, CultureInfo.InvariantCulture));
                    return nf;
                case FieldInfoType.LongField:
                    nf = new NumericField(fieldInfo.Name, 8, fieldInfo.Store, fieldInfo.Index != Field.Index.NO);
                    nf.SetLongValue(Int64.Parse(fieldInfo.Value, CultureInfo.InvariantCulture));
                    return nf;
                case FieldInfoType.SingleField:
                    nf = new NumericField(fieldInfo.Name, fieldInfo.Store, fieldInfo.Index != Field.Index.NO);
                    nf.SetFloatValue(Single.Parse(fieldInfo.Value, CultureInfo.InvariantCulture));
                    return nf;
                case FieldInfoType.DoubleField:
                    nf = new NumericField(fieldInfo.Name, 8, fieldInfo.Store, fieldInfo.Index != Field.Index.NO);
                    nf.SetDoubleValue(Double.Parse(fieldInfo.Value, CultureInfo.InvariantCulture));
                    return nf;
                default:
                    throw new NotImplementedException("IndexFieldInfo." + fieldInfo.Type);
            }
        }

        private static AbstractField CreateInFolderField(string fieldName, string path)
        {
            var parentPath = RepositoryPath.GetParentPath(path) ?? "/";
            return CreateStringField(fieldName, parentPath, InFolderFieldIndexingInfo);
        }
        private static IEnumerable<AbstractField> CreateInTreeFields(string fieldName, string path)
        {
            var separator = "/";
            string[] fragments = path.ToLower().Split(separator.ToCharArray(), StringSplitOptions.None);
            string[] pathSteps = new string[fragments.Length];
            for (int i = 0; i < fragments.Length; i++)
                pathSteps[i] = string.Join(separator, fragments, 0, i + 1);
            return pathSteps.Select(p => CreateStringField(fieldName, p, InTreeFieldIndexingInfo)).ToArray();
        }
        private static AbstractField CreateStringField(string name, string value, PerFieldIndexingInfo indexingInfo)
        {
            var index = indexingInfo.IndexingMode ?? PerFieldIndexingInfo.DefaultIndexingMode;
            var store = indexingInfo.IndexStoringMode ?? PerFieldIndexingInfo.DefaultIndexStoringMode;
            var termVector = indexingInfo.TermVectorStoringMode ?? PerFieldIndexingInfo.DefaultTermVectorStoringMode;
            return new Lucene.Net.Documents.Field(name, value, store, index, termVector);
        }
    }



    public interface IHasCustomIndexField { }
    public interface ICustomIndexFieldProvider
    {
        IEnumerable<Fieldable> GetFields(SenseNet.ContentRepository.Storage.Data.IndexDocumentData docData);
    }
    internal class CustomIndexFieldManager
    {
        internal static IEnumerable<Fieldable> GetFields(IndexDocumentInfo info, SenseNet.ContentRepository.Storage.Data.IndexDocumentData docData)
        {
            Debug.WriteLine("%> adding custom fields for " + docData.Path);
            return Instance.GetFieldsPrivate(info, docData);
        }

        //-------------------------------------------------------------

        private static object _instanceSync = new object();
        private static CustomIndexFieldManager __instance;
        private static CustomIndexFieldManager Instance
        {
            get
            {
                if (__instance == null)
                {
                    lock (_instanceSync)
                    {
                        if (__instance == null)
                        {
                            var instance = new CustomIndexFieldManager();
                            instance._providers = TypeHandler.GetTypesByInterface(typeof(ICustomIndexFieldProvider))
                                .Select(t => (ICustomIndexFieldProvider)Activator.CreateInstance(t)).ToArray();
                            __instance = instance;
                        }
                    }
                }
                return __instance;
            }
        }

        //---------------------------------------------------------------------
        
        private IEnumerable<ICustomIndexFieldProvider> _providers;

        private CustomIndexFieldManager() { }

        private IEnumerable<Fieldable> GetFieldsPrivate(IndexDocumentInfo info, IndexDocumentData docData)
        {
            var fields = new List<Fieldable>();
            foreach (var provider in _providers)
            {
                var f = provider.GetFields(docData);
                if (f != null)
                    fields.AddRange(f);
            }
            return fields.Count == 0 ? null : fields;
        }

    }
}
