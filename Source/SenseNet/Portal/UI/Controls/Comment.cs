using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.Wall;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.UI.Controls
{
    public class Comment : UserControl
    {
        // ================================================================================================ Properties
        public string NodePath { get; set; }
        public string ContextInfoID { get; set; }

        private string _contextPath;
        protected virtual string ContextPath
        {
            get
            {
                if (string.IsNullOrEmpty(_contextPath))
                {
                    var context = UITools.FindContextInfo(this, ContextInfoID);
                    if (context != null)
                    {
                        _contextPath = context.Path;

                        //NodePath may contain a relative path
                        if (!string.IsNullOrEmpty(NodePath))
                        {
                            _contextPath = RepositoryPath.Combine(_contextPath, NodePath);
                        }
                    }
                    else if (!string.IsNullOrEmpty(NodePath))
                        _contextPath = NodePath;
                    else
                        _contextPath = ActionMenu.GetPathFromContentView(Parent);
                }

                return _contextPath ?? string.Empty;
            }
        }

        // ================================================================================================ Methods
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            UITools.AddScript("$skin/scripts/sn/SN.Wall.js");
        }

        protected override void CreateChildControls()
        {
            if (string.IsNullOrEmpty(ContextPath))
                return;

            var contextNode = Node.LoadNode(ContextPath);
            if (contextNode == null)
                return;

            int commentCount;
            int likeCount;
            var markupStr = WallHelper.GetCommentControlMarkup(contextNode, out commentCount, out likeCount);

            this.Controls.Add(new Literal { Text = markupStr });

            this.ChildControlsCreated = true;
        }

    }
}
