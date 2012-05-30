using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Eclipse.IndexingService.COMTypeDef;

namespace Eclipse.IndexingService
{
    /// <summary>
    /// Implements a TextReader that reads from an IFilter.
    /// </summary>
    public class FilterReader : TextReader
    {
        IFilter _filter;
        char[] _buffer;
        uint _currPosition;
        uint _topSize;
        uint _resPosition; 
        uint _resTopSize;
        // Note : enlarge ResBufSize when you meet problem with pdf documents
        readonly uint ResBufSize = 0x10;  //reserved size for extended buffer which will be used when the current size is too small passing to the GetText method or appending half CRLF to the end of buffer
        private bool _endOfCurrChunk = true;
        byte[] Data { get; set; } //hold input bytes ref, will be null after executing INIT
        uint BufferSize { get; set; }
        uint ActBufferSize { get { return BufferSize + ResBufSize; } }
        /// <summary>
        /// File's Extension
        /// </summary>
        string Extension { get; set; }
        /// <summary>
        /// Full Path
        /// </summary>
        public string FileName { get; private set; } 
        /// <summary>
        /// Avoid to be interrupted when parsing unknown extension; Only for Default Filter
        /// </summary>
        public bool IgnoreError { get; set; }
        

        protected override void Dispose(bool disposing)
        {
            if (_filter != null)
            {
                var filterClass = _filter as MixedIFilterClass;
                Marshal.ReleaseComObject(_filter);
                if(null != filterClass)
                    filterClass.Dispose();
                _filter = null;
            }
            if(disposing)
                GC.SuppressFinalize(this);
        }

        ~FilterReader()
        {
            Dispose(false);
        }

        public override int Read()
        {
            if (_filter == null)
                throw new NullReferenceException("internal filter not initialized");
            if (_topSize != 0)
            {
                var c = _buffer[_currPosition];
                _currPosition++;
                if (_currPosition == _topSize)
                    _currPosition = _topSize = 0;
                return c;
            }
            var count = 1;
            ReadIFilterImpl(null, 1, ref count, false, false);
            if (_topSize == 0)
                return -1;
            _currPosition = 1;
            if (_currPosition == _topSize)
                _currPosition = _topSize = 0;
            return _buffer[0];
        }

        public override int Peek()
        {
            if (_filter == null)
                throw new NullReferenceException("internal filter not initialized");
            if (_topSize != 0)
            {
                return _buffer[_currPosition];
            }
            var count = 1;
            ReadIFilterImpl(null, 1, ref count, true, false);
            return _topSize == 0 ? -1 : _buffer[0];
        }

        public override int Read(char[] array, int offset, int count)
        {
            if(_filter == null)
                throw new NullReferenceException("internal filter not initialized");
            if (offset < 0 || count <= 0 || array == null || array.Length < offset + count)
                throw new ArgumentException("invalid parameters");
            return InternalRead(array, offset, count);
        }

        public override string ReadToEnd()
        {
            if (_filter == null)
                throw new NullReferenceException("internal filter not initialized");
            int num;
            var builder = new StringBuilder(0x1000);
            if(_topSize != 0)
            {
                builder.Append(_buffer, (int)_currPosition, (int)(_topSize - _currPosition));
                _currPosition = _topSize = 0;
            }
            while((num = InternalRead(_buffer, 0, (int)BufferSize)) != 0)
            {
                builder.Append(_buffer, 0, num);
                if(num < BufferSize)
                    break;
            }
            return builder.ToString();
        }


        private int InternalRead(char[] array, int offset, int count)
        {
            var numToRead = count;
            var isNeedingToRead = ReadFromBuffer(array, offset, ref count);
            if (isNeedingToRead)
                ReadIFilterImpl(array, offset + numToRead - count, ref count, false, array == _buffer);
            ReadFromBuffer(array, offset + numToRead - count, ref count);
            return numToRead - count;
        }



        private bool ReadFromBuffer(char[] array, int offset, ref int count)
        {
            if(_topSize != 0)
            {
                var length = Math.Min((int)(_topSize - _currPosition), count);
                Array.Copy(_buffer, _currPosition, array, offset, length);
                _currPosition += (uint)length;
                count -= length;
                if (_currPosition == _topSize)
                    _currPosition = _topSize = 0;
                if(count == 0)
                    return false;
            }
            return true;
        }


        private void ReadIFilterImpl(char[] array, int offset, ref int remaining, bool peek, bool forceDirectlyWrite)
        {
            if(_resTopSize > 0)
            {
                var length = Math.Min((int)(_resTopSize - _resPosition), remaining);
                if (peek || (!forceDirectlyWrite && remaining < BufferSize))
                {
                    Array.Copy(_buffer, _resPosition, _buffer, offset, length);
                    _topSize++;
                }
                else
                {
                    Array.Copy(_buffer, BufferSize + _resPosition, array, offset, length);
                    offset += length;
                    remaining -= length;
                }
                _resPosition += (uint)length;
                if (_currPosition == _topSize)
                    _currPosition = _topSize = 0;
                if (_resPosition == _resTopSize)
                    _resPosition = _resTopSize = 0;
                if (remaining == 0)
                    return;
            }
            while (true)
            {
                STAT_CHUNK chunk;
                if (_endOfCurrChunk)
                while (true)
                {
                    var returnCode = _filter.GetChunk(out chunk);
                    _endOfCurrChunk = false;
                    switch (returnCode)
                    {
                        case IFilterReturnCodes.FILTER_E_ACCESS:
                            throw new Exception("General access failure.");
                        case IFilterReturnCodes.FILTER_E_PASSWORD:
                            throw new Exception("Password or other security-related access failure.");
                        case IFilterReturnCodes.FILTER_E_EMBEDDING_UNAVAILABLE:
                        case IFilterReturnCodes.FILTER_E_LINK_UNAVAILABLE:
                            continue;
                        case IFilterReturnCodes.FILTER_E_END_OF_CHUNKS:
                            return;
                        default:
                            if ((chunk.flags & CHUNKSTATE.CHUNK_TEXT) == 0)
                                continue;
                            switch (chunk.breakType)
                            {
                                case CHUNK_BREAKTYPE.CHUNK_NO_BREAK:
                                    break;
                                case CHUNK_BREAKTYPE.CHUNK_EOW:
                                    if (peek || (!forceDirectlyWrite && remaining < BufferSize))
                                    {
                                        _buffer[_topSize++] = ' ';
                                    }
                                    else
                                    {
                                        array[offset++] = ' ';
                                        remaining--;
                                    }
                                    break;
                                case CHUNK_BREAKTYPE.CHUNK_EOC:
                                case CHUNK_BREAKTYPE.CHUNK_EOP:
                                case CHUNK_BREAKTYPE.CHUNK_EOS:
                                    var newline = Environment.NewLine.ToCharArray();
                                    if (BufferSize < _topSize + 2)
                                    {
                                        Array.Copy(newline, 0, _buffer, _topSize++, 2);
                                        _resTopSize++;
                                        return;
                                    }
                                    if (remaining < 2)
                                    {
                                        Debug.Assert(array == _buffer);
                                        Array.Copy(newline, 0, array, offset, 2);
                                        remaining--;
                                        _resTopSize++;
                                        return;
                                    }
                                    if (peek || (!forceDirectlyWrite && remaining < BufferSize))
                                    {
                                        Array.Copy(newline, 0, _buffer, _topSize, 2);
                                        _topSize += 2;
                                    }
                                    else
                                    {
                                        Array.Copy(newline, 0, array, offset, 2);
                                        offset += 2;
                                        remaining -= 2;
                                    }
                                    break;
                            }
                            break;
                    }
                    break;
                }
                while (true)
                {
                    if (remaining <= _topSize)
                        return;
                    bool useBuffer = !forceDirectlyWrite && remaining < BufferSize;
                    var size = BufferSize;
                    if (useBuffer)
                        size -= _topSize;
                    else
                    {
                        if (remaining < BufferSize)
                            size = (uint)remaining;
                    }
                    if (size < ResBufSize)
                        size = ResBufSize;
                    var handle = GCHandle.Alloc(useBuffer ? _buffer : array, GCHandleType.Pinned);
                    var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(useBuffer ? _buffer : array, useBuffer ? (int)_topSize : offset);
                    IFilterReturnCodes returnCode;
                    try
                    {
#if DEBUG
                        Trace.Write(size);
#endif
                        returnCode = _filter.GetText(ref size, ptr);
#if DEBUG
                        Trace.WriteLine("->"+size);
#endif
                    }
                    finally 
                    {
                        handle.Free();
                    }
                    if(returnCode != IFilterReturnCodes.FILTER_E_NO_TEXT)
                    {
                        if (useBuffer)
                            _topSize += size;
                        else
                        {
                            offset += (int)size;
                            remaining -= (int)size;
                        }
                        if(_topSize > BufferSize)
                        {
                            _resTopSize = _topSize - BufferSize;
                            _topSize = BufferSize;
                        }
                    }
                    if (returnCode == IFilterReturnCodes.FILTER_S_LAST_TEXT || returnCode == IFilterReturnCodes.FILTER_E_NO_MORE_TEXT || (returnCode == IFilterReturnCodes.FILTER_E_NO_TEXT && size != 0) || (null == FileName && IgnoreError && returnCode == IFilterReturnCodes.E_INVALIDARG))
                    {
                        _endOfCurrChunk = true;
                        if (remaining <= _topSize)
                            return;
                        break;
                    }
                    if(returnCode != IFilterReturnCodes.S_OK)
                    {
                        throw new Exception("a error occur when getting text by current filter", new Exception(returnCode.ToString()));
                    }
                }
            }
        }

        public FilterReader(string fileName, string extension): this(fileName, extension, 0x2000)
        {
        }

        public FilterReader(string fileName): this(fileName, null, 0x2000)
        {
        }

        public FilterReader(string fileName, uint blockSize): this(fileName, null, blockSize)
        {
        }

        public FilterReader(string fileName, string extension, uint blockSize)
        {
            if(blockSize < 0x2)
                throw new ArgumentOutOfRangeException("blockSize");
            if(String.IsNullOrEmpty(fileName))
                throw new ArgumentException("fileName");
            FileName = fileName;
            Extension = extension;
            BufferSize = blockSize;
        }

        public FilterReader(byte[] bytes, uint blockSize): this(bytes, null, blockSize)
        {
        }

        public FilterReader(byte[] bytes): this(bytes, null)
        {
        }

        public FilterReader(byte[] bytes, string extension): this(bytes, extension, 0x8000)
        {
        }

        public FilterReader(byte[] bytes, string extension, uint blockSize)
        {
            if(null == bytes || bytes.Length == 0)
                throw new ArgumentNullException("bytes");
            if (blockSize < 0x2)
                throw new ArgumentOutOfRangeException("blockSize");
            Data = bytes;
            BufferSize = blockSize;
            Extension = extension;
        }

        public void Init()
        {
            _buffer = new char[ActBufferSize];
            try
            {
                _filter = (null != Data ? (Extension != null ? FilterLoader.LoadIFilterFromStream(Data, Extension) : FilterLoader.LoadIFilterFromStream(Data, true))
                    : Extension == null ? FilterLoader.LoadIFilterFromIPersistFile(FileName) : FilterLoader.LoadIFilterFromIPersistFile(FileName, Extension));
            }
            finally
            {
                Data = null;
            }
            Debug.Assert(_filter != null);
            if (null == _filter)
                throw new Exception("Filter Not Found or Loaded");
        }

        public void Init(out Exception ex)
        {
            ex = null;
            try
            {
                Init();
            }
            catch (Exception e)
            {
                ex = e;
            }
        }
    }
}
