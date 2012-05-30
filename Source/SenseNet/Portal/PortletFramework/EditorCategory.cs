using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.Portal.UI.PortletFramework
{
    public class EditorCategory
    {
        public const string UI = "User interface";
        public const int UI_Order = 50;

        /* ====================================================================== Custom portlet editor categories */
        public const string ContentList = "Content list";
        public const int ContentList_Order = 100;

        public const string Query = "Query";
        public const int Query_Order = 110;

        public const string AddNewPortlet = "New content";
        public const int AddNewPortlet_Order = 120;

        public const string GoogleSearch = "Google search";
        public const int GoogleSearch_Order = 130;

        public const string SiteMenu = "Site menu";
        public const int SiteMenu_Order = 140;

        public const string TagAdmin = "Tag admin";
        public const int TagAdmin_Order = 150;

        public const string Login = "Login";
        public const int Login_Order = 160;

        public const string ImageGallery = "Image gallery";
        public const int ImageGallery_Order = 170;

        public const string ImportExport = "Import-export";
        public const int ImportExport_Order = 180;

        public const string PublicRegistration = "Public registration";
        public const int PublicRegistration_Order = 190;

        public const string QuickSearch = "Search";
        public const int QuickSearch_Order = 200;

        public const string Search = "Search";
        public const int Search_Order = 210;
        
        public const string SingleContentPortlet = "Webcontent";
        public const int SingleContentPortlet_Order = 220;

        public const string Workflow = "Workflow";
        public const int Workflow_Order = 230;

        public const string ADSync = "Active Directory synchronization";
        public const int ADSync_Order = 240;


        /* ====================================================================== Common portlet editor categories */
        public const string ContextBinding = "Context binding";
        public const int ContextBinding_Order = 300;

        public const string Collection = "Collection";
        public const int Collection_Order = 400;

        public const string Cache = "Cache";
        public const int Cache_Order = 500;

        public const string Other = "Other";
        public const int Other_Order = 600;
    }
}
