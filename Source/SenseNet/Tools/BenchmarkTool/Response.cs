using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SenseNet.Benchmarking
{
    internal class Response
    {
        public HttpStatusCode StatusCode;
        public string Content;
        public Dictionary<string, string> ResponseLog;
    }
}
