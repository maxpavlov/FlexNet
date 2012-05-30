using System.Runtime.Remoting.Messaging;
using System.Web;
using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.ContentRepository.Storage
{
    public static class ContextHandler
    {
        public static object GetObject(string ident)
        {
            if(RepositoryConfiguration.IsWebEnvironment)
            {
                return HttpContext.Current.Items[ident];
            }
            else
            {
                return CallContext.GetData(ident);
            }
        }
        public static void SetObject(string identifier, object value)
        {
            if(RepositoryConfiguration.IsWebEnvironment)
            {
                HttpContext.Current.Items[identifier] = value;
            }
            else
            {
                CallContext.SetData(identifier, value);
            }
        }

		private const string TransactionIdent = "SnCr.Transaction";
		private const string TransactionQueueIdent = "SnCr.TransactionQueue";

        internal static ITransactionProvider GetTransaction()
        {
            return (ITransactionProvider)GetObject(TransactionIdent);
        }
        internal static void SetTransaction(ITransactionProvider transaction)
        {
            SetObject(TransactionIdent, transaction);
        }

		internal static TransactionQueue GetTransactionQueue()
        {
			return (TransactionQueue)GetObject(TransactionQueueIdent);
        }
		internal static void SetTransactionQueue(TransactionQueue queue)
        {
			SetObject(TransactionQueueIdent, queue);
        }

    }
}