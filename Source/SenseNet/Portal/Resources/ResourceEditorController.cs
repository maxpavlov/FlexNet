using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Portal.Virtualization;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.i18n;
using System.Globalization;
using System.Web.Script.Serialization;
using SenseNet.Search;
using System.Xml;
using SenseNet.Portal.UI;
using SenseNet.Diagnostics;

namespace SenseNet.Portal
{
    public class ResourceEditorController : Controller
    {
        internal class ResourceData
        {
            public string Lang { get; set; }
            public string Value { get; set; }
        }


        //===================================================================== Public interface
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetStringResources(string classname, string name, string rnd)
        {
            AssertPermission();

            var langs = Site.GetAllLanguages();
            var resources = new Dictionary<string,string>();
            foreach (var lang in langs) {
                try
                {
                    var cultureInfo = new CultureInfo(lang.Value);
                    var s = SenseNetResourceManager.Current.GetObjectOrNull(classname, name, cultureInfo, false) as string;
                    resources.Add(lang.Value, s);
                }
                catch (Exception ex)
                {
                    Logger.WriteException(ex);
                    resources.Add(lang.Value, "ERROR: " + ex.Message);
                }
            }

            return Json(resources.ToArray(), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult SaveResource(string classname, string name, string resources, string rnd)
        {
            AssertPermission();

            var ser = new JavaScriptSerializer();
            var resourcesData = ser.Deserialize<List<ResourceData>>(resources);

            var res = GetResourceForClassName(classname);

            if (res != null)
            {
                // parse xml
                var xml = new XmlDocument();
                using (var stream = res.Binary.GetStream())
                {
                    xml.Load(stream);
                }

                foreach (var resourceData in resourcesData)
                {
                    // search resource in xml
                    var reselement = xml.SelectSingleNode("/Resources/ResourceClass[@name='" + classname + "']/Languages/Language[@cultureName='"+resourceData.Lang+"']/data[@name='"+name+"']/value");
                    if (reselement != null)
                    {
                        reselement.InnerText = resourceData.Value;
                    }
                    else
                    {
                        CreateResDataUnderClass(xml, classname, name, resourceData);
                    }
                }
                using (new SystemAccount())
                {
                    using (var stream = new System.IO.MemoryStream())
                    {
                        xml.Save(stream);
                        res.Binary.SetStream(stream);
                        res.Save();
                    }
                }
            }
            else
            {
                // create new resource file for this classname
                CreateNewResourceFile(classname, name, resourcesData);
            }

            return null;
        }

        
        //===================================================================== Public helpers
        public static void InitEditorScript(System.Web.UI.Page page)
        {
            UITools.AddScript("$skin/scripts/sn/SN.ResourceEditor.js");
            var languages = Portal.Site.GetAllLanguages();

            var langs = string.Empty;
            foreach (var option in languages)
            {
                langs += "'" + option.Value + "',";
            }
            langs = langs.Trim(',');

            UITools.RegisterStartupScript("resourceEditorInit", "SN.ResourceEditor.init(" + langs + ");", page);
        }

        //===================================================================== Resource xml handling
        private void CreateNewResourceFile(string classname, string name, List<ResourceData> resourcesData)
        {
            var parentNode = Node.LoadNode("/Root/Localization");
            if (parentNode == null)
                return;

            var res = new Resource(parentNode);
            res.Name = classname + "Resources.xml";

            var xml = new XmlDocument();
            var root = xml.CreateElement("Resources");
            xml.AppendChild(root);

            var classelement = xml.CreateElement("ResourceClass");
            var classnameattr = xml.CreateAttribute("name");
            classnameattr.Value = classname;
            classelement.Attributes.Append(classnameattr);
            root.AppendChild(classelement);

            var languageselement = xml.CreateElement("Languages");
            classelement.AppendChild(languageselement);

            foreach (var resourceData in resourcesData)
            {
                CreateResDataUnderClass(xml, classname, name, resourceData);
            }

            using (new SystemAccount())
            {
                using (var stream = new System.IO.MemoryStream())
                {
                    xml.Save(stream);
                    res.Binary.SetStream(stream);
                    res.Save();
                }
            }
        }
        private void CreateResDataUnderClass(XmlDocument xml, string classname, string name, ResourceData resourceData)
        {
            // create new element
            var langelement = xml.SelectSingleNode("/Resources/ResourceClass[@name='" + classname + "']/Languages/Language[@cultureName='" + resourceData.Lang + "']");
            if (langelement == null)
            {
                // create language element
                var langparent = xml.SelectSingleNode("/Resources/ResourceClass[@name='" + classname + "']/Languages");
                if (langparent == null)
                    return;

                langelement = xml.CreateElement("Language");
                var cultureNameAttr = xml.CreateAttribute("cultureName");
                cultureNameAttr.Value = resourceData.Lang;
                langelement.Attributes.Append(cultureNameAttr);
                langparent.AppendChild(langelement);
            }

            // create data element under language element
            var dataelement = xml.CreateElement("data");
            var nameAttr = xml.CreateAttribute("name");
            nameAttr.Value = name;
            dataelement.Attributes.Append(nameAttr);
            langelement.AppendChild(dataelement);

            // create value element under data element
            var valueelement = xml.CreateElement("value");
            valueelement.InnerText = resourceData.Value;
            dataelement.AppendChild(valueelement);
        }
        private Resource GetResourceForClassName(string classname)
        {
            var res = Node.LoadNode("/Root/Localization/" + classname + "Resources.xml") as Resource;
            if (res != null)
                return res;

            var resources = ContentQuery.Query("+Type:Resource", new QuerySettings { EnableAutofilters = false }).Nodes.OrderBy(i => i.Index);
            foreach (Resource resc in resources)
            {
                var xml = new XmlDocument();
                xml.Load(resc.Binary.GetStream());
                var classelement = xml.SelectSingleNode("/Resources/ResourceClass[@name='"+classname+"']");
                if (classelement != null)
                    return resc;
            }
            
            return null;
        }


        //===================================================================== Helper methods
        private static readonly string PlaceholderPath = "/Root/System/PermissionPlaceholders/ResourceEditor-mvc";

        private static void AssertPermission()
        {
            if (!HasPermission())
                throw new SenseNetSecurityException("Access denied for " + PlaceholderPath);
        }
        private static bool HasPermission()
        {
            var permissionContent = Node.LoadNode(PlaceholderPath);
            var nopermission = (permissionContent == null || !permissionContent.Security.HasPermission(PermissionType.RunApplication));
            return !nopermission;
        }
    }
}
