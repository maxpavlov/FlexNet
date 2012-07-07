using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;

namespace SenseNet.ContentRepository.Tests
{
    internal class LoggedDataProvider : SenseNet.ContentRepository.Storage.Data.SqlClient.SqlProvider, IDisposable
    {
        DataProvider _wrapped;
        FieldInfo _dataProviderCurrentField;

        private StringBuilder _log = new StringBuilder();
        public string _GetLog()
        {
            return _log.ToString();
        }
        public string _GetLogAndClear()
        {
            var s = _log.ToString();
            _log.Clear();
            return s;
        }

        public LoggedDataProvider()
        {
            _wrapped = (SenseNet.ContentRepository.Storage.Data.SqlClient.SqlProvider)DataProvider.Current;
            _dataProviderCurrentField = typeof(DataProvider).GetField("_current", BindingFlags.Static | BindingFlags.NonPublic);
            _dataProviderCurrentField.SetValue(null, this);
        }
        public void Dispose()
        {
            _dataProviderCurrentField.SetValue(null, _wrapped);
        }

        private void WriteLog(MethodBase methodBase, params object[] prms)
        {
            _log.Append(methodBase.Name).Append("(");
            ParameterInfo[] prmInfos = methodBase.GetParameters();
            for (int i = 0; i < prmInfos.Length; i++)
            {
                if (i > 0)
                    _log.Append(", ");

                _log.Append(prmInfos[i].Name).Append("=<");
                _log.Append(prms[i]).Append(">");
            }
            _log.Append(");").Append("\r\n");
        }

        protected override bool NodeExistsInDatabase(string path)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), path);
            return base.NodeExistsInDatabase(path);
        }
        public override string GetNameOfLastNodeWithNameBase(int parentId, string namebase, string extension)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), parentId, namebase, extension);
            return base.GetNameOfLastNodeWithNameBase(parentId, namebase, extension);
        }
        protected internal override System.Data.DataSet LoadSchema()
        {
            WriteLog(MethodInfo.GetCurrentMethod());
            return base.LoadSchema();
        }
        protected internal override void Reset()
        {
            WriteLog(MethodInfo.GetCurrentMethod());
            base.Reset();
        }
        protected internal override SchemaWriter CreateSchemaWriter()
        {
            WriteLog(MethodInfo.GetCurrentMethod());
            return base.CreateSchemaWriter();
        }
        public override Dictionary<DataType, int> ContentListMappingOffsets
        {
            get
            {
                WriteLog(MethodInfo.GetCurrentMethod());
                return base.ContentListMappingOffsets;
            }
        }
        protected internal override int ContentListStartPage
        {
            get
            {
                WriteLog(MethodInfo.GetCurrentMethod());
                return base.ContentListStartPage;
            }
        }
        protected override PropertyMapping GetPropertyMappingInternal(PropertyType propType)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), propType);
            return base.GetPropertyMappingInternal(propType);
        }
        public override void AssertSchemaTimestampAndWriteModificationDate(long timestamp)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), timestamp);
            base.AssertSchemaTimestampAndWriteModificationDate(timestamp);
        }
        public override int PathMaxLength
        {
            get
            {
                WriteLog(MethodInfo.GetCurrentMethod());
                return base.PathMaxLength;
            }
        }
        public override DateTime DateTimeMinValue
        {
            get
            {
                WriteLog(MethodInfo.GetCurrentMethod());
                return base.DateTimeMinValue;
            }
        }
        public override DateTime DateTimeMaxValue
        {
            get
            {
                WriteLog(MethodInfo.GetCurrentMethod());
                return base.DateTimeMaxValue;
            }
        }
        public override decimal DecimalMinValue
        {
            get
            {
                WriteLog(MethodInfo.GetCurrentMethod());
                return base.DecimalMinValue;
            }
        }
        public override decimal DecimalMaxValue
        {
            get
            {
                WriteLog(MethodInfo.GetCurrentMethod());
                return base.DecimalMaxValue;
            }
        }
        protected internal override ITransactionProvider CreateTransaction()
        {
            WriteLog(MethodInfo.GetCurrentMethod());
            return base.CreateTransaction();
        }
        protected internal override INodeWriter CreateNodeWriter()
        {
            WriteLog(MethodInfo.GetCurrentMethod());
            return new LoggedNodeWriter(_log);
        }
        protected internal override VersionNumber[] GetVersionNumbers(int nodeId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), nodeId);
            return base.GetVersionNumbers(nodeId);
        }
        protected internal override VersionNumber[] GetVersionNumbers(string path)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), path);
            return base.GetVersionNumbers(path);
        }
        protected internal override void LoadNodes(Dictionary<int, NodeBuilder> buildersByVersionId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), buildersByVersionId);
            base.LoadNodes(buildersByVersionId);
        }
        protected internal override bool IsCacheableText(string text)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), text);
            return base.IsCacheableText(text);
        }
        protected internal override string LoadTextPropertyValue(int versionId, int propertyTypeId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), versionId, propertyTypeId);
            return base.LoadTextPropertyValue(versionId, propertyTypeId);
        }
        protected internal override System.IO.Stream LoadBinaryPropertyValue(int versionId, int propertyTypeId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), versionId, propertyTypeId);
            return base.LoadBinaryPropertyValue(versionId, propertyTypeId);
        }
        protected internal override BinaryCacheEntity LoadBinaryCacheEntity(int nodeVersionId, int propertyTypeId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), nodeVersionId, propertyTypeId);
            return base.LoadBinaryCacheEntity(nodeVersionId, propertyTypeId);
        }
        protected internal override byte[] LoadBinaryFragment(int binaryPropertyId, long position, int count)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), binaryPropertyId, position, count);
            return base.LoadBinaryFragment(binaryPropertyId, position, count);
        }
        protected internal override IEnumerable<NodeType> LoadChildTypesToAllow(int sourceNodeId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), sourceNodeId);
            return base.LoadChildTypesToAllow(sourceNodeId);
        }
        protected internal override DataOperationResult MoveNodeTree(int sourceNodeId, int targetNodeId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), sourceNodeId, targetNodeId);
            return base.MoveNodeTree(sourceNodeId, targetNodeId);
        }
        protected internal override DataOperationResult DeleteNodeTree(int nodeId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), nodeId);
            return base.DeleteNodeTree(nodeId);
        }
        protected internal override DataOperationResult DeleteNodeTreePsychical(int nodeId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), nodeId);
            return base.DeleteNodeTreePsychical(nodeId);
        }
        protected internal override bool HasChild(int nodeId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), nodeId);
            return base.HasChild(nodeId);
        }
        protected internal override void DeleteVersion(int versionId, NodeData nodeData, out int lastMajorVersionId, out int lastMinorVersionId)
        {
            base.DeleteVersion(versionId, nodeData, out lastMajorVersionId, out lastMinorVersionId);
            WriteLog(MethodInfo.GetCurrentMethod(), versionId, nodeData, lastMajorVersionId, lastMinorVersionId);
        }
        protected internal override Dictionary<int, List<int>> LoadMemberships()
        {
            WriteLog(MethodInfo.GetCurrentMethod());
            return base.LoadMemberships();
        }
        protected internal override void SetPermission(int principalId, int nodeId, PermissionType permissionType, bool isInheritable, PermissionValue permissionValue)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), principalId, nodeId, permissionType, isInheritable, permissionValue);
            base.SetPermission(principalId, nodeId, permissionType, isInheritable, permissionValue);
        }
        protected internal override void SetPermission(SecurityEntry entry)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), entry);
            base.SetPermission(entry);
        }
        protected internal override void ExplicateGroupMemberships()
        {
            WriteLog(MethodInfo.GetCurrentMethod());
            base.ExplicateGroupMemberships();
        }
        protected internal override void ExplicateOrganizationUnitMemberships(IUser user)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), user);
            base.ExplicateOrganizationUnitMemberships(user);
        }
        protected internal override void BreakInheritance(int nodeId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), nodeId);
            base.BreakInheritance(nodeId);
        }
        protected internal override void RemoveBreakInheritance(int nodeId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), nodeId);
            base.RemoveBreakInheritance(nodeId);
        }
        protected internal override List<int> LoadGroupMembership(int groupId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), groupId);
            return base.LoadGroupMembership(groupId);
        }
        internal override int LoadLastModifiersGroupId()
        {
            WriteLog(MethodInfo.GetCurrentMethod());
            return base.LoadLastModifiersGroupId();
        }
        protected internal override void PersistUploadToken(ContentRepository.Storage.ApplicationMessaging.UploadToken value)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), value);
            base.PersistUploadToken(value);
        }
        protected internal override int GetUserIdByUploadGuid(Guid uploadGuid)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), uploadGuid);
            return base.GetUserIdByUploadGuid(uploadGuid);
        }
        protected override string GetAppModelScriptPrivate(IEnumerable<string> paths, bool all, bool resolveChildren)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), paths, all, resolveChildren);
            return base.GetAppModelScriptPrivate(paths, all, resolveChildren);
        }
        protected internal override IDataProcedure CreateDataProcedureInternal(string commandText)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), commandText);
            return base.CreateDataProcedureInternal(commandText);
        }
        protected override System.Data.IDbDataParameter CreateParameterInternal()
        {
            WriteLog(MethodInfo.GetCurrentMethod());
            return base.CreateParameterInternal();
        }
        protected internal override void CheckScriptInternal(string commandText)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), commandText);
            base.CheckScriptInternal(commandText);
        }
        protected internal override NodeHead LoadNodeHead(string path)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), path);
            return base.LoadNodeHead(path);
        }
        protected internal override NodeHead LoadNodeHead(int nodeId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), nodeId);
            return base.LoadNodeHead(nodeId);
        }
        protected internal override NodeHead LoadNodeHeadByVersionId(int versionId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), versionId);
            return base.LoadNodeHeadByVersionId(versionId);
        }
        protected internal override IEnumerable<NodeHead> LoadNodeHeads(IEnumerable<int> heads)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), heads);
            return base.LoadNodeHeads(heads);
        }
        protected internal override NodeHead.NodeVersion[] GetNodeVersions(int nodeId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), nodeId);
            return base.GetNodeVersions(nodeId);
        }
        protected internal override long GetTreeSize(string path, bool includeChildren)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), path, includeChildren);
            return base.GetTreeSize(path, includeChildren);
        }
        protected override int NodeCount(string path)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), path);
            return base.NodeCount(path);
        }
        protected override int VersionCount(string path)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), path);
            return base.VersionCount(path);
        }
        protected internal override void UpdateIndexDocument(NodeData nodeData, byte[] indexDocumentBytes)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), nodeData, indexDocumentBytes);
            base.UpdateIndexDocument(nodeData, indexDocumentBytes);
        }
        protected internal override IndexDocumentData LoadIndexDocumentByVersionId(int versionId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), versionId);
            return base.LoadIndexDocumentByVersionId(versionId);
        }
        protected internal override IEnumerable<IndexDocumentData> LoadIndexDocumentByVersionId(IEnumerable<int> versionId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), versionId);
            return base.LoadIndexDocumentByVersionId(versionId);
        }
        protected internal override IDataProcedure CreateLoadIndexDocumentCollectionByPathProcedure(string path)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), path);
            return base.CreateLoadIndexDocumentCollectionByPathProcedure(path);
        }
        protected internal override IndexDocumentData GetIndexDocumentDataFromReader(System.Data.Common.DbDataReader reader)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), reader);
            return base.GetIndexDocumentDataFromReader(reader);
        }
        protected internal override IEnumerable<int> GetIdsOfNodesThatDoNotHaveIndexDocument()
        {
            WriteLog(MethodInfo.GetCurrentMethod());
            return base.GetIdsOfNodesThatDoNotHaveIndexDocument();
        }
        protected internal override IndexBackup LoadLastBackup()
        {
            WriteLog(MethodInfo.GetCurrentMethod());
            return base.LoadLastBackup();
        }
        protected internal override IndexBackup CreateBackup(int backupNumber)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), backupNumber);
            return base.CreateBackup(backupNumber);
        }
        protected internal override void StoreBackupStream(string backupFilePath, IndexBackup backup, IndexBackupProgress progress)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), backupFilePath, backup, progress);
            base.StoreBackupStream(backupFilePath, backup, progress);
        }
        protected internal override void SetActiveBackup(IndexBackup backup, IndexBackup lastBackup)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), backup, lastBackup);
            base.SetActiveBackup(backup, lastBackup);
        }
        protected override void KeepOnlyLastIndexBackup()
        {
            WriteLog(MethodInfo.GetCurrentMethod());
            base.KeepOnlyLastIndexBackup();
        }
        protected override Guid GetLastIndexBackupNumber()
        {
            WriteLog(MethodInfo.GetCurrentMethod());
            return base.GetLastIndexBackupNumber();
        }
        protected override IndexBackup RecoverIndexBackup(string backupFilePath)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), backupFilePath);
            return base.RecoverIndexBackup(backupFilePath);
        }
        public override int GetLastActivityId()
        {
            WriteLog(MethodInfo.GetCurrentMethod());
            return base.GetLastActivityId();
        }
        public override IDataProcedure GetTimestampDataForOneNodeIntegrityCheck(string path)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), path);
            return base.GetTimestampDataForOneNodeIntegrityCheck(path);
        }
        public override IDataProcedure GetTimestampDataForRecursiveIntegrityCheck(string path)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), path);
            return base.GetTimestampDataForRecursiveIntegrityCheck(path);
        }
        public override string DatabaseName
        {
            get
            {
                WriteLog(MethodInfo.GetCurrentMethod());
                return base.DatabaseName;
            }
        }
        public override IEnumerable<string> GetScriptsForDatabaseBackup()
        {
            WriteLog(MethodInfo.GetCurrentMethod());
            return base.GetScriptsForDatabaseBackup();
        }
        protected internal override List<ContentListType> GetContentListTypesInTree(string path)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), path);
            return base.GetContentListTypesInTree(path);
        }
        protected internal override IEnumerable<int> GetChildrenIdentfiers(int nodeId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), nodeId);
            return base.GetChildrenIdentfiers(nodeId);
        }
        protected internal override int InstanceCount(int[] nodeTypeIds)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), nodeTypeIds);
            return base.InstanceCount(nodeTypeIds);
        }
        protected internal override IEnumerable<int> QueryNodesByPath(string pathStart, bool orderByPath)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), pathStart, orderByPath);
            return base.QueryNodesByPath(pathStart, orderByPath);
        }
        protected internal override IEnumerable<int> QueryNodesByType(int[] typeIds)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), typeIds);
            return base.QueryNodesByType(typeIds);
        }
        protected internal override IEnumerable<int> QueryNodesByTypeAndPath(int[] nodeTypeIds, string pathStart, bool orderByPath)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), nodeTypeIds, pathStart, orderByPath);
            return base.QueryNodesByTypeAndPath(nodeTypeIds, pathStart, orderByPath);
        }
        protected internal override IEnumerable<int> QueryNodesByTypeAndPathAndName(int[] nodeTypeIds, string pathStart, bool orderByPath, string name)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), nodeTypeIds, pathStart, orderByPath, name);
            return base.QueryNodesByTypeAndPathAndName(nodeTypeIds, pathStart, orderByPath, name);
        }
        protected internal override IEnumerable<int> QueryNodesByTypeAndPathAndProperty(int[] nodeTypeIds, string pathStart, bool orderByPath, List<QueryPropertyData> properties)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), nodeTypeIds, pathStart, orderByPath, properties);
            return base.QueryNodesByTypeAndPathAndProperty(nodeTypeIds, pathStart, orderByPath, properties);
        }
        protected internal override IEnumerable<int> QueryNodesByReferenceAndType(string referenceName, int referredNodeId, int[] allowedTypeIds)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), referenceName, referredNodeId, allowedTypeIds);
            return base.QueryNodesByReferenceAndType(referenceName, referredNodeId, allowedTypeIds);
        }
        protected internal override int InitializeStagingBinaryData(int versionId, int propertyTypeId, string fileName, long fileSize)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), versionId, propertyTypeId, fileName, fileSize);
            return base.InitializeStagingBinaryData(versionId, propertyTypeId, fileName, fileSize);
        }
        protected internal override void SaveChunk(int stagingBinaryDataId, byte[] bytes, int offset)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), stagingBinaryDataId, bytes, offset);
            base.SaveChunk(stagingBinaryDataId, bytes, offset);
        }
        protected internal override void CopyStagingToBinaryData(int versionId, int propertyTypeId, int stagingBinaryDataId, string checksum)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), versionId, propertyTypeId, stagingBinaryDataId, checksum);
            base.CopyStagingToBinaryData(versionId, propertyTypeId, stagingBinaryDataId, checksum);
        }
        protected internal override void DeleteStagingBinaryData(int stagingBinaryDataId)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), stagingBinaryDataId);
            base.DeleteStagingBinaryData(stagingBinaryDataId);
        }
    }
}
