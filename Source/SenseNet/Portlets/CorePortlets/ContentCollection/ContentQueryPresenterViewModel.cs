using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.Portal.Portlets
{
    public class ContentQueryPresenterViewModel : ContentCollectionViewModel
    {

        //protected override void WriteXml(XmlWriter writer, bool withChildren)
        //{
        //    const string thisName = "SearchFolder";
        //    const string thisPath = "/Root/SearchFolder";

        //    writer.WriteStartElement("Content");
        //    base.WriteHead(writer, thisName, thisName, thisName, thisPath, true);

        //    //if (_query != null)
        //    //{
        //    //    writer.WriteStartElement("Query");
        //    //    writer.WriteRaw(_query.ToXml());
        //    //    writer.WriteEndElement();
        //    //}

        //    if (withChildren && Items != null)
        //    {
        //        writer.WriteStartElement("Children");
        //        this.WriteXml(Items, writer);
        //        writer.WriteEndElement();
        //    }

        //    writer.WriteEndElement();
        //}

        //protected override void WriteXml(XmlWriter writer, string queryFilter, QuerySettings querySettings)
        //{
        //    WriteXml(writer, false);
        //}

        //protected override void WriteXml(XmlWriter writer, string referenceMemberName)
        //{
        //    WriteXml(writer, false);
        //}

    }
}
