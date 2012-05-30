using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace SenseNet.ContentRepository.Storage.Data
{
    public static class PsStreamManager
    {
        public class PsUploadSession { public int BinaryDataId; public int LastChunk; public HashAlgorithm HashAlgorithm; public byte[] LastBuffer; }

        private static object _sync = new object();
        private static Dictionary<string, PsUploadSession> _sessions = new Dictionary<string, PsUploadSession>();

        public static PsUploadSession Get(string path, int contentId, int propertyTypeId)
        {
            var key = GetSessionId(path, contentId, propertyTypeId);
            return Get(key);
        }
        private static PsUploadSession Get(string key)
        {
            PsUploadSession tran;
            if (_sessions.TryGetValue(key, out tran))
                return tran;
            return null;
        }
        public static void BeginUpload(string path, string fileName, int versionId, int propertyTypeId, long fileSize)
        {
            var key = GetSessionId(path, versionId, propertyTypeId);
            lock (_sync)
            {
                var session = Get(key);
                if (session != null)
                    throw new ApplicationException(string.Concat("The attachment already saving. Path: {0}, Property: {1}", path, ActiveSchema.PropertyTypes.GetItemById(propertyTypeId).Name));

                var binId = DataProvider.Current.InitializeStagingBinaryData(versionId, propertyTypeId, fileName, fileSize);

                session = new PsUploadSession { BinaryDataId = binId, LastChunk = -1 };
                _sessions.Add(key, session);
            }
        }
        public static void UploadChunk(string path, int versionId, int propertyTypeId, byte[] bytes, int offset, int chunkId)
        {
            PsUploadSession session;
            lock (_sync)
            {
                session = Get(path, versionId, propertyTypeId);
                if (session == null)
                    throw new ApplicationException(string.Concat("Connection lost during attachment upload. Path: {0}, Property: {1}", path, ActiveSchema.PropertyTypes.GetItemById(propertyTypeId).Name));
                if (session.LastChunk + 1 != chunkId)
                    throw new ApplicationException(string.Concat("Connection lost during attachment upload. Path: {0}, Property: {1}, LastChunk: {2}, currentChunk: {3}", path, ActiveSchema.PropertyTypes.GetItemById(propertyTypeId).Name, session.LastChunk, chunkId));
                session.LastChunk++;
            }

            DataProvider.Current.SaveChunk(session.BinaryDataId, bytes, offset);

            if (offset == 0)
                session.HashAlgorithm = MD5.Create();
            else
                session.HashAlgorithm.TransformBlock(session.LastBuffer, 0, session.LastBuffer.Length, session.LastBuffer, 0);
            session.LastBuffer = bytes;
        }
        public static void Commit(string path, int versionId, int propertyTypeId)
        {
            PsUploadSession session;
            lock (_sync)
            {
                var key = GetSessionId(path, versionId, propertyTypeId);
                session = Get(key);
                if (session == null)
                    throw new ApplicationException(string.Concat("Connection lost during attachment upload. Path: {0}, Property: {1}", path, ActiveSchema.PropertyTypes.GetItemById(propertyTypeId).Name));
                _sessions.Remove(key);
            }

            session.HashAlgorithm.TransformFinalBlock(session.LastBuffer, 0, session.LastBuffer.Length);
            var checksum = Convert.ToBase64String(session.HashAlgorithm.Hash);

            DataProvider.Current.CopyStagingToBinaryData(versionId, propertyTypeId, session.BinaryDataId, checksum);
            DataProvider.Current.DeleteStagingBinaryData(session.BinaryDataId);
        }
        public static void Rollback(string path, int versionId, int propertyTypeId)
        {
            PsUploadSession session;
            lock (_sync)
            {
                var key = GetSessionId(path, versionId, propertyTypeId);
                session = Get(key);
                if (session == null)
                    throw new ApplicationException(string.Concat("Connection lost during attachment upload: {0}, Property: {1}", path, ActiveSchema.PropertyTypes.GetItemById(propertyTypeId).Name));
                _sessions.Remove(key);
            }

            DataProvider.Current.DeleteStagingBinaryData(session.BinaryDataId);
        }

        private static string GetSessionId(string path, int versionId, int propertyTypeId)
        {
            return String.Concat(versionId, "\t", propertyTypeId, "\t", path);
        }

        /*
        private byte[] BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            byte[] buffer;
            byte[] oldBuffer;
            int bytesRead;
            int oldBytesRead;
            long size;

            using (Stream stream = File.OpenRead((string)e.Argument))
            using (HashAlgorithm hashAlgorithm = MD5.Create())
            {
                size = stream.Length;

                buffer = new byte[4096];

                bytesRead = stream.Read(buffer, 0, buffer.Length);

                do
                {
                    oldBytesRead = bytesRead;
                    oldBuffer = buffer;

                    buffer = new byte[4096];
                    bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        hashAlgorithm.TransformFinalBlock(oldBuffer, 0, oldBytesRead);
                    }
                    else
                    {
                        hashAlgorithm.TransformBlock(oldBuffer, 0, oldBytesRead, oldBuffer, 0);
                    }

                } while (bytesRead != 0);

                return hashAlgorithm.Hash;
            }
        }
        */
    }
}
