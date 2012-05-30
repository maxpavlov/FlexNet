
using System.Runtime.InteropServices;
using System.Security;

namespace Eclipse.IndexingService.COMTypeDef
{
    [ComImport, Guid("00000139-0000-0000-C000-000000000046"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumSTATPROPSTG
    {
        void Clone(out IEnumSTATPROPSTG ppenum);
        int Next(uint celt, STATPROPSTG rgelt, out uint pceltFetched);
        void Reset();
        void Skip(uint celt);
    }
}
