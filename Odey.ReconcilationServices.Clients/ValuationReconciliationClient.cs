using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Odey.Framework.Infrastructure.Clients;
using Odey.ReconciliationServices.Contracts;
using System.ServiceModel;

namespace Odey.ReconciliationServices.Clients
{
    public class ValuationReconciliationClient : OdeyClientBase<IValuationReconciliation>, IValuationReconciliation
    {

        public MatchingEngineOutput MatchPositionsAgainstKeeley(int fundId, DateTime referenceDate, List<PortfolioReconciliationItem> portfolioItems)
        {
            IValuationReconciliation proxy = factory.CreateChannel();
            try
            {
                MatchingEngineOutput e = proxy.MatchPositionsAgainstKeeley(fundId, referenceDate, portfolioItems);
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
