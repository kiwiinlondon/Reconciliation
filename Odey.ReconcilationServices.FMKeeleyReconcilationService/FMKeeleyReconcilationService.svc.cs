using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Odey.ReconcilationService.Contracts;

namespace Odey.ReconcilationServices.FMKeeleyReconcilationService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class FMKeeleyReconcilationService : IReconcilation
    {

        #region IReconcilation Members

        public void Reconcile()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
