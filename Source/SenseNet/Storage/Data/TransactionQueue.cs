using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace SenseNet.ContentRepository.Storage.Data
{
    //internal class TransactionQueue
    //{
    //    private List<WeakReference> _dataReferences = new List<WeakReference>();

    //    public void Add(ITransactionParticipant data)
    //    {
    //        _dataReferences.Add(new WeakReference(data));
    //    }

    //    public void Commit()
    //    {
    //        foreach (WeakReference reference in _dataReferences)
    //            if (reference.IsAlive)
    //                //DataBackingStore.OnTransactionCommit((ITransactionParticipant)reference.Target);
    //                ((ITransactionParticipant)reference.Target).Commit();
    //    }
    //    public void Rollback()
    //    {
    //        foreach (WeakReference reference in _dataReferences)
    //            if (reference.IsAlive)
    //                //DataBackingStore.OnTransactionRollback((ITransactionParticipant)reference.Target);
    //                ((ITransactionParticipant)reference.Target).Rollback();
    //    }
    //}

    internal class TransactionQueue
    {
        private List<ITransactionParticipant> _dataReferences;

        public void Add(ITransactionParticipant data)
        {
            if (_dataReferences == null)
                _dataReferences = new List<ITransactionParticipant>();
            _dataReferences.Add(data);
        }

        public void Commit()
        {
            if (_dataReferences == null)
                return;
            foreach (var reference in _dataReferences)
                reference.Commit();
            _dataReferences = null;
        }
        public void Rollback()
        {
            if (_dataReferences == null)
                return;
            foreach (var reference in _dataReferences)
                reference.Rollback();
            _dataReferences = null;
        }
    }

}