using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odey.ReconciliationServices.Contracts
{
    public interface IAttributionReconcilation
    {
        void Reconcile(int fundId, DateTime referenceDate);
    }
}
