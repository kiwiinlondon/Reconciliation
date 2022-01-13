using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Odey.ReconciliationServices.Contracts
{
    [ServiceContract(Namespace = "Odey.ReconciliationServices.Contracts")]
    public interface IClientPortfolioReconciliation
    {
        [OperationContract]
        MatchingEngineOutput Reconcile(string fileName, int fundId,int? fundFeederTypeId, DateTime referenceDate);
    }
}
