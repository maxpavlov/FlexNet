using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.UI;
using SenseNet.ContentRepository;
using SenseNet.Diagnostics;
using SenseNet.Portal.Exchange;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using System.Reflection;
using SenseNet.Search;
using SenseNet.Workflow;

namespace SenseNet.Portal.Portlets
{
    public class IncomingEmailSettingsPortlet : ContextBoundPortlet
    {
        public IncomingEmailSettingsPortlet()
        {
            this.Name = "Incoming Email Settings";
            this.Description = "Portlet for handling incoming mail settings of Content Lists.";
            this.Category = new PortletCategory(PortletCategoryType.System);
        }

        protected override void CreateChildControls()
        {
            if (this.ContextNode == null)
                return;
            var content = Content.Create(this.ContextNode);
            var cv = ContentView.Create(content, this.Page, ViewMode.InlineEdit, "$skin/contentviews/ContentList/IncomingEmailSettings.ascx");

            // set default mailprocessworkflow
            var emailworkflow = content["IncomingEmailWorkflow"] as IEnumerable<SenseNet.ContentRepository.Storage.Node>;
            if (emailworkflow == null || emailworkflow.Count() == 0)
                content["IncomingEmailWorkflow"] = Node.LoadNode("/Root/System/Schema/ContentTypes/GenericContent/Workflow/MailProcessorWorkflow");

            cv.UserAction += new EventHandler<UserActionEventArgs>(cv_UserAction);
            
            this.Controls.Add(cv);

            this.ChildControlsCreated = true;
        }

        protected void cv_UserAction(object sender, UserActionEventArgs e)
        {
            if (e.ActionName == "Save")
            {
                var contentView = e.ContentView;
                var content = contentView.Content;

                contentView.UpdateContent();

                if (contentView.IsUserInputValid && content.IsValid)
                {
                    try
                    {
                        var additionalTypes = new List<string>();
                        additionalTypes.Add("File");
                        var option = ((IEnumerable<string>)content["GroupAttachments"]).FirstOrDefault();
                        if (option != null)
                        {
                            switch (option)
                            {
                                case "subject":
                                case "sender":
                                    additionalTypes.Add("Folder");
                                    break;
                                case "email":
                                    additionalTypes.Add("Email");
                                    break;
                            }
                        }
                        ((GenericContent)content.ContentHandler).AllowChildTypes(additionalTypes);

                        content.Save();

                        // remove current workflow
                        RemoveWorkflow();

                        // start new workflow + subscription if email is given
                        var newEmail = content["ListEmail"] as string;
                        if (!string.IsNullOrEmpty(newEmail))
                            StartSubscription();

                        CallDone();
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteException(ex);
                        contentView.ContentException = ex;
                    }
                }
                return;
            }
            if (e.ActionName == "Cancel")
            {
                CallDone();
                return;
            }
        }

        private void RemoveWorkflow()
        {
            // check if workflow is currently running
            var targetPath = RepositoryPath.Combine(this.ContextNode.Path, "Workflows/MailProcess");
            var queryStr = "+TypeIs:MailProcessorWorkflow +InFolder:\"" + targetPath + "\" +WorkflowStatus:" + ((int)WorkflowStatusEnum.Running).ToString();
            var settings = new QuerySettings { EnableAutofilters = false };
            var runningWorkflow = ContentQuery.Query(queryStr, settings);
            if (runningWorkflow.Count > 0)
            {
                using (new SystemAccount())
                {
                    foreach (var wfnode in runningWorkflow.Nodes)
                    {
                        // abort and delete workflow
                        var wf = wfnode as WorkflowHandlerBase;
                        wf.ForceDelete();
                    }
                }
            }
        }

        private void StartSubscription()
        {
            if (Exchange.Configuration.SubscribeToPushNotifications)
            {
                // subscribe to email after saving content. this is done separately from saving the content, 
                // since subscriptionid must be persisted on the content and we use cyclic retrials for that
                ExchangeHelper.Subscribe(this.ContextNode);
            }

            var contextNode = this.ContextNode;
            var parent = GetParent(contextNode);
            if (parent == null)
                return;

            // start workflow
            var incomingEmailWorkflow = contextNode.GetReference<Node>("IncomingEmailWorkflow");
            if (incomingEmailWorkflow == null)
                return;

            var workflowC = Content.CreateNew(incomingEmailWorkflow.Name, parent, incomingEmailWorkflow.Name);
            workflowC["RelatedContent"] = contextNode;

            using (new SystemAccount())
            {
                try
                {
                    workflowC.Save();
                }
                catch (Exception ex)
                {
                    Logger.WriteException(ex, ExchangeHelper.ExchangeLogCategory);
                }
            }

            var t = TypeHandler.GetType("SenseNet.Workflow.InstanceManager");
            if (t != null)
            {
                var m = t.GetMethod("Start", BindingFlags.Static | BindingFlags.Public);
                m.Invoke(null, new object[] { workflowC.ContentHandler });
            }
        }

        private static Node GetParent(Node contextNode)
        {
            var parent = Node.LoadNode(RepositoryPath.Combine(contextNode.Path, "Workflows/MailProcess"));
            if (parent == null)
            {
                var workflows = Node.LoadNode(RepositoryPath.Combine(contextNode.Path, "Workflows"));
                if (workflows == null)
                {
                    using (new SystemAccount())
                    {
                        workflows = new SystemFolder(contextNode);
                        workflows.Name = "Workflows";
                        try
                        {
                            workflows.Save();
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteException(ex, ExchangeHelper.ExchangeLogCategory);
                            return null;
                        }
                    }
                }
                using (new SystemAccount())
                {
                    parent = new SenseNet.ContentRepository.Folder(workflows);
                    parent.Name = "MailProcess";
                    try
                    {
                        parent.Save();
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteException(ex, ExchangeHelper.ExchangeLogCategory);
                        return null;
                    }
                }
            }
            return parent;
        }
    }
}