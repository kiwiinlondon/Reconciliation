using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Odey.ReconciliationServices.Contracts
{
    [ServiceContract(Namespace = "Odey.ReconciliationServices.Contracts")]
    public interface IValuationReconciliation
    {
        [OperationContract]
        MatchingEngineOutput MatchPositionsAgainstKeeley(int fundId, DateTime referenceDate, List<PortfolioReconciliationItem> portfolioItems);
    }
}
