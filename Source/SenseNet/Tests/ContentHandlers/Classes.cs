using System;
using  SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.ContentRepository.Tests.ContentHandlers
{
	public class TestClassBase
	{
	}

	[ContentHandler]
	public class BadHandler1 : TestClassBase
	{
		[RepositoryProperty]
		public int TestInt
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
	}

	public class BadHandler2 : Node
	{
		public BadHandler2(Node parent) : this(parent, "BadHandler2") { }
		public BadHandler2(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected BadHandler2(NodeToken nt) : base(nt) { }

        public override bool IsContentType { get { return false; } }

		[RepositoryProperty]
		public int TestInt
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
	}

	[ContentHandler]
	public class TestNode3 : Node
	{
		public TestNode3(Node parent) : this(parent, "TestNode3") { }
		public TestNode3(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected TestNode3(NodeToken nt) : base(nt) { }

        public override bool IsContentType { get { return false; } }

		[RepositoryProperty]
		public int TestInt
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
	}

	[ContentHandler]
	public class TestNode4 : Node
	{
		public TestNode4(Node parent) : this(parent, "TestNode4") { }
		public TestNode4(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected TestNode4(NodeToken nt) : base(nt) { }

        public override bool IsContentType { get { return false; } }

		[RepositoryProperty("TestInt1")]
		public int TestInt
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
	}

	[ContentHandler]
	public class TestNode5 : Node
	{
		public TestNode5(Node parent) : this(parent, "TestNode5") { }
		public TestNode5(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected TestNode5(NodeToken nt) : base(nt) { }

        public override bool IsContentType { get { return false; } }

		[RepositoryProperty(RepositoryDataType.Int)]
		public int TestInt
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
	}

	[ContentHandler]
	public class TestNode6 : Node
	{
		public TestNode6(Node parent) : this(parent, "TestNode6") { }
		public TestNode6(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected TestNode6(NodeToken nt) : base(nt) { }

        public override bool IsContentType { get { return false; } }

		[RepositoryProperty("TestInt1", RepositoryDataType.Int)]
		public int TestInt
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
	}

	public class TestNodeBase : Node
	{
		public TestNodeBase(Node parent) : this(parent, "TestNodeBase") { }
		public TestNodeBase(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected TestNodeBase(NodeToken nt) : base(nt) { }

        public override bool IsContentType { get { return false; } }

		protected virtual int TestInt
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
	}

	[ContentHandler]
	public class TestNode7 : TestNodeBase
	{
		public TestNode7(Node parent) : this(parent, "TestNode7") { }
		public TestNode7(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected TestNode7(NodeToken nt) : base(nt) { }

		[RepositoryProperty]
		protected override int TestInt
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[RepositoryProperty]
		public int TestInt1
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[RepositoryProperty]
		internal int TestInt2
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[RepositoryProperty]
		protected int TestInt3
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[RepositoryProperty]
		protected internal int TestInt4
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[RepositoryProperty]
		private int TestInt5
		{
			set { throw new NotSupportedException(); }
		}
	}

	[ContentHandler]
	public class TestNode8 : Node
	{
		public TestNode8(Node parent) : this(parent, "TestNode8") { }
		public TestNode8(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected TestNode8(NodeToken nt) : base(nt) { }

        public override bool IsContentType { get { return false; } }

		[RepositoryProperty("bool")]       public bool       TestProp01 { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		[RepositoryProperty("byte")]       public byte       TestProp02 { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		[RepositoryProperty("sbyte")]      public sbyte      TestProp03 { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		[RepositoryProperty("int")]        public int        TestProp04 { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		[RepositoryProperty("Int16")]      public Int16      TestProp05 { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		[RepositoryProperty("Int32")]      public Int32      TestProp06 { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		[RepositoryProperty("Int64")]      public Int64      TestProp07 { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		[RepositoryProperty("UInt16")]     public UInt16     TestProp08 { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		[RepositoryProperty("UInt32")]     public UInt32     TestProp09 { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		[RepositoryProperty("UInt64")]     public UInt64     TestProp10 { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		[RepositoryProperty("DateTime")]   public DateTime   TestProp11 { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		[RepositoryProperty("Single")]     public Single     TestProp12 { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		[RepositoryProperty("Double")]     public Double     TestProp13 { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		[RepositoryProperty("Decimal")]    public Decimal    TestProp14 { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		[RepositoryProperty("BinaryData")] public BinaryData TestProp15 { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
	}

	[ContentHandler]
	public class TestNode9 : Node
	{
		public TestNode9(Node parent) : this(parent, "TestNode9") { }
		public TestNode9(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected TestNode9(NodeToken nt) : base(nt) { }

        public override bool IsContentType { get { return false; } }

		[RepositoryProperty("TestProp1")]                     public int    TestProp1 { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		[RepositoryProperty("TestProp2", RepositoryDataType.String)] public int    TestProp2 { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		[RepositoryProperty("TestProp3", RepositoryDataType.Text)]   public string TestProp3 { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
	}

	//==================================================================== Inheritance

	[ContentHandler]
	public class TestNode10 : Node
	{
		public TestNode10(Node parent) : this(parent, "TestNode10") { }
		public TestNode10(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected TestNode10(NodeToken nt) : base(nt) { }

        public override bool IsContentType { get { return false; } }

		[RepositoryProperty("X")]
		public int X
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
	}

	[ContentHandler]
	public class TestNode11 : TestNode10
	{
		public TestNode11(Node parent) : this(parent, "TestNode11") { }
		public TestNode11(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected TestNode11(NodeToken nt) : base(nt) { }

		[RepositoryProperty("Y")]
		public int Y
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
	}

	[ContentHandler]
	public class TestNode12 : Node
	{
		public TestNode12(Node parent) : this(parent, "TestNode12") { }
		public TestNode12(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected TestNode12(NodeToken nt) : base(nt) { }

        public override bool IsContentType { get { return false; } }

		[RepositoryProperty("A")] public int A { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		[RepositoryProperty("B")] public int B { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		[RepositoryProperty("C")] public int C { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
	}

	[ContentHandler]
	public class TestNode13 : TestNode12
	{
		public TestNode13(Node parent) : this(parent, "TestNode13") { }
		public TestNode13(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected TestNode13(NodeToken nt) : base(nt) { }

		[RepositoryProperty("D")] public int D { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
	}

    [ContentHandler]
    public class EnumTestNode : Node
    {
        public enum TestEnum { Value0, Value1, Value2, Value3, Value4 }

        public EnumTestNode(Node parent) : this(parent, "EnumTestNode") { }
        public EnumTestNode(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected EnumTestNode(NodeToken nt) : base(nt) { }

        public override bool IsContentType { get { return false; } }

        public TestEnum TestProperty { get; set; }
    }

}
