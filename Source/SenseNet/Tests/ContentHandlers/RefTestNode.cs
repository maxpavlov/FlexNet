using System.Collections.Generic;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.ContentRepository.Tests.ContentHandlers
{
	[ContentHandler]
    public class RefTestNode : GenericContent, IFolder
	{
		public static string ContentTypeDefinition = @"<?xml version='1.0' encoding='utf-8'?>
<ContentType name='RepositoryTest_RefTestNode' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.RefTestNode' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
	<Fields />
</ContentType>
";

		public RefTestNode(Node parent) : base(parent, "RepositoryTest_RefTestNode") { }
		protected RefTestNode(NodeToken nt) : base(nt) { }

		#region Properties

		[RepositoryProperty("Wife", RepositoryDataType.Reference)]
		public RefTestNode Wife
		{
			get { return this.GetReference<RefTestNode>("Wife"); }
			set { this.SetReference("Wife", value); }
		}

		[RepositoryProperty("Husband", RepositoryDataType.Reference)]
		public RefTestNode Husband
		{
			get { return this.GetReference<RefTestNode>("Husband"); }
			set { this.SetReference("Husband", value); }
		}

		[RepositoryProperty("Mother", RepositoryDataType.Reference)]
		public RefTestNode Mother
		{
			get { return this.GetReference<RefTestNode>("Mother"); }
			set { this.SetReference("Mother", value); }
		}

		[RepositoryProperty("Father", RepositoryDataType.Reference)]
		public RefTestNode Father
		{
			get { return this.GetReference<RefTestNode>("Father"); }
			set { this.SetReference("Father", value); }
		}

		[RepositoryProperty("Daughter", RepositoryDataType.Reference)]
		public RefTestNode Daughter
		{
			get { return this.GetReference<RefTestNode>("Daughter"); }
			set { this.SetReference("Daughter", value); }
		}

		[RepositoryProperty("Son", RepositoryDataType.Reference)]
		public RefTestNode Son
		{
			get { return this.GetReference<RefTestNode>("Son"); }
			set { this.SetReference("Son", value); }
		}

		[RepositoryProperty("Sister", RepositoryDataType.Reference)]
		public RefTestNode Sister
		{
			get { return this.GetReference<RefTestNode>("Sister"); }
			set { this.SetReference("Sister", value); }
		}

		[RepositoryProperty("Brother", RepositoryDataType.Reference)]
		public RefTestNode Brother
		{
			get { return this.GetReference<RefTestNode>("Brother"); }
			set { this.SetReference("Brother", value); }
		}

		[RepositoryProperty("NickName", RepositoryDataType.String)]
		public string NickName
		{
			get { return this.GetProperty<string>("NickName"); }
			set { this["NickName"] = value; }
		}
		[RepositoryProperty("Age", RepositoryDataType.Int)]
		public int Age
		{
			get { return this.GetProperty<int>("Age"); }
			set { this["Age"] = value; }
		}

		#endregion

		public override object GetProperty(string name)
		{
			switch (name)
			{
				case "Wife":
					return this.Wife;
				case "Husband":
					return this.Husband;
				case "Mother":
					return this.Mother;
				case "Father":
					return this.Father;
				case "Daughter":
					return this.Daughter;
				case "Son":
					return this.Son;
				case "Sister":
					return this.Sister;
				case "Brother":
					return this.Brother;
				case "NickName":
					return this.NickName;
				case "Age":
					return this.Age;
				default:
					return base.GetProperty(name);
			}
		}
		public override void SetProperty(string name, object value)
		{
			switch (name)
			{
				case "Wife":
					this.Wife = (RefTestNode)value;
					break;
				case "Husband":
					this.Husband = (RefTestNode)value;
					break;
				case "Mother":
					this.Mother = (RefTestNode)value;
					break;
				case "Father":
					this.Father = (RefTestNode)value;
					break;
				case "Daughter":
					this.Daughter = (RefTestNode)value;
					break;
				case "Son":
					this.Son = (RefTestNode)value;
					break;
				case "Sister":
					this.Sister = (RefTestNode)value;
					break;
				case "Brother":
					this.Brother = (RefTestNode)value;
					break;
				case "NickName":
					this.NickName = (string)value;
					break;
				case "Age":
					this.Age = (int)value;
					break;
				default:
					base.SetProperty(name, value);
					break;
			}
		}

        //================================================ IFolder

        public virtual IEnumerable<Node> Children
        {
            get { return this.GetChildren(); }
        }
        public virtual int ChildCount
        {
            get { return this.GetChildCount(); }
        }

	}
}
