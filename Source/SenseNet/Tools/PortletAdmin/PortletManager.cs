using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI.WebControls.WebParts;

using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using WebPartTools;
using SNP = SenseNet.Portal;
using SenseNet.ContentRepository;
using SenseNet.Diagnostics;

namespace SenseNet.Tools.PortletAdministration
{
    internal sealed class PortletManager
    {
        private static readonly object _lock = new object();
        private static PortletManager _current;

        private PortletManager()
        {
        }

        internal static PortletManager Current
        {
            get
            {
                if (_current == null)
                {
                    lock (_lock)
                    {
                        if (_current == null)
                        {
                            _current = new PortletManager();
                            Logger.WriteInformation("PortletManager created.");
                        }
                    }
                }
                return _current;
            }
        }

        // Public methods //////////////////////////////////////////////////////////
        public void LoadRepositoryRoot()
        {
            Folder root = null;
            root = Repository.Root;
        }
        public void ListWebPartElements(Dictionary<string, string> parameters)
        {
            string listType = parameters["LIST"];
            switch (listType)
            {
                case "zones":
                case "portlets":
                case "all":
                    ListElements(parameters, listType);
                    break;
                default:
                    throw new NotSupportedException(String.Format("'{0}' listtype is not supported.", listType));
            }
        }
        public void MoveWebParts(Dictionary<string, string> parameters)
        {
            string portletId = parameters["MOVE"];
            if (String.IsNullOrEmpty(portletId))
                return; // WriteUsage();

            string zoneId = parameters["TO"];
            if (String.IsNullOrEmpty(zoneId))
                return; // WriteUsage;


            if (!parameters.ContainsKey("PAGE"))
                return;

            bool isRecursive = IsRecursive(parameters);
            string pagePath = parameters["PAGE"];
            var pageDescription = NodeHead.Get(pagePath);

            if (pageDescription == null)
                throw new ApplicationException(String.Format("{0} does not exist.", pagePath));

            string nodeTypeName = pageDescription.GetNodeType().Name;
            bool isSite = nodeTypeName.Equals(typeof(SNP.Site).Name);


            if (!isRecursive && !isSite)
            {
				var page = Node.Load<SNP.Page>(pagePath);
                if (page == null)
                    throw new ApplicationException(String.Format("{0} couldn't be loaded.", pagePath));


                MoveWebPart(portletId, zoneId, page);
                return;
            }
            else
            {
                var pageList = GetPageList(pagePath, isRecursive);

                foreach (Node node in pageList)
                {
					var page = node as SNP.Page;
                    MoveWebPart(portletId, zoneId, page);
                }
            }
        }
        public void DeleteWebParts(Dictionary<string, string> parameters)
        {
            string portletId = parameters["DEL"];
            if (String.IsNullOrEmpty(portletId))
                return; // WriteUsage();

            if (!parameters.ContainsKey("PAGE"))
                return;

            bool isRecursive = IsRecursive(parameters);
            string pagePath = parameters["PAGE"];
            var pageDescription = NodeHead.Get(pagePath);

            if (pageDescription == null)
                throw new ApplicationException(String.Format("{0} does not exist.", pagePath));

            string nodeTypeName = pageDescription.GetNodeType().Name;
            bool isSite = nodeTypeName.Equals(typeof(SNP.Site).Name);


            if (!isRecursive && !isSite)
            {
				var page = Node.Load<SNP.Page>(pagePath);
                if (page == null)
                    throw new ApplicationException(String.Format("{0} couldn't be loaded.", pagePath));

                DeleteWebPart(portletId, page);
                return;
            }
            var pageList = GetPageList(pagePath, isRecursive);

            foreach (Node node in pageList)
            {
				var page = node as SNP.Page;
                DeleteWebPart(portletId, page);
            }
        }

        // Internals ///////////////////////////////////////////////////////////////
        private void ListElements(Dictionary<string, string> parameters, string listType)
        {
            if (!parameters.ContainsKey("PAGE"))
                return; // WriteUsage();

            bool isRecursive = IsRecursive(parameters);
            string pagePath = parameters["PAGE"];
            var pageDescription = NodeHead.Get(pagePath);

            if (pageDescription == null)
                throw new ApplicationException(String.Format("{0} does not exist.", pagePath));

            string nodeTypeName = pageDescription.GetNodeType().Name;
            bool isSite = nodeTypeName.Equals(typeof(SNP.Site).Name);

            if (!isRecursive && !isSite)
            {
				var page = Node.Load<SNP.Page>(pagePath);
                if (page == null)
                    throw new ApplicationException(String.Format("{0} couldn't be loaded.", pagePath));

                ListWebPartElements(listType, page);
                return;
            }
            var pageList = GetPageList(pagePath, isRecursive);

            foreach (Node node in pageList)
            {
                var page = node as SNP.Page;
                ListWebPartElements(listType, page);
            }
        }
		private void ListWebPartElements(string listType, SNP.Page page)
        {
            if (String.IsNullOrEmpty(listType))
                throw new ArgumentNullException("listType");
            if (page == null)
                throw new ArgumentNullException("page");


            //var page = Node.Load<Page>(pagePath);
            //if (page == null)
            //    throw new ApplicationException("{0} couldn't be loaded.");

            BinaryData sharedDataBlobBinaryData = page.PersonalizationSettings;
            if (sharedDataBlobBinaryData == null)
            {
                Console.WriteLine(String.Format("{0} has no personalization data.", page.Path));
                return;
            }
                

            Console.WriteLine(page.Path);

            PageState pageState = GetPageState(sharedDataBlobBinaryData);
            if (pageState == null)
                return;

            switch (listType)
            {
                case "all":
                    ListZones(pageState, true);
                    break;
                case "zones":
                    ListZones(pageState, false);
                    break;
                case "portlets":
                    ListWebParts(pageState);
                    break;
                default:
                    throw new NotSupportedException(String.Format("{0} listtype is not supported.", listType));
            }
        }
        private void ListWebParts(PageState pageState)
        {
            string portletNameFormat = @"  {0} ({1})";
            Dictionary<string, LocationInfo> locations = pageState.Locations;

            foreach (string key in locations.Keys)
            {
                LocationInfo locationInfo = locations[key];
                Console.WriteLine(portletNameFormat, locationInfo.ControlID, locationInfo.ZoneID);
            }
        }
        private void ListZones(PageState pageState, bool withPortlets)
        {
            string zoneNameFormat = @"  {0}";
            Dictionary<string, LocationInfo> locations = pageState.Locations;

            Dictionary<string, string> zones = GetZones(locations);

            foreach (string key in zones.Keys)
            {
                Console.WriteLine(zoneNameFormat, key);
                if (withPortlets)
                    ListWebPartsByZoneID(pageState, key);
            }
        }
        private void ListWebPartsByZoneID(PageState pageState, string zoneId)
        {
            if (pageState == null)
                throw new ArgumentNullException("pageState");

            if (String.IsNullOrEmpty(zoneId))
                throw new ArgumentNullException("zoneId");

            string portletNameFormat = @"    {0}";
            var portlets = new List<string>();
            Dictionary<string, LocationInfo> locations = pageState.Locations;
            foreach (string webPartID in locations.Keys)
            {
                LocationInfo locationInfo = locations[webPartID];
                if (zoneId.Equals(locationInfo.ZoneID))
                    portlets.Add(locationInfo.ControlID);
            }
            foreach (string portletName in portlets)
            {
                Console.WriteLine(portletNameFormat, portletName);
            }
        }

		private void MoveWebPart(string portletId, string zoneId, SNP.Page page)
        {
            BinaryData sharedDataBlobBinaryData = page.PersonalizationSettings;
            if (sharedDataBlobBinaryData == null)
            {
                Console.WriteLine(String.Format("{0} has no personalization data.", page.Path));
                return;
            }

            Console.WriteLine(page.Path);

            PageState pageState = GetPageState(sharedDataBlobBinaryData);
            if (pageState == null)
                return;

            pageState.Locations[portletId].ZoneID = zoneId;

            SaveNewPersonalizationSettings(page, pageState);
        }
		private void DeleteWebPart(string portletId, SNP.Page page)
        {
            BinaryData sharedDataBlobBinaryData = page.PersonalizationSettings;
            if (sharedDataBlobBinaryData == null)
            {
                Console.WriteLine(String.Format("{0} has no personalization data.", page.Path));
                return;
            }

            Console.WriteLine(page.Path);

            PageState pageState = GetPageState(sharedDataBlobBinaryData);
            if (pageState == null)
                return;

            pageState.DynamicParts.Remove(portletId);
            pageState.WebPartSettings.Remove(portletId);
            pageState.Locations.Remove(portletId);

            SaveNewPersonalizationSettings(page, pageState);
        }

        // Tools ///////////////////////////////////////////////////////////////////
		private void SaveNewPersonalizationSettings(SNP.Page page, PageState pageState)
        {
            string newSharedDataString = pageState.Encode();
            byte[] newSharedDataBlob = Convert.FromBase64String(newSharedDataString);

            page.PersonalizationSettings.SetStream(new MemoryStream(newSharedDataBlob));
            page.Save();
        }
        private bool IsRecursive(Dictionary<string, string> parameters)
        {
            return parameters.ContainsKey("RECURSIVE");
        }
        private Dictionary<string, string> GetZones(Dictionary<string, LocationInfo> locations)
        {
            if (locations == null)
                throw new ArgumentNullException("locations");

            var zones = new Dictionary<string, string>();
            foreach (string key in locations.Keys)
            {
                LocationInfo locationInfo = locations[key];
                if (!zones.ContainsKey(locationInfo.ZoneID))
                    zones.Add(locationInfo.ZoneID, locationInfo.ZoneID);
            }
            return zones;
        }
        private IEnumerable<Node> GetPageList(string pagePath, bool isRecursive)
        {
            var query = new NodeQuery();
            query.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith, pagePath));
            if (isRecursive)
                query.Add(new TypeExpression(ActiveSchema.NodeTypes["Page"]));
            else
                query.Add(new TypeExpression(ActiveSchema.NodeTypes["Page"], true));

            return query.Execute().Nodes;
        }
        private PageState GetPageState(BinaryData binaryData)
        {
            if (binaryData == null)
                throw new ArgumentNullException("binaryData");

            PageState resultPageState = null;
            Stream sharedDataBlobStream = binaryData.GetStream();
            int streamLength = Convert.ToInt32(sharedDataBlobStream.Length);
            var byteContent = new byte[streamLength];

            //try
            //{
            sharedDataBlobStream.Read(byteContent, 0, streamLength);
            resultPageState = new PageState(byteContent, PersonalizationScope.Shared);
            //}
            //catch (Exception exc)
            //{
            //    WriteException(exc);
            //}

            return resultPageState;
        }

        
    }
}