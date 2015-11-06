using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Odey.ReconciliationServices.Contracts
{
    [ServiceContract(Namespace = "Odey.ReconciliationServices.Contracts")]
    public interface IEzeReconciliation
    {
        [OperationContract]
        MatchingEngineOutput GetMatchedNavs(DateTime referenceDate);

        //[OperationContract]
        //List<ThreeWayNavRecOutput> GetThreeWayRecOutput (DateTime referenceDate);
    }
}

