using System;
using System.Runtime.InteropServices;


namespace Eclipse.IndexingService.COMTypeDef
{
    [ComImport, Guid("0000010A-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPersistStorage
    {
        void GetClassID(out Guid pClassID);
        [PreserveSig]
        int IsDirty();
        void InitNew(IStorage pstg);
        [PreserveSig]
        int Load(IStorage pstg);
        void Save(IStorage pStgSave, bool fSameAsLoad);
        void SaveCompleted(IStorage pStgNew);
        void HandsOffStorage();
    }
}
