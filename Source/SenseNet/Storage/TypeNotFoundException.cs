using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.ContentRepository.Storage
{
    [global::System.Serializable]
    public class TypeNotFoundException : Exception
    {
        public TypeNotFoundException() { }
        public TypeNotFoundException(string typeName) : base("Type was not found: " + typeName) { }
        public TypeNotFoundException(string typeName, Exception inner) : base("Type was not found: " + typeName, inner) { }
        protected TypeNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
