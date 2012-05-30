using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Activities.DurableInstancing;
using System.Xml.Linq;
using System.Runtime.DurableInstancing;
using SenseNet.Diagnostics;
using System.Diagnostics;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;
using System.Reflection;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository;
//using System.Configuration;

namespace SenseNet.Workflow
{
    public enum WorkflowApplicationCreationPurpose { StartNew, Resume, Poll, Abort };
    public enum WorkflowApplicationAbortReason { ManuallyAborted, StateContentDeleted, RelatedContentChanged, RelatedContentDeleted };

    public static class InstanceManager
    {
        private const string STATECONTENT = "StateContent";
        private const double MINPOLLINTERVAL = 2000.0;

        public static void StartWorkflowSystem() { }

        //=========================================================================================================== Polling

        static System.Timers.Timer _pollTimer;
        static InstanceManager()
        {
            var pollInterval = Configuration.TimerInterval * 60.0 * 1000.0;

            if (pollInterval >= MINPOLLINTERVAL)
            {
                _pollTimer = new System.Timers.Timer(pollInterval);
                _pollTimer.Elapsed += new System.Timers.ElapsedEventHandler(PollTimerElapsed);
                _pollTimer.Disposed += new EventHandler(PollTimerDisposed);
                _pollTimer.Enabled = true;
                Logger.WriteInformation("Starting polling timer. Interval in minutes: " + Configuration.TimerInterval);
            }
            else
            {
                Logger.WriteWarning(String.Format("Polling timer was not started because the configured interval ({0}) is less than acceptable minimum ({1}). Interval in minutes: ",
                    Configuration.TimerInterval, MINPOLLINTERVAL));
            }
        }
        private static void PollTimerDisposed(object sender, EventArgs e)
        {
            _pollTimer.Elapsed -= new System.Timers.ElapsedEventHandler(PollTimerElapsed);
            _pollTimer.Disposed -= new EventHandler(PollTimerDisposed);
        }
        private static void PollTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _pollTimer.Enabled = false;
            try
            {
                foreach (var item in GetPollingInstances())
                    ExecuteDelays(item);
            }
            finally
            {
                _pollTimer.Enabled = true;
            }
        }
        public static IEnumerable<WorkflowHandlerBase> GetPollingInstances()
        {
            var query = String.Format("WorkflowStatus:{0} .AUTOFILTERS:OFF", (int)WorkflowStatusEnum.Running);
            var result = SenseNet.Search.ContentQuery.Query(query);
            var instances = new Dictionary<string, WorkflowHandlerBase>();
            foreach (WorkflowHandlerBase item in result.Nodes)
            {
                var key = String.Format("{0}-{1}", item.WorkflowTypeName, item.WorkflowDefinitionVersion);
                if (!instances.ContainsKey(key))
                    instances.Add(key, item);
            }
            Logger.WriteVerbose("Trying execute active workflows", Logger.EmptyCategoryList, 
                new Dictionary<string, object> { { "ResultCount", result.Count }, { "PollingItems", instances.Count }, });
Debug.WriteLine("##WF> Trying execute active workflows: " + instances.Count);

            return instances.Values.ToArray();
        }
        public static void _Poll()
        {
            foreach (var item in GetPollingInstances())
                ExecuteDelays(item);
        }

        //=========================================================================================================== Building

        private static string ConnectionString { get { return RepositoryConfiguration.ConnectionString; } }
        private static WorkflowDataClassesDataContext GetDataContext()
        {
            return new WorkflowDataClassesDataContext(ConnectionString);
        }

        private static WorkflowApplication CreateWorkflowApplication(WorkflowHandlerBase workflowInstance, WorkflowApplicationCreationPurpose purpose,
            IDictionary<string, object> parameters)
        {
            string version;
            WorkflowApplication wfApp = null;
            var workflow = workflowInstance.CreateWorkflowInstance(out version);
            switch (purpose)
            {
                case WorkflowApplicationCreationPurpose.StartNew:
                    Dictionary<string, object> arguments = workflowInstance.CreateParameters();
                    arguments.Add(STATECONTENT, new WfContent(workflowInstance));
                    if (parameters != null)
                        foreach (var item in parameters)
                            arguments.Add(item.Key, item.Value);
                    wfApp = new WorkflowApplication(workflow, arguments);
                    workflowInstance.WorkflowDefinitionVersion = version;
                    workflowInstance.WorkflowInstanceGuid = wfApp.Id.ToString();
                    break;
                default:
                    wfApp = new WorkflowApplication(workflow);
                    break;
            }

            var store = CreateInstanceStore(workflowInstance);
            Dictionary<XName, object> wfScope = new Dictionary<XName, object>
            {
                { GetWorkflowHostTypePropertyName(), GetWorkflowHostTypeName(workflowInstance) }
            };
            wfApp.InstanceStore = store;
            wfApp.AddInitialInstanceValues(wfScope);

            wfApp.PersistableIdle = a => { Debug.WriteLine("##WF> Pidle"); return PersistableIdleAction.Unload; };
            wfApp.Unloaded = b => { Debug.WriteLine("##WF> Unload"); };
            wfApp.Completed = OnWorkflowCompleted;
            wfApp.Aborted = OnWorkflowAborted;
            wfApp.OnUnhandledException = HandleError;

            wfApp.Extensions.Add(new ContentWorkflowExtension() { WorkflowInstancePath = workflowInstance.Path });
            return wfApp;

        }
        private static SqlWorkflowInstanceStore CreateInstanceStore(WorkflowHandlerBase workflowInstance)
        {
            var store = new SqlWorkflowInstanceStore(ConnectionString);
            var ownerHandle = store.CreateInstanceHandle();

            var wfHostTypeName = GetWorkflowHostTypeName(workflowInstance);
            var WorkflowHostTypePropertyName = GetWorkflowHostTypePropertyName();

            var ownerCommand =
                new CreateWorkflowOwnerCommand() { InstanceOwnerMetadata = { { WorkflowHostTypePropertyName, new InstanceValue(wfHostTypeName) } } };

            store.DefaultInstanceOwner = store.Execute(ownerHandle, ownerCommand, TimeSpan.FromSeconds(30)).InstanceOwner;
            return store;
        }
        private static XName GetWorkflowHostTypePropertyName()
        {
            return XNamespace.Get("urn:schemas-microsoft-com:System.Activities/4.0/properties").GetName("WorkflowHostType");
        }
        private static XName GetWorkflowHostTypeName(WorkflowHandlerBase workflowInstance)
        {
            //ZPACE TODO WorkflowHostType
            return XName.Get(workflowInstance.WorkflowHostType, "http://www.sensenet.com/2010/workflow");
        }

        //=========================================================================================================== Operations

        public static Guid Start(WorkflowHandlerBase workflowInstance)
        {
            var wfApp = CreateWorkflowApplication(workflowInstance, WorkflowApplicationCreationPurpose.StartNew, null);
            var id = wfApp.Id;
            workflowInstance.WorkflowStatus = WorkflowStatusEnum.Running;
            workflowInstance.DisableObserver(typeof(WorkflowNotificationObserver));
            using (new SystemAccount())
                workflowInstance.Save();
Debug.WriteLine("##WF> Starting id: " + id);
            wfApp.Run();
            return id;
        }

        public static void Abort(WorkflowHandlerBase workflowInstance, WorkflowApplicationAbortReason reason)
        {
            //check permissions
            if (reason == WorkflowApplicationAbortReason.ManuallyAborted && !workflowInstance.Security.HasPermission(PermissionType.Save))
            {
                Logger.WriteVerbose(String.Concat("InstanceManager cannot abort the instance: ", workflowInstance.Path, ", because the user doesn't have the sufficient permissions (Save)."));
                throw new SenseNetSecurityException(workflowInstance.Path, PermissionType.Save, AccessProvider.Current.GetCurrentUser());
            }

            //abort the workflow
            try
            {
                var wfApp = CreateWorkflowApplication(workflowInstance, WorkflowApplicationCreationPurpose.Abort, null);
                
                wfApp.Load(Guid.Parse(workflowInstance.WorkflowInstanceGuid));
                wfApp.Abort();
            }
            catch(Exception e)
            {
                Logger.WriteVerbose(String.Concat("InstanceManager cannot abort the instance: ", workflowInstance.Path, ". Exception message: ", e.Message));
            }

            //write back workflow state
            WriteBackAbortMessage(workflowInstance, reason);
        }

        public static void ExecuteDelays(WorkflowHandlerBase workflowInstance)
        {
            var abortedList = new List<Guid>();
            while (true)
            {
                var wfApp = CreateWorkflowApplication(workflowInstance, WorkflowApplicationCreationPurpose.Poll, null);
                try
                {
                    wfApp.LoadRunnableInstance(TimeSpan.FromSeconds(1));
                    if (ValidWakedUpWorkflow(wfApp))
                    {
                        wfApp.Run();
Debug.WriteLine("##WF> Delay: EXECUTED");
                    }
                    else
                    {
                        if (!abortedList.Contains(wfApp.Id))
                        {
Debug.WriteLine("##WF> Delay: ABORT from delay");
                            abortedList.Add(wfApp.Id);
                            wfApp.Abort();
                        }
                    }
                }
                catch (InstanceNotReadyException)
                {
Debug.WriteLine("##WF> Delays: no");

                    //TODO: deleting lock after polling. The followings do not work:
                    //wfApp.Cancel();
                    //wfApp.Unload();
                    //wfApp.InstanceStore.DefaultInstanceOwner = null;
                    //wfApp.Terminate("Poll");
                    //wfApp.Abort("Poll");
                    break;
                }
                catch (Exception ex)
                {
Debug.WriteLine("##WF> Delay ERROR: " + ex.Message + " STACK: " + ex.StackTrace);
                    throw;
                }
            }
        }

        public static void FireNotification(WorkflowNotification notification, WorkflowNotificationEventArgs eventArgs)
        {
            var wfInstance = Node.Load<WorkflowHandlerBase>(notification.WorkflowNodePath);
            var wfApp = CreateWorkflowApplication(wfInstance, WorkflowApplicationCreationPurpose.Resume, null);
            wfApp.Load(notification.WorkflowInstanceId);
            //wfApp.ResumeBookmark(notification.BookmarkName, notification.NodeId);
            if (ValidWakedUpWorkflow(wfApp))
            {
                wfApp.ResumeBookmark(notification.BookmarkName, eventArgs);
Debug.WriteLine("##WF> FireNotification: EXECUTING");
            }
            else
            {
                wfApp.Abort();
Debug.WriteLine("##WF> FireNotification: ABORT from FireNotification");
            }
        }
        public static void NotifyContentChanged(WorkflowNotificationEventArgs eventArgs)
        {
            WorkflowNotification[] notifications = null;
            using (var dbContext = GetDataContext())
            {
                notifications = dbContext.WorkflowNotifications.Where(notification =>
                    notification.NodeId == eventArgs.NodeId).ToArray();
            }
            foreach (var notification in notifications)
                InstanceManager.FireNotification(notification, eventArgs);
        }
        public static int RegisterWait(int nodeID, Guid wfInstanceId, string bookMarkName, string wfContentPath)
        {
            using (var dbContext = GetDataContext())
            {
                var notification = new WorkflowNotification()
                {
                    BookmarkName = bookMarkName,
                    NodeId = nodeID,
                    WorkflowInstanceId = wfInstanceId,
                    WorkflowNodePath = wfContentPath
                };
                dbContext.WorkflowNotifications.InsertOnSubmit(notification);
                dbContext.SubmitChanges();
                return notification.NotificationId;
            }
        }
        public static void ReleaseWait(int notificationId)
        {
            using (var dbContext = GetDataContext())
            {
                var ent = dbContext.WorkflowNotifications.SingleOrDefault(wn => wn.NotificationId == notificationId);
                dbContext.WorkflowNotifications.DeleteOnSubmit(ent);
                dbContext.SubmitChanges();
            }
        }

        //=========================================================================================================== Events

        private static void OnWorkflowAborted(WorkflowApplicationAbortedEventArgs args)
        {
            DeleteNotifications(args.InstanceId);
            WriteBackTheState(WorkflowStatusEnum.Aborted, args.InstanceId);

            // also write back abort message, if it is not yet given
            var stateContent = GetStateContent(args.InstanceId);
            if (stateContent == null)
                return;

            WriteBackAbortMessage(stateContent, DumpException(args.Reason));
        }
        private static void OnWorkflowCompleted(WorkflowApplicationCompletedEventArgs args)
        {
            DeleteNotifications(args.InstanceId);
            WriteBackTheState(WorkflowStatusEnum.Completed, args.InstanceId);
        }
        private static void DeleteNotifications(Guid instanceId)
        {
            using (var dbContext = GetDataContext())
            {
                var notifications = dbContext.WorkflowNotifications.Where(notification =>
                    notification.WorkflowInstanceId == instanceId);

                dbContext.WorkflowNotifications.DeleteAllOnSubmit(notifications);
                dbContext.SubmitChanges();
            }
        }

        private static UnhandledExceptionAction HandleError(WorkflowApplicationUnhandledExceptionEventArgs args)
        {
            try
            {
                Debug.WriteLine("##WF> WFException: " + args.UnhandledException.Message);
                Logger.WriteException(args.UnhandledException);

                WorkflowHandlerBase stateContent = GetStateContent(args);
                if (stateContent == null)
                    Logger.WriteWarning("The workflow InstanceManager cannot write back the aborting/terminating reason into the workflow state content.");
                else
                    WriteBackAbortMessage(stateContent, DumpException(args));
            }
            catch (Exception e)
            {
                Debug.WriteLine("##WF> EXCEPTION in the InstanceManager.HandleError: " + args.UnhandledException.Message);
                Logger.WriteException(e);
            }
            return UnhandledExceptionAction.Abort;
        }

        //=========================================================================================================== Tools

        private static bool ValidWakedUpWorkflow(WorkflowApplication wfApp)
        {
            var stateContent = GetStateContent(wfApp.Id);
            if (stateContent == null)
            {
                WriteBackAbortMessage(null, WorkflowApplicationAbortReason.StateContentDeleted);
                return false;
            }

            if (!stateContent.ContentWorkflow)
                return true;

            if (stateContent.RelatedContent == null)
            {
                WriteBackAbortMessage(stateContent, WorkflowApplicationAbortReason.RelatedContentDeleted);
                return false;
            }
            if (stateContent.RelatedContentTimestamp != stateContent.RelatedContent.NodeTimestamp)
            {
                WriteBackAbortMessage(stateContent, WorkflowApplicationAbortReason.RelatedContentChanged);
                return false;
            }
            return true;
        }

        private const string ABORTEDBYUSERMESSAGE = "Aborted manually by the following user: ";
        private static string GetAbortMessage(WorkflowApplicationAbortReason reason, WorkflowHandlerBase workflow)
        {
            switch (reason)
            {
                case WorkflowApplicationAbortReason.ManuallyAborted:
                    return String.Concat(ABORTEDBYUSERMESSAGE, AccessProvider.Current.GetCurrentUser().Username);
                case WorkflowApplicationAbortReason.StateContentDeleted:
                    return "Workflow deleted" + (workflow == null ? "." : (": " + workflow.Path));
                case WorkflowApplicationAbortReason.RelatedContentChanged:
                    return "Aborted because the related content was changed.";
                case WorkflowApplicationAbortReason.RelatedContentDeleted:
                    return "Aborted because the related content was moved or deleted.";
                default:
                    return reason.ToString();
            }
        }
        private static void WriteBackAbortMessage(WorkflowHandlerBase stateContent, WorkflowApplicationAbortReason reason)
        {
            var abortMessage = GetAbortMessage(reason, stateContent);
            if (reason == WorkflowApplicationAbortReason.StateContentDeleted)
                Logger.WriteInformation("Workflow aborted. Reason: " + abortMessage);
            else
                WriteBackAbortMessage(stateContent, abortMessage);
        }
        private static void WriteBackAbortMessage(WorkflowHandlerBase stateContent, string abortMessage)
        {
            var state = stateContent.WorkflowStatus;
            if (state == WorkflowStatusEnum.Completed)
                return;

            // if a system message has already been persisted to the workflow content, don't overwrite it
            if (!string.IsNullOrEmpty(stateContent.SystemMessages))
                return;

            var times = 3;
            while (true)
            {
                try
                {
                    stateContent.SystemMessages = abortMessage;
                    stateContent.DisableObserver(typeof(WorkflowNotificationObserver));
                    using (new SystemAccount())
                        stateContent.Save(SenseNet.ContentRepository.SavingMode.KeepVersion);
                    break;
                }
                catch (NodeIsOutOfDateException ne)
                {
                    if (--times == 0)
                        throw new NodeIsOutOfDateException("Node is out of date after 3 trying", ne);
                    var msg = "InstanceManager: Saving system message caused NodeIsOutOfDateException. Trying again.";
                    Logger.WriteVerbose(msg);
                    Debug.WriteLine("##WF> " + msg);
                    stateContent = (WorkflowHandlerBase)Node.LoadNodeByVersionId(stateContent.VersionId);
                }
                catch (Exception e)
                {
                    var msg = String.Format("InstanceManager:  Cannot write back a system message to the workflow state content. InstanceId: {0}. Path: {1}. Message: {2}"
                       , stateContent.Id, stateContent.Path, abortMessage);
                    Debug.WriteLine("##WF> " + msg);
                    Logger.WriteWarning(msg, Logger.EmptyCategoryList, new Dictionary<string, object> { { "Exception", e } });
                    break;
                }
            }
        }

        private static void WriteBackTheState(WorkflowStatusEnum state, Guid instanceId)
        {
            var stateContent = GetStateContent(instanceId);
            if (stateContent == null)
                return;

            switch (stateContent.WorkflowStatus)
            {
                case WorkflowStatusEnum.Created:
                    if (state == WorkflowStatusEnum.Created)
                        return;
                    break;
                case WorkflowStatusEnum.Running:
                    if (state == WorkflowStatusEnum.Created || state == WorkflowStatusEnum.Running)
                        return;
                    break;
                case WorkflowStatusEnum.Aborted:
                case WorkflowStatusEnum.Completed:
                    return;
                default:
                    break;
            }

            var times = 3;
            while (true)
            {
                try
                {
                    stateContent.WorkflowStatus = state;
                    stateContent.DisableObserver(typeof(WorkflowNotificationObserver));
                    using (new SystemAccount())
                        stateContent.Save(SenseNet.ContentRepository.SavingMode.KeepVersion);
Debug.WriteLine(String.Format("##WF> InstanceManager: WriteBackTheState: {0}, id: {1}, path: {2}", state, instanceId, stateContent.Path));
                    break;
                }
                catch (NodeIsOutOfDateException ne)
                {
                    if (--times == 0)
                        throw new NodeIsOutOfDateException("Node is out of date after 3 trying", ne);
                    var msg = "InstanceManager: Writing back the workflow state caused NodeIsOutOfDateException. Trying again";
Debug.WriteLine("##WF> " + msg);
                    Logger.WriteVerbose(msg);
                    stateContent = (WorkflowHandlerBase)Node.LoadNodeByVersionId(stateContent.VersionId);
                }
                catch (Exception e)
                {
                    var msg = String.Format("Workflow state is {0} but cannot write back to the workflow state content. InstanceId: {1}. Path: {2}"
                       , state, instanceId, stateContent.Path);
Debug.WriteLine("##WF> " + msg);
                    Logger.WriteWarning(msg, Logger.EmptyCategoryList, new Dictionary<string, object> { { "Exception", e } });
                    break;
                }
            }
        }

        private static string DumpException(WorkflowApplicationUnhandledExceptionEventArgs args)
        {
            var e = args.UnhandledException;
            var sb = new StringBuilder();
            sb.AppendLine("An unhandled exception occurred during the workflow execution. Please review the following information.<br />");
            sb.AppendLine();
            sb.Append("Workflow instance: ").Append(args.InstanceId.ToString()).AppendLine("<br />");
            sb.AppendFormat("Source activity: {0} ({1}, {2})", args.ExceptionSource.DisplayName, args.ExceptionSource.GetType().FullName, args.ExceptionSource.Id);
            sb.AppendLine("<br />");
            sb.AppendLine("<br />");

            sb.Append(DumpException(e));

            return sb.ToString();
        }
        private static string DumpException(Exception e)
        {
            var sb = new StringBuilder();
            sb.Append("========== Exception:").AppendLine("<br />");
            sb.Append(e.GetType().Name).Append(":").Append(e.Message).AppendLine("<br />");
            DumpTypeLoadError(e as ReflectionTypeLoadException, sb);
            sb.Append(e.StackTrace).AppendLine("<br />");
            while ((e = e.InnerException) != null)
            {
                sb.Append("---- Inner Exception:").AppendLine("<br />");
                sb.Append(e.GetType().Name).Append(": ").Append(e.Message).AppendLine("<br />");
                DumpTypeLoadError(e as ReflectionTypeLoadException, sb);
                sb.Append(e.StackTrace).AppendLine("<br />");
            }
            return sb.ToString();
        }
        private static void DumpTypeLoadError(ReflectionTypeLoadException exc, StringBuilder sb)
        {
            if (exc == null)
                return;
            sb.Append("LoaderExceptions:").AppendLine("<br />");
            foreach (var e in exc.LoaderExceptions)
            {
                sb.Append("-- ");
                sb.Append(e.GetType().FullName);
                sb.Append(": ");
                sb.Append(e.Message).AppendLine("<br />");

                var fileNotFoundException = e as System.IO.FileNotFoundException;
                if (fileNotFoundException != null)
                {
                    sb.Append("FUSION LOG:").AppendLine("<br />");
                    sb.Append(fileNotFoundException.FusionLog).AppendLine("<br />");
                }
            }
        }

        private static WorkflowHandlerBase GetStateContent(WorkflowApplicationUnhandledExceptionEventArgs args)
        {
            WorkflowHandlerBase stateContent = null;
            var exts = args.GetInstanceExtensions<ContentWorkflowExtension>();
            if (exts != null)
            {
                var ext = exts.FirstOrDefault();
                if (ext != null)
                    stateContent = Node.Load<WorkflowHandlerBase>(ext.WorkflowInstancePath);
            }
            return stateContent;
        }
        private static WorkflowHandlerBase GetStateContent(Guid instanceId)
        {
            //var query = String.Concat(".AUTOFILTERS:OFF +WorkflowInstanceGuid:\"", instanceId, "\"");
            var query = String.Format("+TypeIs:Workflow +{0}:\"{1}\" .AUTOFILTERS:OFF", WorkflowHandlerBase.WORKFLOWINSTANCEGUID, instanceId);
            var stateContent = (WorkflowHandlerBase)SenseNet.Search.ContentQuery.Query(query).Nodes.FirstOrDefault();
            return stateContent;
        }

        //=========================================================================================================== RelatedContentProtector

        internal static IDisposable CreateRelatedContentProtector(Node node, ActivityContext context)
        {
            return new RelatedContentProtector(node, context);
        }
        private class RelatedContentProtector : IDisposable
        {
            private Node _node;
            private ActivityContext _context;
            public RelatedContentProtector(Node node, ActivityContext context)
            {
                this._node = node;
                this._context = context;
                node.DisableObserver(typeof(WorkflowNotificationObserver));
            }
            private void Release()
            {
                Debug.WriteLine("##WF> RelatedContentProtector releasing");
                var path = _context.GetExtension<ContentWorkflowExtension>().WorkflowInstancePath;
                var stateContent = Node.Load<WorkflowHandlerBase>(path);
                if (stateContent.RelatedContent.Id == _node.Id)
                {
                    stateContent.RelatedContentTimestamp = _node.NodeTimestamp;
                    using (new SystemAccount())
                        stateContent.Save();
                }
                Debug.WriteLine("##WF> RelatedContentProtector released");
            }


            private bool _disposed;
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            private void Dispose(bool disposing)
            {
                if (!this._disposed)
                    if (disposing)
                        this.Release();
                _disposed = true;
            }
            ~RelatedContentProtector()
            {
                Dispose(false);
            }

        }
    }
}
