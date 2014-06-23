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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fundId"></param>
        /// <param name="referenceDate"></param>
        /// <param name="portfolioItems"></param>
        /// <param name="instrumentClassIdsToExclude">exclude some inst classes from rec eg forward Fx (26). Eg if your position file doesnt have forward FXs in them and Keeley stored proc does</param></param>
        /// <returns></returns>
        [OperationContract]
        MatchingEngineOutput MatchPositionsAgainstKeeley(int fundId, DateTime referenceDate, List<PortfolioReconciliationItem> portfolioItems, int[] instrumentClassIdsToExclude);
    }
}
