using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SenseNet.ContentRepository.Storage;
using  SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Versioning;

namespace SenseNet.ContentRepository
{
	[ContentHandler]
	public class File : FileBase
	{
        [Obsolete("Use typeof(File).Name instead.", true)]
        public static readonly string NodeTypeName = typeof(File).Name;

		//================================================================================= Construction

        public File(Node parent) : this(parent, null) { }
		public File(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected File(NodeToken nt) : base(nt) { }

        //================================================================================= Properties

        public override CheckInCommentsMode CheckInCommentsMode
        {
            get
            {
                return this.VersioningMode == VersioningType.None ? CheckInCommentsMode.None : Repository.CheckInCommentsMode;
            }
        }

        //================================================================================= Methods

        public static void Downloaded(int fileId)
        {
            DownloadCounter.Increment(fileId);
        }

        public static void Downloaded(string filePath)
        {
            DownloadCounter.Increment(filePath);
        }

        public static File CreateByBinary(IFolder parent, BinaryData binaryData)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            if (binaryData == null)
                return new File(parent as Node);

            File file;
            // Resolve filetype by binary-config matching
            BinaryTypeResolver resolver = new BinaryTypeResolver();
            if (!resolver.ParseBinary(binaryData))
            {
                // Unknown file type
                file = new File(parent as Node);
            }
            else
            {
                // Specific File subtype has been found
                file = TypeHandler.CreateInstance<File>(resolver.NodeType.ClassName, parent);

                var fname = binaryData.FileName.FileNameWithoutExtension;
                if (string.IsNullOrEmpty(fname))
                    fname = file.Name;
                else if (fname.Contains("\\"))
                    fname = System.IO.Path.GetFileNameWithoutExtension(fname);

                binaryData.FileName = new BinaryFileName(fname, resolver.FileNameExtension);
                binaryData.ContentType = resolver.ContentType;
            }

            file.Binary = binaryData;
            return file;
        }

        public override string Icon
        {
            // hack, this is an ugly workaround before we implement this into the mime system
            get
            {
                var formats = new Dictionary<string, string>
              {
                {"\\.(doc(x)?|rtf)$", "word"},
                {"\\.(xls(x)?|csv)$", "excel"},
                {"\\.ppt(x)?$", "powerpoint"},
                {"\\.pdf$", "acrobat"},
                {"\\.txt$", "document"},
                {"\\.(jp(e)?g|gif|bmp|png|tif(f)?|psd|ai|cdr)$", "image"}
              };

                foreach (KeyValuePair<string, string> f in formats)
                {
                    var r = new Regex(f.Key, RegexOptions.IgnoreCase);
                    if (r.IsMatch(Name)) 
                        return f.Value;
                }

                return base.Icon;
            }
        }
    }
}