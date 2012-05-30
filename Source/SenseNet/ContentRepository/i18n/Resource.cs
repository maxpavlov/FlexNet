using System;
using System.Collections.Generic;
using System.Text;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.i18n;

namespace SenseNet.ContentRepository.i18n
{
    /// <summary>
    /// Stores the necessary resources. 
    /// </summary>
    [ContentHandler]
    public class Resource : File
    {
        [Obsolete("Use typeof(Resource).Name instead.", true)]
        public static readonly string NodeTypeName = typeof(Resource).Name;

		//================================================================================= Construction

        public Resource(Node parent) : this(parent, null) { }
		public Resource(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected Resource(NodeToken nt) : base(nt) { }

        //================================================================================= IFile Members

        //public static Resource CreateByBinary(IFolder parent, BinaryData binaryData)
        //{
        //    if (parent == null)
        //        throw new ArgumentNullException("parent");

        //    if (binaryData == null)
        //        return new Resource(parent as Node);

        //    Resource resource;
        //    // Resolve filetype by binary-config matching
        //    BinaryTypeResolver resolver = new BinaryTypeResolver();
        //    if (!resolver.ParseBinary(binaryData))
        //    {
        //        // Unknown file type
        //        resource = new Resource(parent as Node);
        //    }
        //    else
        //    {
        //        resource = TypeHandler.CreateInstance<Resource>(resolver.NodeType.ClassName, parent);

        //        binaryData.FileName = new BinaryFileName(resource.Name, resolver.FileNameExtension);
        //        binaryData.ContentType = resolver.ContentType;
        //    }

        //    resource.Binary = binaryData;
        //    return resource;
        //}

        //#region IFile Members


        ////================================================================================= IFile Members

        //[RepositoryProperty("Binary", RepositoryDataType.Binary)]
        //public virtual BinaryData Binary
        //{
        //    get { return this.GetBinary("Binary"); }
        //    set { this.SetBinary("Binary", value); }
        //}
        //public int Downloads
        //{
        //    //TODO: Download counter is a statistical data rather than a node specific one
        //    get { return 0; }
        //}
        //public long Size
        //{
        //    get { return this.GetBinary("Binary").Size; }
        //}
        //public long FullSize
        //{
        //    get { return this.GetBinary("Binary").Size; }
        //}
        //public void IncreaseDownloads()
        //{
        //    //TODO: Download counter is a statistical data rather than a node specific one
        //}
        //public void RestoreVersion(VersionNumber versionNumber)
        //{
        //    throw new Exception("The method or operation is not implemented.");
        //}
        //public event EventHandler FileDownloaded;

        //#endregion

        //================================================================================= Generic Property handling

        public override object GetProperty(string name)
        {
            switch (name)
            {
                //case "Binary":
                //    return this.Binary;
                //case "Downloads":
                //    return this.Downloads;
                //case "Size":
                //    return this.Size;
                //case "FullSize":
                //    return this.FullSize;
                default:
                    return base.GetProperty(name);
            }
        }
        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                //case "Binary":
                //    this.Binary = (BinaryData)value;
                //    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }

        public override void Save()
        {
            base.Save();

            SenseNetResourceManager.Reset();
        }

        public override void Save(SavingMode mode)
        {
            base.Save(mode);

            SenseNetResourceManager.Reset();
        }
    }
}