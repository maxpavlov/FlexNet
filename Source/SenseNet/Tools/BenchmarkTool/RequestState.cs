using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace SenseNet.Benchmarking
{
    internal class RequestState
    {
        public int Id;
        public int Level;
        public string ContentTypeName;
        public string SnPath;
        public string FsPath;
        public long FileSize;
        public WebRequest Request;
        public Response Response;
        public DateTime InitTime = DateTime.Now;
        public DateTime StartTime;
        public DateTime EndTime;
        public TimeSpan ResponseTime;
        public int UrlIndex;
    }
}
