using System;
using System.Collections.Generic;

namespace SenseNet.ContentRepository.Storage.Schema
{
    [System.Diagnostics.DebuggerDisplay("{Name} (Id={Id.ToString()}, default={IsDefaultPermission})")]
    public class PermissionType : SchemaItem
    {
        private static List<string> defaultPermissionNames = new List<string>{"See", "Open", "OpenMinor", "Save", "Publish", "ForceCheckin", "AddNew", "Approve", "Delete", "RecallOldVersion",
			"DeleteOldVersion", "SeePermissions", "SetPermissions", "RunApplication", "ManageListsAndWorkspaces"};

        public static string[] DefaultPermissionNames
        {
            get { return defaultPermissionNames.ToArray(); }
        }

        public static readonly int NumberOfPermissionTypes = 16;

		//===================================================================================== Fields

		private bool _isDefaultPermission;

		//===================================================================================== Properties

		public bool IsDefaultPermission
		{
			get { return _isDefaultPermission; }
		}

		//===================================================================================== Construction

		internal PermissionType(int id, string name, ISchemaRoot schemaRoot) : base(schemaRoot, name, id)
		{
			for (int i = 0; i < DefaultPermissionNames.Length; i++)
				if ((_isDefaultPermission = (name == DefaultPermissionNames[i])) == true)
					break;
		}

		//===================================================================================== Static Default Access

        public static PermissionType GetById(int id)
        {
            return NodeTypeManager.Current.PermissionTypes.GetItemById(id);
        }
		public static PermissionType GetByName(string permissionTypeName)
		{
			return NodeTypeManager.Current.PermissionTypes[permissionTypeName];
		}


		public static PermissionType See
        {
			get { return NodeTypeManager.Current.PermissionTypes["See"]; }
        }
        public static PermissionType Open
        {
			get { return NodeTypeManager.Current.PermissionTypes["Open"]; }
		}
        public static PermissionType OpenMinor
        {
			get { return NodeTypeManager.Current.PermissionTypes["OpenMinor"]; }
		}
        public static PermissionType Save
        {
			get { return NodeTypeManager.Current.PermissionTypes["Save"]; }
		}
		public static PermissionType Publish
		{
			get { return NodeTypeManager.Current.PermissionTypes["Publish"]; }
		}
		public static PermissionType ForceCheckin
		{
			get { return NodeTypeManager.Current.PermissionTypes["ForceCheckin"]; }
		}
		public static PermissionType AddNew
		{
			get { return NodeTypeManager.Current.PermissionTypes["AddNew"]; }
		}
		public static PermissionType Approve
		{
			get { return NodeTypeManager.Current.PermissionTypes["Approve"]; }
		}
		public static PermissionType Delete
		{
			get { return NodeTypeManager.Current.PermissionTypes["Delete"]; }
		}
		public static PermissionType RecallOldVersion
		{
			get { return NodeTypeManager.Current.PermissionTypes["RecallOldVersion"]; }
		}
		public static PermissionType DeleteOldVersion
		{
			get { return NodeTypeManager.Current.PermissionTypes["DeleteOldVersion"]; }
		}
		public static PermissionType SeePermissions
		{
			get { return NodeTypeManager.Current.PermissionTypes["SeePermissions"]; }
		}
		public static PermissionType SetPermissions
		{
			get { return NodeTypeManager.Current.PermissionTypes["SetPermissions"]; }
		}
		public static PermissionType RunApplication
		{
			get { return NodeTypeManager.Current.PermissionTypes["RunApplication"]; }
		}
        public static PermissionType ManageListsAndWorkspaces
        {
            get { return NodeTypeManager.Current.PermissionTypes["ManageListsAndWorkspaces"]; }
        }
	}
}