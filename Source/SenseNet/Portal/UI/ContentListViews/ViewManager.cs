using System;
using System.Collections.Generic;
using System.Linq;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;
using SenseNet.Portal.UI.ContentListViews.Handlers;

namespace SenseNet.Portal.UI.ContentListViews
{
    public class ViewManager : IViewManager
    {
        public static readonly string VIEWSFOLDERNAME = "Views";

        internal static IEnumerable<File> GetViewsForContainer(string path)
        {
            Node cont = Node.LoadNode(path);
            return GetViewsInContext(cont);
        }

        public static IEnumerable<File> GetViewsForContainer(Node container)
        {
            var viewCollection = Node.LoadNode(container.Path + "/Views") as IFolder;
            if (viewCollection != null)
            {
                var views = from Node child in viewCollection.Children
                            where child.Name.EndsWith(".ascx")
                            select child;

                return views.OfType<File>();
            }

            return new List<File>();
        }

        public static IEnumerable<File> GetViewsInContext(Node subnode)
        {
            var subcont = subnode as GenericContent;
            return GetViewsForContainer(subcont.MostRelevantContext);
        }

        private static File LoadView(Node container, string viewName)
        {
            File viewNode =  null;

            if (container != null)
                viewNode = Node.Load<File>(container.Path + "/Views/" + viewName);

            if (viewNode == null && !string.IsNullOrEmpty(viewName))
                viewNode = Node.Load<File>(viewName);

            //hack - fallback view for non-list folders
            if (viewNode == null)
                viewNode = Node.Load<File>("/Root/System/SystemPlugins/ListView/Fallback.ascx");

            return viewNode;
        }

        public static File LoadViewInContext(Node subnode, string viewName)
        {
            var subcont = subnode as GenericContent;
            return LoadView(subcont.MostRelevantContext, viewName);
        }

        public static string GetViewPathInContext(Node subnode, string viewName)
        {
            var view = LoadViewInContext(subnode, viewName);
            if (view != null)
                return view.Path;
            return null;
        }

        public static string GetViewPath(Node container, string viewName)
        {
            var view = LoadView(container, viewName);
            if (view != null)
                return view.Path;
            return null;
        }

        public static File LoadDefaultView(ContentList list)
        {
            return LoadView(list, list.DefaultView);
        }

        public static void AddToDefaultView(FieldSetting fieldSetting, ContentList contentList)
        {
            if (fieldSetting == null || contentList == null)
                return;

            var iv = LoadDefaultView(contentList) as IView;
            if (iv == null) 
                return;

            var viewNode = iv as Node;
            if (viewNode == null)
                return;

            //if the view is global, create local copy first
            if (!viewNode.Path.StartsWith(contentList.Path))
            {
                viewNode = ViewManager.CopyViewLocal(contentList.Path, viewNode.Path, true);
                iv = viewNode as IView;
            }

            fieldSetting.Owner = ContentType.GetByName("ContentList");

            iv.AddColumn(new Column
                             {
                                 FullName = fieldSetting.FullName,
                                 BindingName = fieldSetting.BindingName,
                                 Title = fieldSetting.DisplayName,
                                 Index = iv.GetColumns().Count() + 1
                             });

            viewNode.Save();
        }

        public static Handlers.ViewBase CopyViewLocal(string listPath, string viewPath)
        {
            return CopyViewLocal(listPath, viewPath, false);
        }

        public static Handlers.ViewBase CopyViewLocal(string listPath, string viewPath, bool setAsDefault)
        {
            if (string.IsNullOrEmpty(listPath))
                throw new ArgumentNullException("listPath");
            if (string.IsNullOrEmpty(viewPath))
                throw new ArgumentNullException("viewPath");

            var viewName = RepositoryPath.GetFileNameSafe(viewPath);
            var viewsFolderPath = RepositoryPath.Combine(listPath, ViewManager.VIEWSFOLDERNAME);
            var views = Content.Load(viewsFolderPath) ?? Tools.CreateStructure(viewsFolderPath, "SystemFolder");

            Node.Copy(viewPath, viewsFolderPath);

            var localView = Node.Load<Handlers.ViewBase>(RepositoryPath.Combine(viewsFolderPath, viewName));

            if (setAsDefault)
            {
                var cl = Node.Load<ContentList>(listPath);
                if (cl != null)
                {
                    cl.DefaultView = viewName;
                    cl.Save();
                }
            }

            return localView;
        }

        //=================================================================== IViewManager Members

        void IViewManager.AddToDefaultView(FieldSetting fieldSetting, ContentList contentList)
        {
            AddToDefaultView(fieldSetting, contentList);
        }
    }
}
