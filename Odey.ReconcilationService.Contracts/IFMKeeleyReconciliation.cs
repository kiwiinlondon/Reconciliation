using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Odey.ReconciliationServices.Contracts
{
    [ServiceContract(Namespace = "Odey.ReconciliationServices.Contracts")]
    public interface IFMKeeleyReconciliation
    {
        [OperationContract]
        MatchingEngineOutput GetUnmatchedCVLPositions(int fundId, DateTime fromDate, DateTime toDate, bool returnOnlyMismatches);

        [OperationContract]
        MatchingEngineOutput GetMatchedNavs(DateTime referenceDate);
    }
}
