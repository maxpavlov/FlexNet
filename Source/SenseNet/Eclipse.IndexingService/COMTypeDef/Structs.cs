using System;
using System.Runtime.InteropServices;
using FILETIME=System.Runtime.InteropServices.ComTypes.FILETIME;

namespace Eclipse.IndexingService.COMTypeDef
{

    [StructLayout(LayoutKind.Sequential)]
    public struct FILTERREGION
    {
        private uint idChunk;
        private uint cwcStart;
        private uint cwcExtent;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FULLPROPSPEC
    {
        public Guid guid;
        public PROPSPEC property;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class PROPSPEC
    {
        public ulKind propType;
        public PROPSPECunion union;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PROPSPECunion
    {
        [FieldOffset(0)]
        public IntPtr lpwstr;

        [FieldOffset(0)]
        public uint propId;

    }


    [StructLayout(LayoutKind.Sequential)]
    public struct STAT_CHUNK
    {
        public uint idChunk;
        public CHUNK_BREAKTYPE breakType;
        public CHUNKSTATE flags;
        public uint locale;
        public FULLPROPSPEC attribute;
        public uint idChunkSource;
        public uint cwcStartSource;
        public uint cwcLenSource;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROPVARIANT
    {
        public VARTYPE vt;
        public ushort wReserved1;
        public ushort wReserved2;
        public ushort wReserved3;
        public PropVariantUnion union;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BLOB
    {
        public uint cbSize;
        public IntPtr pBlobData;
    }




    [StructLayout(LayoutKind.Sequential)]
    public struct BSTRBLOB
    {
        public uint cbSize;
        public IntPtr pData;
    }




    [StructLayout(LayoutKind.Sequential)]
    public struct CArray
    {
        public uint cElems;
        public IntPtr pElems;
    }




    [StructLayout(LayoutKind.Sequential)]
    public struct CY
    {
        public uint Lo;
        public int Hi;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PropVariantUnion
    {
        // Fields
        [FieldOffset(0)]
        public BLOB blob;
        [FieldOffset(0)]
        public short boolVal;
        [FieldOffset(0)]
        public BSTRBLOB bstrblobVal;
        [FieldOffset(0)]
        public IntPtr bstrVal;
        [FieldOffset(0)]
        public byte bVal;
        [FieldOffset(0)]
        public CArray cArray;
        [FieldOffset(0)]
        public sbyte cVal;
        [FieldOffset(0)]
        public CY cyVal;
        [FieldOffset(0)]
        public double date;
        [FieldOffset(0)]
        public double dblVal;
        [FieldOffset(0)]
        public FILETIME filetime;
        [FieldOffset(0)]
        public float fltVal;
        [FieldOffset(0)]
        public long hVal;
        [FieldOffset(0)]
        public int intVal;
        [FieldOffset(0)]
        public short iVal;
        [FieldOffset(0)]
        public int lVal;
        [FieldOffset(0)]
        public IntPtr parray;
        [FieldOffset(0)]
        public IntPtr pboolVal;
        [FieldOffset(0)]
        public IntPtr pbstrVal;
        [FieldOffset(0)]
        public IntPtr pbVal;
        [FieldOffset(0)]
        public IntPtr pclipdata;
        [FieldOffset(0)]
        public IntPtr pcVal;
        [FieldOffset(0)]
        public IntPtr pcyVal;
        [FieldOffset(0)]
        public IntPtr pdate;
        [FieldOffset(0)]
        public IntPtr pdblVal;
        [FieldOffset(0)]
        public IntPtr pdecVal;
        [FieldOffset(0)]
        public IntPtr pdispVal;
        [FieldOffset(0)]
        public IntPtr pfltVal;
        [FieldOffset(0)]
        public IntPtr pintVal;
        [FieldOffset(0)]
        public IntPtr piVal;
        [FieldOffset(0)]
        public IntPtr plVal;
        [FieldOffset(0)]
        public IntPtr pparray;
        [FieldOffset(0)]
        public IntPtr ppdispVal;
        [FieldOffset(0)]
        public IntPtr ppunkVal;
        [FieldOffset(0)]
        public IntPtr pscode;
        [FieldOffset(0)]
        public IntPtr pStorage;
        [FieldOffset(0)]
        public IntPtr pStream;
        [FieldOffset(0)]
        public IntPtr pszVal;
        [FieldOffset(0)]
        public IntPtr puintVal;
        [FieldOffset(0)]
        public IntPtr puiVal;
        [FieldOffset(0)]
        public IntPtr pulVal;
        [FieldOffset(0)]
        public IntPtr punkVal;
        [FieldOffset(0)]
        public IntPtr puuid;
        [FieldOffset(0)]
        public IntPtr pvarVal;
        [FieldOffset(0)]
        public IntPtr pVersionedStream;
        [FieldOffset(0)]
        public IntPtr pwszVal;
        [FieldOffset(0)]
        public int scode;
        [FieldOffset(0)]
        public ulong uhVal;
        [FieldOffset(0)]
        public uint uintVal;
        [FieldOffset(0)]
        public ushort uiVal;
        [FieldOffset(0)]
        public uint ulVal;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct STATPROPSTG
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        private string lpwstrName;
        private uint propid;
        private VARTYPE vt;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STATPROPSETSTG
    {
        private Guid fmtid;
        private Guid clsid;
        private uint grfFlags;
        private FILETIME mtime;
        private FILETIME ctime;
        private FILETIME atime;
        private uint dwOSVersion;
    }

}
