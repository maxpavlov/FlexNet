using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Diagnostics;

namespace SenseNet.ContentRepository.Storage.Security
{
	public class LockHandler
	{
		private Node _node;

		//====================================================================== Properties

		public Node Node
		{
			get { return _node; }
		}

		public bool Locked
		{
			get
			{
				return _node.Version.Status == VersionStatus.Locked;
			}
		}

		public IUser LockedBy
		{
			get
			{
				if (this.Locked)
				{
					return Node.LoadNode(_node.LockedById) as IUser;
				}
				else
				{
					return null;
				}
			}
		}

		public string ETag
		{
			get
			{
				return _node.ETag;
			}
			set
			{
				_node.ETag = value;
			}
		}

		public int LockType
		{
			get
			{
				return _node.LockType;
			}
			set
			{
				_node.LockType = value;
			}
		}

		public int LockTimeout
		{
			get
			{
				return _node.LockTimeout;
			}
		}

		public DateTime LockDate
		{
			get
			{
				return _node.LockDate;
			}
		}

		public string LockToken
		{
			get
			{
				return _node.LockToken;
			}
		}

		public DateTime LastLockUpdate
		{
			get
			{
				return _node.LastLockUpdate;
			}
		}

		//====================================================================== Construction

		public LockHandler(Node node)
		{
			_node = node;
		}

		//====================================================================== Methods
		public void Lock()
		{
			Lock(DefaultLockTimeOut);
		}
		public void Lock(int timeout)
		{
			Lock(timeout, VersionRaising.None);
		}
		public void Lock(VersionRaising versionRaising)
		{
			Lock(DefaultLockTimeOut, versionRaising);
		}
		public void Lock(int timeout, VersionRaising versionRaising)
		{
            using (var traceOperation = Logger.TraceOperation("Node.LockHandler.Lock"))
            {
                if (!this.Locked)
                {
                    _node.LockToken = Guid.NewGuid().ToString();
                    _node.LockedById = AccessProvider.Current.GetCurrentUser().Id;
                    _node.LockDate = DateTime.Now;
                    _node.LastLockUpdate = DateTime.Now;
                    _node.LockTimeout = timeout;

                    _node.Save(versionRaising, VersionStatus.Locked);

                }
                else
                {
                    RefreshLock(versionRaising);
                }
                //****Log
                Logger.WriteVerbose("Node is locked.", Logger.GetDefaultProperties, this);
                traceOperation.IsSuccessful = true;
            }
		}

        public void RefreshLock()
        {
            RefreshLock(DefaultLockTimeOut, VersionRaising.None);
        }

		public void RefreshLock(VersionRaising versionRaising)
		{
			RefreshLock(DefaultLockTimeOut, versionRaising);
		}
		public void RefreshLock(int timeout, VersionRaising versionRaising)
		{
            using (var traceOperation = Logger.TraceOperation("Node.LockHandler.Lock"))
            {
                IUser lockUser = this.LockedBy;
                if (lockUser.Id == AccessProvider.Current.GetCurrentUser().Id)
                    RefreshLock(this.LockToken, timeout, versionRaising);
                else
                    throw new SenseNetSecurityException(this.Node.Id, "Node is locked by another user");
                traceOperation.IsSuccessful = true;
            }
		}
		public void RefreshLock(string token, int timeout, VersionRaising versionRaising)
		{
            using (var traceOperation = Logger.TraceOperation("Node.LockHandler.Lock"))
            {
                if (this.Locked && this.LockToken == token)
                {
                    _node.LastLockUpdate = DateTime.Now;
                    _node.LockTimeout = timeout;
                    _node.Save(versionRaising, VersionStatus.Locked);

                    //****Log
                    Logger.WriteVerbose("NodeLock is refreshed."      // message
                        , new Dictionary<string, object>() { { "Id", Node.Id }, { "Path", Node.Path } } // properties
                        );
                }
                else
                {
                    if (Locked)
                        throw new LockedNodeException(this, "Node is locked but passed locktoken is invalid.");
                    else
                        throw new LockedNodeException(this, "Node is not locked or lock timed out");
                }
                traceOperation.IsSuccessful = true;
            }
		}

		public void Unlock(VersionStatus versionStatus, VersionRaising versionRaising)
		{
            using (var traceOperation = Logger.TraceOperation("Node.LockHandler.Unlock"))
            {
                if(this.LockedBy.Id != AccessProvider.Current.GetCurrentUser().Id)
                    this.Node.Security.Assert("Node is locked by another user", PermissionType.ForceCheckin);
                this.Unlock(this.LockToken, versionStatus, versionRaising);
                traceOperation.IsSuccessful = true;
            }
        }
		public void Unlock(string token, VersionStatus versionStatus, VersionRaising versionRaising)
		{
            using (var traceOperation = Logger.TraceOperation("Node.LockHandler.Unlock"))
            {
                if (Locked && this.LockToken == token)
                {
                    _node.LockedById = 0;
                    _node.LockToken = string.Empty;
                    _node.LockTimeout = 0;
                    _node.LockDate = new DateTime(1800, 1, 1);
                    _node.LastLockUpdate = new DateTime(1800, 1, 1);
                    _node.LockType = 0;
                    _node.Save(versionRaising, versionStatus);

                    //****Log
                    Logger.WriteVerbose("Node lock is released."     // message
                        , new Dictionary<string, object>() { { "Id", Node.Id }, { "Path", Node.Path } } // properties
                        );

                }
                else
                {
                    if (Locked)
                        throw new LockedNodeException(this, "Node is not locked or lock timed out");
                    else
                        throw new LockedNodeException(this, "Node is not locked or lock timed out");
                }
                traceOperation.IsSuccessful = true;
            }
        }

		public static int DefaultLockTimeOut
		{
			get
			{
				string timeoutString = ConfigurationManager.AppSettings["DefaultLockTimeout"].ToString();
				if (!string.IsNullOrEmpty(timeoutString))
				{
					try
					{
						return Convert.ToInt32(timeoutString, System.Globalization.CultureInfo.InvariantCulture);
					}
					catch //TODO: TryParse
					{
					}
				}
				throw new ConfigurationErrorsException("Sense/Net Content Repository App.config: DefaultTimeOut is missing or not in a correct format.");
			}
		}
	}
}