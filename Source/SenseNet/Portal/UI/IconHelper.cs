using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Portal.UI
{
    public class IconHelper
    {
        private const string SimpleFormat = @"<img src='{1}' alt='[{0}]' title='{3}' class='{2}' />";
        private const string OverlayFormat = @"<div class='iconoverlay'><img src='{1}' alt='[{0}]' title='{3}' class='{2}' /><div class='overlay'><img src='{3}' alt='[{2}-{0}]' class='{2}' title='{5}' /></div></div>";

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

        public static string RenderIconTagFromPath(string path, string overlay, int size)
        {
            return RenderIconTagFromPath(path, overlay, size, string.Empty);
        }

        public static string RenderIconTagFromPath(string path, int size, string title)
        {
            return RenderIconTagFromPath(path, null, size, title);
        }

        public static string RenderIconTagFromPath(string path, string overlay, int size, string title)
        {
            var iconclasses = "sn-icon sn-icon" + size;

            if (string.IsNullOrEmpty(overlay))
            {
                return string.Format(SimpleFormat, string.Empty, path, iconclasses, title);
            }
            else
            {
                var overlaypath = ResolveIconPath(OverlayPrefix + overlay, size);
                return string.Format(OverlayFormat, RepositoryPath.GetFileName(path), path, overlay, overlaypath, iconclasses, title);
            }
        }

        public static string GetOverlay(Content content, out string title)
        {
            title = string.Empty;

            if (content == null || content.ContentHandler == null)
                return string.Empty;

            var overlay = string.Empty;

            if (content.ContentHandler.Locked)
            {
                overlay = content.ContentHandler.LockedById == User.Current.Id
                              ? "checkedoutbyme"
                              : "checkedout";
                title = content.ContentHandler.LockedBy.Username;
            }
            else if (content.Approvable)
                overlay = "approvable";

            return overlay;
        }
    }
}
