using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace Eclipse.IndexingService.COMTypeDef
{
    [ComImport, Guid(Constants.IFilterGUID), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity, ComVisible(true), AutomationProxy(false)]
    public interface IFilter
    {
        /// <summary>
        /// The IFilter::Init method initializes a filtering session.
        /// </summary>
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall,
MethodCodeType = MethodCodeType.Runtime)]
        IFilterReturnCodes Init(
            //[in] Flag settings from the IFILTER_INIT enumeration for
            // controlling text standardization, property output, embedding
            // scope, and IFilter access patterns. 
          [MarshalAs(UnmanagedType.U4)]IFILTER_INIT grfFlags,

          // [in] The size of the attributes array. When nonzero, cAttributes
            //  takes 
            // precedence over attributes specified in grfFlags. If no
            // attribute flags 
            // are specified and cAttributes is zero, the default is given by
            // the 
            // PSGUID_STORAGE storage property set, which contains the date and
            //  time 
            // of the last write to the file, size, and so on; and by the
            //  PID_STG_CONTENTS 
            // 'contents' property, which maps to the main contents of the
            // file. 
            // For more information about properties and property sets, see
            // Property Sets.
          uint cAttributes,

          //[in] Array of pointers to FULLPROPSPEC structures for the
            // requested properties. 
            // When cAttributes is nonzero, only the properties in aAttributes
            // are returned.
          [MarshalAs(UnmanagedType.LPArray)]
          FULLPROPSPEC[] aAttributes,
            
          // [out] Information about additional properties available to the
            //  caller; from the IFILTER_FLAGS enumeration. 
          out IFILTER_FLAGS pdwFlags);

        /// <summary>
        /// The IFilter::GetChunk method positions the filter at the beginning
        /// of the next chunk, 
        /// or at the first chunk if this is the first call to the GetChunk
        /// method, and returns a description of the current chunk. 
        /// </summary>
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall,
MethodCodeType = MethodCodeType.Runtime)]
        IFilterReturnCodes GetChunk(out STAT_CHUNK pStat);

        /// <summary>
        /// The IFilter::GetText method retrieves text (text-type properties)
        /// from the current chunk, 
        /// which must have a CHUNKSTATE enumeration value of CHUNK_TEXT.
        /// </summary>
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall,
MethodCodeType = MethodCodeType.Runtime)]
        IFilterReturnCodes GetText(
            // [in/out] On entry, the size of awcBuffer array in wide/Unicode
            // characters. On exit, the number of Unicode characters written to
            // awcBuffer. 
            // Note that this value is not the number of bytes in the buffer. 
            ref uint pcwcBuffer,

            // Text retrieved from the current chunk. Do not terminate the
            // buffer with a character.  
            [Out]IntPtr awcBuffer);

        /// <summary>
        /// The IFilter::GetValue method retrieves a value (public
        /// value-type property) from a chunk, 
        /// which must have a CHUNKSTATE enumeration value of CHUNK_VALUE.
        /// </summary>
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall,
MethodCodeType = MethodCodeType.Runtime)]
        IFilterReturnCodes GetValue(
            // Allocate the PROPVARIANT structure with CoTaskMemAlloc. Some
            // PROPVARIANT 
            // structures contain pointers, which can be freed by calling the
            // PropVariantClear function. 
            // It is up to the caller of the GetValue method to call the
            //   PropVariantClear method.            
            // ref IntPtr ppPropValue
            // [MarshalAs(UnmanagedType.Struct)]
            out PROPVARIANT PropVal);

        /// <summary>
        /// The IFilter::BindRegion method retrieves an interface representing
        /// the specified portion of the object. 
        /// Currently reserved for future use.
        /// </summary>
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall,
MethodCodeType = MethodCodeType.Runtime)]
        IFilterReturnCodes BindRegion(ref FILTERREGION origPos,ref Guid riid, ref object ppunk);
    }

    [ComImport, Guid(Constants.IFilterGUID)]
    public abstract class IFilterClass
    {
        [ComVisible(false)]
        public abstract string TmpFilePath { get; set; }
        [ComVisible(false)]
        public abstract Object InternalObj{ get; set; }
     }

    public class MixedIFilterClass : IFilterClass, IDisposable

    {

        public override string TmpFilePath
        {
            get;
            set;
        }

        public override Object InternalObj
        { 
            get;
            set;
        }

        //private MixedIFilterClass()
        //{
        //    InternalPtr = Marshal.GetComInterfaceForObject(this, typeof(IFilter));
        //}


        ~MixedIFilterClass()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(null != InternalObj)
            {
                Marshal.ReleaseComObject(InternalObj);
                InternalObj = null;
            }
            if (null != TmpFilePath)
                try
                {
                    File.Delete(TmpFilePath);
                    TmpFilePath = null;
                }
                catch { }
            if (disposing)
                GC.SuppressFinalize(this);
        }


        public void Dispose()
        {
            Dispose(true);
        }
    }



}
