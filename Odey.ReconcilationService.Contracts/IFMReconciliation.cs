using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Odey.ReconciliationService.Contracts
{
    public interface IFMReconciliation
    {
        void ReconcileCVLPositions(int fundId, DateTime fromDate, DateTime toDate);
    }
}
