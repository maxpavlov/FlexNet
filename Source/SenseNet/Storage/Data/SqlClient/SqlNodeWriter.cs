using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using SenseNet.ContentRepository.Storage.Schema;
using System.Globalization;
using System.Diagnostics;

namespace SenseNet.ContentRepository.Storage.Data.SqlClient
{
    public class SqlNodeWriter : INodeWriter
    {
        FlatPropertyWriter _flatWriter;

        public void Open()
        {
            //
        }
        public void Close()
        {
            if (_flatWriter != null)
                _flatWriter.Execute();
        }

        //============================================================================ Node Insert/Update

        public void UpdateSubTreePath(string oldPath, string newPath)
        {
            if (oldPath == null)
                throw new ArgumentNullException("oldPath");
            if (newPath == null)
                throw new ArgumentNullException("newPath");

            if (oldPath.Length == 0)
                throw new ArgumentException("Old path cannot be empty.", "oldPath");
            if (newPath.Length == 0)
                throw new ArgumentException("New path cannot be empty.", "newPath");

            SqlProcedure cmd = null;
            try
            {
                cmd = new SqlProcedure { CommandText = "proc_Node_UpdateSubTreePath" };
                cmd.Parameters.Add("@OldPath", SqlDbType.NVarChar, 450).Value = oldPath;
                cmd.Parameters.Add("@NewPath", SqlDbType.NVarChar, 450).Value = newPath;
                cmd.ExecuteNonQuery();
            }
            finally
            {
                cmd.Dispose();
            }
        }
        public int InsertNodeRow(NodeData nodeData)
        {
            SqlProcedure cmd = null;
            SqlDataReader reader = null;
            var result = 0;
            try
            {
                cmd = new SqlProcedure { CommandText = "proc_Node_Insert" };
                cmd.Parameters.Add("@NodeTypeId", SqlDbType.Int).Value = nodeData.NodeTypeId;
                cmd.Parameters.Add("@ContentListTypeId", SqlDbType.Int).Value = (nodeData.ContentListTypeId != 0) ? (object)nodeData.ContentListTypeId : DBNull.Value;
                cmd.Parameters.Add("@ContentListId", SqlDbType.Int).Value = (nodeData.ContentListId != 0) ? (object)nodeData.ContentListId : DBNull.Value;
                cmd.Parameters.Add("@IsDeleted", SqlDbType.TinyInt).Value = nodeData.IsDeleted ? 1 : 0;
                cmd.Parameters.Add("@IsInherited", SqlDbType.TinyInt).Value = nodeData.IsInherited ? 1 : 0;
                cmd.Parameters.Add("@ParentNodeId", SqlDbType.Int).Value = (nodeData.ParentId > 0) ? (object)nodeData.ParentId : DBNull.Value;
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 450).Value = nodeData.Name;
                cmd.Parameters.Add("@DisplayName", SqlDbType.NVarChar, 450).Value = (object)nodeData.DisplayName ?? DBNull.Value;
                cmd.Parameters.Add("@Path", SqlDbType.NVarChar, 450).Value = nodeData.Path;
                cmd.Parameters.Add("@Index", SqlDbType.Int).Value = nodeData.Index;
                cmd.Parameters.Add("@Locked", SqlDbType.TinyInt).Value = nodeData.Locked ? 1 : 0;
                cmd.Parameters.Add("@LockedById", SqlDbType.Int).Value = (nodeData.LockedById > 0) ? (object)nodeData.LockedById : DBNull.Value;
                cmd.Parameters.Add("@ETag", SqlDbType.VarChar, 50).Value = nodeData.ETag ?? String.Empty;
                cmd.Parameters.Add("@LockType", SqlDbType.Int).Value = nodeData.LockType;
                cmd.Parameters.Add("@LockTimeout", SqlDbType.Int).Value = nodeData.LockTimeout;
                cmd.Parameters.Add("@LockDate", SqlDbType.DateTime).Value = nodeData.LockDate;
                cmd.Parameters.Add("@LockToken", SqlDbType.VarChar, 50).Value = nodeData.LockToken ?? String.Empty;
                cmd.Parameters.Add("@LastLockUpdate", SqlDbType.DateTime).Value = nodeData.LastLockUpdate;
                cmd.Parameters.Add("@CreationDate", SqlDbType.DateTime).Value = nodeData.NodeCreationDate;
                cmd.Parameters.Add("@CreatedById", SqlDbType.Int).Value = nodeData.NodeCreatedById;
                cmd.Parameters.Add("@ModificationDate", SqlDbType.DateTime).Value = nodeData.NodeModificationDate;
                cmd.Parameters.Add("@ModifiedById", SqlDbType.Int).Value = nodeData.NodeModifiedById;

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    // SELECT [NodeId], [Timestamp] FROM Nodes WHERE NodeId = @@IDENTITY
                    result = Convert.ToInt32(reader[0]);
                    nodeData.NodeTimestamp = SqlProvider.GetLongFromBytes((byte[])reader[1]);
                }
            }
            catch (SqlException e) //rethrow
            {
                throw new DataException(e.Message, e);
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
                cmd.Dispose();
            }
            return result;
        }
        public void UpdateNodeRow(NodeData nodeData)
        {
            SqlProcedure cmd = null;
            SqlDataReader reader = null;
            try
            {
                cmd = new SqlProcedure { CommandText = "proc_Node_Update" };
                cmd.Parameters.Add("@NodeId", SqlDbType.Int).Value = nodeData.Id;
                cmd.Parameters.Add("@NodeTypeId", SqlDbType.Int).Value = nodeData.NodeTypeId;
                cmd.Parameters.Add("@ContentListTypeId", SqlDbType.Int).Value = (nodeData.ContentListTypeId != 0) ? (object)nodeData.ContentListTypeId : DBNull.Value;
                cmd.Parameters.Add("@ContentListId", SqlDbType.Int).Value = (nodeData.ContentListId != 0) ? (object)nodeData.ContentListId : DBNull.Value;
                cmd.Parameters.Add("@IsDeleted", SqlDbType.TinyInt).Value = nodeData.IsDeleted ? 1 : 0;
                cmd.Parameters.Add("@IsInherited", SqlDbType.TinyInt).Value = nodeData.IsInherited ? 1 : 0;
                cmd.Parameters.Add("@ParentNodeId", SqlDbType.Int).Value = (nodeData.ParentId > 0) ? (object)nodeData.ParentId : DBNull.Value;
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 450).Value = nodeData.Name;
                cmd.Parameters.Add("@DisplayName", SqlDbType.NVarChar, 450).Value = (object)nodeData.DisplayName ?? DBNull.Value;
                cmd.Parameters.Add("@Path", SqlDbType.NVarChar, 450).Value = nodeData.Path;
                cmd.Parameters.Add("@Index", SqlDbType.Int).Value = nodeData.Index;
                cmd.Parameters.Add("@Locked", SqlDbType.TinyInt).Value = nodeData.Locked ? 1 : 0;
                cmd.Parameters.Add("@LockedById", SqlDbType.Int).Value = (nodeData.LockedById > 0) ? (object)nodeData.LockedById : DBNull.Value;
                cmd.Parameters.Add("@ETag", SqlDbType.VarChar, 50).Value = nodeData.ETag ?? String.Empty;
                cmd.Parameters.Add("@LockType", SqlDbType.Int).Value = nodeData.LockType;
                cmd.Parameters.Add("@LockTimeout", SqlDbType.Int).Value = nodeData.LockTimeout;
                cmd.Parameters.Add("@LockDate", SqlDbType.DateTime).Value = nodeData.LockDate;
                cmd.Parameters.Add("@LockToken", SqlDbType.VarChar, 50).Value = nodeData.LockToken ?? String.Empty;
                cmd.Parameters.Add("@LastLockUpdate", SqlDbType.DateTime).Value = nodeData.LastLockUpdate;
                cmd.Parameters.Add("@CreationDate", SqlDbType.DateTime).Value = nodeData.NodeCreationDate;
                cmd.Parameters.Add("@CreatedById", SqlDbType.Int).Value = nodeData.NodeCreatedById;
                cmd.Parameters.Add("@ModificationDate", SqlDbType.DateTime).Value = nodeData.NodeModificationDate;
                cmd.Parameters.Add("@ModifiedById", SqlDbType.Int).Value = nodeData.NodeModifiedById;
                cmd.Parameters.Add("@NodeTimestamp", SqlDbType.Timestamp).Value = SqlProvider.GetBytesFromLong(nodeData.NodeTimestamp);

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    // SELECT [Timestamp] FROM Nodes WHERE NodeId = @NodeId
                    nodeData.NodeTimestamp = SqlProvider.GetLongFromBytes((byte[])reader[0]);
                }
            }
            catch (SqlException sex) //rethrow
            {
                if(sex.Message.StartsWith("Node is out of date"))
                    throw new NodeIsOutOfDateException(nodeData.Id, nodeData.Path, nodeData.VersionId, nodeData.Version, sex, nodeData.NodeTimestamp);
                throw new DataException(sex.Message, sex);
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
                cmd.Dispose();
            }
        }

        //============================================================================ Version Insert/Update

        public int InsertVersionRow(NodeData nodeData)
        {
            SqlProcedure cmd = null;
            SqlDataReader reader = null;
            int result = 0;
            try
            {
                cmd = new SqlProcedure { CommandText = "proc_Version_Insert" };
                cmd.Parameters.Add("@NodeId", SqlDbType.Int).Value = nodeData.Id;
                cmd.Parameters.Add("@MajorNumber", SqlDbType.SmallInt).Value = nodeData.Version.Major;
                cmd.Parameters.Add("@MinorNumber", SqlDbType.SmallInt).Value = nodeData.Version.Minor;
                cmd.Parameters.Add("@Status", SqlDbType.SmallInt).Value = nodeData.Version.Status;
                cmd.Parameters.Add("@CreationDate", SqlDbType.DateTime).Value = nodeData.CreationDate;
                cmd.Parameters.Add("@CreatedById", SqlDbType.Int).Value = nodeData.CreatedById;
                cmd.Parameters.Add("@ModificationDate", SqlDbType.DateTime).Value = nodeData.ModificationDate;
                cmd.Parameters.Add("@ModifiedById", SqlDbType.Int).Value = nodeData.ModifiedById;

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    // SELECT VersionId, [Timestamp] FROM Versions WHERE VersionId = @@IDENTITY
                    result = Convert.ToInt32(reader[0]);
                    nodeData.VersionTimestamp = SqlProvider.GetLongFromBytes((byte[])reader[1]);
                }
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
                cmd.Dispose();
            }
            return result;
        }
        public void UpdateVersionRow(NodeData nodeData)
        {
            SqlProcedure cmd = null;
            SqlDataReader reader = null;
            try
            {
                cmd = new SqlProcedure { CommandText = "proc_Version_Update" };
                cmd.Parameters.Add("@VersionId", SqlDbType.Int).Value = nodeData.VersionId;
                cmd.Parameters.Add("@NodeId", SqlDbType.Int).Value = nodeData.Id;
                cmd.Parameters.Add("@MajorNumber", SqlDbType.SmallInt).Value = nodeData.Version.Major;
                cmd.Parameters.Add("@MinorNumber", SqlDbType.SmallInt).Value = nodeData.Version.Minor;
                cmd.Parameters.Add("@Status", SqlDbType.SmallInt).Value = nodeData.Version.Status;
                cmd.Parameters.Add("@CreationDate", SqlDbType.DateTime).Value = nodeData.CreationDate;
                cmd.Parameters.Add("@CreatedById", SqlDbType.Int).Value = nodeData.CreatedById;
                cmd.Parameters.Add("@ModificationDate", SqlDbType.DateTime).Value = nodeData.ModificationDate;
                cmd.Parameters.Add("@ModifiedById", SqlDbType.Int).Value = nodeData.ModifiedById;

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    // SELECT [Timestamp] FROM Versions WHERE VersionId = @VersionId
                    nodeData.VersionTimestamp = SqlProvider.GetLongFromBytes((byte[])reader[0]);
                }
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
                cmd.Dispose();
            }
        }
        public void CopyAndUpdateVersion(NodeData nodeData, int previousVersionId)
        {
            CopyAndUpdateVersion(nodeData, previousVersionId, 0);
        }
        public void CopyAndUpdateVersion(NodeData nodeData, int previousVersionId, int destinationVersionId)
        {
            SqlProcedure cmd = null;
            SqlDataReader reader = null;
            try
            {
                cmd = new SqlProcedure { CommandText = "proc_Version_CopyAndUpdate" };
                cmd.Parameters.Add("@PreviousVersionId", SqlDbType.Int).Value = previousVersionId;
                cmd.Parameters.Add("@DestinationVersionId", SqlDbType.Int).Value = (destinationVersionId != 0) ? (object)destinationVersionId : DBNull.Value;
                cmd.Parameters.Add("@NodeId", SqlDbType.Int).Value = nodeData.Id;
                cmd.Parameters.Add("@MajorNumber", SqlDbType.SmallInt).Value = nodeData.Version.Major;
                cmd.Parameters.Add("@MinorNumber", SqlDbType.SmallInt).Value = nodeData.Version.Minor;
                cmd.Parameters.Add("@Status", SqlDbType.SmallInt).Value = nodeData.Version.Status;
                cmd.Parameters.Add("@CreationDate", SqlDbType.DateTime).Value = nodeData.CreationDate;
                cmd.Parameters.Add("@CreatedById", SqlDbType.Int).Value = nodeData.CreatedById;
                cmd.Parameters.Add("@ModificationDate", SqlDbType.DateTime).Value = nodeData.ModificationDate;
                cmd.Parameters.Add("@ModifiedById", SqlDbType.Int).Value = nodeData.ModifiedById;

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    // SELECT VersionId, [Timestamp] FROM Versions WHERE VersionId = @NewVersionId
                    nodeData.VersionId = Convert.ToInt32(reader[0]);
                    nodeData.VersionTimestamp = SqlProvider.GetLongFromBytes((byte[])reader[1]);
                }
                if (reader.NextResult())
                {
                    // SELECT BinaryPropertyId, PropertyTypeId FROM BinaryProperties WHERE VersionId = @NewVersionId
                    while (reader.Read())
                    {
                        var binId = Convert.ToInt32(reader[0]);
                        var propId = Convert.ToInt32(reader[1]);
                        var binaryData = (BinaryDataValue)nodeData.GetDynamicRawData(propId);
                        binaryData.Id = binId;
                    }
                }

            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();

                cmd.Dispose();
            }
        }

        //============================================================================ Property Insert/Update

        public void SaveStringProperty(int versionId, PropertyType propertyType, string value)
        {
            if (_flatWriter == null)
                _flatWriter = new FlatPropertyWriter(versionId);

            _flatWriter.WriteStringProperty(value, propertyType);
        }
        public void SaveDateTimeProperty(int versionId, PropertyType propertyType, DateTime value)
        {
            if (_flatWriter == null)
                _flatWriter = new FlatPropertyWriter(versionId);

            _flatWriter.WriteDateTimeProperty(value, propertyType);
        }
        public void SaveIntProperty(int versionId, PropertyType propertyType, int value)
        {
            if (_flatWriter == null)
                _flatWriter = new FlatPropertyWriter(versionId);

            _flatWriter.WriteIntProperty(value, propertyType);
        }
        public void SaveCurrencyProperty(int versionId, PropertyType propertyType, decimal value)
        {
            if (_flatWriter == null)
                _flatWriter = new FlatPropertyWriter(versionId);

            _flatWriter.WriteCurrencyProperty(value, propertyType);
        }
        public void SaveTextProperty(int versionId, PropertyType propertyType, bool isLoaded, string value)
        {
            //if (propertyType.Name == "TextExtract" && !isLoaded)
            //    return;

            if (propertyType == null)
                throw new ArgumentNullException("propertyType");
            if (!isLoaded)
                throw new ApplicationException(); // There is no other data that could be 'IsModified'...

            SqlProcedure cmd = null;
            // Delete existing values (otherwise have to check which table used and then switch between
            // the Insert or Update of the specific table. This way only insert is necessary.)
            try
            {
                cmd = new SqlProcedure { CommandText = "proc_TextProperty_Delete" };
                cmd.Parameters.Add("@VersionId", SqlDbType.Int).Value = versionId;
                cmd.Parameters.Add("@PropertyTypeId", SqlDbType.Int).Value = propertyType.Id;
                cmd.ExecuteNonQuery();
            }
            finally
            {
                cmd.Dispose();
            }

            if (value == null)
                return;

            if (value.Length > SqlProvider.TextAlternationSizeLimit)
            {
                // NText
                try
                {
                    cmd = new SqlProcedure { CommandText = "proc_TextProperty_InsertNText" };
                    cmd.Parameters.Add("@VersionId", SqlDbType.Int).Value = versionId;
                    cmd.Parameters.Add("@PropertyTypeId", SqlDbType.Int).Value = propertyType.Id;
                    cmd.Parameters.Add("@Value", SqlDbType.NText).Value = value;
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            else
            {
                // NVarchar
                try
                {
                    cmd = new SqlProcedure { CommandText = "proc_TextProperty_InsertNVarchar" };
                    cmd.Parameters.Add("@VersionId", SqlDbType.Int).Value = versionId;
                    cmd.Parameters.Add("@PropertyTypeId", SqlDbType.Int).Value = propertyType.Id;
                    cmd.Parameters.Add("@Value", SqlDbType.NVarChar, SqlProvider.TextAlternationSizeLimit).Value = value == null ? (object)DBNull.Value : (object)value; ;
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    cmd.Dispose();
                }
            }
        }
        public void SaveReferenceProperty(int versionId, PropertyType propertyType, IEnumerable<int> value)
        {
            if (propertyType == null)
                throw new ArgumentNullException("propertyType");

            // Remove/Add referredNodeIds to update the database state to the proper one
            SqlProcedure cmd = null;
            try
            {
                string referredListXml = SqlProvider.CreateIdXmlForReferencePropertyUpdate(value);

                cmd = new SqlProcedure { CommandText = "proc_ReferenceProperty_Update" };
                cmd.Parameters.Add("@VersionId", SqlDbType.Int).Value = versionId;
                cmd.Parameters.Add("@PropertyTypeId", SqlDbType.Int).Value = propertyType.Id;
                cmd.Parameters.Add("@ReferredNodeIdListXml", SqlDbType.Xml).Value = referredListXml;
                cmd.ExecuteNonQuery();
            }
            finally
            {
                cmd.Dispose();
            }
        }
        public int InsertBinaryProperty(int versionId, int propertyTypeId, BinaryDataValue value)
        {
            if (value.Stream != null && value.Stream.Length > Int32.MaxValue)
                throw new NotSupportedException(); // MS-SQL does not support stream size over [Int32.MaxValue]

            SqlProcedure cmd = null;
            //object pointer;
            int id = 0;
            try
            {
                cmd = new SqlProcedure { CommandText = "proc_BinaryProperty_Insert" };
                cmd.Parameters.Add("@VersionId", SqlDbType.Int).Value = (versionId != 0) ? (object)versionId : DBNull.Value;
                cmd.Parameters.Add("@PropertyTypeId", SqlDbType.Int).Value = (propertyTypeId != 0) ? (object)propertyTypeId : DBNull.Value;
                cmd.Parameters.Add("@ContentType", SqlDbType.VarChar, 50).Value = value.ContentType;
                cmd.Parameters.Add("@FileNameWithoutExtension", SqlDbType.VarChar, 450).Value = value.FileName.FileNameWithoutExtension == null ? (object)DBNull.Value : (object)value.FileName.FileNameWithoutExtension;
                cmd.Parameters.Add("@Extension", SqlDbType.VarChar, 50).Value = ValidateExtension(value.FileName.Extension);
                cmd.Parameters.Add("@Size", SqlDbType.BigInt).Value = value.Size;
                cmd.Parameters.Add("@Checksum", SqlDbType.VarChar, 200).Value = (value.Checksum != null) ? (object)value.Checksum : DBNull.Value; ;

                //SqlParameter pointerParameter = cmd.Parameters.Add("@Pointer", SqlDbType.Binary, 16);
                //pointerParameter.Direction = ParameterDirection.Output;

                id = Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.CurrentCulture);
                //pointer = pointerParameter.Value;
            }
            finally
            {
                cmd.Dispose();
            }

            if (value.Stream != null && value.Stream.Length > 0)
            {
                // Stream exists -> write it
                WriteBinaryStream(value.Stream, id);
            }

            return id;
        }
        public void UpdateBinaryProperty(int binaryDataId, BinaryDataValue value)
        {
            if (value.Stream != null && value.Stream.Length > Int32.MaxValue)
                throw new NotSupportedException(); // MS-SQL does not support stream size over [Int32.MaxValue]

            bool isRepositoryStream = value.Stream is RepositoryStream;

            SqlProcedure cmd = null;
            //object pointer;
            try
            {
                cmd = new SqlProcedure { CommandText = "proc_BinaryProperty_Update" };
                cmd.Parameters.Add("@BinaryPropertyId", SqlDbType.Int).Value = binaryDataId;
                cmd.Parameters.Add("@ContentType", SqlDbType.VarChar, 50).Value = value.ContentType;
                cmd.Parameters.Add("@FileNameWithoutExtension", SqlDbType.VarChar, 450).Value = value.FileName.FileNameWithoutExtension == null ? (object)DBNull.Value : (object)value.FileName.FileNameWithoutExtension;
                cmd.Parameters.Add("@Extension", SqlDbType.VarChar, 50).Value = ValidateExtension(value.FileName.Extension);
                cmd.Parameters.Add("@Size", SqlDbType.BigInt).Value = value.Size;
                // Do not update the stream field in the database if that is not loaded (other change happened)
                cmd.Parameters.Add("@IsStreamModified", SqlDbType.TinyInt).Value = isRepositoryStream ? 0 : 1;
                //cmd.Parameters.Add("@IsStreamModified", SqlDbType.TinyInt).Value = isLoaded ? 1 : 0;
                cmd.Parameters.Add("@Checksum", SqlDbType.VarChar, 200).Value = (value.Checksum != null) ? (object)value.Checksum : DBNull.Value; ;

                //SqlParameter pointerParameter = cmd.Parameters.Add("@Pointer", SqlDbType.Binary, 16);
                //pointerParameter.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();
                //pointer = pointerParameter.Value;
            }
            finally
            {
                cmd.Dispose();
            }

            if (!isRepositoryStream && value.Stream != null && value.Stream.Length > 0)
            //if(isLoaded && stream != null && stream.Length > 0)
            {
                // Stream isloaded, exists -> write it
                WriteBinaryStream(value.Stream, binaryDataId);
            }
        }
        public void DeleteBinaryProperty(int versionId, PropertyType propertyType)
        {
            if (propertyType == null)
                throw new ArgumentNullException("propertyType");
            SqlProcedure cmd = null;
            try
            {
                cmd = new SqlProcedure { CommandText = "proc_BinaryProperty_Delete" };
                cmd.Parameters.Add("@VersionId", SqlDbType.Int).Value = versionId;
                cmd.Parameters.Add("@PropertyTypeId", SqlDbType.Int).Value = propertyType.Id;
                cmd.ExecuteNonQuery();
            }
            finally
            {
                cmd.Dispose();
            }
        }
        private static string ValidateExtension(string originalExtension)
        {
            return (originalExtension.Length == 0)
                ? string.Empty
                : string.Concat(".", originalExtension);
        }
        private static void WriteBinaryStream(Stream stream, int binaryPropertyId)
        {
            SqlProcedure cmd = null;
            try
            {
                int streamSize = Convert.ToInt32(stream.Length);

                cmd = new SqlProcedure { CommandText = "proc_BinaryProperty_WriteStream" };
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = binaryPropertyId;

                SqlParameter offsetParameter = cmd.Parameters.Add("@Offset", SqlDbType.Int);
                SqlParameter valueParameter = cmd.Parameters.Add("@Value", SqlDbType.VarBinary, streamSize);

                int offset = 0;
                byte[] buffer = null;
                stream.Seek(0, SeekOrigin.Begin);

                while (offset < streamSize)
                {
                    // Buffer size may be less at the end os the stream than the limit
                    int bufferSize = streamSize - offset;

                    //partial update is not supported on FILESTREAM columns
                    //if(bufferSize > SqlProvider.BinaryStreamBufferLength)
                    //    bufferSize = SqlProvider.BinaryStreamBufferLength;

                    if (buffer == null || buffer.Length != bufferSize)
                        buffer = new byte[bufferSize];

                    // Write buffered stream segment
                    stream.Read(buffer, 0, bufferSize);

                    offsetParameter.Value = offset;
                    valueParameter.Value = buffer;

                    cmd.ExecuteNonQuery();

                    offset += bufferSize;
                }
            }
            finally
            {
                cmd.Dispose();
            }
        }
    }
}
