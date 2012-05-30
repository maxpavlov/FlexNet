using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.ApplicationModel
{
    public class ShareAction : ClientAction
    {
        public override string MethodName
        {
            get
            {
                return "SN.Wall.openShareDialog";
            }
            set
            {
                base.MethodName = value;
            }
        }

        public override string ParameterList
        {
            get
            {
                return this.Content == null ? string.Empty : string.Format(@"'{0}'", this.Content.Id);
            }
            set
            {
                base.ParameterList = value;
            }
        }
    }
}
