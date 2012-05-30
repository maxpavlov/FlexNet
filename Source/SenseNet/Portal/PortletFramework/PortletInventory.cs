using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.UI.PortletFramework;
using System.Reflection;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Schema;
using System.Text.RegularExpressions;

namespace SenseNet.Portal.UI.PortletFramework
{
    public class PortletInventory
    {
        /* ====================================================================== Public methods */
        public static readonly string PortletsFolderPath = "/Root/Portlets";
        public static SystemFolder GetPortletFolder()
        {
            var parent = Node.LoadNode(PortletsFolderPath);
            if (parent == null)
            {
                // new portletsDir
                parent = new SystemFolder(Node.LoadNode("/Root"));
                parent.Name = "Portlets";
                parent.Save();
            }
            return parent as SystemFolder;
        }
        public static List<PortletInventoryItem> GetPortletsFromDll()
        {
            var portlets = new List<PortletInventoryItem>();
            foreach (Assembly privateAssembly in TypeHandler.GetAssemblies())
            {
                AssemblyName asmName = privateAssembly.GetName();
                foreach (Type privateType in privateAssembly.GetTypes())
                {
                    PortletBase wp = null;
                    if (privateType.BaseType != null)
                    {
                        if (privateType.IsSubclassOf(typeof(PortletBase)) && !privateType.IsAbstract && !privateType.IsNotPublic)
                        {
                            object instance = null;
                            try
                            {
                                instance = Activator.CreateInstance(privateType);
                            }
                            catch (Exception e) //logged
                            {
                                Logger.WriteException(e);
                                continue;
                            }
                            wp = (PortletBase)instance;
                        }
                    }
                    if (wp != null)
                        portlets.Add(PortletInventoryItem.Create(wp, privateAssembly));
                }
            }
            return portlets;
        }
        public static List<PortletCategory> GetCategories(List<PortletInventoryItem> portlets)
        {
            var categories = new List<PortletCategory>();
            foreach (var portlet in portlets)
            {
                if (!categories.Any(c => c.Title == portlet.Portlet.Category.Title))
                    categories.Add(portlet.Portlet.Category);
            }
            return categories;
        }
        public static IEnumerable<Node> GetPortletsFromRepo()
        {
            var query = new NodeQuery();
            var expression = new ExpressionList(ChainOperator.And);
            expression.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith, PortletsFolderPath));
            expression.Add(new TypeExpression(NodeType.GetByName("Portlet")));
            query.Add(expression);
            return query.Execute().Nodes;
        }
        public static IEnumerable<Node> GetCategoriesFromRepo()
        {
            var parent = GetPortletFolder();
            return parent.Children;
        }
        public static void ImportCategory(PortletCategory category, IEnumerable<Node> repoCategories)
        {
            var categoryName = GetValidName(category.Title);
            var parent = GetPortletFolder();
            if (parent == null)
                throw new InvalidOperationException("Portlets folder does not exist.");

            // check if already exists
            if (repoCategories.Any(c => c.Name == categoryName))
                return;

            // create category node
            var categoryNode = Content.CreateNew("PortletCategory", parent, categoryName);
            categoryNode["DisplayName"] = category.Title;
            categoryNode["Description"] = category.Description;
            categoryNode.Save();
        }
        public static void ImportPortlet(PortletInventoryItem portlet, IEnumerable<Node> repoPortlets)
        {
            var portletName = GetValidName(portlet.Portlet.Name);
            
            // check if already exists
            if (repoPortlets.Any(c => c.Name == portletName))
                return;

            // create portlet node
            var categoryNode = Node.LoadNode(RepositoryPath.Combine(PortletsFolderPath, GetValidName(portlet.Portlet.Category.Title)));
            if (categoryNode == null)
                return;

            var portletNode = Content.CreateNew("Portlet", categoryNode, portletName);
            portletNode["DisplayName"] = portlet.Portlet.Name;
            portletNode["Description"] = portlet.Portlet.Description;
            portletNode["TypeName"] = GetPortletTypeName(portlet.Portlet.GetType());
            var imageData = portlet.GetImageFieldData();
            if (imageData != null)
                portletNode["PortletImage"] = imageData;
            portletNode.Save();
        }


        /* ====================================================================== Private methods */
        private static string GetPortletTypeName(Type portletType)
        {
            var fragments = portletType.AssemblyQualifiedName.Split(',');
            // first two fragment: portlet name with namespace and assemblyname without version info
            var portletTypeName = string.Concat(fragments[0], ",", fragments[1]);
            return portletTypeName;
        }
        private static string GetValidName(string title)
        {
            var name = title;
            //foreach (var ch in RepositoryPath.InvalidNameChars)
            //{
            //    name = name.Replace(ch.ToString(), "");
            //}
            name = new Regex(RepositoryPath.InvalidNameCharsPattern).Replace(name, string.Empty);
            name = name.Replace(" ", string.Empty);
            return name;
        }
    }
}
