using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Schema;

namespace SenseNet.ContentRepository.Tests.ContentHandlers
{
    [ContentHandler]
	public class DefaultValueTest : GenericContent
	{
        public DefaultValueTest(Node parent) : this(parent, typeof(DefaultValueTest).Name) { }
		public DefaultValueTest(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected DefaultValueTest(NodeToken nt) : base(nt) { }

		[RepositoryProperty("ShortText")]
		public string ShortText
		{
			get { return (string)this["ShortText"]; }
			set { this["ShortText"] = value; }
		}
		[RepositoryProperty("LongText", RepositoryDataType.Text)]
		public string LongText
		{
			get { return (string)this["LongText"]; }
			set { this["LongText"] = value; }
		}
		[RepositoryProperty("Boolean", RepositoryDataType.Int)]
		public bool Boolean
		{
			get { return (int)this["Boolean"] != 0; }
			set { this["Boolean"] = value ? 1 : 0; }
		}

		[RepositoryProperty("Byte", RepositoryDataType.Int)]
		public Byte Byte
		{
			get { return Convert.ToByte(this["Byte"]); }
			set { this["Byte"] = Convert.ToInt32(value); }
		}
		[RepositoryProperty("Int16", RepositoryDataType.Int)]
		public Int16 Int16
		{
			get { return Convert.ToInt16(this["Int16"]); }
			set { this["Int16"] = Convert.ToInt32(value); }
		}
		[RepositoryProperty("Int32", RepositoryDataType.Int)]
		public Int32 Int32
		{
			get { return (int)this["Int32"]; }
			set { this["Int32"] = value; }
		}
		[RepositoryProperty("Int64", RepositoryDataType.Currency)]
		public Int64 Int64
		{
			get { return Convert.ToInt64(this["Int64"]); }
			set { this["Int64"] = Convert.ToDecimal(value); }
		}
		[RepositoryProperty("Single", RepositoryDataType.Currency)]
		public Single Single
		{
			get { return Convert.ToSingle(this["Single"]); }
			set { this["Single"] = Convert.ToDecimal(value); }
		}
		[RepositoryProperty("Double", RepositoryDataType.Currency)]
		public Double Double
		{
			get { return Convert.ToDouble(this["Double"]); }
			set { this["Double"] = Convert.ToDecimal(value); }
		}
		[RepositoryProperty("Decimal", RepositoryDataType.Currency)]
		public Decimal Decimal
		{
			get { return Convert.ToDecimal(this["Decimal"]); }
			set { this["Decimal"] = Convert.ToDecimal(value); }
		}
		[RepositoryProperty("Number", RepositoryDataType.Currency)]
		public Decimal Number
		{
			get { return Convert.ToDecimal(this["Number"]); }
			set { this["Number"] = Convert.ToDecimal(value); }
		}
		[RepositoryProperty("SByte", RepositoryDataType.Int)]
		public SByte SByte
		{
			get { return Convert.ToSByte(this["SByte"]); }
			set { this["SByte"] = Convert.ToInt32(value); }
		}
		[RepositoryProperty("UInt16", RepositoryDataType.Int)]
		public UInt16 UInt16
		{
			get { return Convert.ToUInt16(this["UInt16"]); }
			set { this["UInt16"] = Convert.ToUInt32(value); }
		}
		[RepositoryProperty("UInt32", RepositoryDataType.Currency)]
		public UInt32 UInt32
		{
			get { return Convert.ToUInt32(this["UInt32"]); }
			set { this["UInt32"] = Convert.ToUInt32(value); }
		}
		[RepositoryProperty("UInt64", RepositoryDataType.Currency)]
		public UInt64 UInt64
		{
			get { return Convert.ToUInt64(this["UInt64"]); }
			set { this["UInt64"] = Convert.ToDecimal(value); }
		}

		[RepositoryProperty("UserReference", RepositoryDataType.Reference)]
		public User UserReference
		{
            get { return this.GetReference<User>("UserReference"); }
            set { this.SetReference("UserReference", value); }
		}
		[RepositoryProperty("UsersReference", RepositoryDataType.Reference)]
		public IEnumerable<User> UsersReference
		{
            get { return this.GetReferences("UsersReference").Cast<User>(); }
            set { this.SetReferences<User>("UsersReference", value); }
		}
		[RepositoryProperty("GeneralReference", RepositoryDataType.Reference)]
		public IEnumerable<Node> GeneralReference
		{
            get { return this.GetReferences("GeneralReference"); }
            set { this.SetReferences("GeneralReference", value); }
		}

		[RepositoryProperty("HyperLink")]
		public string HyperLink
		{
			get { return (string)this["HyperLink"]; }
			set { this["HyperLink"] = value; }
		}
		[RepositoryProperty("VersionNumber", RepositoryDataType.String)]
		public VersionNumber VersionNumber
		{
			get
			{
				string value = (string)this["VersionNumber"];
				if (String.IsNullOrEmpty(value))
					return null;
				return VersionNumber.Parse(value);
			}
			set { this["VersionNumber"] = value == null ? null : value.ToString(); }
		}

        [RepositoryProperty("DateTime", RepositoryDataType.DateTime)]
        public DateTime DateTime
		{
			get { return Convert.ToDateTime(this["DateTime"]); }
			set { this["DateTime"] = value; }
		}

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case "ShortText": return this.ShortText;
                case "LongText": return this.LongText;
                case "Boolean": return this.Boolean;
                case "Byte": return this.Byte;
                case "Int16": return this.Int16;
                case "Int32": return this.Int32;
                case "Int64": return this.Int64;
                case "Single": return this.Single;
                case "Double": return this.Double;
                case "Decimal": return this.Decimal;
                case "Number": return this.Number;
                case "SByte": return this.SByte;
                case "UInt16": return this.UInt16;
                case "UInt32": return this.UInt32;
                case "UInt64": return this.UInt64;
                case "UserReference": return this.UserReference;
                case "UsersReference": return this.UsersReference;
                case "GeneralReference": return this.GeneralReference;
                case "HyperLink": return this.HyperLink;
                case "VersionNumber": return this.VersionNumber;
                case "DateTime": return this.DateTime;
                default: return base.GetProperty(name);
            }
        }
        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case "ShortText": this.ShortText = (string)value; break;
                case "LongText": this.LongText = (string)value; break;
                case "Boolean": this.Boolean = (bool)value; break;
                case "Byte": this.Byte = (byte)value; break;
                case "Int16": this.Int16 = (Int16)value; break;
                case "Int32": this.Int32 = (Int32)value; break;
                case "Int64": this.Int64 = (Int64)value; break;
                case "Single": this.Single = (Single)value; break;
                case "Double": this.Double = (double)value; break;
                case "Decimal": this.Decimal = (decimal)value; break;
                case "Number": this.Number = (decimal)value; break;
                case "SByte": this.SByte = (sbyte)value; break;
                case "UInt16": this.UInt16 = (UInt16)value; break;
                case "UInt32": this.UInt32 = (UInt32)value; break;
                case "UInt64": this.UInt64 = (UInt64)value; break;
                case "UserReference": this.UserReference = (User)value; break;
                case "UsersReference": this.UsersReference = (IEnumerable<User>)value; break;
                case "GeneralReference": this.GeneralReference = (IEnumerable<Node>)value; break;
                case "HyperLink": this.HyperLink = (string)value; break;
                case "VersionNumber": this.VersionNumber = (VersionNumber)value; break;
                case "DateTime": this.DateTime = (DateTime)value; break;
                default: base.SetProperty(name, value); break;
            }
        }
    }
}
