using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web.Compilation;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Diagnostics;
using SenseNet.Search;

namespace SenseNet.Portal
{
    public class WarmUp // : IProcessHostPreloadClient
    {
        //============================================================ Properties

        #region Type names for preload
        private static string[] _typesToPreloadByName = new[]
                                                            {
                                                               "SenseNet.ContentRepository.Storage.Events.NodeObserver",
"SenseNet.ContentRepository.Field",
"SenseNet.ContentRepository.Schema.FieldSetting",
"SenseNet.Search.Parser.LucQueryTemplateReplacer",
"SenseNet.ContentRepository.TemplateReplacerBase",
"SenseNet.Portal.PortletTemplateReplacer",
"SenseNet.Portal.UI.Controls.FieldControl",
"SenseNet.ContentRepository.Storage.ISnService",
"SenseNet.ContentRepository.Storage.Search.IIndexDocumentProvider",
"SenseNet.ContentRepository.Storage.Scripting.IEvaluator",
"SenseNet.ContentRepository.Security.UserAccessProvider",
"SenseNet.ContentRepository.Schema.ContentType",
"SenseNet.Search.Indexing.ExclusiveTypeIndexHandler",
"SenseNet.Search.Indexing.TypeTreeIndexHandler",
"Lucene.Net.Analysis.KeywordAnalyzer",
"SenseNet.Search.Indexing.DepthIndexHandler",
"SenseNet.Search.Indexing.InTreeIndexHandler",
"SenseNet.Search.Indexing.InFolderIndexHandler",
"Lucene.Net.Analysis.Standard.StandardAnalyzer",
"SenseNet.Search.Indexing.SystemContentIndexHandler",
"SenseNet.Search.Indexing.TagIndexHandler",
"SenseNet.ContentRepository.GenericContent",
"SenseNet.ApplicationModel.IncludeBackUrlMode",
"SenseNet.ApplicationModel.Application",
"SenseNet.Portal.Handlers.BackupIndexHandler",
"SenseNet.Portal.UI.Controls.Captcha.CaptchaImageApplication",
"SenseNet.Services.ExportToCsvApplication",
"SenseNet.ContentRepository.HttpEndpointDemoContent",
"SenseNet.Portal.AppModel.HttpStatusApplication",
"SenseNet.Portal.ApplicationModel.ImgResizeApplication",
"SenseNet.Services.RssApplication",
"Lucene.Net.Analysis.WhitespaceAnalyzer",
"SenseNet.Portal.Page",
"SenseNet.Portal.Handlers.XsltApplication",
"SenseNet.ContentRepository.ContentLink",
"SenseNet.ContentRepository.Schema.FieldSettingContent",
"SenseNet.ContentRepository.File",
"SenseNet.ContentRepository.Image",
"SenseNet.ContentRepository.ApplicationCacheFile",
"SenseNet.Portal.MasterPage",
"SenseNet.Portal.PageTemplate",
"SenseNet.ContentRepository.i18n.Resource",
"SenseNet.Portal.UI.ContentListViews.Handlers.ViewBase",
"SenseNet.Portal.UI.ContentListViews.Handlers.ListView",
"SenseNet.Workflow.WorkflowDefinitionHandler",
"SenseNet.ContentRepository.Folder",
"SenseNet.ContentRepository.Security.ADSync.ADFolder",
"SenseNet.ContentRepository.ContentList",
"SenseNet.Portal.Portlets.ContentHandlers.Form",
"SenseNet.ContentRepository.Survey",
"SenseNet.ContentRepository.Voting",
"SenseNet.ApplicationModel.Device",
"SenseNet.ContentRepository.Domain",
"SenseNet.ContentRepository.ExpenseClaim",
"SenseNet.ContentRepository.KPIDatasource",
"SenseNet.ContentRepository.OrganizationalUnit",
"SenseNet.ContentRepository.PortalRoot",
"SenseNet.ContentRepository.RuntimeContentContainer",
"SenseNet.ContentRepository.SmartFolder",
"SenseNet.ContentRepository.ContentRotator",
"SenseNet.ContentRepository.SystemFolder",
"SenseNet.ContentRepository.TrashBag",
"SenseNet.ContentRepository.Workspaces.Workspace",
"SenseNet.Portal.Site",
"SenseNet.ContentRepository.TrashBin",
"SenseNet.ContentRepository.UserProfile",
"SenseNet.ContentRepository.Group",
"SenseNet.Portal.BlogPost",
"SenseNet.ContentRepository.CalendarEvent",
"SenseNet.Portal.Portlets.ContentHandlers.FormItem",
"SenseNet.Portal.Portlets.ContentHandlers.EventRegistrationFormItem",
"SenseNet.Portal.DiscussionForum.ForumEntry",
"SenseNet.ContentRepository.SurveyItem",
"SenseNet.ContentRepository.Task",
"SenseNet.ContentRepository.VotingItem",
"SenseNet.Messaging.NotificationConfig",
"SenseNet.ContentRepository.User",
"SenseNet.Search.Indexing.WikiReferencedTitlesIndexHandler",
"SenseNet.Portal.WikiArticle",
"SenseNet.Workflow.WorkflowStatusEnum",
"SenseNet.Workflow.WorkflowHandlerBase",
"SenseNet.Workflow.ApprovalWorkflow",
"SenseNet.Workflow.RegistrationWorkflow",
"SenseNet.Portal.Workspaces.JournalNode",
"SenseNet.Workflow.InstanceManager",
"SenseNet.Portal.UI.PathTools",
"SenseNet.Portal.Portlets.ContentListPortlet",
"SenseNet.Portal.UI.Controls.DisplayName",
"SenseNet.Portal.UI.PortletFramework.DisplayName",
"SenseNet.Portal.UI.ContentListViews.DisplayName",
"SenseNet.Portal.UI.ContentListViews.FieldControls.DisplayName",
"SenseNet.Portal.UI.Controls.Name",
"SenseNet.Portal.UI.PortletFramework.Name",
"SenseNet.Portal.UI.ContentListViews.Name",
"SenseNet.Portal.UI.ContentListViews.FieldControls.Name",
"SenseNet.Portal.UI.Controls.RichText",
"SenseNet.Portal.UI.PortletFramework.RichText",
"SenseNet.Portal.UI.ContentListViews.RichText",
"SenseNet.Portal.UI.ContentListViews.FieldControls.RichText",
"SenseNet.ContentRepository.Schema.OutputMethod",
"SenseNet.ContentRepository.Schema.FieldVisibility",
"SenseNet.ContentRepository.Fields.TextType",
"SenseNet.ContentRepository.Fields.DateTimeMode",
"SenseNet.Portal.UI.Controls.ShortText",
"SenseNet.Portal.UI.PortletFramework.ShortText",
"SenseNet.Portal.UI.ContentListViews.ShortText",
"SenseNet.Portal.UI.ContentListViews.FieldControls.ShortText",
"SenseNet.ContentRepository.Fields.UrlFormat",
"SenseNet.Portal.UI.Controls.HyperLink",
"SenseNet.Portal.UI.PortletFramework.HyperLink",
"SenseNet.Portal.UI.ContentListViews.HyperLink",
"SenseNet.Portal.UI.ContentListViews.FieldControls.HyperLink",
"SenseNet.ContentRepository.Fields.DisplayChoice",
"SenseNet.Portal.UI.Controls.ColumnSelector",
"SenseNet.Portal.UI.PortletFramework.ColumnSelector",
"SenseNet.Portal.UI.ContentListViews.ColumnSelector",
"SenseNet.Portal.UI.ContentListViews.FieldControls.ColumnSelector",
"SenseNet.Portal.UI.Controls.SortingEditor",
"SenseNet.Portal.UI.PortletFramework.SortingEditor",
"SenseNet.Portal.UI.ContentListViews.SortingEditor",
"SenseNet.Portal.UI.ContentListViews.FieldControls.SortingEditor",
"SenseNet.Portal.UI.Controls.GroupingEditor",
"SenseNet.Portal.UI.PortletFramework.GroupingEditor",
"SenseNet.Portal.UI.ContentListViews.GroupingEditor",
"SenseNet.Portal.UI.ContentListViews.FieldControls.GroupingEditor",
"SenseNet.Portal.UI.Controls.VersioningModeChoice",
"SenseNet.Portal.UI.PortletFramework.VersioningModeChoice",
"SenseNet.Portal.UI.ContentListViews.VersioningModeChoice",
"SenseNet.Portal.UI.ContentListViews.FieldControls.VersioningModeChoice",
"SenseNet.Portal.UI.Controls.ApprovingModeChoice",
"SenseNet.Portal.UI.PortletFramework.ApprovingModeChoice",
"SenseNet.Portal.UI.ContentListViews.ApprovingModeChoice",
"SenseNet.Portal.UI.ContentListViews.FieldControls.ApprovingModeChoice",
"SenseNet.Portal.UI.Controls.SiteRelativeUrl",
"SenseNet.Portal.UI.PortletFramework.SiteRelativeUrl",
"SenseNet.Portal.UI.ContentListViews.SiteRelativeUrl",
"SenseNet.Portal.UI.ContentListViews.FieldControls.SiteRelativeUrl",
"SenseNet.Portal.Portlets.SingleContentPortlet",
"SenseNet.Portal.UI.ContentListViews.ListHelper",
"SenseNet.Portal.UI.Controls.EducationEditor",
"SenseNet.Portal.UI.PortletFramework.EducationEditor",
"SenseNet.Portal.UI.ContentListViews.EducationEditor",
"SenseNet.Portal.UI.ContentListViews.FieldControls.EducationEditor"
                                                            };

        private static string[] _typesToPreloadByBase = new[]
                                                            {
"SenseNet.ContentRepository.Storage.Events.NodeObserver",
"SenseNet.ContentRepository.Field",
"SenseNet.ContentRepository.Schema.FieldSetting",
"SenseNet.Search.Parser.LucQueryTemplateReplacer",
"SenseNet.ContentRepository.TemplateReplacerBase",
"SenseNet.Portal.PortletTemplateReplacer",
"SenseNet.Portal.UI.Controls.FieldControl" 
                                                            };

        private static string[] _typesToPreloadByInterface = new[]
                                                            {
"SenseNet.ContentRepository.Storage.ISnService",
"SenseNet.ContentRepository.Storage.Search.IIndexDocumentProvider",
"SenseNet.ContentRepository.Storage.Scripting.IEvaluator"
                                                            };

        #endregion

        private static IEnumerable<string> TypesToPreloadByName
        {
            get { return _typesToPreloadByName; }
        }

        private static IEnumerable<string> TypesToPreloadByBase
        {
            get { return _typesToPreloadByBase; }
        }

        private static IEnumerable<string> TypesToPreloadByInterface
        {
            get { return _typesToPreloadByInterface; }
        }

        //============================================================ Interface

        public static void Preload()
        {
            if (!Repository.WarmupEnabled)
            {
                Logger.WriteInformation("***** Warmup is not enabled, skipped.");
                return;
            }

            //types
            ThreadPool.QueueUserWorkItem(delegate { PreloadTypes(); });
            
            //template replacers and resolvers
            ThreadPool.QueueUserWorkItem(delegate { TemplateManager.Init(); });
            ThreadPool.QueueUserWorkItem(delegate { NodeQuery.InitTemplateResolvers(); });

            //jscript evaluator
            ThreadPool.QueueUserWorkItem(delegate { JscriptEvaluator.Init(); });

            //xslt
            ThreadPool.QueueUserWorkItem(delegate { PreloadXslt(); });

            //content templates
            ThreadPool.QueueUserWorkItem(delegate { PreloadContentTemplates(); });

            //preload controls
            ThreadPool.QueueUserWorkItem(delegate { PreloadControls(); });

            //preload security items
            ThreadPool.QueueUserWorkItem(delegate { PreloadSecurity(); });
        }

        //============================================================ Helper methods

        private static void PreloadTypes()
        {
            using (var optrace = new OperationTrace("PreloadTypes"))
            {
                try
                {
                    //preload types by name
                    foreach (var typeName in TypesToPreloadByName)
                    {
                        TypeHandler.GetType(typeName);
                    }

                    //preload types by base
                    foreach (var typeName in TypesToPreloadByBase)
                    {
                        TypeHandler.GetTypesByBaseType(TypeHandler.GetType(typeName));
                    }

                    //preload types by interface
                    foreach (var typeName in TypesToPreloadByInterface)
                    {
                        TypeHandler.GetTypesByInterface(TypeHandler.GetType(typeName));
                    }

                    optrace.IsSuccessful = true;
                }
                catch (Exception ex)
                {
                    Logger.WriteException(ex);
                }
            }
        }

        private static void PreloadControls()
        {
            try
            {
                QueryResult controlResult;
                var cc = 0;

                var timer = new Stopwatch();
                timer.Start();

                using (new SystemAccount())
                {
                    var query = ContentQuery.CreateQuery("+Name:'*.ascx' -InTree:'/Root/Global/celltemplates' -Path:'/Root/Global/renderers/MyDataboundView.ascx' .SORT:Path",
                        new QuerySettings { EnableAutofilters = false });
                    if (!string.IsNullOrEmpty(Repository.WarmupControlQueryFilter))
                        query.AddClause(Repository.WarmupControlQueryFilter);
                    
                    controlResult = query.Execute();

                    foreach (var controlId in controlResult.Identifiers)
                    {
                        var head = NodeHead.Get(controlId);
                        try
                        {
                            if (head != null)
                            {
                                var pct = BuildManager.GetCompiledType(head.Path);

                                //if (pct != null)
                                //    Trace.WriteLine(">>>>Precompiled control: " + pct.FullName);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteException(new Exception("Error during control load: " + (head == null ? controlId.ToString() : head.Path), ex));
                            //Trace.WriteLine(">>>>Precompiled error during control load: " + (head == null ? controlId.ToString() : head.Path) + " ERROR: " + ex);
                        }

                        cc++;
                    }
                }

                timer.Stop();

                Logger.WriteInformation(string.Format("***** Control preload time: {0} ******* Count: {1} ({2})", timer.Elapsed, cc, controlResult.Count));
                //Trace.WriteLine(string.Format(">>>>Precompiled preload time: {0} ******* Count: {1} ({2})", timer.Elapsed, cc, controlResult.Count));
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }
        }

        private static void PreloadXslt()
        {
            try
            {
                QueryResult queryResult;
                var cc = 0;

                var timer = new Stopwatch();
                timer.Start();

                using (new SystemAccount())
                {
                    queryResult = ContentQuery.Query("+Name:'*.xslt' .SORT:Path", new QuerySettings { EnableAutofilters = false });

                    foreach (var nodeId in queryResult.Identifiers)
                    {
                        var head = NodeHead.Get(nodeId);
                        try
                        {
                            if (head != null)
                            {
                                var xslt = UI.PortletFramework.Xslt.GetXslt(head.Path, true);
                                //Trace.WriteLine(">>>>Preload (xslt): " + head.Path);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteException(new Exception("Error during xlst load: " + (head == null ? nodeId.ToString() : head.Path), ex));
                            //Trace.WriteLine(">>>>Precompiled error during control load: " + (head == null ? nodeId.ToString() : head.Path) + " ERROR: " + ex);
                        }

                        cc++;
                    }
                }

                timer.Stop();

                Logger.WriteInformation(string.Format("***** XSLT preload time: {0} ******* Count: {1} ({2})", timer.Elapsed, cc, queryResult.Count));
                //Trace.WriteLine(string.Format(">>>>Preload XSLT preload time: {0} ******* Count: {1} ({2})", timer.Elapsed, cc, queryResult.Count));
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }
        }

        private static void PreloadContentTemplates()
        {
            try
            {
                QueryResult queryResult;

                var timer = new Stopwatch();
                timer.Start();

                using (new SystemAccount())
                {
                    queryResult = ContentQuery.Query(
                        string.Format("+InTree:\"{0}\" +Depth:{1}", 
                            Repository.ContentTemplateFolderPath,
                            RepositoryPath.GetDepth(Repository.ContentTemplateFolderPath) + 2),
                        new QuerySettings { EnableAutofilters = false });

                    var templates = queryResult.Nodes.ToList();
                }

                timer.Stop();

                Logger.WriteInformation(string.Format("***** Content template preload time: {0} ******* Count: {1}", timer.Elapsed, queryResult.Count));
                //Trace.WriteLine(string.Format(">>>>Preload: Content template preload time: {0} ******* Count: {1}", timer.Elapsed, queryResult.Count));
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }
        }

        private static void PreloadSecurity()
        {
            try
            {
                var timer = new Stopwatch();
                timer.Start();

                //preload special groups
                var g1 = Group.Everyone;
                var g2 = Group.Administrators;
                var g3 = Group.LastModifiers;
                var g4 = Group.Creators;

                timer.Stop();

                Logger.WriteInformation(string.Format("***** Security preload time: {0} *******", timer.Elapsed));
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }
        }
    }
}
