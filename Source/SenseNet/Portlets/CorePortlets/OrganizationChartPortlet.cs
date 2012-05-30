using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
using SenseNet.Portal.UI.PortletFramework;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Diagnostics;
using System.Web.UI;
using System.Xml.XPath;
using System.IO;
using SenseNet.ContentRepository;
using System.Web.UI.WebControls;
using Content = SenseNet.ContentRepository.Content;
using File = System.IO.File;
using SenseNet.ContentRepository.i18n;

namespace SenseNet.Portal.Portlets
{
    public class OrganizationChartPortlet : CacheablePortlet
    {
        private string _firstManager = "/Root/IMS/BuiltIn/Portal/Administrator";

        /// <summary>
        /// Gets or sets the first manager.
        /// </summary>
        /// <value>The first manager.</value>
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory("OrganizationChart", "CategoryTitle", 5), WebOrder(20)]
        [LocalizedWebDisplayName("OrganizationChart", "FirstManagerTitle"), LocalizedWebDescription("OrganizationChart", "FirstManagerDescription")]
        public string FirstManager
        {
            get { return _firstManager; }
            set { _firstManager = value; }
        }

        private int _depthLimit = 2;

        /// <summary>
        /// Gets or sets the depth limit.
        /// </summary>
        /// <value>The depth limit.</value>
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory("OrganizationChart", "CategoryTitle", 5), WebOrder(30)]
        [LocalizedWebDisplayName("OrganizationChart", "DepthLimitTitle"), LocalizedWebDescription("OrganizationChart", "DepthLimitDescription")]
        public int DepthLimit
        {
            get { return _depthLimit; }
            set { _depthLimit = value; }
        }

        private List<int> _usedNodeId;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationChartPortlet"/> class.
        /// </summary>
        public OrganizationChartPortlet()
        {
            this.Name = SenseNetResourceManager.Current.GetString("OrganizationChart", "PortletTitle");
            this.Description = SenseNetResourceManager.Current.GetString("OrganizationChart", "PortletDescription");
            this.Category = new PortletCategory(PortletCategoryType.Collection);
            this.Renderer = "/Root/System/SystemPlugins/Portlets/OrganizationChart/OrgChartView.xslt";
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <returns></returns>
        protected override object GetModel()
        {
            //var manager = Content.Load("/Root/IMS/BuiltIn/Portal/Administrator");

            var manager = Content.Load(FirstManager);

            if (manager == null)
            {
                throw new NotSupportedException(SenseNetResourceManager.Current.GetString("OrganizationChart", "NoSuchManagerError"));
            }
            
            
            var managerStream = manager.GetXml(true);
            var resultXml = new XmlDocument();
            resultXml.Load(managerStream);

            _usedNodeId = new List<int>();
            _usedNodeId.Add(manager.Id);

            try
            {
                GetEmployees(manager, resultXml.SelectSingleNode("/Content"), 1);
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                throw new NotSupportedException(ex.Message); 
            }
            
            return resultXml;
        }

        /// <summary>
        /// Gets the employees.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="container">The container.</param>
        /// <param name="depth">The depth.</param>
        private void GetEmployees(Content manager, XmlNode container, int depth)
        {
            if(depth>DepthLimit)
                return;

            var employeesNode = container.OwnerDocument.CreateElement("Employees");
            container.AppendChild(employeesNode);

            var query = SenseNet.Search.LucQuery.Parse(string.Format("+Manager:{0}", manager.Id));
            var result = query.Execute();
            
            foreach (var lucObject in result)
            {
                if (!_usedNodeId.Contains(lucObject.NodeId))
                    _usedNodeId.Add(lucObject.NodeId);
                else
                    throw new NotSupportedException(SenseNetResourceManager.Current.GetString("OrganizationChart", "CircularReferenceError"));

                var employee = Content.Load(lucObject.NodeId);
                var employeeStream = employee.GetXml(false);
                var employeeXml = new XmlDocument();
                employeeXml.Load(employeeStream);

                var node = employeesNode.OwnerDocument.ImportNode(employeeXml.DocumentElement,true);

                employeesNode.AppendChild(node);
                var employeeNode = employeesNode.SelectSingleNode(string.Format("Content[SelfLink='{0}']", employee.Path));
                
                GetEmployees(employee, employeeNode, depth + 1);
            }
        }

    }
}
