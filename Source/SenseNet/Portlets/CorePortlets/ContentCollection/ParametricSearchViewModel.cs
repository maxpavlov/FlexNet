using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SenseNet.Portal.Portlets.ContentCollection
{
    [XmlRoot("Model")]
    public class ParametricSearchViewModel : ContentCollectionViewModel
    {
        public SearchParameter[] SearchParameters;

    }

    [XmlRoot("SearchParameter")]
    public class SearchParameter
    {
        public string Name;
        public string Value;

        //public SearchParameter(string name, string value)
        //{
        //    Name = name;
        //    Value = value;
        //}
    }
}
