using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Globalization;

namespace SenseNet.ContentRepository.Storage.Data
{
    public class BinaryCacheEntity
    {
        public byte[] RawData { get; set; }
        public long Length { get; set; }
        public int BinaryPropertyId { get; set; }
    }

    class RepositoryStream : Stream
    {
        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return true; } }
        public override bool CanWrite { get { return false; } }

        public override long Position { get; set; }

        long _length;
        public override long Length
        {
            get { return _length; }
        }

        private byte[] _innerBuffer;

        private long _innerBufferFirstPostion;

        private int _binaryPropertyId;

        
        public RepositoryStream(long size, byte[] binary)
        {
            if (binary == null)
                throw new ArgumentNullException("binary", "Binary cannot be null. If the binary cannot be fully loaded - therefore is null - use the (long size, int binaryPropertyId) ctor.");
            _length = size;
            _innerBuffer = binary;
            //_innerBufferFirstPostion = 0;
        }

        public RepositoryStream(long size, int binaryPropertyId)
        {
            _length = size;
            _binaryPropertyId = binaryPropertyId;
        }
        
        

        public override void SetLength(long value)
		{ throw new NotSupportedException("RepositoryStream does not support setting length."); }

        public override void Write(byte[] buffer, int offset, int count)
		{ throw new NotSupportedException("RepositoryStream does not support writing."); }

        public override void Flush()
		{ throw new NotSupportedException("RepositoryStream does not support flushing."); }


        private bool CanInnerBufferHandleReadRequest(int count)
        {
            if (_innerBuffer == null)
                return false;

            if (Position < _innerBufferFirstPostion)
                return false;


            if ((_innerBufferFirstPostion + _innerBuffer.Length) < (Position + count))
                return false;

            return true;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
			//BinaryTraceHelper.TraceMessage("RepositoryStream.Read(buffer={0}, offset={1}, count={2})", buffer, offset, count);
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset + count > buffer.Length)
                throw new ArgumentException("Offset + count must not be greater than the buffer length.");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "The offset must be greater than zero.");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "The count must be greater than zero.");

            // Calculate the maximum count of the bytes that can be read.
            // Return immediately if nothing to read.
            var maximumReadableByteCount = Length - Position;
            if (maximumReadableByteCount < 1)
                return 0;

            var realCount = (int)Math.Min(count, maximumReadableByteCount);

            if (CanInnerBufferHandleReadRequest(realCount))
            {
                Array.Copy(_innerBuffer, (int)Position - _innerBufferFirstPostion, buffer, offset, realCount);
            }
            else
            {
                // Be kell tölteni a Position-től számítva count byte-ot az _innerBuffer-be
                if (realCount < RepositoryConfiguration.CachedBinarySize)
                {
                    // Csak keveset kell olvasni (pl. ReadByte-nál csak 1-et kéne)
                    // Inkább olvassunk egy chunk-ot, aztán az InnerBufferből szolgáljuk ki
                    _innerBuffer = DataProvider.Current.LoadBinaryFragment(_binaryPropertyId, Position, RepositoryConfiguration.CachedBinarySize);
                    _innerBufferFirstPostion = Position;
                    Array.Copy(_innerBuffer, 0, buffer, offset, realCount);
                }
                else
                {
                    // Nagyot akar olvasni, mondjuk 10MB-t egyben - hát jó, a buffere már megvan, szépen másolgassuk be bele
                    int bytesRead = 0;
                    while (bytesRead < realCount)
                    {
                        int remainingBytes = realCount - bytesRead;
                        int bytesToReadInThisIteration = Math.Min(realCount - bytesRead, RepositoryConfiguration.CachedBinarySize);
                        var binaryFragment = DataProvider.Current.LoadBinaryFragment(_binaryPropertyId, Position + bytesRead, bytesToReadInThisIteration);
                        Array.Copy(binaryFragment, 0, buffer, bytesRead, bytesToReadInThisIteration);
                        bytesRead += bytesToReadInThisIteration;
                    }
                }
            }

			//BinaryTraceHelper.TraceMessage("RepositoryStream.Read will return {0}", realCount);

            Position += realCount;

            return realCount;
        }

        public override int ReadByte()
        {
			//BinaryTraceHelper.TraceMessage("RepositoryStream.ReadByte()");
            return base.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position = Position + offset;
                    break;
                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
                default:
                    throw new NotSupportedException(String.Concat("SeekOrigin type ", origin, " is not supported."));
            }
            return Position;
        }

    }
}
