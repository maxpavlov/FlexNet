using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using asp = System.Web.UI;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;

namespace SenseNet.Portal.UI.Controls
{
    [NonVisualControl]
    public class StyleManager : Control
    {
        // Singletont bevedeni!

        #region properties

        public List<string> Styles { get; private set; }

        #endregion

        public StyleManager()
        {
            Styles = new List<string>();
        }

        public static StyleManager GetCurrent(asp.Page page)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            return (page.Items[typeof(StyleManager)] as StyleManager);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            foreach (string path in Styles)
            {
                //Content styleSheet = Content.Load(path);
                //IEnumerable<PortalActionLink> links = PortalActionLinkManager.GetAvailableActions(styleSheet);
                //...
                writer.WriteLine("<style type='text/css' src='{0}' />", path);
            }
        }
    }
}
