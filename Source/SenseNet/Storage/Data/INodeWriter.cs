using System;
using System.IO;
using SenseNet.ContentRepository.Storage.Schema;
using System.Collections.Generic;

namespace SenseNet.ContentRepository.Storage.Data
{
    public interface INodeWriter
    {
        void Open();
        void Close();

        //============================================================================ Node Insert/Update

        void UpdateSubTreePath(string oldPath, string newPath);
        int InsertNodeRow(NodeData nodeData);
        void UpdateNodeRow(NodeData nodeData);

        //============================================================================ Version Insert/Update

        int InsertVersionRow(NodeData nodeData);
        void UpdateVersionRow(NodeData nodeData);
        void CopyAndUpdateVersion(NodeData nodeData, int previousVersionId);
        void CopyAndUpdateVersion(NodeData nodeData, int previousVersionId, int destinationVersionId);

        //============================================================================ Property Insert/Update

        void SaveStringProperty(int versionId, PropertyType propertyType, string value);
        void SaveDateTimeProperty(int versionId, PropertyType propertyType, DateTime value);
        void SaveIntProperty(int versionId, PropertyType propertyType, int value);
        void SaveCurrencyProperty(int versionId, PropertyType propertyType, decimal value);
        void SaveTextProperty(int versionId, PropertyType propertyType, bool isLoaded, string value);
        void SaveReferenceProperty(int versionId, PropertyType propertyType, IEnumerable<int> value);
        int InsertBinaryProperty(int versionId, int propertyTypeId, BinaryDataValue value);
        void UpdateBinaryProperty(int binaryDataId, BinaryDataValue value);
        void DeleteBinaryProperty(int versionId, PropertyType propertyType);

    }
}