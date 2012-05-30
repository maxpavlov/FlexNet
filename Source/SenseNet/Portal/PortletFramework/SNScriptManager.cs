using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.ComponentModel;
using System.Web.UI.HtmlControls;
using System.Web;
using SenseNet.ContentRepository;

namespace SenseNet.Portal.UI.PortletFramework
{
    public class SNScriptManager : System.Web.UI.AjaxScriptManager
    {
        private SNScriptLoader _smartLoader;
        public SNScriptLoader SmartLoader
        {
            get { return _smartLoader; }
        }

        public SNScriptManager()
        {
            _smartLoader = new SNScriptLoader();
        }

        protected override void OnInit(EventArgs e)
        {
            this.Page.PreRenderComplete += new EventHandler(Page_PreRenderComplete);
            base.OnInit(e);
        }

        private void RenderScriptReferences()
        {
            var smartList = SmartLoader.GetScriptsToLoad();
            foreach (var spath in smartList)
            {
                var lower = spath.ToLower();
                if (lower.EndsWith(".css"))
                {
                    UITools.AddStyleSheetToHeader(UITools.GetHeader(), spath);
                }
                else
                {
                    var sref = new ScriptReference(spath);
                    Scripts.Add(sref);
                }
            }
        }

        protected void Page_PreRenderComplete(object sender, EventArgs e)
        {
            this.RenderScriptReferences();
        }
    }
}