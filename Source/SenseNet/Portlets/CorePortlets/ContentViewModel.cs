using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using System.Xml.Serialization;

namespace SenseNet.Portal.Portlets
{
    public class ContentViewModel
    {
        [XmlIgnore]
        public Content Content { get; internal set; }
    }
}
