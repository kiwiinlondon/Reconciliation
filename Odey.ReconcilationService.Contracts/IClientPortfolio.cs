using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Odey.ReconciliationServices.Contracts
{
    public interface IClientPortfolioReconciliation
    {
        [OperationContract]
        MatchingEngineOutput ReconcileDaiwa(string fileName, int[] fundIds, DateTime referenceDate);
    }
}
