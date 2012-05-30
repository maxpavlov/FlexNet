using System;

namespace Eclipse.IndexingService.COMTypeDef
{
    public enum CHUNK_BREAKTYPE
    {
        CHUNK_NO_BREAK,
        CHUNK_EOW,
        CHUNK_EOS,
        CHUNK_EOP,
        CHUNK_EOC
    }

    [Flags]
    public enum CHUNKSTATE
    {
        CHUNK_FILTER_OWNED_VALUE = 4,
        CHUNK_TEXT = 1,
        CHUNK_VALUE = 2
    }


    public enum IFilterReturnCodes : uint
    {
        /// <summary>
        /// Success
        /// </summary>
        S_OK = 0,
        /// <summary>
        /// The function was denied access to the filter file. 
        /// </summary>
        E_ACCESSDENIED = 0x80070005,
        /// <summary>
        /// The function encountered an invalid handle,
        /// probably due to a low-memory situation. 
        /// </summary>
        E_HANDLE = 0x80070006,
        /// <summary>
        /// The function received an invalid parameter.
        /// </summary>
        E_INVALIDARG = 0x80070057,
        /// <summary>
        /// Out of memory
        /// </summary>
        E_OUTOFMEMORY = 0x8007000E,
        /// <summary>
        /// Not implemented
        /// </summary>
        E_NOTIMPL = 0x80004001,
        /// <summary>
        /// Unknown error
        /// </summary>
        E_FAIL = 0x80000008,
        /// <summary>
        /// File not filtered due to password protection
        /// </summary>
        FILTER_E_PASSWORD = 0x8004170B,
        /// <summary>
        /// The document format is not recognised by the filter
        /// </summary>
        FILTER_E_UNKNOWNFORMAT = 0x8004170C,
        /// <summary>
        /// No text in current chunk
        /// </summary>
        FILTER_E_NO_TEXT = 0x80041705,
        /// <summary>
        /// No values in current chunk
        /// </summary>
        FILTER_E_NO_VALUES = 0x80041706,
        /// <summary>
        /// No more chunks of text available in object
        /// </summary>
        FILTER_E_END_OF_CHUNKS = 0x80041700,
        /// <summary>
        /// No more text available in chunk
        /// </summary>
        FILTER_E_NO_MORE_TEXT = 0x80041701,
        /// <summary>
        /// No more property values available in chunk
        /// </summary>
        FILTER_E_NO_MORE_VALUES = 0x80041702,
        /// <summary>
        /// Unable to access object
        /// </summary>
        FILTER_E_ACCESS = 0x80041703,
        /// <summary>
        /// Moniker doesn't cover entire region
        /// </summary>
        FILTER_W_MONIKER_CLIPPED = 0x00041704,
        /// <summary>
        /// Unable to bind IFilter for embedded object
        /// </summary>
        FILTER_E_EMBEDDING_UNAVAILABLE = 0x80041707,
        /// <summary>
        /// Unable to bind IFilter for linked object
        /// </summary>
        FILTER_E_LINK_UNAVAILABLE = 0x80041708,
        /// <summary>
        ///  This is the last text in the current chunk
        /// </summary>
        FILTER_S_LAST_TEXT = 0x00041709,
        /// <summary>
        /// This is the last value in the current chunk
        /// </summary>
        FILTER_S_LAST_VALUES = 0x0004170A,

        /// <summary>
        /// The data area passed to a system call is too small
        /// </summary>
        ERROR_INSUFFICIENT_BUFFER = 0x8007007A

    }

    [Flags]
    public enum IFILTER_FLAGS
    {
        IFILTER_FLAGS_NONE,
        IFILTER_FLAGS_OLE_PROPERTIES
    }

    [Flags]
    public enum IFILTER_INIT
    {
        IFILTER_INIT_APPLY_CRAWL_ATTRIBUTES = 0x100,
        IFILTER_INIT_APPLY_INDEX_ATTRIBUTES = 0x10,
        IFILTER_INIT_APPLY_OTHER_ATTRIBUTES = 0x20,
        IFILTER_INIT_CANON_HYPHENS = 4,
        IFILTER_INIT_CANON_PARAGRAPHS = 1,
        IFILTER_INIT_CANON_SPACES = 8,
        IFILTER_INIT_FILTER_OWNED_VALUE_OK = 0x200,
        IFILTER_INIT_HARD_LINE_BREAKS = 2,
        IFILTER_INIT_INDEXING_ONLY = 0x40,
        IFILTER_INIT_SEARCH_LINKS = 0x80
    }

    public enum PID_STG
    {
        ACCESSTIME = 0x10,
        ALLOCSIZE = 0x12,
        ATTRIBUTES = 13,
        CHANGETIME = 0x11,
        CLASSID = 3,
        CONTENTS = 0x13,
        CREATETIME = 15,
        DIRECTORY = 2,
        FILEINDEX = 8,
        LASTCHANGEUSN = 9,
        NAME = 10,
        PARENT_WORKID = 6,
        PATH = 11,
        SECONDARYSTORE = 7,
        SHORTNAME = 20,
        SIZE = 12,
        STORAGETYPE = 4,
        VOLUME_ID = 5,
        WRITETIME = 14
    }

    [Flags]
    public enum STGM_FLAGS
    {
        ACCESS = 3,
        CREATE = 0x1000,
        MODE = 0x1000,
        READ = 0,
        READWRITE = 2,
        SHARE_DENY_NONE = 0x40,
        SHARE_DENY_READ = 0x30,
        SHARE_DENY_WRITE = 0x20,
        SHARE_EXCLUSIVE = 0x10,
        SHARING = 0x70,
        WRITE = 1
    }

    public enum VARTYPE : short
    {
        VT_BSTR = 8,
        VT_FILETIME = 0x40,
        VT_LPSTR = 30
    }

    /* Storage instantiation modes */
    [Flags]
    public enum grfMode : uint
    {
        STGM_DIRECT = 0x00000000,
        STGM_TRANSACTED = 0x00010000,
        STGM_SIMPLE = 0x08000000,

        STGM_READ = 0x00000000,
        STGM_WRITE = 0x00000001,
        STGM_READWRITE = 0x00000002,

        STGM_SHARE_DENY_NONE = 0x00000040,
        STGM_SHARE_DENY_READ = 0x00000030,
        STGM_SHARE_DENY_WRITE = 0x00000020,
        STGM_SHARE_EXCLUSIVE = 0x00000010,

        STGM_PRIORITY = 0x00040000,
        STGM_DELETEONRELEASE = 0x04000000,
        STGM_NOSCRATCH = 0x00100000,

        STGM_CREATE = 0x00001000,
        STGM_CONVERT = 0x00020000,
        STGM_FAILIFTHERE = 0x00000000,

        STGM_NOSNAPSHOT = 0x00200000,
        STGM_DIRECT_SWMR = 0x00400000
    }

    public enum tagSTGTY
    {
        STGTY_STORAGE = 1,
        STGTY_STREAM = 2,
        STGTY_LOCKBYTES = 3,
        STGTY_PROPERTY = 4
    }

    public enum ulKind : uint
    {
        PRSPEC_LPWSTR = 0,
        PRSPEC_PROPID = 1
    }

    public enum SumInfoProperty : uint
    {
        PIDSI_TITLE	        = 0x00000002,
        PIDSI_SUBJECT	    = 0x00000003,
        PIDSI_AUTHOR	    = 0x00000004,
        PIDSI_KEYWORDS	    = 0x00000005,
        PIDSI_COMMENTS	    = 0x00000006,
        PIDSI_TEMPLATE	    = 0x00000007,
        PIDSI_LASTAUTHOR	= 0x00000008,
        PIDSI_REVNUMBER	    = 0x00000009,
        PIDSI_EDITTIME	    = 0x0000000A,
        PIDSI_LASTPRINTED	= 0x0000000B,
        PIDSI_CREATE_DTM	= 0x0000000C,
        PIDSI_LASTSAVE_DTM	= 0x0000000D,
        PIDSI_PAGECOUNT     = 0x0000000E,
        PIDSI_WORDCOUNT     = 0x0000000F,
        PIDSI_CHARCOUNT     = 0x00000010,
        PIDSI_THUMBNAIL     = 0x00000011,
        PIDSI_APPNAME       = 0x00000012,
        PIDSI_SECURITY      = 0x00000013
    }

    public enum DocSumInfoProperty : uint
    {
        PIDDSI_NOTECOUNT = 0x00000008,
        PIDDSI_HIDDENCOUNT = 0x00000009,
        PIDDSI_MMCLIPCOUNT = 0x0000000A,
        PIDDSI_SCALE = 0x0000000B,
        PIDDSI_HEADINGPAIR = 0x0000000C,
        PIDDSI_DOCPARTS = 0x0000000D,
        PIDDSI_MANAGER = 0x0000000E,
        PIDDSI_COMPANY = 0x0000000F,
        PIDDSI_LINKSDIRTY = 0x00000010
    }


    /// <summary>
    /// for PSGUID_STORAGE (defined in stgprop.h)
    /// </summary>
    public enum STGPROP : uint
    {
        PID_STG_DIRECTORY       = 0x00000002,
        PID_STG_CLASSID         = 0x00000003,
        PID_STG_STORAGETYPE     = 0x00000004,
        PID_STG_VOLUME_ID       = 0x00000005,
        PID_STG_PARENT_WORKID   = 0x00000006,
        PID_STG_SECONDARYSTORE  = 0x00000007,
        PID_STG_FILEINDEX       = 0x00000008,
        PID_STG_LASTCHANGEUSN   = 0x00000009,
        PID_STG_NAME            = 0x0000000a,
        PID_STG_PATH            = 0x0000000b,
        PID_STG_SIZE            = 0x0000000c,
        PID_STG_ATTRIBUTES      = 0x0000000d,
        PID_STG_WRITETIME       = 0x0000000e,
        PID_STG_CREATETIME      = 0x0000000f,
        PID_STG_ACCESSTIME      = 0x00000010,
        PID_STG_CHANGETIME      = 0x00000011,
        PID_STG_ALLOCSIZE       = 0x00000012,
        PID_STG_CONTENTS        = 0x00000013,
        PID_STG_SHORTNAME       = 0x00000014,
        PID_STG_FRN             = 0x00000015,
        PID_STG_SCOPE           = 0x00000016
    }

    [Flags]
    public enum PROPSETFLAG : uint 
    {
        PROPSETFLAG_DEFAULT = 0,
        PROPSETFLAG_NONSIMPLE = 1,
        PROPSETFLAG_ANSI = 2,
        PROPSETFLAG_UNBUFFERED = 4,
        PROPSETFLAG_CASE_SENSITIVE = 8
        
    }

    public enum CLIPFORMAT: ushort
    {
        CF_TEXT = 1,
        CF_BITMAP = 2,
        CF_METAFILEPICT = 3,
        CF_SYLK = 4,
        CF_DIF = 5,
        CF_TIFF = 6,
        CF_OEMTEXT = 7,
        CF_DIB = 8,
        CF_PALETTE = 9,
        CF_PENDATA = 10,
        CF_RIFF = 11,
        CF_WAVE = 12,
        CF_UNICODETEXT = 13,
        CF_ENHMETAFILE = 14,
        CF_HDROP = 15,
        CF_LOCALE = 16,
        CF_MAX = 17,
        CF_OWNERDISPLAY = 0x80,
        CF_DSPTEXT = 0x81,
        CF_DSPBITMAP = 0x82,
        CF_DSPMETAFILEPICT = 0x83,
        CF_DSPENHMETAFILE = 0x8E,
    }

}
