using System;
using SenseNet.ContentRepository.Storage;
using  SenseNet.ContentRepository.Schema;
using System.Collections.Generic;
using System.Linq;

namespace SenseNet.ContentRepository.Tests.ContentHandlers
{
	[ContentHandler]
	public class FieldTestHandler : Node
	{
		protected FieldTestHandler(Node parent) : this(parent, "FieldTestHandler") { }
		public FieldTestHandler(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected FieldTestHandler(NodeToken nt) : base(nt) { }

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

		[RepositoryProperty("Who", RepositoryDataType.Reference)]
		public User Who
		{
			get			{                return this.GetReference<User>("Who");			}
			set			{                this.SetReference("Who", value);			}
		}
		[RepositoryProperty("When")]
		public DateTime When
		{
			get { return (DateTime)this["When"]; }
			set { this["When"] = value; }
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
	}
}
