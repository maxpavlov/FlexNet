using System;
using System.Runtime.InteropServices;
using FILETIME=System.Runtime.InteropServices.ComTypes.FILETIME;

namespace Eclipse.IndexingService.COMTypeDef
{
    [ComImport, Guid("0000013A-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPropertyStorage
    {
        int ReadMultiple(uint cpspec, PROPSPEC[] rgpspec, PROPVARIANT[] rgpropvar);
        void WriteMultiple(uint cpspec, PROPSPEC[] rgpspec, PROPVARIANT[] rgpropvar, uint propidNameFirst);
        void DeleteMultiple(uint cpspec, PROPSPEC[] rgpspec);
        void ReadPropertyNames(uint cpropid, uint[] rgpropid, string[] rglpwstrName);
        void WritePropertyNames(uint cpropid, uint[] rgpropid, string[] rglpwstrName);
        void DeletePropertyNames(uint cpropid, uint[] rgpropid);
        void SetClass(ref Guid clsid);
        void Commit(uint grfCommitFlags);
        void Revert();
        void Enum(out IEnumSTATPROPSTG ppenum);
        void Stat(out STATPROPSETSTG pstatpsstg);
        void SetTimes(ref FILETIME pctime, ref FILETIME patime, ref FILETIME pmtime);

    }




}
