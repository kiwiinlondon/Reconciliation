using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Odey.ReconciliationServices.Contracts
{

    [ServiceContract(Namespace = "Odey.ReconciliationServices.Contracts")]
    public interface IFMPortfolioCollection
    {
        [OperationContract]
        void CollectForFMFundId(int orgId, DateTime fromDate, DateTime toDate);

        [OperationContract]
        void CollectForFMFundId2(int orgId, DateTime fromDate, DateTime toDate, bool useNew);

        [OperationContract]
        void CollectForLatestValuation();
    }
}
