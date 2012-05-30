using System;
using System.Collections.Generic;
using System.Text;

namespace SenseNet.Portal.Personalization
{
    [global::System.Serializable]
    public class PersonalizationException : Exception
    {
        public PersonalizationException() { }
        public PersonalizationException(string messsage) : base(messsage) { }
        public PersonalizationException(string messsage, Exception innerException) : base(messsage, innerException) { }
        protected PersonalizationException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }

    }
}