using System;
using System.Data;

namespace SenseNet.ContentRepository.Storage.Data
{
    public interface ITransactionProvider : IDisposable
    {
        IsolationLevel IsolationLevel { get; }

        void Begin(IsolationLevel isolationLevel);
        void Commit();
        void Rollback();
    }
}