using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.ContentRepository.Tests
{
    internal class LoggedNodeWriter : INodeWriter
    {
        private StringBuilder _log = new StringBuilder();
        SenseNet.ContentRepository.Storage.Data.SqlClient.SqlNodeWriter _writer = new ContentRepository.Storage.Data.SqlClient.SqlNodeWriter();

        public LoggedNodeWriter(StringBuilder log)
        {
            _log = log;
        }

        private void WriteLog(MethodBase methodBase, params object[] prms)
        {
            _log.Append("NodeWriter: ");
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

        public void Open()
        {
            WriteLog(MethodInfo.GetCurrentMethod());
            _writer.Open();
        }
        public void Close()
        {
            WriteLog(MethodInfo.GetCurrentMethod());
            _writer.Close();
        }
        public void InsertNodeAndVersionRows(NodeData nodeData, out int lastMajorVersionId, out int lastMinorVersionId)
        {
            _writer.InsertNodeAndVersionRows(nodeData, out lastMajorVersionId, out lastMinorVersionId);
            WriteLog(MethodInfo.GetCurrentMethod(), nodeData, lastMajorVersionId, lastMinorVersionId);
        }
        public void UpdateSubTreePath(string oldPath, string newPath)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), oldPath, newPath);
            _writer.UpdateSubTreePath(oldPath, newPath);
        }
        public void UpdateNodeRow(NodeData nodeData)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), nodeData);
            _writer.UpdateNodeRow(nodeData);
        }
        public void UpdateVersionRow(NodeData nodeData, out int lastMajorVersionId, out int lastMinorVersionId)
        {
            _writer.UpdateVersionRow(nodeData, out lastMajorVersionId, out lastMinorVersionId);
            WriteLog(MethodInfo.GetCurrentMethod(), nodeData, lastMajorVersionId, lastMinorVersionId);
        }
        public void CopyAndUpdateVersion(NodeData nodeData, int previousVersionId, out int lastMajorVersionId, out int lastMinorVersionId)
        {
            _writer.CopyAndUpdateVersion(nodeData, previousVersionId, out lastMajorVersionId, out lastMinorVersionId);
            WriteLog(MethodInfo.GetCurrentMethod(), nodeData, previousVersionId, lastMajorVersionId, lastMinorVersionId);
        }
        public void CopyAndUpdateVersion(NodeData nodeData, int previousVersionId, int destinationVersionId, out int lastMajorVersionId, out int lastMinorVersionId)
        {
            _writer.CopyAndUpdateVersion(nodeData, previousVersionId, destinationVersionId, out lastMajorVersionId, out lastMinorVersionId);
            WriteLog(MethodInfo.GetCurrentMethod(), nodeData, previousVersionId, destinationVersionId, lastMajorVersionId, lastMinorVersionId);
        }
        public void SaveStringProperty(int versionId, PropertyType propertyType, string value)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), versionId, propertyType, value);
            _writer.SaveStringProperty(versionId, propertyType, value);
        }
        public void SaveDateTimeProperty(int versionId, PropertyType propertyType, DateTime value)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), versionId, propertyType, value);
            _writer.SaveDateTimeProperty(versionId, propertyType, value);
        }
        public void SaveIntProperty(int versionId, PropertyType propertyType, int value)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), versionId, propertyType, value);
            _writer.SaveIntProperty(versionId, propertyType, value);
        }
        public void SaveCurrencyProperty(int versionId, PropertyType propertyType, decimal value)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), versionId, propertyType, value);
            _writer.SaveCurrencyProperty(versionId, propertyType, value);
        }
        public void SaveTextProperty(int versionId, PropertyType propertyType, bool isLoaded, string value)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), versionId, propertyType, isLoaded, value);
            _writer.SaveTextProperty(versionId, propertyType, isLoaded, value);
        }
        public void SaveReferenceProperty(int versionId, PropertyType propertyType, IEnumerable<int> value)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), versionId, propertyType, value);
            _writer.SaveReferenceProperty(versionId, propertyType, value);
        }
        public int InsertBinaryProperty(int versionId, int propertyTypeId, BinaryDataValue value, bool isNewNode)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), versionId, propertyTypeId, value, isNewNode);
            return _writer.InsertBinaryProperty(versionId, propertyTypeId, value, isNewNode);
        }
        public void UpdateBinaryProperty(int binaryDataId, BinaryDataValue value)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), binaryDataId, value);
            _writer.UpdateBinaryProperty(binaryDataId, value);
        }
        public void DeleteBinaryProperty(int versionId, PropertyType propertyType)
        {
            WriteLog(MethodInfo.GetCurrentMethod(), versionId, propertyType);
            _writer.DeleteBinaryProperty(versionId, propertyType);
        }
    }
}
