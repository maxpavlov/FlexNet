using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.ContentRepository.Storage
{
    public interface ITransactionParticipant
    {
        void Commit();
        void Rollback();
    }

    internal class NodeDataParticipant : ITransactionParticipant
    {
        public NodeData Data { get; set; }
        public NodeSaveSettings Settings { get; set; }
        public void Commit()
        {
            DataBackingStore.OnNodeDataCommit(this);
        }
        public void Rollback()
        {
            DataBackingStore.OnNodeDataRollback(this);
        }
    }
}
