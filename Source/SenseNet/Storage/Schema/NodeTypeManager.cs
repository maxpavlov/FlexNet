using System.Data;
using System.Xml;
using System.Collections.Generic;
using System;
using SenseNet.ContentRepository.Storage.Data;
using System.Runtime.Remoting;
using System.Diagnostics;
using System.Reflection;
using SenseNet.ContentRepository.Storage.ApplicationMessaging;
using SenseNet.ContentRepository.Storage.Events;
using SenseNet.Diagnostics;
using System.Linq;
using System.Configuration;

namespace SenseNet.ContentRepository.Storage.Schema
{
	internal sealed class NodeTypeManager : SchemaRoot
    {
        #region Distributed Action child class
        [Serializable]
        internal class NodeTypeManagerRestartDistributedAction : SenseNet.Communication.Messaging.DistributedAction
        {
            public override void DoAction(bool onRemote, bool isFromMe)
            {
                // Local echo of my action: Return without doing anything
                if (onRemote && isFromMe)
                    return;

                NodeTypeManager.RestartPrivate();
            }
        }
        #endregion

        private static NodeTypeManager _current;
        private static readonly object _lock = new object();

		internal static NodeTypeManager Current
		{
			get
			{
                if(_current == null)
                {
                    lock(_lock)
                    {
                        if (_current == null)
                        {
                            var current = new NodeTypeManager();
                            current.Load();
                            _current = current;
                            current.StartEventSystem();
							NodeObserver.FireOnStart(Start);
                            Logger.WriteInformation("NodeTypeManager created.", Logger.GetDefaultProperties, _current);
                        }
                    }
                }
                return _current;
			}
		}

		private NodeTypeManager()
		{
		}

        /// <summary>
        /// Distributes a NodeTypeManager restart (calls the NodeTypeManager.RestartPrivate()).
        /// </summary>
        internal static void Restart()
        {
            Logger.WriteInformation("NodeTypeManager.Restart called.", Logger.Categories(), 
                new Dictionary<string, object> { { "AppDomain", AppDomain.CurrentDomain.FriendlyName } });
            new NodeTypeManagerRestartDistributedAction().Execute();
        }


        /// <summary>
        /// Restarts the NodeTypeManager without sending an ApplicationMessage.
        /// Do not call this method explicitly, the system will call it if neccessary (when the reset is triggered by an another instance).
        /// </summary>
        private static void RestartPrivate()
        {
            Logger.WriteInformation("NodeTypeManager.Restart executed.", Logger.Categories(), 
                new Dictionary<string, object> { { "AppDomain", AppDomain.CurrentDomain.FriendlyName } });
            OnReset();
            lock (_lock)
            {
                DataProvider.Current.Reset();
                _current = null;
            }
        }

		public static event EventHandler<EventArgs> Start;
		public static event EventHandler<EventArgs> Reset;
		private static void OnReset()
		{
			NodeObserver.FireOnReset(Reset);
		}

		private List<NodeObserver> _nodeObservers;
        internal List<NodeObserver> NodeObservers
        {
            get { return _nodeObservers; }
        }

        private const string DISABLEDNODEOBSERVERSKEY = "DisabledNodeObservers";
        private static List<string> _disabledNodeObservers;
        public static List<string> DisabledNodeObservers
        {
            get
            {
                if (_disabledNodeObservers == null)
                {
                    var setting = ConfigurationManager.AppSettings[DISABLEDNODEOBSERVERSKEY];
                    _disabledNodeObservers = string.IsNullOrEmpty(setting) ? new List<string>() : setting.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                return _disabledNodeObservers;
            }
        }

        private void GatherNodeObservers()
        {
            _nodeObservers = new List<NodeObserver>();
            var nodeObserverTypes = TypeHandler.GetTypesByBaseType(typeof(NodeObserver));
            _nodeObservers = nodeObserverTypes.Select(t => (NodeObserver)Activator.CreateInstance(t, true))
                .Where(n => !DisabledNodeObservers.Contains(n.GetType().FullName)).ToList();
        }

		private void StartEventSystem()
		{
            GatherNodeObservers();

            Logger.WriteInformation("NodeObservers are instantiated. ",
                new Dictionary<string, object> { { "Types", String.Join(", ", _nodeObservers.Select(x => x.GetType().FullName).ToArray()) } });
		}

        public static TypeCollection<PropertyType> GetDynamicSignature(int nodeTypeId, int contentListTypeId)
        {
            System.Diagnostics.Debug.Assert(nodeTypeId > 0);

            var nodePropertyTypes = NodeTypeManager.Current.NodeTypes.GetItemById(nodeTypeId).PropertyTypes;
            var allPropertyTypes = new TypeCollection<PropertyType>(nodePropertyTypes);
            if (contentListTypeId > 0)
                allPropertyTypes.AddRange(NodeTypeManager.Current.ContentListTypes.GetItemById(contentListTypeId).PropertyTypes);

            return allPropertyTypes;
        }

	}
}
