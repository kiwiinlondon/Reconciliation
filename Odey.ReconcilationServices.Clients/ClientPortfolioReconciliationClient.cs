using Odey.Framework.Infrastructure.Clients;
using Odey.ReconciliationServices.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Odey.ReconciliationServices.Clients
{
    public class ClientPortfolioReconciliationClient : OdeyClientBase<IClientPortfolioReconciliation>, IClientPortfolioReconciliation
    {
        public MatchingEngineOutput Reconcile(string fileName, int fundId, int? fundFeederTypeId, DateTime referenceDate)
        {
            IClientPortfolioReconciliation proxy = factory.CreateChannel();
            try
            {
                MatchingEngineOutput e = proxy.Reconcile(fileName, fundId, fundFeederTypeId, referenceDate);
                ((ICommunicationObject)proxy).Close();
                return e;
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }
    }
}
