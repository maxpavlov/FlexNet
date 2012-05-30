using System;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Proxies;


namespace Eclipse.IndexingService.COMTypeDef
{
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid(Constants.IClassFactoryGUID)]
    internal interface IClassFactory
    {
        void CreateInstance([MarshalAs(UnmanagedType.Interface)] object pUnkOuter, ref Guid refiid, [MarshalAs(UnmanagedType.Interface)] out object ppunk);
        void LockServer(bool fLock);
    }
}
