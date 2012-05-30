using System;
using System.Data;
using System.Data.SqlClient;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Caching.Dependency;

namespace SenseNet.ContentRepository.Storage
{
	public sealed class TransactionScope
	{
        private static bool _notSupported;

		private TransactionScope() { }

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        public static bool IsActive
        {
            get
            {
                return (ContextHandler.GetTransaction() != null);
            }
        }

        /// <summary>
        /// Gets the isolation level.
        /// </summary>
        /// <value>The isolation level.</value>
        public static IsolationLevel IsolationLevel
        {
            get
            {
                ITransactionProvider tran = ContextHandler.GetTransaction();
                return (tran != null) ? tran.IsolationLevel : IsolationLevel.Unspecified;
            }
        }

        /// <summary>
        /// Gets the transaction provider.
        /// </summary>
        /// <value>The transaction provider.</value>
        internal static ITransactionProvider Provider
        {
            get { return ContextHandler.GetTransaction(); }
        }


        //////////////////////////////////////// Static Methods ////////////////////////////////////////

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        public static void Begin()
        {
            Begin(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Begins the transaction with the specified isolation level.
        /// </summary>
        /// <param name="isolationLevel">The isolation level.</param>
        public static void Begin(IsolationLevel isolationLevel)
        {
            if(IsActive)
                throw new InvalidOperationException(); // Transaction is already Active (Parallel transactions msg..).

            ITransactionProvider tran = null;
            try
            {
                tran = DataProvider.Current.CreateTransaction();
                if(tran == null)
                {
                    // Transactions are not supported at the current provider.
                    _notSupported = true;
                    return;
                }

                tran.Begin(isolationLevel);
            }
            catch //rethrow
            {
                if(tran != null)
                    tran.Dispose();

                throw;
            }

            ContextHandler.SetTransaction(tran);
            OnBeginTransaction(tran, EventArgs.Empty);
        }

        internal static void Participate(ITransactionParticipant participant)
		{
			if (!IsActive)
				return;

			TransactionQueue queue = ContextHandler.GetTransactionQueue();
			if (queue == null)
			{
				queue = new TransactionQueue();
				ContextHandler.SetTransactionQueue(queue);
			}
            queue.Add(participant);
		}

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        public static void Commit()
        {
            if(_notSupported)
                return;

            ITransactionProvider tran = ContextHandler.GetTransaction();
            if(tran == null) // !IsActive
                throw new InvalidOperationException(); // Transaction is not Active.

            try
            {
                tran.Commit();
                
                var queue = ContextHandler.GetTransactionQueue();
                if (queue != null)
                    queue.Commit();

                OnCommitTransaction(tran, EventArgs.Empty);
            }
            finally
            {
                ContextHandler.SetTransaction(null);
                ContextHandler.SetTransactionQueue(null);
                tran.Dispose();
            }
        }

        /// <summary>
        /// Rollbacks the current transaction.
        /// </summary>
        public static void Rollback()
        {
            if(_notSupported)
                return;

            ITransactionProvider tran = ContextHandler.GetTransaction();
            if(tran == null) // Means: !IsActive (Transaction is not Active)
                throw new InvalidOperationException("Transaction is not active");

            try
            {
                tran.Rollback();

				var queue = ContextHandler.GetTransactionQueue();
				if (queue != null)
					queue.Rollback();

                OnRollbackTransaction(tran, EventArgs.Empty);
            }
            finally
            {
                //Cache.Clear(); // State "rollback" in cache. TODO: cache clear in cluster must be ensured.
                //CACHE: Ez sose mûködött jól... Cache.Clear kéne.
                DistributedApplication.Cache.Reset();

                ContextHandler.SetTransaction(null);
                ContextHandler.SetTransactionQueue(null);
                tran.Dispose();
            }
        }


        //////////////////////////////////////// Static Events ////////////////////////////////////////

        /// <summary>
        /// Occurs when transaction begins.
        /// </summary>
		public static event EventHandler BeginTransaction;
        /// <summary>
        /// Occurs when transaction commits.
        /// </summary>
		public static event EventHandler CommitTransaction;
        /// <summary>
        /// Occurs when transaction rollbacks.
        /// </summary>
		public static event EventHandler RollbackTransaction;

		internal static void OnBeginTransaction(object sender, EventArgs e)
        {
            if (BeginTransaction != null)
                BeginTransaction(sender, e);
        }
        internal static void OnCommitTransaction(object sender, EventArgs e)
        {
            if (CommitTransaction != null)
                CommitTransaction(sender, e);
        }
        internal static void OnRollbackTransaction(object sender, EventArgs e)
        {
            if (RollbackTransaction != null)
                RollbackTransaction(sender, e);
        }

	}
}