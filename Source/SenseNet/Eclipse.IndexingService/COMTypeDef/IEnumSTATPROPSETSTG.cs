using System.Runtime.InteropServices;
using System.Security;

namespace Eclipse.IndexingService.COMTypeDef
{
    [ComImport, Guid("0000013B-0000-0000-C000-000000000046"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumSTATPROPSETSTG
    {
        void Clone(out IEnumSTATPROPSETSTG ppenum);
        int Next(uint celt, STATPROPSETSTG rgelt, out uint pceltFetched);
        void Reset();
        void Skip(uint celt);
    }




}
