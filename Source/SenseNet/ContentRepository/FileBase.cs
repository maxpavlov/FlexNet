using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SenseNet.ContentRepository.Storage;
using  SenseNet.ContentRepository.Schema;

namespace SenseNet.ContentRepository
{
	public abstract class FileBase : GenericContent, IFile
	{
        //================================================================================ Construction

        public FileBase(Node parent) : base(parent) { }
		public FileBase(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected FileBase(NodeToken nt) : base(nt) { }

        //================================================================================= IFile Members

		[RepositoryProperty("Binary", RepositoryDataType.Binary)]
		public virtual BinaryData Binary
		{
			get { return this.GetBinary("Binary"); }
			set { this.SetBinary("Binary", value); }
		}
        public long Size
        {
			get { return this.GetBinary("Binary").Size; }
        }
        public long FullSize
        {
            //TODO: Create logic to calculate the sum size of all versions
            get { return -1; }
		}

		//================================================================================= Generic Property handling

		public override object GetProperty(string name)
		{
			switch (name)
			{
				case "Binary":
					return this.Binary;
                case "Size":
                    return this.Size;
                case "FullSize":
                    return this.FullSize;
				default:
					return base.GetProperty(name);
			}
		}
		public override void SetProperty(string name, object value)
		{
			switch (name)
			{
				case "Binary":
					this.Binary = (BinaryData)value;
					break;
				default:
					base.SetProperty(name, value);
					break;
			}
		}
    }
}