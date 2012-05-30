using System.Runtime.InteropServices;
using System.Security;
using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;

namespace Eclipse.IndexingService.COMTypeDef
{
    [ComImport, Guid("0000000D-0000-0000-C000-000000000046"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumSTATSTG
    {
        [PreserveSig]
        int Next([In, MarshalAs(UnmanagedType.U4)] int celt, [Out, MarshalAs(UnmanagedType.LPArray)] STATSTG[] rgVar, [MarshalAs(UnmanagedType.U4)] out int pceltFetched);
        [PreserveSig]
        int Skip([In, MarshalAs(UnmanagedType.U4)] int celt);
        [PreserveSig]
        int Reset();
        int Clone([MarshalAs(UnmanagedType.Interface)] out IEnumSTATSTG newEnum);
    }
}
