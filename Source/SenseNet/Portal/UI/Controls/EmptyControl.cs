using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace SenseNet.Portal.UI.Controls
{
    public class EmptyControl : FieldControl
    {

        public override object GetData()
        {
            // throw new NotImplementedException();
            return null;
        }

        public override void SetData(object data)
        {
            // throw new NotImplementedException();
        }
    }
}
