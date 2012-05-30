using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;

namespace SenseNet.Portal.UI
{
    public class IconHelper
    {
        private const string SimpleFormat = @"<img src='{1}' alt='[{0}]' title='{3}' class='{2}' />";
        private const string OverlayFormat = @"<img src='{3}' style='background-image: url({1})' alt='[{2}-{0}]' class='{4}' title='{5}' />";

        public static string RelativeIconPath
        {
            get { return WebConfigurationManager.AppSettings["RelativeIconPath"]; }
        }

        public static string OverlayPrefix
        {
            get { return WebConfigurationManager.AppSettings["OverlayPrefix"]; }
        }

        public static string ResolveIconPath(string icon, int size)
        {
            if (string.IsNullOrEmpty(icon))
                icon = WebConfigurationManager.AppSettings["DefaultIcon"];
                //throw new ArgumentNullException("icon");

            var iconroot = RelativeIconPath + "/" + size;
            var iconpath = SkinManager.Resolve(iconroot + "/" + icon + ".png");

            return iconpath;
        }
        
        public static string RenderIconTag(string icon)
        {
            return RenderIconTag(icon, null);
        }

        public static string RenderIconTag(string icon, string overlay)
        {
            return RenderIconTag(icon, overlay, 16);
        }

        public static string RenderIconTag(string icon, string overlay, int size)
        {
            return RenderIconTag(icon, overlay, size, string.Empty);
        }

        public static string RenderIconTag(string icon, string overlay, int size, string title)
        {
            var iconclasses = "sn-icon sn-icon" + size;

            var iconpath = ResolveIconPath(icon, size);

            if (string.IsNullOrEmpty(overlay))
            {
                return string.Format(SimpleFormat, icon, iconpath, iconclasses, title);
            }
            else
            {
                var overlaypath = ResolveIconPath(OverlayPrefix + overlay, size);
                return string.Format(OverlayFormat, icon, iconpath, overlay, overlaypath, iconclasses, title);
            }
        }

        public static string RenderIconTagFromPath(string path, int size)
        {
            return RenderIconTagFromPath(path, size, string.Empty);
        }

        public static string RenderIconTagFromPath(string path, int size, string title)
        {
            var iconclasses = "sn-icon sn-icon" + size;

            return string.Format(SimpleFormat, string.Empty, path, iconclasses, title);
        }
    }
}
