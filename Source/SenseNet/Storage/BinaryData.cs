using System;
using System.IO;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Caching.Dependency;
using System.Diagnostics;
using System.Globalization;

namespace SenseNet.ContentRepository.Storage
{
	public class BinaryDataValue
	{
		internal int Id { get; set; }
		internal long Size { get; set; }
		internal BinaryFileName FileName { get; set; }
		internal string ContentType { get; set; }
        internal string Checksum { get; set; }
		internal Stream Stream { get; set; }

		internal bool IsEmpty
		{
			get
			{
				if (Id > 0) return false;
				if (Size >= 0) return false;
				if (!String.IsNullOrEmpty(FileName)) return false;
				if (!String.IsNullOrEmpty(ContentType)) return false;
				return Stream == null;
			}
		}
	}

    /// <summary>
    /// BinaryData class handles the data of binary properties.
    /// </summary>
	public class BinaryData : IDynamicDataAccessor
	{
		BinaryDataValue __privateValue;

		//=============================================== Accessor Interface

		Node IDynamicDataAccessor.OwnerNode
		{
			get { return OwnerNode; }
			set { OwnerNode = value; }
		}
		PropertyType IDynamicDataAccessor.PropertyType
		{
			get { return PropertyType; }
			set { PropertyType = value; }
		}
		object IDynamicDataAccessor.RawData { 
			get { return RawData; }
			set { RawData = (BinaryDataValue)value; }
		}
		object IDynamicDataAccessor.GetDefaultRawData() { return GetDefaultRawData(); }

		//=============================================== Accessor Implementation

		internal Node OwnerNode { get; set; }
		internal PropertyType PropertyType { get; set; }
		internal static BinaryDataValue GetDefaultRawData()
		{
            return new BinaryDataValue
            {
                Id = 0,
                ContentType = String.Empty,
                FileName = String.Empty,
                Size = -1,
                Checksum = string.Empty,
                Stream = null
            };
		}
		BinaryDataValue RawData
		{
			get
			{
				if (OwnerNode == null)
					return __privateValue;

                // csak ez lesz, belül switchel shared/private-re
				var value = (BinaryDataValue)OwnerNode.Data.GetDynamicRawData(PropertyType);
				return value;
				//return value ?? __privateValue;
			}
			set
			{
                __privateValue = new BinaryDataValue
                {
                    Id = value.Id,
                    ContentType = value.ContentType,
                    FileName = value.FileName,
                    Size = value.Size,
                    Checksum = value.Checksum,
                    Stream = CloneStream(value.Stream)
                };
			}
		}
		public bool IsEmpty
		{
			get
			{
				if (OwnerNode == null)
					return __privateValue.IsEmpty;
				if (RawData == null)
					return true;
				return RawData.IsEmpty;
			}
		}

		//=============================================== Data

		public bool IsModified
		{
			get
			{
				if (OwnerNode == null)
					return true;
				return OwnerNode.Data.IsModified(PropertyType);
			}
		}
		private void Modifying()
		{
			//if (OwnerNode != null)
			//    OwnerNode.BackwardCompatibilityPropertySet(PropertyType.Name, this);

			if (IsModified)
				return;

			//-- Clone
			var orig = (BinaryDataValue)OwnerNode.Data.GetDynamicRawData(PropertyType);
			BinaryDataValue data;
			if (orig == null)
			{
				data = GetDefaultRawData();
			}
			else
			{
				data = new BinaryDataValue
				{
					Id = orig.Id,
					ContentType = orig.ContentType,
					FileName = orig.FileName,
					Size = orig.Size,
                    Checksum = orig.Checksum,
					Stream = orig.Stream
				};
			}
            OwnerNode.MakePrivateData();
            OwnerNode.Data.SetDynamicRawData(PropertyType, data, false);
		}
        private void Modified()
        {
            if(OwnerNode != null)
                if(OwnerNode.Data.SharedData != null)
                    OwnerNode.Data.CheckChanges(PropertyType);
        }

		//=============================================== Accessors

        public int Id
        {
			get { return RawData == null ? 0 : RawData.Id; }
			internal set
			{
				Modifying();
				RawData.Id = value;
                Modified();
			}
        }
		public long Size
        {
			get { return RawData == null ? -1 : RawData.Size; }
			internal set
			{
				Modifying();
				RawData.Size = value;
                Modified();
            }
        }
		public BinaryFileName FileName
		{
            get { return RawData == null ? new BinaryFileName("") : RawData.FileName; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");

				Modifying();
                var rawData = this.RawData;
                rawData.FileName = value;
                rawData.ContentType = GetMimeType(value);
                Modified();
            }
		}
		public string ContentType
		{
			get { return RawData == null ? string.Empty : RawData.ContentType; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");

				Modifying();
				RawData.ContentType = value;
                Modified();
            }
		}
        public string Checksum
        {
            get
            {
                var raw = RawData;
                if (raw == null)
                    return null;
                return raw.Checksum;
            }
        }

		public Stream GetStream()
		{
			var raw = RawData;
			if (raw == null)
				return null;
			var stream = raw.Stream;
			if (stream != null)
				return CloneStream(stream);
			if (OwnerNode == null)
				return null;

            // Itt töltöm be, és adom vissza a Stream-et.
			// Ezt kéne megoldani úgy, hogy RepositoryStream-et adok vissza - a RepositoryStream-et ezek szerint VersionId - PropTypeId-val kell kezelni
            // A cache felelõsséget most átnyomom a DBS-re
            
            return DataBackingStore.GetBinaryStream2(OwnerNode.Id, OwnerNode.VersionId, PropertyType.Id);
            //return DataBackingStore.GetBinaryStream(OwnerNode.VersionId, PropertyType.Id);
		}
		public void SetStream(Stream stream)
		{
			Modifying();
            var rawData = this.RawData;
			if (stream == null)
			{
                rawData.Size = -1;
                rawData.Checksum = string.Empty;
                rawData.Stream = null;
			}
			else
			{
                rawData.Size = stream.Length;
                rawData.Stream = stream;
                rawData.Checksum = CalculateChecksum(stream);
            }
            Modified();
		}
        public static string CalculateChecksum(Stream stream)
        {
            var pos = stream.Position;
            stream.Position = 0;
            var b64 = Convert.ToBase64String(new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(stream));
            stream.Position = pos;
            return b64;
        }

		//===============================================

        public BinaryData()
        {
			__privateValue = GetDefaultRawData();
        }

		public void Reset()
		{
			Id = 0;
			FileName = String.Empty;
			ContentType = String.Empty;
			Size = -1;
			this.SetStream(null);
		}
		public void CopyFrom(BinaryData data)
		{
			//Id = data.Id;
			FileName = data.FileName;
			ContentType = data.ContentType;
			Size = data.Size;
			this.SetStream(data.GetStream());
		}


		private static Stream CloneStream(Stream stream)
		{
			if (stream == null || !stream.CanRead)
				return null;

			long pos = stream.Position;
			stream.Seek(0, SeekOrigin.Begin);
			Stream clone = new MemoryStream(new BinaryReader(stream).ReadBytes((int)stream.Length));
			clone.Seek(0, SeekOrigin.Begin);
			stream.Seek(pos, SeekOrigin.Begin);

			return clone;
		}
		private static string GetMimeType(BinaryFileName value)
		{
            if (value == null)
                return string.Empty;
			string ext = value.Extension;
            if (ext == null)
                return string.Empty;
			if (ext.Length > 0 && ext[0] == '.')
				ext = ext.Substring(1);
            var mimeType = MimeTable.GetMimeType(ext.ToLower(CultureInfo.InvariantCulture));
			return mimeType;
		}
        ////TODO: Not used (de meg kellhet)
        //private static bool BinaryEquals(Stream binary1, Stream binary2)
        //{
        //    if (binary1 == null && binary2 == null)
        //        return true;
        //    else if (binary1 == null)
        //        return false;
        //    else if (binary2 == null)
        //        return false;
        //    else
        //    {
        //        if (binary1.Length != binary2.Length)
        //            return false;

        //        long pos1, pos2;
        //        pos1 = binary1.Position;
        //        pos2 = binary2.Position;
        //        binary1.Seek(0, SeekOrigin.Begin);
        //        binary2.Seek(0, SeekOrigin.Begin);
        //        bool ret = true;
        //        int i1, i2;
        //        while ((i1 = binary1.ReadByte()) > -1)
        //        {
        //            i2 = binary2.ReadByte();
        //            if (i1 != i2)
        //            {
        //                ret = false;
        //                break;
        //            }
        //        }
        //        binary1.Seek(pos1, SeekOrigin.Begin);
        //        binary2.Seek(pos2, SeekOrigin.Begin);
        //        return ret;
        //    }
        //}

	}
}