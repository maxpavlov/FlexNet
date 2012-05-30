using System;
using System.Collections.Generic;
using System.Text;
using SenseNet.ContentRepository.Storage;
using System.IO;
using System.Xml;
using SenseNet.ContentRepository.Storage.Search;

using SenseNet.Portal.UI;
using SenseNet.ContentRepository;

namespace SenseNet.Portal
{
    public	class PageTemplateManager
	{
		#region Variables

		private MasterPage _masterPage;
		private PageTemplate _pageTemplate;
		private MemoryStream _oldStream;
		private bool _isZoneChange;

		private const string ASPXCONTENTTYPE = " ??? ";
		private const string TEMPLATE = "_template";
		private const string TMPPATH = "/Root/Public Site";
		private const string DOUBLEDOT = "__double-dot__";
		private const string HTML = ".html";
		private const string ASPX = ".aspx";
		private const string MASTER = ".Master";
		private const string FORMSTART = "<form id=\"form1\" runat=\"server\">";
        private const string FORMENDADMIN = "<!--- ADMIN STUFF --><div id=\"sndlgToolPanel\" style=\"display:none;\" runat=\"server\"><div id=\"snToolPanel\" runat=\"server\" ><snpe-edit name=\"Editor\"></snpe-edit></div></div></form>";
        //private const string FORMENDADMIN = "<!--- ADMIN STUFF --><div id=\"sndlgToolPanel\" style=\"display:none;\" runat=\"server\"><div id=\"snToolPanel\" runat=\"server\" style=\"height:100%\"><snpe-edit name=\"Editor\"></snpe-edit></div></div></form>";
		private const string FORMEND = "</form>";
        private const string STYLEMANAGER = "<sn:StyleManager ID=\"StyleManager1\" runat=\"server\" />";
        //private const string SCRIPTMANAGER = @"<asp:AjaxScriptManager ID=""ScriptManager1"" runat=""server"" ScriptMode=""{0}"" EnablePartialRendering=""true"" LoadScriptsBeforeUI=""true"" />";
        private const string SCRIPTMANAGER = @"<sn:SNScriptManager ID=""ScriptManager1"" ScriptMode=""{0}"" EnablePartialRendering=""true"" LoadScriptsBeforeUI=""true"" runat=""server"" />";
        private const string WEBPARTMANAGER = "<asp:WebPartManager ID=\"wpm\" runat=\"server\" />";
		private const string WEBPARTZONE = "<asp:Content ID=\"Content_{0}\" ContentPlaceHolderID=\"CP{0}\" runat=\"server\"><asp:WebPartZone ID=\"{0}\" {2} runat=\"server\"><ZoneTemplate>{1}</ZoneTemplate></asp:WebPartZone></asp:Content>";

        //private const string ASPXHEADER = "<%@ Page Language=\"C#\" AutoEventWireup=\"true\" MasterPageFile=\"~{1}{0}\" %>";
        private const string ASPXHEADER = "<%@ Page Language=\"C#\" CompilationMode=\"Never\" MasterPageFile=\"~{1}{0}\" %>";

		private const string MASTERHEADER = "<%@ Master Language=\"C#\" AutoEventWireup=\"true\" %>";
		private const string PLACEHOLDER = "<asp:ContentPlaceHolder ID=\"CP{0}\" runat=\"server\"></asp:ContentPlaceHolder>";
        private const string PAGEEDIT = "<sn:CollapsibleEditorZone ID=\"EditorZone_{0}\" runat=\"server\"><ZoneTemplate></ZoneTemplate></sn:CollapsibleEditorZone>";

		#endregion

		#region Properties

		public PageTemplate PageTemplateNode
		{
			get { return _pageTemplate; }
			set { _pageTemplate = value; }
		}

		private Stream OriginalStream
		{
			get { return _oldStream; }
			set
            {
                if (value==null)
                {
                    _oldStream = null;
                }
                else
                {
                    var tempBuffer = new byte[value.Length];
                    value.Read(tempBuffer, 0, (int)value.Length);
                    _oldStream = new MemoryStream(tempBuffer);
                }
            }
		}

		public string FileName
		{
			get { return PageTemplateNode.Name; }
		}

		#endregion

		#region Static methods

		public static void GetBinaryData(int pageTemplateId, Stream oldStream)
		{
			PageTemplateManager pageTemplateManager = new PageTemplateManager();
			pageTemplateManager.PageTemplateNode = PageTemplate.LoadNode(pageTemplateId) as PageTemplate;
			pageTemplateManager.OriginalStream = oldStream;
			pageTemplateManager.GenerateBinaryData();
		}

		public static BinaryData GetPageBinaryData(Page page, PageTemplate pageTemplate)
		{
			PageTemplateManager pageTemplateManager = new PageTemplateManager();
			return pageTemplateManager.GetASPXBinaryByPageTemplate(page, pageTemplate);	
		}
		
		#endregion

		#region Methods

		private void GenerateBinaryData()
		{
			string pageTmp = string.Empty;
			if (CheckPageTemplateBinaryStream(PageTemplateNode)) pageTmp = Tools.GetStreamString(PageTemplateNode.Binary.GetStream());

			string oldPageTmp = string.Empty;
			if (OriginalStream != null) oldPageTmp = Tools.GetStreamString(OriginalStream);
			
			string fileName = GetFileNameWithoutExt();

			GeneratePage(oldPageTmp, pageTmp, fileName);
		}
		
		private void GeneratePage(string oldPageTmp, string pageTmp, string fileName)
		{
			//-- Master page -----------------------------------------------
			IList<PageZone> zoneList = CreateMaster(oldPageTmp, pageTmp, fileName);

			//-- Pages -----------------------------------------------------
			if (_isZoneChange)
			{
				IEnumerable<Node> nodeList = LoadPageList();
				foreach (Page page in nodeList)
				{
					page.PageTemplateNode = PageTemplateNode;
					page.Save();
				}
			}
		}

		private static string GetPortletXML(IList<PageZone> zoneList)
		{
			StringBuilder portletXML = new StringBuilder();
			
			portletXML.Append("<?xml version='1.0' encoding='utf-8'?><Page.PortletInfo>");

			bool hasPortlet = false;
			foreach (PageZone pageZone in zoneList)
			{
				if (!string.IsNullOrEmpty(pageZone.InnerText))
				{
					portletXML.Append(string.Concat("<Zone name='", pageZone.Name, "'>"));
					portletXML.Append(pageZone.InnerText);
					portletXML.Append("</Zone>");

					hasPortlet = true;
				}
			}

			portletXML.Append("</Page.PortletInfo>");

			return hasPortlet ? portletXML.ToString() : string.Empty;
		}

		#region ASPX

		const string zoneStartElement = "<snpe-zone";
		const string zoneEndElement = "</snpe-zone>";

		const string editStartElement = "<snpe-edit";
		const string editEndElement = "</snpe-edit>";

		const string catalogStartElement = "<snpe-catalog";
		const string catalogEndElement = "</snpe-catalog>";

		private IList<object> SplitPageTemplate(string pageTmp)
		{
			List<object> list = new List<object>();

			if (!string.IsNullOrEmpty(pageTmp))
			{
				int currentPos = 0;

				string posType = string.Empty;
				int pos = GetSmallest(pageTmp, 0, out posType);

				bool end = pos == -1;
				while (!end)
				{
					if (pos != currentPos)
					{
						list.Add(pageTmp.Substring(currentPos, pos - currentPos));
					}

                    int endPos = GetZoneEndPos(pageTmp, pos, posType);
					if (endPos != -1)
					{
						string zoneString = pageTmp.Substring(pos, endPos - pos);

						XmlDocument doc = new XmlDocument();
						doc.LoadXml(zoneString.Replace(":", DOUBLEDOT));
						string name = doc.DocumentElement.Attributes["name"].Value;
						string innerText = doc.DocumentElement.InnerXml.ToString().Replace(DOUBLEDOT, ":");

						list.Add(GetNewElement(posType, name, innerText, GetAttributeString(doc.DocumentElement.Attributes)));

						currentPos = endPos;
					}
					else
					{
						throw new Exception("Invalid template.");
					}

					posType = string.Empty;
					pos = GetSmallest(pageTmp, currentPos, out posType);

					if (pos == -1)
					{
						end = true;
					}
				}
				if (pageTmp.Length != currentPos)
				{
					list.Add(pageTmp.Substring(currentPos));
				}
			}
			return list;
		}
        private int GetZoneEndPos(string pageTmp, int startPos, string posType)
        {
            var p0 = startPos + 1;
            if(p0 >= pageTmp.Length)
                return -1;

            var p1 = pageTmp.IndexOf("/>", p0);

            if (p1 > p0)
                if (!pageTmp.Substring(p0, p1 - p0).Contains("<"))
                    return p1 + 2;

            var end = GetEnd(posType);
            return pageTmp.IndexOf(end, startPos) + end.Length;
        }

		private static string GetAttributeString(XmlAttributeCollection xmlAttrCol)
		{
			StringBuilder sb = new StringBuilder();
			foreach (XmlAttribute xa in xmlAttrCol)
			{
				sb.Append(string.Concat(xa.Name, "=\"", xa.Value, "\" "));
			}
			return sb.ToString();
		}

		private static object GetNewElement(string posType, string name, string innerText, string attrListText)
		{
			switch (posType)
			{
				case "zone": return new PageZone(name, innerText, attrListText);
				case "edit": return new PageEdit(name);
				case "catalog": return new PageCatalog(name);
				default: return string.Empty;
			}
		}

		private static string GetEnd(string posType)
		{
			switch (posType)
			{
				case "zone": return zoneEndElement;
				case "edit": return editEndElement;
				case "catalog": return catalogEndElement;
				default: return string.Empty;
			}
		}

		private static int GetSmallest(string pageTmp, int pos, out string posType)
		{
			int zonePos = pageTmp.IndexOf(zoneStartElement, pos);
			int editPos = pageTmp.IndexOf(editStartElement, pos);
			int catalogPos = pageTmp.IndexOf(catalogStartElement, pos);

			if (zonePos == -1)
			{
				zonePos = Int32.MaxValue;
			}
			if (editPos == -1)
			{
				editPos = Int32.MaxValue;
			}
			if (catalogPos == -1)
			{
				catalogPos = Int32.MaxValue;
			}

			if (zonePos < editPos && zonePos < catalogPos)
			{
				posType = "zone";
				return zonePos;
			}
			else if (editPos < zonePos && editPos < catalogPos)
			{
				posType = "edit";
				return editPos;
			}
			else if (catalogPos < editPos && catalogPos < zonePos)
			{
				posType = "catalog";
				return catalogPos;
			}
			else
			{
				posType = string.Empty;
				return -1;
			}
		}

		private IEnumerable<Node> LoadPageList()
		{
            if (StorageContext.Search.IsOuterEngineEnabled && StorageContext.Search.SearchEngine != InternalSearchEngine.Instance)
            {
                //this NodeQuery will be compiled to LucQuery because the outer engine is enabled
                NodeQuery query = new NodeQuery();
                query.Add(new TypeExpression(ActiveSchema.NodeTypes[typeof(Page).Name], false));
                query.Add(new ReferenceExpression(ActiveSchema.PropertyTypes["PageTemplateNode"], _pageTemplate));
                return query.Execute().Nodes;
            }
            //we need to execute a direct database query because the outer engine is disabled
            return NodeQuery.QueryNodesByReferenceAndType("PageTemplateNode", this.PageTemplateNode.Id, ActiveSchema.NodeTypes[typeof(Page).Name], false).Nodes;
		}

		#endregion

		#region MASTER

		private IList<PageZone> CreateMaster(string oldTemplate, string template, string fileName)
		{
			template = ChangeHead(template);
			template = AddForm(template);

			IList<object> oldFragments = SplitPageTemplate(oldTemplate);
			IList<object> fragments = SplitPageTemplate(template);

			IList<PageZone> oldZoneList = new List<PageZone>();
			for (int i = 0; i < oldFragments.Count; i++)
			{
				if (oldFragments[i] is PageZone)
				{
					PageZone oldZone = oldFragments[i] as PageZone;
					oldZoneList.Add(oldZone);
				}
			}

			IList<PageZone> zoneList = new List<PageZone>();

			StringBuilder master = new StringBuilder();
			master.Append(MASTERHEADER);

			for (int i = 0; i < fragments.Count; i++)
			{
				if (fragments[i] is PageZone)
				{
					PageZone zone = fragments[i] as PageZone;
					master.AppendFormat(PLACEHOLDER, zone.Name);
					zoneList.Add(zone);
				}
				else if (fragments[i] is PageEdit)
				{
					PageEdit edit = fragments[i] as PageEdit;
					master.AppendFormat(PAGEEDIT, edit.Name);
				}
				else if (fragments[i] is PageCatalog)
				{
					//PageCatalog catalog = fragments[i] as PageCatalog;
					//master.AppendFormat(PAGECATALOG, catalog.Name);
				}
				else
				{
					master.Append((string)fragments[i]);
				}
			}

			_isZoneChange = ZoneChanged(oldZoneList, zoneList);

            //MasterPages will be generated next to PageTemplates with appropriate extension and content type.
            SaveMasterPage(string.Concat(fileName, MASTER), Repository.PageTemplatesFolderPath, master.ToString());

			return zoneList;
		}

		private bool ZoneChanged(IList<PageZone> oldZoneList, IList<PageZone> zoneList)
		{
			if (oldZoneList != null && zoneList != null && oldZoneList.Count != zoneList.Count)
			{
				return true;
			}
			foreach (PageZone pageZone in zoneList)
			{
				if (!ExistPageZone(pageZone, oldZoneList))
				{
					return true;
				}
			}
			return false;
		}

		private static bool ExistPageZone(PageZone pageZone, IList<PageZone> pageZoneList)
		{
			foreach (PageZone pz in pageZoneList)
			{
				if (pageZone.Name == pz.Name && pageZone.AttrListText == pz.AttrListText && pageZone.InnerText == pz.InnerText)
				{
					return true;
				}
			}
			return false;
		}

		private void SaveMasterPage(string fileName, string path, string textData)
		{
			MasterPage masterPage = GetMasterPageByPath(path, fileName);
			if (masterPage != null)
			{
				SaveMasterPage(masterPage, textData);
			}
		}

		private void SaveMasterPage(MasterPage masterPage, string textData)
		{
            BinaryData binaryData = new BinaryData();

            if (masterPage.Binary != null)
            {
                masterPage.Binary.SetStream(Tools.GetStreamFromString(textData));
                masterPage.Binary.FileName = new BinaryFileName(ASPX);
            }
            else
            {
                
                binaryData.SetStream(Tools.GetStreamFromString(textData));
                binaryData.FileName = new BinaryFileName(ASPX);
                masterPage.Binary = binaryData;
            }
            masterPage.Save();
		}

		private MasterPage GetMasterPageByPath(string path, string name)
		{
			var parent = Node.LoadNode(path) as IFolder;
			if (parent != null)
			{
				_masterPage = GetExistNode(parent, name) as MasterPage;
				if (_masterPage == null)
				{
					_masterPage = new MasterPage((Node)parent);
					_masterPage.Name = string.Concat(name);
				}
				return _masterPage;
			}
			else
			{
				return null;
			}
		}

		private static Node GetExistNode(IFolder folder, string name)
		{
            return Node.LoadNode(RepositoryPath.Combine(((Node)folder).Path, name));
		}

		private static string ChangeHead(string tmp)
		{
			int startHead = tmp.IndexOf("<head");

			if (startHead > -1)
			{
				int endHead = GetEndPos(tmp, startHead); //tmp.IndexOf(">", startHead);
				string headTag = tmp.Substring(startHead, endHead - startHead + 1);
				if (headTag.IndexOf("runat=\"server\"") == -1)
				{
					tmp = tmp.Insert(startHead + 5, " runat=\"server\"");
				}
			}
			else
			{
				int startHTML = tmp.IndexOf("<html");

				if (startHTML > -1)
				{
					int endHTML = GetEndPos(tmp, startHTML); //tmp.IndexOf(">", startHTML);
					
					tmp = tmp.Insert(endHTML + 1, "<head runat=\"server\" />");
				}
				else
				{
					throw new Exception("No html tag found!");
				}
			}

			return tmp;
		}

		private static string AddForm(string tmp)
		{
			int start = tmp.ToLower().IndexOf("<body");

			if (start > -1)
			{
                int end = GetEndPos(tmp, start);

                string i = string.Concat(FORMSTART, STYLEMANAGER, String.Format(SCRIPTMANAGER, UITools.ScriptMode), WEBPARTMANAGER);
                tmp = tmp.Insert(end + 1, i);
			}
			else
			{
				throw new Exception("No body tag found!");
			}

			int bodyEnd = tmp.IndexOf("</body>");

			if (bodyEnd > -1)
			{
				if (tmp.IndexOf("id=\"sndlgToolPanel\"") > -1)
				{
					tmp = tmp.Insert(bodyEnd, FORMEND);
				}
				else
				{
					tmp = tmp.Insert(bodyEnd, FORMENDADMIN);
				}
			}
			else
			{
				throw new Exception("No body end tag found!");
			}

			return tmp;
		}

		private static int GetEndPos(string tmp, int start)
		{
			int end = tmp.IndexOf(">", start);
			if(tmp[end-1] == '%')
				return GetEndPos(tmp, end+1);

			return end;
		}

		#endregion

		#region Common

		private string GetFileNameWithoutExt()
		{
			return GetFileNameWithoutExt(FileName);
		}

		private static string GetFileNameWithoutExt(string fileName)
		{
			if(!string.IsNullOrEmpty(fileName))
			{
				int dotIndex = fileName.LastIndexOf(".");
				if (dotIndex > 0)
				{
					return fileName.Substring(0, dotIndex);
				}
			}
			return fileName;
		}

		private static string GetFileNameExtension(string fileName)
		{
			string[] fileNameArray = fileName.Split('.');
			if (fileNameArray != null && fileNameArray.Length > 0)
			{
				return fileNameArray[fileNameArray.Length - 1];
			}
			return string.Empty;
		}

		private static bool CheckPageTemplateBinaryStream(PageTemplate pageTemplate)
		{
			return pageTemplate != null && pageTemplate.Binary != null && pageTemplate.Binary.GetStream() != null;
		}

		#endregion

		#region Page

		public BinaryData GetASPXBinaryByPageTemplate(Page page, PageTemplate pageTemplate)
		{
			if (page == null)
			{
				throw new ArgumentNullException("page");
			}

			if (pageTemplate == null)
			{
				throw new ArgumentNullException("pageTemplate");
			}

			BinaryData binaryData = new BinaryData();
            if (page.Binary != null)
            {
				binaryData = page.Binary;
			}

			string pageTmp = Tools.GetStreamString(pageTemplate.Binary.GetStream());
			IList<object> fragments = SplitPageTemplate(pageTmp);
			StringBuilder aspx = new StringBuilder();

            //MasterPage is inside the PageTemplates folder.
            aspx.AppendFormat(ASPXHEADER, string.Concat("/", GetFileNameWithoutExt(pageTemplate.Name), MASTER), Repository.PageTemplatesFolderPath);
			for (int i = 0; i < fragments.Count; i++)
			{
				if (fragments[i] is PageZone)
				{
					PageZone zone = fragments[i] as PageZone;
					aspx.AppendFormat(WEBPARTZONE, zone.Name, zone.InnerText, zone.AttrListText);
				}
			}

			binaryData.SetStream(Tools.GetStreamFromString(aspx.ToString()));
			binaryData.FileName = new BinaryFileName (ASPX);
			binaryData.ContentType = ASPXCONTENTTYPE;

			return binaryData;
		}

		#endregion

		#endregion
	}
}
