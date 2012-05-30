using System;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage.Schema;
using System.ComponentModel;
using SenseNet.ContentRepository.Storage.Search;

namespace SenseNet.ContentRepository.Storage
{
    /// <summary>
    /// ActiveSchema is a wrapper for NodeTypeManager. By using the ActiveSchema class you can the the NodeTypes, PropertyTypes and PermisionTypes currenly in the system.
    /// </summary>
	public static class ActiveSchema
	{
		public static readonly List<string> NodeAttributeNames = new List<string>(new string[]{
			"Id", "Parent", "Name", "Path", //"UrlName", "UrlPath", 
			"Index", "Locked", "LockedBy", "ETag", "LockType", "LockTimeout", "LockDate", "LockToken",        
			"LastLockUpdate", "LastMinorVersionId", "LastMajorVersionId", "MajorVersion", "MinorVersion", 
			"CreationDate", "CreatedBy", "ModificationDate", "ModifiedBy" });

		private static readonly Dictionary<NodeAttribute, DataType> _nodeAttributeDataTypes;

		static ActiveSchema()
		{
			_nodeAttributeDataTypes = new Dictionary<NodeAttribute, DataType>();
			_nodeAttributeDataTypes.Add(NodeAttribute.Id, DataType.Int);
			_nodeAttributeDataTypes.Add(NodeAttribute.Parent, DataType.Reference);			//????
			_nodeAttributeDataTypes.Add(NodeAttribute.Name, DataType.String);
			_nodeAttributeDataTypes.Add(NodeAttribute.Path, DataType.String);
			_nodeAttributeDataTypes.Add(NodeAttribute.Index, DataType.Int);
			_nodeAttributeDataTypes.Add(NodeAttribute.Locked, DataType.Int);
			_nodeAttributeDataTypes.Add(NodeAttribute.LockedBy, DataType.Reference);		//????
			_nodeAttributeDataTypes.Add(NodeAttribute.ETag, DataType.String);
			_nodeAttributeDataTypes.Add(NodeAttribute.LockType, DataType.Int);
			_nodeAttributeDataTypes.Add(NodeAttribute.LockTimeout, DataType.Int);
			_nodeAttributeDataTypes.Add(NodeAttribute.LockDate, DataType.DateTime);
			_nodeAttributeDataTypes.Add(NodeAttribute.LockToken, DataType.String);
			_nodeAttributeDataTypes.Add(NodeAttribute.LastLockUpdate, DataType.DateTime);
			_nodeAttributeDataTypes.Add(NodeAttribute.LastMinorVersionId, DataType.Int);
			_nodeAttributeDataTypes.Add(NodeAttribute.LastMajorVersionId, DataType.Int);
			_nodeAttributeDataTypes.Add(NodeAttribute.MajorVersion, DataType.Int);
			_nodeAttributeDataTypes.Add(NodeAttribute.MinorVersion, DataType.Int);
			_nodeAttributeDataTypes.Add(NodeAttribute.CreationDate, DataType.DateTime);
			_nodeAttributeDataTypes.Add(NodeAttribute.CreatedBy, DataType.Reference);		//????
			_nodeAttributeDataTypes.Add(NodeAttribute.ModificationDate, DataType.DateTime);
			_nodeAttributeDataTypes.Add(NodeAttribute.ModifiedBy, DataType.Reference);	//????
			//NodeTypeManager.Reset += new EventHandler<EventArgs>(NodeTypeManager_Reset);
		}

		/// <summary>
		/// Gets the DataProvider dependent earliest DateTime value
		/// </summary>
		public static DateTime DateTimeMinValue
		{
			get { return SenseNet.ContentRepository.Storage.Data.DataProvider.Current.DateTimeMinValue; }
		}
		/// <summary>
		/// Gets the DataProvider dependent last DateTime value
		/// </summary>
		public static DateTime DateTimeMaxValue
		{
			get { return SenseNet.ContentRepository.Storage.Data.DataProvider.Current.DateTimeMaxValue; }
		}
		/// <summary>
		/// Gets the maximum length of the short text datatype
		/// </summary>
		public static int ShortTextMaxLength { get { return 400; } }
        /// <summary>
        /// Gets the DataProvider dependent smallest decimal value
        /// </summary>
        public static decimal DecimalMinValue
        {
            get { return SenseNet.ContentRepository.Storage.Data.DataProvider.Current.DecimalMinValue; }
        }
        /// <summary>
        /// Gets the DataProvider dependent biggest decimal value
        /// </summary>
        public static decimal DecimalMaxValue
        {
            get { return SenseNet.ContentRepository.Storage.Data.DataProvider.Current.DecimalMaxValue; }
        }


        /// <summary>
        /// Gets the property types.
        /// </summary>
        /// <value>The property types.</value>
		public static TypeCollection<PropertyType> PropertyTypes
		{
			get { return NodeTypeManager.Current.PropertyTypes; }
		}
        /// <summary>
        /// Gets the node types.
        /// </summary>
        /// <value>The node types.</value>
		public static TypeCollection<NodeType> NodeTypes
		{
			get { return NodeTypeManager.Current.NodeTypes; }
		}
		/// <summary>
        /// Gets the ContentList types.
		/// </summary>
        /// <value>The ContentList types.</value>
		public static TypeCollection<ContentListType> ContentListTypes
		{
			get { return NodeTypeManager.Current.ContentListTypes; }
		}
		/// <summary>
        /// Gets the permission types.
        /// </summary>
        /// <value>The permission types.</value>
		public static TypeCollection<PermissionType> PermissionTypes
		{
			get { return NodeTypeManager.Current.PermissionTypes; }
		}

        /// <summary>
        /// Resets the NodeTypeManager instance.
        /// </summary>
		public static void Reset()
		{
            // The NodeTypeManager distributes its restart, no distrib action needed
            NodeTypeManager.Restart();
		}

        /// <summary>
        /// Gets the type of the node attribute data.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <returns></returns>
		public static DataType GetNodeAttributeDataType(NodeAttribute attribute)
		{
			DataType result;
			if (_nodeAttributeDataTypes.TryGetValue(attribute, out result))
				return result;
			throw new NotImplementedException(String.Concat(SR.Exceptions.Schema.Msg_NodeAttributeDoesNotEsist, " ", attribute));
		}

		//==================================================================================== Global events

		//public static event CancellableNodeEventHandler NodeCreating;
		//public static event EventHandler<NodeEventArgs> NodeCreated;
		//public static event CancellableNodeEventHandler NodeModifying;
		//public static event EventHandler<NodeEventArgs> NodeModified;
		//public static event CancellableNodeEventHandler NodeDeleting;
		//public static event EventHandler<NodeEventArgs> NodeDeleted;
		//public static event CancellableNodeEventHandler NodeDeletingPhysically;
		//public static event EventHandler<NodeEventArgs> NodeDeletedPhysically;
		//public static event CancellableNodeOperationEventHandler NodeMoving;
		//public static event EventHandler<NodeOperationEventArgs> NodeMoved;
		//public static event CancellableNodeOperationEventHandler NodeCopying;
		//public static event EventHandler<NodeOperationEventArgs> NodeCopied;

		//internal static void OnNodeCreating(Node sender, CancellableNodeEventArgs args)
		//{
		//    EventDistributor.InvokeCancelEventHandlers(NodeCreating, sender, args);
		//}
		//internal static void OnNodeCreated(Node sender, EventArgs args)
		//{
		//    EventDistributor.InvokeEventHandlers<NodeEventArgs>(NodeCreated, sender, args);
		//}
		//internal static void OnNodeModifying(Node sender, CancellableNodeEventArgs args)
		//{
		//    EventDistributor.InvokeCancelEventHandlers(NodeModifying, sender, args);
		//}
		//internal static void OnNodeModified(Node sender, EventArgs args)
		//{
		//    EventDistributor.InvokeEventHandlers<NodeEventArgs>(NodeModified, sender, args);
		//}
		//internal static void OnNodeDeleting(Node sender, CancellableNodeEventArgs args)
		//{
		//    EventDistributor.InvokeCancelEventHandlers(NodeDeleting, sender, args);
		//}
		//internal static void OnNodeDeleted(Node sender, EventArgs args)
		//{
		//    EventDistributor.InvokeEventHandlers<NodeEventArgs>(NodeDeleted, sender, args);
		//}
		//internal static void OnNodeDeletingPhysically(Node node, CancellableNodeEventArgs args)
		//{
		//    EventDistributor.InvokeCancelEventHandlers(NodeDeletingPhysically, node, args);
		//}
		//internal static void OnNodeDeletedPhysically(Node node, EventArgs args)
		//{
		//    EventDistributor.InvokeEventHandlers<NodeEventArgs>(NodeDeletedPhysically, node, args);
		//}
		//internal static void OnNodeMoving(Node sender, CancellableNodeOperationEventArgs args)
		//{
		//    EventDistributor.InvokeCancelOperationEventHandlers(NodeMoving, sender, args);
		//}
		//internal static void OnNodeMoved(Node sender, NodeOperationEventArgs args)
		//{
		//    EventDistributor.InvokeEventHandlers<NodeOperationEventArgs>(NodeMoved, sender, args);
		//}
		//internal static void OnNodeCopying(Node sender, CancellableNodeOperationEventArgs args)
		//{
		//    EventDistributor.InvokeCancelOperationEventHandlers(NodeCopying, sender, args);
		//}
		//internal static void OnNodeCopied(Node sender, NodeOperationEventArgs args)
		//{
		//    EventDistributor.InvokeEventHandlers<NodeOperationEventArgs>(NodeCopied, sender, args);
		//}

		//========================================================================================== Events and Event handlers

		//static void NodeTypeManager_Reset(object sender, EventArgs e)
		//{
		//    NodeTypeManager.Reset -= new EventHandler<EventArgs>(NodeTypeManager_Reset);
		//    OnRestart();
		//}

		//public static event EventHandler<EventArgs> Restart;
		//private static void OnRestart()
		//{
		//    NodeObserver.FireOnReset(Restart);
		//    //EventDistributor.InvokeEventHandlers<EventArgs>(Restart, null, EventArgs.Empty);
		//}
	}
}