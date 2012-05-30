using System;
using System.Runtime.InteropServices;
using System.Security;
using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;

namespace Eclipse.IndexingService.COMTypeDef
{
    [ComImport, Guid("0000000A-0000-0000-C000-000000000046"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ILockBytes
    {
        void ReadAt([In, MarshalAs(UnmanagedType.U8)] long ulOffset, [Out] IntPtr pv, [In, MarshalAs(UnmanagedType.U4)] int cb, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pcbRead);
        void WriteAt([In, MarshalAs(UnmanagedType.U8)] long ulOffset, IntPtr pv, [In, MarshalAs(UnmanagedType.U4)] int cb, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pcbWritten);
        void Flush();
        void SetSize([In, MarshalAs(UnmanagedType.U8)] long cb);
        void LockRegion([In, MarshalAs(UnmanagedType.U8)] long libOffset, [In, MarshalAs(UnmanagedType.U8)] long cb, [In, MarshalAs(UnmanagedType.U4)] int dwLockType);
        void UnlockRegion([In, MarshalAs(UnmanagedType.U8)] long libOffset, [In, MarshalAs(UnmanagedType.U8)] long cb, [In, MarshalAs(UnmanagedType.U4)] int dwLockType);
        void Stat([Out] STATSTG pstatstg, [In, MarshalAs(UnmanagedType.U4)] int grfStatFlag);
    }
}
