//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Runtime.Serialization;
//using System.Xml.Serialization;

//namespace SenseNet.Services.ContentStore
//{
//    public interface IContent
//    {
//        int Id { get; set; }

//        string Name { get; set; }

//        string Path { get; set; }

//        string Title { get; set; }

//        string Description { get; set; }

//        string Icon { get; set; }

//        string NodeTypeName { get; set; }

//        object NodeType { get; set; }

//        string ContentTypeName { get; set; }

//        object ContentType { get; set; }

//        int Index { get; set; }

//        DateTime CreationDate { get; set; }

//        DateTime ModificationDate { get; set; }

//        string ModifiedBy { get; set; }

//        string LockedBy { get; set; }

//        bool leaf { get; set; }

//        int ChildCount { get; set; }

//        int ParentId { get; set; }

//        bool HasBinary { get; set; }

//        // compatibility hacks, uses and references unknown
//        [DataMember]
//        int id { get; set; }
//        [DataMember]
//        string text { get; set; }
//        [DataMember]
//        string iconCls { get; set; }
//        // endhack

//        // Large enumerations //////////////////////////////////////

//        object[] Properties { get; set; }

//        Content[] Children { get; set; }
//    }
//}
