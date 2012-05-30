using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using Eclipse.IndexingService.COMTypeDef;
using Microsoft.Win32;
using System.Reflection;
using Microsoft.Win32.SafeHandles;

namespace Eclipse.IndexingService
{

    /// <summary>
    /// FilterLoader finds the dll and ClassID of the COM object responsible  
    /// for filtering a specific file extension. 
    /// It then loads that dll, creates the appropriate COM object and returns 
    /// a pointer to an IFilter instance
    /// </summary>
    public static class FilterLoader
    {
        //SqlString is a real nullable string supported by Microsoft, sigh...
        static readonly Dictionary<SqlString, KeyValuePair<LibHandle, IntPtr>> Cache_V1 = new Dictionary<SqlString, KeyValuePair<LibHandle, IntPtr>>();
        static readonly Dictionary<SqlString, Type> Cache_V2 = new Dictionary<SqlString, Type>();

        //DllGetClassObject fuction pointer signature
        delegate int DllGetClassObject(ref Guid ClassId, ref Guid InterfaceId, out IntPtr ppunk);
        delegate int DllCanUnloadNow();
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        [DllImport("kernel32.dll")]
        static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("kernel32.dll")]
        static extern LibHandle LoadLibrary(string lpFileName);
        [DllImport("query.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern int LoadIFilter(string pwcsPath,[MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter,out IFilter ppIUnk);
        [DllImport("query.dll", PreserveSig = false)]
        extern static int BindIFilterFromStream(IStream pStm, IntPtr pUnkOuter, out IFilter ppIUnk);  // The BindIFilterFromStream function takes an IStream interface pointer of a Structured Storage object. Be sure not to create a simple stream (look into STATSTG) to feed this function.
        [DllImport("query.dll", PreserveSig = false)]
        extern static int BindIFilterFromStorage(IStorage pStg, IntPtr pUnkOuter, out IFilter ppIUnk);  // It works fine.
        [SecurityCritical, DllImport("ole32.dll", CharSet = CharSet.Auto, ExactSpelling = true, PreserveSig = false)]
        static extern void CreateStreamOnHGlobal(IntPtr hGlobal, bool fDeleteOnRelease,out IStream ppstm);
        [DllImport("ole32.dll", PreserveSig = false)]
        static extern IStorage StgOpenStorageOnILockBytes(ILockBytes iLockBytes, IStorage pStgPriority, int grfMode, int sndExcluded, int reserved);
        [DllImport("ole32.dll", PreserveSig = false)]
        static extern ILockBytes CreateILockBytesOnHGlobal(HandleRef hGlobal, bool fDeleteOnRelease);



        class LibHandle : SafeHandle
        {
            public LibHandle() : base(IntPtr.Zero, true)
            {
                
            }
            public override bool IsInvalid
            {
                get
                {
                    return this.handle == IntPtr.Zero;
                }
            }

            protected override bool ReleaseHandle()
            {
                bool succeeds = FreeLibrary(this.handle);
                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
                this.handle = IntPtr.Zero;
                return succeeds;
            }
        }

        static readonly DestructorObj _obj = new DestructorObj();
        class DestructorObj
        {
            ~DestructorObj()
            {
                ReleaseClassCache();
            }
        }


        public static void ReleaseClassCache()
        {
            foreach (var pair in Cache_V1)
            {
                while (Marshal.Release(pair.Value.Value) != 0) { }
                IntPtr dllCanUnloadNowPtr = GetProcAddress(pair.Value.Key.DangerousGetHandle(), "DllCanUnloadNow");
                if (dllCanUnloadNowPtr != IntPtr.Zero)
                {
                    var dllCanUnloadNow = (DllCanUnloadNow)Marshal.GetDelegateForFunctionPointer(dllCanUnloadNowPtr, typeof(DllCanUnloadNow));
                    if (dllCanUnloadNow() != Constants.S_OK)
                        continue;
                }
                pair.Value.Key.Close();
            }
            Cache_V1.Clear();
        }

        public static IFilter LoadIFilterFromIPersistFile(string path)
        {
            return LoadIFilterFromIPersistFile(path, Path.GetExtension(path));
        }


        public static IFilter LoadIFilterFromIPersistFile(string path, string extension)
        {

            IFilter filter = LoadIFilter(extension);
            if (null == filter)
                return null;
            var persistFile = (filter as IPersistFile);
            if (null == persistFile)
                throw new Exception("IPersistFile is not implemented by the current interface");
            persistFile.Load(path, 0);
            return InitIFilter(filter);
        }

        public static IFilter LoadIFilterFromStream(byte[] bytes, string extension)
        {
            IFilter filter = LoadIFilter(extension);
            if (filter == null)
                return null;
            if (filter is IPersistStream)
            {
                var iPersistStream = (filter as IPersistStream);
                IStream iStream;
                //var ptr = Marshal.AllocHGlobal(bytes.Length);   //if you need to allocate and fill memory by yourself, be sure to use native api as GlobalAlloc+GlobalLock+Marshal.Copy+GlobalUnlock
                CreateStreamOnHGlobal(IntPtr.Zero, true, out iStream);
                iStream.Write(bytes, bytes.Length, IntPtr.Zero);
                iStream.Commit(0); //STGC_DEFAULT
                iPersistStream.Load(iStream);
            }
            else if (filter is IPersistStorage)
            {
                ILockBytes lockBytes = null;
                IntPtr ptr = IntPtr.Zero;
                try
                {
                    var iPersistStorage = filter as IPersistStorage;
                    lockBytes = CreateILockBytesOnHGlobal(new HandleRef(), true);
                    ptr = Marshal.AllocCoTaskMem(bytes.Length);
                    Marshal.Copy(bytes, 0, ptr, bytes.Length);
                    lockBytes.SetSize(bytes.Length);
                    lockBytes.WriteAt(0, ptr, bytes.Length, null);
                    lockBytes.Flush();
                    var storage = StgOpenStorageOnILockBytes(lockBytes, null, (int)(grfMode.STGM_SHARE_EXCLUSIVE | grfMode.STGM_READ), 0, 0);
                    iPersistStorage.Load(storage);
                    Marshal.ReleaseComObject(storage);
                }
                finally
                {
                    if (null != lockBytes)
                        Marshal.FinalReleaseComObject(lockBytes);
                    if (ptr != IntPtr.Zero)
                        Marshal.FreeCoTaskMem(ptr);
                }
            }
            else
            {
                var persistFile = (filter as IPersistFile);
                if (persistFile == null)
                    throw new Exception("Unknown IFilter@@");
                var path = Path.GetTempFileName();
                File.WriteAllBytes(path, bytes);
                ((MixedIFilterClass)filter).TmpFilePath = path;
                persistFile.Load(path, 0);
            }
            return InitIFilter(filter);
        }

        /// <summary>
        /// Firstly, this method try to load OLE stream from given binary array. If failed, then default filter or Null filter will be last choice.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="safeText">if true, select default filter.</param>
        /// <returns></returns>
        public static IFilter LoadIFilterFromStream(byte[] bytes, bool safeText)
        {
            ILockBytes lockBytes = null;
            IntPtr ptr = IntPtr.Zero;
            try
            {
                IFilter filter;
                lockBytes = CreateILockBytesOnHGlobal(new HandleRef(), true);
                ptr = Marshal.AllocCoTaskMem(bytes.Length);
                Marshal.Copy(bytes, 0, ptr, bytes.Length);
                lockBytes.SetSize(bytes.Length);
                lockBytes.WriteAt(0, ptr, bytes.Length, null);
                lockBytes.Flush();
                var storage = StgOpenStorageOnILockBytes(lockBytes, null, (int)(grfMode.STGM_SHARE_EXCLUSIVE | grfMode.STGM_READ), 0, 0);
                BindIFilterFromStorage(storage, IntPtr.Zero, out filter);
                Marshal.ReleaseComObject(storage);
                return InitIFilter(filter);
            }
            catch
            {
                return safeText ? LoadIFilterFromStream(bytes, "*") : LoadIFilterFromStream(bytes, null);  // here we map two symbol : * to default filter , Null value to null filter (ref to GetPersistentHandlerClass method)
            }
            finally
            {
                if (null != lockBytes)
                    Marshal.FinalReleaseComObject(lockBytes);
                if(ptr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(ptr);
            }
        }



        private static IFilter InitIFilter(IFilter filter)
        {
            IFILTER_FLAGS flags;
            var iflags =
                        IFILTER_INIT.IFILTER_INIT_CANON_HYPHENS |
                        IFILTER_INIT.IFILTER_INIT_CANON_PARAGRAPHS |
                        IFILTER_INIT.IFILTER_INIT_CANON_SPACES |
                        IFILTER_INIT.IFILTER_INIT_APPLY_INDEX_ATTRIBUTES |
                        IFILTER_INIT.IFILTER_INIT_HARD_LINE_BREAKS |
                        IFILTER_INIT.IFILTER_INIT_FILTER_OWNED_VALUE_OK;
            var returnCodes = filter.Init(iflags, 0, null, out flags);
            if (returnCodes == Constants.S_OK)
                return filter;
            Marshal.ReleaseComObject(filter);
            throw new Exception(string.Format("Init failed, returnCodes : {0}", returnCodes));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static IClassFactory GetClassFactory(string ext, string dllName, string filterPersistClass)
        {
            if (Cache_V1.ContainsKey(ext) && Cache_V1[ext].Value != IntPtr.Zero)
                return Marshal.GetObjectForIUnknown(Cache_V1[ext].Value) as IClassFactory;
            var classFactory = Marshal.GetObjectForIUnknown(GetClassFactoryFromDll(ext, dllName, filterPersistClass)) as IClassFactory ;
            return classFactory;
        }

        private static IntPtr GetClassFactoryFromDll(string ext, string dllName, string filterPersistClass)
        {
            var handle = LoadLibrary(dllName);
            if(handle.IsInvalid)
                return IntPtr.Zero;
            IntPtr dllGetClassObjectPtr = GetProcAddress(handle.DangerousGetHandle(), "DllGetClassObject");
            if (dllGetClassObjectPtr == IntPtr.Zero)
                return IntPtr.Zero;

            var dllGetClassObject = (DllGetClassObject)Marshal.GetDelegateForFunctionPointer(dllGetClassObjectPtr, typeof(DllGetClassObject));
            var filterPersistGUID = new Guid(filterPersistClass);
            var IClassFactoryGUID = new Guid(Constants.IClassFactoryGUID); //IClassFactory class id
            IntPtr unk;
            if (dllGetClassObject(ref filterPersistGUID, ref IClassFactoryGUID, out unk) != 0)
                return IntPtr.Zero;
            Cache_V1[ext] = new KeyValuePair<LibHandle, IntPtr>(handle, unk);
            return unk;
        }

        private static IFilter LoadIFilter(string ext)
        {
            string dllName, filterPersistClass;
            if (GetFilterDllAndClass(ext, out dllName, out filterPersistClass))
            {
                return LoadFilterFromDll(ext, dllName, filterPersistClass);
            }
            return null;
        }
        
        private static IFilter LoadFilterFromDll(string ext, string dllName, string filterPersistClass)
        {
            Type T;
            object obj;
            Cache_V2.TryGetValue(ext, out T);
            if (null == T)
                T = Type.GetTypeFromCLSID(new Guid(filterPersistClass));
            if(null != T)
            {
                Cache_V2[ext] = T;
                //obj = T.InvokeMember("_ctor", BindingFlags.CreateInstance, null, null, null); 
                obj = Activator.CreateInstance(T);
            }
            else
            {
                IClassFactory classFactory = GetClassFactory(ext, dllName, filterPersistClass);
                if (null == classFactory)
                    return null;
                var IFilterGUID = new Guid(Constants.IFilterGUID);
                classFactory.CreateInstance(null, ref IFilterGUID, out obj);
            }
            //var mixedFilter = Marshal.GetTypedObjectForIUnknown(ptr, typeof(MixedIFilterClass)) as MixedIFilterClass;
            var mixedFilter = (MixedIFilterClass)Marshal.CreateWrapperOfType(obj, typeof(MixedIFilterClass));
            mixedFilter.InternalObj = obj;
            return (IFilter)mixedFilter;

        }

        private static bool GetFilterDllAndClass(string ext, out string dllName, out string filterPersistClass)
        {
            dllName = filterPersistClass = null;
            var persistentHandlerClass = GetPersistentHandlerClass(ext, true);
            if (persistentHandlerClass != null)
            {
                return GetFilterDllAndClassFromPersistentHandler(persistentHandlerClass,out dllName, out filterPersistClass);
            }
            return false;
        }



        private static bool GetFilterDllAndClassFromPersistentHandler(string persistentHandlerClass, out string dllName, out string filterPersistClass)
        {
            dllName = null;
            filterPersistClass = ReadStrFromHKLM(string.Concat(@"Software\Classes\CLSID\", persistentHandlerClass, @"\PersistentAddinsRegistered\{89BCB740-6119-101A-BCB7-00DD010655AF}"));
            if (String.IsNullOrEmpty(filterPersistClass))
                return false;
            dllName = ReadStrFromHKLM(string.Concat(@"Software\Classes\CLSID\", filterPersistClass, @"\InprocServer32"));
            return (!String.IsNullOrEmpty(dllName));
        }

        private static string GetPersistentHandlerClass(string ext, bool searchContentType)
        {
            if (ext == "*")
                return Constants.PH_IDefaultFilter;
            if (ext == null)
                return Constants.PH_INullFilter;

            //Try getting the info from the file extension
            var persistentHandlerClass = GetPersistentHandlerClassFromExtension(ext);
            if (String.IsNullOrEmpty(persistentHandlerClass))
                //try getting the info from the document type 
                persistentHandlerClass = GetPersistentHandlerClassFromDocumentType(ext);
            if (searchContentType && String.IsNullOrEmpty(persistentHandlerClass))
                //Try getting the info from the Content Type
                persistentHandlerClass = GetPersistentHandlerClassFromContentType(ext);
            return persistentHandlerClass;
        }

        private static string GetPersistentHandlerClassFromContentType(string ext)
        {
            var contentType = ReadStrFromHKLM(string.Concat(@"Software\Classes\", ext), "Content Type");
            if (String.IsNullOrEmpty(contentType))
                return null;

            string contentTypeExtension = ReadStrFromHKLM(string.Concat(@"Software\Classes\MIME\Database\Content Type\", contentType), "Extension");
            if (ext.Equals(contentTypeExtension, StringComparison.CurrentCultureIgnoreCase))
                return null; //No need to look further. This extension does not have any persistent handler

            //We know the extension that is assciated with that content type. Simply try again with the new extension
            return GetPersistentHandlerClass(contentTypeExtension, false); //Don't search content type this time.
        }

        private static string GetPersistentHandlerClassFromDocumentType(string ext)
        {
            //Get the DocumentType of this file extension
            string docType = ReadStrFromHKLM(string.Concat(@"Software\Classes\",ext));
            if (String.IsNullOrEmpty(docType))
                return null;

            //Get the Class ID for this document type
            string docClass = ReadStrFromHKLM(string.Concat(@"Software\Classes\", docType, @"\CLSID"));
            if (String.IsNullOrEmpty(docType))
                return null;

            //Now get the PersistentHandler for that Class ID
            return ReadStrFromHKLM(string.Concat(@"Software\Classes\CLSID\", docClass, @"\PersistentHandler"));
        }

        private static string GetPersistentHandlerClassFromExtension(string ext)
        {
            return ReadStrFromHKLM(string.Concat(@"Software\Classes\", ext, @"\PersistentHandler"));
        }


        #region Registry Read String helper
        static string ReadStrFromHKLM(string key)
        {
            return ReadStrFromHKLM(key, null);
        }
        static string ReadStrFromHKLM(string key, string value)
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey(key);
            if (null == rk)
                rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(key);
            if (null == rk)
                return null;
            using (rk)
            {
                return (string)rk.GetValue(value);
            }
        }
        #endregion
    }
}
