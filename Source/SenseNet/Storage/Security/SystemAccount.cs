using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Diagnostics;

namespace SenseNet.ContentRepository.Storage.Security
{
    public class SystemAccount : IDisposable
    {
        OperationTrace tracer;

        public SystemAccount()
        {
            AccessProvider.ChangeToSystemAccount();
        }
    
        #region IDisposable Members

        public void  Dispose()
        {
            AccessProvider.RestoreOriginalUser();
        }

        #endregion
    }
}
