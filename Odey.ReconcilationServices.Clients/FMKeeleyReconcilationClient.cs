using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Odey.Framework.Infrastructure.Clients;
using Odey.ReconciliationServices.Contracts;
using System.ServiceModel;

namespace Odey.ReconciliationServices.Clients
{
    public class FMKeeleyReconcilationClient : OdeyClientBase<IFMKeeleyReconciliation>, IFMKeeleyReconciliation
    {
        #region IFMReconciliation Members

        public MatchingEngineOutput GetUnmatchedCVLPositions(int fundId, DateTime fromDate, DateTime toDate, bool returnOnlyMismatches)
        {
            IFMKeeleyReconciliation proxy = factory.CreateChannel();
            try
            {
                MatchingEngineOutput e = proxy.GetUnmatchedCVLPositions(fundId, fromDate, toDate, returnOnlyMismatches);
                ((ICommunicationObject)proxy).Close();
                return e;
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }

        #endregion
    }
}
