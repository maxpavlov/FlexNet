using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.UI.Controls
{
    [ParseChildren(true)]
    [ToolboxData("<{0}:RepeaterView runat=server />")]
    public class RepeaterView : Repeater
    {
        private QueryView ParentQueryView
        {
            get
            {
                var control = Parent;

                while (control != null && !(control is QueryView))
                {
                    control = control.Parent;
                }

                return control as QueryView;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var qv = this.ParentQueryView;

            if (qv == null)
                return;

            DataSource = qv.NodeItemList;
            DataBind();
        }

        public ITemplate EmptyDataTemplate { get; set; }

        protected override void OnDataBinding(EventArgs e)
        {
            var qv = this.ParentQueryView;
            if (qv != null && qv.NodeItemList != null && qv.NodeItemList.Count == 0 &&
                EmptyDataTemplate != null)
            {
                //Comment: No header and footer
                //base.OnDataBinding(e);
                EmptyDataTemplate.InstantiateIn(this);
            }
            else
            {
                base.OnDataBinding(e);
            }
        }
    }
}