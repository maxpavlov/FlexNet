using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.ContentRepository
{
    [global::System.Serializable]
    public class InvalidContentActionException : Exception
    {
        public InvalidContentActionException() { }
        public InvalidContentActionException(string message) : base(message) { }
        public InvalidContentActionException(string message, Exception inner) : base(message, inner) { }
        protected InvalidContentActionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
